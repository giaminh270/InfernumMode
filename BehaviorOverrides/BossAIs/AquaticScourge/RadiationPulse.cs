using CalamityMod;
using CalamityMod.Projectiles.BaseProjectiles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;

namespace InfernumMode.BehaviorOverrides.BossAIs.AquaticScourge
{
    public class RadiationPulse : BaseMassiveExplosionProjectile
    {
        public override int Lifetime => 75;

        public override bool UsesScreenshake => false;

        public override Color GetCurrentExplosionColor(float pulseCompletionRatio) => Color.Lerp(Color.YellowGreen * 1.2f, Color.MediumPurple, MathHelper.Clamp(pulseCompletionRatio * 1.8f, 0f, 1f));

        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public static float AcidWaterAccelerationFactor => 8f;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Radiation Pulse");
        }

        public override void SetDefaults()
        {
            projectile.Calamity().canBreakPlayerDefense = true;
            projectile.width = projectile.height = 2;
            projectile.hostile = true;
            projectile.ignoreWater = true;
            projectile.tileCollide = false;
            projectile.penetrate = -1;
            projectile.timeLeft = Lifetime;
            cooldownSlot = 1;
        }

        public override void PostAI()
        {
            // Make the sulphuric water effects go up far more quickly when inside the area of the pulse.
            AquaticScourgeHeadBehaviorOverride.ApplySulphuricPoisoningBoostToPlayersInArea(projectile.Center, CurrentRadius * projectile.scale * 0.325f, AcidWaterAccelerationFactor);
        }

        public override bool CanDamage() => false;
    }
}
