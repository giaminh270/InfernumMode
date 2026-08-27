using InfernumMode.Graphics.Interfaces;
using InfernumMode.ILEditingStuff;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using InfernumMode;

namespace InfernumMode.Graphics.Primitives
{

    public class PixelationRenderTargetManager : IHookEdit
    {
        #region Fields
        private Vector2 previousScreenSize;

        private static RenderTarget2D pixelRenderTarget;
        private static RenderTarget2D pixelRenderTargetBeforeNPCs;

        private static readonly List<IPixelPrimitiveDrawer> pixelPrimDrawersList = new List<IPixelPrimitiveDrawer>();
        private static readonly List<IPixelPrimitiveDrawer> pixelPrimDrawersListBeforeNPCs = new List<IPixelPrimitiveDrawer>();
        #endregion

        #region IHookEdit
        public void Load()
        {
            On.Terraria.Main.CheckMonoliths += DrawToCustomRenderTargets;
            On.Terraria.Main.DrawNPCs += DrawPixelRenderTarget;
            On.Terraria.Main.SetDisplayMode += OnSetDisplayMode;
            ResizePixelRenderTarget(true);
        }

        public void Unload()
        {
            On.Terraria.Main.CheckMonoliths -= DrawToCustomRenderTargets;
            On.Terraria.Main.DrawNPCs -= DrawPixelRenderTarget;
            On.Terraria.Main.SetDisplayMode -= OnSetDisplayMode;

            if (pixelRenderTarget != null && !pixelRenderTarget.IsDisposed)
            {
                pixelRenderTarget.Dispose();
                pixelRenderTarget = null;
            }
            if (pixelRenderTargetBeforeNPCs != null && !pixelRenderTargetBeforeNPCs.IsDisposed)
            {
                pixelRenderTargetBeforeNPCs.Dispose();
                pixelRenderTargetBeforeNPCs = null;
            }
        }
        #endregion

        #region Private methods
        private void OnSetDisplayMode(On.Terraria.Main.orig_SetDisplayMode orig, int width, int height, bool fullscreen)
        {
            orig(width, height, fullscreen);
            previousScreenSize = Vector2.Zero;
            ResizePixelRenderTarget(true);
        }

        private static bool AnyPrimsActive()
            => pixelPrimDrawersList.Count > 0 || pixelPrimDrawersListBeforeNPCs.Count > 0;

        private static void DrawScaledTargetClosed(RenderTarget2D target)
        {
            if (target == null || target.IsDisposed)
                return;

            Main.spriteBatch.Begin(
                SpriteSortMode.Immediate,
                BlendState.AlphaBlend,
                SamplerState.PointClamp,
                DepthStencilState.None,
                Main.instance.Rasterizer,
                null,
                Matrix.Identity);

            // Correct scale for both true half-res and capped high-res targets.
            Vector2 scale = new Vector2(
                (float)Main.screenWidth / target.Width,
                (float)Main.screenHeight / target.Height);

            Main.spriteBatch.Draw(target, Vector2.Zero, null, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
            Main.spriteBatch.End();
        }

        private static void BeginDefaultSpriteBatch()
        {
            Main.spriteBatch.Begin(
                SpriteSortMode.Deferred,
                BlendState.AlphaBlend,
                Main.DefaultSamplerState,
                DepthStencilState.None,
                Main.instance.Rasterizer,
                null,
                Main.GameViewMatrix.TransformationMatrix);
        }

        private void DrawPixelRenderTarget(On.Terraria.Main.orig_DrawNPCs orig, Main self, bool behindTiles)
        {
            // On entry SpriteBatch is already open (Main began it).
            if (!behindTiles && AnyPrimsActive())
            {
                Main.spriteBatch.End();
                DrawScaledTargetClosed(pixelRenderTargetBeforeNPCs);
                BeginDefaultSpriteBatch();
            }

            orig(self, behindTiles);

            if (!behindTiles && AnyPrimsActive())
            {
                Main.spriteBatch.End();
                DrawScaledTargetClosed(pixelRenderTarget);
                BeginDefaultSpriteBatch();
            }
        }

        private void DrawToCustomRenderTargets(On.Terraria.Main.orig_CheckMonoliths orig)
        {
            pixelPrimDrawersList.Clear();
            pixelPrimDrawersListBeforeNPCs.Clear();

            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile projectile = Main.projectile[i];
                if (projectile.active && projectile.modProjectile != null &&
                    projectile.modProjectile is IPixelPrimitiveDrawer pixelPrimitiveProjectile)
                {
                    if (pixelPrimitiveProjectile.DrawBeforeNPCs)
                        pixelPrimDrawersListBeforeNPCs.Add(pixelPrimitiveProjectile);
                    else
                        pixelPrimDrawersList.Add(pixelPrimitiveProjectile);
                }
            }

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && npc.modNPC != null &&
                    npc.modNPC is IPixelPrimitiveDrawer pixelPrimitiveNPC)
                {
                    if (pixelPrimitiveNPC.DrawBeforeNPCs)
                        pixelPrimDrawersListBeforeNPCs.Add(pixelPrimitiveNPC);
                    else
                        pixelPrimDrawersList.Add(pixelPrimitiveNPC);
                }
            }

