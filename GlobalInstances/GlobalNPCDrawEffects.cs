using CalamityMod;
using CalamityMod.NPCs;
using InfernumMode.BehaviorOverrides.BossAIs.Draedon.Ares;
using CalamityMod.NPCs.Cryogen;
using CalamityMod.NPCs.DevourerofGods;
using CalamityMod.NPCs.ExoMechs.Thanatos;
using CalamityMod.NPCs.Leviathan;
using CalamityMod.NPCs.Polterghast;
using CalamityMod.NPCs.Signus;
using CalamityMod.NPCs.SupremeCalamitas;
using CalamityMod.NPCs.ExoMechs.Apollo;
using CalamityMod.NPCs.Yharon;
using CalamityMod.NPCs.Calamitas;
using InfernumMode.BehaviorOverrides.BossAIs.DoG;
using InfernumMode.BehaviorOverrides.BossAIs.MoonLord;
using InfernumMode.BehaviorOverrides.BossAIs.Draedon.ArtemisAndApollo;
using InfernumMode.OverridingSystem;
using InfernumMode.ILEditingStuff;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using InfernumMode.BehaviorOverrides.BossAIs.Yharon;
using CalamityMod.NPCs.ExoMechs.Artemis;
using InfernumMode.BehaviorOverrides.BossAIs.Draedon;
using InfernumMode.BehaviorOverrides.BossAIs.Twins;

namespace InfernumMode.GlobalInstances
{
    public class GlobalNPCDrawEffects : GlobalNPC
    {
        public override bool InstancePerEntity => true;

        #region Get Alpha
        public override Color? GetAlpha(NPC npc, Color drawColor)
        {
            // Give a dark tint to the moon lord.
            if (npc.type == NPCID.MoonLordHand || npc.type == NPCID.MoonLordHead || npc.type == NPCID.MoonLordCore)
            {
                if (InfernumMode.CanUseCustomAIs)
                    return MoonLordCoreBehaviorOverride.OverallTint;
            }

            if (npc.type == ModContent.NPCType<ThanatosHead>() ||
                npc.type == ModContent.NPCType<ThanatosBody1>() ||
                npc.type == ModContent.NPCType<ThanatosBody2>() ||
                npc.type == ModContent.NPCType<ThanatosTail>())
            {
                bool dealsNoContactDamage = npc.damage == 0;
                npc.Infernum().ExtraAI[20] = MathHelper.Clamp(npc.Infernum().ExtraAI[20] + dealsNoContactDamage.ToDirectionInt() * 0.025f, 0f, 1f);
                return Color.Lerp(drawColor * npc.Opacity, new Color(102, 74, 232, 0) * npc.Opacity * 0.6f, npc.Infernum().ExtraAI[20]);
            }

            return base.GetAlpha(npc, drawColor);
        }
        #endregion

