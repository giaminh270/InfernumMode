using CalamityMod;
using InfernumMode.Graphics.Interfaces;
using InfernumMode.Graphics.Primitives;
using InfernumMode.Projectiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using InfernumMode.Effects;
using InfernumMode.ExtraTextures;
using System;

namespace InfernumMode.BehaviorOverrides.BossAIs.ProfanedGuardians
{
    public class HolyAimedDeathray : ModProjectile, IPixelPrimitiveDrawer
    {
		public bool DrawBeforeNPCs => false;
		
        internal PrimitiveTrailCopy BeamDrawer;

        public ref float Time => ref projectile.ai[0];

        public NPC Owner => Main.npc[(int)projectile.ai[1]];

        public static Color BrightFire => new Color(255, 255, 150);

        public const float LaserLength = 7500f;

        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public override void SetStaticDefaults() => DisplayName.SetDefault("Flame Beam");

        public override void SetDefaults()
        {
            projectile.width = projectile.height = 30;
            projectile.hostile = true;
            projectile.ignoreWater = true;
            projectile.tileCollide = false;
            projectile.penetrate = -1;
            projectile.timeLeft = 30;
            projectile.alpha = 255;
            projectile.Calamity().canBreakPlayerDefense = true;
            cooldownSlot = 1;

        }

        public override void AI()
        {
            // Fade in.
            projectile.alpha = Utils.Clamp(projectile.alpha - 25, 0, 255);

            projectile.scale = MathHelper.Clamp((float)Math.Sin(Time / 30f * MathHelper.Pi) * 3f, 0f, 1f);
            projectile.Center = Owner.Center + projectile.velocity * 30f;

            // And create bright light.
            Lighting.AddLight(projectile.Center, Color.Orange.ToVector3() * 1.4f);

            if (!Owner.active || Owner.Infernum().ExtraAI[2] == 1f)
                projectile.Kill();

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
                fire.color = Color.Lerp(Color.White, BrightFire, Main.rand.NextFloat());
                fire.noGravity = true;
            }
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float _ = 0f;
            float width = projectile.width * projectile.scale * 2.2f;
            Vector2 start = projectile.Center;
            Vector2 end = start + projectile.velocity * (LaserLength - 300f);
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), start, end, width, ref _);
        }

        public override bool ShouldUpdatePosition() => false;

        public float WidthFunction(float completionRatio)
        {
            return projectile.width * projectile.scale * 2.5f;
        }

        public Color ColorFunction(float completionRatio) => /*new Color(255, 191, 73)*/ Color.Lerp(WayfinderSymbol.Colors[1], Color.OrangeRed, 0.5f) * projectile.Opacity;

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor) => false;

        public void DrawPixelPrimitives(SpriteBatch spriteBatch)
        {
            if (BeamDrawer == null)
                BeamDrawer = new PrimitiveTrailCopy(WidthFunction, ColorFunction, null, true, InfernumEffectsRegistry.GenericLaserVertexShader);

            InfernumEffectsRegistry.GenericLaserVertexShader.UseColor(BrightFire);
            InfernumEffectsRegistry.GenericLaserVertexShader.SetShaderTexture(InfernumTextureRegistry.HarshNoise);
            InfernumEffectsRegistry.GenericLaserVertexShader.Shader.Parameters["strongerFade"].SetValue(true);

            List<float> originalRotations = new List<float>();
            List<Vector2> points = new List<Vector2>();
            for (int i = 0; i <= 8; i++)
            {
                points.Add(Vector2.Lerp(projectile.Center, projectile.Center + projectile.velocity * LaserLength, i / 8f));
                originalRotations.Add(MathHelper.PiOver2);
            }

            BeamDrawer.DrawPixelated(points, -Main.screenPosition, 30);
            Main.spriteBatch.ExitShaderRegion();
        }

        public override bool CanDamage() => Time >= 8f;
    }
}
