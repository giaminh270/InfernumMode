using CalamityMod;
using CalamityMod.NPCs.SupremeCalamitas;
using CalamityMod.Particles;
using CalamityMod.Projectiles.Boss;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.SupremeCalamitas
{
    public class SepulcherSoulBomb : ModProjectile
    {
        public int ExplodeCountdown;

        public PrimitiveTrailCopy FireDrawer;

        public ref float Time => ref projectile.ai[0];
        
        public ref float Radius => ref projectile.ai[1];

        public const int Lifetime = 360;

        public const float MaxRadius = 500f;

        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public override void SetStaticDefaults() => DisplayName.SetDefault("Wrathful Spirits");

        public override void SetDefaults()
        {
            projectile.width = projectile.height = 8;
            projectile.hostile = true;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;
            projectile.penetrate = -1;
            projectile.timeLeft = Lifetime;
            projectile.scale = 1f;
            projectile.hide = true;
            projectile.Calamity().canBreakPlayerDefense = true;
        }

        public override void SendExtraAI(BinaryWriter writer) => writer.Write(ExplodeCountdown);

        public override void ReceiveExtraAI(BinaryReader reader) => ExplodeCountdown = reader.ReadInt32();

        public override void AI()
        {
            Radius = MathHelper.Lerp(Radius, MaxRadius, 0.02f);
            projectile.Opacity = Utils.InverseLerp(8f, 42f, projectile.timeLeft, true) * 0.55f;

            Time++;

            // Decrement the explosion countdown if appliable.
            if (ExplodeCountdown > 0)
            {
                ExplodeCountdown--;
                if (ExplodeCountdown < 18)
                    Radius = MathHelper.Lerp(Radius, 25f, 0.15f);

                if (ExplodeCountdown <= 0)
                {
                	Main.PlaySound(InfernumMode.Instance.GetLegacySoundSlot(SoundType.Custom, "Sounds/Custom/WyrmElectricCharge"), projectile.Center);

                    int dartRingCount = 3;
                    int dartsPerRing = 15;

                    // Explode into a spread of darts, fire bursts, and souls.
                    for (int i = 0; i < 75; i++)
                    {
                        SquishyLightParticle fire = new SquishyLightParticle(projectile.Center, Main.rand.NextVector2Unit() * Main.rand.NextFloat(6f, 20f), 1f, Color.Orange, 64, 1.4f, 2.7f);
                        GeneralParticleHandler.SpawnParticle(fire);
                    }

                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        // Create darts.
                        for (int i = 0; i < dartRingCount; i++)
                        {
                            float dartSpeed = MathHelper.Lerp(8f, 3f, i / (float)(dartRingCount - 1f));
                            for (int j = 0; j < dartsPerRing; j++)
                            {
                                Vector2 dartVelocity = (MathHelper.TwoPi * j / dartsPerRing).ToRotationVector2() * dartSpeed;
                                if (i % 2 == 0)
                                    dartVelocity = dartVelocity.RotatedBy(MathHelper.Pi / dartsPerRing);
                                Utilities.NewProjectileBetter(projectile.Center, dartVelocity, ModContent.ProjectileType<BrimstoneBarrage>(), 500, 0f);
                            }
                            dartsPerRing += 4;
                        }
                        projectile.Kill();
                    }
                }
                return;
            }

            // If not exploding, hover in front of the Sepulcher's head.
            int sepulcherIndex = NPC.FindFirstNPC(ModContent.NPCType<SCalWormHead>());

            // Die if Sepulcher is not present.
            if (sepulcherIndex == -1)
            {
                projectile.active = false;
                return;
            }

            float mouthOffset = (float)Math.Pow(Radius / MaxRadius, 2.3) * MaxRadius * 0.92f + 45f;
            NPC sepulcher = Main.npc[sepulcherIndex];
            projectile.Center = sepulcher.Center + sepulcher.velocity.SafeNormalize((sepulcher.rotation - MathHelper.PiOver2).ToRotationVector2()) * mouthOffset;

            // Create charge-up particles.
            if (Radius < MaxRadius * 0.98f)
            {
                Vector2 magicVelocity = sepulcher.SafeDirectionTo(projectile.Center).RotatedByRandom(0.7f) * Main.rand.NextFloat(3f, 14f);
                var brimstoneMagic = new SquishyLightParticle(sepulcher.Center, magicVelocity, 1.6f, Color.Red, 70, 1f, 1.65f);
                GeneralParticleHandler.SpawnParticle(brimstoneMagic);
            }
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => Utilities.CircularCollision(targetHitbox.Center.ToVector2(), projHitbox, Radius * 0.72f);

        public override bool CanDamage() => ExplodeCountdown > 0;

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers)
        {
            behindNPCsAndTiles.Add(index);
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            int sideCount = 512;
            Utilities.GetCircleVertices(sideCount, Radius, projectile.Center, out var triangleIndices, out var vertices);

            CalamityUtils.CalculatePerspectiveMatricies(out Matrix view, out Matrix projection);
            GameShaders.Misc["Infernum:RealityTear"].SetShaderTexture(ModContent.GetTexture("InfernumMode/ExtraTextures/BrimstoneSoulLayer"));
            GameShaders.Misc["Infernum:RealityTear"].Shader.Parameters["uWorldViewProjection"].SetValue(view * projection);
            GameShaders.Misc["Infernum:RealityTear"].Shader.Parameters["useOutline"].SetValue(false);
            GameShaders.Misc["Infernum:RealityTear"].Shader.Parameters["uCoordinateZoom"].SetValue(3.2f);
            GameShaders.Misc["Infernum:RealityTear"].Shader.Parameters["uTimeFactor"].SetValue(3.2f);
            GameShaders.Misc["Infernum:RealityTear"].UseSaturation(10f);
            GameShaders.Misc["Infernum:RealityTear"].Apply();

            Main.instance.GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertices.ToArray(), 0, vertices.Count, triangleIndices.ToArray(), 0, sideCount * 2);
            Main.pixelShader.CurrentTechnique.Passes[0].Apply();
            return false;
        }
    }
}
