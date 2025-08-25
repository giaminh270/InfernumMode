using CalamityMod;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework.Graphics;
using System.IO;

namespace InfernumMode.BehaviorOverrides.BossAIs.Cultist
{
    public class DarkBoltLarge : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Dark Bolt");
            ProjectileID.Sets.TrailingMode[projectile.type] = 2;
            ProjectileID.Sets.TrailCacheLength[projectile.type] = 6;
        }

        public override void SetDefaults()
        {
            projectile.width = projectile.height = 22;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;
            projectile.hostile = true;
            projectile.timeLeft = 230;
            projectile.Opacity = 0f;
            projectile.penetrate = -1;
        }

        public override void AI()
        {
            bool canRotate = false;
            bool canMoveTowardsTarget = false;
            float stopMovingTime = 140f;
            float dissipateTime = 30f;

            if (projectile.timeLeft > stopMovingTime)
            {
                canRotate = true;
            }
            else if (projectile.timeLeft > dissipateTime)
            {
                canMoveTowardsTarget = true;
            }
            if (canRotate)
            {
                float offsetInterpolant = (float)Math.Cos(projectile.whoAmI % 6f / 6f + projectile.position.X / 320f + projectile.position.Y / 160f);
                projectile.velocity = projectile.velocity.RotatedBy(MathHelper.Pi * offsetInterpolant / 120f) * 0.98f;
            }

            if (canMoveTowardsTarget)
            {
                int targetIndex = (int)projectile.ai[0];
                Vector2 idealVelocity = projectile.velocity;
                if (projectile.hostile && Main.player.IndexInRange(targetIndex))
                    idealVelocity = projectile.SafeDirectionTo(Main.player[targetIndex].Center) * 34.5f;

                float amount = MathHelper.Lerp(0.056f, 0.12f, Utils.InverseLerp(stopMovingTime, 30f, projectile.timeLeft, true));
                projectile.velocity = Vector2.SmoothStep(projectile.velocity, idealVelocity, amount);
            }
            projectile.Opacity = Utils.InverseLerp(240f, 220f, projectile.timeLeft, true);
            projectile.rotation = projectile.velocity.ToRotation();
        }

        public override Color? GetAlpha(Color lightColor) => Color.White * projectile.Opacity;

        public override bool CanDamage() => projectile.Opacity >= 1f;

        

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            lightColor = Color.Lerp(lightColor, new Color(0.45f, 1f, 0.64f), 0.55f);
            lightColor.A /= 3;
            Utilities.DrawAfterimagesCentered(projectile, lightColor, ProjectileID.Sets.TrailingMode[projectile.type]);
            return false;
        }
    }
}
