using CalamityMod;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.QueenBee
{
    public class TinyBee : ModProjectile
    {
        public ref float Time => ref projectile.ai[0];
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Bee");
            Main.projFrames[projectile.type] = 3;
        }

        public override void SetDefaults()
        {
            projectile.width = projectile.height = 40;
            projectile.ignoreWater = true;
            projectile.timeLeft = 240;
            projectile.scale = 1f;
            projectile.Opacity = 255;
            projectile.tileCollide = false;
            projectile.friendly = false;
            projectile.hostile = true;
            projectile.Calamity().canBreakPlayerDefense = true;
            cooldownSlot = 1;
        }

        public override void AI()
        {
            projectile.Opacity = Utils.Clamp(projectile.Opacity + 0.03f, 0f, 1f);
            projectile.rotation = MathHelper.Clamp(projectile.velocity.X * 0.15f, -0.7f, 0.7f);
            projectile.spriteDirection = (projectile.velocity.X < 0f).ToDirectionInt();

            if (Time < 80f)
                projectile.velocity = Vector2.Lerp(projectile.velocity, Vector2.UnitX * (projectile.velocity.X > 0f).ToDirectionInt() * 10f, 0.02f);

            projectile.frame = projectile.timeLeft / 4 % Main.projFrames[projectile.type];

            Time++;
        }

        public override void OnHitPlayer(Player target, int damage, bool crit) => target.AddBuff(BuffID.Poisoned, 90);

        public override void Kill(int timeLeft)
        {
            Main.PlaySound(SoundID.NPCDeath1, projectile.Center);
            for (int i = 0; i < 12; i++)
            {
                Dust honey = Dust.NewDustDirect(projectile.position, projectile.width, projectile.height, 147, 0f, 0f, 0, default, 0.8f);
                if (Main.rand.NextBool(2))
                    honey.scale *= 1.4f;
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            projectile.DrawProjectileWithBackglowTemp(Color.White * (float)Math.Pow(projectile.Opacity, 2f), lightColor, projectile.Opacity * 6f);
            return false;
        }
    }
}
