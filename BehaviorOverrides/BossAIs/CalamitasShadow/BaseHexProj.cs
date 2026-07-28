using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.CalamitasShadow
{
    public abstract class BaseHexProj : ModProjectile
    {
        public Player Owner => Main.player[projectile.owner];

        public ref float HorizontalOffset => ref projectile.ai[0];

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Calamitous Hex");
            Main.projFrames[projectile.type] = 20;
        }

        public override void SetDefaults()
        {
            projectile.width = 66;
            projectile.height = 86;
            projectile.ignoreWater = true;
            projectile.tileCollide = false;
            projectile.penetrate = -1;
            cooldownSlot = 1;
        }

        public override void AI()
        {
            projectile.frameCounter++;
            projectile.frame = projectile.frameCounter / 5;
            if (projectile.frame >= Main.projFrames[projectile.type])
                projectile.Kill();

            projectile.Bottom = Owner.Top + Vector2.UnitX * HorizontalOffset;
        }

        public override Color? GetAlpha(Color lightColor) => Color.White * projectile.Opacity;
    }
}
