using Terraria;
using Terraria.ModLoader;

namespace InfernumMode.Buffs
{
    public class CatharsisHex : ModBuff
    {
        public override void SetDefaults()
        {
            DisplayName.SetDefault("Hex of Catharsis");
            Description.SetDefault("Natural life regeneration is disabled and angry spirits are released from within you");
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;
            Main.buffNoTimeDisplay[Type] = true;
        }
    }
}
