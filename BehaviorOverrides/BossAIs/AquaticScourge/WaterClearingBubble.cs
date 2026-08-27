using CalamityMod;
using CalamityMod.DataStructures;
using InfernumMode.Effects;
using InfernumMode.ExtraTextures;
using InfernumMode.Sounds;
using InfernumMode.Graphics.Interfaces;
using InfernumMode.Graphics.Primitives;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using InfernumMode.DataStructures;

namespace InfernumMode.BehaviorOverrides.BossAIs.AquaticScourge
{
    public class WaterClearingBubble : ModProjectile, IPixelPrimitiveDrawer, IAdditiveDrawer
    {
        public bool DrawBeforeNPCs => false;

        public PrimitiveTrailCopy WaterDrawer;

        public ref float Time => ref projectile.ai[0];

        public ref float Lifetime => ref projectile.ai[1];

        public static float Radius => 120f;

        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public override void SetStaticDefaults() => DisplayName.SetDefault("Acid Bubble");


        public static void PrepareWater(On.Terraria.GameContent.Liquid.LiquidRenderer.orig_InternalDraw orig, Terraria.GameContent.Liquid.LiquidRenderer self, SpriteBatch spriteBatch, Vector2 drawOffset, int waterStyle, float globalAlpha, bool isBackgroundDraw)
        {
            ClaimAllBubbles();
            orig(self, spriteBatch, drawOffset, waterStyle, globalAlpha, isBackgroundDraw);
        }

        public static void ClaimAllBubbles()
        {
            // Make the nearby water clear.
            SulphuricWaterSafeZoneSystem.NearbySafeTiles.Clear();
            foreach (Projectile bubble in Utilities.AllProjectilesByID(ModContent.ProjectileType<WaterClearingBubble>()))
            {
                if (bubble.Opacity <= 0f || !bubble.WithinRange(Main.LocalPlayer.Center, 2000f))
                    continue;

                Point p = bubble.Center.ToTileCoordinates();
                float power = 0f;
                if (SulphuricWaterSafeZoneSystem.NearbySafeTiles.TryGetValue(p, out float s))
                    power = s;

                SulphuricWaterSafeZoneSystem.NearbySafeTiles[p] = (float)Math.Max(power, bubble.scale);
            }
        }

        public override void SetDefaults()
        {
            projectile.width = projectile.height = (int)Radius;
            projectile.hostile = true;
            projectile.ignoreWater = true;
            projectile.tileCollide = false;
            projectile.netImportant = true;
            projectile.timeLeft = 7200;
            cooldownSlot = 1;
        }

        public override void AI()
        {
            // Initialize the lifetime of the bubble if nothing is inputted.
            if (Lifetime <= 0f)
            {
                Lifetime = 240f;
                projectile.netUpdate = true;
            }

            projectile.Opacity = CalamityUtils.Convert01To010(Time / Lifetime) * 4f;
            if (projectile.Opacity > 1f)
                projectile.Opacity = 1f;
            projectile.scale = projectile.Opacity;

            // Release positive golden/cyan particles.
            Dust positiveParticle = Dust.NewDustPerfect(projectile.Center + Main.rand.NextVector2Circular(220f, 220f) * projectile.scale, 261);
            positiveParticle.color = Main.rand.NextBool() ? Color.Gold : Color.Cyan;
            positiveParticle.velocity = -Vector2.UnitY * Main.rand.NextFloat(0.5f, 3f);
            positiveParticle.scale = 1.5f;
            positiveParticle.noGravity = true;

            // Randomly emit bubbles.
            Vector2 bubbleSpawnPosition = projectile.Center + Main.rand.NextVector2Circular(250f, 250f) * projectile.scale;
            bubbleSpawnPosition += projectile.velocity.SafeNormalize(Vector2.UnitY).RotatedBy(MathHelper.PiOver2) * Main.rand.NextFloatDirection() * 14f;
            if (!Main.rand.NextBool(3))
            {
                for (int i = 0; i < 7; i++)
                {
                    Gore bubble = Gore.NewGorePerfect(bubbleSpawnPosition, projectile.velocity * 0.2f + Main.rand.NextVector2Circular(1f, 1f) * 0.75f, 411);
                    bubble.timeLeft = Main.rand.Next(8, 14);
                    bubble.scale = Main.rand.NextFloat(0.8f, 1.2f);
                    bubble.type = Main.rand.NextBool(3) ? 412 : 411;
                }
            }

            Time++;
            if (Time >= Lifetime)
                projectile.Kill();
        }

