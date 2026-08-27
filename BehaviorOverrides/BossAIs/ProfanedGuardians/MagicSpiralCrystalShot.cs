using CalamityMod;
using InfernumMode.ExtraTextures;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.ProfanedGuardians
{
    public class MagicSpiralCrystalShot : ModProjectile
    {
        public static readonly Color[] ColorSet = new Color[]
        {
            // Pale pink crystal.
            new Color(181, 136, 177),

            // Profaned fire.
            new Color(255, 191, 73),

            // Yellow-orange crystal.
            new Color(255, 194, 161),
        };

        public ref float Timer => ref projectile.ai[0];
        public Color StreakBaseColor => CalamityUtils.MulticolorLerp(projectile.localAI[0] % 0.999f, ColorSet);
        public ref float Direction => ref projectile.ai[1];

        public Vector2 InitialVelocity;
        public Vector2 InitialCenter;
        public float RotationAmount => MathHelper.Lerp(0.034f, 0.001f, Timer / 300f);

        public override string Texture => "CalamityMod/Projectiles/StarProj";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Crystalline Light");
            ProjectileID.Sets.TrailingMode[projectile.type] = 2;
            ProjectileID.Sets.TrailCacheLength[projectile.type] = 24;
        }

        public override void SetDefaults()
        {
            projectile.width = projectile.height = 30;
            projectile.alpha = 255;
            projectile.penetrate = -1;
            projectile.timeLeft = 240;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;
            projectile.hostile = true;
            cooldownSlot = 1;
        }

        public override void AI()
        {
            if (Timer == 0)
            {
                InitialVelocity = projectile.velocity;
                InitialCenter = projectile.Center;
                projectile.velocity = Vector2.Zero;
            }

            if (Timer < 20)
            {
                projectile.Center = InitialCenter;
                Timer++;
                return;
            }
            if (Timer == 20)
                projectile.velocity = InitialVelocity;

            projectile.velocity = projectile.velocity.RotatedBy(Direction * RotationAmount);

            projectile.velocity *= 1.01f;

            if (projectile.timeLeft < 15)
                projectile.damage = 0;

            projectile.Opacity = MathHelper.Clamp(projectile.Opacity + 0.1f, 0f, 1f);
            projectile.rotation = projectile.velocity.ToRotation() + MathHelper.PiOver2;
            Timer++;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            if (Timer < 30)
                DrawLines(Main.spriteBatch);

            Texture2D streakTexture = Main.projectileTexture[projectile.type];
            for (int i = 1; i < projectile.oldPos.Length; i++)
            {
                if (projectile.oldPos[i - 1] == Vector2.Zero || projectile.oldPos[i] == Vector2.Zero)
                    continue;

                float completionRatio = i / (float)projectile.oldPos.Length;
                float fade = (float)Math.Pow(completionRatio, 2f);
                float scale = projectile.scale * MathHelper.Lerp(1.3f, 0.9f, Utils.InverseLerp(0f, 0.24f, completionRatio, true)) *
                    MathHelper.Lerp(0.9f, 0.56f, Utils.InverseLerp(0.5f, 0.78f, completionRatio, true));
                Color drawColor = Color.Lerp(StreakBaseColor, new Color(229, 255, 255), fade) * (1f - fade) * projectile.Opacity;
                drawColor.A = 0;

                Vector2 drawPosition = projectile.oldPos[i - 1] + projectile.Size * 0.5f - Main.screenPosition;
                Vector2 drawPosition2 = Vector2.Lerp(drawPosition, projectile.oldPos[i] + projectile.Size * 0.5f - Main.screenPosition, 0.5f);
                Main.spriteBatch.Draw(streakTexture, drawPosition, null, drawColor, projectile.oldRot[i], streakTexture.Size() * 0.5f, scale, SpriteEffects.None, 0f);
                Main.spriteBatch.Draw(streakTexture, drawPosition2, null, drawColor, projectile.oldRot[i], streakTexture.Size() * 0.5f, scale, SpriteEffects.None, 0f);
            }
            return false;
        }

        // TODO: Optimize this to need to draw less things.
        public void DrawLines(SpriteBatch spriteBatch)
        {
            // The total number of lines to draw.
            int totalDrawPoints = 80;

            Texture2D lineTexture = InfernumTextureRegistry.Pixel;
            // Initialize the previous point + velocity with the projectiles initial ones.
            Vector2 previousDrawPoint = InitialCenter;
            Vector2 previousDrawVelocity = InitialVelocity;

            float lineOpacityScalar = (float)Math.Sin(Timer / 30 * MathHelper.Pi);

            // Loop through the total number of draw points.
            for (int i = 0; i < totalDrawPoints; i++)
            {
                // Get the rotation amount. This is the same as used by the projectiles movement.
                float rotationAmount = MathHelper.Lerp(0.034f, 0.001f, i / 300f);
                // Get a velocity, from rotating the last one by the rotation amount. This is how the projectile moves.
                Vector2 drawVelocity = previousDrawVelocity.RotatedBy(Direction * rotationAmount);
                // And also scale it.
                drawVelocity *= 1.01f;
                // Create a "center" to draw at by adding the current velocity to the previous position.
                Vector2 drawPoint = previousDrawPoint + drawVelocity;
                // Get the direction between the two points.
                Vector2 direction = previousDrawPoint - drawPoint;
                // Get the length of this. This doesn't fully connect normally so adding 0.5 to the length is a shitty
                // hack to make them work. However, this means you cannot use additive drawing due to the overlap being visible.
                float length = direction.Length() + 0.5f;
                // Use this to create a rectangle.
                Rectangle rectangle = new Rectangle(0, 0, (int)length, 4);
                // Set the color of the line.
                Color lineColor = Color.Lerp(Color.HotPink, StreakBaseColor, lineOpacityScalar) * 1.3f;
                // Make it fade out for the last bit.
                if (totalDrawPoints - i <= 50)
                {
                    float interpolant = ((float)i - (totalDrawPoints - 50)) / (totalDrawPoints - (totalDrawPoints - 50));
                    lineColor = Color.Lerp(lineColor, Color.Transparent, interpolant);
                }
                lineColor *= lineOpacityScalar;
                // Draw the line.
                spriteBatch.Draw(lineTexture, previousDrawPoint - Main.screenPosition, rectangle, lineColor, direction.ToRotation(), rectangle.Size() * 0.5f, 1f, SpriteEffects.None, 0f);
                // Update the previous points.
                previousDrawPoint = drawPoint;
                previousDrawVelocity = drawVelocity;
            }
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            for (int i = 0; i < 3; i++)
            {
                if (targetHitbox.Intersects(Utils.CenteredRectangle(projectile.oldPos[i] + projectile.Size * 0.5f, projectile.Size)))
                    return true;
            }
            return false;
        }
    }
}
