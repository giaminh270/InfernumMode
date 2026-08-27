using CalamityMod;
using CalamityMod.NPCs;
using InfernumMode.BehaviorOverrides.BossAIs.Providence;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.ProfanedGuardians
{
    public class ProfanedSpearInfernum : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Profaned Spear");
            ProjectileID.Sets.TrailCacheLength[projectile.type] = 4;
            ProjectileID.Sets.TrailingMode[projectile.type] = 0;
        }

        public override void SetDefaults()
        {
            projectile.width = 32;
            projectile.height = 32;
            projectile.hostile = true;
            projectile.tileCollide = false;
            projectile.penetrate = -1;
            projectile.alpha = 255;
            projectile.timeLeft = 300;
            projectile.Calamity().canBreakPlayerDefense = true;
            cooldownSlot = 1;
        }

        public override void AI()
        {
            projectile.tileCollide = projectile.timeLeft < 210;
            projectile.rotation = projectile.velocity.ToRotation() + MathHelper.PiOver4;
            projectile.Opacity = MathHelper.Clamp(projectile.Opacity + 0.08f, 0f, 1f);

            // Accelerate.
            if (projectile.velocity.Length() < 36f)
                projectile.velocity *= 1.028f;

            Lighting.AddLight(projectile.Center, Vector3.One);
        }

        public override Color? GetAlpha(Color lightColor)
        {
            if (CalamityGlobalNPC.holyBoss != -1 && ProvidenceBehaviorOverride.IsEnraged)
                return Color.Cyan * projectile.Opacity;

            return Color.White * projectile.Opacity;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            float alpha = 1f - (float)projectile.alpha / 255;
            CalamityUtils.DrawAfterimagesCentered(projectile, ProjectileID.Sets.TrailingMode[projectile.type], lightColor * alpha, 1);
            projectile.DrawProjectileWithBackglowTemp(projectile.GetAlpha(Color.White), Color.White, 2f);
            return false;
        }
    }
}
