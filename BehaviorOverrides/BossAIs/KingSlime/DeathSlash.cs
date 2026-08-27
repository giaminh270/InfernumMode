using CalamityMod;
using InfernumMode.Effects;
using InfernumMode.ExtraTextures;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.KingSlime
{
    public class DeathSlash : ModProjectile
    {
        internal PrimitiveTrail SlashDrawer;

        private readonly List<Vector2> TrailCache = new List<Vector2>();

        public float ScaleFactorDelta => projectile.localAI[0];

        public ref float CurrentVerticalOffset => ref projectile.ai[0];

        public ref float Time => ref projectile.ai[1];

        public const int Lifetime = 300;

        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public override void SetStaticDefaults() => DisplayName.SetDefault("Ninja Slice");

        public override void SetDefaults()
        {
            projectile.width = 14;
            projectile.height = 14;
            projectile.alpha = 255;
            projectile.penetrate = -1;
            projectile.ignoreWater = true;
            projectile.tileCollide = false;
            projectile.friendly = true;
            projectile.timeLeft = projectile.MaxUpdates * Lifetime;
            cooldownSlot = 1;
        }

        public override void AI()
        {
            // Disappear if the ninja is not present.
            int ninjaIndex = NPC.FindFirstNPC(ModContent.NPCType<Ninja>());
            if (!Main.npc.IndexInRange(ninjaIndex))
            {
                projectile.Kill();
                return;
            }

            // Get the ninja, and check whether we should be sticking to it.
            NPC ninjaNPC = Main.npc[ninjaIndex];
            bool stickToNinja = ninjaNPC.Infernum().ExtraAI[11] == projectile.whoAmI && ninjaNPC.velocity != Vector2.Zero;

            if (stickToNinja)
            {
                // Add our current position to the List of Vectors to draw.
                TrailCache.Add(projectile.Center);
                // Update our position to be accurate to the ninjas.
                projectile.Center = ninjaNPC.Center + Vector2.UnitY * CurrentVerticalOffset + ninjaNPC.velocity;
                // Randomly change our offset.
                if (Main.rand.NextBool(4))
                {
                    float newIdealOffset = Main.rand.NextBool().ToDirectionInt() * Main.rand.NextFloat(4f, 28f);
                    CurrentVerticalOffset = MathHelper.Lerp(CurrentVerticalOffset, newIdealOffset, 0.667f);

                    projectile.netUpdate = true;
                }
            }

            // Cap the amount of Vectors in the list at 20.
            if (TrailCache.Count > 20)
                TrailCache.RemoveAt(0);

            // If the ninja isnt moving, quickly clear the oldest Vector in the list.
            // This makes the projectile come back into the ninja. They both then disappear afterwards, done in the ninjas code.
            if (ninjaNPC.velocity == Vector2.Zero)
            {
                TrailCache.RemoveAt(0);
            }
            // Fade in.
            float disappearInterpolant = Utils.InverseLerp(0f, 24f, projectile.timeLeft / projectile.MaxUpdates, true);
            float scaleGrowInterpolant = (float)Math.Pow(Utils.InverseLerp(0f, 64f, Time, true), 1.72f);
            projectile.Opacity = Utils.InverseLerp(0f, 24f, Time / projectile.MaxUpdates, true) * disappearInterpolant;
            projectile.scale = MathHelper.Lerp(0.24f, 1f, scaleGrowInterpolant) * disappearInterpolant;
            Time++;
        }
        public override bool CanDamage() => false;

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            for (int i = 0; i < TrailCache.Count; i++)
            {
                if (Utils.CenteredRectangle(TrailCache[i], Vector2.One * WidthFunction(i / (float)(TrailCache.Count - 1f) * 0.7f)).Intersects(targetHitbox))
                    return true;

            }
            return false;
        }

        internal float WidthFunction(float completionRatio)
        {
            float baseWidth = MathHelper.Lerp(32f, 33f, (float)Math.Sin(MathHelper.Pi * 4f * completionRatio) * 0.5f + 0.5f) * projectile.scale;
            return CalamityUtils.Convert01To010(completionRatio) * baseWidth * (1f + ScaleFactorDelta) * 0.5f;
        }

        internal Color ColorFunction(float completionRatio)
        {
            float opacity = CalamityUtils.Convert01To010(completionRatio);
            if (opacity >= 1f)
                opacity = 1f;
            opacity *= projectile.Opacity * 0.18f;
            return Color.White * opacity;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            if (SlashDrawer is null)
				SlashDrawer = new PrimitiveTrail(WidthFunction, ColorFunction, null, InfernumEffectsRegistry.RealityTearVertexShader);

            InfernumEffectsRegistry.RealityTearVertexShader.SetShaderTexture(InfernumTextureRegistry.GrayscaleWater);
            InfernumEffectsRegistry.RealityTearVertexShader.Shader.Parameters["useOutline"].SetValue(true);
            SlashDrawer.Draw(TrailCache, projectile.Size * 0.5f - Main.screenPosition, 60);
            return false;
        }
    }
}
