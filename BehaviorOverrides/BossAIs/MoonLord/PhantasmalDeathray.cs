using CalamityMod;
using InfernumMode.Effects;
using InfernumMode.ExtraTextures;
using CalamityMod.Buffs.DamageOverTime;
using InfernumMode.Graphics.Interfaces;
using InfernumMode.Graphics.Primitives;
using Terraria.ID;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.MoonLord
{
    public class PhantasmalDeathray : ModProjectile, IPixelPrimitiveDrawer
    {
		public bool DrawBeforeNPCs => false;
		
        internal PrimitiveTrailCopy BeamDrawer;

        public int OwnerIndex;

        public ref float Time => ref projectile.ai[0];

        public ref float Lifetime => ref projectile.ai[1];

        public ref float InitialRotationalOffset => ref projectile.localAI[0];

        public const float LaserLength = 4000f;
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";
        public override void SetStaticDefaults() => DisplayName.SetDefault("Phantasmal Deathray");

        public override void SetDefaults()
        {
            projectile.width = projectile.height = 20;
            projectile.hostile = true;
            projectile.ignoreWater = true;
            projectile.tileCollide = false;
            projectile.penetrate = -1;
            projectile.timeLeft = 9000;
            projectile.alpha = 255;
            projectile.Calamity().canBreakPlayerDefense = true;
            cooldownSlot = 1;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(OwnerIndex);
            writer.Write(InitialRotationalOffset);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            OwnerIndex = reader.ReadInt32();
            InitialRotationalOffset = reader.ReadSingle();
        }

        public override void AI()
        {
            if (OwnerIndex <= 0 || Main.npc[OwnerIndex - 1].ai[0] == -2f)
            {
                projectile.Kill();
                return;
            }

            NPC head = Main.npc[OwnerIndex - 1];
            projectile.Center = head.Center + new Vector2(-6f, -10f);

            // Fade in.
            projectile.alpha = Utils.Clamp(projectile.alpha - 25, 0, 255);

            projectile.scale = (float)Math.Sin(MathHelper.Pi * Time / Lifetime) * 4f;
            if (projectile.scale > 1f)
                projectile.scale = 1f;
            if (Time >= Lifetime)
                projectile.Kill();

            // And create bright light.
            Lighting.AddLight(projectile.Center, Color.Purple.ToVector3() * 1.4f);

            Time++;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float _ = 0f;
            float width = projectile.width * 0.6f;
            Vector2 start = projectile.Center;
            Vector2 end = start + projectile.velocity * (LaserLength - 80f);
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), start, end, width, ref _);
        }

        public float WidthFunction(float completionRatio)
        {
            float squeezeInterpolant = Utils.InverseLerp(1f, 0.92f, completionRatio, true);
            return MathHelper.SmoothStep(2f, projectile.width, squeezeInterpolant) * MathHelper.Clamp(projectile.scale, 0.04f, 1f);
        }

        public Color ColorFunction(float completionRatio)
        {
            Color color = Color.Lerp(Color.Turquoise, Color.Cyan, (float)Math.Pow(completionRatio, 2f));
            return color * projectile.Opacity * 1.1f;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor) => false;

        public void DrawPixelPrimitives(SpriteBatch spriteBatch)
        {
            if (BeamDrawer is null)
                BeamDrawer = new PrimitiveTrailCopy(WidthFunction, ColorFunction, null, true, InfernumEffectsRegistry.FireVertexShader);

            var oldBlendState = Main.instance.GraphicsDevice.BlendState;
            Main.instance.GraphicsDevice.BlendState = BlendState.Additive;
            InfernumEffectsRegistry.FireVertexShader.UseSaturation(1.4f);
            InfernumEffectsRegistry.FireVertexShader.SetShaderTexture(InfernumTextureRegistry.CultistRayMap);

            List<float> originalRotations = new List<float>();
            List<Vector2> points = new List<Vector2>();
            for (int i = 0; i <= 8; i++)
            {
                points.Add(Vector2.Lerp(projectile.Center, projectile.Center + projectile.velocity * LaserLength, i / 8f) - projectile.velocity * 500f);
                originalRotations.Add(MathHelper.PiOver2);
            }

            if (Time >= 2f)
            {
				int pointCount = InfernumConfig.Instance.ReducedGraphicsConfig ? 10 : 23;
                BeamDrawer.DrawPixelated(points, projectile.Size * 0.5f - Main.screenPosition, pointCount);
            }
            Main.instance.GraphicsDevice.BlendState = oldBlendState;
        }

        public override void OnHitPlayer(Player target, int damage, bool crit) => target.AddBuff(ModContent.BuffType<Nightwither>(), 300);

        public override bool ShouldUpdatePosition() => false;
    }
}
