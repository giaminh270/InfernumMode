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
}