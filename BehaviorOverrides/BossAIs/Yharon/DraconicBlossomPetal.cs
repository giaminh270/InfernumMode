using CalamityMod.DataStructures;
using InfernumMode.DataStructures;
using InfernumMode.ExtraTextures;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.Yharon
{
    public class DraconicBlossomPetal : ModProjectile, IAdditiveDrawer
    {
        public ref float Time => ref projectile.ai[0];

        public ref float Lifetime => ref projectile.ai[1];

        public static int LifetimeExtensionInWater => 600;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Draconic Blossom Petal");
            Main.projFrames[projectile.type] = 3;
        }

        public override void SetDefaults()
        {
            projectile.width = 12;
            projectile.height = 12;
            projectile.penetrate = -1;
            projectile.timeLeft = 7200;
            projectile.Opacity = 0f;
            projectile.scale = Main.rand?.NextFloat(0.3f, 0.9f) ?? 0.6f;
            projectile.ignoreWater = true;
            projectile.tileCollide = true;
            projectile.netImportant = true;
        }

        public override void AI()
        {
            // Pick a frame and initial lifetime.
            if (projectile.localAI[0] == 0f)
            {
                float lifetimeInterpolant = (float)Math.Pow(Main.rand.NextFloat(), 0.6f);
                Lifetime = (int)MathHelper.Lerp(240f, 480f, lifetimeInterpolant);
                projectile.frame = Main.rand.Next(Main.projFrames[projectile.type]);
                projectile.localAI[0] = 1f;
            }

            // Increase the lifetime if the blossom isn't fading out and lands on water.
            if (Collision.WetCollision(projectile.TopLeft, projectile.width, projectile.height))
            {
                projectile.velocity.X *= 0.96f;
                projectile.velocity.Y = MathHelper.Lerp(projectile.velocity.Y, -2f, 0.09f);
                if (projectile.Opacity >= 0.8f && Lifetime <= LifetimeExtensionInWater)
                {
                    Lifetime += LifetimeExtensionInWater;
                    projectile.netUpdate = true;
                }
            }

            // Slowly fall down.
            float maxSpeed = MathHelper.Lerp(0.7f, 5.4f, projectile.scale);
            projectile.velocity.X = MathHelper.Lerp(projectile.velocity.X, Math.Sign(projectile.velocity.X) * maxSpeed * 1.2f, 0.006f);
            projectile.velocity.Y = MathHelper.Lerp(projectile.velocity.Y + 0.02f, maxSpeed, 0.05f);

            // Rotate.
            projectile.rotation += projectile.velocity.Y * 0.01f;

            // Handle fade effects.
            float fadeIn = Utils.InverseLerp(5f, 25f, Time, true);
            float fadeOut = Utils.InverseLerp(0f, 54f, Lifetime - Time, true);
            projectile.Opacity = fadeIn * fadeOut;

            Time++;
            if (Time >= Lifetime)
                projectile.Kill();
        }

        public override Color? GetAlpha(Color lightColor) => Color.White * projectile.Opacity;

        // Prevent instant death upon touching tiles if the blossom has existed for a brief enough period of time.
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (Time <= 20f)
                projectile.Kill();

            projectile.velocity.X = 0f;
            return Time <= 4f;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Texture2D texture = ModContent.GetTexture(Texture);
            Rectangle frame = texture.Frame(1, Main.projFrames[projectile.type], 0, projectile.frame);
            Vector2 drawPosition = projectile.Center - Main.screenPosition;
            Main.spriteBatch.Draw(texture, drawPosition, frame, projectile.GetAlpha(lightColor), projectile.rotation, frame.Size() * 0.5f, projectile.scale, 0, 0f);
            return false;
        }

        public void AdditiveDraw(SpriteBatch spriteBatch)
        {
            Texture2D circle = InfernumTextureRegistry.LaserCircle;

            float backglowOpacity = projectile.Opacity * 0.3f;
            Vector2 drawPosition = projectile.Center - Main.screenPosition;
            float backglowScale = MathHelper.Lerp(0.85f, 1.15f, (float)Math.Cos(projectile.identity * 7f + Main.GlobalTime * 1.6f) * 0.5f + 0.5f) * projectile.scale * projectile.Opacity * 0.45f;
            Color backglowColor = Color.Lerp(Color.Red, Color.Pink, projectile.Opacity) * backglowOpacity;
            spriteBatch.Draw(circle, drawPosition, null, backglowColor, 0f, circle.Size() * 0.5f, backglowScale, 0, 0f);
            spriteBatch.Draw(circle, drawPosition, null, Color.Magenta * backglowOpacity * 1.1f, 0f, circle.Size() * 0.5f, backglowScale * 0.75f, 0, 0f);
        }
    }
}
