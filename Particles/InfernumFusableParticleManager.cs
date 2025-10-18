using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static InfernumMode.Particles.InfernumBaseFusableParticleSet;

namespace InfernumMode.Particles
{
    public static class InfernumFusableParticleManager
    {
        internal static List<FusableParticleRenderCollection> ParticleSets = new List<FusableParticleRenderCollection>();
        internal static List<Mod> ExtraModsToLoadSetsFrom = new List<Mod>();
        internal static List<Type> ParticleSetTypes = new List<Type>();
        internal static bool HasBeenFormallyDefined = false;
        
        // Optimization: Cache screen size để tránh tạo lại render target liên tục
        private static int lastScreenWidth, lastScreenHeight;
        private static int frameCounter;

        /// <summary>
        /// Loads all render sets với tối ưu hoá.
        /// </summary>
        internal static void LoadParticleRenderSets(bool reload = false, int width = -1, int height = -1)
        {
            if (Main.netMode == NetmodeID.Server)
                return;

            // Sử dụng kích thước tối ưu, không vượt quá 1080p
            width = width == -1 ? Math.Min(1920, Main.screenWidth) : width;
            height = height == -1 ? Math.Min(1080, Main.screenHeight) : height;
            
            // Tránh tạo lại render target liên tục khi kích thước thay đổi nhỏ
            if (reload && Math.Abs(width - lastScreenWidth) < 50 && Math.Abs(height - lastScreenHeight) < 50)
                return;
                
            lastScreenWidth = width;
            lastScreenHeight = height;

            if (!reload)
            {
                ParticleSets = new List<FusableParticleRenderCollection>();
                ParticleSetTypes = new List<Type>();
                ExtraModsToLoadSetsFrom = new List<Mod>();
                HasBeenFormallyDefined = true;

                FindParticleSetTypesInMod(InfernumMode.Instance, width, height);
                foreach (Mod m in ExtraModsToLoadSetsFrom)
                    FindParticleSetTypesInMod(m, width, height);
            }
            else
            {
                DisposeOfOldRenderTargets();
                foreach (Type t in ParticleSetTypes)
                    CreateParticleSetOfType(t, width, height);
            }
        }

        internal static void FindParticleSetTypesInMod(Mod mod, int width, int height)
        {
            foreach (Type type in mod.Code.GetTypes())
            {
                if (type.IsAbstract)
                    continue;

                if (type.IsSubclassOf(typeof(InfernumBaseFusableParticleSet)))
                {
                    ParticleSetTypes.Add(type);
                    CreateParticleSetOfType(type, width, height);
                }
            }
        }

        internal static void CreateParticleSetOfType(Type t, int width, int height)
        {
            InfernumBaseFusableParticleSet instance = Activator.CreateInstance(t) as InfernumBaseFusableParticleSet;
            List<RenderTarget2D> backgroundTargets = new List<RenderTarget2D>();

            if (Main.netMode != NetmodeID.Server)
            {
                // Sử dụng RenderTargetUsage.DiscardContents để tối ưu performance
                for (int i = 0; i < instance.LayerCount; i++)
                {
                    backgroundTargets.Add(new RenderTarget2D(
                        Main.instance.GraphicsDevice, 
                        width, height, 
                        false, 
                        SurfaceFormat.Color, 
                        DepthFormat.None, 
                        0, 
                        RenderTargetUsage.DiscardContents
                    ));
                }
            }

            FusableParticleRenderCollection particleRenderCollection = new FusableParticleRenderCollection(instance, backgroundTargets);
            ParticleSets.Add(particleRenderCollection);
        }

        internal static void CreateParticleSetOfType<T>(int width, int height) where T : InfernumBaseFusableParticleSet =>
            CreateParticleSetOfType(typeof(T), width, height);

        internal static void DisposeOfOldRenderTargets()
        {
            foreach (FusableParticleRenderCollection particleRenderSet in ParticleSets)
            {
                foreach (RenderTarget2D target in particleRenderSet.BackgroundTargets)
                    target?.Dispose();
            }
            ParticleSets.Clear();
        }

