﻿using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;

namespace InfernumMode.Particles
{
    public class ShadowDemonParticleSet : BaseFusableParticleSet
    {
		
        public override float BorderSize => 3f;
        public override bool BorderShouldBeSolid => false;
        public override Color BorderColor => Color.Lerp(Color.Fuchsia, Color.Black, 0.7f) * 0.85f;
		public override FusableParticleRenderLayer RenderLayer => FusableParticleRenderLayer.OverPlayers;


        public override List<Effect> BackgroundShaders => new List<Effect>()
        {
            GameShaders.Misc["CalamityMod:BaseFusableParticleEdge"].Shader,
            GameShaders.Misc["CalamityMod:BaseFusableParticleEdge"].Shader,
        };
        public override List<Texture2D> BackgroundTextures => new List<Texture2D>()
        {
            ModContent.GetTexture("InfernumMode/ExtraTextures/Shadow1"),
            ModContent.GetTexture("InfernumMode/ExtraTextures/Shadow2"),
        };
        public override FusableParticle SpawnParticle(Vector2 center, float sizeStrength)
        {
            Particles.Add(new FusableParticle(center, sizeStrength));
            return Particles.Last();
        }

        public override void UpdateBehavior(FusableParticle particle)
        {
            particle.Size = MathHelper.Clamp(particle.Size - 2.5f, 0f, 400f) * 0.97f;
        }

        public override void PrepareOptionalShaderData(Effect effect, int index)
        {
            switch (index)
            {
                // Background 1.
                case 0:
                    Vector2 offset = Vector2.UnitX * Main.GlobalTime * 0.03f;
                    effect.Parameters["generalBackgroundOffset"].SetValue(offset);
                    break;

                // Background 2.
                case 1:
                    offset = -Vector2.UnitY * Main.GlobalTime * 0.027f;
                    effect.Parameters["generalBackgroundOffset"].SetValue(offset);
                    break;
            }
        }

        public override void DrawParticles()
        {
            Texture2D fusableParticleBase = ModContent.GetTexture("CalamityMod/ExtraTextures/FusableParticleBase");
            foreach (FusableParticle particle in Particles)
            {
                Vector2 drawPosition = particle.Center - Main.screenPosition;
                Vector2 origin = fusableParticleBase.Size() * 0.5f;
                Vector2 scale = Vector2.One * particle.Size / fusableParticleBase.Size();
                Main.spriteBatch.Draw(fusableParticleBase, drawPosition, null, Color.White, 0f, origin, scale, SpriteEffects.None, 0f);
            }
        }
    }
}
