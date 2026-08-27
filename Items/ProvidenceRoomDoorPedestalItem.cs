using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;

namespace InfernumMode.Items
{
    public class ProvidenceRoomDoorPedestalItem : ModItem
    {
        public override void SetDefaults()
        {
            item.width = 16;
            item.height = 16;
            item.maxStack = 99;
            item.useTurn = true;
            item.autoReuse = true;
            item.useAnimation = 15;
            item.useTime = 10;
            item.useStyle = ItemUseStyleID.SwingThrow;
            item.consumable = true;
            item.value = Item.sellPrice(silver: 50);
            item.rare = ItemRarityID.Orange;
            item.createTile = ModContent.TileType<Tiles.ProvidenceRoomDoorPedestal>();
        }

        public override void AddRecipes()
        {
            // Tạo recipe với 5 crystal shard và 1 hellstone bar
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.CrystalShard, 5);
            recipe.AddIngredient(ItemID.HellstoneBar, 1);
            recipe.AddTile(TileID.Anvils);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
    }
}