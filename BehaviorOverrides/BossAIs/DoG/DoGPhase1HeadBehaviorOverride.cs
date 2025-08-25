using CalamityMod;
using CalamityMod.World;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Buffs.StatDebuffs;
using CalamityMod.NPCs;
using CalamityMod.NPCs.DevourerofGods;
using CalamityMod.Projectiles.Boss;
using InfernumMode.BossIntroScreens;
using InfernumMode.OverridingSystem;
using InfernumMode.Skies;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

using DoGHead = CalamityMod.NPCs.DevourerofGods.DevourerofGodsHead;

namespace InfernumMode.BehaviorOverrides.BossAIs.DoG
{
    public class DoGPhase1HeadBehaviorOverride : NPCBehaviorOverride
    {
        public enum Phase2TransitionState
        {
            NotEnteringPhase2,
            NeedsToSummonPortal,
            EnteringPortal
        }

        public static Phase2TransitionState CurrentPhase2TransitionState
        {
            get
            {
                if (CalamityGlobalNPC.DoGHead < 0)
                    return Phase2TransitionState.NotEnteringPhase2;

                NPC npc = Main.npc[CalamityGlobalNPC.DoGHead];
                return (Phase2TransitionState)npc.Infernum().ExtraAI[Phase2TransitionStateIndex];
            }
            set
            {
                if (CalamityGlobalNPC.DoGHead < 0)
                    return;

                NPC npc = Main.npc[CalamityGlobalNPC.DoGHead];
                npc.Infernum().ExtraAI[Phase2TransitionStateIndex] = (int)value;
                npc.netUpdate = true;
            }
        }

        public override int NPCOverrideType => ModContent.NPCType<DoGHead>();

        public override NPCOverrideContext ContentToOverride => NPCOverrideContext.NPCAI | NPCOverrideContext.NPCPreDraw;

        public const float Phase2LifeRatio = 0.8f;

        public const int PassiveMovementTimeP1 = 420;

        public const int AggressiveMovementTimeP1 = 600;

        // Define a bunch of AI indices. This is slightly cursed.
        public const int UniversalFightTimerIndex = 0;

        public const int CurrentFlyAccelerationIndex = 1;

        public const int JawRotationIndex = 2;

        public const int ChompEffectsCountdownIndex = 3;

        public const int Phase2TransitionStateIndex = 4;

        public const int Phase2PortalProjectileIndexIndex = 5;

        public const int InPhase2FlagIndex = 6;

        public const int PhaseCycleTimerIndex = 7;

        public const int PassiveAttackDelayTimerIndex = 8;

        public const int PerformingSpecialAttackFlagIndex = 9;

        public const int SpecialAttackTimerIndex = 10;

        public const int SpecialAttackTypeIndex = 11;

        public const int HasEnteredFinalPhaseFlagIndex = 12;

        public const int AnimationMoveDelayIndex = 13;

        public const int HasPerformedSpecialAttackYetFlagIndex = 14;

        public const int Phase2IntroductionAnimationTimerIndex = 15;

        public const int DeathAnimationTimerIndex = 16;

        public const int DestroyedSegmentsCountIndex = 17;

        public const int InitialUncoilTimerIndex = 18;

        public const int ForceDoGIntoPhase2PortalTimerIndex = 19;

        public const int HasTeleportedAboveTargetFlagIndex = 20;

        public const int HasSpawnedSegmentsIndex = 21;

        public const int ChargeGatePortalIndexIndex = 22;

        public const int ChargeGatePortalTelegraphTimeIndex = 23;

        public const int SegmentNumberIndex = 24;

        public const int BodySegmentFadeTypeIndex = 37;

        public const int AntimatterFormInterpolantIndex = 26;

        public const int SentinelAttackTimerIndex = 27;

        public const int Phase2AggressiveChargeCycleCounterIndex = 28;

        public const int PerpendicularPortalAttackStateIndex = 29;

        public const int PerpendicularPortalAttackTimerIndex = 30;

