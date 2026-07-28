using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ModLoader;

namespace InfernumMode.Particles
{
    public class CloudParticle : Particle
    {
        public bool IsImportant
        {
            get;
            set;
        }

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

        public override bool Important => IsImportant;

        public override string Texture => "InfernumMode/Particles/CloudParticle";

        public CloudParticle(Vector2 relativePosition, Vector2 velocity, Color startingColor, Color endingColor, int lifetime, float scale, bool isImportant = false)
        {
            Position = relativePosition;
            Velocity = velocity;
            StartingScale = scale;
            StartingColor = startingColor;
            EndingColor = endingColor;
            Scale = 0.01f;
            Lifetime = lifetime;
            IsImportant = isImportant;
        }

        public override void Update()
        {
            Velocity *= 0.987f;
            Scale = MathHelper.Lerp(Scale, StartingScale, 0.03f);
            Color = Color.Lerp(StartingColor, EndingColor, LifetimeCompletion);
            Color = Color.Lerp(Color, Color.Transparent, (float)Math.Pow(LifetimeCompletion, 3f));
            Rotation += Velocity.X * 0.008f;
        }

		public override void CustomDraw(SpriteBatch spriteBatch)
		{
			Vector2 drawPosition = Position - Main.screenPosition;


			if (drawPosition.X < -300f || drawPosition.Y < -300f ||
				drawPosition.X > Main.screenWidth + 300f || drawPosition.Y > Main.screenHeight + 300f)
				return;

			int tileX = (int)(Position.X / 16f);
			int tileY = (int)(Position.Y / 16f);

			if (tileX < 0 || tileX >= Main.maxTilesX || tileY < 0 || tileY >= Main.maxTilesY)
				return;

			float brightness = 0.9f;

			try
			{
				brightness = (float)Math.Pow(Lighting.Brightness(tileX, tileY), 0.15f) * 0.9f;
			}
			catch (IndexOutOfRangeException)
			{
				brightness = 0.45f;
			}

			Texture2D texture = ModContent.GetTexture(Texture);
			spriteBatch.Draw(
				texture,
				drawPosition,
				null,
				Color * brightness,
				Rotation,
				texture.Size() * 0.5f,
				Scale,
				SpriteEffects.None,
				0f
			);
		}
    }
}
