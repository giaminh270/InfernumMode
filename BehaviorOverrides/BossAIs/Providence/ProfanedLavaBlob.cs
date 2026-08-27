using CalamityMod.Particles;
using InfernumMode.Particles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.Providence
{
    public class ProfanedLavaBlob : ModProjectile
    {
        public ref float Lifetime => ref projectile.ai[0];

        public ref float BlobSize => ref projectile.ai[1];

        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Lava Blob");
        }

        public override void SetDefaults()
        {
            projectile.width = projectile.height = 2;
            projectile.ignoreWater = true;
            projectile.tileCollide = false;
            projectile.hostile = true;
            projectile.Opacity = 0f;
            projectile.penetrate = -1;
            projectile.timeLeft = 3600;
            projectile.MaxUpdates = 2;
            cooldownSlot = 1;
        }

        public override void AI()
        {
            InfernumFusableParticleManager.GetParticleSetByType<ProfanedLavaParticleSet>()?.SpawnParticle(projectile.Center + Main.rand.NextVector2Circular(BlobSize, BlobSize) / 6f, BlobSize);
            if (projectile.timeLeft <= 3600f - Lifetime)
                projectile.Kill();

            projectile.velocity.Y += 0.06f;
            projectile.Size = Vector2.One * BlobSize * 0.707f;
        }
    }
}
