using CalamityMod.Particles;
using CalamityMod.Projectiles.Boss;
using InfernumMode;
using InfernumMode.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using System;
using System.Collections.Generic;
using System.Linq;
using static System.Math;
using static Microsoft.Xna.Framework.MathHelper;

namespace InfernumMode.BehaviorOverrides.BossAIs.CalamitasShadow
{
    public class CharredWand : ModProjectile
    {
        public ref float Time => ref projectile.ai[0];

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Charred Wand");
            ProjectileID.Sets.TrailCacheLength[projectile.type] = 9;
            ProjectileID.Sets.TrailingMode[projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            projectile.width = projectile.height = 34;
            projectile.scale = 0.8f;
            projectile.ignoreWater = true;
            projectile.tileCollide = false;
            projectile.Opacity = 0f;
            projectile.penetrate = -1;
            projectile.timeLeft = 84;
            cooldownSlot = 1;
        }

        public override void AI()
        {
            projectile.Opacity = Utils.InverseLerp(0f, 45f, projectile.timeLeft, true);
            projectile.rotation += projectile.velocity.X * 0.03f;
            projectile.velocity *= 0.986f;

            // Jitter before exploding.
            projectile.Center += Main.rand.NextVector2Circular(1f, 1f) * (1f - projectile.Opacity) * 2.5f;

            Time++;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            float explosionInterpolant = Utils.InverseLerp(50f, 18f, projectile.timeLeft, true);
            if (explosionInterpolant > 0f)
            {
                Main.spriteBatch.EnterShaderRegion();
				Color explosionTelegraphColor = Color.Lerp(Color.Red, Color.White, 0.4f) * (float)Math.Sqrt(explosionInterpolant);

                Texture2D invisible = ModContent.GetTexture("InfernumMode/ExtraTextures/Invisible");
                Texture2D noise = ModContent.GetTexture("CalamityMod/ExtraTextures/VoronoiShapes");
                Effect fireballShader = Filters.Scene["Infernum:FireballShader"].GetShader().Shader;

                Vector2 scale = Vector2.One * 950f / invisible.Size() * explosionInterpolant * projectile.Opacity;
                fireballShader.Parameters["sampleTexture2"].SetValue(noise);
                fireballShader.Parameters["mainColor"].SetValue(explosionTelegraphColor.ToVector3());
                fireballShader.Parameters["resolution"].SetValue(Vector2.One * 250f);
                fireballShader.Parameters["speed"].SetValue(0.76f);
                fireballShader.Parameters["time"].SetValue(Main.GlobalTime);
                fireballShader.Parameters["zoom"].SetValue(0.0004f);
                fireballShader.Parameters["dist"].SetValue(60f);
                fireballShader.Parameters["opacity"].SetValue(explosionInterpolant * projectile.Opacity * 0.335f);
                fireballShader.CurrentTechnique.Passes[0].Apply();

                Vector2 drawPosition = projectile.Center - Main.screenPosition;
                Main.spriteBatch.Draw(invisible, drawPosition, null, Color.White, projectile.rotation, invisible.Size() * 0.5f, scale, 0, 0f);
                Main.spriteBatch.Draw(invisible, drawPosition, null, Color.White, projectile.rotation, invisible.Size() * 0.5f, scale * 0.5f, 0, 0f);
                Main.spriteBatch.Draw(invisible, drawPosition, null, Color.White, projectile.rotation, invisible.Size() * 0.5f, scale * 0.32f, 0, 0f);
                Main.spriteBatch.ExitShaderRegion();
            }
            //projectile.DrawProjectileWithBackglowTemp(Color.Red, lightColor, (1f - projectile.Opacity) * 10f);
            return false;
        }

        public override void Kill(int timeLeft)
        {
            // Do funny screen stuff.
            Main.LocalPlayer.Infernum().CurrentScreenShakePower = 12f;
            //sus ScreenEffectSystem.SetFlashEffect(projectile.Center, 2f, 45);

            Main.PlaySound(InfernumMode.CalamityMod.GetLegacySoundSlot(SoundType.Custom, "Sounds/Custom/SCalSounds/BrimstoneGigablastImpact"), projectile.Center);

            Utilities.CreateShockwave(projectile.Center, 2, 8, 120f, false);

            Player target = Main.player[Player.FindClosest(projectile.Center, 1, 1)];
            float speedBoost = projectile.Distance(target.Center) * 0.009f;
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                for (int i = 0; i < 35; i++)
                {
                    Vector2 cinderVelocity = (MathHelper.TwoPi * i / 35f).ToRotationVector2() * (speedBoost + 13.5f);
                    Utilities.NewProjectileBetter(projectile.Center, cinderVelocity, ModContent.ProjectileType<DarkMagicFlame>(), CalamitasShadowBehaviorOverride.DarkMagicFlameDamage, 0f);
                }
            }

            for (int i = 0; i < 20; i++)
            {
                Color fireColor = Main.rand.NextBool() ? Color.Yellow : Color.Red;
                CloudParticle fireCloud = new CloudParticle(projectile.Center, (MathHelper.TwoPi * i / 20f).ToRotationVector2() * 9f, fireColor, Color.DarkGray, 45, Main.rand.NextFloat(1.9f, 2.3f));
                GeneralParticleHandler.SpawnParticle(fireCloud);
            }
        }
    }
}
