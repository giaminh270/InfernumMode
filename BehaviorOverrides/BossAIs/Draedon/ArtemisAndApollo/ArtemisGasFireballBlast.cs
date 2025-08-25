using CalamityMod;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.Draedon.ArtemisAndApollo
{
    public class ArtemisGasFireballBlast : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Volatile Fireball Blast");
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
            projectile.tileCollide = true;
            projectile.penetrate = -1;
            projectile.Opacity = 0f;
            projectile.timeLeft = 105;
            projectile.Calamity().canBreakPlayerDefense = true;
            cooldownSlot = 1;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(projectile.localAI[0]);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            projectile.localAI[0] = reader.ReadSingle();
        }

        public override void AI()
        {
            projectile.Opacity = MathHelper.Clamp(projectile.Opacity + 0.35f, 0f, 1f);

            Lighting.AddLight(projectile.Center, 0f, 0.6f * projectile.Opacity, 0f);

            // Handle frames and rotation.
            projectile.frameCounter++;
            projectile.frame = projectile.frameCounter / 5 % Main.projFrames[projectile.type];
            projectile.rotation = projectile.velocity.ToRotation() - MathHelper.PiOver2;

            // Create a burst of dust on the first frame.
            if (projectile.localAI[0] == 0f)
            {
                for (int i = 0; i < 20; i++)
                {
                    Vector2 dustVelocity = projectile.velocity.SafeNormalize(Vector2.Zero).RotatedByRandom(0.35f) * Main.rand.NextFloat(1.8f, 3f);
                    int randomDustType = Main.rand.NextBool() ? 130 : 133;

                    Dust fire = Dust.NewDustDirect(projectile.position, projectile.width, projectile.height, randomDustType, dustVelocity.X, dustVelocity.Y, 200, default, 1.7f);
                    fire.position = projectile.Center + Main.rand.NextVector2Circular(projectile.width, projectile.width);
                    fire.noGravity = true;
                    fire.velocity *= 3f;

                    fire = Dust.NewDustDirect(projectile.position, projectile.width, projectile.height, randomDustType, dustVelocity.X, dustVelocity.Y, 100, default, 0.8f);
                    fire.position = projectile.Center + Main.rand.NextVector2Circular(projectile.width, projectile.width);
                    fire.velocity *= 2f;

                    fire.noGravity = true;
                    fire.fadeIn = 1f;
                    fire.color = Color.Green * 0.5f;
                }

                for (int i = 0; i < 5; i++)
                {
                    Vector2 dustVelocity = projectile.velocity.SafeNormalize(Vector2.Zero).RotatedByRandom(0.35f) * Main.rand.NextFloat(1.8f, 3f);
                    int randomDustType = Main.rand.NextBool() ? 130 : 133;

                    Dust fire = Dust.NewDustDirect(projectile.position, projectile.width, projectile.height, randomDustType, dustVelocity.X, dustVelocity.Y, 0, default, 2f);
                    fire.position = projectile.Center + Vector2.UnitX.RotatedByRandom(MathHelper.Pi).RotatedBy(projectile.velocity.ToRotation()) * projectile.width / 3f;
                    fire.noGravity = true;
                    fire.velocity *= 0.5f;
                }

                projectile.localAI[0] = 1f;
            }
        }

        public override bool CanHitPlayer(Player target) => projectile.Opacity == 1f;

        public override void OnHitPlayer(Player target, int damage, bool crit)
        {
            if (projectile.Opacity != 1f)
                return;

            target.AddBuff(BuffID.OnFire, 360);
            target.AddBuff(BuffID.CursedInferno, 180);
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            lightColor.R = (byte)(255 * projectile.Opacity);
            lightColor.G = (byte)(255 * projectile.Opacity);
            lightColor.B = (byte)(255 * projectile.Opacity);
            CalamityUtils.DrawAfterimagesCentered(projectile, ProjectileID.Sets.TrailingMode[projectile.type], lightColor, 1);
            return false;
        }

        public override void Kill(int timeLeft)
        {
            int height = 90;
            projectile.position = projectile.Center;
            projectile.width = projectile.height = height;
            projectile.Center = projectile.position;
            projectile.Damage();

            Main.PlaySound(SoundID.Item93, projectile.Center);

            // Release fire gas.
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                int type = ModContent.ProjectileType<SuperheatedExofireGas>();
                for (int i = 0; i < 30; i++)
                {
                    Vector2 fireVelocity = Main.rand.NextVector2Circular(15f, 15f);
                    Projectile.NewProjectile(projectile.Center, fireVelocity, type, projectile.damage, 0f, Main.myPlayer);
                }
            }
        }
    }
}
