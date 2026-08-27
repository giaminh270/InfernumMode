using System;
using CalamityMod;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ModLoader;

namespace InfernumMode
{

    public static class ScreenEffectSystem
    {
        #region Blur
        private static Vector2 BlurPosition;

        private static float BlurIntensity;

        private static int BlurLifeTime;

        private static int BlurTime;

        private static bool BlurActive;

        public const float BaseScaleAmount = 0.04f;
        private const float BaseBlurAmount = 4f;

        private static float BlurLifetimeRatio => (float)BlurTime / BlurLifeTime;

        /// <summary>
        /// Call this to set a blur effect. Any existing ones will be replaced.
        /// </summary>
        /// <param name="position">The focal position, in world co-ordinates</param>
        /// <param name="intensity">How intense to make the scale and blur effect. A 0-1 range should be used</param>
        /// <param name="lifetime">How long the effect should last</param>
        public static void SetBlurEffect(Vector2 position, float intensity, int lifetime)
        {
            if (CalamityConfig.Instance.DisableScreenShakes)
                return;

            BlurPosition = position;
            BlurIntensity = intensity;
            BlurLifeTime = lifetime;
            BlurTime = 0;
            BlurActive = true;

            EnsureCaptureIsForced();
        }
        #endregion

        #region Flash
        private static Vector2 FlashPosition;

        private static float FlashIntensity;

        private static int FlashLifeTime;

        private static int FlashTime;

        private static bool FlashActive;

        private static float FlashLifetimeRatio => (float)FlashTime / FlashLifeTime;

        /// <summary>
        /// Call this to set a flash effect. Any existing ones will be replaced.
        /// </summary>
        /// <param name="position">The focal position, in world co-ordinates</param>
        /// <param name="intensity">How bright to make the flash. A 0-1 range should be used</param>
        /// <param name="lifetime">How long the effect should last</param>
        public static void SetFlashEffect(Vector2 position, float intensity, int lifetime)
        {
            if (CalamityConfig.Instance.DisableScreenShakes)
                return;

            FlashPosition = position;
            FlashIntensity = intensity;
            FlashLifeTime = lifetime;
            FlashTime = 0;
            FlashActive = true;

            EnsureCaptureIsForced();
        }
        #endregion

        #region Forcing FilterManager to actually capture the scene
        // Terraria.Graphics.Effects.FilterManager.BeginCapture() (1.3.5.3) only sets
        // Main.screenTarget as the active render target - the step that lets EndCapture
        // (and therefore our render hook) see a fresh copy of the scene - when it has an
        // active filter OR something subscribed to OnPostDraw:
        //
        //   if (this._activeFilterCount == 0 && this.OnPostDraw == null)
        //       this._captureThisFrame = false;
        //   else { this._captureThisFrame = true; SetRenderTarget(Main.screenTarget); ... }
        //
        // _activeFilterCount is private and out of reach, but OnPostDraw is a public event,
        // so subscribing a no-op handler to it while a blur/flash is active is enough to
        // force real capturing to happen. This also makes CanCapture() return true on its
        // own (it already checks "OnPostDraw != null"), so no separate hook for that is needed.
        private static bool subscribedForCapture;

        private static void NoOpForceCapture()
        {
            // Intentionally empty - merely being subscribed is what matters.
        }

        private static void EnsureCaptureIsForced()
        {
            if (!subscribedForCapture)
            {
                Filters.Scene.OnPostDraw += NoOpForceCapture;
                subscribedForCapture = true;
            }
        }

        private static void ReleaseCaptureIfIdle()
        {
            if (subscribedForCapture && !AnyBlurOrFlashActive())
            {
                Filters.Scene.OnPostDraw -= NoOpForceCapture;
                subscribedForCapture = false;
            }
        }
        #endregion

        public static void Load()
        {
        }

        public static void Unload()
        {
            if (subscribedForCapture)
            {
                Filters.Scene.OnPostDraw -= NoOpForceCapture;
                subscribedForCapture = false;
            }

            BlurActive = false;
            FlashActive = false;
        }

        public static bool AnyBlurOrFlashActive() => BlurActive || FlashActive;

        public static void Update()
        {
            if (BlurActive)
            {
                if (BlurTime >= BlurLifeTime)
                {
                    BlurActive = false;
                    BlurTime = 0;
                }
                else
                    BlurTime++;
            }

            if (FlashActive)
            {
                if (FlashTime >= FlashLifeTime)
                {
                    FlashActive = false;
                    FlashTime = 0;
                }
                else
                    FlashTime++;
            }

            ReleaseCaptureIfIdle();
        }

        internal static void DrawBlurEffect(RenderTarget2D sceneSource)
        {
            if (sceneSource is null || sceneSource.IsDisposed)
                return;

            if (BlurActive)
            {
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive);

                // Draw the scene 6 times, getting progressively larger and more transparent.
                for (int i = -3; i <= 3; i++)
                {
                    if (i == 0)
                        continue;

                    // Increase the scale based on the intensity and lifetime of the blur.
                    float scaleAmount = BaseScaleAmount * BlurIntensity;
                    float blurAmount = BaseBlurAmount * BlurIntensity;
                    float scale = 1f + scaleAmount * (1f - BlurLifetimeRatio) * i / blurAmount;
                    Color drawColor = Color.White * 0.42f;
                    // Not doing this causes it to not properly fit on the screen. This extends it to be 100 extra in either direction.
                    Rectangle frameOffset = new Rectangle(-100, -100, Main.screenWidth + 200, Main.screenHeight + 200);
                    // Use that and the position to set the origin to the draw position.
                    Vector2 origin = BlurPosition + new Vector2(100) - Main.screenPosition;
                    Main.spriteBatch.Draw(sceneSource, BlurPosition - Main.screenPosition, frameOffset, drawColor, 0f, origin, scale, SpriteEffects.None, 0f);
                }

                Main.spriteBatch.End();
            }

            // This draws over the blur, so doing them together isn't really ideal.
            else if (FlashActive)
            {
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive);

                Color drawColor = new Color(1f, 1f, 1f, MathHelper.Clamp(MathHelper.Lerp(0.5f, 1f, (1f - FlashLifetimeRatio) * FlashIntensity), 0f, 1f));

                // Not doing this causes it to not properly fit on the screen. This extends it to be 100 extra in either direction.
                Rectangle frameOffset = new Rectangle(-100, -100, Main.screenWidth + 200, Main.screenHeight + 200);
                // Use that and the position to set the origin to the draw position.
                Vector2 origin = FlashPosition + new Vector2(100) - Main.screenPosition;
                for (int i = 0; i < 2; i++)
                    Main.spriteBatch.Draw(sceneSource, FlashPosition - Main.screenPosition, frameOffset, drawColor, 0f, origin, 1f, SpriteEffects.None, 0f);

                Main.spriteBatch.End();
            }
        }
    }
}
