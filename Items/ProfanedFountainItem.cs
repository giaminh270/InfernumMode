using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using InfernumMode.Tiles;
using CalamityMod.Items.Placeables.FurnitureProfaned;
using CalamityMod.Items.Materials;

namespace InfernumMode.Items
{
    public class ProfanedFountainItem : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Profaned Lava Fountain");
        }

        public override void SetDefaults()
        {
            item.width = 22;
            item.height = 42;
            item.maxStack = 999;
            item.useTurn = true;
            item.autoReuse = true;
            item.useAnimation = 15;
            item.useTime = 10;
			item.useStyle = ItemUseStyleID.SwingThrow;
            item.consumable = true;
            item.value = Item.buyPrice(0, 8, 0, 0);
            item.rare = ItemRarityID.White;
            item.createTile = ModContent.TileType<ProfanedFountainTile>();
        }

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ModContent.ItemType<ProfanedRock>(), 20);
            recipe.AddIngredient(ItemID.LavaBucket);
            recipe.AddTile(TileID.Anvils);
			recipe.SetResult(this, 5);
            recipe.AddRecipe();
        }
    }
}
