using CalamityMod;
using CalamityMod.DataStructures;
using CalamityMod.Projectiles.Boss;
using InfernumMode.DataStructures;
using InfernumMode.ExtraTextures;
using InfernumMode.GlobalInstances;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.SupremeCalamitas
{
    public class DemonicBomb : ModProjectile, IAdditiveDrawer
    {
        public bool ExplodeIntoDarts;

        public ref float ExplosionRadius => ref projectile.ai[0];

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Demonic Bomb");
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
            writer.Write(ExplodeIntoDarts);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            projectile.timeLeft = reader.ReadInt32();
            ExplodeIntoDarts = reader.ReadBoolean();
        }

        public override void AI()
        {
            projectile.Opacity = MathHelper.Clamp(projectile.Opacity + 0.08f, 0f, 0.55f);

            projectile.velocity *= 0.99f;
            projectile.frameCounter++;
            projectile.frame = projectile.frameCounter / 5 % Main.projFrames[projectile.type];
            projectile.rotation = projectile.velocity.ToRotation() + MathHelper.PiOver2;

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
                Vector2 scale = Vector2.One * ExplosionRadius / explosionTelegraphTexture.Size() * 1.2f;
                Color explosionTelegraphColor = Color.Lerp(Color.Purple, Color.Red, colorPulse) * circleFadeinInterpolant;
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
                    explosion.ModProjectile<DemonicExplosion>().MaxRadius = ExplosionRadius * 0.7f;
                });
                Utilities.NewProjectileBetter(projectile.Center, Vector2.Zero, ModContent.ProjectileType<DemonicExplosion>(), SupremeCalamitasBehaviorOverride.DemonicExplosionDamage, 0f);

                if (ExplodeIntoDarts)
                {
                    for (int i = 0; i < 6; i++)
                    {
                        Vector2 dartVelocity = (MathHelper.TwoPi * i / 6f).ToRotationVector2() * 7.4f;
                        Utilities.NewProjectileBetter(projectile.Center, dartVelocity, ModContent.ProjectileType<BrimstoneBarrage>(), SupremeCalamitasBehaviorOverride.BrimstoneDartDamage, 0f);
                    }
                }
            }

            // Do some some mild screen-shake effects to accomodate the explosion.
            // This effect is set instead of added to to ensure separate explosions do not together create an excessive amount of shaking.
            float screenShakeFactor = Utilities.Remap(projectile.Distance(Main.LocalPlayer.Center), 2000f, 1300f, 0f, 11f);
            if (Main.LocalPlayer.Calamity().GeneralScreenShakePower < screenShakeFactor)
                Main.LocalPlayer.Calamity().GeneralScreenShakePower = screenShakeFactor;
        }

        public override bool CanDamage() => false;
    }
}
