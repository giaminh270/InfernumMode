using CalamityMod;
using CalamityMod.NPCs;
using CalamityMod.Particles;
using CalamityMod.World;
using InfernumMode.Particles;
using InfernumMode;
using InfernumMode.GlobalInstances;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static System.Math;
using static Microsoft.Xna.Framework.MathHelper;
using InfernumMode.Effects;
using InfernumMode.ExtraTextures;

namespace InfernumMode.BehaviorOverrides.BossAIs.Providence
{
    public class ProfanedLava : ModProjectile
    {
        public PrimitiveTrailCopy LavaDrawer
        {
            get;
            set;
        }

        public Vector2[] SamplePoints
        {
            get
            {
                Vector2[] result = new Vector2[125];
                for (int i = 0; i < result.Length; i++)
                {
                    float horizontalOffset = MathHelper.Lerp(-3900f, 3900f, i / (float)(result.Length - 1f));
                    result[i] = projectile.Bottom + Vector2.UnitX * horizontalOffset;
                }
                return result;
            }
        }

        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public static NPC Providence => Main.npc[CalamityGlobalNPC.holyBoss];

        public ref float Time => ref projectile.ai[0];

        public ref float LavaHeight => ref projectile.ai[1];

        public override void SetStaticDefaults() => DisplayName.SetDefault("Searing Lava of Atonement");

        public override void SetDefaults()
        {
            projectile.width = 2;
            projectile.height = 2;
            projectile.hostile = true;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;
            projectile.penetrate = -1;
            projectile.Opacity = 1f;
            projectile.timeLeft = int.MaxValue;
            projectile.hide = true;
            projectile.Calamity().canBreakPlayerDefense = true;
            cooldownSlot = 1;
        }

        public override void AI()
        {
            // Drain and eventually disappear once Providence is dead.
            if (!Main.npc.IndexInRange(CalamityGlobalNPC.holyBoss))
            {
                LavaHeight = MathHelper.Clamp(LavaHeight - 16f, 0f, 16000f) * 0.98f;
                if (LavaHeight <= 1f)
                    projectile.Kill();

                return;
            }

            // Stick at the bottom of the arena.
            LavaHeight = Providence.Infernum().ExtraAI[ProvidenceBehaviorOverride.LavaHeightIndex];
            projectile.Bottom = PoDWorld.ProvidenceArena.BottomRight() * 16f - Vector2.UnitX * 3000f;

            if (projectile.height != (int)LavaHeight)
                projectile.height = (int)LavaHeight;
            Time++;

            // Emit smoke and fire at the very top of the lava.
            Player closestPlayer = Main.player[Player.FindClosest(projectile.Center, 1, 1)];
            if (Main.myPlayer == closestPlayer.whoAmI)
            {
                for (int i = 0; i < (ScreenEffectSystem.AnyBlurOrFlashActive() ? 30 : 8); i++)
                {
                    Vector2 smokeVelocity = -Vector2.UnitY.RotatedByRandom(0.75f) * Main.rand.NextFloat(1f, 4f);

                    // Make the smoke rise if a flash or blur is happening.
                    if (ScreenEffectSystem.AnyBlurOrFlashActive())
                        smokeVelocity *= 3f;

                    Vector2 smokeSpawnPosition = new Vector2(closestPlayer.Center.X + Main.rand.NextFloatDirection() * 1200f, projectile.Top.Y + Main.rand.NextFloatDirection() * 15f + 35f);
                    CloudParticle smoke = new CloudParticle(smokeSpawnPosition, smokeVelocity, ProvidenceBehaviorOverride.IsEnraged ? Color.Cyan : Color.Orange, Color.DarkGray, 26, 0.8f, true);
                    GeneralParticleHandler.SpawnParticle(smoke);
                }
            }
        }

        public Color ColorFunction(float completionRatio)
        {
            float colorInterpolant = Math.Abs((float)Math.Sin(completionRatio * MathHelper.Pi * 8f + Main.GlobalTime));
            Color c = Color.Lerp(Color.Orange, Color.Red, colorInterpolant * 0.4f);
            if (ProvidenceBehaviorOverride.IsEnraged)
                c = Color.Lerp(c, Color.Blue, 0.95f);
            return c * projectile.Opacity;
        }

        public float WidthFunction(float completionRatio) => LavaHeight;

        public Vector2 OffsetFunction(float completionRatio)
        {
            float maxOffset = LavaHeight * 0.03f;
            if (maxOffset >= 50f)
                maxOffset = 50f;

            float offsetInterpolant = Utilities.FractalBrownianMotion(completionRatio * 5f, Time * projectile.localAI[0] / 500f % 50f, projectile.identity, 4);
            return Vector2.UnitY * offsetInterpolant * maxOffset;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            var samplePoints = SamplePoints;
            for (int i = 0; i < samplePoints.Length; i++)
            {
                float _ = 0f;
                float completionRatio = i / (float)projectile.oldPos.Length;
                Vector2 top = samplePoints[i] + OffsetFunction(completionRatio);
                Vector2 bottom = samplePoints[i] - Vector2.UnitY * LavaHeight + OffsetFunction(completionRatio);
                if (Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), top, bottom, 88f, ref _))
                    return true;
            }
            return false;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            if (LavaDrawer == null)
                LavaDrawer = new PrimitiveTrailCopy(WidthFunction, ColorFunction, OffsetFunction, false, InfernumEffectsRegistry.ProfanedLavaVertexShader);

            InfernumEffectsRegistry.ProfanedLavaVertexShader.SetShaderTexture(InfernumTextureRegistry.Smudges);
            InfernumEffectsRegistry.ProfanedLavaVertexShader.Shader.Parameters["lavaHeightInterpolant"].SetValue(LavaHeight / 1400f);

            Main.spriteBatch.EnterShaderRegion(BlendState.Additive);
            var samplePoints = SamplePoints;
            for (int i = 0; i < 4; i++)
            {
                projectile.localAI[0] = (i % 2 == 0f).ToDirectionInt();
                LavaDrawer.Draw(samplePoints, -Main.screenPosition, 19, 0f);
            }
            Main.spriteBatch.ExitShaderRegion();
            return false;
        }

        // The lava should draw above players, tiles, and NPCs.
        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers)
        {
            overPlayers.Add(index);
        }
    }
}
