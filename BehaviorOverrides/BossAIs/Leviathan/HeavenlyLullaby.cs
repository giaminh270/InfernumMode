using CalamityMod;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.Leviathan
{
    public class HeavenlyLullaby : ModProjectile
    {
        public ref float Time => ref projectile.ai[1];

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Heavenly Lullaby");
            ProjectileID.Sets.TrailingMode[projectile.type] = 0;
            ProjectileID.Sets.TrailCacheLength[projectile.type] = 4;
        }

        public override void SetDefaults()
        {
            projectile.width = 32;
            projectile.height = 32;
            projectile.hostile = true;
            projectile.ignoreWater = true;
            projectile.tileCollide = false;
            projectile.penetrate = -1;
            projectile.timeLeft = 360;
            projectile.Opacity = 0f;
            cooldownSlot = 1;
        }
        
        public override void AI()
        {
            // Determine opacity and scale.
            projectile.Opacity = Utils.InverseLerp(0f, 30f, projectile.timeLeft, true) * Utils.InverseLerp(0f, 10f, Time, true);
            projectile.spriteDirection = (projectile.velocity.X > 0f).ToDirectionInt();
            projectile.scale = projectile.Opacity * MathHelper.Lerp(0.8f, 1.2f, (float)Math.Sin(Time / 7f + projectile.identity * 23f) * 0.5f + 0.5f);
            Time++;

            Lighting.AddLight(projectile.Center, Vector3.One * projectile.Opacity * 0.5f);
        }

        public override bool CanHitPlayer(Player target) => projectile.Opacity == 1f;

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            CalamityUtils.DrawAfterimagesCentered(projectile, ProjectileID.Sets.TrailingMode[projectile.type], lightColor, 1);
            return false;
        }
    }
}
