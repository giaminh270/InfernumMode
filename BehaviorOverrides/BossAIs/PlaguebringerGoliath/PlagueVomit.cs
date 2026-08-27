using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.PlaguebringerGoliath
{
    public class PlagueVomit : ModProjectile
    {
        public ref float Time => ref projectile.ai[0];
        public override void SetStaticDefaults() => DisplayName.SetDefault("Plague Vomit");

        public override void SetDefaults()
        {
            projectile.width = projectile.height = 24;
            projectile.hostile = true;
            projectile.ignoreWater = true;
            projectile.tileCollide = true;
            projectile.penetrate = -1;
            projectile.timeLeft = 120;
            cooldownSlot = 1;
        }

        public override void AI()
        {
            projectile.Opacity = Utils.InverseLerp(0f, 10f, Time, true);
            projectile.rotation = projectile.velocity.ToRotation() + MathHelper.PiOver2;
            Time++;
        }

        public override void Kill(int timeLeft)
        {
            Main.PlaySound(SoundID.Item42, projectile.Center);
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            for (int i = 0; i < 7; i++)
            {
                Vector2 seekerVelocity = (MathHelper.TwoPi * (i + 0.5f) / 7f).ToRotationVector2() * 13.5f;
                Utilities.NewProjectileBetter(projectile.Center, seekerVelocity, ModContent.ProjectileType<HostilePlagueSeeker>(), PlaguebringerGoliathBehaviorOverride.PlagueSeekerDamage, 0f);
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            projectile.DrawProjectileWithBackglowTemp(Color.White, lightColor, 4f);
            return false;
        }

        public override Color? GetAlpha(Color lightColor) => Color.Lerp(Color.White, Color.DarkGreen, Utils.InverseLerp(45f, 0f, projectile.timeLeft, true)) * projectile.Opacity;

        public override bool CanDamage() => projectile.Opacity >= 0.8f;
    }
}
