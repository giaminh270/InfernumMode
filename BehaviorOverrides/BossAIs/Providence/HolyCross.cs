using CalamityMod.Dusts;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System;

namespace InfernumMode.BehaviorOverrides.BossAIs.Providence
{
    public class HolyCross : ModProjectile
    {
        public ref float Time => ref projectile.ai[0];

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Holy Symbol");
            ProjectileID.Sets.TrailingMode[projectile.type] = 0;
            ProjectileID.Sets.TrailCacheLength[projectile.type] = 3;
        }

        public override void SetDefaults()
        {
            projectile.width = projectile.height = 34;
            projectile.hostile = true;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;
            projectile.penetrate = -1;
            projectile.timeLeft = 360;
            cooldownSlot = 1;
        }

        public override void AI()
        {
            if (projectile.velocity.Length() < 22f && Time >= 25f)
                projectile.velocity *= 1.024f;

            projectile.frameCounter++;
            projectile.rotation = projectile.velocity.ToRotation() - MathHelper.PiOver2;

            // Dissipate into ashes if inside of a wall.
            if (Collision.SolidCollision(projectile.TopLeft, projectile.width, projectile.height) && Time >= 90f)
            {
                // Release ashes.
                int ashCount = (int)MathHelper.Lerp(8f, 2f, projectile.Opacity);
                for (int i = 0; i < ashCount; i++)
                {
                    Color startingColor = Color.Lerp(Color.Orange, Color.Gray, Main.rand.NextFloat(0.5f, 0.8f));
                    MediumMistParticle ash = new MediumMistParticle(projectile.Center + Main.rand.NextVector2Circular(20f, 20f), Main.rand.NextVector2Circular(2f, 2f), startingColor, Color.DarkGray, projectile.Opacity * 0.4f, 255f, Main.rand.NextFloatDirection() * 0.014f);
                    GeneralParticleHandler.SpawnParticle(ash);
                }

                projectile.Opacity = MathHelper.Clamp(projectile.Opacity - 0.085f, 0f, 1f);
                if (projectile.Opacity <= 0f)
                    projectile.Kill();
            }

            Time++;
        }

        public override void Kill(int timeLeft)
        {
            for (int i = 0; i < 10; i++)
            {
                Dust holyFire = Dust.NewDustPerfect(projectile.Center, (int)CalamityDusts.ProfanedFire);
                holyFire.velocity = Main.rand.NextVector2Circular(14f, 14f);
                holyFire.scale = 1.7f;
                holyFire.noGravity = true;
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            lightColor = Color.Lerp(lightColor, Color.White, 0.4f);
            lightColor.A /= 2;

            Texture2D texture = ModContent.GetTexture(Texture);
            if (ProvidenceBehaviorOverride.IsEnraged)
                texture = ModContent.GetTexture("InfernumMode/BehaviorOverrides/BossAIs/Providence/HolyCrossNight");

            Utilities.DrawAfterimagesCentered(projectile, lightColor, ProjectileID.Sets.TrailingMode[projectile.type], 1, texture);
            projectile.DrawProjectileWithBackglowTemp(Color.White, Color.White, 3f, null, texture);
            return false;
        }

        public override bool CanDamage() => projectile.Opacity >= 0.67f;
    }
}
