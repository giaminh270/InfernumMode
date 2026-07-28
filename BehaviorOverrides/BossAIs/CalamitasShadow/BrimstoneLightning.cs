using CalamityMod;
using CalamityMod.Particles;
using InfernumMode.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace InfernumMode.BehaviorOverrides.BossAIs.CalamitasShadow
{
    public class BrimstoneLightning : ModProjectile
    {
        internal PrimitiveTrail LightningDrawer;

        public bool HasReachedDestination
        {
            get;
            set;
        }

        public Vector2 Destination
        {
            get;
            set;
        }

        public const int Lifetime = 45;

        public ref float InitialVelocityAngle => ref projectile.ai[0];

        // Technically not a ratio, and more of a seed, but it is used in a 0-2pi squash
        // later in the code to get an arbitrary unit vector (which is then checked).
        public ref float BaseTurnAngleRatio => ref projectile.ai[1];
        public ref float AccumulatedXMovementSpeeds => ref projectile.localAI[0];
        public ref float BranchingIteration => ref projectile.localAI[1];

        public virtual float LightningTurnRandomnessFactor { get; } = 2f;
        public override string Texture => "CalamityMod/Projectiles/LightningProj";
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Brimstone Lightning Bolt");
            ProjectileID.Sets.TrailingMode[projectile.type] = 1;
            ProjectileID.Sets.TrailCacheLength[projectile.type] = 50;
        }

        public override void SetDefaults()
        {
            projectile.width = 16;
            projectile.height = 16;
            projectile.alpha = 255;
            projectile.penetrate = -1;
            projectile.ignoreWater = true;
            projectile.tileCollide = false;
            projectile.hostile = true;
            projectile.MaxUpdates = 6;
            projectile.timeLeft = projectile.MaxUpdates * Lifetime;
            cooldownSlot = 1;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(projectile.timeLeft);
            writer.Write(HasReachedDestination);
            writer.Write(AccumulatedXMovementSpeeds);
            writer.Write(BranchingIteration);
            writer.WriteVector2(Destination);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            projectile.timeLeft = reader.ReadInt32();
            HasReachedDestination = reader.ReadBoolean();
            AccumulatedXMovementSpeeds = reader.ReadSingle();
            BranchingIteration = reader.ReadSingle();
            Destination = reader.ReadVector2();
        }

        public override void AI()
        {
            // FrameCounter in this context is really just an arbitrary timer
            // which allows random turning to occur.
            projectile.frameCounter++;
            projectile.oldPos[1] = projectile.oldPos[0];

            // Create impact effects upon reaching the destination.
            if (!HasReachedDestination && projectile.WithinRange(Destination, 12f))
            {
                // Play a zap sound.
                Main.PlaySound(SoundID.DD2_LightningBugZap, Destination);
                for (int i = 0; i < 8; i++)
                {
                    Vector2 sparkVelocity = Main.rand.NextVector2Unit() * Main.rand.NextFloat(5f, 11f);
                    Color sparkColor = Color.Lerp(Color.Orange, Color.IndianRed, Main.rand.NextFloat(0.4f, 1f));
                    GeneralParticleHandler.SpawnParticle(new SparkParticle(Destination, sparkVelocity, 60, 2f, sparkColor));

                    sparkVelocity = Main.rand.NextVector2Unit() * Main.rand.NextFloat(3f, 10f);
                    Color arcColor = Color.Lerp(Color.Orange, Color.Red, Main.rand.NextFloat(0.3f, 1f));
                    GeneralParticleHandler.SpawnParticle(new ElectricArc(Destination, sparkVelocity, arcColor, 0.84f, 30));
                }

                // Do funny screen stuff.
                Main.LocalPlayer.Infernum().CurrentScreenShakePower = 12f;
                // ScreenEffectSystem.SetBlurEffect(Destination, 0.8f, 45);

                projectile.velocity = Vector2.Zero;
                HasReachedDestination = true;
                projectile.timeLeft = ProjectileID.Sets.TrailCacheLength[projectile.type] - 1;
                projectile.netUpdate = true;
            }

            // Adjust opacity and scale.
            float adjustedTimeLife = projectile.timeLeft / projectile.MaxUpdates;
            projectile.Opacity = Utils.InverseLerp(0f, 9f, adjustedTimeLife, true) * Utils.InverseLerp(Lifetime, Lifetime - 3f, adjustedTimeLife, true);
            projectile.scale = projectile.Opacity;

            Lighting.AddLight(projectile.Center, Color.White.ToVector3());
            if (projectile.frameCounter >= projectile.extraUpdates * 2)
            {
                projectile.frameCounter = 0;

                float originalSpeed = Math.Min(15f, projectile.velocity.Length());
                UnifiedRandom unifiedRandom = new UnifiedRandom((int)BaseTurnAngleRatio);
                int turnTries = 0;
                Vector2 newBaseDirection = -Vector2.UnitY;
                Vector2 potentialBaseDirection;

                do
                {
                    BaseTurnAngleRatio = unifiedRandom.Next() % 100;
                    potentialBaseDirection = (BaseTurnAngleRatio / 100f * MathHelper.TwoPi).ToRotationVector2();

                    // Ensure that the new potential direction base is always moving upwards (this is supposed to be somewhat similar to a -UnitY + RotatedBy).
                    potentialBaseDirection.Y = -Math.Abs(potentialBaseDirection.Y);

                    bool canChangeLightningDirection = true;

                    // Potential directions with very little Y speed should not be considered, because this
                    // consequentially means that the X speed would be quite large.
                    if (potentialBaseDirection.Y > -0.02f)
                        canChangeLightningDirection = false;

                    // This mess of math basically encourages movement at the ends of an extraUpdate cycle,
                    // discourages super frequenent randomness as the accumulated X speed changes get larger,
                    // or if the original speed is quite large.
                    if (Math.Abs(potentialBaseDirection.X * (projectile.extraUpdates + 1) * 2f * originalSpeed + AccumulatedXMovementSpeeds) > projectile.MaxUpdates * LightningTurnRandomnessFactor)
                        canChangeLightningDirection = false;

                    // If the above checks were all passed, redefine the base direction of the lightning.
                    if (canChangeLightningDirection)
                        newBaseDirection = potentialBaseDirection;

                    turnTries++;
                }
                while (turnTries < 100);

                if (projectile.velocity != Vector2.Zero)
                {
                    AccumulatedXMovementSpeeds += newBaseDirection.X * (projectile.extraUpdates + 1) * 2f * originalSpeed;
                    projectile.velocity = newBaseDirection.RotatedBy(InitialVelocityAngle + MathHelper.PiOver2) * originalSpeed;
                    projectile.rotation = projectile.velocity.ToRotation() + MathHelper.PiOver2;
                }
            }
        }

        public float PrimitiveWidthFunction(float completionRatio) => CalamityUtils.Convert01To010(completionRatio) * projectile.scale * projectile.width;

        public Color PrimitiveColorFunction(float completionRatio)
        {
            float colorInterpolant = (float)Math.Sin(projectile.identity / 3f + completionRatio * 20f + Main.GlobalTime * 1.1f) * 0.5f + 0.5f;
            Color color = CalamityUtils.MulticolorLerp(colorInterpolant, Color.Red, Color.Yellow, Color.Pink);
            return color;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            List<Vector2> checkPoints = projectile.oldPos.Where(oldPos => oldPos != Vector2.Zero).ToList();
            if (checkPoints.Count <= 2)
                return false;

            for (int i = 0; i < checkPoints.Count - 1; i++)
            {
                float _ = 0f;
                float width = PrimitiveWidthFunction(i / (float)checkPoints.Count);
                if (Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), checkPoints[i], checkPoints[i + 1], width * 0.8f, ref _))
                    return true;
            }
            return false;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            if (LightningDrawer is null)
                LightningDrawer = new PrimitiveTrail(PrimitiveWidthFunction, PrimitiveColorFunction, PrimitiveTrail.RigidPointRetreivalFunction, GameShaders.Misc["Infernum:LightningArc"]);

            GameShaders.Misc["Infernum:LightningArc"].UseImage("Images/Misc/Perlin");
            GameShaders.Misc["Infernum:LightningArc"].Apply();

            LightningDrawer.Draw(projectile.oldPos, projectile.Size * 0.5f - Main.screenPosition, 18);
            return false;
        }
    }
}