        #region Map Icon Manipulation
        public override void BossHeadSlot(NPC npc, ref int index)
        {
            if (!InfernumMode.CanUseCustomAIs)
                return;

            bool isDoG = npc.type == ModContent.NPCType<DevourerofGodsHead>() || npc.type == ModContent.NPCType<DevourerofGodsBody>() || npc.type == ModContent.NPCType<DevourerofGodsTail>();
            if (isDoG)
            {
                if (npc.Opacity <= 0.02f)
                {
                    index = -1;
                    return;
                }

                bool inPhase2 = DoGPhase2HeadBehaviorOverride.InPhase2;
                if (npc.type == ModContent.NPCType<DevourerofGodsHead>())
                    index = inPhase2 ? DevourerofGodsHead.phase2IconIndex : DevourerofGodsHead.phase1IconIndex;
                else if (npc.type == ModContent.NPCType<DevourerofGodsBody>())
                    index = inPhase2 ? DevourerofGodsBody.phase2IconIndex : -1;
                else if (npc.type == ModContent.NPCType<DevourerofGodsTail>())
                    index = inPhase2 ? DevourerofGodsTail.phase2IconIndex : DevourerofGodsTail.phase1IconIndex;
            }

            // Make Anahita completely invisible on the map when sufficiently faded out.
            if (npc.type == ModContent.NPCType<Siren>() && npc.Opacity < 0.1f)
                index = -1;

            // Make Signus completely invisible on the map.
            if (npc.type == ModContent.NPCType<Signus>() && npc.Opacity < 0.3f)
                index = -1;

            // Prevent Yharon from showing himself amongst his illusions in Subphase 10.
            if (npc.type == ModContent.NPCType<Yharon>())
            {
                if (npc.life / (float)npc.lifeMax <= YharonBehaviorOverride.Subphase8LifeRatio && YharonBehaviorOverride.InSecondPhase)
                    index = -1;
            }

            // Have Cryogen use a custom map icon.
            if (npc.type == ModContent.NPCType<Cryogen>())
                index = ModContent.GetModBossHeadSlot("InfernumMode/BehaviorOverrides/BossAIs/Cryogen/CryogenMapIcon");
            // Have Sepulcher use a custom map icon.
            if (npc.type == ModContent.NPCType<SCalWormHead>())
                index = ModContent.GetModBossHeadSlot("InfernumMode/BehaviorOverrides/BossAIs/SupremeCalamitas/SepulcherMapIcon");
			
			if (npc.type == ModContent.NPCType<CalamitasRun3>())
				index = ModContent.GetModBossHeadSlot("InfernumMode/BehaviorOverrides/BossAIs/CalamitasShadow/CalShadowMapIcon");
			
			if (npc.type == ModContent.NPCType<CalamitasRun>())
				index = ModContent.GetModBossHeadSlot("InfernumMode/BehaviorOverrides/BossAIs/CalamitasShadow/CataclysmMapIcon");
			
			if (npc.type == ModContent.NPCType<CalamitasRun2>())
				index = ModContent.GetModBossHeadSlot("InfernumMode/BehaviorOverrides/BossAIs/CalamitasShadow/CatastropheMapIcon");	

			if (npc.type == ModContent.NPCType<Artemis>())
            {
                if (npc.Opacity <= 0f)
                    index = -1;
                else if (ExoMechManagement.ExoTwinsAreInSecondPhase)
                    index = Artemis.phase2IconIndex;
                else
                    index = Artemis.phase1IconIndex;
            }
            if (npc.type == ModContent.NPCType<Apollo>())
            {
                if (npc.Opacity <= 0f)
                    index = -1;
                else if (ExoMechManagement.ExoTwinsAreInSecondPhase)
                    index = Apollo.phase2IconIndex;
                else
                    index = Apollo.phase1IconIndex;
            }

			if (npc.type == NPCID.Spazmatism)
            {
                if (npc.Opacity <= 0f)
                    index = -1;
                else if (TwinsAttackSynchronizer.PersonallyInPhase2(npc))
                    index = 21;
                else
                    index = 20;
            }
            if (npc.type == NPCID.Retinazer)
            {
                if (npc.Opacity <= 0f)
                    index = -1;
                else if (TwinsAttackSynchronizer.PersonallyInPhase2(npc))
                    index = 16;
                else
                    index = 15;
            }
        }

        public override void BossHeadRotation(NPC npc, ref float rotation)
        {
            bool isDoG = npc.type == ModContent.NPCType<DevourerofGodsHead>() || npc.type == ModContent.NPCType<DevourerofGodsBody>() || npc.type == ModContent.NPCType<DevourerofGodsTail>();
            if (isDoG)
            {
                if (DoGPhase2HeadBehaviorOverride.InPhase2)
                    rotation = npc.rotation;
            }

            if (npc.type == ModContent.NPCType<Polterghast>())
                rotation = npc.rotation;
        }
		
        public override void BossHeadSpriteEffects(NPC npc, ref SpriteEffects spriteEffects)
        {
            if (!InfernumMode.CanUseCustomAIs)
                return;

            if (npc.type == ModContent.NPCType<CalamitasRun3>() || npc.type == ModContent.NPCType<CalamitasRun2>() || npc.type == ModContent.NPCType<CalamitasRun>())
                spriteEffects = npc.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
        }		

        #endregion

