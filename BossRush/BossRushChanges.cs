using CalamityMod;
using CalamityMod.Buffs.StatDebuffs;
using CalamityMod.Events;
using CalamityMod.NPCs.AquaticScourge;
using CalamityMod.NPCs.AstrumAureus;
using CalamityMod.NPCs.AstrumDeus;
using CalamityMod.NPCs.BrimstoneElemental;
using CalamityMod.NPCs.Bumblebirb;
using CalamityMod.NPCs.Calamitas;
using CalamityMod.NPCs.CeaselessVoid;
using CalamityMod.NPCs.Crabulon;
using CalamityMod.NPCs.Cryogen;
using CalamityMod.NPCs.DesertScourge;
using CalamityMod.NPCs.DevourerofGods;
using CalamityMod.NPCs.ExoMechs.Apollo;
using CalamityMod.NPCs.ExoMechs.Ares;
using CalamityMod.NPCs.ExoMechs.Artemis;
using CalamityMod.NPCs.ExoMechs.Thanatos;
using CalamityMod.NPCs.HiveMind;
using CalamityMod.NPCs.Leviathan;
using CalamityMod.NPCs.NormalNPCs;
using CalamityMod.NPCs.OldDuke;
using CalamityMod.NPCs.Perforator;
using CalamityMod.NPCs.PlaguebringerGoliath;
using CalamityMod.NPCs.Polterghast;
using CalamityMod.NPCs.ProfanedGuardians;
using CalamityMod.NPCs.Providence;
using CalamityMod.NPCs.Ravager;
using CalamityMod.NPCs.Signus;
using CalamityMod.NPCs.SlimeGod;
using CalamityMod.NPCs.StormWeaver;
using CalamityMod.NPCs.SupremeCalamitas;
using CalamityMod.NPCs.TownNPCs;
using CalamityMod.NPCs.Yharon;
using CalamityMod.Projectiles.Typeless;
using CalamityMod.World;
using InfernumMode.BehaviorOverrides.BossAIs.AquaticScourge;
using InfernumMode.BehaviorOverrides.BossAIs.AstrumDeus;
using InfernumMode.BehaviorOverrides.BossAIs.BoC;
using InfernumMode.BehaviorOverrides.BossAIs.CalamitasShadow;
using InfernumMode.BehaviorOverrides.BossAIs.Draedon;
using InfernumMode.BehaviorOverrides.BossAIs.Draedon.Ares;
using InfernumMode.BehaviorOverrides.BossAIs.DukeFishron;
using InfernumMode.BehaviorOverrides.BossAIs.EyeOfCthulhu;
using InfernumMode.BehaviorOverrides.BossAIs.KingSlime;
using InfernumMode.BehaviorOverrides.BossAIs.PlaguebringerGoliath;
using InfernumMode.BehaviorOverrides.BossAIs.Polterghast;
using InfernumMode.BehaviorOverrides.BossAIs.ProfanedGuardians;
using InfernumMode.BehaviorOverrides.BossAIs.Signus;
using InfernumMode.BehaviorOverrides.BossAIs.SupremeCalamitas;
using InfernumMode.BehaviorOverrides.BossAIs.Twins;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.World.Generation;
using static CalamityMod.Events.BossRushEvent;

