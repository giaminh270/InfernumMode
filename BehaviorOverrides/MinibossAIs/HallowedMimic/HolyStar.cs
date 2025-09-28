using InfernumMode;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.MinibossAIs.HallowedMimic
{
    public class HolyStar : ModProjectile
    {
        public ref float Time => ref projectile.ai[0];

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Holy Star");
            ProjectileID.Sets.TrailingMode[projectile.type] = 2;
            ProjectileID.Sets.TrailCacheLength[projectile.type] = 3;
        }

        public override void SetDefaults()
        {
            projectile.width = projectile.height = 30;
            projectile.hostile = true;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;
            projectile.timeLeft = 300;
            projectile.penetrate = -1;
        }

        public override void AI()
        {
            Time++;

            projectile.tileCollide = Time >= 105f;
            projectile.rotation += projectile.velocity.X * 0.05f;
            projectile.Opacity = Utils.InverseLerp(0f, 15f, Time, true);

            // Release stardust.
            if (Main.rand.NextBool(10))
                Dust.NewDust(projectile.position, projectile.width, projectile.height, 58, projectile.velocity.X * 0.5f, projectile.velocity.Y * 0.5f, 150, default, 1.2f);
            if (Main.rand.NextBool(20))
                Gore.NewGore(projectile.position, projectile.velocity * 0.2f, Main.rand.Next(16, 18), 1f);
        }

        public override Color? GetAlpha(Color lightColor) => Color.White * projectile.Opacity;

        public override bool CanDamage() => projectile.Opacity >= 1f;

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Utilities.DrawAfterimagesCentered(projectile, lightColor, ProjectileID.Sets.TrailingMode[projectile.type], 3);
            return false;
        }
    }
}
