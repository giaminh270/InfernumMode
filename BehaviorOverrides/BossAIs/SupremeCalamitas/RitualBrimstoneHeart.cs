using CalamityMod;
using CalamityMod.NPCs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.SupremeCalamitas
{
    public class RitualBrimstoneHeart : ModProjectile
    {
        public PrimitiveTrail RayDrawer = null;

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
            float opacity = projectile.Opacity * Utils.InverseLerp(0.97f, 0.9f, completionRatio, true) *
                Utils.InverseLerp(0f, MathHelper.Clamp(15f / LaserLength, 0f, 0.5f), completionRatio, true) *
                (float)Math.Pow(Utils.InverseLerp(60f, 270f, LaserLength, true), 3D);
            float flameInterpolant = (float)Math.Sin(completionRatio * 3f + Main.GlobalTime * 0.5f + projectile.identity * 0.3156f) * 0.5f + 0.5f;
            Color c = Color.Lerp(Color.White, Color.Orange, MathHelper.Lerp(0.5f, 0.8f, flameInterpolant)) * opacity;
            c.A = 0;

            return c * projectile.ai[1] * projectile.Opacity;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            if (RayDrawer is null)
                RayDrawer = new PrimitiveTrail(PrimitiveWidthFunction, PrimitiveColorFunction, specialShader: GameShaders.Misc["Infernum:PrismaticRay"]);

            Vector2 overallOffset = -Main.screenPosition;
            Vector2[] basePoints = new Vector2[24];
            for (int i = 0; i < basePoints.Length; i++)
                basePoints[i] = projectile.Center - Vector2.UnitY * i / (basePoints.Length - 1f) * LaserLength;

            projectile.scale *= 0.8f;
            GameShaders.Misc["Infernum:PrismaticRay"].UseImage("Images/Misc/Perlin");
            Main.instance.GraphicsDevice.Textures[2] = ModContent.GetTexture("InfernumMode/ExtraTextures/PrismaticLaserbeamStreak");
            projectile.scale /= 0.8f;

            RayDrawer.Draw(basePoints, overallOffset, 42);

            projectile.scale *= 1.5f;
            GameShaders.Misc["Infernum:PrismaticRay"].SetShaderTexture(ModContent.GetTexture("InfernumMode/ExtraTextures/CultistRayMap"));
            Main.instance.GraphicsDevice.Textures[2] = ModContent.GetTexture("InfernumMode/ExtraTextures/PrismaticLaserbeamStreak2");
            RayDrawer.Draw(basePoints, overallOffset, 42);
            projectile.scale /= 1.5f;
            return true;
        }
    }
}
