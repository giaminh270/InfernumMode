using CalamityMod;
using InfernumMode.Miscellaneous;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using System;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.World.Generation;

namespace InfernumMode.BehaviorOverrides.BossAIs.Providence
{
    public class CrystalPillar : ModProjectile
    {
        public float Time = 0f;

        public float CurrentHeight = 0f;

        public ref float MaxPillarHeight => ref projectile.ai[1];

        public const int Lifetime = 180;

        public const float StartingHeight = 50f;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Crystal Spike");
        }

        public override void SetDefaults()
        {
            projectile.width = projectile.height = 42;
            projectile.hostile = true;
            projectile.ignoreWater = true;
            projectile.tileCollide = false;
            projectile.penetrate = -1;
            projectile.timeLeft = Lifetime;
            projectile.MaxUpdates = 2;
            projectile.Calamity().canBreakPlayerDefense = true;
            cooldownSlot = 1;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(Time);
            writer.Write(CurrentHeight);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            Time = reader.ReadSingle();
            CurrentHeight = reader.ReadSingle();
        }

        public override void AI()
        {
            Time++;

            if (Time < 60f)
                projectile.Opacity = MathHelper.Lerp(projectile.Opacity, 1f, 0.35f);
            else if (projectile.timeLeft < 40f)
            {
                projectile.damage = 0;
                projectile.Opacity = MathHelper.Lerp(projectile.Opacity, 0f, 0.25f);
            }

            // Initialize the pillar.
            if (MaxPillarHeight == 0f)
            {
                Point newBottom;
                if (projectile.velocity.Y != 0f)
                {
                    WorldUtils.Find(new Vector2(projectile.Top.X, projectile.Top.Y - 160).ToTileCoordinates(), Searches.Chain(new Searches.Down(6000), new GenCondition[]
                    {
                        new Conditions.IsSolid(),
                        new CustomTileConditions.ActiveAndNotActuated()
                    }), out newBottom);
                    bool isHalfTile = CalamityUtils.ParanoidTileRetrieval(newBottom.X, newBottom.Y - 1).halfBrick();
                    projectile.Bottom = newBottom.ToWorldCoordinates(8, isHalfTile ? 8 : 0);
                    MaxPillarHeight = (PoDWorld.ProvidenceArena.Bottom - PoDWorld.ProvidenceArena.Top) * 16f;
                }
                else
                {
                    WorldUtils.Find(new Vector2(projectile.Top.X - 160, projectile.Top.Y).ToTileCoordinates(), Searches.Chain(new Searches.Right(6000), new GenCondition[]
                    {
                        new Conditions.IsSolid(),
                        new CustomTileConditions.ActiveAndNotActuated()
                    }), out newBottom);
                    bool isHalfTile = CalamityUtils.ParanoidTileRetrieval(newBottom.X - 1, newBottom.Y).halfBrick();
                    projectile.Bottom = newBottom.ToWorldCoordinates(isHalfTile ? 8 : 0, 8);
                    MaxPillarHeight = (PoDWorld.ProvidenceArena.Right - PoDWorld.ProvidenceArena.Left) * 20f;
                }

                CurrentHeight = StartingHeight;
                projectile.netUpdate = true;
            }

            // Quickly rise.
            if (Time >= 60f && Time < 90f)
            {
                CurrentHeight = MathHelper.Lerp(StartingHeight, MaxPillarHeight, Utils.InverseLerp(60f, 90f, Time, true));
                if (Time == 74 || Time % 6 == 0)
                    projectile.netUpdate = true;
            }

            // Play a sound when rising.
            if (Time == 80f)
            {
                Player target = Main.player[Player.FindClosest(projectile.Center, 1, 1)];
                Main.PlaySound(SoundID.Item73, target.Center);
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Vector2 direction = projectile.velocity.SafeNormalize(Vector2.UnitY);
            float rotation = direction.Y == 0f ? -MathHelper.PiOver2 : 0f;
            if (Time < 60f)
            {
                float scale = (float)Math.Sin(Time / 60f * MathHelper.Pi) * 5f;
                if (scale > 1f)
                    scale = 1f;
                scale *= 2f;

                Vector2 lineOffset = Vector2.Zero;
                if (direction.Y == 0f)
                    lineOffset.Y += 42f;

                Utils.DrawLine(Main.spriteBatch, projectile.Top + lineOffset, projectile.Top - direction * (-MaxPillarHeight + 240f) + lineOffset, Color.LightGoldenrodYellow, Color.LightGoldenrodYellow, scale);
            }

            Texture2D tipTexture = ModContent.GetTexture(Texture);
            Texture2D pillarTexture = ModContent.GetTexture("InfernumMode/BehaviorOverrides/BossAIs/Providence/CrystalPillarBodyPiece");

            float tipBottom = 0f;
            Color drawColor = projectile.GetAlpha(Color.White);
            for (int i = pillarTexture.Height; i < CurrentHeight + pillarTexture.Height; i += pillarTexture.Height)
            {
                Vector2 drawPosition = projectile.Bottom + direction * i - Main.screenPosition;
                Main.spriteBatch.Draw(pillarTexture, drawPosition, null, drawColor, rotation, pillarTexture.Size() * new Vector2(0.5f, 0f), projectile.scale, SpriteEffects.None, 0f);
                tipBottom = i;
            }

            Vector2 tipDrawPosition = projectile.Bottom + direction * (tipBottom - 8f) - Main.screenPosition;
            Main.spriteBatch.Draw(tipTexture, tipDrawPosition, null, drawColor, rotation, tipTexture.Size() * new Vector2(0.5f, 1f), projectile.scale, SpriteEffects.None, 0f);
            return false;
        }

        public override void ModifyHitPlayer(Player target, ref int damage, ref bool crit)
        {
            target.Calamity().lastProjectileHit = projectile;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            if (Time < 60f)
                return false;

            float _ = 0f;
            Vector2 direction = projectile.velocity.SafeNormalize(Vector2.UnitY);
            Vector2 start = projectile.Bottom;
            Vector2 end = projectile.Bottom + direction * (CurrentHeight - 8f);
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), start, end, projectile.width * projectile.scale, ref _);
        }
    }
}
