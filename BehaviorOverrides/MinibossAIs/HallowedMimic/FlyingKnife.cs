using InfernumMode;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.MinibossAIs.HallowedMimic
{
    public class FlyingKnife : ModProjectile
    {
        public ref float Time => ref projectile.ai[0];

        public const int Lifetime = 270;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Flying Knife");
            ProjectileID.Sets.TrailingMode[projectile.type] = 2;
            ProjectileID.Sets.TrailCacheLength[projectile.type] = 4;
        }

        public override void SetDefaults()
        {
            projectile.width = projectile.height = 26;
            projectile.hostile = true;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;
            projectile.timeLeft = Lifetime;
            projectile.netImportant = true;
            projectile.penetrate = -1;
        }

        public override void AI()
        {
            Time++;
            projectile.rotation += projectile.velocity.X * 0.035f;
            projectile.Opacity = Utils.InverseLerp(0f, 15f, Time, true);

            // Harass the nearest player.
            Player target = Main.player[Player.FindClosest(projectile.Center, 1, 1)];

            float inertia = projectile.WithinRange(target.Center, 250f) ? 15f : 45f;
            if (Time >= 60f)
                projectile.velocity = (projectile.velocity * (inertia - 1f) + projectile.SafeDirectionTo(target.Center) * 14f) / inertia;
            if ((Time % 15f == 14f || projectile.velocity.Length() < 9f) && !projectile.WithinRange(target.Center, 270f))
            {
                Vector2 impulse = projectile.SafeDirectionTo(target.Center) * 5f;
                projectile.velocity = (projectile.velocity + impulse).ClampMagnitude(9.1f, 21.5f);
                projectile.netUpdate = true;
            }
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
