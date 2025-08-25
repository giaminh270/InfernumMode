using CalamityMod;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.Leviathan
{
    public class LeviathanMeteor : ModProjectile
    {
        public ref float Time => ref projectile.ai[1];

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Leviathan Meteor");
            ProjectileID.Sets.TrailingMode[projectile.type] = 2;
            ProjectileID.Sets.TrailCacheLength[projectile.type] = 4;
        }

        public override void SetDefaults()
        {
            projectile.width = 154;
            projectile.height = 154;
            projectile.hostile = true;
            projectile.ignoreWater = true;
            projectile.tileCollide = false;
            projectile.penetrate = -1;
            projectile.timeLeft = 125;
            projectile.Opacity = 0f;
            projectile.Calamity().canBreakPlayerDefense = true;
            cooldownSlot = 1;
        }
        
        public override void AI()
        {
            // Determine opacity and rotation.
            projectile.scale = Utils.InverseLerp(0f, 18f, Time, true);
            projectile.Opacity = Utils.InverseLerp(0f, 30f, projectile.timeLeft, true) * projectile.scale;
            projectile.rotation += projectile.velocity.X * 0.02f;
            projectile.velocity *= 0.987f;

            Lighting.AddLight(projectile.Center, 0f, 0f, 0.5f * projectile.Opacity);
            Time++;
        }

        public override bool CanHitPlayer(Player target) => projectile.scale >= 0.9f;

        public override void Kill(int timeLeft)
        {
            Main.PlaySound(SoundID.DD2_ExplosiveTrapExplode, projectile.Center);
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            for (int i = 0; i < 15; i++)
            {
                Vector2 shootVelocity = (MathHelper.TwoPi * i / 15f).ToRotationVector2() * 12f;
                Utilities.NewProjectileBetter(projectile.Center, shootVelocity, ModContent.ProjectileType<LeviathanVomit>(), 175, 0f);
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            lightColor.R = (byte)(255 * projectile.Opacity);
            lightColor.G = (byte)(255 * projectile.Opacity);
            lightColor.B = (byte)(255 * projectile.Opacity);
            CalamityUtils.DrawAfterimagesCentered(projectile, ProjectileID.Sets.TrailingMode[projectile.type], lightColor, 1);
            return false;
        }
    }
}
