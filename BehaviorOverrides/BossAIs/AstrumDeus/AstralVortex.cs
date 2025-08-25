using CalamityMod;
using CalamityMod.Events;
using CalamityMod.NPCs.AstrumDeus;
using CalamityMod.Particles;
using CalamityMod.Sounds;
using InfernumMode.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Enums;

namespace InfernumMode.BehaviorOverrides.BossAIs.AstrumDeus
{
    public class AstralVortex : ModProjectile
    {
        public int FlameSpawnRate;

        public bool Cyan => projectile.localAI[0] == 1f;

        public ref float Timer => ref projectile.ai[0];

        public ref float OtherVortexIndex => ref projectile.ai[1];

        public Player Target => Main.player[projectile.owner];

        public const int ScaleFadeinTime = 95;

        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public override void SetStaticDefaults() => DisplayName.SetDefault("Consumed Vortex");

        public override void SetDefaults()
        {
            projectile.width = projectile.height = 160;
            projectile.hostile = true;
            projectile.ignoreWater = true;
            projectile.tileCollide = false;
            projectile.penetrate = -1;
            projectile.timeLeft = 1080;
            projectile.scale = 1f;
            projectile.Calamity().canBreakPlayerDefense = true;
            cooldownSlot = 1;
        }

        public override void SendExtraAI(BinaryWriter writer) => writer.Write(FlameSpawnRate);

        public override void ReceiveExtraAI(BinaryReader reader) => FlameSpawnRate = reader.ReadInt32();

        public override void AI()
        {
            // Die if Astrum Deus is not present or is dead.
            if (!NPC.AnyNPCs(ModContent.NPCType<AstrumDeusHeadSpectral>()))
            {
                projectile.Kill();
                return;
            }

            Projectile otherVortex = Main.projectile[(int)OtherVortexIndex];
            projectile.scale = Utilities.UltrasmoothStep(Timer / ScaleFadeinTime) * 2f + Utilities.UltrasmoothStep(Timer / ScaleFadeinTime * 3.2f) * 0.34f;
            projectile.scale = MathHelper.Lerp(projectile.scale, 0f, Utils.InverseLerp(91690f, 91720f, Timer, true));
            projectile.Opacity = MathHelper.Clamp(projectile.scale * 0.87f, 0f, 1f);

            // Move towards the nearest player and try to stay near the other 
            if (projectile.velocity.Length() > 0.001f)
            {
                float generalSpeedFactor = 1f;
                float flameSpeed = 9f;
                float flyTogetherInterpolant = Utils.InverseLerp(ScaleFadeinTime + 180f, ScaleFadeinTime + 225f, Timer, true);

                if (BossRushEvent.BossRushActive)
                {
                    generalSpeedFactor = 1.55f;
                    flameSpeed = 16f;
                }

                if (!projectile.WithinRange(otherVortex.Center, MathHelper.Clamp(1100f - Timer * 2f, 100f, 1100f)))
                    projectile.velocity += projectile.SafeDirectionTo(otherVortex.Center) * generalSpeedFactor * 1.45f;

                if (projectile.velocity.Length() < generalSpeedFactor * 17f)
                {
                    Vector2 vortexOffset = otherVortex.Center - projectile.Center;
                    if (Math.Abs(vortexOffset.X) < 0.01f)
                        vortexOffset.X = 0.01f;
                    if (Math.Abs(vortexOffset.Y) < 0.01f)
                        vortexOffset.Y = 0.01f;

                    float minPushSpeed = MathHelper.Lerp(0.02f, 0.08f, flyTogetherInterpolant);
                    Vector2 force = (Vector2.One * (flyTogetherInterpolant * 3f + 1f) * 0.4f / vortexOffset + projectile.SafeDirectionTo(otherVortex.Center) * minPushSpeed * 0.25f).ClampMagnitude(minPushSpeed, 20f);
                    projectile.velocity += force * generalSpeedFactor + projectile.SafeDirectionTo(Target.Center) * generalSpeedFactor * 0.24f;
                }
                else
                    projectile.velocity *= 0.9f;

                // Idly create flames.
                if (FlameSpawnRate >= 1f && Timer % FlameSpawnRate == FlameSpawnRate - 1)
                {
                    Vector2 crystalSpawnPosition = projectile.Center + Main.rand.NextVector2Unit() * Main.rand.NextFloat(50f, 200f) * projectile.scale;
                    if (!Main.player[Player.FindClosest(crystalSpawnPosition, 1, 1)].WithinRange(crystalSpawnPosition, 300f))
                    {
                        Main.PlaySound(SoundID.Item92, projectile.Center);
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            Vector2 crystalVelocity = (crystalSpawnPosition - projectile.Center).SafeNormalize(Vector2.UnitY).RotatedBy(MathHelper.PiOver2) * flameSpeed;
                            Utilities.NewProjectileBetter(crystalSpawnPosition, crystalVelocity, ModContent.ProjectileType<AstralFlame2>(), 200, 0f);
                        }
                    }
                }

                // Explode if very close and merging.
                if (projectile.WithinRange(otherVortex.Center, 125f) && flyTogetherInterpolant >= 0.75f)
                {
					Main.PlaySound(InfernumMode.CalamityMod.GetLegacySoundSlot(SoundType.Item, "Sounds/Custom/FlareSound"), (int)projectile.position.X, (int)projectile.position.Y);
                	Main.PlaySound(InfernumMode.CalamityMod.GetLegacySoundSlot(SoundType.Custom, "Sounds/Custom/WyrmElectricCharge"), projectile.Center);

                    // Create a bunch of sparkles, along with a circular spread of astral flames.
                    Vector2 impactPoint = (projectile.Center + otherVortex.Center) * 0.5f;
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        for (int i = 0; i < 75; i++)
                        {
                            Vector2 sparkleVelocity = Main.rand.NextVector2Circular(67f, 67f);
                            Utilities.NewProjectileBetter(impactPoint, sparkleVelocity, ModContent.ProjectileType<AstralSparkle>(), 0, 0f);
                        }

                        for (int i = 0; i < 9; i++)
                        {
                            Vector2 flameVelocity = (MathHelper.TwoPi * i / 9f).ToRotationVector2() * 10f;
                            Utilities.NewProjectileBetter(impactPoint, flameVelocity, ModContent.ProjectileType<AstralFlame2>(), 200, 0f);
                        }
                    }
                    Color[] explosionColors = new Color[]
                    {
                        new Color(250, 90, 74, 127),
                        new Color(76, 255, 194, 127)
                    };
                    GeneralParticleHandler.SpawnParticle(new ElectricExplosionRing(impactPoint, Vector2.Zero, explosionColors, 3f, 180, 1.4f));

                    projectile.Kill();
                    otherVortex.Kill();
                }
            }

