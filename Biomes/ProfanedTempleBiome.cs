
using Terraria;
using Terraria.ModLoader;

namespace InfernumMode.Biomes
{
    public class ProfanedTempleBiome
    {
		/*public override MusicPriority Priority => MusicPriority.Environment;

        public override int Music => MusicLoader.GetMusicSlot(Mod, "Sounds/Music/ProfanedTemple");*/
		
		public static void UpdateMusic(ref int music, ref MusicPriority priority)
		{
			if (Main.myPlayer != -1 && !Main.gameMenu)
			{
				Player localPlayer = Main.LocalPlayer;
				if (localPlayer.active && localPlayer.Infernum().InProfanedArena)
					music = InfernumMode.CalamityMod.GetSoundSlot(SoundType.Music, "Sounds/Music/ProfanedTemple");	
					priority = MusicPriority.Environment;
			}
		}	
        /*public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Profaned Temple");
        }

        public override bool IsBiomeActive(Player player)
        {
            return !player.ZoneDungeon && ((InfernumBiomeTileCounterSystem.ProfanedTile > 350 && player.ZoneUnderworldHeight) || player.Infernum().InProfanedArena);
        }*/
    }
}
