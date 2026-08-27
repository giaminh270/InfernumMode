using InfernumMode.Graphics;
using InfernumMode.Graphics.Interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoMod.Cil;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Events;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.ILEditingStuff
{
    /// <summary>
    /// 1.3-compatible backport of Infernum 1.4 ScreenOverlaysSystem.
    /// This is intentionally an IHookEdit because tModLoader 0.11.x does not
    /// provide the 1.4 ModSystem lifecycle/API used by the original class.
    /// </summary>
    public sealed class ScreenOverlaysSystem : IHookEdit
    {
        // 1.4 stores NPC indices here and draws them immediately before the
        // vanilla black background fill.  The old 1.3 implementation had this
        // list typed/used as projectiles, which was incorrect for Ares.
        public static List<int> DrawCacheBeforeBlack = new List<int>(Main.maxNPCs);

        public static List<int> DrawCacheProjsOverSignusBlackening = new List<int>(Main.maxProjectiles);

        // 1.3-specific cache retained from the former DrawBlackEffectHook.
        public static List<int> DrawCacheAdditiveLighting = new List<int>(Main.maxProjectiles);
		
		public static List<DrawData> ThingsToDrawOnTopOfBlur = new List<DrawData>();
		public static List<DrawData> ThingsToDrawOnTopOfBlurAdditive = new List<DrawData>();

        private static void DrawBlackout(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);

            // Ares/background NPCs: draw them after the black-fill call is about
            // to execute, matching the 1.4 ScreenOverlaysSystem strategy.
            if (!cursor.TryGotoNext(MoveType.Before, i => i.MatchCall<Main>("DrawBackgroundBlackFill")))
                return;

            cursor.EmitDelegate<Action>(() =>
            {
                if (Main.gameMenu)
                    return;

                for (int i = 0; i < DrawCacheBeforeBlack.Count; i++)
                {
                    int index = DrawCacheBeforeBlack[i];
                    if (index < 0 || index >= Main.maxNPCs || !Main.npc[index].active)
                        continue;

                    try
                    {
                        Main.instance.DrawNPC(index, false);
                    }
                    catch (Exception e)
                    {
                        TimeLogger.DrawException(e);
                        Main.npc[index].active = false;
                    }
                }
                DrawCacheBeforeBlack.Clear();
            });

            // Everything after MoonlordDeathDrama.DrawWhite is injected into the
            // final entity/overlay region, like the 1.4 implementation.
            if (!cursor.TryGotoNext(MoveType.After, i => i.MatchCall<MoonlordDeathDrama>("DrawWhite")))
                return;

            cursor.EmitDelegate<Action>(() =>
            {
                if (Main.gameMenu)
                    return;

                float fadeToBlack = 0f;
                if (CalamityMod.NPCs.CalamityGlobalNPC.signus != -1)
                {
                    int signus = CalamityMod.NPCs.CalamityGlobalNPC.signus;
                    if (signus >= 0 && signus < Main.maxNPCs && Main.npc[signus].active)
                        fadeToBlack = Main.npc[signus].Infernum().ExtraAI[9];
                }
                if (InfernumMode.BlackFade > 0f)
                    fadeToBlack = InfernumMode.BlackFade;

                if (fadeToBlack > 0f)
                {
                    Color color = Color.Black * fadeToBlack;
                    Main.spriteBatch.Draw(Main.magicPixel, new Rectangle(-2, -2, Main.screenWidth + 4, Main.screenHeight + 4), new Rectangle(0, 0, 1, 1), color);
                }

                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.instance.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

                DrawCachedProjectiles(DrawCacheProjsOverSignusBlackening);
                DrawCacheProjsOverSignusBlackening.Clear();

                // Existing 1.3 Infernum feature: explicit additive-lighting cache.
                DrawCachedProjectiles(DrawCacheAdditiveLighting);
                DrawCacheAdditiveLighting.Clear();

                // These managers were already backported into the 1.3 tree, but
                Main.spriteBatch.End();
                // they must execute here rather than directly after DrawProjectiles
                // to match the 1.4 ScreenOverlays ordering.
                ScreenCullDrawerManager.DrawCulledProjectiles();
                SpecializedDrawRegionManager.DrawSpecializedProjectileGroups();
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.instance.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

                // Preserve the old Infernum madness overlay with 1.3's shader API.
                if (InfernumMode.CanUseCustomAIs)
                {
                    Main.spriteBatch.End();
                    Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearWrap, DepthStencilState.None, Main.instance.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

                    Terraria.Graphics.Effects.Filters.Scene["InfernumMode:Madness"].GetShader().UseSecondaryColor(Color.DarkViolet);
                    Terraria.Graphics.Effects.Filters.Scene["InfernumMode:Madness"].Apply();
                    Main.spriteBatch.Draw(ModContent.GetTexture("Terraria/Misc/noise"), new Rectangle(-2, -2, Main.screenWidth + 4, Main.screenHeight + 4), new Rectangle(0, 0, 1, 1), Color.White);
                    Main.spriteBatch.ExitShaderRegion();
                }
            });
        }

        private static void DrawCachedProjectiles(List<int> cache)
        {
            for (int i = 0; i < cache.Count; i++)
            {
                int index = cache[i];
                if (index < 0 || index >= Main.maxProjectiles || !Main.projectile[index].active)
                    continue;

                try
                {
                    Main.instance.DrawProj(index);
                }
                catch (Exception e)
                {
                    TimeLogger.DrawException(e);
                    Main.projectile[index].active = false;
                }
            }
        }

        public void Load()
        {
            DrawCacheBeforeBlack = new List<int>(Main.maxNPCs);
            DrawCacheProjsOverSignusBlackening = new List<int>(Main.maxProjectiles);
            DrawCacheAdditiveLighting = new List<int>(Main.maxProjectiles);
            IL.Terraria.Main.DoDraw += DrawBlackout;
        }

        public void Unload()
        {
            IL.Terraria.Main.DoDraw -= DrawBlackout;
            DrawCacheBeforeBlack = null;
            DrawCacheProjsOverSignusBlackening = null;
            DrawCacheAdditiveLighting = null;
        }
    }
}
