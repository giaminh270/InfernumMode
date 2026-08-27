using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.AquaticScourge
{
    public class AquaticScourgeGore : ModProjectile
    {
        public ref float Time => ref projectile.ai[0];

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Gore");
            Main.projFrames[projectile.type] = 3;
            ProjectileID.Sets.TrailCacheLength[projectile.type] = 2;
            ProjectileID.Sets.TrailingMode[projectile.type] = 0;
        }

        public override void SetDefaults()
        {
            projectile.width = 24;
            projectile.height = 24;
            projectile.hostile = true;
            projectile.tileCollide = true;
            projectile.ignoreWater = true;
            projectile.timeLeft = 240;
            projectile.Opacity = 0f;
            
            cooldownSlot = 1;
        }

        public override void AI()
        {
            // Fade in.
            projectile.Opacity = Utils.InverseLerp(0f, 8f, Time, true);

            // Initialize the rotation.
            if (projectile.localAI[0] == 0f)
            {
                projectile.rotation = Main.rand.NextFloat(MathHelper.TwoPi);
                projectile.frame = Main.rand.Next(Main.projFrames[projectile.type]);
                projectile.localAI[0] = 1f;
            }

            // Fall downward.
            projectile.velocity.X *= 0.987f;
            projectile.velocity.Y = MathHelper.Clamp(projectile.velocity.Y + 0.25f, -20f, 9.6f);
            projectile.rotation += projectile.velocity.X * 0.014f;
            Time++;
        }
    }
}
