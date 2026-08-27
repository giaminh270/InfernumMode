using CalamityMod;
using CalamityMod.NPCs;
using InfernumMode.Effects;
using InfernumMode.ExtraTextures;
using InfernumMode.Graphics.Interfaces;
using InfernumMode.Graphics.Primitives;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using InfernumMode.ExtraTextures;

namespace InfernumMode.BehaviorOverrides.BossAIs.SupremeCalamitas
{
    public class RitualBrimstoneHeart : ModProjectile, IPixelPrimitiveDrawer
    {
		public bool DrawBeforeNPCs => false;
		
        public PrimitiveTrailCopy RayDrawer;

        public const float LaserLength = 2700f;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Brimstone Heart");
            Main.projFrames[projectile.type] = 6;
            ProjectileID.Sets.TrailCacheLength[projectile.type] = 3;
            ProjectileID.Sets.TrailingMode[projectile.type] = 0;
        }

        public override void SetDefaults()
        {
            projectile.width = 44;
            projectile.height = 60;
            projectile.ignoreWater = true;
            projectile.tileCollide = false;
            projectile.penetrate = -1;
            projectile.timeLeft = 96000;
            cooldownSlot = 1;
        }

        public override void AI()
        {
            // It is SCal's responsibility to move these things around.
            if (CalamityGlobalNPC.SCal == -1 || !Main.npc[CalamityGlobalNPC.SCal].active)
            {
                projectile.Kill();
                return;
            }

            projectile.Opacity = Utils.InverseLerp(96000f, 95960f, projectile.timeLeft, true) * projectile.Infernum().ExtraAI[0];
            projectile.frameCounter++;
            projectile.frame = (int)((projectile.frameCounter / 6 + projectile.ai[0] * 4f) % Main.projFrames[projectile.type]);
        }

        internal float PrimitiveWidthFunction(float completionRatio) => projectile.scale * 30f;

        internal Color PrimitiveColorFunction(float completionRatio)
        {
            float lengthFadeOut = Utils.InverseLerp(0f, MathHelper.Clamp(15f / LaserLength, 0f, 0.5f), completionRatio, true);
            float lengthFadeIn = (float)Math.Pow(Utils.InverseLerp(60f, 270f, LaserLength, true), 3f);
            float endOpacity = Utils.InverseLerp(0.97f, 0.9f, completionRatio, true);
            float opacity = projectile.Opacity * endOpacity * lengthFadeIn * lengthFadeOut;

            float flameInterpolant = (float)Math.Sin(completionRatio * 3f + Main.GlobalTime * 0.5f + projectile.identity * 0.3156f) * 0.5f + 0.5f;
            Color c = Color.Lerp(Color.White, Color.Orange, MathHelper.Lerp(0.5f, 0.8f, flameInterpolant)) * opacity;
            c.A = 0;

            return c * projectile.ai[1] * projectile.Opacity;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor) => true;

        public void DrawPixelPrimitives(SpriteBatch spriteBatch)
        {
            if (RayDrawer is null)
                RayDrawer = new PrimitiveTrailCopy(PrimitiveWidthFunction, PrimitiveColorFunction, specialShader: InfernumEffectsRegistry.PrismaticRayVertexShader);

            Vector2 overallOffset = -Main.screenPosition;
            Vector2[] basePoints = new Vector2[24];
            for (int i = 0; i < basePoints.Length; i++)
                basePoints[i] = projectile.Center - Vector2.UnitY * i / (basePoints.Length - 1f) * LaserLength;

            projectile.scale *= 0.8f;
            InfernumEffectsRegistry.PrismaticRayVertexShader.UseImage("Images/Misc/Perlin");
            Main.instance.GraphicsDevice.Textures[2] = InfernumTextureRegistry.StreakSolid;
            projectile.scale /= 0.8f;

			int rayCount = InfernumConfig.Instance.ReducedGraphicsConfig ? 24 : 42;
            RayDrawer.DrawPixelated(basePoints, overallOffset, rayCount);

            projectile.scale *= 1.5f;
            InfernumEffectsRegistry.PrismaticRayVertexShader.SetShaderTexture(InfernumTextureRegistry.CultistRayMap);
            Main.instance.GraphicsDevice.Textures[2] = InfernumTextureRegistry.StreakFaded;
            RayDrawer.DrawPixelated(basePoints, overallOffset, rayCount);
            projectile.scale /= 1.5f;
        }
    }
}
