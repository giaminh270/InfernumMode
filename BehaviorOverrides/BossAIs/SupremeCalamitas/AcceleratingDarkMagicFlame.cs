using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.SupremeCalamitas
{
    public class AcceleratingDarkMagicFlame : ModProjectile
    {
        public ref float Time => ref projectile.ai[0];
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Dark Magic Flame");
            Main.projFrames[projectile.type] = 5;
            ProjectileID.Sets.TrailCacheLength[projectile.type] = 4;
            ProjectileID.Sets.TrailingMode[projectile.type] = 0;
        }

        public override void SetDefaults()
        {
            projectile.width = projectile.height = 34;
            projectile.hostile = true;
            projectile.ignoreWater = true;
            projectile.tileCollide = false;
            projectile.penetrate = -1;
            projectile.timeLeft = 300;
            cooldownSlot = 1;
        }

        public override void AI()
        {
            projectile.Opacity = Utils.InverseLerp(0f, 12f, Time, true) * Utils.InverseLerp(0f, 20f, projectile.timeLeft, true);
            projectile.frameCounter++;
            projectile.frame = projectile.frameCounter / 5 % Main.projFrames[projectile.type];

            // Accelerate quickly.
            if (projectile.velocity.Length() < 16f)
                projectile.velocity *= 1.019f;

            Time++;
        }

		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
		{
			if (InfernumConfig.Instance.ReducedGraphicsConfig)
			{
				OptimizedDraw();
				return false;
			}

			DefaultDraw();
			return false;
		}

		public void DefaultDraw()
		{
			Utilities.DrawAfterimagesCentered(projectile, Color.White, ProjectileID.Sets.TrailingMode[projectile.type], 1, Main.projectileTexture[projectile.type], false);
		}

		public void OptimizedDraw()
		{
			Texture2D texture = Main.projectileTexture[projectile.type];
			Rectangle frame = texture.Frame(1, Main.projFrames[projectile.type], 0, projectile.frame);
			Vector2 drawPosition = projectile.Center - Main.screenPosition;
			Color color = projectile.GetAlpha(Color.White);
			
			Main.spriteBatch.Draw(texture, drawPosition, frame, color, projectile.rotation, frame.Size() * 0.5f, projectile.scale, SpriteEffects.None, 0f);
		}
    }
}
