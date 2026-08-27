using CalamityMod;
using InfernumMode.ILEditingStuff;
using InfernumMode.ExtraTextures;
using InfernumMode.Graphics.Primitives;
using InfernumMode.GlobalInstances;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System;
using Microsoft.Xna.Framework.Graphics;
using InfernumMode.Effects;

namespace InfernumMode.BehaviorOverrides.BossAIs.Signus
{
    public class ShadowSlash : ModProjectile
    {
        public PrimitiveTrailCopy SlashDrawer
        {
            get;
            set;
        }

        public override string Texture => InfernumTextureRegistry.InvisPath;

        public static int Lifetime => 20;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Shadow Slash");
        }

        public override void SetDefaults()
        {
            projectile.width = 640;
            projectile.height = 100;
            projectile.hostile = true;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;
            projectile.timeLeft = Lifetime;
            projectile.hide = true;
            projectile.Calamity().canBreakPlayerDefense = true;
            cooldownSlot = 1;
        }

        public override void AI()
        {
            projectile.Opacity = projectile.timeLeft / (float)Lifetime;
            projectile.scale = Utils.InverseLerp(Lifetime, Lifetime - 5f, projectile.timeLeft, true);
            projectile.scale *= MathHelper.Lerp(0.7f, 1.1f, projectile.identity % 6f / 6f) * 0.5f;
            projectile.rotation = projectile.ai[0];
        }

        public override Color? GetAlpha(Color lightColor) => Color.DarkViolet * projectile.Opacity * 1.4f;

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float _ = 0f;
            Vector2 start = projectile.Center - projectile.rotation.ToRotationVector2() * projectile.width * projectile.scale * 0.5f;
            Vector2 end = projectile.Center + projectile.rotation.ToRotationVector2() * projectile.width * projectile.scale * 0.5f;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), start, end, projectile.height * 0.5f, ref _);
        }

        public static float PrimitiveWidthFunction(float completionRatio) => Utils.InverseLerp(0f, 0.32f, completionRatio, true) * Utils.InverseLerp(1f, 0.68f, completionRatio, true) * 40f;

        public Color PrimitiveColorFunction2(float completionRatio) => Color.Lerp(Color.Cyan, Color.Fuchsia, (float)Math.Pow(projectile.Opacity, 0.5f));

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            // Initialize the slash drawer.
            var slashShader = InfernumEffectsRegistry.DoGDashIndicatorVertexShader;
            if (SlashDrawer is null)
				SlashDrawer = new PrimitiveTrailCopy(PrimitiveWidthFunction, PrimitiveColorFunction2, null, true, slashShader);

            // Calculate the three points that define the overall shape of the slash.
            Vector2 start = projectile.Center - projectile.rotation.ToRotationVector2() * projectile.width * projectile.scale * 0.5f;
            Vector2 end = projectile.Center + projectile.rotation.ToRotationVector2() * projectile.width * projectile.scale * 0.5f;
            Vector2 middle = (start + end) * 0.5f + (projectile.rotation + MathHelper.PiOver2).ToRotationVector2() * projectile.width * projectile.scale * 0.167f;

            // Create a bunch of points that slash across the Bezier curve created from the above three points.
            List<Vector2> slashPoints = new List<Vector2>();
            for (int i = 0; i < 16; i++)
            {
                float interpolant = i / 15f * (float)Math.Pow(1f - projectile.Opacity, 0.4f);
                slashPoints.Add(Utilities.QuadraticBezier(start, middle, end, interpolant));
            }

            slashShader.UseOpacity((float)Math.Pow(projectile.Opacity, 0.35f));
            slashShader.UseImage("Images/Extra_194");
            slashShader.UseColor(PrimitiveColorFunction2(0.5f));
            SlashDrawer.Draw(slashPoints, -Main.screenPosition, 50);
            SlashDrawer.Draw(slashPoints, -Main.screenPosition, 24);

            return false;
        }

        public override void DrawBehind(int index, List<int> drawCacheProjsBehindNPCsAndTiles, List<int> drawCacheProjsBehindNPCs, List<int> drawCacheProjsBehindProjectiles, List<int> drawCacheProjsOverWiresUI)
        {
            ScreenOverlaysSystem.DrawCacheProjsOverSignusBlackening.Add(index);
        }
    }
}
