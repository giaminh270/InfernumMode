using InfernumMode;
using InfernumMode.BehaviorOverrides.BossAIs.EoW;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.MinibossAIs.CorruptionMimic
{
    public class CursedDart : ModProjectile
    {
        public ref float Time => ref projectile.ai[0];

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Cursed Flame Dart");
            ProjectileID.Sets.TrailingMode[projectile.type] = 2;
            ProjectileID.Sets.TrailCacheLength[projectile.type] = 3;
        }

        public override void SetDefaults()
        {
            projectile.width = projectile.height = 18;
            projectile.hostile = true;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;
            projectile.timeLeft = 300;
            projectile.penetrate = -1;
        }

        public override void AI()
        {
            Time++;

            projectile.tileCollide = Time >= 105f;
            projectile.velocity.Y = MathHelper.Clamp(projectile.velocity.Y + 0.36f, -32f, 9f);
            projectile.rotation = projectile.velocity.ToRotation() + MathHelper.PiOver2;
            projectile.Opacity = Utils.InverseLerp(0f, 12f, Time, true);

            // Fire flames downward.
            if (Main.netMode != NetmodeID.MultiplayerClient && Time >= 12f && Time % 18f == 17f)
                Utilities.NewProjectileBetter(projectile.Center, Vector2.UnitY * 5.6f, ModContent.ProjectileType<CursedBullet>(), 115, 0f);

            float dustCreationChance = Utils.InverseLerp(0f, 12f, Time, true);
            for (int i = 0; i < 2; i++)
            {
                if (Main.rand.NextFloat() > dustCreationChance)
                    continue;

                Dust fire = Dust.NewDustDirect(projectile.TopLeft, projectile.width, projectile.height, 75);
                fire.velocity = Main.rand.NextVector2Circular(1.2f, 1.2f);
                fire.noGravity = true;
            }
        }

        public override Color? GetAlpha(Color lightColor) => Color.White * projectile.Opacity;

        public override bool CanDamage() => Time >= 12f;

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Utilities.DrawAfterimagesCentered(projectile, lightColor, ProjectileID.Sets.TrailingMode[projectile.type], 3);
            return false;
        }
    }
}
