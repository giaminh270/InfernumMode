using CalamityMod.NPCs.Calamitas;
using CalamityMod.NPCs.ProfanedGuardians;
using CalamityMod.NPCs.Ravager;
using CalamityMod.NPCs.SupremeCalamitas;
using CalamityMod.Tiles.FurnitureProfaned;
using CalamityMod.Tiles.LivingFire;
using CalamityMod.Walls;
using InfernumMode.Tiles;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using static InfernumMode.ILEditingStuff.HookManager;

namespace InfernumMode.ILEditingStuff
{
    public class DisableFargosBreakingProfanedTempleHook : IHookEdit
    {
        internal static bool DisableProfanedTempleBreakage(Func<Tile, bool> orig, Tile tile)
        {
            bool profanedTempleTile = tile.type == ModContent.TileType<RunicProfanedBrick>() || tile.type == ModContent.TileType<ProfanedSlab>() || tile.type == ModContent.TileType<ProfanedRock>();
            profanedTempleTile |= tile.type == ModContent.TileType<GuardiansSummoner>() || tile.type == ModContent.TileType<ProvidenceSummoner>() || tile.type == ModContent.TileType<ProvidenceRoomDoorPedestal>();
            profanedTempleTile |= tile.type == ModContent.TileType<ProfanedCandelabra>() || tile.wall == ModContent.WallType<RunicProfanedBrickWall>() || tile.wall == ModContent.WallType<ProfanedSlabWall>();
            profanedTempleTile |= tile.type == ModContent.TileType<LivingHolyFireBlockTile>() || tile.wall == ModContent.WallType<ProfanedCrystalWall>() || tile.wall == ModContent.WallType<ProfanedRockWall>();
            profanedTempleTile |= tile.type == ModContent.TileType<ProfanedCrystal>();
            if (profanedTempleTile)
                return false;

            return orig(tile);
        }
        
        internal static void DisableProfanedTempleBreakageIL(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);
            cursor.GotoNext(MoveType.After, c => c.MatchStloc(4));

            int afterXCoords = cursor.Index;
            ILLabel skipLoop = null;
            cursor.GotoNext(MoveType.After, c => c.MatchBlt(out skipLoop));

            cursor.Goto(afterXCoords);
            cursor.Emit(OpCodes.Ldloc, 3);
            cursor.Emit(OpCodes.Ldloc, 4);
            cursor.EmitDelegate<Func<int, int, bool>>((x, y) =>
            {
                return !PoDWorld.ProvidenceArena.Intersects(new Rectangle(x, y, 1, 1));
            });
            cursor.Emit(OpCodes.Brfalse, skipLoop);
        }

        public void Load()
        {
            if (InfernumMode.FargosMutantMod is null)
                return;

            FargosCanDestroyTile += DisableProfanedTempleBreakage;
            FargosCanDestroyTileWithInstabridge += DisableProfanedTempleBreakageIL;
            FargosCanDestroyTileWithInstabridge2 += DisableProfanedTempleBreakageIL;
        }

        public void Unload()
        {
            if (InfernumMode.FargosMutantMod is null)
                return;

            FargosCanDestroyTile -= DisableProfanedTempleBreakage;
            FargosCanDestroyTileWithInstabridge -= DisableProfanedTempleBreakageIL;
            FargosCanDestroyTileWithInstabridge2 -= DisableProfanedTempleBreakageIL;
        }
    }

    public class AddSepulcherToYABHBHook : IHookEdit
    {
        internal static void ReturnNullInstead(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);

            // Push a "null" bool? onto the stack and return it immediately, bypassing the original
            // "return false;" body entirely.
            cursor.EmitDelegate<Func<bool?>>(() => null);
            cursor.Emit(OpCodes.Ret);
        }

        public void Load()
        {
            if (InfernumMode.YABHBMod is null)
                return;

            SepulcherHeadDrawHealthBar += ReturnNullInstead;

            InfernumMode.YABHBMod.Call(
                "RegisterHealthBarMulti",
                ModContent.NPCType<SCalWormHead>(),
                ModContent.NPCType<SCalWormBody>(),
                ModContent.NPCType<SCalWormBodyWeak>(),
                ModContent.NPCType<SCalWormTail>()
            );
        }

        public void Unload()
        {
            if (InfernumMode.YABHBMod is null)
                return;

            SepulcherHeadDrawHealthBar -= ReturnNullInstead;
        }
    }
    public class AddMoreCalamityBossesToYABHBHook : IHookEdit
    {
        public void Load()
        {
            if (InfernumMode.YABHBMod is null)
                return;

            // Supreme Calamitas' two giant fists, Catastrophe and Cataclysm - always spawned together, so
            // one shared bar shows their combined remaining health.
            InfernumMode.YABHBMod.Call(
                "RegisterHealthBarMulti",
                ModContent.NPCType<SupremeCatastrophe>(),
                ModContent.NPCType<SupremeCataclysm>()
            );

            // Calamitas' Shadow's two clones, Catastrophe and Cataclysm - also always spawned together.
            InfernumMode.YABHBMod.Call(
                "RegisterHealthBarMulti",
                ModContent.NPCType<CalamitasRun2>(),
                ModContent.NPCType<CalamitasRun>()
            );

            // Ravager and all of its independently-alive-and-damageable parts, combined into one bar
            // showing its total remaining health. RavagerBody is listed first so the bar takes its name.
            InfernumMode.YABHBMod.Call(
                "RegisterHealthBarMulti",
                ModContent.NPCType<RavagerBody>(),
                ModContent.NPCType<RavagerHead>(),
                ModContent.NPCType<RavagerHead2>(),
                ModContent.NPCType<RavagerClawLeft>(),
                ModContent.NPCType<RavagerClawRight>(),
                ModContent.NPCType<RavagerLegLeft>(),
                ModContent.NPCType<RavagerLegRight>()
            );
            InfernumMode.YABHBMod.Call("RegisterHealthBar", ModContent.NPCType<ProfanedGuardianBoss2>());
            InfernumMode.YABHBMod.Call("RegisterHealthBar", ModContent.NPCType<ProfanedGuardianBoss3>());
        }

        public void Unload()
        {
            // YABHB has no "unregister" call, and its registration dictionary is keyed by NPC type (which
            // is only ever valid while both mods are loaded together anyway), so there's nothing to undo here.
        }
    }
}