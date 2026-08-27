using CalamityMod.Particles;
using CalamityMod;
using InfernumMode.Particles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.Destroyer
{
    public class EnergyBlast2 : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Energy Blast");
            Main.projFrames[projectile.type] = 6;
        }

        public override void SetDefaults()
        {
            projectile.width = projectile.height = 76;
            projectile.hostile = true;
            projectile.ignoreWater = true;
            projectile.tileCollide = false;
            projectile.penetrate = -1;
            projectile.timeLeft = 240;
            cooldownSlot = 1;
        }

        public override void AI()
        {
            projectile.frameCounter++;
            projectile.frame = projectile.frameCounter / 5 % Main.projFrames[projectile.type];

            Player closestPlayer = Main.player[Player.FindClosest(projectile.Center, 1, 1)];
            projectile.velocity = (projectile.velocity * 59f + projectile.SafeDirectionTo(closestPlayer.Center) * 9f) / 60f;

            if (projectile.WithinRange(closestPlayer.Center, 300f) || projectile.Opacity < 1f)
                projectile.Opacity = MathHelper.Clamp(projectile.Opacity - 0.05f, 0f, 1f);

            if (projectile.Opacity <= 0f)
                projectile.Kill();

            Lighting.AddLight(projectile.Center, Vector3.One);
        }

        // Explode on death.
        public override void Kill(int timeLeft)
        {
            // Create particles and sounds at the explosion point.
            for (int i = 0; i < 20; i++)
            {
                Color fireColor = Main.rand.NextBool() ? Color.Yellow : Color.Red;
                CloudParticle fireCloud = new CloudParticle(projectile.Center, Main.rand.NextVector2Circular(8f, 8f), fireColor, Color.DarkGray, 54, Main.rand.NextFloat(2f, 3f));
                GeneralParticleHandler.SpawnParticle(fireCloud);
            }
            Main.PlaySound(InfernumMode.CalamityMod.GetLegacySoundSlot(SoundType.Item, "Sounds/Item/FlareSound"), projectile.Center);
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            for (int i = 0; i < 35; i++)
            {
                Vector2 fireVelocity = Main.rand.NextVector2Unit() * Main.rand.NextFloat(14f, 21f);
                Utilities.NewProjectileBetter(projectile.Center, fireVelocity, ModContent.ProjectileType<EnergySpark>(), DestroyerHeadBehaviorOverride.EnergySparkDamage, 0f);
            }
        }
    }
}
