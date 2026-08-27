
using InfernumMode.Graphics.Interfaces;
using InfernumMode.Graphics.Primitives;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;
using InfernumMode.Effects;
using InfernumMode.ExtraTextures;

namespace InfernumMode.BehaviorOverrides.BossAIs.Yharon
{
    public class VortexTelegraphBeam : ModProjectile, IPixelPrimitiveDrawer
    {
        public bool DrawBeforeNPCs => false;
        public PrimitiveTrailCopy LaserDrawer;
        internal PrimitiveTrailCopy BeamDrawer;
        public ref float Time => ref projectile.ai[0];
        public ref float LaserLength => ref projectile.ai[1];
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";
        public override void SetStaticDefaults() => DisplayName.SetDefault("Flame Beam Telegraph");

        public override void SetDefaults()
        {
            projectile.width = projectile.height = 18;
            projectile.hostile = true;
            projectile.ignoreWater = true;
            projectile.tileCollide = false;
            projectile.penetrate = -1;
            projectile.timeLeft = 45;
            projectile.alpha = 255;
            projectile.hide = true;
            cooldownSlot = 1;
        }

        public override void AI()
        {
            // Fade in.
            projectile.alpha = Utils.Clamp(projectile.alpha - 25, 0, 255);

            projectile.scale = (float)Math.Sin(Time / 450f * MathHelper.Pi) * 3f;
            if (projectile.scale > 1f)
                projectile.scale = 1f;

            Time++;
        }

        public float WidthFunction(float completionRatio)
        {
            float squeezeInterpolant = Utils.InverseLerp(0f, 0.05f, completionRatio, true) * Utils.InverseLerp(1f, 0.95f, completionRatio, true);
            return MathHelper.SmoothStep(2f, projectile.width, squeezeInterpolant) * MathHelper.Clamp(projectile.scale, 0.01f, 1f);
        }

        public override bool ShouldUpdatePosition() => false;

        public Color ColorFunction(float completionRatio)
        {
            Color color = Color.Lerp(Color.Orange, Color.DarkRed, (float)Math.Pow(completionRatio, 2f));
            color = Color.Lerp(color, Color.Red, 0.65f);
            return color * projectile.Opacity * 0.6f;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor) => false;

        public void DrawPixelPrimitives(SpriteBatch spriteBatch)
        {
            if (BeamDrawer is null)
                BeamDrawer = new PrimitiveTrailCopy(WidthFunction, ColorFunction, null, true, InfernumEffectsRegistry.FireVertexShader);

            InfernumEffectsRegistry.FireVertexShader.UseSaturation(1.4f);
            InfernumEffectsRegistry.FireVertexShader.SetShaderTexture(InfernumTextureRegistry.CultistRayMap);

            List<float> originalRotations = new List<float>();
            List<Vector2> points = new List<Vector2>();
            for (int i = 0; i <= 8; i++)
            {
                points.Add(Vector2.Lerp(projectile.Center, projectile.Center + projectile.velocity * LaserLength, i / 8f));
                originalRotations.Add(MathHelper.PiOver2);
            }

            BeamDrawer.DrawPixelated(points, projectile.Size * 0.5f - Main.screenPosition, 60);
        }

        public override void DrawBehind(int index, List<int> drawCacheProjsBehindNPCsAndTiles, List<int> drawCacheProjsBehindNPCs, List<int> drawCacheProjsBehindProjectiles, List<int> drawCacheProjsOverWiresUI)
        {
            drawCacheProjsBehindProjectiles.Add(index);
        }

        public override bool CanDamage() => false;
    }
}
