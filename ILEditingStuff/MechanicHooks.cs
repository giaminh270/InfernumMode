using CalamityMod;
using CalamityMod.CalPlayer;
using CalamityMod.NPCs;
using CalamityMod.NPCs.ExoMechs;
using CalamityMod.UI;
using CalamityMod.World;
using InfernumMode.Balancing;
using InfernumMode.BehaviorOverrides.BossAIs.Draedon;
using InfernumMode.BehaviorOverrides.BossAIs.Golem;
using InfernumMode.BehaviorOverrides.BossAIs.Providence;
using InfernumMode.DataStructures;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using CalamityMod.Items.Dyes;
using CalamityMod.Particles;
using InfernumMode.Particles;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.Events;
using Terraria.Graphics.Shaders;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI.Chat;
using static InfernumMode.ILEditingStuff.HookManager;

namespace InfernumMode.ILEditingStuff
{
    public class DrawDraedonSelectionUIWithAthena : IHookEdit
    {
        public static ExoMech? PrimaryMechToSummon
        {
            get;
            set;
        } = null;

        public static ExoMech? DestroyerTypeToSummon
        {
            get;
            set;
        } = null;

        internal static void DrawSelectionUI(ILContext context)
        {
            ILCursor cursor = new ILCursor(context);
            cursor.EmitDelegate<Action>(DrawWrapper);
            cursor.Emit(OpCodes.Ret);
        }

        public static void DrawWrapper()
        {
            Vector2 drawAreaVerticalOffset = Vector2.UnitY * 105f;
            Vector2 baseDrawPosition = Main.LocalPlayer.Top + drawAreaVerticalOffset - Main.screenPosition;
            Vector2 destroyerIconDrawOffset = new Vector2(-78f, -124f);
            Vector2 primeIconDrawOffset = new Vector2(0f, -140f);
            Vector2 twinsIconDrawOffset = new Vector2(78f, -124f);

            if (InfernumMode.CanUseCustomAIs)
            {
                bool hoveringOverAnyIcon = false;
                for (int i = 0; i < 3; i++)
                {
                    Vector2 iconDrawOffset = new Vector2(MathHelper.Lerp(-92f, 92f, i / 2f), -145f);
                    hoveringOverAnyIcon |= HandleInteractionWithButton(baseDrawPosition + iconDrawOffset, i + 1, PrimaryMechToSummon == null);
                }

                // Reset the selections if the player clicks on something other than the icons.
                if (!hoveringOverAnyIcon && Main.mouseLeft && Main.mouseLeftRelease)
                    PrimaryMechToSummon = DestroyerTypeToSummon = null;

                var font = Main.fontMouseText;
                string pickTwoText = "Pick two. The first mech will be fought alone. Once sufficiently damaged, the second mech will be summoned and the two will fight together.";
                Vector2 pickToDrawPosition = baseDrawPosition - Vector2.UnitY * 250f;
                foreach (string line in Utils.WordwrapString(pickTwoText, font, 600, 10, out _))
                {
                    if (string.IsNullOrEmpty(line))
                        continue;

                    Vector2 textArea = font.MeasureString(line);
                    ChatManager.DrawColorCodedStringWithShadow(Main.spriteBatch, font, line, pickToDrawPosition - Vector2.UnitX * textArea * 0.5f, Draedon.TextColorEdgy, 0f, textArea * new Vector2(0f, 0.5f), Vector2.One);
                    pickToDrawPosition.Y += 50f;
                }
                return;
            }

            ExoMechSelectionUI.HandleInteractionWithButton(baseDrawPosition + destroyerIconDrawOffset, ExoMech.Destroyer);
            ExoMechSelectionUI.HandleInteractionWithButton(baseDrawPosition + primeIconDrawOffset, ExoMech.Prime);
            ExoMechSelectionUI.HandleInteractionWithButton(baseDrawPosition + twinsIconDrawOffset, ExoMech.Twins);
        }

