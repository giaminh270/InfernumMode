using CalamityMod;
using InfernumMode.Projectiles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace InfernumMode.Tiles
{
    public class WayfinderTile : ModTile
    {
        public const int Width = 2;
        public const int Height = 3;

        public override void SetDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileNoAttach[Type] = true;
            Main.tileSpelunker[Type] = true;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style2x1);
            TileObjectData.newTile.Width = Width;
            TileObjectData.newTile.Height = Height;
            TileObjectData.newTile.Origin = new Point16(0, 2);
            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop | AnchorType.SolidSide, TileObjectData.newTile.Width, 0);
            TileObjectData.newTile.CoordinateHeights = new int[] { 16, 16, 16 };
            TileObjectData.newTile.StyleHorizontal = true;
            TileObjectData.newTile.LavaDeath = false;
            TileObjectData.addTile(Type);
            AddMapEntry(new Color(178, 151, 163));
        }

        public override void NearbyEffects(int i, int j, bool closer)
        {
            Tile t = CalamityUtils.ParanoidTileRetrieval(i, j);
            if (closer && t.frameX == 18 && t.frameY == 18)
            {
                Vector2 spawnPosition = new Point(i, j).ToWorldCoordinates();

                if (Main.LocalPlayer.WithinRange(spawnPosition, 100f))
                {
                    if (Main.myPlayer == 0)
                        Projectile.NewProjectile(spawnPosition, -Vector2.UnitY * 4f, ModContent.ProjectileType<WayfinderItemProjectile>(), 0, 0f, Main.myPlayer);
                    WorldGen.KillTile(i, j);
                }
            }
        }
    }
}
