using System;
using CalamityMod;
using InfernumMode.Effects;
using InfernumMode.ExtraTextures;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.BrimstoneElemental
{
    public class RedFlameTelegraph : ModProjectile
    {
        public ref float Time => ref projectile.ai[0];

        public ref float TelegraphLength => ref projectile.ai[1];

        public static int TelegraphTime => 25;

        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        // Hello, github reader!
        public override void SetStaticDefaults() => DisplayName.SetDefault("Fuck you nobody is going to ever see this name");

        public override void SetDefaults()
        {
            projectile.width = 8;
            projectile.height = 8;
            projectile.hostile = true;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;
            projectile.timeLeft = TelegraphTime;
            cooldownSlot = 1;
        }

        public override Color? GetAlpha(Color lightColor) => Color.White * projectile.Opacity;

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            // Cast a telegraph line before the acceleration gets super strong.
            float opacity = (float)Math.Pow(CalamityUtils.Convert01To010(Time / TelegraphTime), 0.4f);
            Texture2D invisible = InfernumTextureRegistry.Invisible;
            Vector2 drawPosition = projectile.Center - Main.screenPosition;

            Effect laserScopeEffect = InfernumEffectsRegistry.PixelatedSightLine.GetShader().Shader;
            laserScopeEffect.Parameters["sampleTexture2"].SetValue(ModContent.GetTexture("InfernumMode/ExtraTextures/GreyscaleGradients/CertifiedCrustyNoise"));
            laserScopeEffect.Parameters["noiseOffset"].SetValue(Main.GameUpdateCount * -0.003f);
            laserScopeEffect.Parameters["mainOpacity"].SetValue((float)Math.Sqrt(opacity));
            laserScopeEffect.Parameters["Resolution"].SetValue(new Vector2(425f));
            laserScopeEffect.Parameters["laserAngle"].SetValue(-projectile.velocity.ToRotation());
            laserScopeEffect.Parameters["laserWidth"].SetValue(0.003f + (float)Math.Pow(opacity, 4f) * ((float)Math.Sin(Main.GlobalTime * 3f) * 0.001f + 0.001f));
            laserScopeEffect.Parameters["laserLightStrenght"].SetValue(5f);
            laserScopeEffect.Parameters["color"].SetValue(Color.Lerp(Color.Red, Color.Yellow, projectile.identity / 7f % 1f * 0.6f).ToVector3());
            laserScopeEffect.Parameters["darkerColor"].SetValue(Color.Lerp(Color.Yellow, Color.Red, 0.65f).ToVector3());
            laserScopeEffect.Parameters["bloomSize"].SetValue(0.1f + (1f - opacity) * 0.18f);
            laserScopeEffect.Parameters["bloomMaxOpacity"].SetValue(0.4f);
            laserScopeEffect.Parameters["bloomFadeStrenght"].SetValue(3f);

            Main.spriteBatch.EnterShaderRegion(BlendState.Additive);

            laserScopeEffect.CurrentTechnique.Passes[0].Apply();
            Main.spriteBatch.Draw(invisible, drawPosition, null, Color.White, 0f, invisible.Size() * 0.5f, opacity * TelegraphLength, SpriteEffects.None, 0f);
            Main.spriteBatch.ExitShaderRegion();
            Time++;
            return false;
        }

        public override bool ShouldUpdatePosition() => false;
    }
}
