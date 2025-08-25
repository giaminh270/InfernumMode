using CalamityMod;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.ProfanedGuardians
{
    public class ProfanedField : ModProjectile
    {
        public ref float GeneralTimer => ref projectile.ai[0];

        public ref float Radius => ref projectile.ai[1];

        public const float MaxRadius = 336f;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Profaned Defender Field");
        }

        public override void SetDefaults()
        {
            projectile.width = 32;
            projectile.height = 32;
            projectile.hostile = true;
            projectile.alpha = 255;
            projectile.penetrate = -1;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;
            projectile.timeLeft = 240;
        }

        public override void AI()
        {
            // Emit light.
            Lighting.AddLight(projectile.Center, Color.Yellow.ToVector3() * 0.45f);

            // Have the rocks expand outward.
            bool collapse = projectile.timeLeft < 60;
            Radius = MathHelper.Lerp(Radius, collapse ? 0f : MaxRadius, 0.04f);
            if (collapse && Radius >= 3f)
                Radius -= 1.25f;

            // Create a bunch of fire inside of the field.
            for (int i = 0; i < 7; i++)
            {
                Vector2 fireSpawnPosition = projectile.Center + Main.rand.NextVector2Circular(Radius, Radius) * 0.8f;
                if (!Main.LocalPlayer.WithinRange(fireSpawnPosition, 1000f))
                    continue;

                MediumMistParticle fire = new MediumMistParticle(fireSpawnPosition, -Vector2.UnitY.RotatedByRandom(0.85f) * Main.rand.NextFloat(1f, 5.6f), Color.Orange, Color.Yellow, 0.5f, 255f);
                GeneralParticleHandler.SpawnParticle(fire);
            }

            projectile.rotation += projectile.velocity.X * 0.006f;
            projectile.Opacity = MathHelper.Clamp(projectile.Opacity + 0.04f, 0f, 1f);
            GeneralTimer++;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            float spinDirection = (projectile.identity % 2 == 0).ToDirectionInt();
            Vector2 baseDrawPosition = projectile.Center;

            // Draw lines.
            for (int i = 0; i < 6; i++)
            {
                Vector2 drawPosition = baseDrawPosition + (MathHelper.TwoPi * (i - 1f) / 6f + GeneralTimer * spinDirection / 54f).ToRotationVector2() * Radius;
                Vector2 drawPositionNext = baseDrawPosition + (MathHelper.TwoPi * i / 6f + GeneralTimer * spinDirection / 54f).ToRotationVector2() * Radius;
                Main.spriteBatch.DrawLineBetter(drawPosition, drawPositionNext, (Color.Orange * 0.6f), 8f);
                Main.spriteBatch.DrawLineBetter(drawPosition, drawPositionNext, (Color.Yellow * 0.85f), 5f);
                Main.spriteBatch.DrawLineBetter(drawPosition, drawPositionNext, Color.White, 2f);
            }

            for (int i = 1; i <= 6; i++)
            {
                Texture2D texture = ModContent.GetTexture($"CalamityMod/Projectiles/Typeless/ArtifactOfResilienceShard{i}");
                Vector2 origin = texture.Size() * 0.5f;
                Vector2 drawPosition = baseDrawPosition + (MathHelper.TwoPi * (i - 1f) / 6f + GeneralTimer * spinDirection / 54f).ToRotationVector2() * Radius - Main.screenPosition;
                Main.spriteBatch.Draw(texture, drawPosition, null, projectile.GetAlpha(Color.White), projectile.rotation, origin, projectile.scale, 0, 0f);
            }

            return false;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            return CalamityUtils.CircularHitboxCollision(projectile.Center, Radius * 0.75f, targetHitbox);
        }

        public override bool CanDamage() => Radius >= MaxRadius * 0.5f;

        public override void OnHitPlayer(Player target, int damage, bool crit) => target.AddBuff(ModContent.BuffType<HolyFlames>(), 120);
    }
}