namespace InfernumMode.BossRush
{
    public static class BossRushChanges
    {
        public static void Load()
        {
            Bosses = new List<Boss>()
            {
                new Boss(NPCID.KingSlime, permittedNPCs: new int[] { ModContent.NPCType<Ninja>(), ModContent.NPCType<KingSlimeJewel>() }),

                new Boss(NPCID.EyeofCthulhu, TimeChangeContext.Night, permittedNPCs: new int[] { NPCID.ServantofCthulhu, ModContent.NPCType<ExplodingServant>() }),

                new Boss(NPCID.EaterofWorldsHead, permittedNPCs: new int[] { NPCID.EaterofWorldsBody, NPCID.EaterofWorldsTail, NPCID.VileSpit }),

                new Boss(NPCID.WallofFlesh, spawnContext: type =>
                {
                    Player player = Main.player[ClosestPlayerToWorldCenter];
                    NPC.SpawnWOF(player.position);
                }, permittedNPCs: new int[] { NPCID.WallofFleshEye, NPCID.LeechHead, NPCID.LeechBody, NPCID.LeechTail, NPCID.TheHungry, NPCID.TheHungryII }),

                new Boss(ModContent.NPCType<PerforatorHive>(), permittedNPCs: new int[] { ModContent.NPCType<PerforatorHeadLarge>(), ModContent.NPCType<PerforatorBodyLarge>(), ModContent.NPCType<PerforatorTailLarge>(),
                    ModContent.NPCType<PerforatorHeadMedium>(), ModContent.NPCType<PerforatorBodyMedium>(), ModContent.NPCType<PerforatorTailMedium>(), ModContent.NPCType<PerforatorHeadSmall>(),
                    ModContent.NPCType<PerforatorBodySmall>() ,ModContent.NPCType<PerforatorTailSmall>() }),

                new Boss(NPCID.QueenBee, permittedNPCs: new int[] { NPCID.HornetFatty, NPCID.HornetHoney, NPCID.HornetStingy }),


                new Boss(ModContent.NPCType<AstrumAureus>(), TimeChangeContext.Night, permittedNPCs: ModContent.NPCType<AureusSpawn>()),

                new Boss(ModContent.NPCType<CrabulonIdle>(), spawnContext: type =>
                {
                    Player player = Main.player[ClosestPlayerToWorldCenter];
                    int thePefectOne = NPC.NewNPC((int)(player.position.X + Main.rand.Next(-100, 101)), (int)(player.position.Y - 400f), type, 1);
                    Main.npc[thePefectOne].timeLeft *= 20;
                    CalamityUtils.BossAwakenMessage(thePefectOne);
                }, specialSpawnCountdown: 300, permittedNPCs: new int[] { ModContent.NPCType<CrabShroom>() }),

                new Boss(ModContent.NPCType<AquaticScourgeHead>(), permittedNPCs: new int[] { ModContent.NPCType<AquaticScourgeBody>(), ModContent.NPCType<AquaticScourgeBodyAlt>(),
                    ModContent.NPCType<AquaticScourgeTail>(), ModContent.NPCType<AquaticParasite2>(), ModContent.NPCType<AquaticSeekerHead2>(),
                    ModContent.NPCType<AquaticSeekerBody2>(), ModContent.NPCType<AquaticSeekerTail2>() }),

                new Boss(ModContent.NPCType<DesertScourgeHead>(), spawnContext: type =>
                {
                    NPC.SpawnOnPlayer(ClosestPlayerToWorldCenter, ModContent.NPCType<DesertScourgeHead>());
                }, permittedNPCs: new int[] { ModContent.NPCType<DesertScourgeBody>(), ModContent.NPCType<DesertScourgeTail>() }),

                new Boss(ModContent.NPCType<ProfanedGuardianBoss>(), TimeChangeContext.Day,
                    permittedNPCs: new int[] { ModContent.NPCType<ProfanedGuardianBoss2>(), ModContent.NPCType<ProfanedGuardianBoss3>(), ModContent.NPCType<EtherealHand>() }),
                
                // Tier 2.
                new Boss(ModContent.NPCType<CeaselessVoid>(), permittedNPCs: ModContent.NPCType<DarkEnergy>()),

                new Boss(ModContent.NPCType<StormWeaverHead>(), TimeChangeContext.Day, permittedNPCs: new int[] { ModContent.NPCType<StormWeaverBody>(), ModContent.NPCType<StormWeaverTail>(), }),

                new Boss(ModContent.NPCType<BrimstoneElemental>(), permittedNPCs: ModContent.NPCType<Brimling>()),

                new Boss(ModContent.NPCType<Siren>(), TimeChangeContext.Day, permittedNPCs: new int[] { ModContent.NPCType<Leviathan>(),
                    ModContent.NPCType<SirenIce>(), NPCID.DetonatingBubble, ModContent.NPCType<RedirectingBubble>() }),

                new Boss(ModContent.NPCType<RavagerBody>(), spawnContext: type =>
                {
                    Player player = Main.player[ClosestPlayerToWorldCenter];

                    Main.PlaySound(SoundID.Roar, player.position, 2);
                    int ravager = NPC.NewNPC((int)(player.position.X + Main.rand.Next(-100, 101)), (int)(player.position.Y - 400f), type, 1);
                    Main.npc[ravager].timeLeft *= 20;
                    CalamityUtils.BossAwakenMessage(ravager);
                }, usesSpecialSound: true, permittedNPCs: new int[] { ModContent.NPCType<FlamePillar>(), ModContent.NPCType<RockPillar>(), ModContent.NPCType<RavagerLegLeft>(), ModContent.NPCType<RavagerLegRight>(),
                    ModContent.NPCType<RavagerClawLeft>(), ModContent.NPCType<RavagerClawRight>() }),

                new Boss(ModContent.NPCType<HiveMind>(), spawnContext: type =>
                {
                    NPC.SpawnOnPlayer(ClosestPlayerToWorldCenter, type);
                }, permittedNPCs: new int[] { ModContent.NPCType<DankCreeper>(), ModContent.NPCType<DarkHeart>(), ModContent.NPCType<HiveBlob>(), ModContent.NPCType<HiveBlob2>() }),

                new Boss(NPCID.DukeFishron, spawnContext: type =>
                {
                    Player player = Main.player[ClosestPlayerToWorldCenter];
                    int dukeFishron = NPC.NewNPC((int)(player.position.X + Main.rand.Next(-100, 101)), (int)(player.position.Y - 400f), type, 1);
                    Main.npc[dukeFishron].timeLeft *= 20;
                    CalamityUtils.BossAwakenMessage(dukeFishron);
                }, permittedNPCs: new int[] { NPCID.DetonatingBubble, NPCID.Sharkron, NPCID.Sharkron2, ModContent.NPCType<RedirectingBubble>() }),

                new Boss(ModContent.NPCType<Cryogen>()),

                new Boss(NPCID.BrainofCthulhu, permittedNPCs: new int[] { NPCID.Creeper, ModContent.NPCType<BrainIllusion>() }),


                new Boss(ModContent.NPCType<Signus>(), specialSpawnCountdown: 360, permittedNPCs: new int[] { ModContent.NPCType<UnworldlyEntity>() }),

                new Boss(ModContent.NPCType<Bumblefuck>(), TimeChangeContext.Day, permittedNPCs: new int[] { ModContent.NPCType<Bumblefuck2>(), NPCID.Spazmatism, NPCID.Retinazer }),

                new Boss(ModContent.NPCType<SlimeGodCore>(), permittedNPCs: new int[] { ModContent.NPCType<SlimeGodCore>(), ModContent.NPCType<SlimeGod>(), ModContent.NPCType<SlimeGodRun>(), ModContent.NPCType<SlimeGodSplit>(),
                    ModContent.NPCType<SlimeGodRunSplit>() }),
                
                // Tier 3.
                new Boss(NPCID.SkeletronHead, TimeChangeContext.Night, type =>
                {
                    Player player = Main.player[ClosestPlayerToWorldCenter];
                    int sans = NPC.NewNPC((int)(player.position.X + Main.rand.Next(-100, 101)), (int)(player.position.Y - 400f), type, 1);
                    Main.npc[sans].timeLeft *= 20;
                    CalamityUtils.BossAwakenMessage(sans);
                }, permittedNPCs: NPCID.SkeletronHand),

                new Boss(NPCID.Plantera, permittedNPCs: new int[] { NPCID.PlanterasTentacle, NPCID.PlanterasHook, NPCID.Spore }),

                new Boss(NPCID.TheDestroyer, TimeChangeContext.Night, specialSpawnCountdown: 300, permittedNPCs: new int[] { NPCID.TheDestroyerBody, NPCID.TheDestroyerTail, NPCID.Probe }),

                new Boss(ModContent.NPCType<PlaguebringerGoliath>(), permittedNPCs: new int[] { ModContent.NPCType<BuilderDroneSmall>(), ModContent.NPCType<BuilderDroneBig>(), ModContent.NPCType<SmallDrone>(),
                    ModContent.NPCType<PlagueMine>(), ModContent.NPCType<ExplosivePlagueCharger>() }),

                new Boss(ModContent.NPCType<AstrumDeusHeadSpectral>(), TimeChangeContext.Night, type =>
                {
                    Player player = Main.player[ClosestPlayerToWorldCenter];

                    Main.PlaySound(InfernumMode.CalamityMod.GetLegacySoundSlot(SoundType.Custom, "Sounds/Custom/AstrumDeusSpawn"), player.Center);
                    NPC.SpawnOnPlayer(ClosestPlayerToWorldCenter, type);
                }, usesSpecialSound: true, permittedNPCs: new int[] { ModContent.NPCType<AstrumDeusBodySpectral>(), ModContent.NPCType<AstrumDeusTailSpectral>(), ModContent.NPCType<DeusSpawn>() }),

                // Filler.
                new Boss(NPCID.CultistBoss),

                new Boss(NPCID.CultistBoss, spawnContext: type =>
                {
                    Player player = Main.player[ClosestPlayerToWorldCenter];
                    int doctorLooneyTunes = NPC.NewNPC((int)player.Center.X, (int)player.Center.Y - 400, type, 1);
                    Main.npc[doctorLooneyTunes].direction = Main.npc[doctorLooneyTunes].spriteDirection = Math.Sign(player.Center.X - player.Center.X - 90f);
                    Main.npc[doctorLooneyTunes].timeLeft *= 20;
                    CalamityUtils.BossAwakenMessage(doctorLooneyTunes);
                }, permittedNPCs: new int[] { NPCID.CultistBossClone, NPCID.CultistDragonHead, NPCID.CultistDragonBody1, NPCID.CultistDragonBody2, NPCID.CultistDragonBody3, NPCID.CultistDragonBody4,
                    NPCID.CultistDragonTail, NPCID.AncientCultistSquidhead, NPCID.AncientLight, NPCID.AncientDoom }),

                new Boss(NPCID.SkeletronPrime, TimeChangeContext.Night, permittedNPCs: new int[] { NPCID.PrimeCannon, NPCID.PrimeSaw, NPCID.PrimeVice, NPCID.PrimeLaser, NPCID.Probe }),

                new Boss(ModContent.NPCType<OldDuke>(), spawnContext: type =>
                {
                    Player player = Main.player[ClosestPlayerToWorldCenter];
                    int boomerDuke = NPC.NewNPC((int)(player.position.X + Main.rand.Next(-100, 101)), (int)(player.position.Y - 400f), type, 1);
                    Main.npc[boomerDuke].timeLeft *= 20;
                    CalamityUtils.BossAwakenMessage(boomerDuke);
                }, permittedNPCs: new int[] { ModContent.NPCType<OldDukeToothBall>(), ModContent.NPCType<OldDukeSharkron>() }),

                new Boss(NPCID.Golem, TimeChangeContext.Day, type =>
                {
                    int sans = NPC.NewNPC((int)(Main.player[ClosestPlayerToWorldCenter].position.X + Main.rand.Next(-100, 101)), (int)(Main.player[ClosestPlayerToWorldCenter].position.Y - 400f), type, 1);
                    Main.npc[sans].timeLeft *= 20;
                    CalamityUtils.BossAwakenMessage(sans);
                }, permittedNPCs: new int[] { NPCID.GolemFistLeft, NPCID.GolemFistRight, NPCID.GolemHead, NPCID.GolemHeadFree }),
                
                // Tier 4.

                new Boss(NPCID.Spazmatism, TimeChangeContext.Night, type =>
                {
                    NPC.SpawnOnPlayer(ClosestPlayerToWorldCenter, NPCID.Spazmatism);
                    NPC.SpawnOnPlayer(ClosestPlayerToWorldCenter, NPCID.Retinazer);
                }, permittedNPCs: new int[] { NPCID.Retinazer, ModContent.NPCType<EnergyOrb>(), ModContent.NPCType<CursedOrb>() }),

                new Boss(ModContent.NPCType<Polterghast>(), TimeChangeContext.Day, permittedNPCs: new int[]
                    { ModContent.NPCType<PhantomFuckYou>(), ModContent.NPCType<PolterghastHook>(), ModContent.NPCType<PolterPhantom>(), ModContent.NPCType<PolterghastLeg>() }),

                new Boss(NPCID.MoonLordCore, spawnContext: type =>
                {
                    NPC.SpawnOnPlayer(ClosestPlayerToWorldCenter, type);
                }, permittedNPCs: new int[] { NPCID.MoonLordLeechBlob, NPCID.MoonLordHand, NPCID.MoonLordHead, NPCID.MoonLordFreeEye }),

                new Boss(ModContent.NPCType<CalamitasRun>(), TimeChangeContext.Night, specialSpawnCountdown: 420, dimnessFactor: 0.6f, permittedNPCs: new int[] { ModContent.NPCType<CalamitasRun2>(), ModContent.NPCType<CalamitasRun3>(),
                         ModContent.NPCType<SoulSeeker>() }),
                
                // Tier 5.
                new Boss(ModContent.NPCType<DevourerofGodsHead>(), TimeChangeContext.Day, type =>
                {
                    CalamityUtils.DisplayLocalizedText("Mods.CalamityMod.BossRushTierFourEndText2", XerocTextColor);
                    Player player = Main.player[ClosestPlayerToWorldCenter];
                    for (int playerIndex = 0; playerIndex < Main.maxPlayers; playerIndex++)
                    {
                        if (Main.player[playerIndex].active)
                        {
                            Player player2 = Main.player[playerIndex];
                            if (player2.FindBuffIndex(ModContent.BuffType<ExtremeGrav>()) > -1)
                                player2.ClearBuff(ModContent.BuffType<ExtremeGrav>());
                        }
                    }

                    Main.PlaySound(InfernumMode.CalamityMod.GetLegacySoundSlot(SoundType.Custom, "Sounds/Custom/DevourerSpawn"), player.Center);
                    NPC.SpawnOnPlayer(ClosestPlayerToWorldCenter, type);
                }, usesSpecialSound: true, permittedNPCs: new int[] { ModContent.NPCType<DevourerofGodsBody>(), ModContent.NPCType<DevourerofGodsTail>() }),

                new Boss(ModContent.NPCType<Yharon>(), TimeChangeContext.Day),

                new Boss(ModContent.NPCType<Providence>(), TimeChangeContext.Day, type =>
                {
                    Player player = Main.player[ClosestPlayerToWorldCenter];

                    int prov = NPC.NewNPC((int)(player.position.X + Main.rand.Next(-500, 501)), (int)(player.position.Y - 250f), type, 1);
                    Main.npc[prov].timeLeft *= 20;
                    CalamityUtils.BossAwakenMessage(prov);
                }, usesSpecialSound: true, permittedNPCs: new int[] { ModContent.NPCType<ProvSpawnOffense>(), ModContent.NPCType<ProvSpawnHealer>(), ModContent.NPCType<ProvSpawnDefense>() }),

                new Boss(ModContent.NPCType<Apollo>(), spawnContext: type =>
                {
                    Player player = Main.player[ClosestPlayerToWorldCenter];
                    int apollo = NPC.NewNPC((int)player.Center.X, (int)player.Center.Y - 2400, type, 1);
                    Main.npc[apollo].Infernum().ExtraAI[ExoMechManagement.SecondaryMechNPCTypeIndex] = ModContent.NPCType<ThanatosHead>();

                }, permittedNPCs: new int[] { ModContent.NPCType<Artemis>(), ModContent.NPCType<Apollo>(), ModContent.NPCType<AresBody>(),
                    ModContent.NPCType<AresLaserCannon>(), ModContent.NPCType<AresTeslaCannon>(), ModContent.NPCType<AresPlasmaFlamethrower>(), ModContent.NPCType<AresGaussNuke>(), ModContent.NPCType<AresPulseCannon>(),
                    ModContent.NPCType<ThanatosHead>(), ModContent.NPCType<ThanatosBody1>(), ModContent.NPCType<ThanatosBody2>(), ModContent.NPCType<ThanatosTail>() }),

                new Boss(ModContent.NPCType<SupremeCalamitas>(), spawnContext: type =>
                {
                    Main.PlaySound(InfernumMode.CalamityMod.GetLegacySoundSlot(SoundType.Custom, "Sounds/Custom/SupremeCalamitasSpawn"), Main.player[ClosestPlayerToWorldCenter].Center);
                    CalamityUtils.SpawnBossBetter(Main.player[ClosestPlayerToWorldCenter].Top - new Vector2(42f, 84f), type);
                }, dimnessFactor: 0.5f, permittedNPCs: new int[] { ModContent.NPCType<SCalWormArm>(), ModContent.NPCType<SCalWormHead>(), ModContent.NPCType<SCalWormBody>(),
                    ModContent.NPCType<SCalWormBodyWeak>(), ModContent.NPCType<SCalWormTail>(),
                    ModContent.NPCType<SoulSeekerSupreme>(), ModContent.NPCType<BrimstoneHeart>(), ModContent.NPCType<SupremeCataclysm>(),
                    ModContent.NPCType<SupremeCatastrophe>(), ModContent.NPCType<ShadowDemon>() }),
            };

            BossDeathEffects = new Dictionary<int, Action<NPC>>()
            {
                [NPCID.WallofFlesh] = npc =>
                {
                    BringPlayersBackToSpawn();
                },
                [ModContent.NPCType<ProfanedGuardianBoss>()] = npc =>
                {
                    CalamityUtils.DisplayLocalizedText("Mods.CalamityMod.BossRushTierOneEndText", XerocTextColor);
                    BringPlayersBackToSpawn();
                },
                [ModContent.NPCType<SlimeGodCore>()] = npc =>
                {
                    CalamityUtils.DisplayLocalizedText("Mods.CalamityMod.BossRushTierTwoEndText", XerocTextColor);
                },
                [NPCID.Golem] = npc =>
                {
                    CalamityUtils.DisplayLocalizedText("Mods.CalamityMod.BossRushTierThreeEndText", XerocTextColor);
                },
                [ModContent.NPCType<CalamitasRun3>()] = npc =>
                {
                    CalamityUtils.DisplayLocalizedText("Mods.CalamityMod.BossRushTierFourEndText", XerocTextColor);
                },
                [ModContent.NPCType<Providence>()] = npc =>
                {
                    BringPlayersBackToSpawn();
                },
                [ModContent.NPCType<SupremeCalamitas>()] = npc =>
                {
                    CalamityUtils.KillAllHostileProjectiles();
                    CalamityWorld.bossRushHostileProjKillCounter = 3;

                    if (Main.netMode != NetmodeID.MultiplayerClient)
                        Projectile.NewProjectile(npc.Center, Vector2.Zero, ModContent.ProjectileType<BossRushEndEffectThing>(), 0, 0f, Main.myPlayer);
                }
            };

            BossIDsAfterDeath[ModContent.NPCType<Apollo>()] = new int[]
            {
                ModContent.NPCType<Apollo>(),
                ModContent.NPCType<Artemis>(),
                ModContent.NPCType<AresBody>(),
                ModContent.NPCType<AresLaserCannon>(),
                ModContent.NPCType<AresPlasmaFlamethrower>(),
                ModContent.NPCType<AresTeslaCannon>(),
                ModContent.NPCType<AresGaussNuke>(),
                ModContent.NPCType<AresPulseCannon>(),
                ModContent.NPCType<ThanatosHead>(),
                ModContent.NPCType<ThanatosBody1>(),
                ModContent.NPCType<ThanatosBody2>(),
                ModContent.NPCType<ThanatosTail>(),
            };
        }

