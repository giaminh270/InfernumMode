using CalamityMod;
using InfernumMode.ExtraTextures;
using InfernumMode.Graphics.Interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.ProfanedGuardians
{
    public class TelegraphedProfanedSpearInfernum : ModProjectile, IScreenCullDrawer
    {
        public ref float Timer => ref projectile.ai[0];

        public Projectile Parent => Main.projectile[(int)projectile.ai[1]];

        public Vector2 OriginalVelocity;

        public int TelegraphDuration => 30;

        public override string Texture => "InfernumMode/BehaviorOverrides/BossAIs/ProfanedGuardians/ProfanedSpearInfernum";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Profaned Spear");
            ProjectileID.Sets.TrailCacheLength[projectile.type] = 4;
            ProjectileID.Sets.TrailingMode[projectile.type] = 0;
        }

        public override void SetDefaults()
        {
            projectile.width = 32;
            projectile.height = 32;
            projectile.hostile = true;
            projectile.tileCollide = false;
            projectile.penetrate = -1;
            projectile.alpha = 255;
            projectile.timeLeft = 300;
            projectile.Calamity().canBreakPlayerDefense = true;
            cooldownSlot = 1;
        }

        public override void AI()
        {
            if (Timer == 0)
            {
                OriginalVelocity = projectile.velocity;
                projectile.velocity = Vector2.Zero;
            }
            else if (Timer == TelegraphDuration)
                projectile.velocity = OriginalVelocity;

            projectile.tileCollide = Timer - TelegraphDuration > 90;
            projectile.rotation = projectile.velocity.ToRotation() + MathHelper.PiOver4;

            if (Timer > TelegraphDuration)
            {
                // Accelerate.
                if (projectile.velocity.Length() < 36f)
                    projectile.velocity *= 1.028f;
                projectile.Opacity = MathHelper.Clamp(projectile.Opacity + 0.08f, 0f, 1f);
            }
            else
            {
                if (!Parent.active && Parent.type != ModContent.ProjectileType<HolyPushbackWall>())
                {
                    projectile.Kill();
                    return;
                }
                projectile.Center = new Vector2(Parent.Center.X, projectile.Center.Y);
            }

            Lighting.AddLight(projectile.Center, Vector3.One);
            Timer++;
        }

        public override Color? GetAlpha(Color lightColor) => Color.White * projectile.Opacity;

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            if (Timer >= TelegraphDuration)
            {
                CalamityUtils.DrawAfterimagesCentered(projectile, ProjectileID.Sets.TrailingMode[projectile.type], lightColor * projectile.Opacity, 1);
                Color whiteNoAlpha = Color.White;
                whiteNoAlpha.A = 0;
                projectile.DrawProjectileWithBackglowTemp(whiteNoAlpha, Color.White, 2f);
            }
            return false;
        }

        public void CullDraw(SpriteBatch spriteBatch)
        {
            if (Timer < TelegraphDuration)
            {
                Texture2D texture = InfernumTextureRegistry.BloomLineSmall;
                Vector2 position = projectile.Center - Main.screenPosition;
                Color colorInner = Color.Gold * 0.75f;
                colorInner.A = 0;
                Color colorOuter = Color.Lerp(colorInner, Color.White, 0.5f) * 0.75f;
                colorOuter.A = 0;
                float rotation = MathHelper.PiOver2;

                float scaleInterpolant = MathHelper.Clamp((float)Math.Sin(Timer / TelegraphDuration * MathHelper.Pi) * 3f, 0f, 1f);
                Vector2 scaleInner = new Vector2(0.75f * scaleInterpolant, 5550f / texture.Height);
                Vector2 scaleOuter = scaleInner * new Vector2(1.5f, 1f);
                Vector2 origin = texture.Size() * new Vector2(0.5f, 0f);
                Main.spriteBatch.Draw(texture, position, null, colorOuter, rotation, origin, scaleOuter, SpriteEffects.None, 0);
                Main.spriteBatch.Draw(texture, position, null, colorInner, rotation, origin, scaleInner, SpriteEffects.None, 0);
            }
        }
    }
}
