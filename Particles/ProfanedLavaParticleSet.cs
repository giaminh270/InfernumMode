using CalamityMod.NPCs;
using InfernumMode.Particles;
using InfernumMode.BehaviorOverrides.BossAIs.Providence;
using InfernumMode.Projectiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;
using InfernumMode.Effects;
using InfernumMode.ExtraTextures;

namespace InfernumMode.Particles
{
    public class ProfanedLavaParticleSet : InfernumBaseFusableParticleSet
    {
        public override Color BorderColor => ProvidenceBehaviorOverride.IsEnraged ? Color.DeepSkyBlue : Color.Lerp(WayfinderSymbol.Colors[0], WayfinderSymbol.Colors[2], 0f);

        public override bool BorderShouldBeSolid => true;

        public override float BorderSize => 2f;

        // These are the shaders used for the background of the particle effect.
        public override List<Effect> BackgroundShaders => new List<Effect>()
        {
            InfernumEffectsRegistry.BaseFusableParticleEdge.Shader,
            InfernumEffectsRegistry.BaseFusableParticleEdge.Shader,
        };

        public override List<Texture2D> BackgroundTextures => new List<Texture2D>()
        {
            Main.gameMenu ? Main.magicPixel : PreferredBackground,
            Main.gameMenu ? Main.magicPixel : PreferredBackground,
        };

        public static Texture2D PreferredBackground => ProvidenceBehaviorOverride.IsEnraged && CalamityGlobalNPC.holyBoss != -1 ?
            InfernumTextureRegistry.HolyFirePixelLayerNight : InfernumTextureRegistry.HolyFirePixelLayer;

        // This property is used to determine the layer in which the particle effect is rendered.
        public override InfernumFusableParticleRenderLayer RenderLayer => InfernumFusableParticleRenderLayer.Default;
        public override void DrawParticles()
        {
            Texture2D fusableParticleBase = ModContent.GetTexture("InfernumMode/Particles/FusableParticleBase");
            foreach (FusableParticle particle in Particles)
            {
                Vector2 drawPosition = particle.Center - Main.screenPosition;
                Color drawColor = Color.Lerp(BorderColor, WayfinderSymbol.Colors[1], 0.5f);
                Vector2 origin = fusableParticleBase.Size() * 0.5f;
                Vector2 scale = Vector2.One * particle.Size / fusableParticleBase.Size();
                Main.spriteBatch.Draw(fusableParticleBase, drawPosition, null, drawColor, 0f, origin, scale, SpriteEffects.None, 0f);
            }
        }

        public override FusableParticle SpawnParticle(Vector2 center, float sizeStrength)
        {
            // Memory safety: hard cap particle count on tML 1.3 to avoid OOM during long fights.
            const int MaxParticles = 250;
            try
            {
                if (InfernumConfig.Instance != null && InfernumConfig.Instance.ReducedGraphicsConfig)
                {
                    if (Particles.Count >= 120)
                        Particles.RemoveRange(0, Particles.Count - 100);
                }
                else if (Particles.Count >= MaxParticles)
                    Particles.RemoveRange(0, Particles.Count - (MaxParticles - 20));
            }
            catch { }

            Particles.Add(new FusableParticle(center, sizeStrength));
            return Particles.Last();
        }

        public override void UpdateBehavior(FusableParticle particle)
        {
            particle.Size = MathHelper.Clamp(particle.Size - 0.5f, 0f, 400f) * 0.978f;
        }

        public override void PrepareOptionalShaderData(Effect effect, int index)
        {
            effect.Parameters["upscaleFactor"].SetValue(Vector2.One * 0.2f);
            switch (index)
            {
                // Background 1.
                case 0:
                    Vector2 offset = Vector2.UnitX * Main.GlobalTime * 0.12f;
                    effect.Parameters["generalBackgroundOffset"].SetValue(offset);
                    break;

                // Background 2.
                case 1:
                    offset = -Vector2.UnitY * Main.GlobalTime * 0.13f;
                    effect.Parameters["generalBackgroundOffset"].SetValue(offset);
                    break;
            }
        }
    }
}
