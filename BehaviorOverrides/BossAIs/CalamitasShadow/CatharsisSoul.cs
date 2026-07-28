using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.CalamitasShadow
{
    public class CatharsisSoul : ModProjectile
    {
        public ref float Time => ref projectile.ai[0];

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Burning Soul of Catharsis");
            Main.projFrames[projectile.type] = 4;
            ProjectileID.Sets.TrailingMode[projectile.type] = 0;
            ProjectileID.Sets.TrailCacheLength[projectile.type] = 4;
        }

        public override void SetDefaults()
        {
            projectile.width = projectile.height = 28;
            projectile.hostile = true;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;
            projectile.penetrate = -1;
            projectile.Opacity = 0f;
            projectile.timeLeft = 160;
            cooldownSlot = 1;
        }

        public override void AI()
        {
            projectile.Opacity = Utils.InverseLerp(0f, 12f, Time, true) * Utils.InverseLerp(0f, 25f, projectile.timeLeft, true);
            projectile.rotation = projectile.velocity.ToRotation();
            projectile.spriteDirection = (Math.Cos(projectile.rotation) > 0f).ToDirectionInt();
            if (projectile.spriteDirection == -1)
                projectile.rotation += MathHelper.Pi;

            if (Time < 60f)
                projectile.velocity *= 0.98f;
            else
            {
                Player target = Main.player[Player.FindClosest(projectile.Center, 1, 1)];
                if (target.dead || !target.active)
                {    
                    projectile.Kill();
                    return; 
                }
                projectile.velocity = (projectile.velocity * 37f + projectile.SafeDirectionTo(target.Center) * 11f) / 38f;
            }

            Time++;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            float oldScale = projectile.scale;
            projectile.scale *= 1.2f;
            lightColor = Color.Lerp(lightColor, Color.Red, 0.9f);
            lightColor.A = 24;
            Utilities.DrawAfterimagesCentered(projectile, lightColor, ProjectileID.Sets.TrailingMode[projectile.type]);
            projectile.scale = oldScale;

            lightColor = Color.Lerp(lightColor, Color.White, 0.5f);
            lightColor.A = 24;
            Utilities.DrawAfterimagesCentered(projectile, lightColor, ProjectileID.Sets.TrailingMode[projectile.type]);

            return false;
        }

        public override bool CanDamage() => Time >= 60f && projectile.Opacity >= 0.65f;
    }
}
