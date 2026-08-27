using System;
using CalamityMod;
using InfernumMode.Effects;
using InfernumMode.ExtraTextures;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.AquaticScourge
{
    public class SulphurousRockRubble : ModProjectile
    {
        public ref float Time => ref projectile.ai[0];

        public static int TelegraphTime => 32;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Sulphuric Rubble");
            Main.projFrames[projectile.type] = 3;
            ProjectileID.Sets.TrailCacheLength[projectile.type] = 3;
            ProjectileID.Sets.TrailingMode[projectile.type] = 0;
        }

        public override void SetDefaults()
        {
            projectile.width = 30;
            projectile.height = 30;
            projectile.hostile = true;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;
            projectile.timeLeft = 240;
            
            cooldownSlot = 1;
        }

        public override void AI()
        {
            // Accelerate.
            if (projectile.velocity.Length() < 20f)
                projectile.velocity *= 1.023f;
            projectile.rotation += projectile.velocity.X * 0.014f;

            // Interact with tiles after enough time has passed.
            projectile.tileCollide = Time >= 90f;
            Time++;

            // Decide the frame.
            if (projectile.localAI[0] == 0f)
            {
                projectile.frame = Main.rand.Next(Main.projFrames[projectile.type]);
                projectile.localAI[0] = 1f;
            }
        }

        public override void Kill(int timeLeft)
        {
            // Emit rubble.
            if (Main.netMode == NetmodeID.Server)
                return;

            Main.PlaySound(SoundID.Item51, projectile.Center);
            Gore.NewGore(projectile.position, projectile.velocity, mod.GetGoreSlot("Gores/SulphurousRubble1"), projectile.scale); 
            Gore.NewGore(projectile.position, projectile.velocity, mod.GetGoreSlot("Gores/SulphurousRubble2"), projectile.scale); 
        }

        public override Color? GetAlpha(Color lightColor) => Color.White * projectile.Opacity;

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            // Cast a telegraph line before the acceleration gets super strong.
            float opacity = CalamityUtils.Convert01To010(Time / TelegraphTime);
            if (opacity > 0f)
            {
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
            Utilities.DrawAfterimagesCentered(projectile, lightColor, 0);
            Utilities.DrawProjectileWithBackglowTemp(projectile, Color.Lime * opacity, lightColor, opacity * 6f);

            return false;
        }
    }
}