        internal static void UnloadParticleRenderSets()
        {
            DisposeOfOldRenderTargets();
            
            foreach (FusableParticleRenderCollection particleRenderSet in ParticleSets)
                particleRenderSet.ParticleSet = null;

            HasBeenFormallyDefined = false;
            ParticleSets = null;
            ExtraModsToLoadSetsFrom = null;
            ParticleSetTypes = null;
        }

        internal static void PrepareFusableParticleTargets()
        {
            if (Main.netMode == NetmodeID.Server || ParticleSets is null || Main.gameMenu)
                return;

            // Kiểm tra xem có cần resize render targets không
            if (Main.screenWidth != lastScreenWidth || Main.screenHeight != lastScreenHeight)
            {
                LoadParticleRenderSets(true, Main.screenWidth, Main.screenHeight);
            }

            // Chỉ update particles mỗi 2 frame để giảm CPU usage
            frameCounter++;
            bool shouldUpdateParticles = frameCounter % 2 == 0;

            for (int i = 0; i < ParticleSets.Count; i++)
            {
                if (shouldUpdateParticles)
                    ParticleSets[i].ParticleSet.UpdateAllParticles();
                    
                ParticleSets[i].ParticleSet.PrepareRenderTargetForDrawing();
            }
        }

        internal static void RenderAllFusableParticles()
        {
            if (Main.netMode == NetmodeID.Server || Main.gameMenu)
                return;

            for (int i = 0; i < ParticleSets.Count; i++)
            {
                InfernumBaseFusableParticleSet particleSet = ParticleSets[i].ParticleSet;

                // Optimization: Skip empty particle sets
                if (particleSet.Particles.Count <= 0)
                    continue;

                List<RenderTarget2D> backgroundTargets = particleSet.GetBackgroundTargets;

                Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, 
                                     Main.DefaultSamplerState, DepthStencilState.None, 
                                     Main.instance.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

                for (int j = 0; j < particleSet.LayerCount; j++)
                {
                    Effect shader = particleSet.BackgroundShaders[j];
                    Texture2D backgroundTexture = particleSet.BackgroundTextures[j];

                    // Set shader parameters
                    shader.Parameters["edgeBorderSize"].SetValue(particleSet.BorderSize);
                    shader.Parameters["borderShouldBeSolid"].SetValue(particleSet.BorderShouldBeSolid);
                    shader.Parameters["edgeBorderColor"].SetValue(particleSet.BorderColor.ToVector3());
                    shader.Parameters["screenArea"].SetValue(new Vector2(Main.screenWidth, Main.screenHeight) / Main.GameViewMatrix.Zoom);
                    shader.Parameters["screenMoveOffset"].SetValue(Main.screenPosition - Main.screenLastPosition);
                    shader.Parameters["uWorldPosition"].SetValue(Main.screenPosition);
                    shader.Parameters["renderTargetArea"].SetValue(new Vector2(backgroundTargets[j].Width, backgroundTargets[j].Height));
                    shader.Parameters["invertedScreen"].SetValue(Main.LocalPlayer.gravDir == -1f);
                    shader.Parameters["upscaleFactor"].SetValue(Vector2.One);
                    shader.Parameters["generalBackgroundOffset"].SetValue(Vector2.Zero);

                    Main.graphics.GraphicsDevice.Textures[1] = backgroundTexture;
                    shader.Parameters["uImageSize1"].SetValue(backgroundTexture.Size());

                    particleSet.PrepareOptionalShaderData(shader, j);
                    shader.CurrentTechnique.Passes[0].Apply();

                    Main.spriteBatch.Draw(backgroundTargets[j], Vector2.Zero, Color.White);
                }

                Main.spriteBatch.End();
            }
        }

        internal static FusableParticleRenderCollection GetParticleRenderCollectionByType(Type type)
        {
            if (Main.netMode == NetmodeID.Server)
                return null;

            return ParticleSets.FirstOrDefault(s => s.ParticleSet.GetType() == type);
        }

        public static T GetParticleSetByType<T>() where T : InfernumBaseFusableParticleSet
        {
            if (Main.netMode == NetmodeID.Server)
                return null;

            if (ParticleSets.Count <= 0)
                LoadParticleRenderSets(true);

            return ParticleSets.FirstOrDefault(s => s.ParticleSet.GetType() == typeof(T))?.ParticleSet as T;
        }
    }
}