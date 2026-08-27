using InfernumMode.Graphics.Interfaces;
using InfernumMode.ILEditingStuff;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace InfernumMode.Graphics
{
    /// <summary>
    /// Draws all IScreenCullDrawer projectiles under one scissor-enabled batch.
    ///
    /// Does NOT hook DrawProjectiles itself — SpecializedDrawRegionManager invokes
    /// DrawCulledProjectiles() so projectile drawing is not run twice and batch
    /// open/close stays consistent for Catalyst / DrawPlayers.
    /// </summary>
    public class ScreenCullDrawerManager
    {
        // Dedicated instance — never mutate Main.Rasterizer / RasterizerState.CullNone.
        private static readonly RasterizerState ScissorEnabled = new RasterizerState
        {
            CullMode = CullMode.None,
            ScissorTestEnable = true
        };

        /// <summary>
        /// Call while SpriteBatch is CLOSED (e.g. right after Main.DrawProjectiles).
        /// Leaves SpriteBatch CLOSED on exit.
        /// </summary>
        public static void DrawCulledProjectiles()
        {
            bool any = false;
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile p = Main.projectile[i];
                if (p.active && p.modProjectile != null && p.modProjectile is IScreenCullDrawer)
                {
                    any = true;
                    break;
                }
            }
            if (!any)
                return;

            GraphicsDevice device = Main.instance.GraphicsDevice;

            // Scissor must stay inside the current viewport or XNA throws.
            Rectangle scissor = device.Viewport.Bounds;
            // Slight padding like 1.4 (-50..+100) but clamped to the viewport.
            int pad = 50;
            scissor.X = System.Math.Max(0, scissor.X - pad);
            scissor.Y = System.Math.Max(0, scissor.Y - pad);
            scissor.Width = System.Math.Min(device.Viewport.Width - scissor.X, scissor.Width + pad * 2);
            scissor.Height = System.Math.Min(device.Viewport.Height - scissor.Y, scissor.Height + pad * 2);

            device.ScissorRectangle = scissor;

            Main.spriteBatch.Begin(
                SpriteSortMode.Deferred,
                BlendState.AlphaBlend,
                Main.DefaultSamplerState,
                DepthStencilState.None,
                ScissorEnabled,
                null,
                Main.GameViewMatrix.TransformationMatrix);

            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile p = Main.projectile[i];
                if (!p.active || p.modProjectile == null)
                    continue;

                IScreenCullDrawer drawer = p.modProjectile as IScreenCullDrawer;
                if (drawer != null)
                    drawer.CullDraw(Main.spriteBatch);
            }

            // Leave CLOSED for subsequent draws / DrawPlayers.
            Main.spriteBatch.End();
        }
    }
}
