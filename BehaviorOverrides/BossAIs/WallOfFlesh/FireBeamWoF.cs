using CalamityMod;
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

namespace InfernumMode.BehaviorOverrides.BossAIs.WallOfFlesh
{
    public class FireBeamWoF : ModProjectile, IPixelPrimitiveDrawer
    {
	    public bool DrawBeforeNPCs => false;
        internal PrimitiveTrailCopy BeamDrawer;
        public ref float Time => ref projectile.ai[0];
        public NPC Owner => Main.npc[(int)projectile.ai[1]];
        public const float LaserLength = 2400f;
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";
        public override void SetStaticDefaults() => DisplayName.SetDefault("Flame Beam");

        public override void SetDefaults()
        {
            projectile.width = projectile.height = 30;
            projectile.hostile = true;
            projectile.ignoreWater = true;
            projectile.tileCollide = false;
            projectile.penetrate = -1;
            projectile.timeLeft = 120;
            projectile.alpha = 255;
            projectile.Calamity().canBreakPlayerDefense = true;
            cooldownSlot = 1;
        }

        public override void AI()
        {
            // Fade in.
            projectile.alpha = Utils.Clamp(projectile.alpha - 25, 0, 255);

            projectile.scale = (float)Math.Sin(Time / 120f * MathHelper.Pi) * 3f;
            if (projectile.scale > 1f)
                projectile.scale = 1f;

            projectile.Center = Owner.Center + projectile.velocity * 30f;

            // And create bright light.
            Lighting.AddLight(projectile.Center, Color.Orange.ToVector3() * 1.4f);

            if (!Owner.active || Owner.Infernum().ExtraAI[2] == 1f)
                projectile.Kill();

            Owner.rotation = projectile.velocity.ToRotation();
            if (Owner.direction < 0)
                Owner.rotation += MathHelper.Pi;

            CreateDustAtBeginning();

            Time++;
        }

        public void CreateDustAtBeginning()
        {
            for (int i = 0; i < 6; i++)
            {
                Dust fire = Dust.NewDustPerfect(projectile.Center + Main.rand.NextVector2Circular(50f, 50f), 222);
                fire.velocity = -Vector2.UnitY * Main.rand.NextFloat(1.5f, 3.25f);
                fire.velocity *= Main.rand.NextBool(2).ToDirectionInt();
                fire.scale = 1f + fire.velocity.Length() * 0.1f;
                fire.color = Color.Lerp(Color.White, Color.OrangeRed, Main.rand.NextFloat());
                fire.noGravity = true;
            }
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float _ = 0f;
            float width = projectile.width * 0.8f;
            Vector2 start = projectile.Center;
            Vector2 end = start + projectile.velocity * (LaserLength - 80f);
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), start, end, width, ref _);
        }

        public float WidthFunction(float completionRatio)
        {
            return projectile.width * projectile.scale * 2f;
        }

        public override bool ShouldUpdatePosition() => false;

        public Color ColorFunction(float completionRatio)
        {
            Color color = Color.Lerp(Color.Red, Color.Orange, 0.65f);
            return color * projectile.Opacity * (float)Math.Pow(Utils.InverseLerp(0f, 0.1f, completionRatio, true), 3f);
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor) => false;

        public void DrawPixelPrimitives(SpriteBatch spriteBatch)
        {
            if (BeamDrawer is null)
                BeamDrawer = new PrimitiveTrailCopy(WidthFunction, ColorFunction, null, true, InfernumEffectsRegistry.GenericLaserVertexShader);

            Color middleColor = Color.Lerp(Color.White, Color.Orange, 0.6f);
            Color middleColor2 = Color.Lerp(Color.Red, Color.DarkRed, 0.5f);
            Color finalColor = Color.Lerp(middleColor, middleColor2, Time / 120);

            InfernumEffectsRegistry.GenericLaserVertexShader.UseColor(Color.OrangeRed);
            InfernumEffectsRegistry.GenericLaserVertexShader.SetShaderTexture(InfernumTextureRegistry.StreakFire);

            List<float> originalRotations = new List<float>();
            List<Vector2> points = new List<Vector2>();
            for (int i = 0; i <= 8; i++)
            {
                points.Add(Vector2.Lerp(projectile.Center, projectile.Center + projectile.velocity * LaserLength, i / 8f));
                originalRotations.Add(MathHelper.PiOver2);
            }

            BeamDrawer.DrawPixelated(points, -Main.screenPosition, 32);
            Main.spriteBatch.ExitShaderRegion();
        }

        public override bool CanDamage() => Time >= 8f;
    }
}
