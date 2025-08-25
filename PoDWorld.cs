using CalamityMod.NPCs;
using CalamityMod.NPCs.ExoMechs;
using CalamityMod.Tiles.FurnitureProfaned;
using InfernumMode.Tiles;
using Microsoft.Xna.Framework;
using CalamityMod.Walls;
using System.Collections.Generic;
using System;
using System.IO;
using Terraria;
using Terraria.GameContent.Generation;
using Terraria.ID;
using CalamityMod.Schematics;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.World.Generation;
using static CalamityMod.Schematics.SchematicManager;
using ModInstance = InfernumMode.InfernumMode;

namespace InfernumMode
{
    public class PoDWorld : ModWorld
    {
		public static int ProfanedTile
        {
            get;
            set;
        } = 0;
		
        public static bool HasGeneratedProfanedShrine
        {
            get;
            set;
        } = false;

        public static bool HasBeatedInfernumProvRegularly
        {
            get;
            set;
        }

        public static bool HasBeatedInfernumNightProvBeforeDay
        {
            get;
            set;
        }

        public static bool InfernumMode = false;

        public static Rectangle ProvidenceArena
        {
            get;
            set;
        } = Rectangle.Empty;

        public static int ProvidenceDoorXPosition
        {
            get;
            set;
        }

        public static bool HasSepulcherAnimationBeenPlayed
        {
            get;
            set;
        }

        public static bool HasProvidenceDoorShattered
        {
            get;
            set;
        } = false;

        public override void Initialize()
        {
            InfernumMode = false;
        }
		
        #region Save
        public override TagCompound Save()
        {
            var downed = new List<string>();
            if (InfernumMode)
                downed.Add("fuckYouMode");
            if (HasGeneratedProfanedShrine)
                downed.Add("HasGeneratedProfanedShrine");
            if (HasBeatedInfernumProvRegularly)
                downed.Add("HasBeatedInfernumProvRegularly");
            if (HasBeatedInfernumNightProvBeforeDay)
                downed.Add("HasBeatedInfernumNightProvBeforeDay");
            if (HasProvidenceDoorShattered)
                downed.Add("HasProvidenceDoorShattered");
            if (HasSepulcherAnimationBeenPlayed)
                downed.Add("HasSepulcherAnimationBeenPlayed");

            TagCompound tag = new TagCompound();
            tag["downed"] = downed;
            tag["ProvidenceArenaX"] = ProvidenceArena.X;
            tag["ProvidenceArenaY"] = ProvidenceArena.Y;
            tag["ProvidenceArenaWidth"] = ProvidenceArena.Width;
            tag["ProvidenceArenaHeight"] = ProvidenceArena.Height;
            tag["ProvidenceDoorXPosition"] = ProvidenceDoorXPosition;
			return tag;
        }
        #endregion

        #region Load
        public override void Load(TagCompound tag)
        {
            var downed = tag.GetList<string>("downed");
            InfernumMode = downed.Contains("fuckYouMode");
			HasGeneratedProfanedShrine = downed.Contains("HasGeneratedProfanedShrine");
            HasBeatedInfernumProvRegularly = downed.Contains("HasBeatedInfernumProvRegularly");
			HasBeatedInfernumNightProvBeforeDay = downed.Contains("HasBeatedInfernumNightProvBeforeDay");
			HasProvidenceDoorShattered = downed.Contains("HasProvidenceDoorShattered");
            ProvidenceArena = new Rectangle(tag.GetInt("ProvidenceArenaX"), tag.GetInt("ProvidenceArenaY"), tag.GetInt("ProvidenceArenaWidth"), tag.GetInt("ProvidenceArenaHeight"));
            ProvidenceDoorXPosition = tag.GetInt("ProvidenceDoorXPosition");
        }
        #endregion

        #region LoadLegacy
        public override void LoadLegacy(BinaryReader reader)
        {
            int loadVersion = reader.ReadInt32();
            if (loadVersion == 0)
            {
                BitsByte flags = reader.ReadByte();
                InfernumMode = flags[0];
            }
        }
        #endregion

