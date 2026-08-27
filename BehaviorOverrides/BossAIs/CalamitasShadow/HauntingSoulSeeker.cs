using CalamityMod;
using CalamityMod.Particles;
using InfernumMode.GlobalInstances;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.CalamitasShadow
{
    public class HauntingSoulSeeker : ModProjectile
    {
        public Player Owner => Main.player[projectile.owner];

        public ref float SpinOffsetAngle => ref projectile.ai[0];

        public override string Texture => "CalamityMod/NPCs/Calamitas/SoulSeeker";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Soul Seeker");
            Main.projFrames[projectile.type] = 5;
        }

        public override void SetDefaults()
        {
            projectile.width = projectile.height = 24;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;
            projectile.penetrate = -1;
            projectile.timeLeft = 7200;
            projectile.Opacity = 0f;
            cooldownSlot = 1;
        }

        public override void AI()
        {
            projectile.frameCounter++;
            projectile.frame = projectile.frameCounter / 5 % Main.projFrames[projectile.type];
            projectile.rotation = projectile.AngleTo(Owner.Center) + MathHelper.Pi;

            // Spin around the target.
            SpinOffsetAngle += MathHelper.Pi / 150f;
            projectile.Center = Owner.Center - Vector2.UnitY.RotatedBy(SpinOffsetAngle) * 550f;

            // Fade in and release fire mist.
            projectile.Opacity = Owner.Infernum_CalShadowHex().HexStatuses["Indignation"].Intensity;
            if (projectile.Opacity < 0.85f)
            {
                Color fireMistColor = Color.Lerp(Color.Red, Color.Yellow, Main.rand.NextFloat(0.66f));
                var mist = new MediumMistParticle(projectile.Center + Main.rand.NextVector2Circular(24f, 24f), Main.rand.NextVector2Circular(4.5f, 4.5f), fireMistColor, Color.Gray, Main.rand.NextFloat(0.6f, 1.3f), 208 - Main.rand.Next(50), 0.02f);
                GeneralParticleHandler.SpawnParticle(mist);
            }

            // Periodically release dark magic bolts.
            int shootRate = 40;
            if (projectile.timeLeft % shootRate == shootRate - 1f && projectile.Opacity >= 0.9f)
            {
                // Release some fire mist.
                Vector2 magicVelocity = projectile.SafeDirectionTo(Owner.Center) * Main.rand.NextFloat(7.5f, 9f);
                for (int i = 0; i < 8; i++)
                {
                    Color fireMistColor = Color.Lerp(Color.Red, Color.Yellow, Main.rand.NextFloat(0.66f));
                    var mist = new MediumMistParticle(projectile.Center + magicVelocity * 2f + Main.rand.NextVector2Circular(10f, 10f), Vector2.Zero, fireMistColor, Color.Gray, Main.rand.NextFloat(0.6f, 1.3f), 195 - Main.rand.Next(50), 0.02f)
                    {
                        Velocity = magicVelocity.RotatedByRandom(0.2f) * Main.rand.NextFloat(0.9f, 2.4f)
                    };
                    GeneralParticleHandler.SpawnParticle(mist);
                }

                Main.PlaySound(SoundID.Item72, projectile.Center);
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    ProjectileSpawnManagementSystem.PrepareProjectileForSpawning(flame =>
                    {
                        flame.ModProjectile<DarkMagicFlame>().FromSeekerHex = true;
                    });
                    Utilities.NewProjectileBetter(projectile.Center + magicVelocity * 2f, magicVelocity, ModContent.ProjectileType<DarkMagicFlame>(), CalamitasShadowBehaviorOverride.DarkMagicFlameDamage, 0f, ai1: 1f);
                }
            }

            // Fade away if the hex has been lifted.
            if (projectile.timeLeft < 7140 && projectile.Opacity <= 0.02f)
                projectile.Kill();
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            projectile.DrawBackglow(Color.IndianRed, 12f);
            projectile.DrawProjectileWithBackglowTemp(Color.Yellow * 0.4f, Color.White * 0.5f, 5f);
            return false;
        }
    }
}
