using InfernumMode;
using CalamityMod;
using CalamityMod.NPCs;
using CalamityMod.NPCs.ProfanedGuardians;
using InfernumMode.GlobalInstances;
using InfernumMode.OverridingSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using static InfernumMode.BehaviorOverrides.BossAIs.ProfanedGuardians.GuardianComboAttackManager;
using InfernumMode.Projectiles;
using InfernumMode.Effects;
using InfernumMode.ExtraTextures;
using System;

namespace InfernumMode.BehaviorOverrides.BossAIs.ProfanedGuardians
{
    public class HealerGuardianBehaviorOverride : NPCBehaviorOverride
    {
        public override int NPCOverrideType => ModContent.NPCType<ProfanedGuardianBoss3>();

        public override NPCOverrideContext ContentToOverride => NPCOverrideContext.NPCAI | NPCOverrideContext.NPCPreDraw | NPCOverrideContext.NPCCheckDead;

        public override int? NPCIDToDeferToForTips => ModContent.NPCType<ProfanedGuardianBoss>();

        internal PrimitiveTrailCopy ShieldEnergyDrawer;

        #region AI
        public override bool PreAI(NPC npc)
        {
            int commanderIndex = GuardianComboAttackManager.FindCommanderIndex();
            if (commanderIndex == -1)
            {
                npc.active = false;
                npc.netUpdate = true;
                return false;
            }

            npc.timeLeft = 3600;
            CalamityGlobalNPC.doughnutBoss = commanderIndex;

            // Declare us as the healer.
            CalamityGlobalNPC.doughnutBossHealer = npc.whoAmI;

            NPC commander = Main.npc[commanderIndex];
            Player target = Main.player[commander.target];
            ref float attackState = ref commander.ai[0];
            ref float attackTimer = ref commander.ai[1];
            ref float drawShieldConnections = ref npc.ai[2];
            drawShieldConnections = 0f;
            ref float connectionsWidthScale = ref commander.Infernum().ExtraAI[HealerConnectionsWidthScaleIndex];

            npc.damage = 0;
            npc.target = commander.target;
            npc.spriteDirection = (npc.velocity.X > 0).ToDirectionInt();

            // Have DR.
            npc.Calamity().DR = 0.35f;

            // Reset taking damage.
            npc.dontTakeDamage = false;

            switch ((GuardiansAttackType)attackState)
            {
                case GuardiansAttackType.SpawnEffects:
                    DoBehavior_SpawnEffects(npc, target, ref attackTimer);
                    break;

                case GuardiansAttackType.FlappyBird:
                    DoBehavior_FlappyBird(npc, target, ref attackTimer, commander);
                    break;

                case GuardiansAttackType.SoloHealer:
                    DoBehavior_SoloHealer(npc, target, ref attackTimer, commander);
                    break;

                case GuardiansAttackType.SoloDefender:
                    DoBehavior_SoloDefender(npc, target, ref attackTimer, commander);
                    break;

                case GuardiansAttackType.HealerAndDefender:
                    DoBehavior_HealerAndDefender(npc, target, ref attackTimer, commander);
                    break;

                case GuardiansAttackType.HealerDeathAnimation:
                    DoBehavior_HealerDeathAnimation(npc, target, ref attackTimer, commander);
                    break;
            }

            if (drawShieldConnections == 1)
                connectionsWidthScale = MathHelper.Clamp(connectionsWidthScale + 0.1f, 0f, 1f);
            else
                connectionsWidthScale = MathHelper.Clamp(connectionsWidthScale - 0.1f, 0f, 1f);

            return false;
        }
        #endregion

