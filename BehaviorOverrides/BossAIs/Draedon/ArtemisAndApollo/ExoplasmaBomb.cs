using CalamityMod;
using CalamityMod.DataStructures;
using InfernumMode.DataStructures;
using InfernumMode.ExtraTextures;
using InfernumMode.GlobalInstances;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.Draedon.ArtemisAndApollo
{
    public class ExoplasmaBomb : ModProjectile, IAdditiveDrawer
    {
        public ref float ExplosionRadius => ref projectile.ai[0];

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Exoplasma Bomb");
            Main.projFrames[projectile.type] = 6;
            ProjectileID.Sets.TrailingMode[projectile.type] = 0;
            ProjectileID.Sets.TrailCacheLength[projectile.type] = 4;
        }

        public override void SetDefaults()
        {
            projectile.width = projectile.height = 20;
            projectile.hostile = true;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;
            projectile.penetrate = -1;
            projectile.timeLeft = 900;
            projectile.Opacity = 0f;
            cooldownSlot = 1;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(projectile.timeLeft);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            projectile.timeLeft = reader.ReadInt32();
        }

        public override void AI()
        {
            // Create a burst of dust on the first frame.
            if (projectile.localAI[0] == 0f)
            {
                for (int i = 0; i < 40; i++)
                {
                    Vector2 dustVelocity = projectile.velocity.SafeNormalize(Vector2.Zero).RotatedByRandom(0.35f) * Main.rand.NextFloat(1.8f, 3f);
                    int randomDustType = Main.rand.NextBool() ? 107 : 110;

                    Dust plasma = Dust.NewDustDirect(projectile.position, projectile.width, projectile.height, randomDustType, dustVelocity.X, dustVelocity.Y, 200, default, 1.7f);
                    plasma.position = projectile.Center + Main.rand.NextVector2Circular(projectile.width, projectile.width);
                    plasma.noGravity = true;
                    plasma.velocity *= 3f;

                    plasma = Dust.NewDustDirect(projectile.position, projectile.width, projectile.height, randomDustType, dustVelocity.X, dustVelocity.Y, 100, default, 0.8f);
                    plasma.position = projectile.Center + Main.rand.NextVector2Circular(projectile.width, projectile.width);
                    plasma.velocity *= 2f;

                    plasma.noGravity = true;
                    plasma.fadeIn = 1f;
                    plasma.color = Color.Green * 0.5f;
                }

                for (int i = 0; i < 20; i++)
                {
                    Vector2 dustVelocity = projectile.velocity.SafeNormalize(Vector2.Zero).RotatedByRandom(0.35f) * Main.rand.NextFloat(1.8f, 3f);
                    int randomDustType = Main.rand.NextBool() ? 107 : 110;

                    Dust plasma = Dust.NewDustDirect(projectile.position, projectile.width, projectile.height, randomDustType, dustVelocity.X, dustVelocity.Y, 0, default, 2f);
                    plasma.position = projectile.Center + Vector2.UnitX.RotatedByRandom(MathHelper.Pi).RotatedBy(projectile.velocity.ToRotation()) * projectile.width / 3f;
                    plasma.noGravity = true;
                    plasma.velocity *= 0.5f;
                }

                projectile.localAI[0] = 1f;
            }

            projectile.Opacity = MathHelper.Clamp(projectile.Opacity + 0.04f, 0f, 0.8f);

            projectile.velocity *= 0.99f;
            projectile.frameCounter++;
            projectile.frame = projectile.frameCounter / 5 % Main.projFrames[projectile.type];
            projectile.rotation = projectile.velocity.ToRotation() - MathHelper.PiOver2;

            Lighting.AddLight(projectile.Center, Color.Yellow.ToVector3() * 0.5f);
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            lightColor = Color.Lerp(lightColor, Color.White, 0.4f);
            lightColor.A = 128;
            Utilities.DrawAfterimagesCentered(projectile, lightColor, ProjectileID.Sets.TrailingMode[projectile.type]);
            return false;
        }

        public void AdditiveDraw(SpriteBatch spriteBatch)
        {
            float explosionInterpolant = Utils.InverseLerp(200f, 35f, projectile.timeLeft, true) * Utils.InverseLerp(0f, 45f, projectile.frameCounter, true);
            float circleFadeinInterpolant = Utils.InverseLerp(0f, 0.15f, explosionInterpolant, true);
            float pulseInterpolant = Utils.InverseLerp(0.75f, 0.85f, explosionInterpolant, true);
            float colorPulse = ((float)Math.Sin(Main.GlobalTime * 6.3f + projectile.identity) * 0.5f + 0.5f) * pulseInterpolant;
            if (explosionInterpolant > 0f)
            {
                Texture2D explosionTelegraphTexture = InfernumTextureRegistry.HollowCircleSoftEdge;
                Vector2 scale = Vector2.One * ExplosionRadius / explosionTelegraphTexture.Size() * projectile.Opacity * 1.2f;
                Color explosionTelegraphColor = Color.Lerp(Color.Lime, Color.Yellow, colorPulse) * circleFadeinInterpolant;
                spriteBatch.Draw(explosionTelegraphTexture, projectile.Center - Main.screenPosition, null, explosionTelegraphColor, 0f, explosionTelegraphTexture.Size() * 0.5f, scale, 0, 0f);
            }
        }

        public override void Kill(int timeLeft)
        {
            Main.PlaySound(SoundID.DD2_KoboldExplosion, projectile.Center);
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                ProjectileSpawnManagementSystem.PrepareProjectileForSpawning(explosion =>
                {
                    explosion.ModProjectile<ExoplasmaExplosion>().MaxRadius = ExplosionRadius * 0.7f;
                });
                Utilities.NewProjectileBetter(projectile.Center, Vector2.Zero, ModContent.ProjectileType<ExoplasmaExplosion>(), DraedonBehaviorOverride.StrongerNormalShotDamage, 0f);
            }
        }

        public override bool CanDamage() => false;
    }
}
