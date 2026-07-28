using CalamityMod;
using CalamityMod.Events;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.CalamitasShadow
{
    public class BrimstoneSlash : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Brimstone Slash");
            ProjectileID.Sets.TrailCacheLength[projectile.type] = 3;
            ProjectileID.Sets.TrailingMode[projectile.type] = 0;
        }

        public override void SetDefaults()
        {
            projectile.width = 80;
            projectile.height = 34;
            projectile.hostile = true;
            projectile.ignoreWater = true;
            projectile.tileCollide = false;
            projectile.timeLeft = 300;
            projectile.Opacity = 0f;
            projectile.Calamity().canBreakPlayerDefense = true;
            cooldownSlot = 1;
        }

        public override void AI()
        {
            CreateVisuals();
            PerformMovement();
        }

        public void CreateVisuals()
        {
            if (Main.dedServ)
                return;

            // Emit crimson light.
            Lighting.AddLight(projectile.Center, 0.25f, 0f, 0f);

            // Fade in quickly.
            projectile.Opacity = MathHelper.Clamp(projectile.Opacity + 0.075f, 0f, 1f);

            // Determine rotation.
            projectile.rotation = projectile.velocity.ToRotation() + MathHelper.PiOver2;

            // Emit small magic particles.
            Vector2 magicSpawnPosition = projectile.Center + Vector2.UnitY.RotatedBy(projectile.rotation + MathHelper.PiOver2) * Main.rand.NextFloat(4f, 45f) * (Main.rand.NextBool() ? 1f : -1f);
            Dust magic = Dust.NewDustPerfect(magicSpawnPosition, 261);
            magic.position += Main.rand.NextVector2Circular(10f, 10f);
            magic.velocity = projectile.velocity * Main.rand.NextFloat(-0.3f, 0.08f);
            magic.color = Color.Lerp(Color.Red, Color.Cyan, Main.rand.NextFloat());
            magic.noGravity = true;
        }

        public void PerformMovement()
        {
            float maxSpeed = 18f;
            if (BossRushEvent.BossRushActive)
                maxSpeed = 24.5f;
            if (projectile.velocity.Length() < maxSpeed)
                projectile.velocity *= 1.01f;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Color drawColor = Color.Lerp(Color.Cyan, Color.Red, 0.35f) * projectile.Opacity * 0.45f;
            drawColor.A = 0;
            Texture2D slashTexture = ModContent.GetTexture(Texture);
            for (int i = 0; i < 4; i++)
            {
                Vector2 drawOffset = (projectile.rotation - MathHelper.PiOver2).ToRotationVector2() * new Vector2(6f, 3f);
                Vector2 drawPosition = projectile.Center + drawOffset - Main.screenPosition;
                Main.spriteBatch.Draw(slashTexture, drawPosition, null, drawColor, projectile.rotation, slashTexture.Size() * 0.5f, projectile.scale, SpriteEffects.None, 0f);
            }
            CalamityUtils.DrawAfterimagesCentered(projectile, ProjectileID.Sets.TrailingMode[projectile.type], projectile.GetAlpha(Color.DeepSkyBlue), 1);
            return false;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => projectile.RotatingHitboxCollision(targetHitbox.TopLeft(), targetHitbox.Size());
    }
}
