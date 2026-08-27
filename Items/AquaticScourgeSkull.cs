using InfernumMode.Tiles;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.Items
{
    public class AquaticScourgeSkull : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Aquatic Scourge Skull");
        }

        public override void SetDefaults()
        {
            item.width = 102;
            item.height = 82;
            item.maxStack = 999;
            item.useTurn = true;
            item.autoReuse = true;
            item.useAnimation = 15;
            item.useTime = 10;
            item.useStyle = ItemUseStyleID.SwingThrow;
            item.consumable = true;
            item.value = Item.buyPrice(0, 20, 0, 0);
            item.rare = ItemRarityID.LightPurple;
            item.createTile = ModContent.TileType<AquaticScourgeSkullTile>();
        }
    }
}
