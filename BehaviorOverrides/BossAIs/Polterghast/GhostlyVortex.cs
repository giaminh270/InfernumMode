using CalamityMod;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.Polterghast
{
    public class GhostlyVortex : ModProjectile
    {
        public ref float MaxSpeed => ref projectile.ai[0];

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Phantoplasm Vortex");
            ProjectileID.Sets.TrailingMode[projectile.type] = 2;
            ProjectileID.Sets.TrailCacheLength[projectile.type] = 3;
        }

        public override void SetDefaults()
        {
            projectile.width = projectile.height = 28;
            projectile.hostile = true;
            projectile.friendly = false;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;
            projectile.penetrate = -1;
            projectile.timeLeft = 300;
            projectile.Calamity().canBreakPlayerDefense = true;
            cooldownSlot = 1;
        }

        public override void AI()
        {
            float maxSpeed = MaxSpeed > 0f ? MaxSpeed : 27f;
            if (projectile.timeLeft < 90f)
                maxSpeed += 12f;

            if (projectile.velocity.Length() < maxSpeed)
                projectile.velocity *= 1.045f;

            projectile.rotation -= MathHelper.Pi / 12f;
            projectile.Opacity = Utils.InverseLerp(300f, 295f, projectile.timeLeft, true) * Utils.InverseLerp(0f, 25f, projectile.timeLeft, true);

            bool reducedGraphics = PolterghastPerformanceUtils.ReducedGraphics;
            if (!reducedGraphics || projectile.timeLeft % 3 == 0)
                Lighting.AddLight(projectile.Center, Color.White.ToVector3());

            // Spawn expensive dust less frequently on low-end graphics.
            if (!reducedGraphics && Main.rand.NextBool(5))
            {
                Vector2 offsetDirection = Main.rand.NextVector2Unit();
                Dust phantoplasm = Dust.NewDustDirect(projectile.Center - offsetDirection * 30f, 0, 0, 60, 0f, 0f, 0, default, 1f);
                phantoplasm.noGravity = true;
                phantoplasm.position = projectile.Center - offsetDirection * Main.rand.Next(10, 21);
                phantoplasm.velocity = offsetDirection.RotatedBy(MathHelper.PiOver2) * 6f;
                phantoplasm.scale = Main.rand.NextFloat(0.9f, 1.9f);
                phantoplasm.fadeIn = 0.5f;
                phantoplasm.customData = projectile;

                offsetDirection = Main.rand.NextVector2Unit();
                phantoplasm.noGravity = true;
                phantoplasm.position = projectile.Center - offsetDirection * Main.rand.Next(10, 21);
                phantoplasm.velocity = offsetDirection.RotatedBy(MathHelper.PiOver2) * 6f;
                phantoplasm.scale = Main.rand.NextFloat(0.9f, 1.9f);
                phantoplasm.fadeIn = 0.5f;
                phantoplasm.customData = projectile;
                phantoplasm.color = Color.Crimson;
            }
            else if (!reducedGraphics || projectile.timeLeft % 8 == 0)
            {
                Vector2 offsetDirection = Main.rand.NextVector2Unit();
                Dust phantoplasm = Dust.NewDustDirect(projectile.Center - offsetDirection * 30f, 0, 0, 60, 0f, 0f, 0, default, reducedGraphics ? 0.75f : 1f);
                phantoplasm.noGravity = true;
                phantoplasm.position = projectile.Center - offsetDirection * Main.rand.Next(20, 31);
                phantoplasm.velocity = offsetDirection.RotatedBy(-MathHelper.PiOver2) * 5f;
                phantoplasm.scale = reducedGraphics ? 0.8f : Main.rand.NextFloat(0.9f, 1.9f);
                phantoplasm.fadeIn = 0.5f;
                phantoplasm.customData = projectile;
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            if (!PolterghastPerformanceUtils.ReducedGraphics)
            {
                CalamityUtils.DrawAfterimagesCentered(projectile, ProjectileID.Sets.TrailingMode[projectile.type], lightColor, 1);
                return false;
            }

            Texture2D texture = Main.projectileTexture[projectile.type];
            Vector2 drawPosition = projectile.Center - Main.screenPosition;
            Rectangle frame = texture.Frame(1, Main.projFrames[projectile.type], 0, projectile.frame);
            spriteBatch.Draw(texture, drawPosition, frame, projectile.GetAlpha(lightColor), projectile.rotation, frame.Size() * 0.5f, projectile.scale, SpriteEffects.None, 0f);
            return false;
        }

        public override Color? GetAlpha(Color lightColor)
        {
            return new Color(255 - projectile.alpha, 255 - projectile.alpha, 255 - projectile.alpha, 255 - projectile.alpha);
        }

        public override bool CanDamage() => projectile.Opacity >= 1f;
    }
}
