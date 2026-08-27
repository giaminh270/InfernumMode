using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.Twins
{
    public class SpazmatismFlamethrower : ModProjectile
    {
        public ref float Timer => ref projectile.ai[0];

        public const int Lifetime = 150;

        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Cursed Flamethrower");
        }

        public override void SetDefaults()
        {
            projectile.width = projectile.height = 28;
            projectile.hostile = true;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;
            projectile.penetrate = -3;
            projectile.MaxUpdates = 10;
            projectile.timeLeft = Lifetime;
            cooldownSlot = 1;
        }

        public override void AI()
        {
            // Emit light.
            Lighting.AddLight(projectile.Center, projectile.Opacity * 0.3f, projectile.Opacity * 0.65f, projectile.Opacity * 0.03f);

            // Emit fire particles.
            float lifetimeInterpolant = Timer / Lifetime;
            float particleScale = MathHelper.Lerp(0.03f, 1.67f, (float)Math.Pow(lifetimeInterpolant, 0.64f));
            float opacity = Utils.InverseLerp(0.96f, 0.7f, lifetimeInterpolant, true);
            float fadeToBlack = Utils.InverseLerp(0.5f, 0.84f, lifetimeInterpolant, true);

            // Start with a random color between green and red. The variance from this leads to a pseudo-gradient look.
            Color fireColor = Color.Lerp(Color.Lime, Color.Red, Main.rand.NextFloat(0.2f, 0.8f));

            // Have the fire color dissipate into smoke as it reaches death.
            fireColor = Color.Lerp(fireColor, Color.DarkGray, fadeToBlack);

            // Use a blue flame at the start of the flame's life, indicating cursed flames.
            fireColor = Color.Lerp(fireColor, Color.ForestGreen, Utils.InverseLerp(0.5f, 0.2f, lifetimeInterpolant, true));

            // Emit light.
            Lighting.AddLight(projectile.Center, fireColor.ToVector3() * opacity);

            if (projectile.timeLeft % 5 == 0)
            {
                var particle = new HeavySmokeParticle(projectile.Center, projectile.velocity * 0.1f + Main.rand.NextVector2Circular(2f, 2f), fireColor, 16, particleScale, opacity, 0.05f, Main.rand.NextFloat() > Math.Pow(fadeToBlack, 0.2), 0f, true);
                GeneralParticleHandler.SpawnParticle(particle);
            }

            // Adjust the hitbox.
            projectile.Size = Vector2.One * particleScale * 40f;

            Timer++;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            return Utilities.CircularCollision(projectile.Center - projectile.velocity * projectile.MaxUpdates * 2f, targetHitbox, projectile.Size.Length() * 0.707f);
        }
    }
}
