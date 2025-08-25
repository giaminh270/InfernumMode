using CalamityMod;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using CalamityMod.Buffs.DamageOverTime;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.DoG
{
    public class HomingDoGBurst : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Death Fire");
            Main.projFrames[projectile.type] = 6;
            ProjectileID.Sets.TrailCacheLength[projectile.type] = 3;
            ProjectileID.Sets.TrailingMode[projectile.type] = 0;
        }

        public override void SetDefaults()
        {
            projectile.width = 30;
            projectile.height = 30;
            projectile.hostile = true;
            projectile.ignoreWater = true;
            projectile.tileCollide = false;
            projectile.alpha = 255;
            projectile.penetrate = -1;
            projectile.timeLeft = 300;
        }

        public override void AI()
        {
            // Play a shoot sound.
            if (projectile.localAI[0] == 0f)
            {
                projectile.localAI[0] = 1f;
                Main.PlaySound(SoundID.Item20, projectile.position);
            }

            projectile.frameCounter++;
            projectile.frame = projectile.frameCounter / 6 % Main.projFrames[projectile.type];
            projectile.Opacity = Utils.InverseLerp(300f, 285f, projectile.timeLeft, true) * Utils.InverseLerp(0f, 35f, projectile.timeLeft, true);

            projectile.rotation = projectile.velocity.ToRotation() + MathHelper.PiOver2;
            if (projectile.velocity.Length() < 38f)
                projectile.velocity *= 1.022f;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Utilities.DrawAfterimagesCentered(projectile, Color.White, ProjectileID.Sets.TrailingMode[projectile.type], 1);
            return false;
        }
        public override void OnHitPlayer(Player target, int damage, bool crit)
        {
            target.AddBuff(ModContent.BuffType<GodSlayerInferno>(), 90);
            target.AddBuff(BuffID.Frostburn, 90, true);
            target.AddBuff(BuffID.Darkness, 90, true);
		}
		
        public override void Kill(int timeLeft) => Main.PlaySound(SoundID.Item, (int)projectile.position.X, (int)projectile.position.Y, 74);	
    }
}
