using CalamityMod;
using InfernumMode.Miscellaneous;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;
using Terraria.World.Generation;

namespace InfernumMode.BehaviorOverrides.BossAIs.EoW
{
    public class CorruptThorn : ModProjectile
    {
        public ref float MaxPillarHeight => ref projectile.ai[0];
        public ref float Time => ref projectile.ai[1];
        public float CurrentHeight = 0f;
        public const float StartingHeight = 22f;
        public override void SetStaticDefaults() => DisplayName.SetDefault("Corrupt Thorn");

        public override void SetDefaults()
        {
            projectile.width = projectile.height = 28;
            projectile.hostile = true;
            projectile.ignoreWater = true;
            projectile.tileCollide = false;
            projectile.penetrate = -1;
            projectile.timeLeft = 480;
            projectile.Calamity().canBreakPlayerDefense = true;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(CurrentHeight);
            writer.Write(projectile.rotation);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            CurrentHeight = reader.ReadSingle();
            projectile.rotation = reader.ReadSingle();
        }

        public override void AI()
        {
            Time++;

            // Fade in at the beginning of the projectile's life.
            if (Time < 60f)
                projectile.Opacity = MathHelper.Lerp(projectile.Opacity, 1f, 0.35f);

            // Wither away at the end of the projectile's life.
            else if (projectile.timeLeft < 40f)
            {
                projectile.damage = 0;
                projectile.scale = MathHelper.Lerp(projectile.scale, 0.05f, 0.08f);
                projectile.Opacity = MathHelper.Lerp(projectile.Opacity, 0f, 0.25f);
            }

            // Initialize the pillar.
            if (Main.netMode != NetmodeID.MultiplayerClient && MaxPillarHeight == 0f)
                InitializePillarProperties();

            // Quickly rise.
            if (Main.netMode != NetmodeID.MultiplayerClient && Time >= 60f && Time < 75f)
            {
                CurrentHeight = MathHelper.Lerp(StartingHeight, MaxPillarHeight, Utils.InverseLerp(60f, 75f, Time, true));
                if (Time % 6 == 0)
                    projectile.netUpdate = true;
            }

            // Play a sound when rising.
            if (Time == 70)
            {
                Player target = Main.player[Player.FindClosest(projectile.Center, 1, 1)];
                Main.PlaySound(SoundID.Item45, target.Center);
            }
        }

        public void InitializePillarProperties()
        {
            WorldUtils.Find(new Vector2(projectile.Top.X, projectile.Top.Y - 160).ToTileCoordinates(), Searches.Chain(new Searches.Down(6000), new GenCondition[]
            {
                new Conditions.IsSolid(),
                new CustomTileConditions.ActiveAndNotActuated(),
                new CustomTileConditions.NotPlatform()
            }), out Point newBottom);

            bool isHalfTile = CalamityUtils.ParanoidTileRetrieval(newBottom.X, newBottom.Y - 1).halfBrick();
            projectile.Bottom = newBottom.ToWorldCoordinates(8, isHalfTile ? 8 : 0);

            Player target = Main.player[Player.FindClosest(projectile.Center, 1, 1)];
            MaxPillarHeight = MathHelper.Max(0f, projectile.Top.Y - target.Top.Y) + StartingHeight + 320f + Math.Abs(target.velocity.Y * 25f);

            // Add some variance to the pillar height to make them feel a bit more alive.
            MaxPillarHeight += MathHelper.Lerp(0f, 100f, projectile.identity / 7f % 7f) * Main.rand.NextFloat(0.45f, 1.55f);

            CurrentHeight = StartingHeight;
            projectile.rotation = Main.rand.NextFloat(-0.15f, 0.15f);

            if (!Collision.CanHit(projectile.Bottom - Vector2.UnitY * 10f, 2, 2, projectile.Bottom - Vector2.UnitY * 32f, 2, 2))
                projectile.Kill();

            projectile.netUpdate = true;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Texture2D tipTexture = ModContent.GetTexture(Texture);
            Vector2 aimDirection = Vector2.UnitY.RotatedBy(projectile.rotation);
            if (Time < 60f)
            {
                float telegraphLineWidth = (float)Math.Sin(Time / 60f * MathHelper.Pi) * 3f;
                if (telegraphLineWidth > 2f)
                    telegraphLineWidth = 2f;
                spriteBatch.DrawLineBetter(projectile.Top + aimDirection * 10f, projectile.Top + aimDirection * -MaxPillarHeight, Color.Gray, telegraphLineWidth);
            }

            float tipBottom = 0f;
            Vector2 scale = new Vector2(projectile.scale, 1f);

            DrawVine(spriteBatch, scale, aimDirection, tipTexture, ref tipBottom);

            Vector2 tipDrawPosition = projectile.Bottom - aimDirection * (tipBottom + 4f) - Main.screenPosition;
            spriteBatch.Draw(tipTexture, tipDrawPosition, null, projectile.GetAlpha(Color.White), projectile.rotation, tipTexture.Size() * 0.5f, scale, SpriteEffects.None, 0f);
            return false;
        }

        public void DrawVine(SpriteBatch spriteBatch, Vector2 scale, Vector2 aimDirection, Texture2D tipTexture, ref float tipBottom)
        {
            Texture2D thornBodyPiece = ModContent.GetTexture("InfernumMode/BehaviorOverrides/BossAIs/EoW/CorruptThornPiece");

            UnifiedRandom sideThornRNG = new UnifiedRandom(projectile.identity);
            for (int i = thornBodyPiece.Height; i < CurrentHeight + thornBodyPiece.Height; i += thornBodyPiece.Height)
            {
                Vector2 drawPosition = projectile.Bottom - aimDirection * i - Main.screenPosition;

                // Draw sideThorns on the side from time to time.
                if (sideThornRNG.NextFloat() < 0.7f && Math.Abs(i - (CurrentHeight + thornBodyPiece.Height)) > 60f)
                {
                    float offsetRotation = -sideThornRNG.NextFloat(0.25f);

                    // Sometimes draw sideThorns at an opposite angle.
                    if (sideThornRNG.NextBool(3))
                        offsetRotation = MathHelper.Pi - offsetRotation + MathHelper.PiOver4;

                    float sideThornRotation = aimDirection.RotatedBy(offsetRotation).ToRotation();
                    Vector2 sideThornPosition = drawPosition + sideThornRotation.ToRotationVector2().RotatedBy(-MathHelper.PiOver2) * 18f;
                    spriteBatch.Draw(tipTexture, sideThornPosition, null, projectile.GetAlpha(Color.White), sideThornRotation, tipTexture.Size() * 0.5f, scale, SpriteEffects.None, 0f);
                }

                spriteBatch.Draw(thornBodyPiece, drawPosition, null, projectile.GetAlpha(Color.White), projectile.rotation, thornBodyPiece.Size() * new Vector2(0.5f, 0f), scale, SpriteEffects.None, 0f);
                tipBottom = i;
            }
        }

        

        public override bool CanDamage() => Time >= 70f;

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float _ = 0f;
            Vector2 start = projectile.Bottom;
            Vector2 end = projectile.Bottom - Vector2.UnitY.RotatedBy(projectile.rotation) * CurrentHeight;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), start, end, projectile.width * projectile.scale, ref _);
        }
    }
}

