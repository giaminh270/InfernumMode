using CalamityMod;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.SupremeCalamitas
{
    public class FlameOverloadBeam : ModProjectile
    {
        public PrimitiveTrail RayDrawer = null;

        public NPC Owner => Main.npc[(int)projectile.ai[0]];

        public ref float LaserLength => ref projectile.ai[1];

        public ref float Time => ref projectile.localAI[1];

        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public const float MaxLaserLength = 3950f;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Brimstone Flame Overload Ray");;
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
            float angularVelocity = MathHelper.Lerp(0.006f, 0.0174f, (float)Math.Pow(spinInterpolant, 1.75));
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
                (float)Math.Pow(Utils.InverseLerp(60f, 270f, LaserLength, true), 3D);
            float flameInterpolant = (float)Math.Sin(completionRatio * 3f + Main.GlobalTime * 0.5f + projectile.identity * 0.3156f) * 0.5f + 0.5f;
            Color c = Color.Lerp(Color.White, Color.Orange, MathHelper.Lerp(0.5f, 0.8f, flameInterpolant)) * opacity;
            c.A = 0;

            return c;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            if (RayDrawer is null)
                RayDrawer = new PrimitiveTrail(PrimitiveWidthFunction, PrimitiveColorFunction, null, GameShaders.Misc["Infernum:PrismaticRay"]);

            Vector2 overallOffset = -Main.screenPosition;
            Vector2[] basePoints = new Vector2[24];
            for (int i = 0; i < basePoints.Length; i++)
                basePoints[i] = projectile.Center + projectile.velocity * i / (basePoints.Length - 1f) * LaserLength;

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
            return false;
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
