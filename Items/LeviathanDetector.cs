using CalamityMod;
using CalamityMod.NPCs.Leviathan;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.Items
{
    public class LeviathanDetector : ModItem
    {
        public int frameCounter = 0;
        
        public int frame = 0;
        
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Leviathan Detector");
            Tooltip.SetDefault("Summons Anahita\n" +
                "Does not need to be used at the ocean\n" +
                "Not consumable");
        }

        public override void SetDefaults()
        {
            item.width = 18;
            item.height = 18;
            item.rare = ItemRarityID.Lime;
            item.useAnimation = 45;
            item.useTime = 45;
            item.useStyle = ItemUseStyleID.HoldingUp;
            item.consumable = false;
        }

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.Wire, 100);
            recipe.AddRecipeGroup("AnyCopperBar", 10);
            recipe.AddIngredient(ItemID.Glass, 15);
            recipe.AddIngredient(ItemID.SoulofLight, 4);
            recipe.AddIngredient(ItemID.SoulofNight, 4);
            recipe.AddTile(TileID.DemonAltar);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }

        public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frameI, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            Texture2D texture = ModContent.GetTexture("InfernumMode/Items/LeviathanDetector_Animated");
            Rectangle f = item.GetCurrentFrame(ref frame, ref frameCounter, 8, 5);
            Main.spriteBatch.Draw(texture, position, f, Color.White, 0f, f.Size() * new Vector2(0.16f, 0.25f), scale, SpriteEffects.None, 0);
            return false;
        }

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            Texture2D texture = ModContent.GetTexture("InfernumMode/Items/LeviathanDetector_Animated");
            Main.spriteBatch.Draw(texture, item.position - Main.screenPosition, item.GetCurrentFrame(ref frame, ref frameCounter, 8, 5), lightColor, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0);
            return false;
        }

        public override bool CanUseItem(Player player) => !NPC.AnyNPCs(ModContent.NPCType<Siren>()) && !NPC.AnyNPCs(ModContent.NPCType<Leviathan>()) && (player.Center.X < 9000f || player.Center.X > Main.maxTilesX * 16f - 9000f);

        public override bool UseItem(Player player)
        {
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                Vector2 spawnPosition = player.Center - Vector2.UnitY * 350f;
                NPC.NewNPC((int)spawnPosition.X, (int)spawnPosition.Y, ModContent.NPCType<Siren>(), player.whoAmI);
            }
            return true;
        }
    }
}
