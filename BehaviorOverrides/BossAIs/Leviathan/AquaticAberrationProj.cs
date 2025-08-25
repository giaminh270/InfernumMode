using CalamityMod;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.Leviathan
{
    public class AquaticAberrationProj : ModProjectile
    {
        public ref float Time => ref projectile.ai[1];

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Aquatic Aberration");
            Main.projFrames[projectile.type] = 7;
            ProjectileID.Sets.TrailingMode[projectile.type] = 2;
            ProjectileID.Sets.TrailCacheLength[projectile.type] = 4;
        }

        public override void SetDefaults()
        {
            projectile.width = 54;
            projectile.height = 54;
            projectile.hostile = true;
            projectile.ignoreWater = true;
            projectile.tileCollide = true;
            projectile.penetrate = -1;
            projectile.timeLeft = 210;
            projectile.Opacity = 0f;
            cooldownSlot = 1;
        }

        public override void AI()
        {
            // Determine opacity.
            projectile.Opacity = Utils.InverseLerp(0f, 36f, Time, true);
            projectile.spriteDirection = (projectile.velocity.X < 0f).ToDirectionInt();

            // Determine frames and rotation.
            projectile.frameCounter++;
            projectile.frame = projectile.frameCounter / 5 % Main.projFrames[projectile.type];
            projectile.rotation = projectile.velocity.ToRotation();
            if (projectile.spriteDirection == 1)
                projectile.rotation += MathHelper.Pi;

            // Try to hover towards the target at first.
            if (Time < 54f)
            {
                float inertia = 16f;
                float oldSpeed = projectile.velocity.Length();
                Player target = Main.player[Player.FindClosest(projectile.Center, 1, 1)];
                Vector2 homingVelocity = projectile.SafeDirectionTo(target.Center) * oldSpeed;

                projectile.velocity = (projectile.velocity * (inertia - 1f) + homingVelocity) / inertia;
                projectile.velocity = projectile.velocity.SafeNormalize(-Vector2.UnitY) * oldSpeed;
            }
            Time++;

            Lighting.AddLight(projectile.Center, Vector3.One * projectile.Opacity * 0.5f);
        }

        public override void Kill(int timeLeft)
        {
            for (int k = 0; k < 5; k++)
                Dust.NewDust(projectile.position, projectile.width, projectile.height, DustID.Blood, Math.Sign(projectile.velocity.X), -1f, 0, default, 1f);

            Main.PlaySound(SoundID.NPCDeath12, projectile.Center);
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            for (int i = 0; i < 4; i++)
            {
                Vector2 shootVelocity = (MathHelper.TwoPi * i / 4f).ToRotationVector2() * 8.5f;
                Utilities.NewProjectileBetter(projectile.Center, shootVelocity, ModContent.ProjectileType<LeviathanVomit>(), 175, 0f);
            }
        }

        public override bool CanHitPlayer(Player target) => projectile.Opacity == 1f;

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            CalamityUtils.DrawAfterimagesCentered(projectile, ProjectileID.Sets.TrailingMode[projectile.type], lightColor, 1);
            return false;
        }
    }
}
