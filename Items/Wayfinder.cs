using CalamityMod;
using CalamityMod.CalPlayer;
using InfernumMode.Effects;
using InfernumMode.Projectiles;
using InfernumMode.GlobalInstances;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.Items
{
    public class Wayfinder : ModItem
    {
        public int FrameCounter;

        public int Frame;

        public override string Texture => GetTexture();
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("The Wayfinder");
            Tooltip.SetDefault("Creates a magical gate that allows you to fast travel to it\nDoes not work when a boss is alive\nModifedInModifyTooltips");
            ItemID.Sets.AnimatesAsSoul[item.type] = true;
            Main.RegisterItemAnimation(item.type, new DrawAnimationVertical(6, 8));
        }

        public override void SetDefaults()
        {
            item.width = 56;
            item.height = 60;
            item.value = Item.sellPrice(0, 5, 0, 0);
            item.rare = ItemRarityID.Red;
            item.useTime = 30;
            item.useAnimation = 30;
            item.noUseGraphic = true;
            item.channel = true;
            item.useStyle = ItemUseStyleID.HoldingOut;
            item.shoot = ProjectileID.ConfettiGun;
        }
        public override bool AltFunctionUse(Player player) => true;

        public override bool CanUseItem(Player player) => !CalamityPlayer.areThereAnyDamnBosses && !Main.projectile.Any((p) => p.active && p.owner == player.whoAmI && p.type == ModContent.ProjectileType<WayfinderHoldout>());

        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            WayfinderHoldout.UseContext useContext;

            // If holding both up and down.
            if (InfernumMode.WayfinderDestroyKey.Current)
                useContext = WayfinderHoldout.UseContext.Destroy;
            // If holding up.
            else if (InfernumMode.WayfinderCreateKey.Current)
                useContext = WayfinderHoldout.UseContext.Create;
            // If just normally using.
            else
                useContext = WayfinderHoldout.UseContext.Teleport;

            Projectile.NewProjectile(player.Center, Vector2.Zero, ModContent.ProjectileType<WayfinderHoldout>(), 0, 0, player.whoAmI, 0, (float)useContext);
            return false;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            foreach (TooltipLine l in tooltips)
            {
                if (l.text == null)
                    continue;

                Color mainColor = CalamityUtils.ColorSwap(WayfinderSymbol.Colors[1], WayfinderSymbol.Colors[2], 4);

                if (l.text.StartsWith("ModifedInModifyTooltips"))
                {
                    l.text = $"Hold LMB to teleport to the gate" +
                    $"\nHold LMB and {InfernumMode.WayfinderCreateKey.GetAssignedKeys().FirstOrDefault() ?? "unbound"} to set the gate to your position" +
                    $"\nHold LMB and {InfernumMode.WayfinderDestroyKey.GetAssignedKeys().FirstOrDefault() ?? "unbound"} to remove the gate";
                    l.overrideColor = mainColor;
                }
            }

        }

        public static string GetTexture()
        {
            if (PoDWorld.WayfinderGateLocation != Vector2.Zero)
                return "InfernumMode/Items/WayfinderAlt";
            return "InfernumMode/Items/Wayfinder";
        }

        #region Drawing
        public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            Texture2D texture = ModContent.GetTexture(Texture);
            if (PoDWorld.WayfinderGateLocation != Vector2.Zero)
            {
                for (int i = 0; i < 12; i++)
                {
                    Vector2 afterimageOffset = (MathHelper.TwoPi * i / 12f).ToRotationVector2() * 2;
                    Color afterimageColor = new Color(1f, 0.6f, 0.4f, 0f) * 0.7f;
                    Main.spriteBatch.Draw(texture, position + afterimageOffset, item.GetCurrentFrame(ref Frame, ref FrameCounter, 6, 8, false), afterimageColor, 0, origin, scale, SpriteEffects.None, 0f);
                }
                spriteBatch.Draw(texture, position, item.GetCurrentFrame(ref Frame, ref FrameCounter, 6, 8, false), Color.White, 0f, origin, scale, SpriteEffects.None, 0f);

                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.instance.Rasterizer, null, Main.UIScaleMatrix);

                DrawData drawData = new DrawData(texture, position, item.GetCurrentFrame(ref Frame, ref FrameCounter, 6, 8, false), drawColor * 0.1f, 0f, origin, scale, SpriteEffects.None, 0);
                InfernumEffectsRegistry.RealityTear2Shader.SetShaderTexture(ModContent.GetTexture("InfernumMode/ExtraTextures/ScrollingLayers/WayfinderLayer"));
                InfernumEffectsRegistry.RealityTear2Shader.Apply(drawData);

                drawData.Draw(spriteBatch);

                spriteBatch.End();
                PlayerInput.SetZoom_UI();
                Matrix transformMatrix = Main.UIScaleMatrix;
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, transformMatrix);
            }

            spriteBatch.Draw(texture, position, item.GetCurrentFrame(ref Frame, ref FrameCounter, 6, 8), Color.White, 0f, origin, scale, SpriteEffects.None, 0f);

            return false;
        }

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            Texture2D texture = ModContent.GetTexture(Texture);
            if (PoDWorld.WayfinderGateLocation != Vector2.Zero)
            {
                for (int i = 0; i < 12; i++)
                {
                    Vector2 afterimageOffset = (MathHelper.TwoPi * i / 12f).ToRotationVector2() * 4;
                    Color afterimageColor = new Color(1f, 0.6f, 0.4f, 0f) * 0.7f;
                    Main.spriteBatch.Draw(texture, item.position - Main.screenPosition + afterimageOffset, item.GetCurrentFrame(ref Frame, ref FrameCounter, 6, 8, false), afterimageColor, 0, Vector2.Zero, scale, SpriteEffects.None, 0f);
                }
                spriteBatch.Draw(texture, item.position - Main.screenPosition, item.GetCurrentFrame(ref Frame, ref FrameCounter, 6, 8, false), Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);

                spriteBatch.EnterShaderRegion();

                DrawData drawData = new DrawData(texture, item.position - Main.screenPosition, item.GetCurrentFrame(ref Frame, ref FrameCounter, 6, 8, true), lightColor * 0.1f, 0f, Vector2.Zero, scale, SpriteEffects.None, 0);
                InfernumEffectsRegistry.RealityTear2Shader.SetShaderTexture(ModContent.GetTexture("InfernumMode/ExtraTextures/ScrollingLayers/WayfinderLayer"));
                InfernumEffectsRegistry.RealityTear2Shader.Apply(drawData);

                drawData.Draw(spriteBatch);
                spriteBatch.ExitShaderRegion();
                return false;
            }

            spriteBatch.Draw(texture, item.position - Main.screenPosition, item.GetCurrentFrame(ref Frame, ref FrameCounter, 6, 8), lightColor, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
            return false;
        }

        public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
        {
            Texture2D texture = ModContent.GetTexture(Texture + "Glow");
            spriteBatch.Draw(texture, item.position - Main.screenPosition, item.GetCurrentFrame(ref Frame, ref FrameCounter, 6, 8, frameCounterUp: false), Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
        }
        #endregion
    }
}
