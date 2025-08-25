using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.CeaselessVoid
{
    public class CelestialBarrage : ModProjectile
    {
        public float Power => projectile.ai[1];

        public ref float Time => ref projectile.ai[0];

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Otherwordly Bolt");
            ProjectileID.Sets.TrailingMode[projectile.type] = 2;
            ProjectileID.Sets.TrailCacheLength[projectile.type] = 4;
        }

        public override void SetDefaults()
        {
            projectile.width = 18;
			projectile.height = 18;
            projectile.hostile = true;
            projectile.ignoreWater = true;
            projectile.penetrate = -1;
            projectile.tileCollide = false;
            projectile.timeLeft = 240;
        }

        public override void AI()
        {
            // Home in on the target before accelerating.
            if (Time <= 45f)
            {
                float homeSpeed = Power * 8.5f + 21.5f;
                Player target = Main.player[Player.FindClosest(projectile.Center, 1, 1)];
                if (!projectile.WithinRange(target.Center, 200f))
                    projectile.velocity = (projectile.velocity * 14f + projectile.SafeDirectionTo(target.Center) * homeSpeed) / 15f;
            }
            else if (projectile.velocity.Length() < 24f)
                projectile.velocity *= Power * 0.025f + 1.02f;

            projectile.Opacity = Utils.InverseLerp(0f, 20f, Time, true);
            projectile.rotation = projectile.velocity.ToRotation() + MathHelper.PiOver2;
            Time++;
        }

        public override bool CanDamage() => projectile.Opacity >= 0.9f;

        public override Color? GetAlpha(Color lightColor) => new Color(255, 108, 50, 0) * projectile.Opacity;

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Texture2D texture = Main.projectileTexture[projectile.type];

            for (int i = 0; i < 5; i++)
                Utilities.DrawAfterimagesCentered(projectile, lightColor, ProjectileID.Sets.TrailingMode[projectile.type], 1, texture);
            return false;
        }
    }
}
