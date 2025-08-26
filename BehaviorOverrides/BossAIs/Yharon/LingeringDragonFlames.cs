using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.DataStructures;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using InfernumMode;

namespace InfernumMode.BehaviorOverrides.BossAIs.Yharon
{
    public class LingeringDragonFlames : ModProjectile, IAdditiveDrawer
    {
        public ref float Time => ref projectile.ai[0];
        public ref float LaserLength => ref projectile.ai[1];
        public override string Texture => "InfernumMode/ExtraTextures/Smoke";

        public override void SetStaticDefaults() => DisplayName.SetDefault("Dragonfire");

        public override void SetDefaults()
        {
            projectile.width = projectile.height = 112;
            projectile.hostile = true;
            projectile.ignoreWater = true;
            projectile.tileCollide = false;
            projectile.penetrate = -1;
            projectile.timeLeft = 150;
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
            projectile.Size = Vector2.One * projectile.scale * 200f;
            projectile.velocity *= 0.98f;
            projectile.rotation += MathHelper.Clamp(projectile.velocity.X * 0.04f, -0.06f, 0.06f);

            Time++;
        }

        public Color ColorFunction(float completionRatio)
        {
            Color color = Color.Lerp(Color.Orange, Color.DarkRed, (float)Math.Pow(completionRatio, 2D));
            color = Color.Lerp(color, Color.Red, 0.65f);
            return color * projectile.Opacity * 0.6f;
        }

        public void AdditiveDraw(SpriteBatch spriteBatch)
        {
            Texture2D texture = ModContent.GetTexture(Texture);
            Color color = projectile.GetAlpha(Color.White);
            Vector2 drawPosition = projectile.Center - Main.screenPosition;
            spriteBatch.Draw(texture, drawPosition, null, color, projectile.rotation, texture.Size() * 0.5f, projectile.scale, 0, 0f);
            spriteBatch.Draw(texture, drawPosition, null, Color.White * projectile.Opacity * 0.7f, projectile.rotation, texture.Size() * 0.5f, projectile.scale, 0, 0f);
        }

        public override Color? GetAlpha(Color lightColor)
        {
            Color c = Color.Lerp(Color.Orange, Color.Red, projectile.identity % 10f / 16f);
            return c * 1.15f;
        }

        public override void OnHitPlayer(Player target, int damage, bool crit)
        {
            target.AddBuff(ModContent.BuffType<LethalLavaBurn>(), 180);
        }
    }
}
