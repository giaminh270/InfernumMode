using CalamityMod;
using CalamityMod.Items.SummonItems;
using CalamityMod.NPCs.OldDuke;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.Items
{
    public class BloodwormPlatter : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Bloodworm Platter");
            Tooltip.SetDefault("Summons the Old Duke\n" +
                "Can only be used in the Sulphurous Sea\n" +
                "Not consumable");
        }

        public override void SetDefaults()
        {
            item.width = 38;
            item.height = 54;
            item.rare = ItemRarityID.Red;
            item.useAnimation = 40;
            item.useTime = 40;
            item.useStyle = ItemUseStyleID.EatingUsing;
            item.consumable = false;
        }

        public override bool CanUseItem(Player player) => !NPC.AnyNPCs(ModContent.NPCType<OldDuke>()) && player.Calamity().ZoneSulphur;

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ModContent.ItemType<BloodwormItem>(), 3);
            recipe.AddTile(TileID.DemonAltar);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }

        public override bool UseItem(Player player)
        {
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                Vector2 spawnPosition = player.Center - Vector2.UnitY * 800f;
                NPC.NewNPC((int)spawnPosition.X, (int)spawnPosition.Y, ModContent.NPCType<OldDuke>());
            }
            return true;
        }
    }
}
