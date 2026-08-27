using CalamityMod;
using InfernumMode.Effects;
using InfernumMode.ExtraTextures;
using InfernumMode.Graphics.Interfaces;
using InfernumMode.GlobalInstances;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.Providence
{
    public class HolyBomb : ModProjectile, ISpecializedDrawRegion
    {
        public float ExplosionRadius => projectile.ai[0];

        public ref float Time => ref projectile.ai[1];

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Holy Bomb");
            Main.projFrames[projectile.type] = 4;
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
            projectile.timeLeft = 240;
            projectile.Opacity = 0f;
        }

        public override void SendExtraAI(BinaryWriter writer) => writer.Write(projectile.timeLeft);

        public override void ReceiveExtraAI(BinaryReader reader) => projectile.timeLeft = reader.ReadInt32();

        public override void AI()
        {
            projectile.Opacity = MathHelper.Clamp(projectile.Opacity + 0.08f, 0f, 0.48f);

            projectile.velocity *= 0.99f;
            projectile.frameCounter++;
            projectile.frame = projectile.frameCounter / 5 % Main.projFrames[projectile.type];

            if (projectile.velocity != Vector2.Zero)
                projectile.rotation = projectile.velocity.ToRotation() - MathHelper.PiOver2;

            Lighting.AddLight(projectile.Center, Color.Yellow.ToVector3() * 0.5f);
            Time++;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Texture2D texture = ModContent.GetTexture(Texture);
            if (ProvidenceBehaviorOverride.IsEnraged)
                texture = ModContent.GetTexture("InfernumMode/BehaviorOverrides/BossAIs/Providence/HolyBombNight");

            // Make the light color considerably brighter, especially at the beginning of the bomb's lifetime.
            lightColor = Color.Lerp(lightColor, Color.White, 0.55f);
            lightColor.A = (byte)(lightColor.A / Utilities.Remap(Time, 0f, 45f, 6f, 2f));

            Utilities.DrawAfterimagesCentered(projectile, lightColor * projectile.Opacity, ProjectileID.Sets.TrailingMode[projectile.type], 1, texture);

            return false;
        }

        public override void Kill(int timeLeft)
        {
            Main.PlaySound(SoundID.DD2_KoboldExplosion, projectile.Center);
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                int explosionDamage = !ProvidenceBehaviorOverride.IsEnraged ? 350 : 600;

                ProjectileSpawnManagementSystem.PrepareProjectileForSpawning(explosion =>
                {
                    explosion.ModProjectile<HolySunExplosion>().MaxRadius = ExplosionRadius * 0.7f;
                });
                Utilities.NewProjectileBetter(projectile.Center, Vector2.Zero, ModContent.ProjectileType<HolySunExplosion>(), explosionDamage, 0f);
            }

            // Do some some mild screen-shake effects to accomodate the explosion.
            // This effect is set instead of added to to ensure separate explosions do not together create an excessive amount of shaking.
            float screenShakeFactor = Utilities.Remap(projectile.Distance(Main.LocalPlayer.Center), 2000f, 1300f, 0f, 5f);
            if (Main.LocalPlayer.Calamity().GeneralScreenShakePower < screenShakeFactor)
                Main.LocalPlayer.Calamity().GeneralScreenShakePower = screenShakeFactor;
        }

        public override bool CanDamage() => false;

        public void SpecialDraw(SpriteBatch spriteBatch)
        {
            float explosionInterpolant = Utils.InverseLerp(200f, 35f, projectile.timeLeft, true);
            float pulseInterpolant = Utils.InverseLerp(0.75f, 0.85f, explosionInterpolant, true);
            float circleFadeinInterpolant = Utils.InverseLerp(0f, 0.15f, explosionInterpolant, true);
            float colorPulse = ((float)Math.Cos(Main.GlobalTime * 7.2f + projectile.identity) * 0.5f + 0.5f) * pulseInterpolant * 0.6f;
            colorPulse += (float)(Math.Cos(Main.GlobalTime * 6.1f + projectile.identity * 1.3f) * 0.5f + 0.5f) * 0.4f;

            if (explosionInterpolant > 0f)
            {
                Color explosionTelegraphColor = Color.Lerp(Color.Yellow, Color.Orange, colorPulse) * circleFadeinInterpolant;
                if (ProvidenceBehaviorOverride.IsEnraged)
                    explosionTelegraphColor = Color.Lerp(Color.Cyan, Color.Lime, colorPulse * 0.67f) * circleFadeinInterpolant;
                explosionTelegraphColor = Color.Lerp(explosionTelegraphColor, Color.White, (1f - pulseInterpolant) * 0.45f);

                Texture2D invisible = InfernumTextureRegistry.Invisible;
                Texture2D noise = ModContent.GetTexture("InfernumMode/ExtraTextures/GreyscaleGradients/VoronoiShapes2");
                Effect fireballShader = InfernumEffectsRegistry.FireballShader.GetShader().Shader;

                Vector2 scale = Vector2.One * ExplosionRadius / invisible.Size() * circleFadeinInterpolant * projectile.Opacity * 1.67f;
                fireballShader.Parameters["sampleTexture2"].SetValue(noise);
                fireballShader.Parameters["mainColor"].SetValue(explosionTelegraphColor.ToVector3());
                fireballShader.Parameters["resolution"].SetValue(Vector2.One * 250f);
                fireballShader.Parameters["speed"].SetValue(0.76f);
                fireballShader.Parameters["time"].SetValue(Main.GlobalTime);
                fireballShader.Parameters["zoom"].SetValue(0.0004f);
                fireballShader.Parameters["dist"].SetValue(60f);
                fireballShader.Parameters["opacity"].SetValue(circleFadeinInterpolant * projectile.Opacity / 0.48f * 0.335f);
                fireballShader.CurrentTechnique.Passes[0].Apply();

                Vector2 drawPosition = projectile.Center + Vector2.UnitY * projectile.scale * 18f - Main.screenPosition;
                Main.spriteBatch.Draw(invisible, drawPosition, null, Color.White, projectile.rotation, invisible.Size() * 0.5f, scale, 0, 0f);
                Main.spriteBatch.Draw(invisible, drawPosition, null, Color.White, projectile.rotation, invisible.Size() * 0.5f, scale * 0.5f, 0, 0f);
                Main.spriteBatch.Draw(invisible, drawPosition, null, Color.White, projectile.rotation, invisible.Size() * 0.5f, scale * 0.32f, 0, 0f);
            }
        }

        public void PrepareSpriteBatch(SpriteBatch spriteBatch)
        {
            spriteBatch.EnterShaderRegion(BlendState.Additive);
        }
    }
}
