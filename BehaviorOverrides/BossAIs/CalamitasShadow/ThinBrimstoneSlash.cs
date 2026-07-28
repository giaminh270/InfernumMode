using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static System.Math;
using static Microsoft.Xna.Framework.MathHelper;

namespace InfernumMode.BehaviorOverrides.BossAIs.CalamitasShadow
{
    public class ThinBrimstoneSlash : ModProjectile
    {
        public override string Texture => "InfernumMode/Projectiles/Slash";

        public override void SetStaticDefaults() => DisplayName.SetDefault("Brimstone Slash");

        public override void SetDefaults()
        {
            projectile.width = Main.rand?.Next(256, 512) ?? 256;
            projectile.height = 24;
            projectile.friendly = true;
            projectile.ignoreWater = true;
            projectile.tileCollide = false;
            projectile.penetrate = -1;
            projectile.Opacity = 1f;
            projectile.timeLeft = 35;
            projectile.MaxUpdates = 2;
            projectile.scale = 0.75f;
            cooldownSlot = 1;
        }

        public override void AI()
        {
            projectile.rotation = projectile.velocity.ToRotation();
            projectile.Opacity = projectile.timeLeft / 35f;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            return projectile.RotatingHitboxCollision(targetHitbox.TopLeft(), targetHitbox.Size());
        }

        public override bool ShouldUpdatePosition() => true;

        public override Color? GetAlpha(Color lightColor) => Color.Lerp(Color.OrangeRed, Color.Yellow, projectile.identity / 7f % 1f) * projectile.Opacity;

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            if (projectile.timeLeft >= 34f)
                return false;

            Main.spriteBatch.SetBlendState(BlendState.Additive);

            float progress = (33f - projectile.timeLeft) / 33f;

            Texture2D texture = ModContent.GetTexture(Texture);
            Texture2D bloomTexture = ModContent.GetTexture("CalamityMod/Particles/BloomCircle");

            Vector2 drawPosition = projectile.Center - Main.screenPosition;
            Vector2 origin = texture.Size() * 0.5f;
            Vector2 scale = new Vector2(Lerp(0.8f, 1.25f, (float)Pow(progress, 0.45f)), Lerp(0.6f, 0.24f, (float)Pow(progress, 0.4f))) * projectile.scale;

            // Draw an inner bloom circle to signify power at the center of the strike along with two thinner lines.
            Vector2 bloomScale = projectile.Size / bloomTexture.Size() * new Vector2(1f, 2f);
            Vector2 bloomOrigin = bloomTexture.Size() * 0.5f;
            Main.spriteBatch.Draw(bloomTexture, drawPosition, null, Color.White * projectile.Opacity, projectile.rotation, bloomOrigin, bloomScale, 0, 0f);
            Main.spriteBatch.Draw(texture, drawPosition, null, projectile.GetAlpha(lightColor), projectile.rotation, origin, scale * new Vector2(projectile.width / 512f, 1f), 0, 0f);
            Main.spriteBatch.Draw(texture, drawPosition, null, Color.White * projectile.Opacity, projectile.rotation, origin, scale * new Vector2(projectile.width / 512f, 0.6f), 0, 0f);

            Main.spriteBatch.SetBlendState(BlendState.AlphaBlend);
            return false;
        }
    }
}
