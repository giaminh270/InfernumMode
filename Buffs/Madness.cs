using Terraria;
using Terraria.ModLoader;

namespace InfernumMode.Buffs
{
    public class Madness : ModBuff
    {
        public override void SetDefaults()
        {
            DisplayName.SetDefault("Madness");
            Description.SetDefault("Going insane...");
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex) => player.Infernum().Madness = true;
    }
}
