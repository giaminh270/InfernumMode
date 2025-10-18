using CalamityMod;
using CalamityMod.Projectiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.Yharon
{
    public class YharonMajesticSparkle : ModProjectile
    {
        private static Texture2D sparkleTexture;
        
        public float Time
        {
            get => projectile.ai[0];
            set => projectile.ai[0] = value;
        }
        public float ColorSpectrumHue
        {
            get => projectile.ai[1];
            set => projectile.ai[1] = value;
        }
        public const int Lifetime = 90;
        public const int FadeinTime = 18;
        public const int FadeoutTime = 18;
        
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Majestic Sparkle");
            if (Main.netMode != NetmodeID.Server)
                sparkleTexture = ModContent.GetTexture(Texture);
        }

        public override void SetDefaults()
        {
            projectile.width = 72;
            projectile.height = 72;
            projectile.penetrate = -1;
            projectile.tileCollide = false;
            projectile.timeLeft = Lifetime;
            projectile.usesLocalNPCImmunity = true;
            projectile.localNPCHitCooldown = 10;
            projectile.scale = 0.001f;
            cooldownSlot = 1;
        }

        public override void AI()
        {
            if (Time == 1f)
            {
                projectile.scale = Main.rand.NextFloat(0.3f, 0.75f);
                CalamityGlobalProjectile.ExpandHitboxBy(projectile, (int)(72 * projectile.scale));
                ColorSpectrumHue = Main.rand.NextFloat(0f, 0.9999f);
                projectile.netUpdate = true;
                projectile.rotation = projectile.velocity.ToRotation() + MathHelper.PiOver2;
            }
            Time++;

            projectile.velocity *= 0.96f;
            projectile.rotation = projectile.rotation.AngleLerp(MathHelper.PiOver2, 0.085f);
            ColorSpectrumHue = (ColorSpectrumHue + 0.333f / Lifetime) % 0.999f;

            projectile.Opacity = Utils.InverseLerp(0f, FadeinTime, Time, true) * Utils.InverseLerp(Lifetime, Lifetime - FadeoutTime, Time, true);
            projectile.velocity = projectile.velocity.RotatedBy(Math.Sin(Time / 30f) * 0.0125f);
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            // Skip drawing if opacity is too low
            if (projectile.Opacity < 0.01f)
                return false;

            if (sparkleTexture == null)
                return false;

            // Pre-calculate values to avoid repeated calculations
            float opacity = projectile.Opacity;
            float scale = projectile.scale * opacity;
            
            Color sparkleColor = GetSparkleColor() * opacity * 0.5f;
            sparkleColor.A = 0;

            Color orthogonalsparkleColor = Color.Lerp(sparkleColor, Color.White, 0.5f) * 0.5f;
            Vector2 origin = sparkleTexture.Size() / 2f;

            // Reduce draw calls by combining similar operations
            DrawSparkle(spriteBatch, sparkleTexture, sparkleColor, orthogonalsparkleColor, origin, scale);
            
            return false;
        }

        private Color GetSparkleColor()
        {
            float intensity = MathHelper.Lerp(1f, 1.5f, Utils.InverseLerp(Lifetime * 0.5f - 15f, Lifetime * 0.5f + 15f, Time, true));
            return CalamityUtils.MulticolorLerp(ColorSpectrumHue, Color.Orange, Color.Purple, Color.Pink, Color.Green) * intensity;
        }

        private void DrawSparkle(SpriteBatch spriteBatch, Texture2D texture, Color mainColor, Color orthogonalColor, Vector2 origin, float baseScale)
        {
            Vector2 center = projectile.Center - Main.screenPosition + Vector2.UnitY * projectile.gfxOffY;
            float rotation = projectile.rotation;
            
            // Main orthogonal sparkle
            spriteBatch.Draw(texture, center, null, mainColor, MathHelper.PiOver2 + rotation, origin, 
                            new Vector2(0.3f, 2f) * baseScale, SpriteEffects.None, 0f);
            
            // Main sparkle
            spriteBatch.Draw(texture, center, null, mainColor, rotation, origin, 
                            new Vector2(0.3f, 1f) * baseScale, SpriteEffects.None, 0f);
            
            // Reduced orthogonal sparkle (only draw if opacity is significant)
            if (projectile.Opacity > 0.3f)
            {
                spriteBatch.Draw(texture, center, null, orthogonalColor, MathHelper.PiOver2 + rotation, origin, 
                                new Vector2(0.3f, 2f) * baseScale * 0.6f, SpriteEffects.None, 0f);
                
                spriteBatch.Draw(texture, center, null, orthogonalColor, rotation, origin, 
                                new Vector2(0.3f, 1f) * baseScale * 0.6f, SpriteEffects.None, 0f);
            }
        }
    }
}