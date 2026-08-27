using CalamityMod;
using InfernumMode.GlobalInstances;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.SupremeCalamitas
{
    public class DemonicTelegraphLine : ModProjectile
    {
        public bool DontMakeProjectile
        {
            get => projectile.localAI[1] == 1f;
            set => projectile.localAI[1] = value.ToInt();
        }

        public ref float Time => ref projectile.ai[0];

        public ref float Lifetime => ref projectile.ai[1];

        public ref float BombRadius => ref projectile.localAI[0];

        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public override void SetStaticDefaults() => DisplayName.SetDefault("Telegraph");

        public override void SetDefaults()
        {
            projectile.width = projectile.height = 2;
            projectile.ignoreWater = true;
            projectile.tileCollide = false;
            projectile.penetrate = -1;
            projectile.timeLeft = 900;
            cooldownSlot = 1;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(BombRadius);
            writer.Write(DontMakeProjectile);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            BombRadius = reader.ReadSingle();
            DontMakeProjectile = reader.ReadBoolean();
        }

        public override void AI()
        {
            projectile.Opacity = CalamityUtils.Convert01To010(Time / Lifetime) * 3f;
            if (projectile.Opacity > 1f)
                projectile.Opacity = 1f;
            if (Time >= Lifetime)
                projectile.Kill();

            Time++;
        }

        public override void Kill(int timeLeft)
        {
            if (DontMakeProjectile)
                return;

            Main.PlaySound(SoundID.Item74, projectile.Center);
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                Vector2 bombShootVelocity = projectile.velocity.SafeNormalize(Vector2.UnitY) * 19.5f;

                ProjectileSpawnManagementSystem.PrepareProjectileForSpawning(bomb =>
                {
                    bomb.timeLeft = Main.rand.Next(135, 185);
                });
                Utilities.NewProjectileBetter(projectile.Center, bombShootVelocity, ModContent.ProjectileType<DemonicBomb>(), 0, 0f, -1, BombRadius);
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            float telegraphWidth = MathHelper.Lerp(0.3f, 3f, CalamityUtils.Convert01To010(Time / Lifetime));

            // Draw a telegraph line outward.
            Vector2 start = projectile.Center - projectile.velocity.SafeNormalize(Vector2.UnitY) * 3000f;
            Vector2 end = projectile.Center + projectile.velocity.SafeNormalize(Vector2.UnitY) * 3000f;
            Main.spriteBatch.DrawLineBetter(start, end, Color.Red, telegraphWidth);
            return false;
        }
    }
}