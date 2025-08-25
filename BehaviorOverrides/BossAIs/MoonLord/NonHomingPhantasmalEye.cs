using CalamityMod.Dusts;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.MoonLord
{
    public class NonHomingPhantasmalEye : ModProjectile
    {
        public ref float Time => ref projectile.ai[0];

        public ref float SpinSpeed => ref projectile.ai[1];

        public override void SetStaticDefaults()
        {
            // HOLY SHIT IS THAT A FARGO REFERENCE OH MY GOD I AM GOING TO CANCEL DOMINIC VON KARMA FOR THIS
            DisplayName.SetDefault("Phantasmal Eye");
            ProjectileID.Sets.TrailingMode[projectile.type] = 0;
            ProjectileID.Sets.TrailCacheLength[projectile.type] = 3;
        }

        public override void SetDefaults()
        {
            projectile.width = projectile.height = 16;
            projectile.hostile = true;
            projectile.ignoreWater = true;
            projectile.tileCollide = false;
            projectile.penetrate = -1;
            projectile.timeLeft = 420;
            projectile.alpha = 225;
            cooldownSlot = 1;
        }

        public override void AI()
        {
            projectile.velocity = projectile.velocity.RotatedBy(SpinSpeed);
            projectile.rotation = projectile.velocity.ToRotation() + MathHelper.PiOver2;
            projectile.Opacity = MathHelper.Clamp(projectile.Opacity + 0.08f, 0f, 1f);
            Lighting.AddLight(projectile.Center, 0f, projectile.Opacity * 0.4f, projectile.Opacity * 0.4f);

            Time++;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor) => true;

        public override void Kill(int timeLeft)
        {
            Main.PlaySound(SoundID.Item20, projectile.Center);
            for (int dust = 0; dust < 2; dust++)
                Dust.NewDust(projectile.position + projectile.velocity, projectile.width, projectile.height, (int)CalamityDusts.Nightwither, 0f, 0f);
        }

        public override void ModifyHitPlayer(Player target, ref int damage, ref bool crit)
        {
            
        }
    }
}