        #region NetSend
        public override void NetSend(BinaryWriter writer)
        {
            BitsByte flags = new BitsByte();
            flags[0] = InfernumMode;
            flags[1] = HasBeatedInfernumNightProvBeforeDay;
            flags[2] = HasBeatedInfernumProvRegularly;
            flags[3] = HasProvidenceDoorShattered;
            writer.Write(flags);
        }
        #endregion

        #region NetReceive
        public override void NetReceive(BinaryReader reader)
        {
            BitsByte flags = reader.ReadByte();
            InfernumMode = flags[0];
            HasBeatedInfernumNightProvBeforeDay = flags[1];
            HasBeatedInfernumProvRegularly = flags[2];
			HasProvidenceDoorShattered = flags[3];
        }
        #endregion

        #region Updating
        public override void PostUpdate()
        {
            // Disable natural GSS spawns.
            if (ModInstance.CanUseCustomAIs)
                CalamityMod.CalamityMod.sharkKillCount = 0;

            if (!NPC.AnyNPCs(ModContent.NPCType<Draedon>()))
                CalamityGlobalNPC.draedon = -1;
        }
        #endregion Updating

        #region Worldgen
        public override void ModifyWorldGenTasks(List<GenPass> tasks, ref float totalWeight)
        {;
            int finalCleanupIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Final Cleanup"));
            if (finalCleanupIndex != -1)
            {
                tasks.Insert(++finalCleanupIndex, new PassLegacy("Prov Arena", progress =>
                {
                    progress.Message = "Constructing a temple for an ancient goddess";
                    //GenerateProfanedArena(progress);
					GenerateProfanedShrine(progress);
                }));
            }
        }

        /*public static void GenerateUndergroundDesertArea(GenerationProgress progress)
        {
			Vector2 cutoutAreaCenter = WorldGen.UndergroundDesertLocation.Center.ToVector2();
            cutoutAreaCenter.Y -= 100f;

            for (int i = 0; i < 4; i++)
            {
                cutoutAreaCenter += WorldGen.genRand.NextVector2Circular(15f, 15f);
                WorldUtils.Gen(cutoutAreaCenter.ToPoint(), new Shapes.Mound(75, 48), Actions.Chain(
                    new Modifiers.Blotches(12),
                    new Actions.ClearTile(),
                    new Actions.PlaceWall(WallID.Sandstone)
                    ));
            }
        }

        public static void GenerateUndergroundJungleArea(GenerationProgress progress)
        {
            for (int j = 0; j < 5000; j++)
            {
                int x = WorldGen.genRand.Next(Main.maxTilesX / 10, Main.maxTilesX * 9 / 10);
                int y = WorldGen.genRand.Next((int)Main.rockLayer, Main.maxTilesY - 740);

                if (Main.tile[x, y].active() && Main.tile[x, y].type == TileID.JungleGrass && Main.tile[x, y].wall != WallID.LihzahrdBrick)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        x += WorldGen.genRand.Next(-15, 15);
                        y += WorldGen.genRand.Next(-15, 15);
                        WorldUtils.Gen(new Point(x, y), new Shapes.Circle(70), Actions.Chain(
                            new Modifiers.Blotches(12),
                            new Modifiers.SkipTiles(TileID.LihzahrdBrick),
                            new Actions.ClearTile(),
                            new Actions.PlaceWall(WallID.MudUnsafe)
                            ));
                    }
                    break;
                }
            }
        }

        public static void GenerateDungeonArea(GenerationProgress progress)
        {
            int boxArea = 95;
            byte dungeonWallID = 0;
            ushort dungeonTileID = 0;
            
            // Dungeon type detection for 1.3
            if (WorldGen.dungeonX < Main.maxTilesX / 2)
            {
                dungeonWallID = (byte)WallID.BlueDungeonSlabUnsafe;
                dungeonTileID = TileID.BlueDungeonBrick;
            }
            else
            {
                if (WorldGen.dungeonY > Main.maxTilesY / 2)
                {
                    dungeonWallID = (byte)WallID.GreenDungeonSlabUnsafe;
                    dungeonTileID = TileID.GreenDungeonBrick;
                }
                else
                {
                    dungeonWallID = (byte)WallID.PinkDungeonSlabUnsafe;
                    dungeonTileID = TileID.PinkDungeonBrick;
                }
            }
            
            Point dungeonCenter = new Point(WorldGen.dungeonX, WorldGen.dungeonY);
            WorldUtils.Gen(dungeonCenter, new Shapes.Rectangle(boxArea, boxArea), Actions.Chain(
                new Actions.SetTile(dungeonTileID, true)));
            WorldUtils.Gen(new Point(dungeonCenter.X + 2, dungeonCenter.Y + 2), new Shapes.Rectangle(boxArea - 2, boxArea - 2), Actions.Chain(
                new Actions.ClearTile(),
                new Actions.PlaceWall(dungeonWallID)));
        }*/

