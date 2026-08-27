using InfernumMode;
using CalamityMod;
using CalamityMod.NPCs.ProfanedGuardians;
using CalamityMod.Particles;
using InfernumMode.Effects;
using InfernumMode.ExtraTextures;
using InfernumMode.Particles;
using InfernumMode.Projectiles;
using InfernumMode.Sounds;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.ProfanedGuardians
{
    public class ProfanedCirclingRock : ProfanedRock
    {
        public new float Timer => Lifetime - projectile.timeLeft;

        public float WaitTime;

        public const float ReelbackTime = 20;

        public int Lifetime => (int)(WaitTime + ReelbackTime + 240);

        public float RotationOffset => projectile.ai[0];

        public override void SetDefaults()
        {
            // These get changed later, but are this be default.
            base.SetDefaults();
            projectile.Opacity = 0;
            projectile.timeLeft = Lifetime;
        }

        public override void AI()
        {
            if (!Owner.active || Owner.type != ModContent.NPCType<ProfanedGuardianBoss2>())
            {
                projectile.Kill();
                return;
            }

            if (projectile.localAI[1] == 0f)
            {
                projectile.localAI[1] = 1f;
                projectile.timeLeft = Lifetime;
            }
            Player target = Main.player[Owner.target];

            projectile.Opacity = MathHelper.Clamp(projectile.Opacity + 0.05f, 0f, 1f);

            if (Timer < WaitTime)
                projectile.Center = projectile.Center.MoveTowards(Owner.Center - ((Timer / 15f) + RotationOffset).ToRotationVector2() * 100f, 30f);
            else if (Timer == WaitTime)
            {
                projectile.velocity = projectile.Center.DirectionTo(target.Center) * -3.2f;
                Main.PlaySound(InfernumSoundRegistry.VassalJumpSound, target.Center);
            }
            else if (Timer == WaitTime + ReelbackTime)
            {
                Main.PlaySound(SoundID.DD2_ExplosiveTrapExplode, target.Center);
                projectile.velocity = projectile.Center.DirectionTo(target.Center) * 17f;
                for (int i = 0; i < (InfernumConfig.Instance.ReducedGraphicsConfig ? 8 : 20); i++)
                {
                    Vector2 velocity = -projectile.velocity.SafeNormalize(Vector2.UnitY).RotatedBy(Main.rand.NextFloat(-0.15f, 0.15f)) * Main.rand.NextFloat(4f, 6f);
                    Particle rockParticle = new SandyDustParticle(projectile.Center + Main.rand.NextVector2Circular(projectile.width / 2f, projectile.height / 2f), velocity,
                        Color.SandyBrown, Main.rand.NextFloat(1.25f, 1.55f), 90);
                    GeneralParticleHandler.SpawnParticle(rockParticle);

                    Particle fire = new HeavySmokeParticle(projectile.Center + Main.rand.NextVector2Circular(projectile.width / 2f, projectile.height / 2f), Vector2.Zero,
                        Main.rand.NextBool() ? WayfinderSymbol.Colors[1] : WayfinderSymbol.Colors[2], 30, Main.rand.NextFloat(0.2f, 0.4f), 1f, glowing: true,
                        rotationSpeed: Main.rand.NextFromList(-1, 1) * 0.01f);
                    GeneralParticleHandler.SpawnParticle(fire);
                }
                if (!CalamityConfig.Instance.DisableScreenShakes)
                    target.Infernum().CurrentScreenShakePower = 2f;
            }
            if (Timer > WaitTime + ReelbackTime)
            {
                Particle rockParticle = new SandyDustParticle(projectile.Center + Main.rand.NextVector2Circular(projectile.width / 3f, projectile.height / 3f), Vector2.Zero,
                    Color.SandyBrown, Main.rand.NextFloat(0.45f, 0.75f), 30);
                GeneralParticleHandler.SpawnParticle(rockParticle);
                projectile.rotation -= 0.1f;
                if (Main.rand.NextBool() && Main.netMode != NetmodeID.Server)
                {
                    ModContent.GetTexture(Texture).CreateMetaballsFromTexture(ref InfernumFusableParticleManager.GetParticleSetByType<ProfanedLavaParticleSet>().Particles, projectile.Center - projectile.velocity * 0.5f, 0f, projectile.scale * 0.8f, 15f, 170);
                }
            }
        }

        public override bool CanHitPlayer(Player target) => Timer >= WaitTime;

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Texture2D texture = ModContent.GetTexture(Texture);
            Vector2 drawPosition = projectile.Center - Main.screenPosition;
            Vector2 origin = texture.Size() * 0.5f;

            if (Timer >= WaitTime && Timer < WaitTime + ReelbackTime)
            {
                Texture2D invis = InfernumTextureRegistry.Invisible;
                float opacity = (float)Math.Sin((Timer - WaitTime) / ReelbackTime * (float)Math.PI);
                Effect laserScopeEffect = InfernumEffectsRegistry.PixelatedSightLine.GetShader().Shader;
                laserScopeEffect.Parameters["sampleTexture2"].SetValue(ModContent.GetTexture("InfernumMode/ExtraTextures/GreyscaleGradients/CertifiedCrustyNoise"));
                laserScopeEffect.Parameters["noiseOffset"].SetValue(Main.GameUpdateCount * -0.003f);
                laserScopeEffect.Parameters["mainOpacity"].SetValue((float)Math.Pow(opacity, 0.5f));
                laserScopeEffect.Parameters["Resolution"].SetValue(new Vector2(340f));
                Player target = Main.player[Owner.target];
                laserScopeEffect.Parameters["laserAngle"].SetValue((target.Center - projectile.Center).ToRotation() * -1f);
                laserScopeEffect.Parameters["laserWidth"].SetValue(0.0025f + (float)Math.Pow(opacity, 5f) * ((float)Math.Sin(Main.GlobalTime * 3f) * 0.002f + 0.002f));
                laserScopeEffect.Parameters["laserLightStrenght"].SetValue(3f);
                laserScopeEffect.Parameters["color"].SetValue(Color.Lerp(WayfinderSymbol.Colors[1], Color.OrangeRed, 0.5f).ToVector3());
                laserScopeEffect.Parameters["darkerColor"].SetValue(WayfinderSymbol.Colors[2].ToVector3());
                laserScopeEffect.Parameters["bloomSize"].SetValue(0.06f + (1f - opacity) * 0.1f);
                laserScopeEffect.Parameters["bloomMaxOpacity"].SetValue(0.4f);
                laserScopeEffect.Parameters["bloomFadeStrenght"].SetValue(3f);

                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.instance.Rasterizer, laserScopeEffect, Main.GameViewMatrix.TransformationMatrix);
                Main.spriteBatch.Draw(invis, drawPosition, null, Color.White, 0f, invis.Size() * 0.5f, 1500f, SpriteEffects.None, 0f);
                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.instance.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            }

            Color backglowColor = Color.Lerp(WayfinderSymbol.Colors[0], WayfinderSymbol.Colors[1], 0.5f);
            backglowColor.A = 0;
            float backglowAmount = 12;
            for (int i = 0; i < backglowAmount; i++)
            {
                Vector2 backglowOffset = (MathHelper.TwoPi * i / backglowAmount).ToRotationVector2() * 4f;
                Main.spriteBatch.Draw(texture, drawPosition + backglowOffset, null, backglowColor * projectile.Opacity, projectile.rotation, texture.Size() * 0.5f, projectile.scale, SpriteEffects.None, 0);
            }
            Main.spriteBatch.Draw(texture, drawPosition, null, projectile.GetAlpha(lightColor) * projectile.Opacity, projectile.rotation, origin, projectile.scale, SpriteEffects.None, 0);
            if (Timer >= WaitTime - 30)
            {
                float opacityScalar = (1f + (float)Math.Sin((Timer - WaitTime / 30) / (WaitTime + ReelbackTime) - (Timer - WaitTime / 30) * 2 * (float)Math.PI)) / 2f;
                backglowColor = Color.Lerp(backglowColor, Color.OrangeRed, opacityScalar);
                for (int i = 0; i < 3; i++)
                    Main.spriteBatch.Draw(texture, drawPosition, null, backglowColor * projectile.Opacity * opacityScalar, projectile.rotation, origin, projectile.scale, SpriteEffects.None, 0);
            }
            return false;
        }
    }
}