        public const int PerpendicularPortalAngleIndex = 31;

        public const int PreviousSpecialAttackTypeIndex = 32;

        public const int PreviousSnapAngleIndex = 33;

        public const int TimeSinceLastSnapIndex = 34;

        public const int DamageImmunityCountdownIndex = 35;

        public const int BodySegmentDefense = 70;

        public const float BodySegmentDR = 0.925f;

        public override float[] PhaseLifeRatioThresholds => new float[]
        {
            Phase2LifeRatio,
            DoGPhase2HeadBehaviorOverride.CanUseSpecialAttacksLifeRatio,
            DoGPhase2HeadBehaviorOverride.FinalPhaseLifeRatio
        };

        private void RedefineMapSlotConditions(NPC npc, ref int index)
        {
            bool isDoG = npc.type == ModContent.NPCType<DoGHead>() || npc.type == ModContent.NPCType<DevourerofGodsBody>() || npc.type == ModContent.NPCType<DevourerofGodsTail>();
            if (isDoG)
            {
                if (npc.Opacity <= 0.02f)
                {
                    index = -1;
                    return;
                }

                int p1HeadIcon = ModContent.GetModBossHeadSlot("InfernumMode/Content/BehaviorOverrides/BossAIs/DoG/DoGP1HeadMapIcon");
                int p1TailIcon = ModContent.GetModBossHeadSlot("InfernumMode/Content/BehaviorOverrides/BossAIs/DoG/DoGP1TailMapIcon");
                int p2HeadIcon = ModContent.GetModBossHeadSlot("InfernumMode/Content/BehaviorOverrides/BossAIs/DoG/DoGP2HeadMapIcon");
                int p2BodyIcon = ModContent.GetModBossHeadSlot("InfernumMode/Content/BehaviorOverrides/BossAIs/DoG/DoGP2BodyMapIcon");
                int p2TailIcon = ModContent.GetModBossHeadSlot("InfernumMode/Content/BehaviorOverrides/BossAIs/DoG/DoGP2TailMapIcon");
                bool inPhase2 = DoGPhase2HeadBehaviorOverride.InPhase2;

                if (npc.type == ModContent.NPCType<DoGHead>())
                    index = inPhase2 ? p2HeadIcon : p1HeadIcon;
                else if (npc.type == ModContent.NPCType<DevourerofGodsBody>())
                    index = inPhase2 ? p2BodyIcon : -1;
                else if (npc.type == ModContent.NPCType<DevourerofGodsTail>())
                    index = inPhase2 ? p2TailIcon : p1TailIcon;
            }
        }

        private bool UpdateLifeTriggers(NPC npc, ref double damage, int realDamage, int defense, ref float knockback, int hitDirection, ref bool crit)
        {
            // Make DoG enter the second phase once ready.
            bool isDoG = npc.type == ModContent.NPCType<DoGHead>() || npc.type == ModContent.NPCType<DevourerofGodsBody>() || npc.type == ModContent.NPCType<DevourerofGodsTail>();
            return !isDoG || HandleDoGLifeBasedHitTriggers(npc, realDamage, ref damage);
        }

        public static bool HandleDoGLifeBasedHitTriggers(NPC npc, double realDamage, ref double damage)
        {
            int life = npc.realLife >= 0 ? Main.npc[npc.realLife].life : npc.life;

            // Disable damage and enter phase 2 if the hit would bring DoG down to a sufficiently low quantity of HP.
            if (life - realDamage <= npc.lifeMax * Phase2LifeRatio && !DoGPhase2HeadBehaviorOverride.InPhase2 && CurrentPhase2TransitionState == Phase2TransitionState.NotEnteringPhase2)
            {
                damage = 0;
                npc.dontTakeDamage = true;
                CurrentPhase2TransitionState = Phase2TransitionState.NeedsToSummonPortal;
                return false;
            }

            // Disable damage and start the death animation if the hit would kill DoG.
            if (life - realDamage <= 1000 && DoGPhase2HeadBehaviorOverride.InPhase2)
            {
                damage = 0;
                npc.dontTakeDamage = true;
                if (npc.Infernum().ExtraAI[DeathAnimationTimerIndex] == 0f)
                {
                    Main.PlaySound(InfernumMode.CalamityMod.GetLegacySoundSlot(SoundType.Custom, "Sounds/Custom/DevourerSpawn"), npc.Center);
                    npc.Infernum().ExtraAI[DeathAnimationTimerIndex] = 1f;
                }
                return false;
            }
            return true;
        }

