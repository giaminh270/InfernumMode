using System;
using CalamityMod;
using InfernumMode.Effects;
using InfernumMode.ExtraTextures;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.AquaticScourge
{
    public class FallingAcid : ModProjectile
    {
        public ref float Time => ref projectile.ai[0];

        public bool Telegraphed => projectile.ai[1] == 1f;

        public static int TelegraphTime => 30;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Acid");
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
            
            cooldownSlot = 1;
        }

        public override void AI()
        {
            projectile.Opacity = Utils.InverseLerp(0f, 35f, Time, true) * Utils.InverseLerp(0f, 56f, projectile.timeLeft, true);

            // Fall downward.
            projectile.velocity.X *= 0.987f;
            projectile.velocity.Y = MathHelper.Clamp(projectile.velocity.Y + 0.4f, -40f, 13f);
            projectile.rotation = projectile.velocity.ToRotation() - MathHelper.PiOver2;
            Time++;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            // Cast a telegraph line before the acceleration gets super strong if necessary.
            float opacity = CalamityUtils.Convert01To010(Time / TelegraphTime);
            Vector2 drawPosition = projectile.position + projectile.Size * 0.5f - Main.screenPosition;
            if (opacity > 0f && Telegraphed)
            {
                Texture2D invisible = InfernumTextureRegistry.Invisible;
                Effect laserScopeEffect = InfernumEffectsRegistry.PixelatedSightLine.GetShader().Shader;
                laserScopeEffect.Parameters["sampleTexture2"].SetValue(ModContent.GetTexture("InfernumMode/ExtraTextures/GreyscaleGradients/CertifiedCrustyNoise"));
                laserScopeEffect.Parameters["noiseOffset"].SetValue(Main.GameUpdateCount * -0.003f);
                laserScopeEffect.Parameters["mainOpacity"].SetValue((float)Math.Sqrt(opacity));
                laserScopeEffect.Parameters["Resolution"].SetValue(new Vector2(425f));
                laserScopeEffect.Parameters["laserAngle"].SetValue(-projectile.velocity.ToRotation());
                laserScopeEffect.Parameters["laserWidth"].SetValue(0.003f + (float)Math.Pow(opacity, 4f) * ((float)Math.Sin(Main.GlobalTime * 3f) * 0.001f + 0.001f));
                laserScopeEffect.Parameters["laserLightStrenght"].SetValue(5f);
                laserScopeEffect.Parameters["color"].SetValue(Color.Lerp(Color.Lime, Color.Olive, 0.7f).ToVector3());
                laserScopeEffect.Parameters["darkerColor"].SetValue(Color.Lerp(Color.Yellow, Color.SaddleBrown, 0.8f).ToVector3());
                laserScopeEffect.Parameters["bloomSize"].SetValue(0.06f + (1f - opacity) * 0.1f);
                laserScopeEffect.Parameters["bloomMaxOpacity"].SetValue(0.4f);
                laserScopeEffect.Parameters["bloomFadeStrenght"].SetValue(3f);

                Main.spriteBatch.EnterShaderRegion(BlendState.Additive);

                laserScopeEffect.CurrentTechnique.Passes[0].Apply();
                Main.spriteBatch.Draw(invisible, drawPosition, null, Color.White, 0f, invisible.Size() * 0.5f, opacity * 2500f, SpriteEffects.None, 0f);
                Main.spriteBatch.ExitShaderRegion();
            }

            Texture2D texture = Main.projectileTexture[projectile.type];
            Vector2 origin = texture.Size() * 0.5f;
            Color backAfterimageColor = projectile.GetAlpha(new Color(85, 224, 60, 0) * 0.5f);
            for (int i = 0; i < 8; i++)
            {
                Vector2 drawOffset = (MathHelper.TwoPi * i / 8f).ToRotationVector2() * 4f;
                Main.spriteBatch.Draw(texture, drawPosition + drawOffset, null, backAfterimageColor, projectile.rotation, origin, projectile.scale, 0, 0f);
            }
            Utilities.DrawAfterimagesCentered(projectile, new Color(117, 95, 133, 184) * projectile.Opacity, ProjectileID.Sets.TrailingMode[projectile.type], 2);

            return false;
        }
    }
}
