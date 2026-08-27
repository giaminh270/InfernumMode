using CalamityMod;
using CalamityMod.NPCs.ProfanedGuardians;
using CalamityMod.NPCs.Providence;
using CalamityMod.Particles;
using InfernumMode.Effects;
using InfernumMode.ExtraTextures;
using InfernumMode.Sounds;
using InfernumMode.Particles;
using InfernumMode.BehaviorOverrides.BossAIs.Providence;
using InfernumMode.Projectiles;
using InfernumMode.GlobalInstances;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using System;
using Terraria.ID;
using Terraria.ModLoader;
using static InfernumMode.BehaviorOverrides.BossAIs.ProfanedGuardians.GuardianComboAttackManager;

namespace InfernumMode.Projectiles
{
    public class GuardiansSummonerProjectile : ModProjectile
    {
        public ref float Time => ref projectile.ai[0];

        public ref float GuardsGlowAmount => ref projectile.ai[1];

        public const int Lifetime = 390;

        public const int MoveTime = 30;

        public static int DustTime => 50;

        public const int FireballCreateTime = 75;

        public const int SpawnTime = 300;

        public static Vector2 MainPosition => CrystalPosition + new Vector2(300f, 150f);

        public static Vector2 ProviLightPosition => MainPosition + new Vector2(1000f, -65f);

        public static Player Player => Main.LocalPlayer;

        private Vector2 DefaultPlayerPosition;

        public float FireballScale
        {
            get => projectile.Opacity;
            set => projectile.Opacity = value;
        }

        public float GuardiansScale
        {
            get => projectile.scale;
            set => projectile.scale = value;
        }

        public static Texture2D CommanderTexture => ModContent.GetTexture("InfernumMode/BehaviorOverrides/BossAIs/ProfanedGuardians/Textures/CommanderTexture");
        public static Texture2D DefenderTexture => ModContent.GetTexture("InfernumMode/BehaviorOverrides/BossAIs/ProfanedGuardians/Textures/DefenderTexture");
        public static Texture2D HealerTexture => ModContent.GetTexture("InfernumMode/BehaviorOverrides/BossAIs/ProfanedGuardians/Textures/HealerTexture");

        public override string Texture => "CalamityMod/Items/SummonItems/ProfanedShard";

        public override void SetDefaults()
        {
            projectile.width = 24;
            projectile.height = 24;
            projectile.aiStyle = -1;
            projectile.ignoreWater = true;
            projectile.tileCollide = false;
            projectile.timeLeft = Lifetime;
            projectile.Opacity = 0f;
            projectile.scale = 0f;
            projectile.penetrate = -1;
        }

