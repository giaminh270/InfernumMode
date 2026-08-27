using CalamityMod;
using CalamityMod.NPCs;
using InfernumMode.Effects;
using InfernumMode.ExtraTextures;
using InfernumMode.Graphics.Interfaces;
using InfernumMode.Graphics.Primitives;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.CalamitasShadow
{
    public class EntropyBeam : ModProjectile, IPixelPrimitiveDrawer
    {
		public bool DrawBeforeNPCs => false;
		
        public PrimitiveTrailCopy BeamDrawer
        {
            get;
            set;
        }

        public ref float Time => ref projectile.ai[0];

        public ref float LaserLength => ref projectile.ai[1];

        public const int Lifetime = 90;

        public const float MaxLaserLength = 5500f;

        public static NPC CalShadow => Main.npc[CalamityGlobalNPC.calamitas];

        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public override void SetStaticDefaults() => DisplayName.SetDefault("Entropic Beam");

        public override void SetDefaults()
        {
            projectile.width = projectile.height = 42;
            projectile.hostile = true;
            projectile.ignoreWater = true;
            projectile.tileCollide = false;
            projectile.penetrate = -1;
            projectile.timeLeft = Lifetime;
            projectile.alpha = 255;
            projectile.Calamity().canBreakPlayerDefense = true;
            cooldownSlot = 1;
        }

        public override void AI()
        {
            // Disappear if the shadow is not present.
            if (CalamityGlobalNPC.calamitas == -1)
            {
                projectile.Kill();
                return;
            }

            // Fade in.
            projectile.alpha = Utils.Clamp(projectile.alpha - 25, 0, 255);

            projectile.scale = CalamityUtils.Convert01To010(projectile.timeLeft / (float)Lifetime) * 3f;
            if (projectile.scale > 1f)
                projectile.scale = 1f;

            // Calculate the laser length.
            LaserLength = Utils.InverseLerp(-1f, 10f, Time, true) * MaxLaserLength;

            // Inherit the direction from the shadow's arm.
            projectile.velocity = (CalShadow.Infernum().ExtraAI[CalamitasShadowBehaviorOverride.ArmRotationIndex] + MathHelper.PiOver2).ToRotationVector2();
            projectile.BottomRight = CalShadow.Center;

            Time++;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float _ = 0f;
            float width = projectile.width * 0.8f;
            Vector2 start = projectile.Center;
            Vector2 end = start + projectile.velocity * (LaserLength - 80f) * 0.65f;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), start, end, width, ref _);
        }

        public float WidthFunction(float completionRatio)
        {
            float squeezeInterpolant = Utils.InverseLerp(1f, 0.95f, completionRatio, true);
            float baseWidth = MathHelper.SmoothStep(2f, projectile.width, squeezeInterpolant) * MathHelper.Clamp(projectile.scale, 0.01f, 1f);
            return baseWidth * MathHelper.Lerp(1f, 2.3f, projectile.localAI[0]);
        }

        public override bool ShouldUpdatePosition() => false;

        public Color ColorFunction(float completionRatio)
        {
            float opacity = Utils.InverseLerp(0.97f, 0.6f, completionRatio, true) * MathHelper.Lerp(1f, 0.45f, projectile.localAI[0]) * projectile.Opacity * 0.95f;
            Color color = Color.Lerp(Color.Red, Color.Yellow, (float)Math.Abs(Math.Sin(completionRatio * (float)MathHelper.Pi * 10f - 4f * Main.GlobalTime)) * 0.5f + 0.2f);
            return color * opacity;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor) => false;
        public void DrawPixelPrimitives(SpriteBatch spriteBatch)
        {
            if (BeamDrawer is null) 
				BeamDrawer = new PrimitiveTrailCopy(WidthFunction, ColorFunction, null, true, InfernumEffectsRegistry.ArtemisLaserVertexShader);

            // Select textures to pass to the shader, along with the electricity color.
            InfernumEffectsRegistry.ArtemisLaserVertexShader.UseColor(Color.Red);
            InfernumEffectsRegistry.ArtemisLaserVertexShader.SetShaderTexture(InfernumTextureRegistry.StreakMagma);
            InfernumEffectsRegistry.ArtemisLaserVertexShader.UseImage("Images/Misc/Perlin");
            InfernumEffectsRegistry.ArtemisLaserVertexShader.Shader.Parameters["uStretchReverseFactor"].SetValue((LaserLength + 1f) / MaxLaserLength * 4f);

            List<Vector2> points = new List<Vector2>();
            for (int i = 0; i <= 8; i++)
                points.Add(Vector2.Lerp(projectile.Center - projectile.velocity * 18f, projectile.Center + projectile.velocity * LaserLength, i / 8f));

            BeamDrawer.DrawPixelated(points, projectile.Size * 0.5f - Main.screenPosition, 60);
            Main.spriteBatch.ExitShaderRegion();
        }

        public override bool CanDamage() => Time >= 4f;
    }
}
