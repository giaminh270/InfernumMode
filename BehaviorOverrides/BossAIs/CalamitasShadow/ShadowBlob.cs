using CalamityMod.NPCs;
using CalamityMod.Particles;
using InfernumMode.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.CalamitasShadow
{
    public class ShadowBlob : ModProjectile
    {
        public ref float Time => ref projectile.ai[0];

        public static NPC CalShadow => Main.npc[CalamityGlobalNPC.calamitas];

        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public override void SetStaticDefaults() => DisplayName.SetDefault("Shadow Blob");

        public override void SetDefaults()
        {
            projectile.width = projectile.height = 42;
            projectile.ignoreWater = true;
            projectile.tileCollide = false;
            projectile.penetrate = -1;
            projectile.timeLeft = 250;
            cooldownSlot = 1;
        }

        public override void AI()
        {
            // Disappear if the shadow is not present.
            if (CalamityGlobalNPC.calamitas == -1)
            {
                projectile.Kill();
                return;
            }

            if (Time >= 12f)
            {
                Vector2 idealVelocity = (CalShadow.Center - projectile.Center) * 0.1f;
                idealVelocity = idealVelocity.ClampMagnitude(20f, 50f);

                // Die if touching the shadow.
                if (projectile.WithinRange(CalShadow.Center, 45f))
                    projectile.Kill();

                projectile.velocity = Vector2.Lerp(projectile.velocity, idealVelocity, 0.18f);
            }
            else
                projectile.velocity *= 0.9f;

            Time++;

            // Create blob particles.
            InfernumFusableParticleManager.GetParticleSetByType<ShadowDemonParticleSet>()?.SpawnParticle(projectile.Center + Main.rand.NextVector2Circular(12f, 12f), (20f + projectile.velocity.Length()) * projectile.scale);
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor) => false;
    }
}
