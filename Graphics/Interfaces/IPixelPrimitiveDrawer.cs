using Microsoft.Xna.Framework.Graphics;

namespace InfernumMode.Graphics.Interfaces
{
    /// <summary>
    /// Interface for projectiles / NPCs that want to draw primitives into a half-resolution
    /// render target (for a clean pixelated look when upscaled).
    /// Backported from InfernumMode 1.4.3 to tModLoader 0.11 / Terraria 1.3.5.3.
    /// </summary>
    public interface IPixelPrimitiveDrawer
    {
        /// <summary>
        /// If true, the primitives are drawn before over-tiles NPCs.
        /// Default implementation in 1.4 was => false; implementers should provide the property.
        /// </summary>
        bool DrawBeforeNPCs { get; }

        /// <summary>
        /// Called while the pixelation render target is active.
        /// Draw your trails / strips / primitives here.
        /// </summary>
        void DrawPixelPrimitives(SpriteBatch spriteBatch);
    }
}
