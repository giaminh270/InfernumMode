using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using static System.Math;
using static Microsoft.Xna.Framework.MathHelper;

namespace InfernumMode.Particles
{
    public class CartoonAngerParticle : Particle
    {
        public float StartingScale
        {
            get;
            set;
        }

        public Color StartingColor
        {
            get;
            set;
        }

        public Color EndingColor
        {
            get;
            set;
        }

        public override bool SetLifetime => true;

        public override bool UseCustomDraw => true;

        public override bool UseAdditiveBlend => true;

        public override string Texture => "InfernumMode/Particles/CartoonAngerParticle";

        public CartoonAngerParticle(Vector2 relativePosition, Color startingColor, Color endingColor, int lifetime, float rotation, float scale)
        {
            Position = relativePosition;
            Velocity = Vector2.Zero;
            StartingScale = scale;
            StartingColor = startingColor;
            EndingColor = endingColor;
            Scale = 0.01f;
            Rotation = rotation;
            Lifetime = lifetime;
        }

        public override void Update()
        {
            float scaleFactor = Lerp(0.7f, 1.3f, (float)Sin(TwoPi * Time / 27f + ID) * 0.5f + 0.5f);
            Scale = Utilities.Remap(Time, 0f, 30f, 0.01f, StartingScale * scaleFactor);
            Color = Color.Lerp(StartingColor, EndingColor, LifetimeCompletion);
            Color = Color.Lerp(Color, Color.Transparent, (float)Pow(LifetimeCompletion, 3.5f));
        }

        public override void CustomDraw(SpriteBatch spriteBatch)
        {
            Texture2D texture = ModContent.GetTexture(Texture);
            spriteBatch.Draw(texture, Position - Main.screenPosition, null, Color, Rotation, texture.Size() * 0.5f, Scale, 0, 0f);
        }
    }
}
