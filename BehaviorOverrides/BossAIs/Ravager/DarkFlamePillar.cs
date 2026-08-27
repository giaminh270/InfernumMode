using CalamityMod;
using InfernumMode.Buffs;
using InfernumMode.Effects;
using InfernumMode.ExtraTextures;
using InfernumMode.Graphics.Interfaces;
using InfernumMode.Graphics.Primitives;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.Ravager
{
    public class DarkFlamePillar : ModProjectile, IPixelPrimitiveDrawer
    {
        public bool DrawBeforeNPCs => false;

        public int OwnerIndex;

        public PrimitiveTrailCopy FireDrawer;

        public ref float Time => ref projectile.ai[0];

        public ref float InitialRotationalOffset => ref projectile.localAI[0];

        public const int Lifetime = 136;

        public float Height => MathHelper.Lerp(4f, projectile.height, projectile.scale * projectile.Opacity);

        public float Width => MathHelper.Lerp(3f, projectile.width, projectile.scale * projectile.Opacity);

        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public override void SetStaticDefaults() => DisplayName.SetDefault("Dark Flame Pillar");

        public override void SetDefaults()
        {
            projectile.width = 90;
            projectile.height = 960;
            projectile.hostile = true;
            projectile.ignoreWater = true;
            projectile.tileCollide = false;
            projectile.penetrate = -1;
            projectile.timeLeft = Lifetime;
            projectile.alpha = 255;
            projectile.Calamity().canBreakPlayerDefense = true;
            projectile.MaxUpdates = 2;
            cooldownSlot = 1;
        }

        public override void AI()
        {
            // Fade in.
            projectile.Opacity = MathHelper.Clamp(projectile.Opacity + 0.04f, 0f, 1f);

            projectile.scale = (float)Math.Sin(MathHelper.Pi * Time / Lifetime) * 2f;
            if (projectile.scale > 1f)
                projectile.scale = 1f;

            // Create bright light.
            Lighting.AddLight(projectile.Center, Color.Purple.ToVector3() * 1.4f);

            Time++;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float _ = 0f;
            Vector2 start = projectile.Top;
            Vector2 end = start - Vector2.UnitY.RotatedBy(projectile.rotation) * Height * 0.72f;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), start, end, Width * 0.82f, ref _);
        }

        public override bool CanHitPlayer(Player target) => projectile.Opacity >= 0.9f;

        public float WidthFunction(float completionRatio)
        {
            float tipFadeoffInterpolant = MathHelper.SmoothStep(0f, 1f, Utils.InverseLerp(1f, 0.75f, completionRatio, true));
            float baseFadeoffInterpolant = MathHelper.SmoothStep(2.4f, 1f, 1f - CalamityUtils.Convert01To010(Utils.InverseLerp(0f, 0.19f, completionRatio, true)));
            float widthAdditionFactor = (float)Math.Sin(Main.GlobalTime * -13f + projectile.identity + completionRatio * MathHelper.Pi * 4f) * 0.2f;
            return Width * tipFadeoffInterpolant * baseFadeoffInterpolant * (1f + widthAdditionFactor);
        }

        public Color ColorFunction(float completionRatio)
        {
            Color darkFlameColor = new Color(58, 107, 252);
            Color lightFlameColor = new Color(45, 207, 239);
            float colorShiftInterpolant = (float)Math.Sin(-Main.GlobalTime * 6.7f + completionRatio * MathHelper.TwoPi) * 0.5f + 0.5f;
            Color color = Color.Lerp(darkFlameColor, lightFlameColor, (float)Math.Pow(colorShiftInterpolant, 1.64f));
            return color * projectile.Opacity;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            // Create a telegraph line upward that fades away away the pillar fades in.
            Vector2 start = projectile.Top;
            Vector2 end = start - Vector2.UnitY.RotatedBy(projectile.rotation) * Height;
            if (projectile.Opacity < 1f)
                spriteBatch.DrawLineBetter(start + projectile.Size * 0.5f, end + projectile.Size * 0.5f, Color.Cyan * (1f - projectile.Opacity), projectile.Opacity * 6f);

            return false;
        }

        public void DrawPixelPrimitives(SpriteBatch spriteBatch)
        {
            if (FireDrawer is null)
                FireDrawer = new PrimitiveTrailCopy(WidthFunction, ColorFunction, null, true, InfernumEffectsRegistry.DarkFlamePillarVertexShader);

            var oldBlendState = Main.instance.GraphicsDevice.BlendState;
            Main.instance.GraphicsDevice.BlendState = BlendState.Additive;
            InfernumEffectsRegistry.DarkFlamePillarVertexShader.UseSaturation(1.4f);
            InfernumEffectsRegistry.DarkFlamePillarVertexShader.SetShaderTexture(InfernumTextureRegistry.StreakFaded);
            Main.instance.GraphicsDevice.Textures[2] = InfernumTextureRegistry.StreakFaded;

            Vector2 start = projectile.Top;
            Vector2 end = start - Vector2.UnitY.RotatedBy(projectile.rotation) * Height;

            List<Vector2> points = new List<Vector2>();
            for (int i = 0; i <= 64; i++)
                points.Add(Vector2.Lerp(start, end, i / 64f) + Vector2.UnitY * 20f);

            if (Time >= 2f)
                FireDrawer.DrawPixelated(points, projectile.Size * 0.5f - Main.screenPosition, 166);
            Main.instance.GraphicsDevice.BlendState = oldBlendState;
        }

        public override void OnHitPlayer(Player target, int damage, bool crit) => target.AddBuff(ModContent.BuffType<DarkFlames>(), 360);

        public override bool ShouldUpdatePosition() => false;
    }
}
