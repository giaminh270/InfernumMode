using CalamityMod;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Dusts;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.AstrumDeus
{
    public class AstralPlasmaFireball : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Astral Plasma Flame");
            Main.projFrames[projectile.type] = 6;
            ProjectileID.Sets.TrailCacheLength[projectile.type] = 4;
            ProjectileID.Sets.TrailingMode[projectile.type] = 0;
        }

        public override void SetDefaults()
        {
            projectile.width = 36;
            projectile.height = 36;
            projectile.hostile = true;
            projectile.ignoreWater = true;
            projectile.tileCollide = false;
            projectile.Opacity = 0f;
            projectile.penetrate = -1;
            projectile.timeLeft = 105;
            cooldownSlot = 1;
        }

        public override void AI()
        {
            // Handle frames and rotation.
            projectile.frameCounter++;
            projectile.frame = projectile.frameCounter / 5 % Main.projFrames[projectile.type];
            projectile.rotation = projectile.velocity.ToRotation() - MathHelper.PiOver2;

            Lighting.AddLight(projectile.Center, Color.Yellow.ToVector3() * projectile.scale * 0.5f);

            // Fade in.
            projectile.Opacity = MathHelper.Clamp(projectile.Opacity + 0.1f, 0f, 1f);
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            lightColor.R = (byte)(255 * projectile.Opacity);
            lightColor.G = (byte)(255 * projectile.Opacity);
            lightColor.B = (byte)(255 * projectile.Opacity);
            Utilities.DrawAfterimagesCentered(projectile, lightColor, ProjectileID.Sets.TrailingMode[projectile.type], 1);
            return false;
        }

        public override void OnHitPlayer(Player target, int damage, bool crit) => target.AddBuff(ModContent.BuffType<AstralInfectionDebuff>(), 240);

        public override void Kill(int timeLeft)
        {
            Main.PlaySound(SoundID.Item93, projectile.Center);

            // Release plasma bolts.
            if (Main.netMode != NetmodeID.MultiplayerClient && projectile.ai[1] != -1f)
            {
                int totalProjectiles = 6;
                int type = ModContent.ProjectileType<AstralPlasmaSpark>();
                Vector2 spinningPoint = Main.rand.NextVector2Circular(8f, 8f);
                for (int i = 0; i < totalProjectiles; i++)
                {
                    Vector2 shootVelocity = spinningPoint.RotatedBy(MathHelper.TwoPi / totalProjectiles * i);
                    Projectile.NewProjectile(projectile.Center, shootVelocity, type, (int)(projectile.damage * 0.85), 0f, Main.myPlayer);
                }
            }

            for (int i = 0; i < 40; i++)
            {
                float dustSpeed = 16f;
                if (i < 50)
                    dustSpeed = 6f;
                if (i < 33)
                    dustSpeed = 4f;
                if (i < 13)
                    dustSpeed = 2f;

                float scale = 1f;
                Dust astralPlasma = Dust.NewDustDirect(projectile.Center, 6, 6, Main.rand.NextBool(2) ? 107 : 110, 0f, 0f, 100, default, 1f);
                switch ((int)dustSpeed)
                {
                    case 4:
                        scale = 1.2f;
                        break;
                    case 8:
                        scale = 1.1f;
                        break;
                    case 12:
                        scale = 1f;
                        break;
                    case 16:
                        scale = 0.9f;
                        break;
                    default:
                        break;
                }

                astralPlasma.color = Color.Lerp(Color.Yellow, Color.Red, Main.rand.NextFloat(0.7f));
                astralPlasma.velocity *= 0.5f;
                astralPlasma.velocity += astralPlasma.velocity.SafeNormalize(Vector2.UnitY) * dustSpeed;
                astralPlasma.scale = scale;
                astralPlasma.noGravity = true;
            }

            for (int i = 0; i < 2; i++)
                Dust.NewDustDirect(projectile.position, projectile.width, projectile.height, ModContent.DustType<AstralOrange>(), 0f, 0f, 50, default, 1f);

            for (int i = 0; i < 5; i++)
            {
                Dust fire = Dust.NewDustDirect(projectile.position, projectile.width, projectile.height, ModContent.DustType<AstralOrange>(), 0f, 0f, 0, default, 1.5f);
                fire.noGravity = true;
                fire.velocity *= 1.5f;
            }
        }
    }
}