            if (AnyPrimsActive())
            {
                ResizePixelRenderTarget(false);

                DrawPrimsToRenderTarget(pixelRenderTarget, pixelPrimDrawersList);
                DrawPrimsToRenderTarget(pixelRenderTargetBeforeNPCs, pixelPrimDrawersListBeforeNPCs);

                Main.instance.GraphicsDevice.SetRenderTarget(null);
            }

            orig();
        }

        private static void DrawPrimsToRenderTarget(RenderTarget2D renderTarget, List<IPixelPrimitiveDrawer> pixelPrimitives)
        {
            if (renderTarget == null || renderTarget.IsDisposed)
                return;

            try
            {
                renderTarget.SwapToRenderTarget();
            }
            catch
            {
                return;
            }

            if (pixelPrimitives.Count == 0)
                return;

            bool batchOpen = false;
            try
            {
                Main.spriteBatch.Begin(
                    SpriteSortMode.Immediate,
                    BlendState.AlphaBlend,
                    Main.DefaultSamplerState,
                    DepthStencilState.None,
                    Main.instance.Rasterizer,
                    null);
                batchOpen = true;

                foreach (IPixelPrimitiveDrawer drawer in pixelPrimitives)
                {
                    try
                    {
                        drawer.DrawPixelPrimitives(Main.spriteBatch);
                    }
                    catch
                    {
                        // One drawer failed — continue with the rest.
                    }
                }
            }
            catch
            {
                // Begin itself failed (rare).
            }
            finally
            {
                if (batchOpen)
                {
                    try { Main.spriteBatch.End(); } catch { }
                }
            }
        }

        private void ResizePixelRenderTarget(bool load)
        {
            if (Main.dedServ)
                return;

            if (!Main.gameMenu || load)
            {
                Vector2 currentScreenSize = new Vector2(Main.screenWidth, Main.screenHeight);

                bool needsRecreate = currentScreenSize != previousScreenSize
                    || pixelRenderTarget == null || pixelRenderTarget.IsDisposed
                    || pixelRenderTargetBeforeNPCs == null || pixelRenderTargetBeforeNPCs.IsDisposed;

                if (needsRecreate)
                {
                    if (pixelRenderTarget != null && !pixelRenderTarget.IsDisposed)
                        pixelRenderTarget.Dispose();
                    if (pixelRenderTargetBeforeNPCs != null && !pixelRenderTargetBeforeNPCs.IsDisposed)
                        pixelRenderTargetBeforeNPCs.Dispose();

                    int maxDim = 960; // ~ half of 1920
                    if (InfernumConfig.Instance != null)
                    {
                        if (InfernumConfig.Instance.ReducedGraphicsConfig)
                            maxDim = 640;
                    }

                    // Keep aspect ratio when capping so lasers/primitives don't stretch.
                    int halfW = Math.Max(Main.screenWidth / 2, 1);
                    int halfH = Math.Max(Main.screenHeight / 2, 1);
                    int width = halfW;
                    int height = halfH;
                    if (halfW > maxDim || halfH > maxDim)
                    {
                        float scaleDown = Math.Min((float)maxDim / halfW, (float)maxDim / halfH);
                        width = Math.Max((int)(halfW * scaleDown), 1);
                        height = Math.Max((int)(halfH * scaleDown), 1);
                    }

                    pixelRenderTarget = new RenderTarget2D(Main.instance.GraphicsDevice, width, height,
                        false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
                    pixelRenderTargetBeforeNPCs = new RenderTarget2D(Main.instance.GraphicsDevice, width, height,
                        false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
                }

                previousScreenSize = currentScreenSize;
            }
        }
        #endregion
    }
}