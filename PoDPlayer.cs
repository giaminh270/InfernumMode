using CalamityMod;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.CalPlayer;
using CalamityMod.Events;
using CalamityMod.Items.Armor;
using CalamityMod.NPCs;
using CalamityMod.NPCs.OldDuke;
using CalamityMod.NPCs.Perforator;
using CalamityMod.NPCs.Polterghast;
using CalamityMod.World;
using InfernumMode.BehaviorOverrides.BossAIs.Draedon;
using InfernumMode.Buffs;
using InfernumMode.Dusts;
using InfernumMode.MachineLearning;
using InfernumMode;
using InfernumMode.Biomes;
using InfernumMode.Tiles;
using InfernumMode.Skies;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Terraria;

using Terraria.DataStructures;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace InfernumMode
{
    public class PoDPlayer : ModPlayer
    {
        public int MadnessTime;
        public bool RedElectrified = false;
        public bool ShadowflameInferno = false;
        public bool DarkFlames = false;
        public bool Madness = false;
        public float CurrentScreenShakePower;
        public float MusicMuffleFactor;
		public float ShimmerSoundVolumeInterpolant;

        public int ProvidenceRoomShatterTimer;
        public bool ProfanedTempleAnimationHasPlayed;
		private MLAttackSelector thanatosLaserTypeSelector = null;
        private MLAttackSelector aresSpecialAttackTypeSelector = null;
        private MLAttackSelector twinsSpecialAttackTypeSelector = null;
        public MLAttackSelector ThanatosLaserTypeSelector
        {
            get
            {
                if (thanatosLaserTypeSelector is null)
                    thanatosLaserTypeSelector = new MLAttackSelector(3, "ThanatosLaser");
                return thanatosLaserTypeSelector;
            }
            set => thanatosLaserTypeSelector = value;
        }
        public MLAttackSelector AresSpecialAttackTypeSelector
        {
            get
            {
                if (aresSpecialAttackTypeSelector is null)
                    aresSpecialAttackTypeSelector = new MLAttackSelector(2, "AresSpecialAttack");
                return aresSpecialAttackTypeSelector;
            }
            set => twinsSpecialAttackTypeSelector = value;
        }
        public MLAttackSelector TwinsSpecialAttackTypeSelector
        {
            get
            {
                if (twinsSpecialAttackTypeSelector is null)
                    twinsSpecialAttackTypeSelector = new MLAttackSelector(2, "TwinsSpecialAttack");
                return twinsSpecialAttackTypeSelector;
            }
            set => twinsSpecialAttackTypeSelector = value;
        }


        public bool CreateALotOfHolyCinders;



        public float MadnessInterpolant => MathHelper.Clamp(MadnessTime / 600f, 0f, 1f);

        public bool InProfanedArena
        {
            get
            {
                Rectangle arena = PoDWorld.ProvidenceArena;
                arena.X *= 16;
                arena.Y *= 16;
                arena.Width *= 16;
                arena.Height *= 16;
                return player.Hitbox.Intersects(arena);
            }
        }
        public bool InProfanedArenaAntiCheeseZone
        {
            get
            {
                Rectangle arena = PoDWorld.ProvidenceArena;
                arena.X *= 16;
                arena.Y *= 16;
                arena.Width *= 16;
                arena.Height *= 16;
                arena.Inflate(1080, 1080);

                return player.Hitbox.Intersects(arena);
            }
        }

        public Vector2 ScreenFocusPosition;
        public float ScreenFocusInterpolant = 0f;
        internal Point? CornerOne = null;
        internal Point? CornerTwo = null;
        public bool ProfanedLavaFountain
        {
            get;
            set;
        }

        // Property with a getter that dynamically assembles the corners to produce a meaningful Rectangle.
        internal Rectangle? SelectedProvidenceArena
        {
            get
            {
                if (!CornerOne.HasValue || !CornerTwo.HasValue)
                    return null;

                Point c1 = CornerOne.GetValueOrDefault();
                Point c2 = CornerTwo.GetValueOrDefault();

                // It is possible the player dragged the corners in any direction, so use Abs and Min to find the true upper left corner.
                int startingX = Math.Min(c1.X, c2.X);
                int width = Math.Abs(c1.X - c2.X);
                int startingY = Math.Min(c1.Y, c2.Y);
                int height = Math.Abs(c1.Y - c2.Y);
                return new Rectangle(startingX, startingY, width, height);
            }
        }
        public bool ZoneProfaned = false;
        #region Nurse Cheese Death
        public override bool ModifyNurseHeal(NPC nurse, ref int health, ref bool removeDebuffs, ref string chatText)
        {
            if (InfernumMode.CanUseCustomAIs && CalamityPlayer.areThereAnyDamnBosses)
            {
                chatText = "I cannot help you. Good luck.";
                return false;
            }
            return true;
        }
        #endregion Nurse Cheese Death
        #region Skies
        internal static readonly FieldInfo EffectsField = typeof(SkyManager).GetField("_effects", BindingFlags.NonPublic | BindingFlags.Instance);
        public override void UpdateBiomeVisuals()
        {
            if (!InfernumMode.CanUseCustomAIs)
                return;

            bool useFolly = NPC.AnyNPCs(InfernumMode.CalamityMod.NPCType("Bumblefuck")) && (Main.npc[NPC.FindFirstNPC(InfernumMode.CalamityMod.NPCType("Bumblefuck"))].Infernum().ExtraAI[8] > 0f);
            player.ManageSpecialBiomeVisuals("InfernumMode:Dragonfolly", useFolly);

            if (!BossRushEvent.BossRushActive)
            {
                int hiveMindID = InfernumMode.CalamityMod.NPCType("HiveMind");
                int hiveMind = NPC.FindFirstNPC(hiveMindID);
                NPC hiveMindNPC = hiveMind >= 0 ? Main.npc[hiveMind] : null;
                bool useHIV = hiveMindNPC != null && (hiveMindNPC.Infernum().ExtraAI[10] == 1f || hiveMindNPC.life < hiveMindNPC.lifeMax * 0.2f);
                player.ManageSpecialBiomeVisuals("InfernumMode:HiveMind", useHIV);

                bool useDeus = NPC.AnyNPCs(InfernumMode.CalamityMod.NPCType("AstrumDeusHeadSpectral"));
                player.ManageSpecialBiomeVisuals("InfernumMode:Deus", useDeus);

                int oldDukeID = ModContent.NPCType<OldDuke>();
                int oldDuke = NPC.FindFirstNPC(oldDukeID);
                NPC oldDukeNPC = oldDuke >= 0 ? Main.npc[oldDuke] : null;
                bool useOD = oldDukeNPC != null && oldDukeNPC.Infernum().ExtraAI[6] >= 2f;
                player.ManageSpecialBiomeVisuals("InfernumMode:OldDuke", useOD);

                int perforatorHiveID = ModContent.NPCType<PerforatorHive>();
                int perforatorHive = NPC.FindFirstNPC(perforatorHiveID);
                NPC perforatorHiveNPC = perforatorHive >= 0 ? Main.npc[perforatorHive] : null;
                player.ManageSpecialBiomeVisuals("InfernumMode:Perforators", perforatorHiveNPC != null && perforatorHiveNPC.localAI[1] > 0f);

                bool useDoG = DoGSkyInfernum.CanSkyBeActive;
                if (useDoG)
                    SkyManager.Instance.Activate("InfernumMode:DoG", player.Center);
                else
                    SkyManager.Instance.Deactivate("InfernumMode:DoG");
            }
        }
        #endregion
        #region Reset Effects
        public override void ResetEffects()
        {
            RedElectrified = false;
            ShadowflameInferno = false;
            DarkFlames = false;
            ScreenFocusInterpolant = 0f;
            MusicMuffleFactor = 0f;

            // Disable block placement and destruction in the profaned arena.
            if (InProfanedArenaAntiCheeseZone)
            {
                player.AddBuff(BuffID.NoBuilding, 10);
                player.noBuilding = true;
            }
        }
        #endregion
        #region Update Dead
        public override void UpdateDead()
        {
            RedElectrified = false;
            ShadowflameInferno = false;
            DarkFlames = false;
            Madness = false;
            MadnessTime = 0;

            if (PoDWorld.InfernumMode)
                player.respawnTimer = Utils.Clamp(player.respawnTimer - 1, 0, 3600);
        }
        #endregion
        #region Update
        public override void PreUpdate()
        {
            ProfanedLavaFountain = false;
            int profanedFountainID = ModContent.TileType<ProfanedFountainTile>();
            for (int dx = -75; dx < 75; dx++)
            {
                for (int dy = -75; dy < 75; dy++)
                {
                    int x = (int)(player.Center.X / 16f + dx);
                    int y = (int)(player.Center.Y / 16f + dy);
                    if (!WorldGen.InWorld(x, y))
                        continue;

                    if (Main.tile[x, y].active() && Main.tile[x, y].type == profanedFountainID && Main.tile[x, y].frameX < 36)
                    {
                        ProfanedLavaFountain = true;
                        goto LeaveLoop;
                    }
                }
            }
            LeaveLoop:

            if (Main.netMode == NetmodeID.Server)
                return;

            /* Handle shimmer sound looping when near the Providence door.
            if (SoundEngine.TryGetActiveSound(ShimmerSoundID, out var sound))
            {
                float idealVolume = Main.soundVolume * ShimmerSoundVolumeInterpolant;
                if (sound.Sound.Volume != idealVolume)
                    sound.Sound.Volume = idealVolume;

                if (PoDWorld.HasProvidenceDoorShattered || ShimmerSoundVolumeInterpolant <= 0f)
                    sound.Stop();
                if (ShimmerSoundVolumeInterpolant > 0f)
                    sound.Resume();
            }
            else
            {
                if (ShimmerSoundVolumeInterpolant > 0f && !PoDWorld.HasProvidenceDoorShattered)
                    ShimmerSoundID = SoundEngine.PlaySound(InfernumSoundRegistry.ProvidenceDoorShimmerSoundLoop with { Volume = 0.0001f });
            }
            ShimmerSoundVolumeInterpolant = MathHelper.Clamp(ShimmerSoundVolumeInterpolant - 0.02f, 0f, 1f);*/
        }

        public override void PostUpdate()
        {
            // Keep the player out of the providence arena if the door is around.
            if (PoDWorld.ProvidenceDoorXPosition != 0 && !PoDWorld.HasProvidenceDoorShattered && player.Bottom.Y >= (Main.maxTilesY - 220f) * 16f)
            {
                bool passedDoor = false;
                float doorX = PoDWorld.ProvidenceDoorXPosition;
                while (player.Right.X >= doorX || (passedDoor && Collision.SolidCollision(player.TopLeft, player.width, player.height)))
                {
                    player.velocity.X = 0f;
                    player.position.X -= 0.1f;
                    passedDoor = true;
                }
            }

            if (Main.myPlayer != player.whoAmI || !ZoneProfaned || !player.ZoneUnderworldHeight)
                return;

            bool createALotOfHolyCinders = CreateALotOfHolyCinders;
            float cinderSpawnInterpolant = CalamityPlayer.areThereAnyDamnBosses ? 0.9f : 0.1f;
            int cinderSpawnRate = (int)MathHelper.Lerp(6f, 2f, cinderSpawnInterpolant);
            float cinderFlySpeed = MathHelper.Lerp(6f, 12f, cinderSpawnInterpolant);
            if (createALotOfHolyCinders)
            {
                cinderSpawnRate = 1;
                cinderFlySpeed = 13.25f;
                CreateALotOfHolyCinders = false;
            }

            for (int i = 0; i < 3; i++)
            {
                if (!Main.rand.NextBool(cinderSpawnRate) || Main.gfxQuality < 0.35f)
                    continue;

                Vector2 cinderSpawnOffset = new Vector2(Main.rand.NextFloatDirection() * 1550f, 650f);
                Vector2 cinderVelocity = -Vector2.UnitY.RotatedBy(Main.rand.NextFloat(0.23f, 0.98f)) * Main.rand.NextFloat(0.6f, 1.2f) * cinderFlySpeed;
                if (Main.rand.NextBool())
                {
                    cinderSpawnOffset = cinderSpawnOffset.RotatedBy(-MathHelper.PiOver2) * new Vector2(0.9f, 1f);
                    cinderVelocity = cinderVelocity.RotatedBy(-MathHelper.PiOver2) * new Vector2(1.8f, -1f);
                }

                if (Main.rand.NextBool(createALotOfHolyCinders ? 2 : 6))
                    cinderVelocity.X *= -1f;

                Utilities.NewProjectileBetter(player.Center + cinderSpawnOffset, cinderVelocity, ModContent.ProjectileType<ProfanedTempleCinder>(), 0, 0f);
            }
        }
        #endregion Update


        #region Pre Hurt
		public override bool PreHurt(bool pvp, bool quiet, ref int damage, ref int hitDirection, ref bool crit, ref bool customDamage, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource)
        {
            if (InfernumMode.CanUseCustomAIs && CalamityGlobalNPC.adultEidolonWyrmHead >= 0 && Main.npc[CalamityGlobalNPC.adultEidolonWyrmHead].Calamity().CurrentlyEnraged)
                damage = (int)MathHelper.Max(5500f / (1f - player.endurance + 1e-6f), damage);
            return true;
        }
        #endregion Pre Hurt
        #region Pre Kill
        public override bool PreKill(double damage, int hitDirection, bool pvp, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource)
        {
            if (damage == 10.0 && hitDirection == 0 && damageSource.SourceOtherIndex == 8)
            {
                if (RedElectrified)
                    damageSource = PlayerDeathReason.ByCustomReason($"{player.name} could not withstand the red lightning.");
                if (DarkFlames)
                    damageSource = PlayerDeathReason.ByCustomReason($"{player.name} was incinerated by ungodly fire.");
                if (Madness)
                    damageSource = PlayerDeathReason.ByCustomReason($"{player.name} went mad.");
            }
            return base.PreKill(damage, hitDirection, pvp, ref playSound, ref genGore, ref damageSource);
        }
        #endregion
        #region Kill
        public override void Kill(double damage, int hitDirection, bool pvp, PlayerDeathReason damageSource)
        {
            ExoMechManagement.RecordAttackDeath(player);
        }
        #endregion Kill
        #region Life Regen
        public override void UpdateLifeRegen()
        {
            void causeLifeRegenLoss(int regenLoss)
            {
                if (player.lifeRegen > 0)
                    player.lifeRegen = 0;
                player.lifeRegenTime = 0;
                player.lifeRegen -= regenLoss;
            }
            if (RedElectrified)
                causeLifeRegenLoss(player.controlLeft || player.controlRight ? 64 : 16);

            if (ShadowflameInferno)
                causeLifeRegenLoss(23);
            if (DarkFlames)
            {
                causeLifeRegenLoss(30);
                player.statDefense -= 8;
            }
            if (Madness)
                causeLifeRegenLoss(NPC.AnyNPCs(ModContent.NPCType<Polterghast>()) ? 800 : 50);
            MadnessTime = Utils.Clamp(MadnessTime + (Madness ? 1 : -8), 0, 660);			
        }
        #endregion
        #region Drawing

        public static readonly PlayerLayer RedLightningEffect = new PlayerLayer("CalamityMod", "MiscEffectsBack", PlayerLayer.MiscEffectsBack, drawInfo =>
        {
            if (drawInfo.shadow != 0f || !drawInfo.drawPlayer.Infernum().RedElectrified)
                return;

            Texture2D texture2D2 = Main.glowMaskTexture[25];
            int frame = drawInfo.drawPlayer.miscCounter / 5;
            for (int l = 0; l < 2; l++)
            {
                frame %= 7;
                Player player = drawInfo.drawPlayer;
                SpriteEffects spriteEffects = drawInfo.drawPlayer.direction == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
                if (frame > 1 && frame < 5)
                {
                    Color lightningColor = Color.Red;
                    lightningColor.A = 0;

                    Rectangle frameRectangle = new Rectangle(0, frame * texture2D2.Height / 7, texture2D2.Width, texture2D2.Height / 7);
                    Vector2 fuck = new Vector2((int)(drawInfo.position.X - Main.screenPosition.X - (player.bodyFrame.Width / 2) + (player.width / 2)), (int)(drawInfo.position.Y - Main.screenPosition.Y + player.height - player.bodyFrame.Height + 4f)) + player.bodyPosition + player.bodyFrame.Size() * 0.5f;
                    DrawData lightningEffect = new DrawData(texture2D2, fuck, frameRectangle, lightningColor, player.bodyRotation, frameRectangle.Size() * 0.5f, 1f, spriteEffects, 0);
                    Main.playerDrawData.Add(lightningEffect);
                }
                frame += 3;
            }
        });

        public override void ModifyDrawLayers(List<PlayerLayer> layers)
        {
            layers.Add(RedLightningEffect);
        }

        #endregion
        #region Screen Shaking
        public override void ModifyScreenPosition()
        {
            if (ScreenFocusInterpolant > 0f && InfernumConfig.Instance.BossIntroductionAnimationsAreAllowed)
            {
                Vector2 idealScreenPosition = ScreenFocusPosition - new Vector2(Main.screenWidth, Main.screenHeight) * 0.5f;
                Main.screenPosition = Vector2.Lerp(Main.screenPosition, idealScreenPosition, ScreenFocusInterpolant);
            }

            if (CurrentScreenShakePower > 0f)
                CurrentScreenShakePower = Utils.Clamp(CurrentScreenShakePower - 0.2f, 0f, 15f);
            else
                return;

            if (CalamityConfig.Instance.DisableScreenShakes)
                return;

            Main.screenPosition += Main.rand.NextVector2CircularEdge(CurrentScreenShakePower, CurrentScreenShakePower);
        }
        #endregion
        #region Saving and Loading
        public override TagCompound Save()
        {
            TagCompound tag = new TagCompound();
            tag["ProfanedTempleAnimationHasPlayed"] = ProfanedTempleAnimationHasPlayed;
			ThanatosLaserTypeSelector?.Save(tag);
            AresSpecialAttackTypeSelector?.Save(tag);
            TwinsSpecialAttackTypeSelector?.Save(tag);
			return tag;
        }

        public override void Load(TagCompound tag)
        {
			ThanatosLaserTypeSelector = MLAttackSelector.Load(tag, "ThanatosLaser");
            AresSpecialAttackTypeSelector = MLAttackSelector.Load(tag, "AresSpecialAttack");
            TwinsSpecialAttackTypeSelector = MLAttackSelector.Load(tag, "TwinsSpecialAttack");
            ProfanedTempleAnimationHasPlayed = tag.GetBool("ProfanedTempleAnimationHasPlayed");
        }
        #endregion Saving and Loading
        #region Misc Effects
        public override void PostUpdateMiscEffects()
        {
            if (player.mount.Active && player.mount.Type == Mount.Slime && NPC.AnyNPCs(InfernumMode.CalamityMod.NPCType("DesertScourgeHead")) && InfernumMode.CanUseCustomAIs)
            {
                player.mount.Dismount(player);
            }

            // Ensure that Revengeance Mode is always active while Infernum is active.
            if (PoDWorld.InfernumMode && !CalamityWorld.revenge)
                CalamityWorld.revenge = true;

            // Ensure that Malice Mode is never active while Infernum is active.
            if (PoDWorld.InfernumMode && CalamityWorld.malice)
            {
                CalamityUtils.DisplayLocalizedText("Mods.CalamityMod.MaliceText2", Color.Crimson);
                CalamityWorld.malice = false;
            }

            if (PoDWorld.InfernumMode && CalamityWorld.DoGSecondStageCountdown > 600)
            {
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    if (Main.npc[i].active &&
                        (Main.npc[i].type == InfernumMode.CalamityMod.NPCType("Signus") ||
                        (Main.npc[i].type == InfernumMode.CalamityMod.NPCType("StormWeaverHead")) ||
                        (Main.npc[i].type == InfernumMode.CalamityMod.NPCType("StormWeaverBody")) ||
                        (Main.npc[i].type == InfernumMode.CalamityMod.NPCType("StormWeaverTail")) ||
                        (Main.npc[i].type == InfernumMode.CalamityMod.NPCType("StormWeaverNakedHead")) ||
                        (Main.npc[i].type == InfernumMode.CalamityMod.NPCType("StormWeaverNakedBody")) ||
                        (Main.npc[i].type == InfernumMode.CalamityMod.NPCType("StormWeaverNakedTail")) ||
                        (Main.npc[i].type == InfernumMode.CalamityMod.NPCType("CeaselessVoid")) ||
                        (Main.npc[i].type == InfernumMode.CalamityMod.NPCType("DarkEnergy")) ||
                        (Main.npc[i].type == InfernumMode.CalamityMod.NPCType("DarkEnergy2")) ||
                        (Main.npc[i].type == InfernumMode.CalamityMod.NPCType("DarkEnergy3"))))
                    {
                        Main.npc[i].active = false;
                    }
                }
                CalamityWorld.DoGSecondStageCountdown = 599;
            }

            if (ShadowflameInferno)
            {
                for (int i = 0; i < 2; i++)
                {
                    Dust shadowflame = Dust.NewDustDirect(player.position, player.width, player.height, 28);
                    shadowflame.velocity = player.velocity.SafeNormalize(Vector2.UnitX * player.direction);
                    shadowflame.velocity = shadowflame.velocity.RotatedByRandom(0.4f) * -Main.rand.NextFloat(2.5f, 5.4f);
                    shadowflame.scale = Main.rand.NextFloat(0.95f, 1.3f);
                    shadowflame.noGravity = true;
                }
            }

            if (DarkFlames)
            {
                for (int i = 0; i < 3; i++)
                {
                    Dust shadowflame = Dust.NewDustDirect(player.position, player.width, player.height, ModContent.DustType<RavagerMagicDust>());
                    shadowflame.velocity = player.velocity.SafeNormalize(Vector2.UnitX * player.direction);
                    shadowflame.velocity = shadowflame.velocity.RotatedByRandom(0.4f) * -Main.rand.NextFloat(2.5f, 5.4f);
                    shadowflame.velocity += Main.rand.NextVector2Circular(3f, 3f);
                    shadowflame.scale = Main.rand.NextFloat(0.95f, 1.25f);
                    shadowflame.noGravity = true;
                }
            }
        }
        #endregion
        #region Fuck
        public override void UpdateEquips(ref bool wallSpeedBuff, ref bool tileSpeedBuff, ref bool tileRangeBuff)
        {
            if (player.armor[0].type == ModContent.ItemType<AuricTeslaCuisses>())
                player.moveSpeed -= 0.1f;
            if (player.armor[0].type == ModContent.ItemType<AuricTeslaPlumedHelm>())
                player.moveSpeed -= 0.15f;
        }
        #endregion
    }
}