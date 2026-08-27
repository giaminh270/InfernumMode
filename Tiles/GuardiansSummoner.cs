using CalamityMod;
using InfernumMode.Projectiles;
using CalamityMod.Events;
using CalamityMod.Items.SummonItems;
using CalamityMod.NPCs.ProfanedGuardians;
using CalamityMod.NPCs.Providence;
using InfernumMode;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using CalamityMod.CalPlayer;
using InfernumMode.GlobalInstances;
using InfernumMode.UI;

namespace InfernumMode.Tiles
{
    public class GuardiansSummoner : ModTile
    {
        public const int Width = 3;
        public const int Height = 6;

        public override void SetDefaults()
        {
            minPick = int.MaxValue;
            Main.tileFrameImportant[Type] = true;
            Main.tileNoAttach[Type] = true;
            Main.tileSpelunker[Type] = true;

            // Apparently this is necessary in multiplayer for some reason???
            minPick = int.MaxValue;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style3x2);
            TileObjectData.newTile.Width = Width;
            TileObjectData.newTile.Height = Height;
            TileObjectData.newTile.Origin = new Point16(1, 5);
            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop | AnchorType.SolidSide, TileObjectData.newTile.Width, 0);
            TileObjectData.newTile.CoordinateHeights = new int[] { 16, 16, 16, 16, 16, 16 };
            TileObjectData.newTile.DrawYOffset = 4;
            TileObjectData.newTile.StyleHorizontal = true;
            TileObjectData.newTile.LavaDeath = false;
            //ModTileEntity tileEntity = ModContent.GetInstance<GuardiansPlaqueTileEntity>();
            //TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(tileEntity.Hook_AfterPlacement, -1, 0, processedCoordinates: true);
            TileObjectData.addTile(Type);
            AddMapEntry(new Color(122, 66, 59));
        }

        public override bool CanExplode(int i, int j) => false;

        public override bool CanKillTile(int i, int j, ref bool blockDamaged) => false;

        public override bool CreateDust(int i, int j, ref int type)
        {
            // Fire dust.
            type = 6;
            return true;
        }

        public override bool NewRightClick(int i, int j)
        {
            // Don't open if a boss is active, to avoid popping up in the guards fight.
            if (CalamityPlayer.areThereAnyDamnBosses)
                return false;

            UIPlayer player = Main.LocalPlayer.Infernum_UI();
            player.DrawPlaqueUI = !player.DrawPlaqueUI;
            if (player.DrawPlaqueUI)
                Main.PlaySound(SoundID.MenuOpen);
            else
                Main.PlaySound(SoundID.MenuClose);
            return true;
        }

        public override void MouseOver(int i, int j)
        {
            MouseOver();
        }

        public override void MouseOverFar(int i, int j)
        {
            MouseOver();
        }

        private void MouseOver()
        {
            // Don't show if a boss is active, to avoid popping up in the guards fight.
            if (CalamityPlayer.areThereAnyDamnBosses)
                return;

            if (!GuardiansPlaqueUIManager.ShouldDraw)
            {
                Player player = Main.LocalPlayer;
                player.showItemIconText = "Read";
                player.showItemIcon2 = ModContent.ItemType<ProfanedShard>();
                player.showItemIcon = true;
            }
        }
    }
}
