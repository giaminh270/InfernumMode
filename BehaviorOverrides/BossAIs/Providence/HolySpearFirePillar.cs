using CalamityMod;
using InfernumMode.Effects;
using InfernumMode.ExtraTextures;
using InfernumMode.Graphics.Interfaces;
using InfernumMode.Graphics.Primitives;
using InfernumMode.Projectiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.Providence
{
    public class HolySpearFirePillar : ModProjectile, IPixelPrimitiveDrawer
    {
		public bool DrawBeforeNPCs => false;
		
        public PrimitiveTrailCopy LavaDrawer
        {
            get;
            private set;
        }

        public PrimitiveTrailCopy TelegraphDrawer
        {
            get;
            private set;
        }

        public float VariableWidth => Width * MathHelper.SmoothStep(0f, 1f, CurrentLength / MaxLength) * 1.6f;

        public static int Lifetime => MaxTime + TelegraphTime;

        public static float MaxLength => 6000f;

        public static int TelegraphTime => 32;

        public static int MaxTime => 45;

        public static float Width => 60f;

        public ref float Timer => ref projectile.ai[0];

        public ref float CurrentLength => ref projectile.ai[1];

        public ref float StretchOffset => ref projectile.localAI[0];

        public override string Texture => InfernumTextureRegistry.InvisPath;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Lava Geyser");
        }

        public override void SetDefaults()
        {
            projectile.width = 4;
            projectile.height = 4;
            projectile.hostile = true;
            projectile.friendly = false;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;
            projectile.penetrate = -1;
            projectile.timeLeft = Lifetime;
            cooldownSlot = 1;
        }

        public override void AI()
        {
            // Initialize things.
            if (StretchOffset <= 0f)
            {
                projectile.timeLeft = Lifetime;
                StretchOffset = Main.rand.NextFloat(-0.1f, 0f) * Main.rand.NextFromList(-1f, 1f);
            }

            if (Timer >= TelegraphTime - 20)
                CurrentLength = MaxLength * (float)Math.Sin((Timer - TelegraphTime - 10f) / (Lifetime - TelegraphTime - 10f) * (float)Math.PI);
            Timer++;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            Vector2 start = projectile.Center;
            Vector2 end = start + projectile.velocity.SafeNormalize(-Vector2.UnitY) * CurrentLength;
            float _ = 0f;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), start, end, VariableWidth * 0.67f, ref _);
        }

        public float WidthFunction(float _) => VariableWidth * Utils.InverseLerp(0f, 0.12f, _, true);

        public static Color ColorFunction(float _)
        {
            float interpolant = (1f + (float)Math.Sin(Main.GlobalTime * 2f)) / 2f;
            float colorInterpolant = MathHelper.Lerp(0.3f, 0.5f, interpolant);

            if (ProvidenceBehaviorOverride.IsEnraged)
                return Color.Lerp(Color.DeepSkyBlue, Color.Cyan, colorInterpolant);

            return Color.Lerp(Color.OrangeRed, Color.Gold, colorInterpolant);
        }

        public static float TelegraphWidthFunction(float _) => Width * 0.75f;

        public static Color TelegraphColorFunction(float completionRatio)
        {
            Color c = Color.Lerp(Color.OrangeRed, WayfinderSymbol.Colors[2], 0.5f);
            c = Color.Lerp(c, WayfinderSymbol.Colors[0], completionRatio);
            if (ProvidenceBehaviorOverride.IsEnraged)
                c = Color.Lerp(c, Color.Cyan, 0.7f);

            return c * Utils.InverseLerp(0f, 0.12f, completionRatio, true) * 0.2f;
        }

        public void DrawPixelPrimitives(SpriteBatch spriteBatch)
        {
            // Draw the telegraph.
            if (Timer < TelegraphTime + 10f)
            {
                if (TelegraphDrawer == null)
                    TelegraphDrawer = new PrimitiveTrailCopy(TelegraphWidthFunction, TelegraphColorFunction, null, true, InfernumEffectsRegistry.SideStreakVertexShader);

                InfernumEffectsRegistry.SideStreakVertexShader.SetShaderTexture(InfernumTextureRegistry.CultistRayMap);
                float opacityScalar = (float)Math.Sin(Utilities.SineInOutEasing(Timer / (TelegraphTime + 10f), 0) * Math.PI) * 3f;
                InfernumEffectsRegistry.SideStreakVertexShader.UseOpacity(0.5f * opacityScalar);

                Vector2 startT = projectile.Center;
                Vector2 endT = startT + projectile.velocity.SafeNormalize(-Vector2.UnitY) * MaxLength * 1.2f;
                Vector2[] drawPositionsT = new Vector2[8];
                for (int i = 0; i < drawPositionsT.Length; i++)
                    drawPositionsT[i] = Vector2.Lerp(startT, endT, (float)i / drawPositionsT.Length);

                TelegraphDrawer.DrawPixelated(drawPositionsT, -Main.screenPosition, 14);

                Texture2D warningSymbol = InfernumTextureRegistry.VolcanoWarning;
                Vector2 drawPosition = startT + projectile.velocity.SafeNormalize(-Vector2.UnitY) * 1200f - Main.screenPosition;
                Color drawColor = Color.Lerp(TelegraphColorFunction(0.5f), Color.Orange, 0.5f) * projectile.Opacity;
                drawColor.A = 0;
                Vector2 origin = warningSymbol.Size() * 0.5f;
                float scale = 0.38f;

                spriteBatch.Draw(warningSymbol, drawPosition, null, drawColor * (float)Math.Sqrt(opacityScalar), 0f, origin, scale, SpriteEffects.None, 0f);
            }

            // Draw the laser.
            if (Timer > TelegraphTime)
            {
                if (LavaDrawer == null)
                    LavaDrawer = new PrimitiveTrailCopy(WidthFunction, ColorFunction, null, true, InfernumEffectsRegistry.GuardiansLaserVertexShader);

                InfernumEffectsRegistry.GuardiansLaserVertexShader.SetShaderTexture(InfernumTextureRegistry.LavaNoise);
                InfernumEffectsRegistry.GuardiansLaserVertexShader.SetShaderTexture(InfernumTextureRegistry.CultistRayMap);
                InfernumEffectsRegistry.GuardiansLaserVertexShader.UseColor(Color.LightGoldenrodYellow);
                InfernumEffectsRegistry.GuardiansLaserVertexShader.Shader.Parameters["flipY"].SetValue(false);

                float lengthScalar = CurrentLength / MaxLength;
                InfernumEffectsRegistry.GuardiansLaserVertexShader.Shader.Parameters["stretchAmount"].SetValue(1.3f + StretchOffset * lengthScalar);
                InfernumEffectsRegistry.GuardiansLaserVertexShader.Shader.Parameters["pillarVarient"].SetValue(true);
                InfernumEffectsRegistry.GuardiansLaserVertexShader.Shader.Parameters["scrollSpeed"].SetValue(1.75f);
                Vector2 start = projectile.Center;
                Vector2 end = start + projectile.velocity.SafeNormalize(-Vector2.UnitY) * CurrentLength;
                Vector2[] drawPositions = new Vector2[24];
                for (int i = 0; i < drawPositions.Length; i++)
                    drawPositions[i] = Vector2.Lerp(start, end, (float)i / drawPositions.Length);

                LavaDrawer.DrawPixelated(drawPositions, -Main.screenPosition, 23);
            }
        }

        public override bool ShouldUpdatePosition() => false;
    }
}
