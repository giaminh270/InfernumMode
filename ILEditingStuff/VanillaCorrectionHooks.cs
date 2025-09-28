using CalamityMod;
using CalamityMod.Balancing;
using CalamityMod.Events;
using CalamityMod.NPCs;
using CalamityMod.NPCs.AstrumAureus;
using CalamityMod.NPCs.DesertScourge;
using CalamityMod.NPCs.ExoMechs;
using CalamityMod.NPCs.ProfanedGuardians;
using CalamityMod.NPCs.SlimeGod;
using CalamityMod.Schematics;
using CalamityMod.World;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using static CalamityMod.Events.BossRushEvent;
using static InfernumMode.ILEditingStuff.HookManager;
using InfernumBalancingManager = InfernumMode.Balancing.BalancingChangesManager;

namespace InfernumMode.ILEditingStuff
{
    public class ReplaceGoresHook : IHookEdit
    {
        internal static int AlterGores(On.Terraria.Gore.orig_NewGore orig, Vector2 Position, Vector2 Velocity, int Type, float Scale)
        {
            if (InfernumMode.CanUseCustomAIs && Type >= GoreID.Cultist1 && Type <= GoreID.CultistBoss2)
                return Main.maxDust;

            if (InfernumMode.CanUseCustomAIs && Type == 573)
                Type = InfernumMode.Instance.GetGoreSlot("Gores/DukeFishronGore1");
            if (InfernumMode.CanUseCustomAIs && Type == 574)
                Type = InfernumMode.Instance.GetGoreSlot("Gores/DukeFishronGore3");
            if (InfernumMode.CanUseCustomAIs && Type == 575)
                Type = InfernumMode.Instance.GetGoreSlot("Gores/DukeFishronGore2");
            if (InfernumMode.CanUseCustomAIs && Type == 576)
                Type = InfernumMode.Instance.GetGoreSlot("Gores/DukeFishronGore4");

            return orig(Position, Velocity, Type, Scale);
        }

        public void Load() => On.Terraria.Gore.NewGore += AlterGores;

        public void Unload() => On.Terraria.Gore.NewGore -= AlterGores;
    }

    public class MoveDraedonHellLabHook : IHookEdit
    {
        internal static void SlideOverHellLab(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);
            cursor.EmitDelegate<Action>(() =>
            {
                int tries = 0;
                string mapKey = "Hell Laboratory";
                SchematicMetaTile[,] schematic = CalamitySchematicIO.LoadSchematic("Schematics/HellLaboratory.csch");

                do
                {
                    int underworldTop = Main.maxTilesY - 200;
                    int placementPositionX = WorldGen.genRand.Next((int)(Main.maxTilesX * 0.7), (int)(Main.maxTilesX * 0.82));
                    int placementPositionY = WorldGen.genRand.Next(Main.maxTilesY - 150, Main.maxTilesY - 125);

                    Point placementPoint = new Point(placementPositionX, placementPositionY);
                    Vector2 schematicSize = new Vector2(schematic.GetLength(0), schematic.GetLength(1));
                    int xCheckArea = 30;
                    bool canGenerateInLocation = true;

                    // new Vector2 is used here since a lambda expression cannot capture a ref, out, or in parameter.
                    float totalTiles = (schematicSize.X + xCheckArea * 2) * schematicSize.Y;
                    for (int x = placementPoint.X - xCheckArea; x < placementPoint.X + schematicSize.X + xCheckArea; x++)
                    {
                        for (int y = placementPoint.Y; y < placementPoint.Y + schematicSize.Y; y++)
                        {
                            Tile tile = CalamityUtils.ParanoidTileRetrieval(x, y);
                            if (DraedonStructures.ShouldAvoidLocation(new Point(x, y), false))
                                canGenerateInLocation = false;
                        }
                    }
                    if (!canGenerateInLocation)
                    {
                        tries++;
                    }
                    else
                    {
                        bool hasPlacedMurasama = false;
                        SchematicManager.PlaceSchematic(mapKey, new Point(placementPoint.X, placementPoint.Y), SchematicAnchor.TopLeft, ref hasPlacedMurasama, new Action<Chest, int, bool>(DraedonStructures.FillHellLaboratoryChest));
                        CalamityWorld.HellLabCenter = placementPoint.ToWorldCoordinates() + new Vector2(schematic.GetLength(0), schematic.GetLength(1)) * 8f;
                        break;
                    }
                }
                while (tries <= 50000);
            });
            cursor.Emit(OpCodes.Ret);
        }