        public override void AI()
        {
            if (Time == 0)
            {
                Player.Infernum().ScreenFocusHoldInPlaceTime = Lifetime;

                DefaultPlayerPosition = Player.Center;

                // If infernum is not enabled, just spawn the guardians.
                if (Main.netMode != NetmodeID.MultiplayerClient && !PoDWorld.InfernumMode)
                {
                    NPC.SpawnOnPlayer(Main.player[projectile.owner].whoAmI, ModContent.NPCType<ProfanedGuardianBoss>());
                    projectile.Kill();
                    return;
                }

            }

            Player.Center = DefaultPlayerPosition;

            float particleCircleSize = MathHelper.Lerp(500f, 200f, Time / SpawnTime);
            int rockSpawnRate = (int)MathHelper.Lerp(3f, 1f, Time / SpawnTime);

            if (Time >= MoveTime && Time <= SpawnTime)
            {
                for (int i = 0; i < 3; i++)
                {
                    Vector2 basePos;
				    switch (i)
				    {
				        case 1:
				            basePos = DefenderStartingHoverPosition;
				            break;
				        case 2:
				            basePos = HealerStartingHoverPosition;
				            break;
				        default:
				            basePos = CommanderStartingHoverPosition;
				            break;
				    }
                    if (Time % rockSpawnRate == 0)
                    {
                        Vector2 position = basePos + Main.rand.NextVector2Circular(particleCircleSize, particleCircleSize);
                        int lifeTime = 30;
                        Vector2 velocity = position.DirectionTo(basePos) * (Vector2.Distance(position, basePos) / (lifeTime));
                        ProfanedRockParticle rock = new ProfanedRockParticle(position, velocity, Color.White, Main.rand.NextFloat(1.2f, 1.5f), lifeTime, gravity: false, fadeIn: true);
                        GeneralParticleHandler.SpawnParticle(rock);
                    }
                    for (int j = 0; j < 1; j++)
                    {
                        Vector2 position = basePos + Main.rand.NextVector2Circular(50f, 50f);
                        var fire = new MediumMistParticle(position, Vector2.Zero, WayfinderSymbol.Colors[1], Color.Gray, Main.rand.NextFloat(0.8f, 1.2f), 210f);
                        GeneralParticleHandler.SpawnParticle(fire);
                    }

                    if (Time <= DustTime)
                    {
                        Color energyColor = Color.Lerp(Color.Yellow, WayfinderSymbol.Colors[1], Main.rand.NextFloat(0.7f));
                        Vector2 energySpawnPosition = basePos + Main.rand.NextVector2Unit() * Main.rand.NextFloat(116f, 166f);
                        Vector2 energyVelocity = (basePos - energySpawnPosition) * 0.032f;
                        SquishyLightParticle laserEnergy = new SquishyLightParticle(energySpawnPosition, energyVelocity, 0.5f, energyColor, 32, 1f, 5f);
                        GeneralParticleHandler.SpawnParticle(laserEnergy);

                        // Connect the temple to the guardian positions via dust.
                        float sineValue = (1f + (float)Math.Sin(Main.GlobalTime * 10.5f)) / 2f;
                        float completion = Utilities.SineInOutEasing(sineValue, 1);
                        Dust dust = Dust.NewDustPerfect(ProviLightPosition, 264);
                        dust.velocity = Vector2.UnitY.RotatedByRandom(0.17000000178813934) * (0f - Main.rand.NextFloat(2.7f, 4.1f));
                        dust.color = WayfinderSymbol.Colors[2];
                        dust.noLight = true;
                        dust.fadeIn = 0.6f;
                        dust.noGravity = true;
                        for (int j = 1; j <= 10; j++)
                        {
                            float actualCompletion = completion * (1f / j);
                            Dust dust2 = Dust.NewDustPerfect(Vector2.CatmullRom(ProviLightPosition + Vector2.UnitY * 500f, ProviLightPosition, basePos, basePos + Vector2.UnitY * 500f, actualCompletion), 267);
                            dust2.scale = 1.67f;
                            dust2.velocity = Main.rand.NextVector2CircularEdge(0.2f, 0.2f);
                            dust2.fadeIn = 0.67f;
                            dust2.color = WayfinderSymbol.Colors[1];
                            dust2.noGravity = true;
                        }
                    }
                }
            }

            // Play a rumble sound.
            if (Time == FireballCreateTime)
                Main.PlaySound(InfernumSoundRegistry.LeviathanRumbleSound);
            if (Time >= FireballCreateTime && Time <= SpawnTime)
            {
                Player.Infernum().CreateALotOfHolyCinders = true;
                GuardiansScale = MathHelper.Clamp(GuardiansScale + 0.005f, 0f, 1f);
                FireballScale = MathHelper.Clamp(FireballScale + 0.01f, 0f, 1f);
            }

            if (Player.WithinRange(MainPosition, 20000))
            {
                Player.Infernum().ScreenFocusPosition = MainPosition;
                Player.Infernum().ScreenFocusInterpolant = Utilities.SineInOutEasing(MathHelper.Clamp(Time / MoveTime, 0f, 1f), 0);

                // Disable input and UI during the animation.
                Main.blockInput = true;
                Main.hideUI = true;
            }

            if (Time >= 210f && Time <= SpawnTime)
            {
                // Create screen shake effects.
                Player.Infernum().CurrentScreenShakePower = 3;
                GuardsGlowAmount = MathHelper.Clamp(GuardsGlowAmount + 0.025f, 0f, 1f);

                if (Time == 250)
                    Main.PlaySound(InfernumSoundRegistry.GuardianRockShieldDeactivate);
            }

            if (Time == SpawnTime)
            {
                Player.Infernum().CurrentScreenShakePower = 20f;

                // Make the crystal shatter.
                Main.PlaySound(InfernumMode.CalamityMod.GetLegacySoundSlot(SoundType.NPCHit, "Sounds/NPCHit/ProvidenceHurt"));
                Main.PlaySound(InfernumSoundRegistry.ProvidenceBurnSound);
                Main.PlaySound(InfernumSoundRegistry.ProvidenceLavaEruptionSound);

                ScreenEffectSystem.SetBlurEffect(MainPosition, 1f, 45);

                // Create an explosion and summon the Guardian Commander.
                if (Main.netMode != NetmodeID.MultiplayerClient)
                    CalamityUtils.SpawnBossBetter(CommanderStartingHoverPosition + new Vector2(20f, 90f), ModContent.NPCType<ProfanedGuardianBoss>());
                for (int i = 0; i < 3; i++)
                {
                    Vector2 basePos;
                    switch (i)
                    {
                        case 1:
                            basePos = DefenderStartingHoverPosition;
                            break;
                        case 2:
                            basePos = HealerStartingHoverPosition;
                            break;
                        default:
                            basePos = CommanderStartingHoverPosition;
                            break;
                    }
                    CreateSpawnExplosion(basePos);
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        ProjectileSpawnManagementSystem.PrepareProjectileForSpawning(explosion =>
                        {
                            explosion.ModProjectile<HolySunExplosion>().MaxRadius = 200f;
                        });
                        Utilities.NewProjectileBetter(basePos, Vector2.Zero, ModContent.ProjectileType<HolySunExplosion>(), 0, 0f);
                    }
                }
            }

