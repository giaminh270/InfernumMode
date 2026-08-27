using CalamityMod.DataStructures;
using InfernumMode.BehaviorOverrides.BossAIs.AquaticScourge;
using InfernumMode.DataStructures;
using InfernumMode.ExtraTextures;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.AquaticScourge
{
    public class AquaticScourgeBodySpike : ModProjectile, IAdditiveDrawer
    {
        public ref float AuraRadius => ref projectile.ai[0];

        public ref float Time => ref projectile.ai[1];

        public static float AcidWaterAccelerationFactor => 5f;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Irradiated Spike");
            ProjectileID.Sets.TrailingMode[projectile.type] = 0;
            ProjectileID.Sets.TrailCacheLength[projectile.type] = 2;
            ProjectileID.Sets.CanDistortWater[projectile.type] = false;
        }

        public override void SetDefaults()
        {
            projectile.width = projectile.height = 24;
            projectile.hostile = true;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;
            projectile.penetrate = -1;
            projectile.timeLeft = 200;
            
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
            // Make the sulphuric water effects go up far more quickly when inside the area of the pulse.
            AquaticScourgeHeadBehaviorOverride.ApplySulphuricPoisoningBoostToPlayersInArea(projectile.Center, AuraRadius * 0.6f, AcidWaterAccelerationFactor);

            // Home at first.
            if (Time < 60f)
            {
                Player target = Main.player[Player.FindClosest(projectile.Center, 1, 1)];
                projectile.velocity = Vector2.Lerp(projectile.velocity, projectile.SafeDirectionTo(target.Center) * 12f, 0.1f);
                projectile.velocity = projectile.velocity.ClampMagnitude(4f, 12f);
            }

            // Accelerate after homing for long enough.
            else if (projectile.velocity.Length() < 26f)
                projectile.velocity *= 1.025f;

            // Prevent spike clumping behavior.
            float pushForce = 0.85f;
            for (int k = 0; k < Main.maxProjectiles; k++)
            {
                Projectile otherProj = Main.projectile[k];

                // Short circuits to make the loop as fast as possible
                if (!otherProj.active || otherProj.type != projectile.type || k == projectile.whoAmI)
                    continue;

                // If the other projectile is indeed the same owned by the same player and they're too close, nudge them away.
                bool sameProjType = otherProj.type == projectile.type;
                float taxicabDistance = Math.Abs(projectile.position.X - otherProj.position.X) + Math.Abs(projectile.position.Y - otherProj.position.Y);
                if (sameProjType && taxicabDistance < projectile.width)
                {
                    if (projectile.position.X < otherProj.position.X)
                        projectile.velocity.X -= pushForce;
                    else
                        projectile.velocity.X += pushForce;

                    if (projectile.position.Y < otherProj.position.Y)
                        projectile.velocity.Y -= pushForce;
                    else
                        projectile.velocity.Y += pushForce;
                }
            }

            projectile.rotation = projectile.velocity.ToRotation() + MathHelper.Pi + MathHelper.PiOver4;

            Lighting.AddLight(projectile.Center, Color.Yellow.ToVector3() * 0.5f);
            Time++;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            lightColor = Color.Lerp(lightColor, Color.White, 0.8f);
            Utilities.DrawAfterimagesCentered(projectile, lightColor, ProjectileID.Sets.TrailingMode[projectile.type]);
            projectile.DrawProjectileWithBackglowTemp(Color.White * 0.75f, lightColor, projectile.Opacity * 5f);
            return false;
        }

        public void AdditiveDraw(SpriteBatch spriteBatch)
        {
            float telegraphInterpolant = Utils.InverseLerp(0f, 35f, projectile.timeLeft, true) * Utils.InverseLerp(0f, 45f, Time, true);
            float circleFadeinInterpolant = Utils.InverseLerp(0f, 0.15f, telegraphInterpolant, true);
            float colorPulse = (float)Math.Cos(Main.GlobalTime * 6.1f + projectile.identity) * 0.5f + 0.5f;
            float fadePulse = (Main.GlobalTime * 0.5f + projectile.identity * 0.2721f) % 1f;
            if (telegraphInterpolant > 0f)
            {
                Texture2D explosionTelegraphTexture = InfernumTextureRegistry.DistortedBloomRing;
                Vector2 scale = Vector2.One * AuraRadius / explosionTelegraphTexture.Size() * projectile.Opacity * (fadePulse * 0.5f + 1.2f);
                float telegraphRotation = MathHelper.TwoPi * projectile.identity / 13f % 1f + Main.GlobalTime * 1.427f;
                Color telegraphColor = Color.Lerp(Color.Lime, Color.Olive, colorPulse) * circleFadeinInterpolant * 0.3f;
                telegraphColor *= Utils.InverseLerp(0f, 0.08f, fadePulse, true) * Utils.InverseLerp(1f, 0.3f, fadePulse, true);

                spriteBatch.Draw(explosionTelegraphTexture, projectile.Center - Main.screenPosition, null, telegraphColor, telegraphRotation, explosionTelegraphTexture.Size() * 0.5f, scale, 0, 0f);
                spriteBatch.Draw(explosionTelegraphTexture, projectile.Center - Main.screenPosition, null, telegraphColor * 1.5f, -telegraphRotation, explosionTelegraphTexture.Size() * 0.5f, scale * 0.95f, 0, 0f);
            }
        }
    }
}