        public static void GenerateProfanedArena(GenerationProgress progress)
        {
            bool _3 = false;
            Point bottomLeftOfWorld = new Point(Main.maxTilesX - 30, Main.maxTilesY - 42);
			SchematicMetaTile[,] schematic = CalamitySchematicIO.LoadSchematic("Schematics/ProfanedArena.csch");
            int width = schematic.GetLength(0);
            int height = schematic.GetLength(1);

			PlaceSchematic<Action<Chest>>("Profaned Arena", bottomLeftOfWorld, SchematicAnchor.BottomRight, ref _3);
            ProvidenceArena = new Rectangle(bottomLeftOfWorld.X - width, bottomLeftOfWorld.Y - height, width, height);
            HasGeneratedProfanedShrine = true;
        }
		
		public static void GenerateProfanedShrine(GenerationProgress progress)
        {
            int width = 250;
            int height = 125;
            ushort slabID = (ushort)ModContent.TileType<ProfanedSlab>();
            ushort runeID = (ushort)ModContent.TileType<RunicProfanedBrick>();
            ushort provSummonerID = (ushort)ModContent.TileType<ProvidenceSummoner>();

            int left = Main.maxTilesX - width - Main.offLimitBorderTiles;
            int top = Main.maxTilesY - 180;
            int bottom = top + height;
            int centerX = left + width / 2;

            // Define the arena area.
            PoDWorld.ProvidenceArena = new Rectangle(left, top, width, height);

            // Clear out the entire area where the shrine will be made.
            for (int x = left; x < left + width; x++)
            {
                for (int y = top; y < bottom; y++)
                {
                    Main.tile[x, y].liquid = 0;
                    Main.tile[x, y].active(false);
                }
            }

            // Create the floor and ceiling.
            for (int x = left; x < left + width; x++)
            {
                int y = bottom - 1;
                while (!Main.tile[x, y].active())
                {
                    Main.tile[x, y].liquid = 0;
                    Main.tile[x, y].active(true);
                    Main.tile[x, y].type = WorldGen.genRand.NextBool(5) ? runeID : slabID;
                    Main.tile[x, y].slope(0);
                    Main.tile[x, y].halfBrick(false);

                    y++;

                    if (y >= Main.maxTilesY)
                        break;
                }

                y = top + 1;
                while (!Main.tile[x, y].active())
                {
                    Main.tile[x, y].liquid = 0;
                    Main.tile[x, y].active(true);
                    Main.tile[x, y].type = WorldGen.genRand.NextBool(5) ? runeID : slabID;
                    Main.tile[x, y].slope(0);
                    Main.tile[x, y].halfBrick(false);

                    y--;

                    if (y < top - 40)
                        break;
                }
            }

            // Create the right wall.
            for (int y = top; y < bottom + 2; y++)
            {
                int x = left + width - 1;
                Main.tile[x, y].liquid = 0;
                Main.tile[x, y].active(true);
                Main.tile[x, y].type = WorldGen.genRand.NextBool(5) ? runeID : slabID;
                Main.tile[x, y].slope(0);
                Main.tile[x, y].halfBrick(false);
            }

            // Find the vertical point at which stairs should be placed.
            int stairLeft = left - 1;
            int stairTop = bottom - 1;
            while (Main.tile[stairLeft, stairTop].liquid > 0 || Main.tile[stairLeft, stairTop].active())
                stairTop--;

            // Create stairs until a bottom is reached.
            int stairWidth = bottom - stairTop;
            for (int x = stairLeft - 3; x < stairLeft + stairWidth; x++)
            {
                int stairHeight = stairWidth - (x - stairLeft);
                if (x < stairLeft)
                    stairHeight = stairWidth;

                for (int y = -stairHeight; y < 0; y++)
                {
                    Main.tile[x, y + bottom].liquid = 0;
                    Main.tile[x, y + bottom].type = WorldGen.genRand.NextBool(5) ? runeID : slabID;
                    Main.tile[x, y + bottom].active(true);
                    Main.tile[x, y + bottom].slope(0);
                    Main.tile[x, y + bottom].halfBrick(false);
                    WorldGen.TileFrame(x, y + bottom);
                }
            }

            // Settle liquids.
            Liquid.QuickWater(3);
            WorldGen.WaterCheck();

            Liquid.quickSettle = true;
            for (int i = 0; i < 10; i++)
            {
                while (Liquid.numLiquid > 0)
                    Liquid.UpdateLiquid();
                WorldGen.WaterCheck();
            }
            Liquid.quickSettle = false;

            // Clear out any liquids.
            for (int x = left - 20; x < Main.maxTilesX; x++)
            {
                for (int y = top - 15; y < bottom + 8; y++)
                    Main.tile[x, y].liquid = 0;
            }

            // Create the Providence altar.
            short frameX = 0;
            short frameY;
            for (int x = centerX; x < centerX + ProvidenceSummoner.Width; x++)
            {
                frameY = 0;
                for (int y = bottom - 3; y < bottom + ProvidenceSummoner.Height - 3; y++)
                {
                    Main.tile[x, y].liquid = 0;
                    Main.tile[x, y].type = provSummonerID;
                    Main.tile[x, y].frameX = frameX;
                    Main.tile[x, y].frameY = frameY;
                    Main.tile[x, y].active(true);
                    Main.tile[x, y].slope(0);
                    Main.tile[x, y].halfBrick(false);

                    frameY += 18;
                }
                frameX += 18;
            }
        }

