using CalamityMod.Particles;
using InfernumMode.Sounds;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using System;

namespace InfernumMode.BehaviorOverrides.BossAIs.Providence
{
    public class CleansingFireball : ModProjectile
    {
        public bool InLava
        {
            get
            {
                IEnumerable<Projectile> lavaProjectiles = Utilities.AllProjectilesByID(ModContent.ProjectileType<ProfanedLava>());
                if (!lavaProjectiles.Any())
                    return false;

                return lavaProjectiles.Any(l => l.Colliding(l.Hitbox, projectile.Hitbox));
            }
        }

        public bool CollidingWithWall => HasCollidedWithWall || Collision.SolidCollision(projectile.TopLeft, projectile.width, projectile.height);

        public ref float Time => ref projectile.ai[0];

        public bool HasCollidedWithWall
        {
            get => projectile.ai[1] == 1f;
            set => projectile.ai[1] = value.ToInt();
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Cleansing Fireball");
            Main.projFrames[projectile.type] = 4;
            ProjectileID.Sets.TrailingMode[projectile.type] = 2;
            ProjectileID.Sets.TrailCacheLength[projectile.type] = 4;
        }

        public override void SetDefaults()
        {
            projectile.width = projectile.height = 240;
            projectile.hostile = true;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;
            projectile.penetrate = -1;
            projectile.Opacity = 0f;
            projectile.timeLeft = 300;
            
            cooldownSlot = 1;
        }

        public override void AI()
        {
            // Decide the fireball's rotation.
            projectile.rotation = projectile.velocity.ToRotation();

            // Decide frames.
            projectile.frameCounter++;
            projectile.frame = projectile.frameCounter / 6 % Main.projFrames[projectile.type];

            // Dissipate into ashes if inside of a wall.
            if (CollidingWithWall && Time >= 90f)
            {
                if (!HasCollidedWithWall)
                {
                    Main.PlaySound(SoundID.NPCDeath55, projectile.Center);
                    HasCollidedWithWall = true;
                }

                // Release ashes.
                int ashCount = (int)MathHelper.Lerp(8f, 2f, projectile.Opacity);
                for (int i = 0; i < ashCount; i++)
                {
                    Color startingColor = Color.Lerp(Color.Orange, Color.Gray, Main.rand.NextFloat(0.5f, 0.8f));
                    MediumMistParticle ash = new MediumMistParticle(projectile.Center + Main.rand.NextVector2Circular(150f, 150f), Main.rand.NextVector2Circular(3f, 3f), startingColor, Color.DarkGray, projectile.Opacity, 255f, Main.rand.NextFloatDirection() * 0.014f);
                    GeneralParticleHandler.SpawnParticle(ash);
                }

                projectile.Opacity = MathHelper.Clamp(projectile.Opacity - 0.085f, 0f, 1f);
                if (projectile.Opacity <= 0f)
                    projectile.Kill();
            }

            // Prepare to explode if inside the lava.
            else if (InLava)
            {
                projectile.Opacity = MathHelper.Clamp(projectile.Opacity - 0.085f, 0f, 1f);
                if (projectile.Opacity <= 0f)
                {
                    Main.PlaySound(InfernumSoundRegistry.ProvidenceLavaEruptionSound, Main.player[Player.FindClosest(projectile.Center, 1, 1)].Center);

                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        for (int i = 0; i < 15; i++)
                        {
                            // Release a bunch of lava particles from below.
                            int lavaLifetime = Main.rand.Next(120, 167);
                            float blobSize = MathHelper.Lerp(12f, 34f, (float)Math.Pow(Main.rand.NextFloat(), 1.85f));
                            if (Main.rand.NextBool(6))
                                blobSize *= 1.4f;
                            Vector2 lavaVelocity = -Vector2.UnitY.RotatedByRandom(0.5f) * Main.rand.NextFloat(4f, 5f);
                            Utilities.NewProjectileBetter(projectile.Center + Main.rand.NextVector2Circular(40f, 40f), lavaVelocity, ModContent.ProjectileType<ProfanedLavaBlob>(), ProvidenceBehaviorOverride.SmallLavaBlobDamage, 0f, -1, lavaLifetime, blobSize);
                        }

                        // Release four cinders up from below as well.
                        for (int i = 0; i < 4; i++)
                        {
                            Vector2 cinderVelocity = -Vector2.UnitY.RotatedBy(MathHelper.Lerp(-0.51f, 0.51f, i / 3f)) * 4.25f;
                            Utilities.NewProjectileBetter(projectile.Center, cinderVelocity, ModContent.ProjectileType<HolyCinder>(), ProvidenceBehaviorOverride.CinderDamage, 0f);
                        }
                    }

                    projectile.Kill();
                }
            }

            // Fade in if none of the above conditions were met.
            else
            {
                projectile.Opacity = Utils.InverseLerp(0f, 10f, Time, true);
                projectile.scale = projectile.Opacity;
            }

            Time++;

            Lighting.AddLight(projectile.Center, Color.Yellow.ToVector3() * 0.75f);
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Texture2D texture = !ProvidenceBehaviorOverride.IsEnraged ? ModContent.GetTexture(Texture) : ModContent.GetTexture("CalamityMod/Projectiles/Boss/HolyBlastNight");
            Utilities.DrawAfterimagesCentered(projectile, Color.White, ProjectileID.Sets.TrailingMode[projectile.type], 1, texture);
            return false;
        }

        public override bool CanDamage() => projectile.Opacity >= 0.36f;
    }
}
