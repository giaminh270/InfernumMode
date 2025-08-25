using InfernumMode.BehaviorOverrides.BossAIs.Twins;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.Dragonfolly
{
    public class BigFollyFeather : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Feather");
            Main.projFrames[projectile.type] = 1;
        }

        public override void SetDefaults()
        {
            projectile.width = 36;
            projectile.height = 36;
            projectile.hostile = false;
            projectile.friendly = false;
            projectile.tileCollide = true;
            projectile.scale = 0.96f;
            projectile.timeLeft = 105;
        }

        public override void AI()
        {
            if (projectile.localAI[0] == 0f)
            {
                projectile.frame = Main.rand.Next(Main.projFrames[projectile.type]);
                projectile.localAI[0] = 1f;
            }

            projectile.velocity.Y += MathHelper.ToRadians(2.4f);
            Vector2 movementDirection = new Vector2(-(float)Math.Sin(projectile.velocity.Y * 2f) * 4f, Math.Abs((float)Math.Cos(projectile.velocity.Y * 2f)) * 6f);
            Vector2 collisionDirection = Collision.TileCollision(projectile.position, movementDirection, (int)(projectile.width * projectile.scale), (int)(projectile.height * projectile.scale));
            if (movementDirection != collisionDirection)
                projectile.velocity.Y = -1f;

            projectile.position += movementDirection;
            projectile.rotation = movementDirection.ToRotation() - MathHelper.PiOver4;

            if (projectile.timeLeft < 30)
                projectile.alpha = Utils.Clamp(projectile.alpha + 10, 0, 255);

            if (Main.rand.NextBool(8))
            {
                Dust redLightning = Dust.NewDustPerfect(projectile.Center, 60, Main.rand.NextVector2Circular(3f, 3f));
                redLightning.scale *= Main.rand.NextFloat(1.85f, 2.25f);
                redLightning.fadeIn = 1f;
                redLightning.noGravity = true;
            }
        }

        public override void Kill(int timeLeft)
        {
            // Explode and fire a red lightning spark at the nearest player.
            for (int i = 0; i < 7; i++)
            {
                Dust redLightning = Dust.NewDustPerfect(projectile.Center, 267, Main.rand.NextVector2Circular(2f, 2f));
                redLightning.velocity *= Main.rand.NextFloat(1f, 1.7f);
                redLightning.scale *= Main.rand.NextFloat(1.85f, 2.25f);
                redLightning.color = Color.Lerp(Color.White, Color.Red, Main.rand.NextFloat(0.5f, 1f));
                redLightning.fadeIn = 1f;
                redLightning.noGravity = true;
            }

            for (float speed = 2f; speed <= 6f; speed += 0.7f)
            {
                float lifePersistance = Main.rand.NextFloat(0.8f, 1.7f);
                for (int i = 0; i < 40; i++)
                {
                    Dust energy = Dust.NewDustPerfect(projectile.Center, 267);
                    energy.velocity = (MathHelper.TwoPi * i / 40f).ToRotationVector2() * speed;
                    energy.noGravity = true;
                    energy.color = Main.hslToRgb(Main.rand.NextFloat(0f, 0.08f), 0.85f, 0.6f);
                    energy.fadeIn = lifePersistance;
                    energy.scale = 1.4f;
                }
            }

            Main.PlaySound(SoundID.DD2_KoboldExplosion, projectile.Center);

            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            Player target = Main.player[Player.FindClosest(projectile.Center, 1, 1)];
            Vector2 lightningVelocity = projectile.SafeDirectionTo(target.Center) * 1.35f;
            int lightning = Utilities.NewProjectileBetter(projectile.Center, lightningVelocity, ModContent.ProjectileType<RedLightning>(), 250, 0f);
            if (Main.projectile.IndexInRange(lightning))
            {
                Main.projectile[lightning].ai[0] = Main.projectile[lightning].velocity.ToRotation();
                Main.projectile[lightning].ai[1] = Main.rand.Next(100);
            }
        }
    }
}
