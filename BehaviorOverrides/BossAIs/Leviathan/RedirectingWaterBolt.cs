using CalamityMod;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.Leviathan
{
    public class RedirectingWaterBolt : ModProjectile
    {
        public ref float Time => ref projectile.ai[1];

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Water Spear");
            ProjectileID.Sets.TrailingMode[projectile.type] = 0;
            ProjectileID.Sets.TrailCacheLength[projectile.type] = 4;
        }

        public override void SetDefaults()
        {
            projectile.width = 14;
            projectile.height = 14;
            projectile.hostile = true;
            projectile.ignoreWater = true;
            projectile.tileCollide = false;
            projectile.penetrate = -1;
            projectile.timeLeft = 300;
            projectile.Opacity = 0f;
            cooldownSlot = 1;
        }

        public override void AI()
        {
            // Create ice dust on the first frame.
            if (projectile.localAI[1] == 0f)
            {
                for (int i = 0; i < 5; i++)
                {
                    Dust ice = Dust.NewDustDirect(projectile.position, projectile.width, projectile.height, 33, 0f, 0f, 100, default, 2f);
                    ice.velocity *= 3f;
                    if (Main.rand.NextBool())
                    {
                        ice.scale = 0.5f;
                        ice.fadeIn = Main.rand.NextFloat(1f, 2f);
                    }
                }
                projectile.localAI[1] = 1f;
            }

            // Determine opacity and rotation.
            projectile.Opacity = Utils.InverseLerp(0f, 30f, projectile.timeLeft, true) * Utils.InverseLerp(0f, 30f, Time, true);
            projectile.rotation = projectile.velocity.ToRotation() + MathHelper.PiOver2;

            // Decide frames.
            projectile.frameCounter++;
            if (projectile.frameCounter >= 8)
            {
                projectile.frame = (projectile.frame + 1) % Main.projFrames[projectile.type];
                projectile.frameCounter = 0;
            }

            // Redirect towards the player.
            if (Time < 54f)
            {
                float inertia = 8f;
                float oldSpeed = projectile.velocity.Length();
                Player target = Main.player[Player.FindClosest(projectile.Center, 1, 1)];
                Vector2 homingVelocity = projectile.SafeDirectionTo(target.Center) * oldSpeed;

                if (!projectile.WithinRange(target.Center, 270f))
                {
                	projectile.velocity = (projectile.velocity * (inertia - 1f) + homingVelocity) / inertia;
                	projectile.velocity = projectile.velocity.SafeNormalize(-Vector2.UnitY) * oldSpeed;
                }
            }
            else if (projectile.velocity.Length() < 23.5f)
                projectile.velocity *= 1.015f;

            Time++;

            Lighting.AddLight(projectile.Center, 0f, 0f, 0.5f * projectile.Opacity);
        }

        public override bool CanHitPlayer(Player target) => projectile.Opacity >= 0.6f;

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            lightColor.R = (byte)(255 * projectile.Opacity);
            lightColor.G = (byte)(255 * projectile.Opacity);
            lightColor.B = (byte)(255 * projectile.Opacity);
            CalamityUtils.DrawAfterimagesCentered(projectile, ProjectileID.Sets.TrailingMode[projectile.type], lightColor, 1);
            return false;
        }

        public override void Kill(int timeLeft)
        {
            for (int k = 0; k < 3; k++)
                Dust.NewDust(projectile.position + projectile.velocity, projectile.width, projectile.height, 33, projectile.oldVelocity.X * 0.5f, projectile.oldVelocity.Y * 0.5f);
        }
    }
}
