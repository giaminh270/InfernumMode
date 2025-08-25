using CalamityMod;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.SlimeGod
{
    public class DeceleratingCrimulanGlob : ModProjectile
    {
        public ref float Time => ref projectile.ai[1];

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Unstable Crimulan Glob");
            ProjectileID.Sets.TrailingMode[projectile.type] = 2;
            ProjectileID.Sets.TrailCacheLength[projectile.type] = 4;
        }

        public override void SetDefaults()
        {
            projectile.width = 30;
            projectile.height = 30;
            projectile.hostile = true;
            projectile.ignoreWater = true;
            projectile.tileCollide = false;
            projectile.penetrate = -1;
            projectile.timeLeft = 240;
            projectile.Opacity = 0f;
        }
        
        public override void AI()
        {
            // Determine opacity and rotation.
            projectile.Opacity = Utils.InverseLerp(0f, 30f, projectile.timeLeft, true) * Utils.InverseLerp(0f, 20f, Time, true);

            if (projectile.localAI[0] == 0f)
            {
                projectile.rotation = projectile.velocity.ToRotation() + MathHelper.PiOver2;
                projectile.localAI[0] = 1f;
            }
            projectile.rotation += projectile.velocity.X * 0.02f;

            if (projectile.velocity.Length() < 21.5f)
                projectile.velocity *= 1.02f;

            Lighting.AddLight(projectile.Center, 0f, 0f, 0.5f * projectile.Opacity);
            Time++;
        }

        public override bool CanHitPlayer(Player target) => projectile.Opacity >= 0.6f;

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
