using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.Prime
{
    public class SawSpark : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Saw Spark");
            Main.projFrames[projectile.type] = 3;
        }

        public override void SetDefaults()
        {
            projectile.width = projectile.height = 8;
            projectile.hostile = true;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;
            projectile.penetrate = -1;
            projectile.timeLeft = 300;
            projectile.Opacity = 0f;
            cooldownSlot = 1;
        }

        public override void AI()
        {
            if (projectile.velocity.Length() < 17f)
                projectile.velocity *= 1.02f;

            // Fade in.
            projectile.Opacity = MathHelper.Clamp(projectile.Opacity + 0.1f, 0f, 1f);

            // Decide rotation.
            projectile.rotation = projectile.velocity.ToRotation();

            // Decide frames.
            projectile.frameCounter++;
            projectile.frame = projectile.frameCounter / 5 % Main.projFrames[projectile.type];

            Lighting.AddLight(projectile.Center, Color.Orange.ToVector3());
        }

		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Texture2D texture = Main.projectileTexture[projectile.type];
            Vector2 drawPosition = projectile.Center - Main.screenPosition;
            Rectangle frame = texture.Frame(1, Main.projFrames[projectile.type], 0, projectile.frame);
            Vector2 origin = frame.Size() * 0.5f;

            Main.spriteBatch.Draw(texture, drawPosition, frame, projectile.GetAlpha(Color.White), projectile.rotation, origin, projectile.scale, SpriteEffects.None, 0f);
            return false;
        }
    }
}
