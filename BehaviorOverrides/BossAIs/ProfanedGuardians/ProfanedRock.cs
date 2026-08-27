using InfernumMode;
using CalamityMod;
using CalamityMod.Particles;
using InfernumMode.Effects;
using InfernumMode.ExtraTextures;
using InfernumMode.Particles;
using InfernumMode.Projectiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.ProfanedGuardians
{
    public class ProfanedRock : ModProjectile
    {
        public enum RockType
        {
            Aimed,
            Accelerating,
            Gravity
        }

        public static string[] Textures => new string[4]
        {
            "ProfanedRock",
            "ProfanedRock2",
            "ProfanedRock3",
            "ProfanedRock4",
        };

        public string CurrentVarient = Textures[0];

        public int RedHotGlowTimer = 30;

        public int RockTypeVarient = (int)RockType.Aimed;

        public bool DoNotDrawLine;

        public ref float Timer => ref projectile.ai[0];

        public NPC Owner => Main.npc[(int)projectile.ai[1]];

        public override string Texture => "InfernumMode/BehaviorOverrides/BossAIs/ProfanedGuardians/Rocks/" + CurrentVarient;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Profaned Rock");
            ProjectileID.Sets.TrailingMode[projectile.type] = 2;
            ProjectileID.Sets.TrailCacheLength[projectile.type] = 6;
        }

        public override void SetDefaults()
        {
            // These get changed later, but are this be default.
            projectile.width = 42;
            projectile.height = 36;

            projectile.friendly = false;
            projectile.hostile = true;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;
            projectile.penetrate = 1;
            projectile.Opacity = 1;
            projectile.timeLeft = 240;
            cooldownSlot = 1;
        }

        public override void AI()
        {
            if (!Owner.active)
            {
                projectile.Kill();
                return;
            }

            if (projectile.localAI[0] == 0)
            {
                projectile.localAI[0] = 1;
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    int varient = Main.rand.Next(4);
                    switch (varient)
                    {
                        case 0:
                            CurrentVarient = Textures[varient];
                            break;
                        case 1:
                            CurrentVarient = Textures[varient];
                            projectile.width = 34;
                            projectile.height = 38;
                            break;
                        case 2:
                            CurrentVarient = Textures[varient];
                            projectile.width = 36;
                            projectile.height = 46;
                            break;
                        case 3:
                            CurrentVarient = Textures[varient];
                            projectile.width = 28;
                            projectile.height = 36;
                            break;
                    }
                    projectile.netUpdate = true;
                }

                if ((RockType)RockTypeVarient == RockType.Gravity)
                {
                    RedHotGlowTimer = 120;
                    projectile.timeLeft = 360;
                }
            }

            Player target = Main.player[Owner.target];

            switch ((RockType)RockTypeVarient)
            {
                case RockType.Aimed:
                    if (Timer == 0)
                    {
                        Main.PlaySound(SoundID.DD2_ExplosiveTrapExplode, target.Center);
                        for (int i = 0; i < (InfernumConfig.Instance.ReducedGraphicsConfig ? 8 : 20); i++)
                        {
                            Vector2 velocity = -projectile.velocity.SafeNormalize(Vector2.UnitY).RotatedBy(Main.rand.NextFloat(-0.15f, 0.15f)) * Main.rand.NextFloat(4f, 6f);
                            Particle rock = new SandyDustParticle(projectile.Center + Main.rand.NextVector2Circular(projectile.width / 2f, projectile.height / 2f), velocity, Color.SandyBrown,
                                Main.rand.NextFloat(1.25f, 1.55f), 90);
                            GeneralParticleHandler.SpawnParticle(rock);

                            Particle fire = new HeavySmokeParticle(projectile.Center + Main.rand.NextVector2Circular(projectile.width / 2f, projectile.height / 2f), Vector2.Zero,
                                Main.rand.NextBool() ? WayfinderSymbol.Colors[1] : WayfinderSymbol.Colors[2], 30, Main.rand.NextFloat(0.2f, 0.4f), 1f, glowing: true,
                                rotationSpeed: Main.rand.NextFromList(-1, 1) * 0.01f);
                            GeneralParticleHandler.SpawnParticle(fire);
                        }
                        if (!CalamityConfig.Instance.DisableScreenShakes)
                            target.Infernum().CurrentScreenShakePower = 2f;
                    }
                    break;

                case RockType.Accelerating:
                    if (projectile.velocity.Length() < 30f)
                        projectile.velocity *= 1.035f;
                    break;

                case RockType.Gravity:
                    if (projectile.velocity.Y < 16f)
                    {
                        projectile.velocity.X *= 0.995f;
                        projectile.velocity.Y += 0.35f;
                    }
                    break;
            }

            Particle rockParticle = new SandyDustParticle(projectile.Center + Main.rand.NextVector2Circular(projectile.width / 3f, projectile.height / 3f), Vector2.Zero, Color.SandyBrown,
                Main.rand.NextFloat(0.45f, 0.75f), 30);
            GeneralParticleHandler.SpawnParticle(rockParticle);

            if (Main.rand.NextBool() && Main.netMode != NetmodeID.Server)
            {
                ModContent.GetTexture(Texture).CreateMetaballsFromTexture(ref InfernumFusableParticleManager.GetParticleSetByType<ProfanedLavaParticleSet>().Particles,
                    projectile.Center + projectile.velocity * 0.5f, 0f, projectile.scale * 0.8f, 12f, 190);
            }

            projectile.rotation -= 0.1f;
            Timer++;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Texture2D texture = ModContent.GetTexture(Texture);
            Vector2 drawPosition = projectile.Center - Main.screenPosition;
            Vector2 origin = texture.Size() * 0.5f;

            Color backglowColor = Color.Lerp(WayfinderSymbol.Colors[0], WayfinderSymbol.Colors[1], 0.5f);
            backglowColor.A = 0;

            if (Timer <= RedHotGlowTimer && !DoNotDrawLine)
            {
                Texture2D invis = InfernumTextureRegistry.Invisible;
                float opacity = (float)Math.Sin(Timer / RedHotGlowTimer * (float)Math.PI);
                Effect laserScopeEffect = InfernumEffectsRegistry.PixelatedSightLine.GetShader().Shader;
                laserScopeEffect.Parameters["sampleTexture2"].SetValue(ModContent.GetTexture("InfernumMode/ExtraTextures/GreyscaleGradients/CertifiedCrustyNoise"));
                laserScopeEffect.Parameters["noiseOffset"].SetValue(Main.GameUpdateCount * -0.003f);
                laserScopeEffect.Parameters["mainOpacity"].SetValue((float)Math.Pow(opacity, 0.5f));
                laserScopeEffect.Parameters["Resolution"].SetValue(new Vector2(340f));
                laserScopeEffect.Parameters["laserAngle"].SetValue(projectile.velocity.ToRotation() * -1f);
                laserScopeEffect.Parameters["laserWidth"].SetValue(0.005f + (float)Math.Pow(opacity, 5f) * ((float)Math.Sin(Main.GlobalTime * 3f) * 0.002f + 0.002f));
                laserScopeEffect.Parameters["laserLightStrenght"].SetValue(5f);
                laserScopeEffect.Parameters["color"].SetValue(Color.Lerp(WayfinderSymbol.Colors[1], Color.OrangeRed, 0.5f).ToVector3());
                laserScopeEffect.Parameters["darkerColor"].SetValue(WayfinderSymbol.Colors[2].ToVector3());
                laserScopeEffect.Parameters["bloomSize"].SetValue(0.06f + (1f - opacity) * 0.1f);
                laserScopeEffect.Parameters["bloomMaxOpacity"].SetValue(0.4f);
                laserScopeEffect.Parameters["bloomFadeStrenght"].SetValue(3f);

                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.instance.Rasterizer, laserScopeEffect, Main.GameViewMatrix.TransformationMatrix);
                Main.spriteBatch.Draw(invis, drawPosition, null, Color.White, 0f, invis.Size() * 0.5f, 750f, SpriteEffects.None, 0f);
                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.instance.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            }

            float backglowAmount = 12;
            for (int i = 0; i < backglowAmount; i++)
            {
                Vector2 backglowOffset = (MathHelper.TwoPi * i / backglowAmount).ToRotationVector2() * 4f;
                Main.spriteBatch.Draw(texture, drawPosition + backglowOffset, null, backglowColor * projectile.Opacity, projectile.rotation, texture.Size() * 0.5f, projectile.scale, SpriteEffects.None, 0);
            }
            Main.spriteBatch.Draw(texture, drawPosition, null, projectile.GetAlpha(lightColor) * projectile.Opacity, projectile.rotation, origin, projectile.scale, SpriteEffects.None, 0);
            if (Timer <= RedHotGlowTimer)
            {
                float interpolant = Timer / RedHotGlowTimer;
                backglowColor = Color.OrangeRed * (1 - interpolant);
                for (int i = 0; i < 3; i++)
                    Main.spriteBatch.Draw(texture, drawPosition, null, backglowColor * projectile.Opacity, projectile.rotation, origin, projectile.scale, SpriteEffects.None, 0);
            }
            return false;
        }
    }
}
