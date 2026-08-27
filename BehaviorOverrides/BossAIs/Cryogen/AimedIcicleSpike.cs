using CalamityMod;
using CalamityMod.Events;
using CalamityMod.Particles;
using InfernumMode.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.Cryogen
{
    public class AimedIcicleSpike : ModProjectile
    {
        public ref float Time => ref projectile.ai[0];
        public ref float AimAheadFactor => ref projectile.ai[1];
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Icicle Spike");
        }

        public override void SetDefaults()
        {
            projectile.width = projectile.height = 28;
            projectile.hostile = true;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;
            projectile.timeLeft = 240;
            projectile.extraUpdates = BossRushEvent.BossRushActive ? 1 : 0;
            projectile.Calamity().canBreakPlayerDefense = true;
            cooldownSlot = 1;
        }

        public override void AI()
        {
            if (projectile.alpha > 0)
                projectile.alpha -= 12;

            Player closestPlayer = Main.player[Player.FindClosest(projectile.Center, 1, 1)];
            if (Time < 60f)
            {
                float spinSlowdown = Utils.InverseLerp(56f, 40f, Time, true);
                projectile.velocity *= 0.93f;
                projectile.rotation += (projectile.velocity.X > 0f).ToDirectionInt() * spinSlowdown * 0.3f;
                if (spinSlowdown < 1f)
                {
                    Vector2 aimAhead = closestPlayer.velocity * AimAheadFactor;
                    projectile.rotation = projectile.rotation.AngleLerp(projectile.AngleTo(closestPlayer.Center + aimAhead) - MathHelper.PiOver2, (1f - spinSlowdown) * 0.6f);
                }
            }

            if (Time == 60f)
            {
                projectile.velocity = projectile.SafeDirectionTo(closestPlayer.Center + closestPlayer.velocity * AimAheadFactor) * 9f;
                Main.PlaySound(SoundID.Item8, projectile.Center);
            }
            if (Time > 60f && projectile.velocity.Length() < 18f)
                projectile.velocity *= BossRushEvent.BossRushActive ? 1.02f : 1.01f;

            if (Time % 10 == 0)
            {
                // Leave a trail of particles.
                Particle iceParticle = new SnowyIceParticle(projectile.Center, projectile.velocity * 0.5f, Color.White, Main.rand.NextFloat(0.75f, 0.95f), 30);
                GeneralParticleHandler.SpawnParticle(iceParticle);
            }

            Lighting.AddLight(projectile.Center, Vector3.One * projectile.Opacity * 0.4f);
            Time++;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Texture2D texture = Main.projectileTexture[projectile.type];

            // Draw backglow effects.
            for (int i = 0; i < 12; i++)
            {
                Vector2 afterimageOffset = (MathHelper.TwoPi * i / 12f).ToRotationVector2() * 4f;
                Color afterimageColor = new Color(46, 188, 234, 0f) * 0.4f * projectile.Opacity;
                Main.spriteBatch.Draw(texture, projectile.Center - Main.screenPosition + afterimageOffset, null, projectile.GetAlpha(afterimageColor), projectile.rotation, texture.Size() * 0.5f, projectile.scale, SpriteEffects.None, 0f);
            }
            Main.spriteBatch.Draw(texture, projectile.Center - Main.screenPosition, null, Color.White * projectile.Opacity, projectile.rotation, texture.Size() * 0.5f, 1, 0, 0);
            return false;
        }
        public override bool CanDamage() => Time >= 60f;
    }
}
