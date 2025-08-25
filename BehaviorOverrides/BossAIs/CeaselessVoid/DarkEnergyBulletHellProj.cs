using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.CeaselessVoid
{
    public class DarkEnergyBulletHellProj : ModProjectile
    {
        public ref float Time => ref projectile.ai[0];
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Dark Energy");
            Main.projFrames[projectile.type] = 6;
            ProjectileID.Sets.TrailingMode[projectile.type] = 2;
            ProjectileID.Sets.TrailCacheLength[projectile.type] = 4;
        }

        public override void SetDefaults()
        {
            projectile.width = 66;
			projectile.height = 66;
            projectile.hostile = true;
            projectile.ignoreWater = true;
            projectile.tileCollide = false;
            projectile.penetrate = -1;
            projectile.timeLeft = 300;
			cooldownSlot = 1;;
        }

        public override void AI()
        {
            projectile.Opacity = Utils.InverseLerp(0f, 20f, Time, true) * Utils.InverseLerp(0f, 20f, projectile.timeLeft, true);
            projectile.scale = projectile.Opacity * 0.7f;

            // Accelerate.
            if (projectile.velocity.Length() < 19f)
                projectile.velocity *= 1.0145f;

            projectile.frameCounter++;
            projectile.frame = projectile.frameCounter / 5 % Main.projFrames[projectile.type];

            Time++;
        }

        public override Color? GetAlpha(Color lightColor)
        {
            float alpha = Utils.InverseLerp(0f, 30f, Time, true);
            return lightColor * projectile.Opacity * MathHelper.Lerp(0.6f, 1f, alpha);
        }

        public override bool CanDamage()/* tModPorter Suggestion: Return null instead of false */ => projectile.Opacity >= 1f;

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Texture2D glowmask1 = ModContent.GetTexture("InfernumMode/BehaviorOverrides/BossAIs/CeaselessVoid/DarkEnergyGlow");
            Texture2D glowmask2 = ModContent.GetTexture("InfernumMode/BehaviorOverrides/BossAIs/CeaselessVoid/DarkEnergyGlow2");

            Utilities.DrawAfterimagesCentered(projectile, Color.Fuchsia, ProjectileID.Sets.TrailingMode[projectile.type], 1);
            Utilities.DrawAfterimagesCentered(projectile, lightColor, ProjectileID.Sets.TrailingMode[projectile.type], 1);
            Utilities.DrawAfterimagesCentered(projectile, Color.White, ProjectileID.Sets.TrailingMode[projectile.type], 1, glowmask1);
            Utilities.DrawAfterimagesCentered(projectile, Color.White, ProjectileID.Sets.TrailingMode[projectile.type], 1, glowmask2);
            return false;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            return Utilities.CircularCollision(projectile.Center, targetHitbox, projectile.Size.Length() * projectile.scale * 0.5f);
        }

        public override void ModifyHitPlayer(Player target, ref int damage, ref bool crit)
        {
            
        }
    }
}