using CalamityMod;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.Providence
{
    public class CrystalTelegraphLine : ModProjectile
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
            float telegraphWidth = MathHelper.Lerp(0.3f, 3f, CalamityUtils.Convert01To010(Time / Lifetime));

            // Draw a telegraph line outward.
            Color telegraphColor = Main.dayTime ? Color.Yellow : Color.Lerp(Color.Cyan, Color.Green, 0.15f);
            Vector2 start = projectile.Center;
            Vector2 end = projectile.Center + projectile.velocity.SafeNormalize(Vector2.UnitY) * 3000f;
            Main.spriteBatch.DrawLineBetter(start, end, telegraphColor, telegraphWidth);
            return false;
        }
    }
}