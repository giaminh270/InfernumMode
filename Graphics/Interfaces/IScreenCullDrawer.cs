using Microsoft.Xna.Framework.Graphics;

namespace InfernumMode.Graphics.Interfaces
{
    /// <summary>
    /// Defers projectile drawing into one shared SpriteBatch with a common
    /// rasterizer scissor state, instead of each PreDraw restarting the batch.
    /// Backported from InfernumMode 1.4.
    ///
    /// Do not also draw in PreDraw — return false from PreDraw and implement CullDraw.
    /// </summary>
    public interface IScreenCullDrawer
    {
        void CullDraw(SpriteBatch spriteBatch);
    }
}
