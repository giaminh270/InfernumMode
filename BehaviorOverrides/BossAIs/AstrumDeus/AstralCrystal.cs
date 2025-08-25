using CalamityMod.Buffs.DamageOverTime;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.AstrumDeus
{
    public class AstralCrystal : ModProjectile
    {
        public ref float Time => ref projectile.ai[0];
        
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Astral Crystal");
            ProjectileID.Sets.TrailCacheLength[projectile.type] = 4;
            ProjectileID.Sets.TrailingMode[projectile.type] = 0;
        }

        public override void SetDefaults()
        {
            projectile.width = 26;
            projectile.height = 26;
            projectile.hostile = true;
            projectile.ignoreWater = true;
            projectile.tileCollide = false;
            projectile.Opacity = 0f;
            projectile.penetrate = -1;
            projectile.timeLeft = 330;
            cooldownSlot = 1;
        }

        public override void AI()
        {
            // Decide rotation.
            projectile.rotation = projectile.velocity.ToRotation() - MathHelper.PiOver2;

            // Fade in and out.
            projectile.Opacity = Utils.InverseLerp(0f, 12f, Time, true) * Utils.InverseLerp(0f, 32f, projectile.timeLeft, true);

            // Weakly home in on the target.
            float flySpeed = 10f;
            Player target = Main.player[Player.FindClosest(projectile.Center, 1, 1)];
            projectile.velocity = (projectile.velocity * 29f + projectile.SafeDirectionTo(target.Center) * flySpeed) / 30f;
            projectile.velocity = projectile.velocity.SafeNormalize(Vector2.UnitY) * flySpeed;

            Time++;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Texture2D texture = ModContent.GetTexture(Texture);
            Vector2 drawPosition = projectile.Center - Main.screenPosition;
            Vector2 origin = texture.Size() * 0.5f;
            Main.spriteBatch.Draw(texture, drawPosition, null, Color.White * projectile.Opacity, projectile.rotation, origin, projectile.scale, 0, 0f);
            return false;
        }

        public override Color? GetAlpha(Color lightColor) => Color.White * projectile.Opacity;
    }
}
