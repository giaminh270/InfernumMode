using CalamityMod;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.SupremeCalamitas
{
    public class RedirectingLostSoulProj : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Burning Soul");
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
            projectile.timeLeft = 230;
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
                float offsetInterpolant = (float)Math.Cos(projectile.whoAmI % 6f / 6f + projectile.position.X / 380f + projectile.position.Y / 160f);
                projectile.velocity = projectile.velocity.RotatedBy(MathHelper.Pi * offsetInterpolant / 120f) * 0.98f;
            }

            if (canMoveTowardsTarget)
            {
                int targetIndex = (int)projectile.ai[0];
                Vector2 idealVelocity = projectile.velocity;
                if (projectile.hostile && Main.player.IndexInRange(targetIndex))
                {
                    idealVelocity = projectile.SafeDirectionTo(Main.player[targetIndex].Center) * 41f;
                    if (projectile.localAI[0] > 0f)
                        idealVelocity *= projectile.localAI[0];
                }

                float amount = MathHelper.Lerp(0.056f, 0.12f, Utils.InverseLerp(stopMovingTime, 30f, projectile.timeLeft, true));
                projectile.velocity = Vector2.SmoothStep(projectile.velocity, idealVelocity, amount);
            }
            projectile.Opacity = Utils.InverseLerp(240f, 220f, projectile.timeLeft, true) * Utils.InverseLerp(0f, 25f, projectile.timeLeft, true);

            projectile.rotation = projectile.velocity.ToRotation();
            projectile.spriteDirection = (Math.Cos(projectile.rotation) > 0f).ToDirectionInt();
            if (projectile.spriteDirection == -1)
                projectile.rotation += MathHelper.Pi;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            // Draw a brief telegraph line.
            float telegraphInterpolant = Utils.InverseLerp(300f, 275f, projectile.timeLeft, true);
            if (telegraphInterpolant < 1f)
            {
                Color telegraphColor = Color.Red * (float)Math.Sqrt(telegraphInterpolant);
                float telegraphWidth = CalamityUtils.Convert01To010(telegraphInterpolant) * 3f;
                Main.spriteBatch.DrawLineBetter(projectile.Center, projectile.Center + projectile.velocity.SafeNormalize(Vector2.UnitY) * 3600f, telegraphColor, telegraphWidth);
            }

            float oldScale = projectile.scale;
            projectile.scale *= 1.2f;
            lightColor = Color.Lerp(lightColor, Color.Red, 0.9f);
            lightColor.A = 128;
            Utilities.DrawAfterimagesCentered(projectile, lightColor, ProjectileID.Sets.TrailingMode[projectile.type]);
            projectile.scale = oldScale;

            lightColor = Color.Lerp(lightColor, Color.White, 0.5f);
            lightColor.A = 128;
            Utilities.DrawAfterimagesCentered(projectile, lightColor, ProjectileID.Sets.TrailingMode[projectile.type]);

            return false;
        }

        public override bool CanDamage() => projectile.alpha < 20;
    }
}
