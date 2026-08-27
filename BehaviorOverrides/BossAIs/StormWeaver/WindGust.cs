using System;
using CalamityMod;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework.Graphics;

namespace InfernumMode.BehaviorOverrides.BossAIs.StormWeaver
{
    public class WindGust : ModProjectile
    {
        public Vector2 SpinCenter
        {
            get;
            set;
        }

        public float SpinDirection
        {
            get;
            set;
        }

        public ref float SpinOffsetAngle => ref projectile.ai[0];

        public ref float Time => ref projectile.ai[1];

        public static int Lifetime => 90;

        public static float SpinConvergencePower => 3.7f;

        public override void SetStaticDefaults()
        {
            Main.projFrames[projectile.type] = 6;
            ProjectileID.Sets.TrailingMode[projectile.type] = 2;
            ProjectileID.Sets.TrailCacheLength[projectile.type] = 8;
            DisplayName.SetDefault("Wind Gust");
        }

        public override void SetDefaults()
        {
            projectile.width = projectile.height = 42;
            projectile.penetrate = -1;
            projectile.tileCollide = false;
            projectile.hostile = true;
            projectile.timeLeft = Lifetime;
            projectile.Opacity = 0f;
            cooldownSlot = 1;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(SpinDirection);
            writer.WriteVector2(SpinCenter);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            SpinDirection = reader.ReadSingle();
            SpinCenter = reader.ReadVector2();
        }

        public override void AI()
        {
            // Spin in place.
            float spinAngularVelocity = Utilities.Remap(Time, 0f, 45f, MathHelper.Pi / 75f, MathHelper.Pi / 359f);
            float spinRadius = (1f - (float)Math.Pow(Utils.InverseLerp(0f, Lifetime - 4f, Time, true), SpinConvergencePower)) * 600f;
            SpinOffsetAngle += spinAngularVelocity * SpinDirection;
            projectile.Center = SpinCenter + SpinOffsetAngle.ToRotationVector2() * spinRadius;

            // Rotate based on the positional difference from the last frame.
            projectile.rotation = (projectile.position - projectile.oldPosition).X * 0.01f;

            // Animate frames.
            projectile.frameCounter++;
            projectile.frame = projectile.frameCounter / 5 % Main.projFrames[projectile.type];

            // Fade in and out.
            projectile.Opacity = Utils.InverseLerp(0f, 20f, Time, true) * Utils.InverseLerp(0f, 20f, projectile.timeLeft, true);

            // Emit some dust and air particles.
            float dustSpawnChance = MathHelper.Lerp(1f, 0.2f, projectile.Opacity);
            for (int i = 0; i < 6; i++)
            {
                if (Main.rand.NextFloat() > dustSpawnChance)
                    continue;

                int d = Dust.NewDust(projectile.Center, projectile.width, projectile.height, DustID.Smoke, 0f, 0f, 136, new Color(232, 251, 250, 200), 1.4f);
                Main.dust[d].noGravity = true;
                Main.dust[d].position = projectile.Center + Main.rand.NextVector2Circular(20f, 20f);
                Main.dust[d].velocity = -Vector2.UnitY * 3f + (projectile.position - projectile.oldPosition).RotatedBy(MathHelper.PiOver2).RotatedByRandom(0.37f) * 0.1f;
            }

            if (projectile.timeLeft <= 7)
            {
                MediumMistParticle mist = new MediumMistParticle(projectile.Center + Main.rand.NextVector2Circular(12f, 12f), Vector2.Zero, new Color(172, 238, 255), new Color(145, 170, 188), Main.rand.NextFloat(0.5f, 1.5f), 245 - Main.rand.Next(50), 0.02f)
                {
                    Velocity = Main.rand.NextVector2Circular(7.5f, 7.5f)
                };
                GeneralParticleHandler.SpawnParticle(mist);
            }

            Time++;
        }

        public override bool CanDamage() => Time >= 12f;

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Utilities.DrawAfterimagesCentered(projectile, Color.White, ProjectileID.Sets.TrailingMode[projectile.type]);

            int backglowStartTime = (int)((float)Math.Pow(0.1f, 1f / SpinConvergencePower) * Lifetime);
            float backglowInterpolant = CalamityUtils.Convert01To010(Utils.InverseLerp(backglowStartTime - 12f, backglowStartTime + 8f, Time, true));
            projectile.DrawProjectileWithBackglowTemp(Color.Cyan * backglowInterpolant * projectile.Opacity, Color.White, backglowInterpolant * 6f);
            return false;
        }
    }
}