        public static bool HandleInteractionWithButton(Vector2 drawPosition, int exoMech, bool selectingPrimaryMech)
        {
            float iconScale;
            string description;
            Texture2D iconMechTexture;

            switch (exoMech)
            {
                case 1:
                    iconScale = ExoMechSelectionUI.DestroyerIconScale;
                    iconMechTexture = ModContent.GetTexture("CalamityMod/ExtraTextures/UI/HeadIcon_THanos");
                    description = "Thanatos, a serpentine terror with impervious armor and innumerable laser turrets.";
                    break;
                case 2:
                    iconScale = ExoMechSelectionUI.PrimeIconScale;
                    iconMechTexture = ModContent.GetTexture("CalamityMod/ExtraTextures/UI/HeadIcon_Ares");
                    description = "Ares, a heavyweight, diabolical monstrosity with four Exo superweapons.";
                    break;
                case 3:
                default:
                    iconScale = ExoMechSelectionUI.TwinsIconScale;
                    iconMechTexture = ModContent.GetTexture("CalamityMod/ExtraTextures/UI/HeadIcon_ArtemisApollo");
                    description = "Artemis and Apollo, a pair of extremely agile destroyers with pulse cannons.";
                    break;
            }

            // Check for mouse collision/clicks.
            Rectangle clickArea = Utils.CenteredRectangle(drawPosition, iconMechTexture.Size() * iconScale * 0.9f);

            // Check if the mouse is hovering over the contact button area.
            bool alreadySelected = (int)(PrimaryMechToSummon ?? (ExoMech)999) == exoMech || (int)(DestroyerTypeToSummon ?? (ExoMech)999) == exoMech;
            bool hoveringOverIcon = ExoMechSelectionUI.MouseScreenArea.Intersects(clickArea);
            if (hoveringOverIcon)
            {
                // If so, cause the button to inflate a little bit.
                iconScale = MathHelper.Clamp(iconScale + 0.0375f, 1f, 1.35f);

                // Make the selection known if a click is done and the icon isn't already in use.
                if (Main.mouseLeft && Main.mouseLeftRelease)
                {
                    if (selectingPrimaryMech)
                        PrimaryMechToSummon = alreadySelected ? (ExoMech?)null : (ExoMech)exoMech;
                    else
                        DestroyerTypeToSummon = alreadySelected ? (ExoMech?)null : (ExoMech)exoMech;

                    int draedon = NPC.FindFirstNPC(ModContent.NPCType<Draedon>());
                    if (draedon != -1 && PrimaryMechToSummon.HasValue && DestroyerTypeToSummon.HasValue)
                    {
                        Main.npc[draedon].ai[0] = Draedon.ExoMechChooseDelay + 8f;
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            DraedonBehaviorOverride.SummonExoMech(Main.player[Main.npc[draedon].target]);
                            PrimaryMechToSummon = DestroyerTypeToSummon = null;
                        }
                    }
                }
                Main.blockMouse = Main.LocalPlayer.mouseInterface = true;
            }

            // Otherwise, if not hovering and not selected, cause the button to deflate back to its normal size.
            else if (!alreadySelected)
                iconScale = MathHelper.Clamp(iconScale - 0.05f, 1f, 1.2f);

            // Draw the icon with the new scale.
            Color iconColor = alreadySelected ? Color.Black * 0.8f : Color.White;
            if (alreadySelected)
            {
                for (int i = 0; i < 4; i++)
                {
                    Vector2 drawOffset = -Vector2.UnitY.RotatedBy(MathHelper.TwoPi * i / 4f) * iconScale * 2f;
                    Main.spriteBatch.Draw(iconMechTexture, drawPosition + drawOffset, null, Color.Red, 0f, iconMechTexture.Size() * 0.5f, iconScale, SpriteEffects.None, 0f);
                }
            }

            Main.spriteBatch.Draw(iconMechTexture, drawPosition, null, iconColor, 0f, iconMechTexture.Size() * 0.5f, iconScale, SpriteEffects.None, 0f);

            // Draw the descrption if hovering over the icon.
            if (hoveringOverIcon)
            {
                drawPosition.X -= Main.fontMouseText.MeasureString(description).X * 0.5f;
                drawPosition.Y += 36f;
                Utils.DrawBorderStringFourWay(Main.spriteBatch, Main.fontMouseText, description, drawPosition.X, drawPosition.Y, ExoMechSelectionUI.HoverTextColor, Color.Black, Vector2.Zero, 1f);
            }

            // And update to reflect the new scale.
            switch (exoMech)
            {
                case 1:
                    ExoMechSelectionUI.DestroyerIconScale = iconScale;
                    break;
                case 2:
                    ExoMechSelectionUI.PrimeIconScale = iconScale;
                    break;
                case 3:
                    ExoMechSelectionUI.TwinsIconScale = iconScale;
                    break;
            }
            return hoveringOverIcon;
        }

        public void Load() => ExoMechSelectionUIDraw += DrawSelectionUI;

