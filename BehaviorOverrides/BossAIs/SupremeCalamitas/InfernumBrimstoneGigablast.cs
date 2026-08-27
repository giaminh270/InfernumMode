using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Dusts;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using CalamityMod;
using CalamityMod.Projectiles.Boss;

namespace InfernumMode.BehaviorOverrides.BossAIs.SupremeCalamitas
{
    public class InfernumBrimstoneGigablast : ModProjectile
    {
        public ref float Time => ref projectile.ai[0];

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Brimstone Gigablast");
            Main.projFrames[projectile.type] = 6;
        }

        public override void SetDefaults()
        {
            projectile.width = 50;
            projectile.height = 50;
            projectile.hostile = true;
            projectile.ignoreWater = true;
            projectile.penetrate = 1;
            projectile.timeLeft = 90;
            projectile.Opacity = 0f;
            projectile.Calamity().canBreakPlayerDefense = true;
            cooldownSlot = 1;
        }

        public override void AI()
        {
            projectile.frameCounter++;
            projectile.frame = projectile.frameCounter / 5 % Main.projFrames[projectile.type];
            projectile.Opacity = Utils.InverseLerp(0f, 20f, Time, true) * Utils.InverseLerp(0f, 20f, projectile.timeLeft, true);
            projectile.rotation = projectile.velocity.ToRotation() + MathHelper.PiOver2;

            Lighting.AddLight(projectile.Center, projectile.Opacity * 0.9f, 0f, 0f);

            if (projectile.localAI[0] == 0f)
            {
                Main.PlaySound(SoundID.Item20, projectile.Center);
                projectile.localAI[0] = 1f;
            }

            Player target = Main.player[Player.FindClosest(projectile.Center, 1, 1)];
            float flySpeed = projectile.velocity.Length();
            projectile.velocity = (projectile.velocity * 24f + projectile.SafeDirectionTo(target.Center) * flySpeed) / 25f;
            projectile.velocity = projectile.velocity.SafeNormalize(Vector2.UnitY) * flySpeed;
            Time++;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Texture2D texture = ModContent.GetTexture(Texture);
            int frameHeight = texture.Height / Main.projFrames[projectile.type];
            int drawStart = frameHeight * projectile.frame;
            lightColor.R = (byte)(255 * projectile.Opacity);
            Vector2 drawPosition = projectile.Center - Main.screenPosition + new Vector2(0f, projectile.gfxOffY);
            Rectangle frame = new Rectangle(0, drawStart, texture.Width, frameHeight);
            Main.spriteBatch.Draw(texture, drawPosition, frame, projectile.GetAlpha(lightColor), projectile.rotation, frame.Size() * 0.5f, projectile.scale, 0, 0);
            return false;
        }

        public override bool CanHitPlayer(Player target) => projectile.Opacity == 1f;

        public override void OnHitPlayer(Player target, int damage, bool crit)
        {
            if (projectile.Opacity != 1f)
                return;

            target.AddBuff(ModContent.BuffType<VulnerabilityHex>(), 180, true);
        }

        public override void Kill(int timeLeft)
        {
			Main.PlaySound(InfernumMode.CalamityMod.GetLegacySoundSlot(SoundType.Custom, "Sounds/Custom/SCalSounds/BrimstoneGigablastImpact"), projectile.Center);

            if (projectile.owner == Main.myPlayer)
            {
                int barrageCount = 45;
                if (projectile.ai[1] >= 2f)
                    barrageCount = (int)projectile.ai[1];

                for (int i = 0; i < barrageCount; i++)
                {
                    Vector2 dartVelocity = (MathHelper.TwoPi * i / barrageCount + projectile.AngleTo(Main.player[projectile.owner].Center)).ToRotationVector2() * 5f;
                    Projectile.NewProjectile(projectile.Center, dartVelocity, ModContent.ProjectileType<BrimstoneBarrage>(), projectile.damage, projectile.knockBack, projectile.owner, 0f, 1f);
                }
            }

            for (int j = 0; j < 2; j++)
            {
                Dust.NewDust(projectile.position, projectile.width, projectile.height, (int)CalamityDusts.Brimstone, 0f, 0f, 50, default, 1f);
            }
            for (int k = 0; k < 8; k++)
            {
                int redFire = Dust.NewDust(projectile.position, projectile.width, projectile.height, (int)CalamityDusts.Brimstone, 0f, 0f, 0, default, 1.5f);
                Main.dust[redFire].noGravity = true;
                Main.dust[redFire].velocity *= 3f;
                redFire = Dust.NewDust(projectile.position, projectile.width, projectile.height, (int)CalamityDusts.Brimstone, 0f, 0f, 50, default, 1f);
                Main.dust[redFire].velocity *= 2f;
                Main.dust[redFire].noGravity = true;
            }
        }
    }
}
