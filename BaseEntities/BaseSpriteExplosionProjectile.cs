using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace InfernumMode.BaseEntities
{
    public abstract class BaseSpriteExplosionProjectile : ModProjectile
    {
        public abstract Color ExplosionColor { get; }

        public abstract int GetFrameUpdateRate { get; }

        public ref float Time => ref projectile.ai[0];

        public override string Texture => "InfernumMode/BaseEntities/BaseSpriteExplosionProjectile";

        public override void SetDefaults()
        {
            Main.projFrames[projectile.type] = 7;
            projectile.width = projectile.height = 4;
            projectile.penetrate = -1;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;
            projectile.timeLeft = GetFrameUpdateRate * Main.projFrames[projectile.type] + 1;
        }

        public override void AI()
        {
            projectile.frame = (int)(Time / GetFrameUpdateRate) % Main.projFrames[projectile.type];
            Time++;
        }

        public sealed override Color? GetAlpha(Color lightColor) => ExplosionColor * projectile.Opacity;
    }
}
