using Terraria;
using Terraria.ModLoader;

namespace InfernumMode.Buffs
{
    public class ZealHex : ModBuff
    {
        public override void SetDefaults()
        {
            DisplayName.SetDefault("Hex of Zeal");
            Description.SetDefault("Your opponent's magic accelerates wildly");
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;
            Main.buffNoTimeDisplay[Type] = true;
        }
    }
}
