using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;

namespace InfernumMode.Graphics
{
    /// <summary>
    /// Lightweight port of Luminance.Core.Graphics.ManagedRenderTarget for tModLoader 0.11 / Terraria 1.3.
    /// Wraps a RenderTarget2D, auto-recreates on screen resize when requested, and registers with RenderTargetManager.
    /// </summary>
    public class ManagedRenderTarget : IDisposable
    {
        public delegate RenderTarget2D RenderTargetInitializationAction(int width, int height);

        public RenderTarget2D Target { get; private set; }

        /// <summary>If true, Target is recreated when Main.screenWidth/Height change.</summary>
        public bool ShouldResetUponScreenResize { get; }

        public RenderTargetInitializationAction InitializationAction { get; }

        /// <summary>True while the target is valid and not disposed.</summary>
        public bool IsReady => Target != null && !Target.IsDisposed;

        public int Width => IsReady ? Target.Width : 0;
        public int Height => IsReady ? Target.Height : 0;

        private Vector2 lastScreenSize;

        public ManagedRenderTarget(bool shouldResetUponScreenResize, RenderTargetInitializationAction initializationAction, bool createImmediately = true)
        {
            ShouldResetUponScreenResize = shouldResetUponScreenResize;
            InitializationAction = initializationAction ?? CreateScreenSizedTarget;
            lastScreenSize = new Vector2(Main.screenWidth, Main.screenHeight);

            RenderTargetManager.Register(this);

            if (createImmediately && !Main.dedServ)
                Recreate();
        }

        public static RenderTarget2D CreateScreenSizedTarget(int width, int height)
        {
            width = Math.Max(1, width);
            height = Math.Max(1, height);
            return new RenderTarget2D(
                Main.instance.GraphicsDevice,
                width,
                height,
                false,
                SurfaceFormat.Color,
                DepthFormat.None,
                0,
                RenderTargetUsage.PreserveContents);
        }

        /// <summary>Half-resolution target (useful for pixelation).</summary>
        public static RenderTarget2D CreateHalfScreenSizedTarget(int width, int height)
        {
            return CreateScreenSizedTarget(Math.Max(1, width / 2), Math.Max(1, height / 2));
        }

        public void Recreate()
        {
            if (Main.dedServ)
                return;

            DisposeTargetOnly();

            int w = Math.Max(1, Main.screenWidth);
            int h = Math.Max(1, Main.screenHeight);
            Target = InitializationAction(w, h);
            lastScreenSize = new Vector2(Main.screenWidth, Main.screenHeight);
        }

        /// <summary>Called by RenderTargetManager each frame (or on resize).</summary>
        public void CheckScreenResize()
        {
            if (!ShouldResetUponScreenResize || Main.dedServ)
                return;

            Vector2 current = new Vector2(Main.screenWidth, Main.screenHeight);
            if (current != lastScreenSize || !IsReady)
                Recreate();
        }

        /// <summary>
        /// Bind this target, clear to transparent (or given color).
        /// SpriteBatch should be closed before calling.
        /// </summary>
        public void SwapTo(Color? flushColor = null)
        {
            if (!IsReady)
                Recreate();
            if (!IsReady)
                return;

            GraphicsDevice device = Main.instance.GraphicsDevice;
            device.SetRenderTarget(Target);
            device.Clear(flushColor ?? Color.Transparent);
        }

        private void DisposeTargetOnly()
        {
            if (Target != null && !Target.IsDisposed)
            {
                Target.Dispose();
                Target = null;
            }
        }

        public void Dispose()
        {
            DisposeTargetOnly();
            RenderTargetManager.Unregister(this);
        }

        /// <summary>Implicit conversion so existing code can treat ManagedRenderTarget like RenderTarget2D.</summary>
        public static implicit operator RenderTarget2D(ManagedRenderTarget managed)
            => managed?.Target;
    }
}
