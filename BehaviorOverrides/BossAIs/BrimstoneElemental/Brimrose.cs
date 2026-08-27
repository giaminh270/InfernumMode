using InfernumMode.Sounds;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.BrimstoneElemental
{
    public class Brimrose : ModProjectile
    {
        public const int Lifetime = 420;

        public ref float Time => ref projectile.ai[0];

        public override void SetStaticDefaults() => DisplayName.SetDefault("Brimrose");

        public override void SetDefaults()
        {
            projectile.width = 82;
            projectile.height = 126;
            projectile.ignoreWater = true;
            projectile.tileCollide = true;
            projectile.penetrate = -1;
            projectile.timeLeft = Lifetime;
        }

        public override void AI()
        {
            // Choose a direction on the first frame.
            if (projectile.localAI[0] == 0f)
            {
                projectile.spriteDirection = Main.rand.NextBool().ToDirectionInt();
                projectile.localAI[0] = 1f;
                projectile.netUpdate = true;
            }

            // Fall to the side if on ground.
            if (Math.Abs(projectile.velocity.Y) <= 0.41f)
            {
                projectile.rotation = MathHelper.Lerp(projectile.rotation + projectile.spriteDirection * 0.08f, projectile.spriteDirection * MathHelper.PiOver2, 0.1f);
                projectile.rotation = MathHelper.Clamp(projectile.rotation, -MathHelper.PiOver2, MathHelper.PiOver2);
            }
            else
                projectile.rotation = 0f;

            // Fade away over time.
            projectile.Opacity = Utils.InverseLerp(0f, 120f, projectile.timeLeft, true);

            projectile.gfxOffY = Math.Abs(projectile.rotation / MathHelper.PiOver2) * 24f;

            // Fall down.
            projectile.velocity.Y = MathHelper.Clamp(projectile.velocity.Y + 0.4f, -8f, 20f);
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (projectile.localAI[1] == 0f)
            {
                Main.PlaySound(InfernumSoundRegistry.BrimstoneElementalShellGroundHit, projectile.Center);
                projectile.localAI[1] = 1f;
            }
            return false;
        }
    }
}
