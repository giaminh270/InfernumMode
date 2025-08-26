using Terraria;
using Terraria.ModLoader;

namespace InfernumMode.Buffs
{
    public class ElysianGrace : ModBuff
    {
        public override void SetDefaults()
        {
            DisplayName.SetDefault("Elysian Grace");
            Description.SetDefault("You have infinite flight time");
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;
        }
    }
}
