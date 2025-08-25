using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.StormWeaver
{
    public class StormWeaverFrostWaveTelegraph : ModProjectile
    {
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Frost Wave Telegraph");
        }

        public override void SetDefaults()
        {
            projectile.width = 32;
            projectile.height = 32;
            projectile.ignoreWater = true;
            projectile.penetrate = -1;
            projectile.timeLeft = 300;
            projectile.tileCollide = false;
            projectile.extraUpdates = 4;
            projectile.Opacity = 1f;
        }

        public override void AI()
        {
            if (projectile.velocity.Length() < projectile.ai[1])
            {
                projectile.velocity *= 1.01f;
                if (projectile.velocity.Length() > projectile.ai[1])
                {
                    projectile.velocity.Normalize();
                    projectile.velocity *= projectile.ai[1];
                }
            }

            if (projectile.timeLeft < 60)
                projectile.Opacity = projectile.timeLeft / 60f;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Texture2D pulseTexture = ModContent.GetTexture("CalamityMod/Projectiles/DraedonsArsenal/PulseAura");

            // Aura drawing.
            for (int i = 0; i < 5; i++)
            {
                Vector2 offset = (i / 5f * MathHelper.TwoPi).ToRotationVector2() * 24f;
                float time = (float)Math.Sin(Main.GlobalTime * 1.8f);
                float angle = time * MathHelper.Pi + Main.GlobalTime * 2.1f;
                float scale = 1.1f + time * 0.2f;
                Main.spriteBatch.Draw(pulseTexture, projectile.Center + offset - Main.screenPosition, null, Color.LightCyan * 0.3f * projectile.Opacity, angle, pulseTexture.Size() * 0.5f, scale, SpriteEffects.None, 0);
            }

            return false;
        }
    }
}
