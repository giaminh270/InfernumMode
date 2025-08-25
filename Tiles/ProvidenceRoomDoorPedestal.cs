using CalamityMod;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Tiles.FurnitureProfaned;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using CalamityMod.World;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ModLoader;
using Terraria.ObjectData;
using System.Collections.Generic;
using System;
using System.IO;


namespace InfernumMode.Tiles
{
    public class ProvidenceRoomDoorPedestal : ModTile
    {
        public const int Width = 4;
        public const int Height = 1;

        public override void SetDefaults()
        {
            minPick = int.MaxValue;
            Main.tileFrameImportant[Type] = true;
            Main.tileNoAttach[Type] = true;
            Main.tileSpelunker[Type] = true;
            Main.tileNoAttach[ModContent.TileType<ProfanedCrystal>()] = false;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style2x1);
            TileObjectData.newTile.Width = Width;
            TileObjectData.newTile.Height = Height;
            TileObjectData.newTile.Origin = new Point16(2, 0);
            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop | AnchorType.SolidSide, TileObjectData.newTile.Width, 0);
            TileObjectData.newTile.CoordinateHeights = new int[] { 16 };
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

        public override void NearbyEffects(int i, int j, bool closer)
        {
            if (Main.gamePaused)
                return;

            // Calculate the door position in the world if it has yet to be initialized.
            Tile tile = CalamityUtils.ParanoidTileRetrieval(i, j);
            Vector2 bottom = new Vector2(i, j).ToWorldCoordinates(8f, 0f);
            if (PoDWorld.ProvidenceDoorXPosition == 0 && tile.frameX == 18 && tile.frameX == 0)
            {
                PoDWorld.ProvidenceDoorXPosition = (int)bottom.X;
                CalamityNetcode.SyncWorld();
            }

            ref int shatterTimer = ref Main.LocalPlayer.Infernum().ProvidenceRoomShatterTimer;
            
            if (PoDWorld.HasProvidenceDoorShattered)
            {
                Main.LocalPlayer.Infernum().ShimmerSoundVolumeInterpolant = 0f;
                return;
            }

            int verticalOffset = 0;
            for (int k = 2; k < 200; k++)
            {
                if (WorldGen.SolidTile(i, j - k))
                {
                    verticalOffset = k * 16 + 24;
                    break;
                }
            }

            bool close = Main.LocalPlayer.WithinRange(bottom, 300f) || shatterTimer >= 2;
            shatterTimer = Utils.Clamp(shatterTimer + close.ToDirectionInt(), 0, 420);
            if (!CalamityWorld.downedGuardians)
                shatterTimer = 0;

            if (shatterTimer == 2)
				Main.PlaySound(InfernumMode.CalamityMod.GetLegacySoundSlot(SoundType.Custom, "Sounds/Custom/ProvidenceDoorShatter"));

            // Do some screen shake anticipation effects.
            //if (close && CalamityWorld.downedGuardians)
            //    Main.LocalPlayer.Calamity().GeneralScreenShakePower = Utils.Remap(shatterTimer, 240f, 360f, 1f, 16f);

            // Have the door shatter into a bunch of crystals.
            if (CalamityWorld.downedGuardians && shatterTimer >= 360f)
            {
                for (int k = 0; k < verticalOffset; k += Main.rand.Next(6, 12))
                {
                    Vector2 crystalSpawnPosition = bottom - Vector2.UnitY * k + Main.rand.NextVector2Circular(24f, 24f);
                    Vector2 crystalVelocity = -Vector2.UnitY.RotatedByRandom(1.06f) * Main.rand.NextFloat(4f, 10f);

                    if (!Collision.SolidCollision(crystalSpawnPosition, 1, 1))
                        Gore.NewGore(crystalSpawnPosition, crystalVelocity, mod.GetGoreSlot($"ProvidenceDoor{Main.rand.Next(1, 3)}"), 1.16f);
                }
                
                for (int k = 0; k < verticalOffset; k += Main.rand.Next(4, 9))
                {
                    Vector2 crystalShardSpawnPosition = bottom - Vector2.UnitY * k + Main.rand.NextVector2Circular(8f, 8f);
                    Vector2 shardVelocity = Main.rand.NextVector2Unit() * Main.rand.NextFloat(3.6f, 13.6f);
                    Dust shard = Dust.NewDustPerfect(crystalShardSpawnPosition, 255, shardVelocity);
                    shard.noGravity = Main.rand.NextBool();
                    shard.scale = Main.rand.NextFloat(1.3f, 1.925f);
                    shard.velocity.Y -= 5f;
                }
                PoDWorld.HasProvidenceDoorShattered = true;
                CalamityNetcode.SyncWorld();
                shatterTimer = 0;
            }

            int horizontalBuffer = 32;
            Vector2 top = bottom - Vector2.UnitY * verticalOffset;
            Rectangle area = new Rectangle((int)top.X - Width * 8 + horizontalBuffer / 2, (int)top.Y, Width * 16 - horizontalBuffer, verticalOffset);

            // Hurt the player if they touch the spikes.
            if (Main.LocalPlayer.Hitbox.Intersects(area))
            {
                Main.LocalPlayer.Hurt(PlayerDeathReason.ByCustomReason($"{Main.LocalPlayer.name} was somehow impaled by a pillar of crystals."), 100, 0);
                Main.LocalPlayer.AddBuff(Main.dayTime ? ModContent.BuffType<HolyFlames>() : ModContent.BuffType<Nightwither>(), 180);
            }
            //Main.LocalPlayer.Infernum().ShimmerSoundVolumeInterpolant = Utils.Remap(Main.LocalPlayer.Distance(bottom), 750f, 100f, 0f, 0.4f);
        }

        public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref Color drawColor, ref int nextSpecialDrawIndex)
        {
            int xFrameOffset = Main.tile[i, j].frameX;
            int yFrameOffset = Main.tile[i, j].frameY;
            if (xFrameOffset != 0 || yFrameOffset != 0)
                return;

            if ((Main.tile[i - 1, j - 1].type != Type || Main.tile[i, j - 1].type != Type || Main.tile[i + 1, j - 1].type != Type ||
                Main.tile[i - 1, j - 2].type != Type || Main.tile[i, j - 2].type != Type || Main.tile[i + 1, j - 2].type != Type) && 
                nextSpecialDrawIndex < Main.specX.Length)
            {
                Main.specX[nextSpecialDrawIndex] = i;
                Main.specY[nextSpecialDrawIndex] = j;
                nextSpecialDrawIndex++;
            }
        }

        public override void SpecialDraw(int i, int j, SpriteBatch spriteBatch)
        {
            if (PoDWorld.HasProvidenceDoorShattered)
                return;

            Texture2D door = ModContent.GetTexture("InfernumMode/Tiles/ProvidenceRoomDoor");
            Vector2 drawOffest = (Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange));
            Vector2 drawPosition = new Vector2((float)(i * 16) - Main.screenPosition.X, (float)(j * 16) - Main.screenPosition.Y) + drawOffest;
            Color drawColour = Color.White;

            int verticalOffset = 0;
            for (int k = 2; k < 200; k++)
            {
                if (WorldGen.SolidTile(i, j - k))
                {
                    verticalOffset = k * 16 + 24;
                    break;
                }
            }

            for (int dy = verticalOffset; dy >= 0; dy -= 96)
            {
                Vector2 drawOffset = new Vector2(-12f, -dy - 48f);
                spriteBatch.Draw(door, drawPosition + drawOffset, null, drawColour, 0f, Vector2.Zero, 0.8f, SpriteEffects.None, 0f);
            }
        }
    }
}
