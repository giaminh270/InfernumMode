using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.Twins
{
    public class CursedFireballBomb : ModProjectile
    {
        public ref float Time => ref projectile.ai[0];

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Cursed Fireball Bomb");
            Main.projFrames[projectile.type] = 6;
            ProjectileID.Sets.TrailingMode[projectile.type] = 0;
            ProjectileID.Sets.TrailCacheLength[projectile.type] = 4;
        }

        public override void SetDefaults()
        {
            projectile.width = projectile.height = 36;
            projectile.hostile = true;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;
            projectile.penetrate = -1;
            projectile.timeLeft = 300;
            projectile.Opacity = 0f;
        }

        public override void AI()
        {
            // Create a burst of dust on the first frame.
            if (projectile.localAI[0] == 0f)
            {
                for (int i = 0; i < 40; i++)
                {
                    Vector2 dustVelocity = projectile.velocity.SafeNormalize(Vector2.Zero).RotatedByRandom(0.35f) * Main.rand.NextFloat(1.8f, 3f);
                    int randomDustType = Main.rand.NextBool() ? 107 : 110;

                    Dust cursedFire = Dust.NewDustDirect(projectile.position, projectile.width, projectile.height, randomDustType, dustVelocity.X, dustVelocity.Y, 200, default, 1.7f);
                    cursedFire.position = projectile.Center + Main.rand.NextVector2Circular(projectile.width, projectile.width);
                    cursedFire.noGravity = true;
                    cursedFire.velocity *= 3f;

                    cursedFire = Dust.NewDustDirect(projectile.position, projectile.width, projectile.height, randomDustType, dustVelocity.X, dustVelocity.Y, 100, default, 0.8f);
                    cursedFire.position = projectile.Center + Main.rand.NextVector2Circular(projectile.width, projectile.width);
                    cursedFire.velocity *= 2f;

                    cursedFire.noGravity = true;
                    cursedFire.fadeIn = 1f;
                    cursedFire.color = Color.Green * 0.5f;
                }

                for (int i = 0; i < 20; i++)
                {
                    Vector2 dustVelocity = projectile.velocity.SafeNormalize(Vector2.Zero).RotatedByRandom(0.35f) * Main.rand.NextFloat(1.8f, 3f);
                    int randomDustType = Main.rand.NextBool() ? 107 : 110;

                    Dust cursedFire = Dust.NewDustDirect(projectile.position, projectile.width, projectile.height, randomDustType, dustVelocity.X, dustVelocity.Y, 0, default, 2f);
                    cursedFire.position = projectile.Center + Vector2.UnitX.RotatedByRandom(MathHelper.Pi).RotatedBy(projectile.velocity.ToRotation()) * projectile.width / 3f;
                    cursedFire.noGravity = true;
                    cursedFire.velocity *= 0.5f;
                }

                projectile.localAI[0] = 1f;
            }

            projectile.Opacity = MathHelper.Clamp(projectile.Opacity + 0.04f, 0f, 0.8f);

            projectile.velocity *= 1.018f;
            projectile.frameCounter++;
            projectile.frame = projectile.frameCounter / 5 % Main.projFrames[projectile.type];
            projectile.rotation = projectile.velocity.ToRotation() - MathHelper.PiOver2;

            // Collide with tiles after alive for long enough.
            projectile.tileCollide = Time >= 20f;
            Time++;

            Lighting.AddLight(projectile.Center, Color.Yellow.ToVector3() * 0.5f);
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            lightColor = Color.Lerp(lightColor, Color.White, 0.8f);
            lightColor.A = 0;
            Utilities.DrawAfterimagesCentered(projectile, lightColor, ProjectileID.Sets.TrailingMode[projectile.type]);
            return false;
        }

        public override void Kill(int timeLeft)
        {
            for (int i = 0; i < 25; i++)
            {
                Vector2 dustVelocity = Main.rand.NextVector2Unit() * Main.rand.NextFloat(2f, 5f);
                int randomDustType = Main.rand.NextBool() ? 107 : 110;
                Dust cursedFire = Dust.NewDustDirect(projectile.position, projectile.width, projectile.height, randomDustType, dustVelocity.X, dustVelocity.Y, 0, default, 2f);
                cursedFire.noGravity = true;
                cursedFire.velocity *= 0.67f;
            }
        }

        public override bool CanDamage() => false;
    }
}
