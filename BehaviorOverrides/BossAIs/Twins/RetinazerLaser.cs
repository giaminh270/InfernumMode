using CalamityMod;
using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework.Graphics;

namespace InfernumMode.BehaviorOverrides.BossAIs.Twins
{
    public class RetinazerLaser : ModProjectile
    {
        public ref float Time => ref projectile.ai[0];

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Death Laser");
            Main.projFrames[projectile.type] = 4;
        }

        public override void SetDefaults()
        {
            projectile.width = projectile.height = 30;
            projectile.hostile = true;
            projectile.ignoreWater = true;
            projectile.tileCollide = false;
            projectile.penetrate = -1;
            projectile.MaxUpdates = 2;
            projectile.timeLeft = 600;
            projectile.scale = 0.75f;
            projectile.Opacity = 0f;
        }
        public override void AI()
        {
            // Create a small puff of laser dust on the first frame.
            if (projectile.localAI[0] == 0f)
            {
                for (int i = 0; i < 18; i++)
                {
                    Dust laser = Dust.NewDustPerfect(projectile.Center + Main.rand.NextVector2Circular(8f, 8f) - projectile.velocity.SafeNormalize(Vector2.UnitY) * projectile.scale * 20f, 182);
                    laser.velocity = projectile.velocity.SafeNormalize(Vector2.UnitY).RotatedByRandom(0.19f) * Main.rand.NextFloat(4f, 12f);
                    laser.noGravity = true;
                    laser.scale *= 1.25f;
                }
                projectile.localAI[0] = 1f;
            }

            projectile.Opacity = MathHelper.Clamp(projectile.Opacity + 0.1f, 0f, 1f);
            projectile.rotation = projectile.velocity.ToRotation();
            projectile.frameCounter++;
            projectile.frame = projectile.frameCounter / 5 % Main.projFrames[projectile.type];

            Time++;
        }

        public override Color? GetAlpha(Color lightColor)
        {
            if (lightColor.A != 255)
                return lightColor * projectile.Opacity;

            return Color.White * projectile.Opacity;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            return CalamityUtils.CircularHitboxCollision(projHitbox.Center(), projectile.Size.Length() * 0.5f, targetHitbox);
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Vector2 backOffset = projectile.velocity.SafeNormalize(Vector2.Zero) * projectile.scale * -30f;
            Vector2 oldCenter = projectile.Center;

            float materializeInterpolant = Utils.InverseLerp(0f, 64f, Time, true);
            if (projectile.ai[1] == 0f)
                materializeInterpolant = 1f;

            for (int i = 0; i < (projectile.ai[1] == 0f ? 1 : 6); i++)
            {
                Vector2 drawOffset = (MathHelper.TwoPi * i / 6f).ToRotationVector2() * (1f - materializeInterpolant) * 12f + backOffset;
                lightColor = Color.Lerp(lightColor, Color.Wheat , (1f - materializeInterpolant) * 0.6f) * Utils.InverseLerp(0.1f, 0.5f, materializeInterpolant, true);
                projectile.Center = oldCenter + drawOffset;
                Utilities.DrawProjectileWithBackglowTemp(projectile, Color.OrangeRed  * (float)Math.Pow(materializeInterpolant, 4f), lightColor, 2f);
            }
            projectile.Center = oldCenter;
            return false;
        }
    }
}
