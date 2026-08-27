using System;
using InfernumMode.Sounds;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Utilities;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.AquaticScourge
{
    public class SulphuricTornado : ModProjectile
    {
        public SlotId WindSlot
        {
            get;
            set;
        }

        public ref float Time => ref projectile.ai[0];

        public ref float FlyDirection => ref projectile.ai[1];

        public static int Lifetime => 720;

        public override string Texture => "CalamityMod/Projectiles/Boss/OldDukeVortex";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Sulphuric Typhoon");
            ProjectileID.Sets.TrailCacheLength[projectile.type] = 6;
            ProjectileID.Sets.TrailingMode[projectile.type] = 0;
        }

        public override void SetDefaults()
        {
            projectile.width = projectile.height = 10;
            projectile.scale = 0.004f;
            projectile.hostile = true;
            projectile.Opacity = 0f;
            projectile.penetrate = -1;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;
            projectile.timeLeft = Lifetime;
            cooldownSlot = 1;
        }

        public override void AI()
        {
            projectile.rotation -= projectile.Opacity * 0.15f;
            projectile.width = (int)(projectile.scale * 208f);
            projectile.height = (int)(projectile.scale * 936f);

            // Fade in and grow to the appropriate size.
            projectile.Opacity = Utils.InverseLerp(0f, 120f, Time, true) * Utils.InverseLerp(0f, 45f, projectile.timeLeft, true);
            projectile.scale = Utils.InverseLerp(0f, 108f, Time, true);

            // Move upward.
            Player target = Main.player[Player.FindClosest(projectile.Center, 1, 1)];
            projectile.velocity.X = (float)Math.Cos(MathHelper.TwoPi * Time / 270f) * FlyDirection * projectile.Opacity * 19.5f;
            projectile.velocity.Y = -4.5f;
            projectile.position.X += projectile.SafeDirectionTo(target.Center).X * projectile.Opacity * 13f;

            // Create a large column of bubbles before the tornado becomes full-sized, so that the play knows to avoid its general location.
            if (Main.netMode != NetmodeID.Server && Time < 90f)
            {
                for (int i = 0; i < 3; i++)
                {
                    int bubbleID = 421;
                    if (Main.rand.NextBool(4))
                        bubbleID = 422;
                    if (Main.rand.NextBool(8))
                        bubbleID = 423;
                    if (Main.rand.NextBool(25))
                        bubbleID = 424;

                    float horizontalOffset = MathHelper.Lerp(-projectile.width * 0.65f, projectile.width * 0.65f, (float)Math.Pow(Main.rand.NextFloat(), 2f));
                    Vector2 bubbleSpawnPosition = projectile.Bottom + new Vector2(horizontalOffset, Main.rand.NextFloat(400f, -projectile.height - 800f));
                    Vector2 bubbleSpawnVelocity = -Vector2.UnitY.RotatedByRandom(0.4f) * Main.rand.NextFloat(2f, 8f);
                    Gore bubble = Gore.NewGorePerfect(bubbleSpawnPosition, bubbleSpawnVelocity, bubbleID);
                    bubble.timeLeft = Main.rand.Next(35, 60);
                    bubble.type = bubbleID;
                }
            }

            // Release a spray of falling acid.
            if (Main.netMode != NetmodeID.MultiplayerClient && projectile.Opacity >= 0.6f && projectile.height >= 640f && Time % 4f == 3f)
            {
                Vector2 acidSpawnPosition = projectile.Top + new Vector2(Main.rand.NextFloatDirection() * 100f, Main.rand.NextFloat(250f));
                Vector2 acidSpawnVelocity = -Vector2.UnitY.RotatedByRandom(1.1f) * new Vector2(1.6f, 1f) * Main.rand.NextFloat(10f, 25f);
                Utilities.NewProjectileBetter(acidSpawnPosition, acidSpawnVelocity, ModContent.ProjectileType<FallingAcid>(), AquaticScourgeHeadBehaviorOverride.AcidDropDamage, 0f);
            }

            // Handle sound stuff.
            ActiveSound windSound = Main.GetActiveSound(WindSlot);
            if (windSound != null && windSound.IsPlaying)
            {
                if (windSound.Position != projectile.Center)
                    windSound.Position = projectile.Center;
            }
            else
                WindSlot = Main.PlayTrackedSound(InfernumSoundRegistry.CloudElementalWindSound, projectile.Center);

            Time++;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Texture2D texture = ModContent.GetTexture(Texture);
            for (float dy = 0; dy < projectile.height; dy += 30f)
            {
                float rotation = projectile.rotation + MathHelper.Pi * dy / projectile.height;
                float opacity = MathHelper.Lerp(1f, 0.6f, dy / projectile.height) * projectile.Opacity * 0.3f;
                Vector2 drawPosition = projectile.Bottom - Main.screenPosition - Vector2.UnitY * dy;
                Color tornadoColor = Color.White * opacity;
                tornadoColor.A /= 3;

                Main.spriteBatch.Draw(texture, drawPosition, null, tornadoColor, rotation, texture.Size() * 0.5f, TornadoPieceScale(dy), 0, 0);
            }
            return false;
        }

        public float TornadoPieceScale(float dy) => MathHelper.Lerp(0.3f, 1.4f, dy / projectile.height) * projectile.scale;

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            for (float dy = 0; dy < projectile.height; dy += 30f)
            {
                if (Utilities.CircularCollision(projectile.Bottom - Vector2.UnitY * dy, targetHitbox, TornadoPieceScale(dy) * 196f))
                    return true;
            }
            return false;
        }

        public override bool CanDamage() => projectile.Opacity >= 0.7f;
    }
}
