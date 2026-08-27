using CalamityMod.Projectiles.Boss;
using InfernumMode.Sounds;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.Yharon
{
    public class InfernadoSpawner : ModProjectile
    {
        public bool HomeInOnTarget => projectile.ai[0] == 1f;

        public ref float Time => ref projectile.ai[1];

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Big Flare");
            Main.projFrames[projectile.type] = 4;
            ProjectileID.Sets.TrailCacheLength[projectile.type] = 5;
            ProjectileID.Sets.TrailingMode[projectile.type] = 0;
        }

        public override void SetDefaults()
        {
            projectile.width = 100;
            projectile.height = 100;
            projectile.alpha = 255;
            projectile.penetrate = -1;
            projectile.timeLeft = 180;
            projectile.scale = 0.15f;
            projectile.tileCollide = false;
            cooldownSlot = 1;
        }

        public override void AI()
        {
            // Decide frames.
            projectile.frameCounter++;
            projectile.frame = projectile.frameCounter / 5 % Main.projFrames[projectile.type];

            // Fade in.
            projectile.Opacity = MathHelper.Clamp(projectile.Opacity + 0.04f, 0f, 1f);

            // Grow to maximum size.
            projectile.scale = MathHelper.Lerp(projectile.scale, 1.25f, 0.05f);

            // Move towards the target if necessary.
            if (HomeInOnTarget)
            {
                Player target = Main.player[Player.FindClosest(projectile.Center, 1, 1)];
                float flySpeed = Time * 0.145f + 9f;
                projectile.velocity = Vector2.Lerp(projectile.velocity, projectile.SafeDirectionTo(target.Center) * flySpeed, 0.062f);

                // Release the tornado if close enough to the target.
                if (projectile.WithinRange(target.Center, 64f))
                    projectile.Kill();
            }

            Time++;
        }

        public override Color? GetAlpha(Color lightColor)
        {
            return new Color(255, Main.DiscoG, 53, projectile.alpha);
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Utilities.DrawAfterimagesCentered(projectile, lightColor, ProjectileID.Sets.TrailingMode[projectile.type], 1);
            return false;
        }

        public override void Kill(int timeLeft)
        {
            Main.PlaySound(InfernumSoundRegistry.YharonInfernado);

            if (Main.netMode != NetmodeID.MultiplayerClient)
                Utilities.NewProjectileBetter(projectile.Center, Vector2.Zero, ModContent.ProjectileType<DraconicInfernado>(), YharonBehaviorOverride.InfernadoDamage, 0f);
        }
    }
}
