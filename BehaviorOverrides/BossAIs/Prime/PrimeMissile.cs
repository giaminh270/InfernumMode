using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.Prime
{
    public class PrimeMissile : ModProjectile
    {
        public ref float Time => ref projectile.ai[0];

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Missile");
            Main.projFrames[projectile.type] = 3;
            ProjectileID.Sets.TrailingMode[projectile.type] = 0;
            ProjectileID.Sets.TrailCacheLength[projectile.type] = 3;
        }

        public override void SetDefaults()
        {
            projectile.width = projectile.height = 22;
            projectile.hostile = true;
            projectile.friendly = false;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;
            projectile.penetrate = -1;
            projectile.timeLeft = 300;
            cooldownSlot = 1;
        }

        public override void AI()
        {
            if (projectile.velocity.Length() < 14f)
                projectile.velocity *= 1.02f;

            projectile.frameCounter++;
            projectile.frame = projectile.frameCounter / 5 % Main.projFrames[projectile.type];
            projectile.rotation = projectile.velocity.ToRotation() + MathHelper.PiOver2;

            // Emit light.
            Lighting.AddLight(projectile.Center, Color.Red.ToVector3());

            // Interact with tiles after enough time has passed.
            projectile.tileCollide = Time > 75f;

            // Very, very weakly home in on players.
            Player target = Main.player[Player.FindClosest(projectile.Center, 1, 1)];
            if (target.active && !target.dead)
            {
                float oldSpeed = projectile.velocity.Length();
                projectile.velocity = (projectile.velocity * 90f + projectile.SafeDirectionTo(target.Center) * oldSpeed) / 91f;
                projectile.velocity = projectile.velocity.SafeNormalize(Vector2.UnitY) * oldSpeed;
            }

            Time++;
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            // Explode when a tile is hit.
            Main.PlaySound(SoundID.Item14, projectile.Center);

            for (int i = 0; i < 12; i++)
            {
                int randomDustType = Main.rand.NextBool(2) ? 222 : 219;
                Dust fire = Dust.NewDustDirect(projectile.position, projectile.width, projectile.height, randomDustType, 0f, 0f, 100, default, 1f);
                fire.velocity *= 3f;
                fire.noGravity = true;
                if (Main.rand.NextBool(2))
                {
                    fire.scale = 0.5f;
                    fire.fadeIn = 1f + Main.rand.Next(10) * 0.1f;
                }
            }
            for (int i = 0; i < 15; i++)
            {
                int randomDustType = Main.rand.NextBool(2) ? 222 : 219;
                Dust fire = Dust.NewDustDirect(projectile.position, projectile.width, projectile.height, randomDustType, 0f, 0f, 100, default, 1.3f);
                fire.noGravity = true;
                fire.velocity *= 5f;

                fire = Dust.NewDustDirect(projectile.position, projectile.width, projectile.height, randomDustType, 0f, 0f, 100, default, 1f);
                fire.noGravity = true;
                fire.velocity *= 2f;
            }

            return true;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Utilities.DrawAfterimagesCentered(projectile, Color.White, ProjectileID.Sets.TrailingMode[projectile.type]);
            return false;
        }
    }
}
