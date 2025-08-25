using CalamityMod;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.SlimeGod
{
    public class EvilBolt : ModProjectile
    {
        public ref float Time => ref projectile.ai[1];

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Evil Fire");
            Main.projFrames[projectile.type] = 4;
            ProjectileID.Sets.TrailingMode[projectile.type] = 2;
            ProjectileID.Sets.TrailCacheLength[projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            projectile.width = 18;
            projectile.height = 18;
            projectile.hostile = true;
            projectile.ignoreWater = true;
            projectile.tileCollide = false;
            projectile.penetrate = -1;
            projectile.timeLeft = 360;
            projectile.Opacity = 0f;
        }
        
        public override void AI()
        {
            projectile.frameCounter++;
            projectile.frame = projectile.frameCounter / 5 % Main.projFrames[projectile.type];

            // Determine opacity and rotation.
            projectile.Opacity = Utils.InverseLerp(0f, 20f, Time, true);
            projectile.velocity *= 1.04f;
            projectile.rotation = projectile.velocity.ToRotation() - MathHelper.PiOver2;
            Lighting.AddLight(projectile.Center, 0f, 0f, 0.5f * projectile.Opacity);

            Time++;
        }

        public override bool CanHitPlayer(Player target) => projectile.Opacity >= 0.6f;

        public override Color? GetAlpha(Color lightColor)
        {
            Color c = projectile.ai[0] == 1f ? Color.Yellow : Color.Lime;
            return c * projectile.Opacity;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            CalamityUtils.DrawAfterimagesCentered(projectile, ProjectileID.Sets.TrailingMode[projectile.type], lightColor, 1);
            return false;
        }
    }
}
