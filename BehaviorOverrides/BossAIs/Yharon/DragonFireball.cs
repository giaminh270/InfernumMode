using CalamityMod;
using CalamityMod.Buffs.DamageOverTime;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace InfernumMode.BehaviorOverrides.BossAIs.Yharon
{
    public class DragonFireball : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Fireball");
            Main.projFrames[projectile.type] = 4;
        }

        public override void SetDefaults()
        {
            projectile.width = 36;
            projectile.height = 36;
            projectile.penetrate = -1;
            projectile.hostile = true;
            projectile.tileCollide = false;
            projectile.timeLeft = 180;
			projectile.Opacity = 0f;
            projectile.Calamity().canBreakPlayerDefense = true;
			cooldownSlot = 1;
		}

        public override void AI()
        {
			// Fade in and determine rotation.
			projectile.alpha = Utils.Clamp(projectile.alpha - 40, 0, 255);
			projectile.rotation = projectile.velocity.ToRotation();
			// Emit light.
			Lighting.AddLight(projectile.Center, 1.1f, 0.9f, 0.4f);

			// Create fire and smoke dust effects.
			projectile.localAI[0] += 1f;
			if (projectile.localAI[0] % 12f == 11f)
			{
				for (int i = 0; i < 12; i++)
				{
					Vector2 fireSpawnOffset = Vector2.UnitX * projectile.width * -0.5f;
					fireSpawnOffset += -Vector2.UnitY.RotatedBy(MathHelper.TwoPi * i / 12f) * new Vector2(8f, 16f);
					fireSpawnOffset = fireSpawnOffset.RotatedBy((double)(projectile.rotation - 1.57079637f));
					Dust fire = Dust.NewDustDirect(projectile.Center, 0, 0, 6, 0f, 0f, 160, default, 1f);
					fire.scale = 1.1f;
					fire.noGravity = true;
					fire.position = projectile.Center + fireSpawnOffset;
					fire.velocity = projectile.velocity * 0.1f;
					fire.velocity = Vector2.Normalize(projectile.Center - projectile.velocity * 3f - fire.position) * 1.25f;
				}
			}
			if (Main.rand.NextBool(4))
			{
				Vector2 offsetDirection = -Vector2.UnitX.RotatedByRandom(MathHelper.Pi / 12f).RotatedBy(projectile.velocity.ToRotation());
				Dust smoke = Dust.NewDustDirect(projectile.position, projectile.width, projectile.height, 31, 0f, 0f, 100, default, 1f);
				smoke.velocity *= 0.1f;
				smoke.position = projectile.Center + offsetDirection * projectile.width / 2f;
				smoke.fadeIn = 0.9f;
			}
			if (Main.rand.NextBool(32))
			{
				Vector2 offsetDirection = -Vector2.UnitX.RotatedByRandom(MathHelper.Pi / 8f).RotatedBy(projectile.velocity.ToRotation());
				Dust smoke = Dust.NewDustDirect(projectile.position, projectile.width, projectile.height, 31, 0f, 0f, 155, default, 0.8f);
				smoke.velocity *= 0.3f;
				smoke.position = projectile.Center + offsetDirection * projectile.width / 2f;
				if (Main.rand.NextBool(2))
					smoke.fadeIn = 1.4f;
			}
			if (Main.rand.NextBool(2))
			{
				for (int i = 0; i < 2; i++)
				{
					Vector2 offsetDirection = -Vector2.UnitX.RotatedByRandom(MathHelper.PiOver4).RotatedBy((double)projectile.velocity.ToRotation());
					Dust fire = Dust.NewDustDirect(projectile.position, projectile.width, projectile.height, 6, 0f, 0f, 0, default, 1.2f);
					fire.velocity *= 0.3f;
					fire.noGravity = true;
					fire.position = projectile.Center + offsetDirection * projectile.width / 2f;
					if (Main.rand.NextBool(2))
						fire.fadeIn = 1.4f;
				}
			}

			projectile.frameCounter++;
            projectile.frame = projectile.frameCounter / 3 % Main.projFrames[projectile.type];
        }

        public override void OnHitPlayer(Player target, int damage, bool crit)
        {
            target.AddBuff(ModContent.BuffType<LethalLavaBurn>(), 180);
        }
    }
}