            Timer++;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            return CalamityUtils.CircularHitboxCollision(projectile.Center, projectile.scale * 80f, targetHitbox);
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Texture2D noiseTexture = ModContent.GetTexture("CalamityMod/ExtraTextures/VoronoiShapes");
            Vector2 drawPosition = projectile.Center - Main.screenPosition;
            Vector2 origin = noiseTexture.Size() * 0.5f;
            Main.spriteBatch.EnterShaderRegion();

            Color fadedColor = Cyan ? Color.LightCyan : Color.Orange;
            Color primaryColor = Cyan ? new Color(109, 242, 196) : new Color(237, 93, 83);

            Vector2 diskScale = projectile.scale * new Vector2(1f, 0.85f);
            GameShaders.Misc["CalamityMod:DoGPortal"].UseOpacity(projectile.Opacity);
            GameShaders.Misc["CalamityMod:DoGPortal"].UseColor(fadedColor);
            GameShaders.Misc["CalamityMod:DoGPortal"].UseSecondaryColor(fadedColor);
            GameShaders.Misc["CalamityMod:DoGPortal"].Apply();

            Main.spriteBatch.Draw(noiseTexture, drawPosition, null, Color.White, 0f, origin, diskScale, 0, 0f);
            GameShaders.Misc["CalamityMod:DoGPortal"].UseOpacity(projectile.Opacity);
            GameShaders.Misc["CalamityMod:DoGPortal"].UseColor(primaryColor);
            GameShaders.Misc["CalamityMod:DoGPortal"].UseSecondaryColor(primaryColor);
            GameShaders.Misc["CalamityMod:DoGPortal"].Apply();

            for (int i = 0; i < 3; i++)
                Main.spriteBatch.Draw(noiseTexture, drawPosition, null, Color.White, 0f, origin, diskScale, 0, 0f);
            Main.spriteBatch.ExitShaderRegion();
            return false;
        }
    }
}