        #region AI
        public override bool PreAI(NPC npc)
        {
            // Disable secondary teleport effects.
            //Main.player[npc.target].Calamity().normalityRelocator = false;
            //Main.player[npc.target].Calamity().spectralVeil = false;

            ref float universalFightTimer = ref npc.Infernum().ExtraAI[UniversalFightTimerIndex];
            ref float flyAcceleration = ref npc.Infernum().ExtraAI[CurrentFlyAccelerationIndex];
            ref float jawRotation = ref npc.Infernum().ExtraAI[JawRotationIndex];
            ref float chompEffectsCountdown = ref npc.Infernum().ExtraAI[ChompEffectsCountdownIndex];
            ref float portalIndex = ref npc.Infernum().ExtraAI[Phase2PortalProjectileIndexIndex];
            ref float phaseCycleTimer = ref npc.Infernum().ExtraAI[PhaseCycleTimerIndex];
            ref float passiveAttackDelay = ref npc.Infernum().ExtraAI[PassiveAttackDelayTimerIndex];
            ref float uncoilTimer = ref npc.Infernum().ExtraAI[InitialUncoilTimerIndex];
            ref float segmentFadeType = ref npc.Infernum().ExtraAI[BodySegmentFadeTypeIndex];
            ref float getInTheFuckingPortalTimer = ref npc.Infernum().ExtraAI[ForceDoGIntoPhase2PortalTimerIndex];

            // Increment timers.
            universalFightTimer++;
            phaseCycleTimer++;
            passiveAttackDelay++;

            // Adjust scale.
            npc.scale = 1.2f;

            // Adjust DR and defense.
            npc.defense = 0;
            npc.Calamity().DR = 0.3f;
            npc.takenDamageMultiplier = 2f;

            // Declare this NPC as the occupant of the DoG whoAmI index.
            CalamityGlobalNPC.DoGHead = npc.whoAmI;

            // Stop rain, because DoG doesn't like it when rain detracts from him trying to snap your head off.
            if (Main.raining)
                Main.raining = false;

            // Prevent the Godslayer Inferno and Whispering Death debuff from being a problem by completely disabling both for the target.
            if (Main.player[npc.target].HasBuff(ModContent.BuffType<GodSlayerInferno>()))
                Main.player[npc.target].ClearBuff(ModContent.BuffType<GodSlayerInferno>());
            if (Main.player[npc.target].HasBuff(ModContent.BuffType<WhisperingDeath>()))
                Main.player[npc.target].ClearBuff(ModContent.BuffType<WhisperingDeath>());

            // Disable most debuffs.
            DoGPhase1BodyBehaviorOverride.KillUnbalancedDebuffs(npc);

            // Emit light.
            Lighting.AddLight((int)(npc.Center.X / 16f), (int)(npc.Center.Y / 16f), 0.2f, 0.05f, 0.2f);

            // Reset the NPC index that stores this segment's true HP.
            if (npc.ai[3] > 0f)
                npc.realLife = (int)npc.ai[3];

            npc.dontTakeDamage = CurrentPhase2TransitionState == Phase2TransitionState.EnteringPortal;

            // Determine the hitbox size.
            npc.Size = Vector2.One * 132f;

            // Defer all further execution to the second phase AI manager if in the second phase.
            if (DoGPhase2HeadBehaviorOverride.InPhase2)
            {
                npc.Calamity().CanHaveBossHealthBar = true;
                npc.ModNPC<DoGHead>().Phase2Started = true;
                npc.Size = Vector2.One * 176f;
                return DoGPhase2HeadBehaviorOverride.Phase2AI(npc, ref phaseCycleTimer, ref passiveAttackDelay, ref portalIndex, ref segmentFadeType, ref universalFightTimer);
            }

            // Set music.

            // Do through the portal once ready to enter the second phase.
            if (CurrentPhase2TransitionState != Phase2TransitionState.NotEnteringPhase2)
            {
				npc.modNPC.music = (InfernumMode.CalamityMod as CalamityMod.CalamityMod).GetMusicFromMusicMod("DevourerOfGodsP2") ?? MusicID.LunarBoss;
                HandlePhase2TransitionEffect(npc, ref portalIndex);
                getInTheFuckingPortalTimer++;
                if (getInTheFuckingPortalTimer >= 540f)
                {
                    DoGPhase2HeadBehaviorOverride.InPhase2 = true;
                    CurrentPhase2TransitionState = Phase2TransitionState.NotEnteringPhase2;
                }

                return false;
            }

            // Reset opacity.
            npc.Opacity = MathHelper.Lerp(npc.Opacity, 1f, 0.25f);

            // Select a new target if an old one was lost.
            npc.TargetClosestIfTargetIsInvalid();
            Player target = Main.player[npc.target];

            // Teleport above the target on the very first frame. This ensures that DoG will always be in a consistent spot before the fight begins.
            if (npc.Infernum().ExtraAI[HasTeleportedAboveTargetFlagIndex] == 0f)
            {
                npc.Center = target.Center - Vector2.UnitX * target.direction * 3200f;
				//npc.Center = target.Center - Vector2.UnitY * 2000f;
                // Bring segments to the teleport position.
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    if (Main.npc[i].active && (Main.npc[i].type == ModContent.NPCType<DevourerofGodsBody>() || Main.npc[i].type == ModContent.NPCType<DevourerofGodsTail>()))
                    {
                        Main.npc[i].Center = npc.Center;
                        Main.npc[i].netUpdate = true;
                    }
                }

                npc.Infernum().ExtraAI[HasTeleportedAboveTargetFlagIndex] = 1f;
                npc.netUpdate = true;
            }

