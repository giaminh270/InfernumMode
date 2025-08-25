using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.Destroyer
{
    public class EnergySpark2 : ModProjectile
    {
        public override void SetStaticDefaults() => DisplayName.SetDefault("Red Spark");

        public override void SetDefaults()
        {
            projectile.width = projectile.height = 18;
            projectile.penetrate = -1;
            projectile.tileCollide = false;
            projectile.hostile = true;
            projectile.timeLeft = 360;
            projectile.Opacity = 0f;
            projectile.hide = true;
			cooldownSlot = 1;
        }

        public override void AI()
        {
            projectile.Opacity = MathHelper.Clamp(projectile.Opacity + 0.1f, 0f, 1f);
            projectile.rotation = projectile.velocity.ToRotation() + MathHelper.PiOver2;

            if (projectile.timeLeft == 330)
                Main.PlaySound(SoundID.Item12, projectile.Center);
        }

        public override Color? GetAlpha(Color lightColor)
        {
            return new Color(255, 255, 255, 56) * projectile.Opacity;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            if (projectile.timeLeft > 330)
            {
                float width = (float)Math.Sin(MathHelper.Pi * Utils.InverseLerp(360f, 330f, projectile.timeLeft, true)) * 5f + 1f;
                Vector2 end = projectile.Center + projectile.velocity.SafeNormalize(Vector2.Zero) * 5100f;
                spriteBatch.DrawLineBetter(projectile.Center, end, Color.Red * 0.4f, width * 1.8f);
                spriteBatch.DrawLineBetter(projectile.Center, end, Color.White * 0.6f, width);
                return false;
            }

            Texture2D texture = Main.projectileTexture[projectile.type];
            Vector2 origin = texture.Size() * 0.5f;

            for (int i = 0; i < 7; i++)
            {
                Vector2 drawOffset = -projectile.velocity.SafeNormalize(Vector2.Zero) * i * 7f;
                Vector2 afterimageDrawPosition = projectile.Center + drawOffset - Main.screenPosition;
                Color backAfterimageColor = projectile.GetAlpha(lightColor) * ((7f - i) / 7f);
                spriteBatch.Draw(texture, afterimageDrawPosition, null, backAfterimageColor, projectile.rotation, origin, projectile.scale, SpriteEffects.None, 0f);
            }

            Color frontAfterimageColor = projectile.GetAlpha(lightColor) * 0.15f;
            for (int i = 0; i < 8; i++)
            {
                Vector2 drawOffset = (MathHelper.TwoPi * i / 8f + projectile.rotation - MathHelper.PiOver2).ToRotationVector2() * 4f;
                Vector2 afterimageDrawPosition = projectile.Center + drawOffset - Main.screenPosition;
                spriteBatch.Draw(texture, afterimageDrawPosition, null, frontAfterimageColor, projectile.rotation, origin, projectile.scale, SpriteEffects.None, 0f);
            }
            return false;
        }

        public override bool ShouldUpdatePosition() => projectile.timeLeft < 330;

        public override void DrawBehind(int index, List<int> drawCacheProjsBehindNPCsAndTiles, List<int> drawCacheProjsBehindNPCs, List<int> drawCacheProjsBehindProjectiles, List<int> drawCacheProjsOverWiresUI)
        {
            drawCacheProjsBehindProjectiles.Add(index);
        }
    }
}