            if (Time >= SpawnTime + 30f)
            {
                GuardiansScale = MathHelper.Clamp(GuardiansScale - 0.1f, 0f, 1f);
                FireballScale = MathHelper.Clamp(FireballScale - 0.1f, 0f, 1f);
                GuardsGlowAmount = MathHelper.Clamp(GuardsGlowAmount - 0.1f, 0f, 1f);
            }
            Time++;
        }

        public override void Kill(int timeLeft)
        {
            Main.blockInput = false;
            Main.hideUI = false;
        }

        private static void CreateSpawnExplosion(Vector2 impactCenter)
        {
            // Create a fire explosion.
            for (int i = 0; i < 30; i++)
            {
                Vector2 position = impactCenter + Main.rand.NextVector2Circular(100f, 100f);
                CloudParticle fireExplosion = new CloudParticle(position, impactCenter.DirectionTo(position) * Main.rand.NextFloat(2f, 3f),
                    Main.rand.NextBool() ? WayfinderSymbol.Colors[0] : WayfinderSymbol.Colors[1],
                    Color.Gray, 120, Main.rand.NextFloat(0.85f, 1.35f), true);
                GeneralParticleHandler.SpawnParticle(fireExplosion);
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            DrawProviLight();
            if (FireballScale > 0f)
                DrawFireballs();
            if (GuardiansScale > 0f)
                DrawGuardians();
            return false;
        }

        private void DrawProviLight()
        {
            Texture2D light = ModContent.GetTexture("InfernumMode/BehaviorOverrides/BossAIs/Polterghast/Light");
            float opacity = MathHelper.Lerp(0f, 1f, FireballScale);
            if (Time <= SpawnTime)
                opacity = MathHelper.Clamp(opacity, 0.5f, 1f);

            Vector2 drawPosition = ProviLightPosition - Main.screenPosition;
            Color lightColor = WayfinderSymbol.Colors[1];
            Color coloredLight = WayfinderSymbol.Colors[0];

            Main.spriteBatch.Draw(light, drawPosition, null, lightColor * opacity, -MathHelper.PiOver2, new Vector2(265f, 354f), new Vector2(1.3f, 1.25f), SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(light, drawPosition, null, coloredLight * opacity, -MathHelper.PiOver2, new Vector2(265f, 354f), new Vector2(1.3f, 1.25f), SpriteEffects.None, 0f);

            Texture2D lightTexture = Main.extraTexture[59];
            Main.spriteBatch.Draw(lightTexture, drawPosition + new Vector2(-40f, 0f), null, Color.Lerp(lightColor, coloredLight, 0.5f) * opacity * 0.85f, -MathHelper.PiOver2, lightTexture.Size() * 0.5f, new Vector2(2.5f, 2.7f), SpriteEffects.None, 0f);
        }

        private void DrawFireballs()
        {
            Texture2D invis = InfernumTextureRegistry.Invisible;
            Texture2D noise = InfernumTextureRegistry.HarshNoise;
            Effect fireball = InfernumEffectsRegistry.FireballShader.GetShader().Shader;

            fireball.Parameters["sampleTexture2"].SetValue(noise);
            fireball.Parameters["mainColor"].SetValue(Color.Lerp(WayfinderSymbol.Colors[1], WayfinderSymbol.Colors[2], 0.3f).ToVector3());
            fireball.Parameters["resolution"].SetValue(new Vector2(250f, 250f));
            fireball.Parameters["speed"].SetValue(0.76f);
            fireball.Parameters["time"].SetValue(Main.GlobalTime);
            fireball.Parameters["zoom"].SetValue(0.0004f);
            fireball.Parameters["dist"].SetValue(60f);
            fireball.Parameters["opacity"].SetValue(1f);

            float scale = 250f * FireballScale;

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.instance.Rasterizer, fireball, Main.GameViewMatrix.TransformationMatrix);
            Main.spriteBatch.Draw(invis, CommanderStartingHoverPosition - Main.screenPosition, null, Color.White, projectile.rotation, invis.Size() * 0.5f, scale, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(invis, DefenderStartingHoverPosition - Main.screenPosition, null, Color.White, projectile.rotation, invis.Size() * 0.5f, scale, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(invis, HealerStartingHoverPosition - Main.screenPosition, null, Color.White, projectile.rotation, invis.Size() * 0.5f, scale, SpriteEffects.None, 0f);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.instance.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        }

        private void DrawGuardians()
        {
            float smoothed = Utilities.SineInEasing(GuardsGlowAmount, 0);
            float radius = MathHelper.Lerp(0f, 10f, smoothed);
            if (radius > 0.5f)
            {
                for (int i = 0; i < 10; i++)
                {
                    Vector2 drawOffset = (MathHelper.TwoPi * i / 24f).ToRotationVector2().RotatedBy(Main.GlobalTime * 4) * radius;
                    Color backimageColor = Color.Lerp(Color.Brown, WayfinderSymbol.Colors[0], smoothed);
                    backimageColor.A = (byte)MathHelper.Lerp(0f, 10f, smoothed);
                    for (int j = 0; j < 3; j++)
                    {
                        Vector2 drawPosition;
                        switch (j)
                        {
                            case 1:
                                drawPosition = DefenderStartingHoverPosition;
                                break;
                            case 2:
                                drawPosition = HealerStartingHoverPosition;
                                break;
                            default:
                                drawPosition = CommanderStartingHoverPosition;
                                break;
                        }
                        drawPosition -= Main.screenPosition;

                        Texture2D texture;
                        switch (j)
                        {
                            case 1:
                                texture = DefenderTexture;
                                break;
                            case 2:
                                texture = HealerTexture;
                                break;
                            default:
                                texture = CommanderTexture;
                                break;
                        }
                        Main.spriteBatch.Draw(texture, drawPosition + drawOffset, null, backimageColor * smoothed, 0f, texture.Size() * 0.5f, GuardiansScale * 0.8f, 0, 0f);
                    }
                }
            }
        }
    }
}
