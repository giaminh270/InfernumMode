using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.PlaguebringerGoliath
{
    public class HostilePlagueSeeker : ModProjectile
    {
        public ref float Time => ref projectile.ai[0];
        public override string Texture => "CalamityMod/Projectiles/StarProj";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Plague Seeker");
            ProjectileID.Sets.TrailingMode[projectile.type] = 2;
            ProjectileID.Sets.TrailCacheLength[projectile.type] = 12;
        }

        public override void SetDefaults()
        {
            projectile.width = projectile.height = 14;
            projectile.hostile = true;
            projectile.ignoreWater = true;
            projectile.tileCollide = false;
            projectile.penetrate = -1;
            projectile.timeLeft = 150;
            cooldownSlot = 1;
        }

        public override void AI()
        {
            if (Time >= 5f)
            {
                Dust plague = Dust.NewDustDirect(projectile.position, projectile.width, projectile.height, DustID.TerraBlade, 0f, 0f, 100, default, 0.75f);
                plague.noGravity = true;
                plague.velocity = Vector2.Zero;
            }

            // If not close to death, home in on the closest player.
            if (Time >= 56f)
            {
                Player target = Main.player[Player.FindClosest(projectile.Center, 1, 1)];
                if (!projectile.WithinRange(target.Center, 50f))
                    projectile.velocity = (projectile.velocity * 69f + projectile.SafeDirectionTo(target.Center) * 16f) / 70f;
            }
            projectile.rotation = projectile.velocity.ToRotation() + MathHelper.PiOver2;
            Time++;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Texture2D boltTexture = ModContent.GetTexture(Texture);
            for (int i = 0; i < projectile.oldPos.Length; i++)
            {
                float completionRatio = i / (float)projectile.oldPos.Length;
                Color drawColor = Color.Lerp(lightColor, Color.Olive, 0.6f);
                drawColor = Color.Lerp(drawColor, Color.Lime, 0.425f);
                drawColor = Color.Lerp(drawColor, Color.Black, completionRatio);
                drawColor = Color.Lerp(drawColor, Color.Transparent, completionRatio);

                Vector2 drawPosition = projectile.oldPos[i] + projectile.Size * 0.5f - Main.screenPosition;
                Main.spriteBatch.Draw(boltTexture, drawPosition, null, projectile.GetAlpha(drawColor), projectile.oldRot[i], boltTexture.Size() * 0.5f, projectile.scale, SpriteEffects.None, 0);
            }
            return false;
        }

        public override bool CanDamage() => projectile.Opacity >= 0.8f;
    }
}
