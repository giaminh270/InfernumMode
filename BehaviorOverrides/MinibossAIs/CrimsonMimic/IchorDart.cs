using InfernumMode;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.MinibossAIs.CrimsonMimic
{
    public class IchorDart : ModProjectile
    {
        public bool HasSplit => projectile.ai[1] == 1f;

        public ref float Time => ref projectile.ai[0];

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Ichor Dart");
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

            projectile.tileCollide = Time >= 75f;
            projectile.rotation = projectile.velocity.ToRotation() + MathHelper.PiOver2;
            projectile.Opacity = Utils.InverseLerp(0f, 12f, Time, true);
            if (HasSplit)
                projectile.Opacity = 1f;

            // Split into multiple darts.
            if (Main.netMode != NetmodeID.MultiplayerClient && Time >= 40f && !HasSplit)
            {
                for (int i = 0; i < 5; i++)
                {
                    float shootOffsetAngle = MathHelper.Lerp(-0.54f, 0.54f, i / 4f);
                    Vector2 shootVelocity = projectile.velocity.RotatedBy(shootOffsetAngle);
                    int splitDart = Utilities.NewProjectileBetter(projectile.Center, shootVelocity, projectile.type, 115, 0f);
                    if (Main.projectile.IndexInRange(splitDart))
                        Main.projectile[splitDart].ai[1] = 1f;
                }
                projectile.Kill();
                return;
            }
            
            for (int i = 0; i < 2; i++)
            {
                if (Main.rand.NextFloat() > projectile.Opacity)
                    continue;

                Dust ichor = Dust.NewDustDirect(projectile.TopLeft, projectile.width, projectile.height, 170);
                ichor.velocity = Main.rand.NextVector2Circular(1.2f, 1.2f);
                ichor.noGravity = true;
            }
        }

        public override Color? GetAlpha(Color lightColor) => Color.White * projectile.Opacity;

        public override bool CanDamage() => projectile.Opacity >= 1f;

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Utilities.DrawAfterimagesCentered(projectile, lightColor, ProjectileID.Sets.TrailingMode[projectile.type], 3);
            return false;
        }
    }
}
