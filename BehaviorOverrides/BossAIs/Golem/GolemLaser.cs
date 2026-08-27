using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.Golem
{
    public class GolemLaser : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Heat Laser");
            Main.projFrames[projectile.type] = 1;
        }

        public override void SetDefaults()
        {
            projectile.width = projectile.height = 8;
            projectile.hostile = true;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;
            projectile.penetrate = -1;
            projectile.timeLeft = 240;
            projectile.Opacity = 0f;
            cooldownSlot = 1;
        }

        public override void AI()
        {
            // Create a puff of energy when spawned.
            if (projectile.localAI[0] == 0f)
            {
                for (int i = 0; i < 8; i++)
                {
                    Vector2 fireSpawnPosition = projectile.Center + Main.rand.NextVector2Circular(16f, 16f);
                    Vector2 fireVelocity = (MathHelper.TwoPi * i / 8f).ToRotationVector2() * Main.rand.NextFloat(1.5f, 2f);
                    Particle fire = new MediumMistParticle(fireSpawnPosition, fireVelocity, Color.Orange, Color.Gray, Main.rand.NextFloat(0.7f, 0.9f), 236f, Main.rand.NextFloat(-0.04f, 0.04f));
                    GeneralParticleHandler.SpawnParticle(fire);
                }
                projectile.localAI[0] = 1f;
            }

            // Fade in.
            projectile.Opacity = MathHelper.Clamp(projectile.Opacity + 0.1f, 0f, 1f);

            // Decide rotation.
            projectile.rotation = projectile.velocity.ToRotation();

            // Decide frames.
            projectile.frameCounter++;
            projectile.frame = projectile.frameCounter / 5 % Main.projFrames[projectile.type];

            // Accelerate.
            if (projectile.velocity.Length() < 13f)
                projectile.velocity *= 1.015f;

            Lighting.AddLight(projectile.Center, Color.Red.ToVector3());
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Texture2D texture = Main.projectileTexture[projectile.type];
            Vector2 drawPosition = projectile.Center - Main.screenPosition;
            Rectangle frame = texture.Frame(1, Main.projFrames[projectile.type], 0, projectile.frame);
            Vector2 origin = frame.Size() * 0.5f;

            Main.spriteBatch.Draw(texture, drawPosition, frame, projectile.GetAlpha(Color.White), projectile.rotation, origin, projectile.scale, SpriteEffects.None, 0f);
            return false;
        }
    }
}
