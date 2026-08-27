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
    public class AcidBubble : ModProjectile, IPixelPrimitiveDrawer, IAdditiveDrawer
    {
        public bool DrawBeforeNPCs => false;

        public PrimitiveTrailCopy WaterDrawer;

        public ref float Time => ref projectile.ai[0];

        public static int Lifetime => 240;

        public static float Radius => 60f;

        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public override void SetStaticDefaults() => DisplayName.SetDefault("Acid Bubble");

        public override void SetDefaults()
        {
            projectile.width = projectile.height = (int)Radius;
            projectile.hostile = true;
            projectile.ignoreWater = true;
            projectile.tileCollide = false;
            projectile.timeLeft = Lifetime;
            
            cooldownSlot = 1;
        }

        public override void AI()
        {
            projectile.Opacity = CalamityUtils.Convert01To010(projectile.timeLeft / (float)Lifetime) * 3.6f;
            if (projectile.Opacity > 1f)
                projectile.Opacity = 1f;
            projectile.scale = projectile.Opacity * MathHelper.Lerp(0.6f, 1f, projectile.identity * MathHelper.Pi % 1f);

            // Randomly emit bubbles.
            Vector2 bubbleSpawnPosition = projectile.Center + Main.rand.NextVector2Circular(120f, 120f) * projectile.scale;
            bubbleSpawnPosition += projectile.velocity.SafeNormalize(Vector2.UnitY).RotatedBy(MathHelper.PiOver2) * Main.rand.NextFloatDirection() * 14f;
            if (Main.rand.NextBool(3))
            {
                for (int i = 0; i < 4; i++)
                {
                    Gore bubble = Gore.NewGorePerfect(bubbleSpawnPosition, projectile.velocity * 0.2f + Main.rand.NextVector2Circular(1f, 1f) * 0.75f, 411);
                    bubble.timeLeft = Main.rand.Next(8, 14);
                    bubble.scale = Main.rand.NextFloat(0.5f, 0.5f);
                    bubble.type = Main.rand.NextBool(3) ? 422 : 421;
                }
            }

            Time++;
        }

        public float WidthFunction(float completionRatio) => Radius * projectile.scale * CalamityUtils.Convert01To010(completionRatio);

        public Color ColorFunction(float completionRatio)
        {
            float colorInterpolant = (float)Math.Pow(Math.Abs((float)Math.Sin(completionRatio * MathHelper.Pi + Main.GlobalTime)), 3f) * 0.5f;
            return Color.Lerp(new Color(140, 234, 87), new Color(144, 114, 166), colorInterpolant) * projectile.Opacity * 0.3f;
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

                float adjustedAngle = offsetAngle + Main.GlobalTime * 2.2f;
                Vector2 offsetDirection = adjustedAngle.ToRotationVector2();
                Vector2 radius = Vector2.One * Radius;
                radius.Y *= MathHelper.Lerp(1f, 2f, Math.Abs((float)Math.Cos(Main.GlobalTime * 1.9f)));

                for (int i = 0; i <= 8; i++)
                    drawPoints.Add(Vector2.Lerp(projectile.Center - offsetDirection * radius * 0.8f, projectile.Center + offsetDirection * radius * 0.8f, i / 8f));

                WaterDrawer.DrawPixelated(drawPoints, -Main.screenPosition, 15, adjustedAngle);
            }
        }

        // Draw an additive bubble overlay over the prims.
        public void AdditiveDraw(SpriteBatch spriteBatch)
        {
            Texture2D bubble = InfernumTextureRegistry.Bubble;
            Vector2 drawPosition = projectile.Center - Main.screenPosition;
            Color bubbleColor = projectile.GetAlpha(Color.Lerp(Color.YellowGreen, Color.Wheat, 0.75f)) * 0.7f;
            Vector2 bubbleScale = Vector2.One * (projectile.scale * 0.3f + (float)Math.Cos(Main.GlobalTime * 1.1f + projectile.identity) * 0.025f);

            // Make the bubble scale squish a bit in one of the four cardinal directions for more a fluid aesthetic.
            Vector2 scalingDirection = -Vector2.UnitY.RotatedBy(projectile.identity % 4 / 4f * MathHelper.TwoPi);
            bubbleScale += scalingDirection * (float)(Math.Cos(Main.GlobalTime * 3.1f + projectile.identity) * 0.5f + 0.5f) * 0.07f;

            Main.spriteBatch.Draw(bubble, drawPosition, null, bubbleColor, projectile.rotation, bubble.Size() * 0.5f, bubbleScale, 0, 0);
        }
    }
}