        #region Drawing
        public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Color lightColor)
        {
            Texture2D texture = Main.npcTexture[npc.type];
            Texture2D glowmask = ModContent.GetTexture("CalamityMod/NPCs/ProfanedGuardians/ProfanedGuardianBoss3Glow");
            Texture2D glowmask2 = ModContent.GetTexture("CalamityMod/NPCs/ProfanedGuardians/ProfanedGuardianBoss3Glow2");
            Vector2 drawPosition = npc.Center - Main.screenPosition;
            Vector2 origin = npc.frame.Size() * 0.5f;
            SpriteEffects direction = npc.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            if (CalamityGlobalNPC.doughnutBoss == -1)
                return false;

            NPC commander = Main.npc[CalamityGlobalNPC.doughnutBoss];
            // If maintaining the front shield or shielding the commander, glow.
            if ((GuardiansAttackType)commander.ai[0] != GuardiansAttackType.SpawnEffects)
                DrawNPCBackglow(npc, spriteBatch, texture, direction);

            // Draw the npc.
            // If the white glow should be drawn.
            if ((GuardiansAttackType)commander.ai[0] == GuardiansAttackType.HealerDeathAnimation && npc.Infernum().ExtraAI[0] > 0)
                DrawWhiteGlowOverlay(npc, spriteBatch, texture, glowmask, glowmask2, commander, lightColor, direction);
            else
            {
                Main.spriteBatch.Draw(texture, drawPosition, npc.frame, npc.GetAlpha(lightColor), npc.rotation, origin, npc.scale, direction, 0f);
                Main.spriteBatch.Draw(glowmask, drawPosition, npc.frame, npc.GetAlpha(Color.White), npc.rotation, origin, npc.scale, direction, 0f);
                Main.spriteBatch.Draw(glowmask2, drawPosition, npc.frame, npc.GetAlpha(Color.White), npc.rotation, origin, npc.scale, direction, 0f);
            }
            // If shield connections should be drawn.
            if (npc.ai[2] == 1f || commander.Infernum().ExtraAI[HealerConnectionsWidthScaleIndex] > 0f)
                DrawShieldConnections(npc);
            return false;
        }

        public static void DrawNPCBackglow(NPC npc, SpriteBatch spriteBatch, Texture2D npcTexture, SpriteEffects direction)
        {
            int backglowAmount = InfernumConfig.Instance.ReducedGraphicsConfig ? 6 : 12;
            for (int i = 0; i < backglowAmount; i++)
            {
                Vector2 backglowOffset = (MathHelper.TwoPi * i / backglowAmount).ToRotationVector2() * 4f;
                Color backglowColor = MagicSpiralCrystalShot.ColorSet[0];
                backglowColor.A = 0;
                spriteBatch.Draw(npcTexture, npc.Center + backglowOffset - Main.screenPosition, npc.frame, backglowColor, npc.rotation, npc.frame.Size() * 0.5f, npc.scale, direction, 0);
            }
        }

        public static Color EnergyColorFunction(float completionRatio) => MagicSpiralCrystalShot.ColorSet[0];

