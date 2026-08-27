using CalamityMod;
using InfernumMode.Effects;
using InfernumMode.ExtraTextures;
using InfernumMode.Graphics.Interfaces;
using InfernumMode.Graphics.Primitives;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.Golem
{
    public class ThermalDeathray : ModProjectile, IPixelPrimitiveDrawer
    {
		public bool DrawBeforeNPCs => false;
		
        internal PrimitiveTrailCopy BeamDrawer;

        public float LaserLength
        {
            get;
            set;
        }

        public ref float Time => ref projectile.ai[0];

        public ref float Lifetime => ref projectile.ai[1];

        public ref float AngularVelocity => ref projectile.localAI[0];

        public const float MaxLaserLength = 4000f;

        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public override void SetStaticDefaults() => DisplayName.SetDefault("Thermal Deathray");

        public override void SetDefaults()
        {
            projectile.width = projectile.height = 54;
            projectile.hostile = true;
            projectile.ignoreWater = true;
            projectile.tileCollide = false;
            projectile.netImportant = true;
            projectile.penetrate = -1;
            projectile.timeLeft = 9000;
            projectile.alpha = 255;
            projectile.Calamity().canBreakPlayerDefense = true;
            cooldownSlot = 1;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(LaserLength);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            LaserLength = reader.ReadSingle();
        }

        public override void AI()
        {
            if (!Main.npc.IndexInRange(NPC.golemBoss) || !Main.npc[NPC.golemBoss].active)
            {
                projectile.Kill();
                return;
            }

            NPC golem = Main.npc[NPC.golemBoss];
            projectile.Center = golem.Center - Vector2.UnitY * 6f;

            // Fade in.
            projectile.alpha = Utils.Clamp(projectile.alpha - 25, 0, 255);

            // Determine the scale of the laser.
            CalculateScale();

            projectile.velocity = projectile.velocity.RotatedBy(Utils.InverseLerp(0f, 45f, Time, true) * golem.Infernum().ExtraAI[25]);
            if (Time >= Lifetime)
                projectile.Kill();

            // Make the laser quickly move outward.
            LaserLength = (float)Math.Pow(Utils.InverseLerp(4f, 30f, Time, true), 2.4f) * MaxLaserLength;

            // And create bright light.
            Lighting.AddLight(projectile.Center, Color.Purple.ToVector3() * 1.4f);

            Time++;
        }

        public void CalculateScale()
        {
            projectile.scale = CalamityUtils.Convert01To010(Time / Lifetime) * 1.45f;
            if (projectile.scale > 1f)
                projectile.scale = 1f;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float _ = 0f;
            float width = projectile.width * 0.8f;
            Vector2 start = projectile.Center;
            Vector2 end = start + projectile.velocity * (LaserLength - 80f);
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), start, end, width, ref _);
        }

        public float WidthFunction(float completionRatio)
        {
            float squeezeInterpolant = Utils.InverseLerp(1f, 0.92f, completionRatio, true);
            return MathHelper.SmoothStep(2f, projectile.width, squeezeInterpolant) * MathHelper.Clamp(projectile.scale, 0.04f, 1f);
        }

        public Color ColorFunction(float completionRatio)
        {
            Color color = Color.Lerp(Color.Yellow, Color.Red, 0.6f);
            return color * projectile.Opacity * 1.15f;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Main.spriteBatch.SetBlendState(BlendState.Additive);
            DrawFrontGlow();
            DrawBloomFlare();
            Main.spriteBatch.ResetBlendState();

            return false;
        }

        public void DrawFrontGlow()
        {
            float pulse = (float)Math.Cos(Main.GlobalTime * 36f);
            Texture2D backglowTexture = ModContent.GetTexture("CalamityMod/ExtraTextures/XerocLight");
            Vector2 origin = backglowTexture.Size() * 0.5f;
            Vector2 drawPosition = projectile.Center - Main.screenPosition + Vector2.UnitY * projectile.scale * 20f;
            Vector2 baseScale = new Vector2(1f + pulse * 0.05f, 1f) * projectile.scale * 0.7f;
            Main.spriteBatch.Draw(backglowTexture, drawPosition, null, Color.White * projectile.scale, 0f, origin, baseScale * 0.7f, 0, 0f);
            Main.spriteBatch.Draw(backglowTexture, drawPosition, null, Color.Yellow * projectile.scale * 0.4f, 0f, origin, baseScale * 1.2f, 0, 0f);
            Main.spriteBatch.Draw(backglowTexture, drawPosition, null, Color.Orange * projectile.scale * 0.3f, 0f, origin, baseScale * 1.7f, 0, 0f);
        }

        public void DrawBloomFlare()
        {
            Texture2D bloomFlare = ModContent.GetTexture("InfernumMode/ExtraTextures/GreyscaleObjects/BloomFlare");
            Vector2 drawPosition = projectile.Center - Main.screenPosition + Vector2.UnitY * projectile.scale * 20f;
            Color bloomFlareColor = Color.Lerp(Color.Wheat, Color.Yellow, 0.7f);
            float bloomFlareRotation = Main.GlobalTime * 1.64f;
            float bloomFlareScale = projectile.scale * 0.33f;
            Main.spriteBatch.Draw(bloomFlare, drawPosition, null, bloomFlareColor, -bloomFlareRotation, bloomFlare.Size() * 0.5f, bloomFlareScale, 0, 0f);

            bloomFlareColor = Color.Lerp(Color.Wheat, Main.hslToRgb((Main.GlobalTime * 0.2f + 0.5f) % 1f, 1f, 0.55f), 0.7f);
            bloomFlareColor = Color.Lerp(bloomFlareColor, Color.Red, 0.6f);
            Main.spriteBatch.Draw(bloomFlare, drawPosition, null, bloomFlareColor, bloomFlareRotation, bloomFlare.Size() * 0.5f, bloomFlareScale, 0, 0f);
        }

        public void DrawPixelPrimitives(SpriteBatch spriteBatch)
        {
			if (BeamDrawer is null)
                BeamDrawer = new PrimitiveTrailCopy(WidthFunction, ColorFunction, null, true, InfernumEffectsRegistry.ArtemisLaserVertexShader);

            var oldBlendState = Main.instance.GraphicsDevice.BlendState;
            Main.instance.GraphicsDevice.BlendState = BlendState.Additive;
            InfernumEffectsRegistry.ArtemisLaserVertexShader.UseSaturation(1.4f);
            InfernumEffectsRegistry.ArtemisLaserVertexShader.UseOpacity(-0.1f);
            InfernumEffectsRegistry.ArtemisLaserVertexShader.SetShaderTexture(InfernumTextureRegistry.HarshNoise);
            InfernumEffectsRegistry.ArtemisLaserVertexShader.Shader.Parameters["uStretchReverseFactor"].SetValue((LaserLength + 1f) / MaxLaserLength);

            List<float> originalRotations = new List<float>();
            List<Vector2> points = new List<Vector2>();
            for (int i = 0; i <= 16; i++)
            {
                points.Add(Vector2.Lerp(projectile.Center, projectile.Center + projectile.velocity * LaserLength, i / 16f));
                originalRotations.Add(MathHelper.PiOver2);
            }

            if (Time >= 2f)
            {
                float backwardsOffset = Math.Min(LaserLength * 0.1f, 100f);
                BeamDrawer.DrawPixelated(points, -Main.screenPosition - projectile.velocity * backwardsOffset, 47);
            }
            Main.instance.GraphicsDevice.BlendState = oldBlendState;
        }

        public override bool ShouldUpdatePosition() => false;
    }
}
