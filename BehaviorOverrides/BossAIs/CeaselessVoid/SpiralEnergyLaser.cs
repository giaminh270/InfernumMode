﻿using CalamityMod;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework.Graphics;


namespace InfernumMode.BehaviorOverrides.BossAIs.CeaselessVoid
{
    public class SpiralEnergyLaser : ModProjectile
    {
        private const float LaserLength = 80f;
        private const float LaserLengthChangeRate = 2f;

        // Do not change these unless you are absolutely sure you know how to fix the wave math.
        private const float WaveTheta = 0.038f;
        private const int WaveTwistFrames = 9;

        private ref float WaveFrameState => ref projectile.localAI[1];

        public override string Texture => "CalamityMod/Projectiles/LaserProj";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Energy Laser");
            ProjectileID.Sets.TrailingMode[projectile.type] = 2;
            ProjectileID.Sets.TrailCacheLength[projectile.type] = 8;
        }

        public override void SetDefaults()
        {
            projectile.width = 5;
            projectile.height = 5;
            projectile.hostile = true;
            projectile.alpha = 255;
            projectile.penetrate = -1;
            projectile.MaxUpdates = 1;
            projectile.timeLeft = 1200;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;
        }

        public override void AI()
        {
            // Very rapidly fade into existence.
            projectile.alpha = Utils.Clamp(projectile.alpha - 25, 0, 255);

            float waveSign = WaveFrameState < 0f ? -1f : 1f;

            // Initialize waving. Setting localAI[1] to a number between -1 and 1 tells it which way to wave.
            // Exactly 0 is a coinflip.
            if (Math.Abs(WaveFrameState) < 1f)
            {
                float dirToUse = WaveFrameState == 0f ? (Main.rand.NextBool() ? -1f : 1f) : waveSign;
                waveSign = -dirToUse;
                WaveFrameState = dirToUse * WaveTwistFrames * 0.5f;

                // Backfill old rotations to prevent visual glitches.
                float iterRotation = projectile.velocity.ToRotation();
                for (int i = 0; i < projectile.oldRot.Length; ++i)
                {
                    projectile.oldRot[i] = iterRotation;
                    iterRotation += waveSign * WaveTheta;
                }
            }
            // Switch waving directions as necessary.
            else if (Math.Abs(WaveFrameState) > WaveTwistFrames)
                WaveFrameState = -waveSign;
            else
                WaveFrameState += waveSign;

            // Apply a constant, rapid wave to the laser's motion.
            projectile.velocity = projectile.velocity.RotatedBy(waveSign * WaveTheta);
            projectile.rotation = projectile.velocity.ToRotation();

            // Emit light.
            if (WaveFrameState < 0f)
                Lighting.AddLight(projectile.Center, 0.87f, 0.65f, 0.1725f);
            else
                Lighting.AddLight(projectile.Center, 0.1725f, 0.87f, 0.65f);

            // Laser length shenanigans. If the laser is still growing, increase localAI 0 to indicate it is getting longer.
            projectile.localAI[0] += 10f; // LaserLengthChangeRate;

            // Cap it at max length.
            if (projectile.localAI[0] > LaserLength)
                projectile.localAI[0] = LaserLength;

            // Otherwise it's shrinking. Once it reaches zero length it dies for good.
            else
            {
                projectile.localAI[0] -= LaserLengthChangeRate;
                if (projectile.localAI[0] <= 0f)
                    projectile.Kill();
            }
        }
        
        public override Color? GetAlpha(Color lightColor) => new Color(48, 244, 244, projectile.alpha);

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor) => projectile.DrawBeam(LaserLength, 2f, lightColor, curve: true);

        public override void Kill(int timeLeft)
        {
            int dustID = 226;
            int dustAmt = 4;
            Vector2 dustPos = projectile.Center - projectile.velocity / 2f;
            Vector2 dustVel = projectile.velocity / 4f;
            for (int i = 0; i < dustAmt; ++i)
            {
                Dust d = Dust.NewDustDirect(dustPos, 0, 0, dustID, 0f, 0f, Scale: 1.5f);
                d.velocity += dustVel;
                d.velocity *= Main.rand.NextFloat(0.4f, 1f);
                d.noGravity = true;
            }
        }
    }
}