        public void Load() => PlaceHellLab += SlideOverHellLab;

        public void Unload() => PlaceHellLab -= SlideOverHellLab;
    }

    public class GetRidOfOnHitDebuffsHook : IHookEdit
    {
        public void Load()
        {
            YharonOnHitPlayer += SepulcherOnHitProjectileEffectRemovalHook.EarlyReturn;
            SCalOnHitPlayer += SepulcherOnHitProjectileEffectRemovalHook.EarlyReturn;
        }

        public void Unload()
        {
            YharonOnHitPlayer -= SepulcherOnHitProjectileEffectRemovalHook.EarlyReturn;
            SCalOnHitPlayer -= SepulcherOnHitProjectileEffectRemovalHook.EarlyReturn;
        }
    }

    public class ChangeBossRushTiersHooks : IHookEdit
    {
		internal void AdjustBossRushTiers(ILContext il)
		{
			ILCursor cursor = new ILCursor(il);

			if (cursor.TryGotoNext(MoveType.After, i => i.MatchLdcI4(1)))
			{
				cursor.Emit(OpCodes.Pop);
				cursor.EmitDelegate<Func<int>>(() =>
				{
					int tier2Boss = NPCID.TheDestroyer;
					int tier3Boss = NPCID.CultistBoss;
					if (InfernumMode.CanUseCustomAIs)
					{
						tier2Boss = ModContent.NPCType<ProfanedGuardianBoss>();
						tier3Boss = ModContent.NPCType<SlimeGodCore>();
					}

					if (BossRushStage > Bosses.FindIndex(boss => boss.EntityID == tier3Boss))
						return 3;
					if (BossRushStage > Bosses.FindIndex(boss => boss.EntityID == tier2Boss))
						return 2;
					return 1;
				});
			}
		}

        public void Load() => BossRushTier += AdjustBossRushTiers;

        public void Unload() => BossRushTier -= AdjustBossRushTiers;
    }
	
    public class DisableExoMechsSkyInBRHook : IHookEdit
    {
        internal void DisableSky(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);

            cursor.EmitDelegate<Func<bool>>(() =>
            {
                int draedon = CalamityGlobalNPC.draedon;
                if (draedon == -1 || !Main.npc[draedon].active)
                    return Draedon.ExoMechIsPresent && !BossRushActive;

                if ((Main.npc[draedon]?.ModNPC<Draedon>()?.DefeatTimer ?? 0) <= 0 && !Draedon.ExoMechIsPresent)
                    return false;

                return !BossRushActive;
            });
            cursor.Emit(OpCodes.Ret);
        }

        public void Load() => ExoMechsSkyIsActive += DisableSky;

        public void Unload() => ExoMechsSkyIsActive -= DisableSky;
    }
	
    public class GetRidOfProvidenceLootBoxHook : IHookEdit
    {
        public void Load() => SpawnProvLootBox += SepulcherOnHitProjectileEffectRemovalHook.EarlyReturn;

        public void Unload() => SpawnProvLootBox -= SepulcherOnHitProjectileEffectRemovalHook.EarlyReturn;
    }

    public class AureusPlatformWalkingHook : IHookEdit
    {
        internal static bool LetAureusWalkOnPlatforms(On.Terraria.NPC.orig_Collision_DecideFallThroughPlatforms orig, NPC npc)
        {
            if (npc.type == ModContent.NPCType<AstrumAureus>())
            {
                if (Main.player[npc.target].position.Y > npc.Bottom.Y)
                    return true;
                return false;
            }
            return orig(npc);
        }

        public void Load() => On.Terraria.NPC.Collision_DecideFallThroughPlatforms += LetAureusWalkOnPlatforms;

        public void Unload() => On.Terraria.NPC.Collision_DecideFallThroughPlatforms -= LetAureusWalkOnPlatforms;
    }

    public class FishronSkyDistanceLeniancyHook : IHookEdit
    {
        internal static void AdjustFishronScreenDistanceRequirement(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);
            cursor.GotoNext(i => i.MatchLdcR4(3000f));
            cursor.Remove();
            cursor.Emit(OpCodes.Ldc_R4, 6000f);
        }

        public void Load() => IL.Terraria.GameContent.Events.ScreenDarkness.Update += AdjustFishronScreenDistanceRequirement;

        public void Unload() => IL.Terraria.GameContent.Events.ScreenDarkness.Update -= AdjustFishronScreenDistanceRequirement;
    }

    public class LessenDesertTileRequirementsHook : IHookEdit
    {
        internal static void MakeDesertRequirementsMoreLenient(On.Terraria.Player.orig_UpdateBiomes orig, Player self)
        {
            orig(self);
            self.ZoneDesert = Main.sandTiles > 300;
        }

        public void Load() => On.Terraria.Player.UpdateBiomes += MakeDesertRequirementsMoreLenient;

        public void Unload() => On.Terraria.Player.UpdateBiomes -= MakeDesertRequirementsMoreLenient;
    }

    public class SepulcherOnHitProjectileEffectRemovalHook : IHookEdit
    {
        internal static void EarlyReturn(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);
            cursor.Emit(OpCodes.Ret);
        }

        public void Load()
        {
            SepulcherHeadModifyProjectile += EarlyReturn;
            SepulcherBodyModifyProjectile += EarlyReturn;
            SepulcherBody2ModifyProjectile += EarlyReturn;
            SepulcherTailModifyProjectile += EarlyReturn;
        }

        public void Unload()
        {
            SepulcherHeadModifyProjectile -= EarlyReturn;
            SepulcherBodyModifyProjectile -= EarlyReturn;
            SepulcherBody2ModifyProjectile -= EarlyReturn;
            SepulcherTailModifyProjectile -= EarlyReturn;
        }
    }

    public class GetRidOfDesertNuisancesHook : IHookEdit
    {
        internal static void GetRidOfDesertNuisances(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);
            cursor.Emit(OpCodes.Ldarg_1);
            cursor.EmitDelegate<Action<Player>>(player =>
            {
                int scourgeID = ModContent.NPCType<DesertScourgeHead>();
                if (NPC.AnyNPCs(scourgeID))
                    return;

                Main.PlaySound(SoundID.Roar, player.Center, 0);
                if (Main.netMode != NetmodeID.MultiplayerClient)
                    NPC.SpawnOnPlayer(player.whoAmI, scourgeID);
                else
                    NetMessage.SendData(MessageID.SpawnBoss, -1, -1, null, player.whoAmI, scourgeID);

                // Summon nuisances if not in Infernum mode.
                if (CalamityWorld.revenge && !InfernumMode.CanUseCustomAIs)
                {
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                        NPC.SpawnOnPlayer(player.whoAmI, ModContent.NPCType<DesertNuisanceHead>());
                    else
                        NetMessage.SendData(MessageID.SpawnBoss, -1, -1, null, player.whoAmI, ModContent.NPCType<DesertNuisanceHead>());

                    if (Main.netMode != NetmodeID.MultiplayerClient)
                        NPC.SpawnOnPlayer(player.whoAmI, ModContent.NPCType<DesertNuisanceHead>());
                    else
                        NetMessage.SendData(MessageID.SpawnBoss, -1, -1, null, player.whoAmI, ModContent.NPCType<DesertNuisanceHead>());
                }
            });
            cursor.Emit(OpCodes.Ldc_I4_1);
            cursor.Emit(OpCodes.Ret);
        }

        public void Load() => DesertScourgeItemUseItem += GetRidOfDesertNuisances;

        public void Unload() => DesertScourgeItemUseItem -= GetRidOfDesertNuisances;
    }

    public class LetAresHitPlayersHook : IHookEdit
    {
        internal static void LetAresHitPlayer(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);
            cursor.Emit(OpCodes.Ldc_I4_1);
            cursor.Emit(OpCodes.Ret);
        }

        public void Load() => AresBodyCanHitPlayer += LetAresHitPlayer;

        public void Unload() => AresBodyCanHitPlayer -= LetAresHitPlayer;
    }
}