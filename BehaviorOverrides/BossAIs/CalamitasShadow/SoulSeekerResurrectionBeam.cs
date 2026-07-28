using CalamityMod;
using CalamityMod.NPCs.Calamitas;
using CalamityMod.Particles;
using CalamityMod.Projectiles.Boss;
using InfernumMode.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Graphics.Shaders;
using static System.Math;
using static Microsoft.Xna.Framework.MathHelper;

namespace InfernumMode.BehaviorOverrides.BossAIs.CalamitasShadow
{
    public class SoulSeekerResurrectionBeam : ModProjectile
    {
        public PrimitiveTrailCopy BeamDrawer
        {
            get;
            set;
        }

        public bool HasSummonedSeeker
        {
            get;
            set;
        }

        public ref float Time => ref projectile.ai[0];

        public ref float LaserLength => ref projectile.ai[1];

        public const int Lifetime = 30;

        public const float MaxLaserLength = 5500f;

        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public override void SetStaticDefaults() => DisplayName.SetDefault("Necromantic Beam");

        public override void SetDefaults()
        {
            projectile.width = projectile.height = 24;
            projectile.ignoreWater = true;
            projectile.tileCollide = false;
            projectile.penetrate = -1;
            projectile.timeLeft = Lifetime;
            projectile.alpha = 255;
            projectile.Calamity().canBreakPlayerDefense = true;
            cooldownSlot = 1;
        }

        public override void AI()
        {
            // Fade in.
            projectile.alpha = Utils.Clamp(projectile.alpha - 25, 0, 255);

            projectile.scale = CalamityUtils.Convert01To010(projectile.timeLeft / (float)Lifetime) * 3f;
            if (projectile.scale > 1f)
                projectile.scale = 1f;

            // Calculate the laser length.
            float maxCheckDistance = Utils.InverseLerp(-1f, 10f, Time, true) * MaxLaserLength;
            float[] distanceSamples = new float[12];
            Collision.LaserScan(projectile.Center, projectile.velocity, projectile.width, maxCheckDistance, distanceSamples);
            LaserLength = distanceSamples.Average();

            // Summon a seeker if necessary.
            if (Time >= 10f && !HasSummonedSeeker && (LaserLength < MaxLaserLength - 200f || Time >= Lifetime - 1f))
            {
                Vector2 seekerSpawnPosition = projectile.Center + projectile.velocity * LaserLength;
                Main.PlaySound(InfernumMode.Instance.GetLegacySoundSlot(SoundType.Custom, "Sounds/Custom/BrimstoneMonsterSpawn"), seekerSpawnPosition);

                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    int seekerID = ModContent.NPCType<SoulSeeker>();
                    int totalSeekers = NPC.CountNPCS(seekerID) + 1;

                    if (totalSeekers >= 7)
                    {
                        HasSummonedSeeker = true;
                        return;
                    }

                    int seeker = NPC.NewNPC((int)seekerSpawnPosition.X, (int)seekerSpawnPosition.Y, seekerID);
                    if (Main.npc.IndexInRange(seeker))
                        NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, seeker);

                    int seekerIndex = 0;
                    for (int i = 0; i < Main.maxNPCs; i++)
                    {
                        NPC n = Main.npc[i];

                        if (n.active && n.type == seekerID)
                        {
                            n.ai[0] = MathHelper.TwoPi * seekerIndex / totalSeekers;
                            n.ai[1] = 0f;
                            n.ai[3] = 0f;
                            n.netUpdate = true;
                            seekerIndex++;
                        }
                    }
                }

                for (int i = 0; i < 10; i++)
                {
                    Color fireColor = Main.rand.NextBool() ? Color.Yellow : Color.Red;
                    CloudParticle fireCloud = new CloudParticle(seekerSpawnPosition, (MathHelper.TwoPi * i / 10f).ToRotationVector2() * 6f, fireColor, Color.DarkGray, 36, Main.rand.NextFloat(1.9f, 2.3f));
                    GeneralParticleHandler.SpawnParticle(fireCloud);
                }
                HasSummonedSeeker = true;
            }

            Time++;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float _ = 0f;
            float width = projectile.width * 0.8f;
            Vector2 start = projectile.Center;
            Vector2 end = start + projectile.velocity * (LaserLength - 80f) * 0.65f;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), start, end, width, ref _);
        }

        public float WidthFunction(float completionRatio)
        {
            float squeezeInterpolant = Utils.InverseLerp(0f, 0.16f, completionRatio, true) * Utils.InverseLerp(1f, 0.95f, completionRatio, true);
            float baseWidth = SmoothStep(2f, projectile.width, squeezeInterpolant) * MathHelper.Clamp(projectile.scale, 0.01f, 1f);
            return baseWidth * Lerp(1f, 2.3f, projectile.localAI[0]);
        }

        public override bool ShouldUpdatePosition() => false;

        public Color ColorFunction(float completionRatio)
        {
            float opacity = Utils.InverseLerp(0.92f, 0.6f, completionRatio, true) * Lerp(1f, 0.45f, projectile.localAI[0]) * projectile.Opacity * 0.4f;
            Color color = Color.Lerp(Color.Red, Color.Yellow, (float)Math.Abs(Math.Sin(completionRatio * MathHelper.Pi + Main.GlobalTime)) * 0.5f);
            return color * opacity;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            if (BeamDrawer == null)
				BeamDrawer = new PrimitiveTrailCopy(WidthFunction, ColorFunction, null, true, GameShaders.Misc["Infernum:ArtemisLaser"]);

            // Select textures to pass to the shader, along with the electricity color.
            GameShaders.Misc["Infernum:ArtemisLaser"].UseColor(Color.Red);
            GameShaders.Misc["Infernum:ArtemisLaser"].SetShaderTexture(ModContent.GetTexture("InfernumMode/ExtraTextures/StreakMagma"));
            GameShaders.Misc["Infernum:ArtemisLaser"].UseImage("Images/Misc/Perlin");
            GameShaders.Misc["Infernum:ArtemisLaser"].Shader.Parameters["uStretchReverseFactor"].SetValue((LaserLength + 1f) / MaxLaserLength * 8f);

            List<Vector2> points = new List<Vector2>();
            for (int i = 0; i <= 8; i++)
                points.Add(Vector2.Lerp(projectile.Center - projectile.velocity * 18f, projectile.Center + projectile.velocity * LaserLength, i / 8f));

            BeamDrawer.Draw(points, projectile.Size * 0.5f - Main.screenPosition, 60);
            Main.spriteBatch.ExitShaderRegion();
            return false;			
        }		

        public override bool CanDamage()/* tModPorter Suggestion: Return null instead of false */ => Time >= 8f;
    }
}
