using InfernumMode.Graphics.Interfaces;
using InfernumMode.Graphics.Primitives;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.Draedon.Ares
{
    public class HotMetal : ModProjectile, IPixelPrimitiveDrawer
    {
        public bool DrawBeforeNPCs => false;
		
        internal PrimitiveTrailCopy TrailDrawer;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Superheated Metal");
            Main.projFrames[projectile.type] = 3;
            ProjectileID.Sets.TrailingMode[projectile.type] = 2;
            ProjectileID.Sets.TrailCacheLength[projectile.type] = 10;
        }

        public override void SetDefaults()
        {
            projectile.width = projectile.height = 26;
            projectile.penetrate = -1;
            projectile.tileCollide = false;
            projectile.hostile = true;
            projectile.timeLeft = 360;
            projectile.Opacity = 0f;
            cooldownSlot = 1;
        }

        public override void AI()
        {
            // Initialize frames.
            if (projectile.localAI[0] == 0f)
            {
                projectile.frame = Main.rand.Next(Main.projFrames[projectile.type]);
                projectile.localAI[0] = 1f;
            }

            projectile.Opacity = MathHelper.Clamp(projectile.Opacity + 0.067f, 0f, 1f);
            projectile.rotation += projectile.velocity.Y * 0.02f;

            projectile.velocity.X *= 0.993f;
            if (projectile.velocity.Y < 11f)
                projectile.velocity.Y += 0.25f;
        }

        public override Color? GetAlpha(Color lightColor)
        {
            return Color.Lerp(Color.White, Color.Red, projectile.ai[0]) * projectile.Opacity;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Rectangle frame = Main.projectileTexture[projectile.type].Frame(1, Main.projFrames[projectile.type], 0, projectile.frame);
            projectile.DrawProjectileWithBackglowTemp(Color.White, Color.White, 4f, frame);
            return false;
        }

        public float WidthFunction(float completionRatio)
        {
            float baseWidth = projectile.scale * projectile.width * 1.5f;
            return MathHelper.SmoothStep(baseWidth, 3.5f, completionRatio);
        }

        public Color ColorFunction(float completionRatio) => Color.Lerp(Color.Lerp(Color.Red, Color.OrangeRed, 0.5f), Color.Transparent, completionRatio) * 0.7f * projectile.Opacity;

        public void DrawPixelPrimitives(SpriteBatch spriteBatch)
        {
			if (TrailDrawer is null)
            	TrailDrawer = new PrimitiveTrailCopy(WidthFunction, ColorFunction, null, true, GameShaders.Misc["CalamityMod:ImpFlameTrail"]);
            GameShaders.Misc["CalamityMod:ImpFlameTrail"].UseImage("Images/Extra_189");
            TrailDrawer.DrawPixelated(projectile.oldPos, projectile.Size * 0.5f - Main.screenPosition, 25);
        }
    }
}