        internal static void BringPlayersBackToSpawn()
        {
            // Post-Wall of Flesh teleport back to spawn.
            for (int playerIndex = 0; playerIndex < Main.maxPlayers; playerIndex++)
            {
                bool appropriatePlayer = Main.myPlayer == playerIndex;
                if (Main.player[playerIndex].active && appropriatePlayer)
                {
                    Main.player[playerIndex].Spawn();
                }
            }
        }

        public static void HandleTeleports()
        {
            Vector2 teleportPosition = Vector2.Zero;

            // Teleport the player to the garden for the guardians fight in boss rush.
            if (BossRushStage < Bosses.Count - 1 && !CalamityUtils.AnyBossNPCS() && !Main.LocalPlayer.ZoneUnderworldHeight)
            {
                if (CurrentlyFoughtBoss == NPCID.WallofFlesh)
                    teleportPosition = new Vector2(Main.maxTilesX - 1f, 3200f);
                if (CurrentlyFoughtBoss == ModContent.NPCType<ProfanedGuardianBoss>())
                    teleportPosition = PoDWorld.ProvidenceArena.TopLeft() * 16f + new Vector2(PoDWorld.ProvidenceArena.Width * 3.2f - 16f, 800f);
                if (CurrentlyFoughtBoss == ModContent.NPCType<Providence>())
                    teleportPosition = PoDWorld.ProvidenceArena.TopRight() * 16f + new Vector2(PoDWorld.ProvidenceArena.Width * -3.2f - 16f, 800f);
            }

            // Teleport the player.
            if (teleportPosition != Vector2.Zero)
            {
                if (CurrentlyFoughtBoss != NPCID.SkeletronHead && WorldUtils.Find(teleportPosition.ToTileCoordinates(), Searches.Chain(new Searches.Down(100), new Conditions.IsSolid()), out Point p))
                    teleportPosition = p.ToWorldCoordinates(8f, -32f);

                Main.LocalPlayer.Teleport(teleportPosition);
            }
        }
    }
}
