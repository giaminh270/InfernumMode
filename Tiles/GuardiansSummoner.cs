using CalamityMod;
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

            TileObjectData.newTile.CopyFrom(TileObjectData.Style3x2);
            TileObjectData.newTile.Width = Width;
            TileObjectData.newTile.Height = Height;
            TileObjectData.newTile.Origin = new Point16(1, 5);
            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop | AnchorType.SolidSide, TileObjectData.newTile.Width, 0);
            TileObjectData.newTile.CoordinateHeights = new int[] { 16, 16, 16, 16, 16, 16 };
            TileObjectData.newTile.DrawYOffset = 4;
            TileObjectData.newTile.StyleHorizontal = true;
            TileObjectData.newTile.LavaDeath = false;
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
            Tile tile = Main.tile[i, j];

            int left = i - tile.frameX / 18;
            int top = j - tile.frameY / 18;

            if (!Main.LocalPlayer.HasItem(ModContent.ItemType<ProfanedShard>()))
                return true;

            if (NPC.AnyNPCs(ModContent.NPCType<Providence>()) || NPC.AnyNPCs(ModContent.NPCType<ProfanedGuardianBoss>()) || BossRushEvent.BossRushActive)
                return true;

            if (CalamityUtils.CountProjectiles(ModContent.ProjectileType<GuardiansSummonerProjectile>()) > 0)
                return true;

            Vector2 ritualSpawnPosition = new Vector2(left + Width * 0.5f, top).ToWorldCoordinates();
            ritualSpawnPosition += new Vector2(-10f, 76f);

            Main.PlaySound(SoundID.DD2_EtherianPortalOpen, ritualSpawnPosition);
            Projectile.NewProjectile(ritualSpawnPosition, Vector2.Zero, ModContent.ProjectileType<GuardiansSummonerProjectile>(), 0, 0f, Main.myPlayer);

            return true;
        }

        public override void MouseOver(int i, int j)
        {
            Main.LocalPlayer.showItemIcon2 = ModContent.ItemType<ProfanedCoreUnlimited>();
            Main.LocalPlayer.noThrow = 2;
            Main.LocalPlayer.showItemIcon = true;
        }

        public override void MouseOverFar(int i, int j)
        {
            Main.LocalPlayer.showItemIcon2 = ModContent.ItemType<ProfanedShard>();
            Main.LocalPlayer.noThrow = 2;
            Main.LocalPlayer.showItemIcon = true;
        }
    }
}
