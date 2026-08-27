using System;
using CalamityMod;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.HiveMind
{
    public class ShadeFire : ModProjectile
    {
        public ref float Time => ref projectile.ai[0];

        public const int Lifetime = 65;

        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Fire");
        }

        public override void SetDefaults()
        {
            projectile.width = 6;
            projectile.height = 6;
            projectile.hostile = true;
            projectile.ignoreWater = true;
            projectile.penetrate = -1;
            projectile.extraUpdates = 3;
            projectile.timeLeft = Lifetime;
            projectile.tileCollide = false;
            projectile.Calamity().canBreakPlayerDefense = true;
            cooldownSlot = 1;
        }

        public override void AI()
        {
            Lighting.AddLight(projectile.Center, projectile.Opacity * 0.15f, 0f, projectile.Opacity * 0.2f);
            if (projectile.timeLeft > Lifetime)
                projectile.timeLeft = Lifetime;

            // Start with a random color between green and a muted purple. The variance from this leads to a pseudo-gradient look.
            float lifetimeInterpolant = 1f - projectile.timeLeft / (float)Lifetime;
            float particleScale = MathHelper.Lerp(0.03f, 1.2f, (float)Math.Pow(lifetimeInterpolant, 0.53f));
            float opacity = Utils.InverseLerp(0.96f, 0.7f, lifetimeInterpolant, true) * 0.84f;
            float fadeToBlack = Utils.InverseLerp(5f, 32f, projectile.timeLeft, true);
            Color fireColor = Color.Lerp(Color.MediumPurple, Color.ForestGreen, Main.rand.NextFloat(0.2f, 0.67f));

            // Have the fire color dissipate into smoke as it reaches death.
            fireColor = Color.Lerp(fireColor, Color.MediumPurple, fadeToBlack);

            // Use a lime flame at the start of the flame's life, indicating extraordinary quantities of heat.
            fireColor = Color.Lerp(fireColor, Color.LimeGreen, Utils.InverseLerp(0.29f, 0f, lifetimeInterpolant, true) * 0.85f);

            fireColor = Color.Lerp(fireColor, Color.Green, 0.18f);

            // Emit light.
            Lighting.AddLight(projectile.Center, fireColor.ToVector3() * opacity);

            if (Main.rand.NextBool(2))
            {
                var particle = new HeavySmokeParticle(projectile.Center, projectile.velocity * 0.1f + Main.rand.NextVector2Circular(0.4f, 0.4f), fireColor, 30, particleScale, opacity, 0.05f, true, 0f, true);
                GeneralParticleHandler.SpawnParticle(particle);
            }
        }
    }
}
