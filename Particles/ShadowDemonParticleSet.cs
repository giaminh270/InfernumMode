﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;

namespace InfernumMode.Particles
{
    public class ShadowDemonParticleSet : InfernumBaseFusableParticleSet
    {
        public override float BorderSize => 3f;
        public override bool BorderShouldBeSolid => false;
        public override Color BorderColor => Color.Lerp(Color.Fuchsia, Color.Black, 0.7f) * 0.85f;
        public override InfernumFusableParticleRenderLayer RenderLayer => InfernumFusableParticleRenderLayer.OverNPCsBeforeProjectiles;

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

        // Giới hạn số hạt tối đa
        private const int MaxParticles = 100;
        
        // Cache texture để tránh load lại nhiều lần
        private static Texture2D _fusableParticleBase;
        private Texture2D FusableParticleBase => _fusableParticleBase = ModContent.GetTexture("CalamityMod/ExtraTextures/FusableParticleBase");

        public override FusableParticle SpawnParticle(Vector2 center, float sizeStrength)
        {
            // Giới hạn số hạt tối đa
            if (Particles.Count >= MaxParticles)
            {
                // Tìm và xoá hạt nhỏ nhất thay vì hạt cũ nhất
                var smallestParticle = Particles.OrderBy(p => p.Size).First();
                Particles.Remove(smallestParticle);
            }

            var particle = new FusableParticle(center, sizeStrength);
            Particles.Add(particle);
            AddParticleToGrid(particle);
            return particle;
        }

        public override void UpdateBehavior(FusableParticle particle)
        {
            // Giảm kích thước với tốc độ phụ thuộc vào kích thước hiện tại
            float reduction = particle.Size > 50f ? 2.5f : 1.2f;
            particle.Size = MathHelper.Clamp(particle.Size - reduction, 0f, 400f) * 0.97f;
            
            // Cập nhật spatial grid
            UpdateParticleInGrid(particle);
        }

        public override void PrepareOptionalShaderData(Effect effect, int index)
        {
            switch (index)
            {
                case 0: // Background 1
                    Vector2 offset = Vector2.UnitX * Main.GlobalTime * 0.03f;
                    effect.Parameters["generalBackgroundOffset"].SetValue(offset);
                    break;
                case 1: // Background 2
                    offset = -Vector2.UnitY * Main.GlobalTime * 0.027f;
                    effect.Parameters["generalBackgroundOffset"].SetValue(offset);
                    break;
            }
        }

        public override void DrawParticles()
        {
            var visibleParticles = GetVisibleParticles();
            
            // Batch draw tất cả các hạt cùng lúc
            foreach (var particle in visibleParticles)
            {
                Vector2 drawPosition = particle.Center - Main.screenPosition;
                Vector2 origin = FusableParticleBase.Size() * 0.5f;
                Vector2 scale = Vector2.One * particle.Size / FusableParticleBase.Size();
                
                // LOD: làm mờ các hạt nhỏ
                Color drawColor = Color.White;
                if (particle.Size < 8f)
                    drawColor *= particle.Size / 8f;
                    
                Main.spriteBatch.Draw(FusableParticleBase, drawPosition, null, drawColor, 0f, origin, scale, SpriteEffects.None, 0f);
            }
        }

        private List<FusableParticle> GetVisibleParticles()
        {
            var screenRect = new Rectangle(
                (int)Main.screenPosition.X - 200, // Thêm padding
                (int)Main.screenPosition.Y - 200,
                Main.screenWidth + 400,
                Main.screenHeight + 400
            );
            
            var particlesInView = GetParticlesInArea(screenRect);
            return particlesInView.Where(p => ShouldDrawParticle(p)).ToList();
        }
    }
}