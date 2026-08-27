using CalamityMod.DataStructures;
using InfernumMode.DataStructures;
using InfernumMode.ExtraTextures;
using InfernumMode.Projectiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.ProfanedGuardians
{
    public class LingeringHolyFire : ModProjectile, IAdditiveDrawer
    {
        public ref float Time => ref projectile.ai[0];

        public override string Texture => InfernumTextureRegistry.InvisPath;

        public override void SetStaticDefaults() => DisplayName.SetDefault("Holy Fire");

        public override void SetDefaults()
        {
            projectile.width = projectile.height = 70;
            projectile.hostile = true;
            projectile.ignoreWater = true;
            projectile.tileCollide = false;
            projectile.penetrate = -1;
            projectile.timeLeft = 120;
            projectile.alpha = 255;
            projectile.hide = true;
            cooldownSlot = 1;
        }

        public override void AI()
        {
            // Fade in.
            projectile.scale = (float)Math.Sin(Time / 150f * MathHelper.Pi) * 4f;
            if (projectile.scale > 1f)
                projectile.scale = 1f;
            projectile.Opacity = projectile.scale;
            projectile.scale *= MathHelper.Lerp(0.8f, 1.1f, projectile.identity % 9f / 9f);
            //projectile.Size = Vector2.One * projectile.scale * 200f;
            projectile.velocity *= 0.98f;
            projectile.rotation += MathHelper.Clamp(projectile.velocity.X * 0.04f, -0.06f, 0.06f);

            Time++;
        }

        public void AdditiveDraw(SpriteBatch spriteBatch)
        {
            Texture2D texture = InfernumTextureRegistry.Cloud2;
            Color color = projectile.GetAlpha(Color.White);
            Vector2 drawPosition = projectile.Center - Main.screenPosition;
            spriteBatch.Draw(texture, drawPosition, null, color, projectile.rotation, texture.Size() * 0.5f, projectile.scale * 0.6f, 0, 0f);
            spriteBatch.Draw(texture, drawPosition, null, Color.White * projectile.Opacity * 0.7f, projectile.rotation, texture.Size() * 0.6f, projectile.scale * 0.5f, 0, 0f);
        }

        public override Color? GetAlpha(Color lightColor)
        {
            Color c = Color.Lerp(WayfinderSymbol.Colors[1], WayfinderSymbol.Colors[0], projectile.identity % 10f / 16f);
            return c * 1.15f;
        }
    }
}
