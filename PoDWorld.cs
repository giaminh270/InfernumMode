using CalamityMod.NPCs;
using CalamityMod.NPCs.ExoMechs;
using CalamityMod.Tiles.FurnitureProfaned;
using InfernumMode.Tiles;
using InfernumMode.BossRush;
using Microsoft.Xna.Framework;
using CalamityMod.Walls;
using System.Collections.Generic;
using System;
using System.IO;
using Terraria;
using Terraria.GameContent.Generation;
using Terraria.ID;
using InfernumMode.Schematics;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.World.Generation;
using static InfernumMode.Schematics.InfernumSchematicManager;
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
				downed.Add("InfernumModeActive");
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

            return new TagCompound
			{
				["downed"] = downed,
				["ProvidenceArenaX"] = ProvidenceArena.X,
				["ProvidenceArenaY"] = ProvidenceArena.Y,
				["ProvidenceArenaWidth"] = ProvidenceArena.Width,
				["ProvidenceArenaHeight"] = ProvidenceArena.Height,
				["ProvidenceDoorXPosition"] = ProvidenceDoorXPosition
			};
			
		}
        #endregion

        #region Load
        public override void Load(TagCompound tag)
        {
            var downed = tag.GetList<string>("downed");
            InfernumMode = downed.Contains("InfernumModeActive");
			HasGeneratedProfanedShrine = downed.Contains("HasGeneratedProfanedShrine");
            HasBeatedInfernumProvRegularly = downed.Contains("HasBeatedInfernumProvRegularly");
			HasBeatedInfernumNightProvBeforeDay = downed.Contains("HasBeatedInfernumNightProvBeforeDay");
			HasProvidenceDoorShattered = downed.Contains("HasProvidenceDoorShattered");
			HasSepulcherAnimationBeenPlayed = downed.Contains("HasSepulcherAnimationBeenPlayed");
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
            flags[4] = HasSepulcherAnimationBeenPlayed;			
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
			HasSepulcherAnimationBeenPlayed = flags[4];
        }
        #endregion

        #region Updating
        public override void PostUpdate()
        {
            // Disable natural GSS spawns.
            if (ModInstance.CanUseCustomAIs)
                CalamityMod.CalamityMod.sharkKillCount = 0;
			
			BossRushChanges.HandleTeleports();

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
                tasks.Insert(++finalCleanupIndex, new PassLegacy("Prov Arena", (GenerationProgress progress) =>
                {
                    progress.Message = "Constructing a temple for an ancient goddess";
                    GenerateProfanedArena(progress);
                }));
            }
        }


        public static void GenerateProfanedArena(GenerationProgress progress)
        {
            bool _3 = false;
            Point bottomLeftOfWorld = new Point(Main.maxTilesX - 42, Main.maxTilesY - 42);
			PlaceSchematic<Action<Chest>>("Profaned Arena", bottomLeftOfWorld, SchematicAnchor.BottomRight, ref _3);			
			SchematicMetaTile[,] schematic = InfernumSchematicIO.LoadSchematic("Schematics/ProfanedArena.csch");
            int width = schematic.GetLength(0);
            int height = schematic.GetLength(1);

            ProvidenceArena = new Rectangle(bottomLeftOfWorld.X - width, bottomLeftOfWorld.Y - height, width, height);
            HasGeneratedProfanedShrine = true;
        }
		
	
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