        public float WidthFunction(float completionRatio) => Radius * projectile.scale * CalamityUtils.Convert01To010(completionRatio);

        public Color ColorFunction(float completionRatio)
        {
            float colorInterpolant = (float)Math.Pow(Math.Abs((float)Math.Sin(completionRatio * MathHelper.Pi + Main.GlobalTime)), 3f) * 0.5f;
            return Color.Lerp(new Color(103, 218, 224), new Color(144, 114, 166), colorInterpolant) * projectile.Opacity * 0.3f;
        }

        public override void Kill(int timeLeft)
        {
            Main.PlaySound(InfernumSoundRegistry.BubblePop, projectile.Center);
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor) => false;

        public void DrawPixelPrimitives(SpriteBatch spriteBatch)
        {
			if (WaterDrawer is null)
				WaterDrawer = new PrimitiveTrailCopy(WidthFunction, ColorFunction, null, true, InfernumEffectsRegistry.DukeTornadoVertexShader);

            InfernumEffectsRegistry.DukeTornadoVertexShader.UseImage("Images/Misc/Perlin");
            List<Vector2> drawPoints = new List<Vector2>();

            for (float offsetAngle = -MathHelper.PiOver2; offsetAngle <= MathHelper.PiOver2; offsetAngle += MathHelper.Pi / 6f)
            {
                drawPoints.Clear();

                float adjustedAngle = offsetAngle + Main.GlobalTime * 1.2f;
                Vector2 offsetDirection = adjustedAngle.ToRotationVector2();
                Vector2 radius = Vector2.One * Radius;
                radius.Y *= MathHelper.Lerp(1f, 2f, Math.Abs((float)Math.Cos(Main.GlobalTime * 1.1f)));

                for (int i = 0; i <= 8; i++)
                {
                    drawPoints.Add(Vector2.Lerp(projectile.Center - offsetDirection * radius * 0.8f, projectile.Center + offsetDirection * radius * 0.8f, i / 8f));
                }

                WaterDrawer.DrawPixelated(drawPoints, -Main.screenPosition, 12, adjustedAngle);
            }
        }

        // Draw an additive bubble overlay over the prims.
        public void AdditiveDraw(SpriteBatch spriteBatch)
        {
            Texture2D bubble = InfernumTextureRegistry.Bubble;
            Vector2 drawPosition = projectile.Center - Main.screenPosition;
            Color bubbleColor = projectile.GetAlpha(Color.Lerp(Color.DeepSkyBlue, Color.Wheat, 0.4f)) * 0.9f;
            Vector2 bubbleScale = Vector2.One * (projectile.scale * 0.8f + (float)Math.Cos(Main.GlobalTime * 1.1f + projectile.identity) * 0.04f);

            // Make the bubble scale squish a bit in one of the four cardinal directions for more a fluid aesthetic.
            Vector2 scalingDirection = -Vector2.UnitY.RotatedBy(projectile.identity % 4 / 4f * MathHelper.TwoPi);
            bubbleScale += scalingDirection * (float)(Math.Cos(Main.GlobalTime * 3.1f + projectile.identity) * 0.5f + 0.5f) * 0.16f;

            Main.spriteBatch.Draw(bubble, drawPosition, null, bubbleColor, projectile.rotation, bubble.Size() * 0.5f, bubbleScale, 0, 0);
        }
    }
}