        #region Manual Drawing
        public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Color drawColor)
        {
            if (InfernumMode.CanUseCustomAIs)
            {
                bool isDoG = npc.type == ModContent.NPCType<DevourerofGodsHead>() || npc.type == ModContent.NPCType<DevourerofGodsBody>() || npc.type == ModContent.NPCType<DevourerofGodsTail>();
                if (isDoG && npc.alpha >= 252)
                    return false;

                if (OverridingListManager.InfernumPreDrawOverrideList.ContainsKey(npc.type))
                {
                    if (Main.LocalPlayer.Calamity().trippy)
                    {
                        SpriteEffects direction = SpriteEffects.None;
                        if (npc.spriteDirection == 1)
                            direction = SpriteEffects.FlipHorizontally;

                        Vector2 origin = npc.frame.Size() * 0.5f;
                        Color shroomColor = npc.GetAlpha(new Color(Main.DiscoR, Main.DiscoG, Main.DiscoB, 0));
                        float colorFadeFactor = 0.99f;
                        shroomColor.R = (byte)(shroomColor.R * colorFadeFactor);
                        shroomColor.G = (byte)(shroomColor.G * colorFadeFactor);
                        shroomColor.B = (byte)(shroomColor.B * colorFadeFactor);
                        shroomColor.A = (byte)(shroomColor.A * colorFadeFactor);
                        for (int i = 0; i < 4; i++)
                        {
                            Vector2 drawPosition = npc.Center;
                            float horizontalOffset = Math.Abs(npc.Center.X - Main.LocalPlayer.Center.X);
                            float verticalOffset = Math.Abs(npc.Center.Y - Main.LocalPlayer.Center.Y);

                            if (i == 0 || i == 2)
                                drawPosition.X = Main.LocalPlayer.Center.X + horizontalOffset;
                            else
                                drawPosition.X = Main.LocalPlayer.Center.X - horizontalOffset;

                            if (i == 0 || i == 1)
                                drawPosition.Y = Main.LocalPlayer.Center.Y + verticalOffset;
                            else
                                drawPosition.Y = Main.LocalPlayer.Center.Y - verticalOffset;
                            drawPosition.Y += npc.gfxOffY;
                            drawPosition -= Main.screenPosition;

                            Main.spriteBatch.Draw(Main.npcTexture[npc.type], drawPosition, npc.frame, shroomColor, npc.rotation, origin, npc.scale, direction, 0f);
                        }
                    }
                    return OverridingListManager.InfernumPreDrawOverrideList[npc.type].Invoke(npc, Main.spriteBatch, drawColor);
                }
            }
            return base.PreDraw(npc, Main.spriteBatch, drawColor);
        }
        #endregion

        #region Healthbar Manipulation
        public override bool? DrawHealthBar(NPC npc, byte hbPosition, ref float scale, ref Vector2 position)
        {
            if (!InfernumMode.CanUseCustomAIs)
                return base.DrawHealthBar(npc, hbPosition, ref scale, ref position);

            if (npc.type == NPCID.CultistBoss || npc.type == NPCID.CultistBossClone)
                scale = 1f;

            bool isDoG = npc.type == ModContent.NPCType<DevourerofGodsHead>() || npc.type == ModContent.NPCType<DevourerofGodsBody>() || npc.type == ModContent.NPCType<DevourerofGodsTail>();
            if (isDoG && npc.alpha >= 252)
                return false;
			
            // Don't draw HP bars if Ares is in the background.
            if (npc.realLife == CalamityGlobalNPC.draedonExoMechPrime && CalamityGlobalNPC.draedonExoMechPrime >= 0 && Math.Abs(Main.npc[CalamityGlobalNPC.draedonExoMechPrime].ai[2]) >= 0.25f)
                return false;			

            if (npc.type == NPCID.EaterofWorldsBody)
                return false;

            return base.DrawHealthBar(npc, hbPosition, ref scale, ref position);
        }

        #endregion
		
        #region Layering Manipulation
        public override void DrawBehind(NPC npc, int index)
        {
            if (!InfernumMode.CanUseCustomAIs)
                return;

            bool isAres = npc.whoAmI == CalamityGlobalNPC.draedonExoMechPrime || npc.realLife == CalamityGlobalNPC.draedonExoMechPrime;
            if (isAres && CalamityGlobalNPC.draedonExoMechPrime >= 0 && AresBodyBehaviorOverride.ShouldDrawBehindTiles && npc.hide)
            {
                Main.instance.DrawCacheNPCProjectiles.Remove(index);
                ScreenOverlaysSystem.DrawCacheBeforeBlack.Add(index);
            }
        }
        #endregion Layering Manipulation		

        #region Frame Manipulation
        public override void FindFrame(NPC npc, int frameHeight)
        {
            if (OverridingListManager.InfernumFrameOverrideList.ContainsKey(npc.type) && InfernumMode.CanUseCustomAIs)
                OverridingListManager.InfernumFrameOverrideList[npc.type].DynamicInvoke(npc, frameHeight);
        }
        #endregion
		
		
    }
}