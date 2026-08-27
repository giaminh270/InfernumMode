using CalamityMod;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.DesertScourge
{
    public class SandBlastInfernum : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Sand Blast");
            ProjectileID.Sets.TrailCacheLength[projectile.type] = 4;
            ProjectileID.Sets.TrailingMode[projectile.type] = 0;
        }

        public override void SetDefaults()
        {
            projectile.width = 10;
            projectile.height = 10;
            projectile.hostile = true;
            projectile.ignoreWater = true;
            projectile.tileCollide = false;
            projectile.penetrate = -1;
            projectile.timeLeft = 1200;
            projectile.Opacity = 0f;
            cooldownSlot = 1;
        }

        public override void AI()
        {
            // Decide rotation.
            projectile.rotation = projectile.velocity.ToRotation() + MathHelper.PiOver2;

            // Fade in.
            projectile.Opacity = MathHelper.Clamp(projectile.Opacity + 0.1f, 0f, 1f);

            Dust sand = Dust.NewDustDirect(projectile.TopLeft, projectile.width, projectile.height, 85, 0f, 0f, 100, default, 0.8f);
            sand.noGravity = true;
            sand.velocity *= 0f;
        }

        public override void Kill(int timeLeft) => Main.PlaySound(SoundID.Item14, projectile.position);

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            lightColor.R = (byte)(255 * projectile.Opacity);
            lightColor.G = (byte)(255 * projectile.Opacity);
            lightColor.B = (byte)(255 * projectile.Opacity);
            CalamityUtils.DrawAfterimagesCentered(projectile, ProjectileID.Sets.TrailingMode[projectile.type], lightColor, 1);
            return false;
        }
    }
}
