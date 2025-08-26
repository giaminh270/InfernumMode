using CalamityMod.Buffs.DamageOverTime;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.Providence
{
    public class AcceleratingCrystalShard : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Holy Crystal Shard");
            ProjectileID.Sets.TrailingMode[projectile.type] = 0;
            ProjectileID.Sets.TrailCacheLength[projectile.type] = 4;
        }

        public override void SetDefaults()
        {
            projectile.width = projectile.height = 32;
            projectile.hostile = true;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;
            projectile.penetrate = -1;
            projectile.Opacity = 0f;
            projectile.timeLeft = 300;
        }

        public override void AI()
        {
            projectile.Opacity = MathHelper.Clamp(projectile.Opacity + 0.1f, 0f, 1f);
            projectile.rotation = projectile.velocity.ToRotation() - MathHelper.PiOver2;

            if (projectile.velocity.Length() < 33f)
                projectile.velocity *= 1.035f;

            Lighting.AddLight(projectile.Center, Color.Yellow.ToVector3() * 0.5f);
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            float oldScale = projectile.scale;
            projectile.scale *= 1.2f;
            Color rainbowColor = Main.hslToRgb(projectile.identity / 7f % 1f, 1f, 0.5f);
            if (!Main.dayTime)
                rainbowColor = Color.Lerp(Color.Cyan, Color.Green, projectile.identity / 7f % 0.6f);
            lightColor = Color.Lerp(lightColor, rainbowColor, 0.9f);
            lightColor.A = 0;
            Utilities.DrawAfterimagesCentered(projectile, lightColor, ProjectileID.Sets.TrailingMode[projectile.type]);
            projectile.scale = oldScale;

            lightColor = Color.Lerp(lightColor, Color.White, 0.2f);
            lightColor.A = 0;
            Utilities.DrawAfterimagesCentered(projectile, lightColor, ProjectileID.Sets.TrailingMode[projectile.type]);

            return false;
        }

        public override void OnHitPlayer(Player target, int damage, bool crit)
        {
            if (Main.dayTime)
                target.AddBuff(ModContent.BuffType<HolyFlames>(), 120);
            else
                target.AddBuff(ModContent.BuffType<Nightwither>(), 60);
        }

        public override bool CanDamage() => projectile.alpha < 20;
    }
}