        public void DrawShieldConnections(NPC npc)
        {
            NPC commander = Main.npc[CalamityGlobalNPC.doughnutBoss];
            if (ShieldEnergyDrawer == null)
                ShieldEnergyDrawer = new PrimitiveTrailCopy((float _) => commander.width * 0.35f,
                    EnergyColorFunction, null, true, InfernumEffectsRegistry.PulsatingLaserVertexShader);
					
            NPC npcToConnectTo = null;
            if (Main.npc.IndexInRange(GlobalNPCOverrides.ProfanedCrystal))
            {
                if (Main.npc[GlobalNPCOverrides.ProfanedCrystal].active && Main.npc[GlobalNPCOverrides.ProfanedCrystal].type == ModContent.NPCType<HealerShieldCrystal>())
                    npcToConnectTo = Main.npc[GlobalNPCOverrides.ProfanedCrystal];
                else if (Main.npc.IndexInRange(CalamityGlobalNPC.doughnutBoss) && Main.npc[CalamityGlobalNPC.doughnutBoss].active)
                    npcToConnectTo = Main.npc[CalamityGlobalNPC.doughnutBoss];
            }

            // Messy hack to stop the healer connecting to the commander immediately after the crystal dies. This should be made nicer.
            else if (Main.npc.IndexInRange(CalamityGlobalNPC.doughnutBoss) && (((GuardiansAttackType)commander.ai[0] is GuardiansAttackType.SoloHealer && commander.ai[1] > 15) || (GuardiansAttackType)commander.ai[0] is GuardiansAttackType.SoloDefender))
            {
                if (Main.npc[CalamityGlobalNPC.doughnutBoss].active)
                    npcToConnectTo = Main.npc[CalamityGlobalNPC.doughnutBoss];
            }
            else
                return;

            if (npcToConnectTo is null)
                return;

            Vector2 startPos = npc.Center + new Vector2(npc.spriteDirection * 32f, 12f);
            Vector2 endPos = npcToConnectTo.type == CommanderType ? npcToConnectTo.Top + new Vector2(-50, 50) : npcToConnectTo.Top;

            InfernumEffectsRegistry.PulsatingLaserVertexShader.SetShaderTexture(InfernumTextureRegistry.StreakBubbleGlow);
            InfernumEffectsRegistry.PulsatingLaserVertexShader.UseColor(Color.Lerp(MagicSpiralCrystalShot.ColorSet[0], Color.White, 0.1f));
            InfernumEffectsRegistry.PulsatingLaserVertexShader.UseSaturation(2.5f * commander.Infernum().ExtraAI[HealerConnectionsWidthScaleIndex]);
            InfernumEffectsRegistry.PulsatingLaserVertexShader.Shader.Parameters["usePulsing"].SetValue(true);
            InfernumEffectsRegistry.PulsatingLaserVertexShader.Shader.Parameters["reverseDirection"].SetValue(false);

            Vector2[] drawPositions = new Vector2[8];
            for (int i = 0; i < drawPositions.Length; i++)
                drawPositions[i] = Vector2.Lerp(startPos, endPos, (float)i / drawPositions.Length);

            ShieldEnergyDrawer.Draw(drawPositions, -Main.screenPosition, 30);
            endPos = npcToConnectTo.type == CommanderType ? npcToConnectTo.Bottom - new Vector2(50, 50) : npcToConnectTo.Bottom;
            for (int i = 0; i < drawPositions.Length; i++)
                drawPositions[i] = Vector2.Lerp(startPos, endPos, (float)i / drawPositions.Length);
            ShieldEnergyDrawer.Draw(drawPositions, -Main.screenPosition, 30);

            // Draw a glow over the crystal.
            Texture2D glowTexture = ModContent.GetTexture("CalamityMod/Particles/BloomCircle");
            Vector2 drawPosition = startPos - Main.screenPosition;
            Vector2 glowOrigin = glowTexture.Size() * 0.5f;
            Color baseColor = MagicSpiralCrystalShot.ColorSet[0];
            baseColor.A = 0;
            float widthScale = commander.Infernum().ExtraAI[HealerConnectionsWidthScaleIndex];
            Color modifiedColor = Color.Lerp(Color.Transparent, baseColor, widthScale);
            float scaleSine = (1f + (float)Math.Sin(Main.GlobalTime)) / 2f;
            float glowScale = MathHelper.Lerp(1.1f, 1.15f, scaleSine);
            Color finalColor = Color.Lerp(modifiedColor, new Color(1f, 1f, 1f, 0f), scaleSine);
            Main.spriteBatch.Draw(glowTexture, drawPosition, null, finalColor, 0f, glowOrigin, 1f, SpriteEffects.None, 0);

            // Draw two spinning gleams over the crystal.
            Texture2D gleamTexture = ModContent.GetTexture("CalamityMod/Projectiles/StarProj");
            Vector2 gleamOrigin = gleamTexture.Size() * 0.5f;
            float gleamScale = MathHelper.Lerp(2.2f, 2.4f, scaleSine);
            float gleamRotation = MathHelper.Pi * Main.GlobalTime * 2f;
            Main.spriteBatch.Draw(gleamTexture, drawPosition, null, finalColor, gleamRotation, gleamOrigin, gleamScale, 0, 0f);
            Main.spriteBatch.Draw(gleamTexture, drawPosition, null, finalColor, -gleamRotation, gleamOrigin, gleamScale, 0, 0f);
        }

