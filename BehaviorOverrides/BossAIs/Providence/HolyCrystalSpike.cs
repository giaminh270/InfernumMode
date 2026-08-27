using InfernumMode;
using System;
using System.Linq;
using CalamityMod.Particles;
using InfernumMode.ExtraTextures;
using InfernumMode.Particles;
using InfernumMode.Sounds;
using InfernumMode.Graphics.Interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoMod.Cil;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.Providence
{
    public class HolyCrystalSpike : ModProjectile, IScreenCullDrawer
    {
        public bool HasHitTile
        {
            get => projectile.localAI[0] == 1f;
            set => projectile.localAI[0] = value.ToInt();
        }

        public ref float Time => ref projectile.ai[0];

        public ref float CurrentLength => ref projectile.ai[1];

        public static float MaxLength => 3200f;

        public override string Texture => InfernumTextureRegistry.InvisPath;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Crystal Spike");
        }

        public override void SetDefaults()
        {
            projectile.width = 42;
            projectile.height = 42;
            projectile.hostile = true;
            projectile.friendly = false;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;
            projectile.penetrate = -1;
            projectile.timeLeft = 50;
            cooldownSlot = 1;
        }

        public override void AI()
        {
            Time++;

            // Ensure that the velocity is normalized.
            projectile.velocity = projectile.velocity.SafeNormalize(Vector2.UnitY);

            // Decide rotation
            projectile.rotation = projectile.velocity.ToRotation() + MathHelper.PiOver2;

            // Make the spike extend outward until it hits a tile.
            if (!HasHitTile)
            {
                float stretchInterpolant = Utilities.Remap(CurrentLength, 600f, 1600f, 0.018f, 0.055f);
                float nextLength = MathHelper.Lerp(CurrentLength, MaxLength, stretchInterpolant);
                Vector2 previousPosition = projectile.Center + projectile.velocity * CurrentLength;
                Vector2 nextPosition = projectile.Center + projectile.velocity * nextLength;

                // Get raycast distance information and determine whether it suggests that there's more distance to travel.
                float[] sampleDistances = new float[18];
                Collision.LaserScan(previousPosition, projectile.velocity, projectile.width, MaxLength, sampleDistances);
                float distanceToHit = sampleDistances.Average();
                bool canReachNextPosition = distanceToHit >= Vector2.Distance(previousPosition, nextPosition);

                // Adjust the length to the ideal there is no impeding tile.
                if (canReachNextPosition)
                    CurrentLength = nextLength;

                // Otherwise move forward just enough to meet the tile.
                else if (Math.Abs(distanceToHit) >= 0.01f)
                {
                    CurrentLength += distanceToHit;
                    nextPosition = projectile.Center + projectile.velocity * CurrentLength;
                    HasHitTile = true;
                    projectile.netUpdate = true;

                    // Create impact effects and release rocks outward.
                    Main.PlaySound(InfernumSoundRegistry.ProvidenceBurnSound, projectile.Center);
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        // Release rocks.
                        Vector2 directionToTarget = (Main.player[Player.FindClosest(projectile.Center, 1, 1)].Center - nextPosition).SafeNormalize(Vector2.UnitY);
                        for (int i = 0; i < 3; i++)
                        {
                            float shootOffsetAngle = MathHelper.Lerp(-0.91f, 0.91f, i / 2f);
                            Vector2 rockVelocity = directionToTarget.RotatedBy(shootOffsetAngle) * 3.5f;
                            if (ProvidenceBehaviorOverride.IsEnraged)
                                rockVelocity *= 1.4f;

                            Utilities.NewProjectileBetter(nextPosition, rockVelocity, ModContent.ProjectileType<AcceleratingMagicProfanedRock>(), ProvidenceBehaviorOverride.MagicRockDamage, 0f);
                        }

                        for (int i = 0; i < 6; i++)
                        {
                            Vector2 rockVelocity = directionToTarget.RotatedByRandom(0.92f) * Main.rand.NextFloat(2f, 13f);
                            Particle rockParticle = new SandyDustParticle(nextPosition + Main.rand.NextVector2Circular(10f, 10f), rockVelocity, Color.SandyBrown, Main.rand.NextFloat(0.45f, 0.75f), 27);
                            GeneralParticleHandler.SpawnParticle(rockParticle);
                        }

                        Utilities.NewProjectileBetter(nextPosition, projectile.velocity, ModContent.ProjectileType<AcceleratingMagicProfanedRock>(), 0, 0f, -1, 0f, 1f);
                    }
                }
            }
        }

        public override void Kill(int timeLeft)
        {
            // Explode into a barrage of crystals.
            Main.PlaySound(InfernumSoundRegistry.ProvidenceCrystalPillarShatterSound, projectile.Center + projectile.velocity * CurrentLength * 0.5f);
            if (Main.netMode == NetmodeID.Server)
                return;

            Main.LocalPlayer.Infernum().CurrentScreenShakePower = 15f;
            ScreenEffectSystem.SetBlurEffect(Main.LocalPlayer.Center - Vector2.UnitY * 300f, 1.45f, 30);

            for (float k = 0; k < CurrentLength; k += Main.rand.NextFloat(9f, 16f))
            {
                Vector2 crystalSpawnPosition = projectile.Center + projectile.velocity * k + Main.rand.NextVector2Circular(10f, 10f);
                Vector2 crystalVelocity = projectile.velocity.RotatedByRandom(1.06f) * Main.rand.NextFloat(4f, 10f);

                if (!Collision.SolidCollision(crystalSpawnPosition, 1, 1))
                {
                    int goreType = ModGore.GetGoreSlot($"ProvidenceDoor{Main.rand.Next(1, 3)}");
                    Gore.NewGore(crystalSpawnPosition, crystalVelocity, goreType, 0.4f);
                }

            }

            for (float k = 0; k < CurrentLength; k += Main.rand.NextFloat(9f, 16f))
            {
                Vector2 crystalShardSpawnPosition = projectile.Center + projectile.velocity * k + Main.rand.NextVector2Circular(6f, 6f);
                Vector2 shardVelocity = Main.rand.NextVector2Unit() * Main.rand.NextFloat(3.6f, 13.6f);
                Dust shard = Dust.NewDustPerfect(crystalShardSpawnPosition, 255, shardVelocity);
                shard.noGravity = Main.rand.NextBool();
                shard.scale = Main.rand.NextFloat(1.3f, 1.925f);
                shard.velocity.Y -= 5f;
            }
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            Vector2 start = projectile.Center;
            Vector2 end = start + projectile.velocity * CurrentLength;
            float _ = 0f;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), start, end, projectile.width, ref _);
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor) => false;

        public void CullDraw(SpriteBatch spriteBatch)
        {
            Texture2D crystalSegment = ModContent.GetTexture("InfernumMode/Tiles/ProvidenceRoomDoor");
            Vector2 start = projectile.Center - Main.screenPosition;
            Vector2 end = start + projectile.velocity * CurrentLength;
            Vector2 drawPosition = end;
            Vector2 scale = Vector2.One * projectile.width / crystalSegment.Width;

            for (int i = 0; i < 1000; i++)
            {
                drawPosition += (start - end).SafeNormalize(Vector2.UnitY) * crystalSegment.Height * scale * 0.8f;
                if (Vector2.Distance(drawPosition, start) <= 35f)
                    break;

                spriteBatch.Draw(crystalSegment, drawPosition, null, projectile.GetAlpha(Color.White), projectile.rotation, crystalSegment.Size() * new Vector2(0.5f, 1f), scale, 0, 0f);
            }
        }

        public override bool ShouldUpdatePosition() => false;
    }
}
