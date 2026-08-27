using System;
using InfernumMode.ExtraTextures;
using InfernumMode.Graphics.Interfaces;
using InfernumMode.Graphics.Primitives;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.StormWeaver
{
    public class HomingWeaverSpark : ModProjectile, IPixelPrimitiveDrawer
    {
        public bool DrawBeforeNPCs => false;

        public PrimitiveTrailCopy ElectricTrailDrawer
        {
            get;
            set;
        }

        public ref float Time => ref projectile.ai[0];

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Spark");
            ProjectileID.Sets.TrailingMode[projectile.type] = 2;
            ProjectileID.Sets.TrailCacheLength[projectile.type] = 8;
        }

        public override void SetDefaults()
        {
            projectile.width = projectile.height = 18;
            projectile.penetrate = -1;
            projectile.tileCollide = false;
            projectile.hostile = true;
            projectile.timeLeft = 300;
            projectile.Opacity = 0f;
            cooldownSlot = 1;
        }

        public override void AI()
        {
            projectile.Opacity = MathHelper.Clamp(projectile.Opacity + 0.1f, 0f, 1f);

            // Home in on the target at first.
            if (Time < 35f)
            {
                Player target = Main.player[Player.FindClosest(projectile.Center, 1, 1)];
                projectile.velocity = (projectile.velocity * 29f + projectile.SafeDirectionTo(target.Center) * 21f) / 20f;
            }

            // Accelerate after homing.
            else if (projectile.velocity.Length() < 36f)
                projectile.velocity *= 1.026f;

            projectile.rotation = projectile.velocity.ToRotation() + MathHelper.PiOver2;
            Time++;
        }

        public override Color? GetAlpha(Color lightColor)
        {
            return new Color(255, 255, 255, 56) * projectile.Opacity;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            projectile.DrawProjectileWithBackglowTemp(Color.White, lightColor, 4f);
            return false;
        }

        public float TrailWidthFunction(float completionRatio)
        {
            return MathHelper.SmoothStep(25f, 2f, completionRatio) * projectile.Opacity;
        }

        public Color TrailColorFunction(float completionRatio)
        {
            return Color.Cyan * (float)Math.Sqrt(completionRatio) * projectile.Opacity;
        }

        public void DrawPixelPrimitives(SpriteBatch spriteBatch)
        {
            // Initialize the trail drawer.
			if (ElectricTrailDrawer is null)
            	ElectricTrailDrawer = new PrimitiveTrailCopy(TrailWidthFunction, TrailColorFunction, null, true, GameShaders.Misc["CalamityMod:ImpFlameTrail"]);

            GameShaders.Misc["CalamityMod:ImpFlameTrail"].SetShaderTexture(InfernumTextureRegistry.StreakLightning);
            ElectricTrailDrawer.DrawPixelated(projectile.oldPos, projectile.Size * 0.5f - Main.screenPosition, 61);
        }
    }
}