        public void Unload() => ExoMechSelectionUIDraw -= DrawSelectionUI;
    }

    public class DrawBlackEffectHook : IHookEdit
    {
        public static List<int> DrawCacheBeforeBlack = new List<int>(Main.maxProjectiles);
        public static List<int> DrawCacheProjsOverSignusBlackening = new List<int>(Main.maxProjectiles);
        public static List<int> DrawCacheAdditiveLighting = new List<int>(Main.maxProjectiles);
        internal static void DrawBlackout(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);

            if (!cursor.TryGotoNext(MoveType.Before, i => i.MatchCall<Main>("DrawBackgroundBlackFill")))
                return;

            cursor.EmitDelegate<Action>(() =>
            {
                for (int i = 0; i < DrawCacheBeforeBlack.Count; i++)
                {
                    try
                    {
                        Main.instance.DrawProj(DrawCacheBeforeBlack[i]);
                    }
                    catch (Exception e)
                    {
                        TimeLogger.DrawException(e);
                        Main.projectile[DrawCacheBeforeBlack[i]].active = false;
                    }
                }
                DrawCacheBeforeBlack.Clear();
            });

            if (!cursor.TryGotoNext(MoveType.After, i => i.MatchCall<MoonlordDeathDrama>("DrawWhite")))
                return;

            cursor.EmitDelegate<Action>(() =>
            {
                float fadeToBlack = 0f;
                if (CalamityGlobalNPC.signus != -1)
                    fadeToBlack = Main.npc[CalamityGlobalNPC.signus].Infernum().ExtraAI[9];
                if (InfernumMode.BlackFade > 0f)
                    fadeToBlack = InfernumMode.BlackFade;

                if (fadeToBlack > 0f)
                {
                    Color color = Color.Black * fadeToBlack;
                    Main.spriteBatch.Draw(Main.magicPixel, new Rectangle(-2, -2, Main.screenWidth + 4, Main.screenHeight + 4), new Rectangle(0, 0, 1, 1), color);
                }

                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.instance.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
                for (int i = 0; i < DrawCacheProjsOverSignusBlackening.Count; i++)
                {
                    try
                    {
                        Main.instance.DrawProj(DrawCacheProjsOverSignusBlackening[i]);
                    }
                    catch (Exception e)
                    {
                        TimeLogger.DrawException(e);
                        Main.projectile[DrawCacheProjsOverSignusBlackening[i]].active = false;
                    }
                }
                DrawCacheProjsOverSignusBlackening.Clear();

                Main.spriteBatch.SetBlendState(BlendState.Additive);
                for (int i = 0; i < DrawCacheAdditiveLighting.Count; i++)
                {
                    try
                    {
                        Main.instance.DrawProj(DrawCacheAdditiveLighting[i]);
                    }
                    catch (Exception e)
                    {
                        TimeLogger.DrawException(e);
                        Main.projectile[DrawCacheAdditiveLighting[i]].active = false;
                    }
                }
                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.instance.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
                DrawCacheAdditiveLighting.Clear();

                // Draw the madness effect.
                if (InfernumMode.CanUseCustomAIs)
                {
                    Main.spriteBatch.End();
                    Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearWrap, DepthStencilState.None, Main.instance.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

                    Filters.Scene["InfernumMode:Madness"].GetShader().UseSecondaryColor(Color.DarkViolet);
                    Filters.Scene["InfernumMode:Madness"].Apply();
                    Main.spriteBatch.Draw(ModContent.GetTexture("Terraria/Misc/noise"), new Rectangle(-2, -2, Main.screenWidth + 4, Main.screenHeight + 4), new Rectangle(0, 0, 1, 1), Color.White);
                    Main.spriteBatch.ExitShaderRegion();
                }
            });
        }

        public void Load()
        {
            DrawCacheProjsOverSignusBlackening = new List<int>();
            DrawCacheAdditiveLighting = new List<int>();
            IL.Terraria.Main.DoDraw += DrawBlackout;
        }

        public void Unload()
        {
            DrawCacheProjsOverSignusBlackening = DrawCacheAdditiveLighting = null;
            IL.Terraria.Main.DoDraw -= DrawBlackout;
        }
    }

    public class DisableMoonLordBuildingHook : IHookEdit
    {
        internal static void DisableMoonLordBuilding(ILContext instructionContext)
        {
            var c = new ILCursor(instructionContext);

            if (!c.TryGotoNext(MoveType.After, i => i.MatchLdcI4(ItemID.SuperAbsorbantSponge)))
                return;

            c.EmitDelegate<Action>(() =>
            {
                if (NPC.AnyNPCs(NPCID.MoonLordCore) && PoDWorld.InfernumMode)
                    Main.LocalPlayer.noBuilding = true;
            });
        }

        public void Load() => IL.Terraria.Player.ItemCheck += DisableMoonLordBuilding;

        public void Unload() => IL.Terraria.Player.ItemCheck -= DisableMoonLordBuilding;
    }

    public class ChangeHowMinibossesSpawnInDD2EventHook : IHookEdit
    {
        internal static int GiveDD2MinibossesPointPriority(On.Terraria.GameContent.Events.DD2Event.orig_GetMonsterPointsWorth orig, int slainMonsterID)
        {
            if (OldOnesArmyMinibossChanges.GetMinibossToSummon(out int minibossID) && minibossID != NPCID.DD2Betsy && PoDWorld.InfernumMode)
                return slainMonsterID == minibossID ? 99999 : 0;

            return orig(slainMonsterID);
        }

        public void Load() => On.Terraria.GameContent.Events.DD2Event.GetMonsterPointsWorth += GiveDD2MinibossesPointPriority;

        public void Unload() => On.Terraria.GameContent.Events.DD2Event.GetMonsterPointsWorth -= GiveDD2MinibossesPointPriority;
    }

    public class DrawVoidBackgroundDuringMLFightHook : IHookEdit
    {
        public static void PrepareShaderForBG(On.Terraria.Main.orig_DrawSurfaceBG orig, Main self)
        {
            int moonLordIndex = NPC.FindFirstNPC(NPCID.MoonLordCore);
            bool useShader = InfernumMode.CanUseCustomAIs && moonLordIndex >= 0 && moonLordIndex < Main.maxNPCs && !Main.gameMenu;

            try
            {
                orig(self);
            }
            catch (IndexOutOfRangeException) { }

            if (useShader)
            {
                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, Main.instance.Rasterizer, null, Matrix.Identity);
                
                Vector2 scale = new Vector2(Main.screenWidth, Main.screenHeight) / Main.magicPixel.Size() * Main.GameViewMatrix.Zoom * 2f;
                Main.spriteBatch.Draw(Main.magicPixel, Vector2.Zero, null, Color.Black, 0f, Vector2.Zero, scale * 1.5f, 0, 0f);

                Main.spriteBatch.End();
                Main.spriteBatch.Begin();
            }
        }

        public void Load() => On.Terraria.Main.DrawSurfaceBG += PrepareShaderForBG;

        public void Unload() => On.Terraria.Main.DrawSurfaceBG -= PrepareShaderForBG;
    }
	
	public class InfernumAdditiveDrawingSystem : IHookEdit
	{
        private static void AdditiveDrawing(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);
            if (!cursor.TryGotoNext(MoveType.After, i => i.MatchCall<MoonlordDeathDrama>("DrawWhite")))
                return;

            cursor.EmitDelegate<Action>(() =>
            {
                Main.spriteBatch.SetBlendState(BlendState.Additive);

                // Draw Projectiles.
                for (int i = 0; i < Main.maxProjectiles; i++)
                {
                    if (!Main.projectile[i].active)
                        continue;

                    if (Main.projectile[i].modProjectile is IAdditiveDrawer)
					{
						IAdditiveDrawer d = (IAdditiveDrawer)Main.projectile[i].modProjectile;
						d.AdditiveDraw(Main.spriteBatch);
					}
                }

                // Draw NPCs.
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    if (!Main.npc[i].active)
                        continue;

                    if (Main.npc[i].modNPC is IAdditiveDrawer)
					{
						IAdditiveDrawer d = (IAdditiveDrawer)Main.npc[i].modNPC;
                        d.AdditiveDraw(Main.spriteBatch);
					}
                }

                Main.spriteBatch.SetBlendState(BlendState.AlphaBlend);
            });
        }
        public void Load() => IL.Terraria.Main.DoDraw += AdditiveDrawing;

        public void Unload() => IL.Terraria.Main.DoDraw -= AdditiveDrawing;		
	}
	
	#region General Particle Rendering	
	public class InfernumFusableParticle : IHookEdit
	{
        /*private static void DrawFusableParticles(On.Terraria.Main.orig_SortDrawCacheWorms orig, Main self)
        {
            DeathAshParticle.DrawAll();
            InfernumFusableParticleManager.RenderAllFusableParticles();

            orig(self);
        }

        private static void DrawForegroundParticles(On.Terraria.Main.orig_DrawInfernoRings orig, Main self)
        {
            GeneralParticleHandler.DrawAllParticles(Main.spriteBatch);
            orig(self);
        }

        private static void ResetRenderTargetSizes(On.Terraria.Main.orig_SetDisplayMode orig, int width, int height, bool fullscreen)
        {
            if (InfernumFusableParticleManager.HasBeenFormallyDefined)
                InfernumFusableParticleManager.LoadParticleRenderSets(true, width, height);
            orig(width, height, fullscreen);
        }*/
		
        private static void DrawGeneralParticles(On.Terraria.Main.orig_DrawInterface orig, Main self, GameTime gameTime)
        {
            GeneralParticleHandler.DrawAllParticles(Main.spriteBatch);
            DeathAshParticle.DrawAll();

            orig(self, gameTime);
        }

        private static void DrawFusableParticles(On.Terraria.Main.orig_SortDrawCacheWorms orig, Main self)
        {
            InfernumFusableParticleManager.RenderAllFusableParticles();

            orig(self);
        }		
		
        public void Load() => On.Terraria.Main.SortDrawCacheWorms += DrawFusableParticles;

        public void Unload() => On.Terraria.Main.SortDrawCacheWorms -= DrawFusableParticles;	

	}
	#endregion General Particle Rendering
}