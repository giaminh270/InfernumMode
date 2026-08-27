using CalamityMod.Particles;
using InfernumMode.Particles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.Plantera
{
    public class ExplodingFlower : ModProjectile
    {
        public override void SetStaticDefaults() => DisplayName.SetDefault("Flower");

        public override void SetDefaults()
        {
            projectile.width = projectile.height = 22;
            projectile.hostile = true;
            projectile.tileCollide = false;
            projectile.timeLeft = 150;
            projectile.penetrate = -1;
            cooldownSlot = 1;
        }

        public override void AI()
        {
            projectile.scale = Utils.InverseLerp(150f, 130f, projectile.timeLeft, true);
            projectile.Opacity = Utils.InverseLerp(0f, 20f, projectile.timeLeft, true);
        }

        public override void Kill(int timeLeft)
        {
            for (int i = 0; i < 10; i++)
            {
                CloudParticle sporeGas = new CloudParticle(projectile.Center, Main.rand.NextVector2Circular(3f, 3f), Color.Pink, Color.Lime, 36, Main.rand.NextFloat(0.6f, 0.85f));
                GeneralParticleHandler.SpawnParticle(sporeGas);
            }

            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            Player closestPlayer = Main.player[Player.FindClosest(projectile.Center, 1, 1)];
            for (int i = 0; i < 3; i++)
            {
                float offsetAngle = MathHelper.Lerp(-0.38f, 0.38f, i / 2f);
                Vector2 petalShootVelocity = projectile.SafeDirectionTo(closestPlayer.Center, -Vector2.UnitY).RotatedBy(offsetAngle) * 7.5f;
                Utilities.NewProjectileBetter(projectile.Center, petalShootVelocity, ModContent.ProjectileType<Petal>(), PlanteraBehaviorOverride.PetalDamage, 0f);
            }
        }

        public override Color? GetAlpha(Color lightColor) => Color.White;
    }
}
