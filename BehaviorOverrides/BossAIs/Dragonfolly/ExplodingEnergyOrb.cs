using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.Dragonfolly
{
    public class ExplodingEnergyOrb : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Plasma Orb");
        }

        public override void SetDefaults()
        {
            projectile.width = 36;
            projectile.height = 36;
            projectile.hostile = false;
            projectile.friendly = false;
            projectile.tileCollide = true;
            projectile.scale = 0.96f;
            projectile.timeLeft = 90;
        }

        public override void AI()
        {
            projectile.rotation = projectile.velocity.ToRotation() + MathHelper.PiOver4;

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
            // Explode and fire a burst of sparks at the nearest player.
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
            for (int i = 0; i < 5; i++)
            {
                float shootOffsetAngle = MathHelper.Lerp(-0.56f, 0.56f, i / 4f);
                Vector2 lightningVelocity = projectile.SafeDirectionTo(target.Center).RotatedBy(shootOffsetAngle) * 8f;
                Utilities.NewProjectileBetter(projectile.Center, lightningVelocity, ModContent.ProjectileType<RedSpark>(), 240, 0f);
            }
        }
    }
}
