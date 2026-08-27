using CalamityMod.NPCs;
using InfernumMode.Effects;
using InfernumMode.ExtraTextures;
using InfernumMode.Graphics.Interfaces;
using InfernumMode.Graphics.Primitives;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.Yharon
{
    public class DraconicInfernado : ModProjectile, IPixelPrimitiveDrawer
    {
		public bool DrawBeforeNPCs => false;
		
        public PrimitiveTrailCopy TornadoDrawer
        {
            get;
            set;
        }

        public const int Lifetime = 540;

        public ref float Time => ref projectile.ai[0];

        public override string Texture => InfernumTextureRegistry.InvisPath;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Draconic Infernado");
        }

        public override void SetDefaults()
        {
            projectile.width = 320;
            projectile.height = 1560;
            projectile.penetrate = -1;
            projectile.tileCollide = false;
            projectile.timeLeft = Lifetime;
            projectile.hostile = true;
            projectile.Opacity = 0f;
            cooldownSlot = 1;
        }

        public override void AI()
        {
            // Correct the position on the first frame.
            if (projectile.localAI[0] == 0f)
            {
                projectile.Bottom = projectile.Center;
                projectile.localAI[0] = 1f;
            }

            // Begin to disappear if Yharon is gone.
            if (Time > 32f)
                Time = 32f;

            // Calculate the opacity of the tornado. At the start and end of its life it will fade in/out.
            projectile.Opacity = Utils.InverseLerp(0, 35f, Time, true) * Utils.InverseLerp(0f, 40f, projectile.timeLeft, true);

            Time++;
        }

        public float TornadoWidthFunction(float completionRatio)
        {
            float scale = MathHelper.Lerp(0.04f, 1f, (float)Math.Pow(completionRatio, 0.82f)) * projectile.scale;
            float width = projectile.width + (float)Math.Sin(MathHelper.Pi * completionRatio * 3f - Time / 5f) * 16f;
            return width * scale;
        }

        public Color TornadoColorFunction(float completionRatio)
        {
            float opacity = Utils.InverseLerp(0.95f, 0.8f, completionRatio, true) * Utils.InverseLerp(0f, 0.12f, completionRatio, true) * projectile.Opacity;
            return Color.Lerp(Color.Orange, Color.Yellow, 0.4f) * opacity;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float _ = 0f;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), projectile.Top + Vector2.UnitY * 60f, projectile.Bottom - Vector2.UnitY * 125f, projectile.width * 0.72f, ref _);
        }

        public void DrawPixelPrimitives(SpriteBatch spriteBatch)
        {
            // Initialize the tornado drawer.
			if (TornadoDrawer is null)
				TornadoDrawer = new PrimitiveTrailCopy(TornadoWidthFunction, TornadoColorFunction, null, true, InfernumEffectsRegistry.YharonInfernadoShader);

            // Calculate draw points for the tornado.
            Vector2[] drawPositions = new Vector2[8];
            for (int i = 0; i < drawPositions.Length; i++)
                drawPositions[i] = Vector2.Lerp(projectile.Bottom, projectile.Top, i / (float)(drawPositions.Length - 1f));

            // Prepare the shader.
            InfernumEffectsRegistry.YharonInfernadoShader.Shader.Parameters["edgeTaperPower"].SetValue(0.51f);
            InfernumEffectsRegistry.YharonInfernadoShader.Shader.Parameters["scrollSpeed"].SetValue(0.9f);
            InfernumEffectsRegistry.YharonInfernadoShader.Shader.Parameters["additiveNoiseStrength"].SetValue(2.15f);
            InfernumEffectsRegistry.YharonInfernadoShader.Shader.Parameters["subtractiveNoiseStrength"].SetValue(1.11f);
            InfernumEffectsRegistry.YharonInfernadoShader.SetShaderTexture(InfernumTextureRegistry.WavyNoise);
            InfernumEffectsRegistry.YharonInfernadoShader.SetShaderTexture(InfernumTextureRegistry.SmokyNoise);

            // Draw the tornado.
            TornadoDrawer.DrawPixelated(drawPositions, -Main.screenPosition, 54);
        }

        // Disable damage if the tornado is not suffiently faded in.
        public override bool CanDamage() => projectile.Opacity >= 0.8f && Time >= 90f;
    }
}
