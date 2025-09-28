using InfernumMode;
using CalamityMod;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.MinibossAIs.CrimsonMimic
{
    public class LifeDrainingField : ModProjectile
    {
        public float Radius => Utilities.Remap(Time, 5f, 64f, 1f, 250f) * Utils.InverseLerp(0f, 30f, projectile.timeLeft, true);

        public ref float Time => ref projectile.ai[0];

        public const int Lifetime = 240;

        public override void SetStaticDefaults() => DisplayName.SetDefault("Life-Draining Field");

        public override void SetDefaults()
        {
            projectile.width = 22;
            projectile.height = 22;
            projectile.hostile = true;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;
            projectile.timeLeft = Lifetime;
            projectile.netImportant = true;
            projectile.penetrate = -1;
        }

        public override void AI()
        {
            Time++;

            Player target = Main.player[Player.FindClosest(projectile.Center, 1, 1)];
            float flySpeed = Utilities.Remap(Time, 0f, 40f, 0f, 4.5f);
            if (Time >= 6f)
                projectile.velocity = Vector2.Zero.MoveTowards(target.Center - projectile.Center, flySpeed);


        }

        public override Color? GetAlpha(Color lightColor) => Color.White * projectile.Opacity;
        
        public override bool CanDamage() => Time >= 45f;

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            return CalamityUtils.CircularHitboxCollision(projectile.Center, Radius, targetHitbox);
        }
    }
}
