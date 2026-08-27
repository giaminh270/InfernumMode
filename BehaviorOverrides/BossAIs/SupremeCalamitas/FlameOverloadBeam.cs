using CalamityMod;
using CalamityMod.NPCs;
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
using Terraria.ID;
using Terraria.ModLoader;
using static InfernumMode.BehaviorOverrides.BossAIs.SupremeCalamitas.SupremeCalamitasBehaviorOverride;

namespace InfernumMode.BehaviorOverrides.BossAIs.SupremeCalamitas
{
    public class FlameOverloadBeam : ModProjectile, IPixelPrimitiveDrawer
    {
		public bool DrawBeforeNPCs => false;
		
        public PrimitiveTrailCopy RayDrawer;

        public NPC Owner => Main.npc[(int)projectile.ai[0]];

        public ref float LaserLength => ref projectile.ai[1];

        public ref float Time => ref projectile.localAI[1];

        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public const float MaxLaserLength = 3950f;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Brimstone Flame Overload Ray");
        }

        public override void SetDefaults()
        {
            projectile.width = projectile.height = 32;
            projectile.hostile = true;
            projectile.penetrate = -1;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;
            projectile.hide = true;
            projectile.netImportant = true;
            projectile.Calamity().canBreakPlayerDefense = true;
            cooldownSlot = 1;
        }

        public override void AI()
        {
            // Die if the owner is no longer present.
            if (!Owner.active)
            {
                projectile.Kill();
                return;
            }

            if (projectile.localAI[0] == 0f)
            {
                projectile.scale = 0.05f;
                projectile.localAI[0] = 1f;
            }

            // Grow bigger up to a point.
            float maxScale = MathHelper.Lerp(2f, 0.051f, Owner.Infernum().ExtraAI[1]);
            projectile.scale = MathHelper.Clamp(projectile.scale + 0.04f, 0.05f, maxScale);

            // Die after sufficiently shrunk.
            if (Owner.Infernum().ExtraAI[1] >= 1f)
                projectile.Kill();

            // Update the laser length.
            LaserLength = MaxLaserLength;

            // Spin.
            float spinInterpolant = Utils.InverseLerp(16f, 150f, Time, true);
            float angularVelocity = MathHelper.Lerp(0.006f, 0.0174f, (float)Math.Pow(spinInterpolant, 1.75f));
            projectile.velocity = projectile.velocity.RotatedBy(angularVelocity);

            // Make the beam cast light along its length. The brightness of the light is reliant on the scale of the beam.
            DelegateMethods.v3_1 = Color.Orange.ToVector3() * projectile.scale * 0.6f;
            Utils.PlotTileLine(projectile.Center, projectile.Center + projectile.velocity * LaserLength, projectile.width * projectile.scale, DelegateMethods.CastLight);
            Time++;
        }

        internal float PrimitiveWidthFunction(float completionRatio) => projectile.scale * 60f;

        internal Color PrimitiveColorFunction(float completionRatio)
        {
            float opacity = projectile.Opacity * Utils.InverseLerp(0.97f, 0.9f, completionRatio, true) * 
                Utils.InverseLerp(0f, MathHelper.Clamp(15f / LaserLength, 0f, 0.5f), completionRatio, true) *
                (float)Math.Pow(Utils.InverseLerp(60f, 270f, LaserLength, true), 3f);
				
            float flameInterpolant = (float)Math.Sin(completionRatio * 3f + Main.GlobalTime * 0.5f + projectile.identity * 0.3156f) * 0.5f + 0.5f;
            Color flameColor = Color.Orange;
            if (SupremeCalamitasBehaviorOverride.CurrentPhase == SCalPhase.SCalLament)
                flameColor = Color.Lerp(flameColor, Color.Blue, 0.6f);

            Color c = Color.Lerp(Color.White, flameColor, MathHelper.Lerp(0.5f, 0.8f, flameInterpolant)) * opacity;
            c.A = 0;

            return c;
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor) => false;

        public void DrawPixelPrimitives(SpriteBatch spriteBatch)
        {
            if (RayDrawer is null)
                RayDrawer = new PrimitiveTrailCopy(PrimitiveWidthFunction, PrimitiveColorFunction, specialShader: InfernumEffectsRegistry.PrismaticRayVertexShader);

            Vector2 overallOffset = -Main.screenPosition;
            Vector2[] basePoints = new Vector2[24];
            for (int i = 0; i < basePoints.Length; i++)
                basePoints[i] = projectile.Center + projectile.velocity * i / (basePoints.Length - 1f) * LaserLength;

            projectile.scale *= 0.8f;
            InfernumEffectsRegistry.PrismaticRayVertexShader.UseImage("Images/Misc/Perlin");
            Main.instance.GraphicsDevice.Textures[2] = InfernumTextureRegistry.TrypophobiaNoise;
            projectile.scale /= 0.8f;

            RayDrawer.DrawPixelated(basePoints, overallOffset, 42);

            projectile.scale *= 1.5f;
            InfernumEffectsRegistry.PrismaticRayVertexShader.SetShaderTexture(InfernumTextureRegistry.CultistRayMap);
            Main.instance.GraphicsDevice.Textures[2] = InfernumTextureRegistry.TrypophobiaNoise;
            RayDrawer.DrawPixelated(basePoints, overallOffset, 42);
            projectile.scale /= 1.5f;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float _ = 0f;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), projectile.Center, projectile.Center + projectile.velocity * (LaserLength - 50f), projectile.scale * 60f, ref _);
        }

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers)
        {
            behindProjectiles.Add(index);
        }

        public override bool ShouldUpdatePosition() => false;
    }
}
