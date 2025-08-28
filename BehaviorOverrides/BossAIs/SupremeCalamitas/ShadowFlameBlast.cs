using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.SupremeCalamitas
{
    public class ShadowFlameBlast : ModProjectile
    {
        public const int Lifetime = 32;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Shadow Blast");
            Main.projFrames[projectile.type] = 8;
            ProjectileID.Sets.TrailingMode[projectile.type] = 0;
            ProjectileID.Sets.TrailCacheLength[projectile.type] = 4;
        }

        public override void SetDefaults()
        {
            projectile.width = projectile.height = 52;
            projectile.hostile = true;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;
            projectile.penetrate = -1;
            projectile.timeLeft = Lifetime;
            projectile.Opacity = 0f;
        }

        public override void AI()
        {
            projectile.Opacity = MathHelper.Clamp(projectile.Opacity + 0.2f, 0f, 1f);

            projectile.velocity *= 0.995f;
            projectile.frameCounter++;
            projectile.frame = (int)Math.Ceiling((1f - projectile.timeLeft / (float)Lifetime) * 4f);
            projectile.rotation = projectile.velocity.ToRotation() - MathHelper.PiOver2;
        }

        public override Color? GetAlpha(Color lightColor) => Color.White * projectile.Opacity;

        public override void Kill(int timeLeft)
        {
            Main.PlaySound(SoundID.Item104, projectile.Center);
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            for (int i = 0; i < 6; i++)
			{
                Vector2 shadowSparkVelocity = (MathHelper.TwoPi * i / 6f).ToRotationVector2() * 7f;
                Utilities.NewProjectileBetter(projectile.Center, shadowSparkVelocity, ModContent.ProjectileType<ShadowSpark>(), 500, 0f);
			}
        }
    }
}
