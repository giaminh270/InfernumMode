using CalamityMod;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.AstrumDeus
{
    public class AstralTelegraphLine : ModProjectile
    {
        public ref float Time => ref projectile.ai[0];

        public ref float Lifetime => ref projectile.ai[1];

        public override void SetStaticDefaults() => DisplayName.SetDefault("Telegraph");

        public override void SetDefaults()
        {
            projectile.width = projectile.height = 2;
            projectile.ignoreWater = true;
            projectile.tileCollide = false;
            projectile.penetrate = -1;
            projectile.timeLeft = 900;
        }

        public override void AI()
        {
            projectile.Opacity = CalamityUtils.Convert01To010(Time / Lifetime) * 3f;
            if (projectile.Opacity > 1f)
                projectile.Opacity = 1f;
            if (Time >= Lifetime)
                projectile.Kill();

            Time++;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            float telegraphWidth = MathHelper.Lerp(0.3f, 6f, CalamityUtils.Convert01To010(Time / Lifetime));

            // Draw a telegraph line outward.
            Vector2 start = projectile.Center - projectile.velocity.SafeNormalize(Vector2.UnitY) * 3000f;
            Vector2 end = projectile.Center + projectile.velocity.SafeNormalize(Vector2.UnitY) * 3000f;
            Main.spriteBatch.DrawLineBetter(start, end, Color.Lerp(Color.Orange, Color.Red, 0.5f), telegraphWidth);
            return false;
        }

        public override bool ShouldUpdatePosition() => false;
    }
}