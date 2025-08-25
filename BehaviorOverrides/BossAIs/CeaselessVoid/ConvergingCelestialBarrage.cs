using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.CeaselessVoid
{
    public class ConvergingCelestialBarrage : ModProjectile
    {
        public ref float Time => ref projectile.ai[0];

        public ref float IdealDirection => ref projectile.ai[1];

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Otherwordly Bolt");
            ProjectileID.Sets.TrailingMode[projectile.type] = 2;
            ProjectileID.Sets.TrailCacheLength[projectile.type] = 4;
        }

        public override void SetDefaults()
        {
            projectile.width = projectile.height = 18;
            projectile.hostile = true;
            projectile.ignoreWater = true;
            projectile.penetrate = -1;
            projectile.tileCollide = false;
            projectile.timeLeft = 300;
        }
        
        public static Vector2 DetermineVelocity(Vector2 old, float idealDirection)
        {
            return (Vector2.Lerp(old, idealDirection.ToRotationVector2() * old.Length(), 0.0132f) * 1.03f).ClampMagnitude(0f, 30f);
        }

        public static Vector2 SimulateMotion(Vector2 startingPosition, Vector2 startingVelocity, float idealDirection, int frames)
        {
            Vector2 endingPosition = startingPosition;
            Vector2 velocity = startingVelocity;
            for (int i = 0; i < frames; i++)
            {
                endingPosition += velocity;
                velocity = DetermineVelocity(velocity, idealDirection);
            }
            return endingPosition;
        }

        public override void AI()
        {
            projectile.velocity = DetermineVelocity(projectile.velocity, IdealDirection);
            projectile.Opacity = Utils.InverseLerp(0f, 20f, Time, true);
            projectile.rotation = projectile.velocity.ToRotation() + MathHelper.PiOver2;
            Time++;
        }

        public override bool CanDamage() => projectile.Opacity >= 0.1f;

        public override Color? GetAlpha(Color lightColor) => new Color(255, 150, 255, 108) * projectile.Opacity;

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Texture2D texture = Main.projectileTexture[projectile.type];
            Utilities.DrawAfterimagesCentered(projectile, lightColor, ProjectileID.Sets.TrailingMode[projectile.type], 1, texture);
            return false;
        }
    }
}
