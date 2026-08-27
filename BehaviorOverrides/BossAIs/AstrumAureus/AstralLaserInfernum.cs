using System;
using CalamityMod;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.AstrumAureus
{
    public class AstralLaserInfernum : ModProjectile
    {
        public ref float Time => ref projectile.ai[0];

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Astral Laser Volley");
            Main.projFrames[projectile.type] = 3;
            ProjectileID.Sets.TrailCacheLength[projectile.type] = 4;
            ProjectileID.Sets.TrailingMode[projectile.type] = 0;
        }

        public override void SetDefaults()
        {
            projectile.width = 20;
            projectile.height = 20;
            projectile.hostile = true;
            projectile.ignoreWater = true;
            projectile.tileCollide = false;
            projectile.alpha = 255;
            projectile.penetrate = -1;
            projectile.timeLeft = 480;
            cooldownSlot = 1;
        }

        public override void AI()
        {
            projectile.frameCounter++;
            if (projectile.frameCounter > 8)
            {
                projectile.frame++;
                projectile.frameCounter = 0;
            }
            if (projectile.frame > 2)
                projectile.frame = 0;

            projectile.rotation = projectile.velocity.ToRotation();
            if (projectile.velocity.X < 0f)
            {
                projectile.spriteDirection = -1;
                projectile.rotation += MathHelper.Pi;
            }
            else
                projectile.spriteDirection = 1;

            Lighting.AddLight(projectile.Center, 0.3f, 0.3f, 0f);

            Time++;
            if (Time > 18f)
                projectile.alpha = Utils.Clamp(projectile.alpha - 3, 0, 255);
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            CalamityUtils.DrawAfterimagesCentered(projectile, ProjectileID.Sets.TrailingMode[projectile.type], lightColor, 1);
            return false;
        }

        public override bool CanHitPlayer(Player target) => projectile.timeLeft >= 85;

        public override Color? GetAlpha(Color lightColor)
        {
            if (projectile.timeLeft < 85)
            {
                byte b2 = (byte)(projectile.timeLeft * 3);
                byte a2 = (byte)(projectile.alpha * (b2 / 255f));
                return new Color(b2, b2, b2, a2);
            }
            return new Color(255, 255, 255, projectile.alpha);
        }
    }
}
