using Microsoft.Xna.Framework.Graphics;

namespace InfernumMode.Graphics.Interfaces
{

    public interface ISpecializedDrawRegion
    {
        void PrepareSpriteBatch(SpriteBatch spriteBatch);

        void SpecialDraw(SpriteBatch spriteBatch);
    }
}
