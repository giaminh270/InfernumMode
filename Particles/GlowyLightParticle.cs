using CalamityMod;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using System;
using static System.Math;
using static Microsoft.Xna.Framework.MathHelper;

namespace InfernumMode.Particles
{
    public class GlowyLightParticle : Particle
    {
        private float Opacity;

        private readonly float OriginalOpacity;

        private readonly bool Gravity;

        public override bool SetLifetime => true;

        public override bool UseCustomDraw => true;

        public override bool UseAdditiveBlend => true;

        public override string Texture => "InfernumMode/Particles/GlowyLightParticle";

        public GlowyLightParticle(Vector2 position, Vector2 velocity, Color color, int lifetime, float scale, float opacity, bool gravity)
        {
            Position = position;
            Velocity = velocity;
            Color = color;
            Scale = scale;
            Lifetime = lifetime;
            OriginalOpacity = Opacity = opacity;
            Gravity = gravity;
        }

        public override void Update()
        {
            if (Time <= 5f)
                Opacity = Lerp(OriginalOpacity, OriginalOpacity * 1.5f, Time / 5f);
            else
				Opacity = Lerp(OriginalOpacity * 1.5f, 0f, -((float)Math.Cos(((Time - 5f) / (Lifetime - 5f)) * Math.PI) - 1) / 2f);
            if (Gravity)
            {
                Velocity.X *= 0.99f;
                Velocity.Y += 0.2f;
            }
        }

        public override void CustomDraw(SpriteBatch spriteBatch)
        {
            Texture2D texture = ModContent.GetTexture(Texture);
            spriteBatch.Draw(texture, Position - Main.screenPosition, null, Color * Opacity, Rotation, texture.Size() * 0.5f, Scale, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, Position - Main.screenPosition, null, Color * Opacity * 0.5f, Rotation, texture.Size() * 0.5f, Scale, SpriteEffects.None, 0f);
        }
    }
}
