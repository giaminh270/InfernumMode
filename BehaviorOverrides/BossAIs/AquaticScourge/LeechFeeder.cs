using CalamityMod;
using CalamityMod.Dusts;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.AquaticScourge
{
    public class LeechFeeder : ModProjectile
    {
        public ref float Time => ref projectile.ai[0];

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Feeder");
            Main.projFrames[projectile.type] = 4;
            ProjectileID.Sets.TrailCacheLength[projectile.type] = 5;
            ProjectileID.Sets.TrailingMode[projectile.type] = 0;
        }

        public override void SetDefaults()
        {
            projectile.width = 18;
            projectile.height = 20;
            projectile.hostile = true;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;
            projectile.timeLeft = 480;
            projectile.Opacity = 0f;
            
            cooldownSlot = 1;
        }

        public override void AI()
        {
            // Decide frames.
            projectile.frameCounter++;
            projectile.frame = projectile.frameCounter / 5 % Main.projFrames[projectile.type];

            // Be invisible outside of water.
            if (!Collision.WetCollision(projectile.TopLeft, projectile.width, projectile.height))
                projectile.Opacity = MathHelper.Clamp(projectile.Opacity - 0.06f, 0f, 1f);

            // Look at the target.
            NPC target = Main.npc[(int)projectile.ai[0]];

            // Disappear if the target has no more flesh to consume or no longer exists.
            if (!target.active || target.localAI[1] >= 1f)
            {
                projectile.velocity = projectile.SafeDirectionTo(target.Center) * -8f;
                projectile.Opacity -= 0.02f;
            }

            // Consume the flesh of the target if there is any.
            else
            {
                if (!projectile.WithinRange(target.Center, 45f))
                    projectile.velocity = Vector2.Lerp(projectile.velocity, projectile.SafeDirectionTo(target.Center) * 6f, 0.08f);
                if (projectile.Hitbox.Intersects(target.Hitbox))
                {
                    target.localAI[1] += 0.01f;
                    Dust.NewDustDirect(target.TopLeft, target.width, target.height, (int)CalamityDusts.SulfurousSeaAcid);

                    if (Main.rand.NextBool(150))
                        Main.PlaySound(SoundID.Item17, projectile.Center);
                }

                projectile.Opacity += 0.02f;
            }

            projectile.spriteDirection = Math.Sign(projectile.velocity.X);
        }
    }
}
