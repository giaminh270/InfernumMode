using CalamityMod;
using InfernumMode.ExtraTextures;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.ProfanedGuardians
{
    public class MagicCrystalShot : ModProjectile
    {
        public Color StreakBaseColor => CalamityUtils.MulticolorLerp(projectile.localAI[0] % 0.999f, MagicSpiralCrystalShot.ColorSet);

        public ref float Timer => ref projectile.ai[0];

        public ref float Direction => ref projectile.ai[1];

        public const int TelegraphLength = 30;

        public override string Texture => "CalamityMod/Projectiles/StarProj";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Crystalline Light");
            ProjectileID.Sets.TrailingMode[projectile.type] = 2;
            ProjectileID.Sets.TrailCacheLength[projectile.type] = 12;
        }

        public override void SetDefaults()
        {
            projectile.width = projectile.height = 30;
            projectile.alpha = 255;
            projectile.penetrate = -1;
            projectile.timeLeft = 240;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;
            projectile.hostile = true;
            cooldownSlot = 1;
        }

        public override void AI()
        {
            if (projectile.timeLeft < 15)
                projectile.damage = 0;

            if (projectile.velocity.Length() < 40f)
                projectile.velocity *= 1.04f;
            projectile.Opacity = MathHelper.Clamp(projectile.Opacity + 0.1f, 0f, 1f);
            projectile.rotation = projectile.velocity.ToRotation() + MathHelper.PiOver2;
            Timer++;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            if (Timer <= TelegraphLength)
            {
                float interpolant = Timer / TelegraphLength;
                float scalar = (float)Math.Sin(interpolant * (float)Math.PI);
                float yScale = MathHelper.Lerp(0f, 1f, scalar);
                Color telegraphColor = StreakBaseColor;
                telegraphColor.A = 0;
                Texture2D telegraphTexture = InfernumTextureRegistry.BloomLineSmall;
                Vector2 scaleInner = new Vector2(yScale, InfernumTextureRegistry.BloomLineSmall.Height);
                Vector2 scaleOuter = scaleInner * new Vector2(1.5f, 1f);
                Vector2 origin = InfernumTextureRegistry.BloomLineSmall.Size() * new Vector2(0.5f, 0f);

                Color hotPinkNoAlpha = Color.HotPink;
                hotPinkNoAlpha.A = 0;
                Main.spriteBatch.Draw(telegraphTexture, projectile.Center - Main.screenPosition, null, hotPinkNoAlpha * 2, projectile.velocity.ToRotation() + MathHelper.PiOver2, origin, scaleOuter, SpriteEffects.None, 0f);
                Main.spriteBatch.Draw(telegraphTexture, projectile.Center - Main.screenPosition, null, telegraphColor * 2, projectile.velocity.ToRotation() + MathHelper.PiOver2, origin, scaleInner, SpriteEffects.None, 0f);

            }
            Texture2D streakTexture = Main.projectileTexture[projectile.type];
            for (int i = 1; i < projectile.oldPos.Length; i++)
            {
                if (projectile.oldPos[i - 1] == Vector2.Zero || projectile.oldPos[i] == Vector2.Zero)
                    continue;

                float completionRatio = i / (float)projectile.oldPos.Length;
                float fade = (float)Math.Pow(completionRatio, 2f);
                float scale = projectile.scale * MathHelper.Lerp(1.3f, 0.9f, Utils.InverseLerp(0f, 0.24f, completionRatio, true)) *
                    MathHelper.Lerp(0.9f, 0.56f, Utils.InverseLerp(0.5f, 0.78f, completionRatio, true));
                Color drawColor = Color.Lerp(StreakBaseColor, new Color(229, 255, 255), fade) * (1f - fade) * projectile.Opacity;
                drawColor.A = 0;

                Vector2 drawPosition = projectile.oldPos[i - 1] + projectile.Size * 0.5f - Main.screenPosition;
                Vector2 drawPosition2 = Vector2.Lerp(drawPosition, projectile.oldPos[i] + projectile.Size * 0.5f - Main.screenPosition, 0.5f);
                Main.spriteBatch.Draw(streakTexture, drawPosition, null, drawColor, projectile.oldRot[i], streakTexture.Size() * 0.5f, scale, SpriteEffects.None, 0f);
                Main.spriteBatch.Draw(streakTexture, drawPosition2, null, drawColor, projectile.oldRot[i], streakTexture.Size() * 0.5f, scale, SpriteEffects.None, 0f);
            }
            return false;
        }
    }
}