        public static void DrawWhiteGlowOverlay(NPC npc, SpriteBatch spriteBatch, Texture2D texture, Texture2D glowmask, Texture2D glowmask2, NPC commander, Color lightColor, SpriteEffects direction)
        {
            float commanderTimer = commander.ai[1];
            float whiteGlowTime = 120f;
            float ashesTime = 90;
            Vector2 drawPosition = npc.Center - Main.screenPosition;
            float opacityScalar = npc.Infernum().ExtraAI[0];

            float opacityScalar2 = 1f;
            if (commanderTimer > whiteGlowTime)
            {
                float interlopant = (commanderTimer - whiteGlowTime) / ashesTime;
                opacityScalar2 = 1 - Utilities.LinearEasing(interlopant, 0);
            }

            // Commander glow effect, as if the commander is trying to stop it dying.
            Texture2D glow = ModContent.GetTexture("CalamityMod/Particles/BloomCircle");
            Color drawColor = Color.Lerp(WayfinderSymbol.Colors[1], WayfinderSymbol.Colors[2], 0.5f);
            drawColor.A = 0;
            Color drawColor2 = Color.Lerp(WayfinderSymbol.Colors[0], WayfinderSymbol.Colors[1], 0.5f);
            drawColor2.A = 0;
            Vector2 origin = glow.Size() * 0.5f;
            spriteBatch.Draw(glow, drawPosition, null, drawColor * opacityScalar * opacityScalar2, 0f, origin, 3.5f, SpriteEffects.None, 0f);
            spriteBatch.Draw(glow, drawPosition, null, drawColor2 * opacityScalar * opacityScalar2, 0f, origin, 2.5f, SpriteEffects.None, 0f);

            // Draw back afterimages, indicating that the guardian is fading away into ashes.
            if (commanderTimer > whiteGlowTime)
            {
                float interlopant = (commanderTimer - whiteGlowTime) / ashesTime;
                float smoothed = Utilities.SineInEasing(interlopant, 0);
                float radius = MathHelper.Lerp(0f, 55f, smoothed);
                if (radius > 0.5f)
                {
                    for (int i = 0; i < (InfernumConfig.Instance.ReducedGraphicsConfig ? 10 : 24); i++)
                    {
                        Vector2 drawOffset = (MathHelper.TwoPi * i / 24f).ToRotationVector2() * radius;
                        Color backimageColor = WayfinderSymbol.Colors[0];
                        backimageColor.A = (byte)MathHelper.Lerp(164f, 0f, MathHelper.Lerp(1f, 0f, smoothed));
                        spriteBatch.Draw(texture, drawPosition + drawOffset, npc.frame, backimageColor * npc.Opacity * (1f - smoothed), npc.rotation, npc.frame.Size() * 0.5f, npc.scale, direction, 0f);
                    }
                }
            }

            // Draw the main sprites.
            Main.spriteBatch.Draw(texture, drawPosition, npc.frame, npc.GetAlpha(lightColor) * opacityScalar2, npc.rotation, npc.frame.Size() * 0.5f, npc.scale, direction, 0f);
            Main.spriteBatch.Draw(glowmask, drawPosition, npc.frame, npc.GetAlpha(Color.White) * opacityScalar2, npc.rotation, npc.frame.Size() * 0.5f, npc.scale, direction, 0f);
            Main.spriteBatch.Draw(glowmask2, drawPosition, npc.frame, npc.GetAlpha(Color.White) * opacityScalar2, npc.rotation, npc.frame.Size() * 0.5f, npc.scale, direction, 0f);

            // Draw a white overlay
            float overlayAmount = MathHelper.Lerp(0f, 5f, opacityScalar2);
            for (int i = 0; i < overlayAmount; i++)
            {
                Vector2 backglowOffset = (MathHelper.TwoPi * i / overlayAmount).ToRotationVector2() * 2f;
                Color backglowColor = Color.Lerp(MagicSpiralCrystalShot.ColorSet[0], Color.White, commanderTimer / whiteGlowTime);
                backglowColor.A = 0;
                spriteBatch.Draw(texture, npc.Center + backglowOffset - Main.screenPosition, npc.frame, backglowColor * opacityScalar * opacityScalar2, npc.rotation, npc.frame.Size() * 0.5f, npc.scale, direction, 0);
            }

            // Easings for making the shield bob in and out opacity wise.
            float opacity0to1 = (float)Math.Sin(Math.PI * (commanderTimer / whiteGlowTime));
            float shieldOpacity;
            if (commanderTimer >= whiteGlowTime / 2f)
                shieldOpacity = Utilities.EaseInBounce(opacity0to1);
            else
                shieldOpacity = Utilities.ExpInEasing(opacity0to1, 0);

            // Draw the shield, as if its trying to protect itself in its last moments instead of the commander.
            AttackerGuardianBehaviorOverride.DrawHealerShield(npc, spriteBatch, 3.3f, shieldOpacity * 1.15f);
        }
        #endregion

        #region Death Effects
        public override bool CheckDead(NPC npc)
        {
            NPC commander = Main.npc[CalamityGlobalNPC.doughnutBoss];
            DespawnTransitionProjectiles();
            SelectNewAttack(commander, ref commander.ai[1], (float)GuardiansAttackType.HealerDeathAnimation);
            npc.life = npc.lifeMax;
            npc.netUpdate = true;
            return false;
        }
        #endregion
    }
}
