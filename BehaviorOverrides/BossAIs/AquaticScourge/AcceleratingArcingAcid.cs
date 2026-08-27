using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.AquaticScourge
{
    public class AcceleratingArcingAcid : ModProjectile
    {
        public ref float Time => ref projectile.ai[0];

        public ref float ArcAngularVelocity => ref projectile.ai[1];

        public Player ClosestPlayer => Main.player[Player.FindClosest(projectile.Center, 1, 1)];

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Acid");
            ProjectileID.Sets.TrailCacheLength[projectile.type] = 5;
            ProjectileID.Sets.TrailingMode[projectile.type] = 0;
        }

        public override void SetDefaults()
        {
            projectile.width = 18;
            projectile.height = 20;
            projectile.hostile = true;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;
            projectile.timeLeft = 240;
            
            cooldownSlot = 1;
        }

        public override void AI()
        {
            projectile.Opacity = Utils.InverseLerp(0f, 35f, Time, true) * Utils.InverseLerp(0f, 56f, projectile.timeLeft, true);
            Time++;

            // Arc and accelerate.
            if (Time >= 15f)
                projectile.velocity = projectile.velocity.RotatedBy(ArcAngularVelocity);
            if (projectile.velocity.Length() < 16f)
                projectile.velocity *= 1.016f;
            projectile.rotation = projectile.velocity.ToRotation() - MathHelper.PiOver2;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Texture2D texture = Main.projectileTexture[projectile.type];
            Vector2 drawPosition = projectile.position + projectile.Size * 0.5f - Main.screenPosition;
            Vector2 origin = texture.Size() * 0.5f;
            Color backAfterimageColor = projectile.GetAlpha(new Color(85, 224, 60, 0) * 0.5f);
            for (int i = 0; i < 8; i++)
            {
                Vector2 drawOffset = (MathHelper.TwoPi * i / 8f).ToRotationVector2() * 4f;
                Main.spriteBatch.Draw(texture, drawPosition + drawOffset, null, backAfterimageColor, projectile.rotation, origin, projectile.scale, 0, 0f);
            }
            Utilities.DrawAfterimagesCentered(projectile, new Color(117, 95, 133, 184) * projectile.Opacity, ProjectileID.Sets.TrailingMode[projectile.type], 2);

            return false;
        }
    }
}