        /*public static void GenerateProfanedShrinePillar(Point bottom, int topY)
        {
            ushort runicBrickWallID = (ushort)ModContent.WallType<RunicProfanedBrickWall>();
            ushort profanedSlabWallID = (ushort)ModContent.WallType<ProfanedSlabWall>();
            ushort profanedRockWallID = (ushort)ModContent.WallType<ProfanedRockWall>();

            int y = bottom.Y;

            while (y > topY)
            {
                for (int dx = -2; dx <= 2; dx++)
                {
                    for (int dy = 0; dy < 6; dy++)
                    {
                        ushort wallID = profanedRockWallID;
                        if (Math.Abs(dx) >= 2 || dy <= 1 || dy == 4)
                            wallID = profanedSlabWallID;
                        if (Math.Abs(dx) <= 1 && dy >= 1 && dy <= 3)
                            wallID = runicBrickWallID;
                        if (Math.Abs(dx) == 2 || dy == 5)
                            wallID = profanedRockWallID;

                        int x = bottom.X + dx;
                        Main.tile[x, y + dy].wall = wallID;
                        if (y + dy == (bottom.Y + topY) / 2 - 1 && dx == 0)
                        {
                            Main.tile[x, y + dy].type = TileID.Torches;
                            Main.tile[x, y + dy].active(true);
                        }
                    }
                }
                y -= 6;
            }

            // Frame everything.
            for (y = topY; y < bottom.Y; y += 6)
            {
                for (int dx = -2; dx <= 2; dx++)
                {
                    for (int dy = 0; dy < 6; dy++)
                    {
                        int x = bottom.X + dx;
                        WorldGen.SquareWallFrame(x, y + dy);
                    }
                }
            }
        }*/
        #endregion Worldgen
		public override void ResetNearbyTileEffects()
        {
            ProfanedTile = 0;
        }

        public override void TileCountsAvailable(int[] tileCounts)
        {
            ProfanedTile = tileCounts[ModContent.TileType<ProfanedSlab>()] + tileCounts[ModContent.TileType<RunicProfanedBrick>()] + tileCounts[ModContent.TileType<ProfanedRock>()];
        }
    }
}