            // Stay away from the target if the screen is being obstructed by the intro animation.
            if (IntroScreenManager.ScreenIsObstructed && universalFightTimer == 1f)
            {
                npc.dontTakeDamage = true;
                npc.Center = target.Center - Vector2.UnitX * target.direction * 3200f;
                npc.netUpdate = true;
            }

            npc.damage = npc.dontTakeDamage ? 0 : 885;

            // Spawn segments
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                if (npc.Infernum().ExtraAI[HasSpawnedSegmentsIndex] == 0f && npc.ai[0] == 0f)
                {
                    int previousSegment = npc.whoAmI;
                    for (int segmentSpawn = 0; segmentSpawn < 81; segmentSpawn++)
                    {
                        int segment;
                        if (segmentSpawn >= 0 && segmentSpawn < 80)
                            segment = NPC.NewNPC((int)npc.position.X + npc.width / 2, (int)npc.position.Y + npc.height / 2, InfernumMode.CalamityMod.NPCType("DevourerofGodsBody"), npc.whoAmI);
                        else
                            segment = NPC.NewNPC((int)npc.position.X + npc.width / 2, (int)npc.position.Y + npc.height / 2, InfernumMode.CalamityMod.NPCType("DevourerofGodsTail"), npc.whoAmI);

                        Main.npc[segment].realLife = npc.whoAmI;
                        Main.npc[segment].ai[2] = npc.whoAmI;
                        Main.npc[segment].ai[1] = previousSegment;
                        Main.npc[previousSegment].ai[0] = segment;
                        Main.npc[segment].Infernum().ExtraAI[SegmentNumberIndex] = 80f - segmentSpawn;
                        NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, segment, 0f, 0f, 0f, 0);
                        previousSegment = segment;
                    }
                    portalIndex = -1f;
                    npc.Infernum().ExtraAI[HasSpawnedSegmentsIndex] = 1f;
                }
            }

            // Chomping after attempting to eat the player.
            bool chomping = !npc.dontTakeDamage && DoGPhase2HeadBehaviorOverride.DoChomp(npc, ref chompEffectsCountdown, ref jawRotation);

            // Despawn if no valid target exists.
            if (target.dead || !target.active)
                DoGPhase2HeadBehaviorOverride.Despawn(npc);

            // Initially uncoil.
            else if (uncoilTimer < 45f)
            {
                uncoilTimer++;
                npc.velocity = Vector2.Lerp(npc.velocity, -Vector2.UnitY * 27f, 0.125f);
            }
            else if (phaseCycleTimer % (PassiveMovementTimeP1 + AggressiveMovementTimeP1) < AggressiveMovementTimeP1)
            {
                bool dontChompYet = phaseCycleTimer % (PassiveMovementTimeP1 + AggressiveMovementTimeP1) < 90f;
                if (phaseCycleTimer % (PassiveMovementTimeP1 + AggressiveMovementTimeP1) == 1f)
                    DoGSkyInfernum.CreateLightningBolt(new Color(1f, 0f, 0f, 0.2f), 16, true);

                DoGPhase2HeadBehaviorOverride.DoAggressiveFlyMovement(npc, target, dontChompYet, chomping, ref jawRotation, ref chompEffectsCountdown, ref universalFightTimer, ref flyAcceleration);
            }
            else
            {
                if (phaseCycleTimer % (PassiveMovementTimeP1 + AggressiveMovementTimeP1) == AggressiveMovementTimeP1 + 1f)
                    DoGSkyInfernum.CreateLightningBolt(Color.White, 16, true);

                DoGPhase2HeadBehaviorOverride.DoPassiveFlyMovement(npc, ref jawRotation, ref chompEffectsCountdown, false);

                // Idly release laserbeams.
                if (phaseCycleTimer % 150f == 0f && passiveAttackDelay >= 300f)
                {
                    Main.PlaySound(SoundID.Item12, target.position);
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        for (int i = 0; i < 16; i++)
                        {
                            Vector2 spawnOffset = (MathHelper.TwoPi * i / 16f).ToRotationVector2() * 1650f + Main.rand.NextVector2Circular(130f, 130f);
                            Vector2 laserShootVelocity = spawnOffset.SafeNormalize(Vector2.UnitY) * -Main.rand.NextFloat(20f, 24f) + Main.rand.NextVector2Circular(2f, 2f);
                            int laser = Utilities.NewProjectileBetter(target.Center + spawnOffset, laserShootVelocity, ModContent.ProjectileType<DoGDeathInfernum>(), 455, 0f);
                            if (Main.projectile.IndexInRange(laser))
                                Main.projectile[laser].MaxUpdates = 3;
                        }
                    }
                }
            }

            npc.rotation = npc.velocity.ToRotation() + MathHelper.PiOver2;
            return false;
        }

        public static void HandlePhase2TransitionEffect(NPC npc, ref float portalIndex)
        {
            npc.Calamity().CanHaveBossHealthBar = false;
            npc.velocity = npc.velocity.ClampMagnitude(32f, 60f);
            npc.velocity = npc.velocity.SafeNormalize(Vector2.UnitY) * MathHelper.Lerp(npc.velocity.Length(), 50f, 0.1f);
            npc.damage = 0;

            // Summon the portal and become fully opaque if the portal hasn't been created yet.
            if (CurrentPhase2TransitionState == Phase2TransitionState.NeedsToSummonPortal)
            {
                // Spawn the portal.
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    Vector2 spawnPosition = npc.Center + npc.velocity.SafeNormalize(Vector2.UnitX) * 2150f;
                    portalIndex = Projectile.NewProjectile(spawnPosition, Vector2.Zero, ModContent.ProjectileType<DoGChargeGate>(), 0, 0f);

                    Main.projectile[(int)portalIndex].localAI[0] = 1f;
                    Main.projectile[(int)portalIndex].localAI[1] = DoGPhase2IntroPortalGate.Phase2AnimationTime;
                }

                int headType = ModContent.NPCType<DoGHead>();
                int bodyType = ModContent.NPCType<DevourerofGodsBody>();
                int tailType = ModContent.NPCType<DevourerofGodsTail>();
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    if (Main.npc[i].active && (Main.npc[i].type == headType || Main.npc[i].type == bodyType || Main.npc[i].type == tailType))
                    {
                        Main.npc[i].Opacity = 1f;
                        Main.npc[i].netUpdate = true;
                    }
                }

                npc.Opacity = 1f;
                CurrentPhase2TransitionState = Phase2TransitionState.EnteringPortal;
            }

            // Enter the portal if it's being touched.
            if (Main.projectile[(int)portalIndex].Hitbox.Intersects(npc.Hitbox))
                npc.alpha = Utils.Clamp(npc.alpha + 140, 0, 255);

            // Vanish if the target died in the middle of the transition.
            if (Main.player[npc.target].dead || !Main.player[npc.target].active)
            {
                npc.TargetClosest();
                if (Main.player[npc.target].dead || !Main.player[npc.target].active)
                    npc.active = false;
            }
        }

        #endregion AI

        #region Drawing
        public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Color lightColor)
        {
            if (DoGPhase2HeadBehaviorOverride.InPhase2)
                return DoGPhase2HeadBehaviorOverride.PreDraw(npc, spriteBatch, lightColor);

            SpriteEffects spriteEffects = SpriteEffects.None;
            if (npc.spriteDirection == 1)
                spriteEffects = SpriteEffects.FlipHorizontally;

            float jawRotation = npc.Infernum().ExtraAI[JawRotationIndex];

            Texture2D headTexture = ModContent.GetTexture("InfernumMode/BehaviorOverrides/BossAIs/DoG/DoGP1Head");
            Vector2 drawPosition = npc.Center - Main.screenPosition;
            Vector2 headTextureOrigin = headTexture.Size() * 0.5f;

            Texture2D jawTexture = ModContent.GetTexture("InfernumMode/BehaviorOverrides/BossAIs/DoG/DoGP1Jaw");
            Vector2 jawOrigin = jawTexture.Size() * 0.5f;

            for (int i = -1; i <= 1; i += 2)
            {
                float jawBaseOffset = 20f;
                SpriteEffects jawSpriteEffect = spriteEffects;
                if (i == 1)
                {
                    jawSpriteEffect |= SpriteEffects.FlipHorizontally;
                    jawBaseOffset *= -1f;
                }
                Vector2 jawPosition = drawPosition;
                jawPosition += Vector2.UnitX.RotatedBy(npc.rotation + jawRotation * i) * (18f + i * (34f + jawBaseOffset + (float)Math.Sin(jawRotation) * 20f));
                jawPosition -= Vector2.UnitY.RotatedBy(npc.rotation) * (16f + (float)Math.Sin(jawRotation) * 20f);
                spriteBatch.Draw(jawTexture, jawPosition, null, lightColor, npc.rotation + jawRotation * i, jawOrigin, npc.scale, jawSpriteEffect, 0f);
            }

            Rectangle headFrame = headTexture.Frame();
            spriteBatch.Draw(headTexture, drawPosition, headFrame, npc.GetAlpha(lightColor), npc.rotation, headTextureOrigin, npc.scale, spriteEffects, 0f);

            Texture2D glowmaskTexture = ModContent.GetTexture("InfernumMode/BehaviorOverrides/BossAIs/DoG/DoGP1HeadGlow");
            spriteBatch.Draw(glowmaskTexture, drawPosition, headFrame, Color.White, npc.rotation, headTextureOrigin, npc.scale, spriteEffects, 0f);
            return false;
        }
        #endregion Drawing
    }
}
