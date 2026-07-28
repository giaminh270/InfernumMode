using CalamityMod;

using InfernumMode.Projectiles;
using InfernumMode.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using static InfernumMode.Particles.InfernumBaseFusableParticleSet;

namespace InfernumMode
{
    public static partial class Utilities
    {
        private static readonly FieldInfo shaderTextureField = typeof(MiscShaderData).GetField("_uImage", BindingFlags.NonPublic | BindingFlags.Instance);

        // Use reflection to set the image. Its underlying data is private and the only way to change it publicly
        // is via a method that only accepts paths to vanilla textures.
        public static void SetShaderTexture(this MiscShaderData shader, Texture2D texture) => shaderTextureField.SetValue(shader, new Ref<Texture2D>(texture));

        /// <summary>
        /// Prepares a <see cref="SpriteBatch"/> for shader-based drawing.
        /// </summary>
        /// <param name="spriteBatch">The sprite batch.</param>
        public static void EnterShaderRegion(this SpriteBatch spriteBatch)
        {
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.instance.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        }

        /// <summary>
        /// Ends changes to a <see cref="SpriteBatch"/> based on shader-based drawing in favor of typical draw begin states.
        /// </summary>
        /// <param name="spriteBatch">The sprite batch.</param>
        public static void ExitShaderRegion(this SpriteBatch spriteBatch)
        {
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.instance.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        }

        /// <summary>
        /// Sets a <see cref="SpriteBatch"/>'s <see cref="BlendState"/> arbitrarily.
        /// </summary>
        /// <param name="spriteBatch">The sprite batch.</param>
        /// <param name="blendState">The blend state to use.</param>
        public static void SetBlendState(this SpriteBatch spriteBatch, BlendState blendState)
        {
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, blendState, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);
        }

        /// <summary>
        /// Reset's a <see cref="SpriteBatch"/>'s <see cref="BlendState"/> based to a typical <see cref="BlendState.AlphaBlend"/>.
        /// </summary>
        /// <param name="spriteBatch">The sprite batch.</param>
        /// <param name="blendState">The blend state to use.</param>
        public static void ResetBlendState(this SpriteBatch spriteBatch) => Main.spriteBatch.SetBlendState(BlendState.AlphaBlend);

        /// <summary>
        /// Draws a line significantly more efficiently than <see cref="Utils.DrawLine(SpriteBatch, Vector2, Vector2, Color, Color, float)"/> using just one scaled line texture. Positions are automatically converted to screen coordinates.
        /// </summary>
        /// <param name="spriteBatch">The sprite batch by which the line should be drawn.</param>
        /// <param name="start">The starting point of the line in world coordinates.</param>
        /// <param name="end">The ending point of the line in world coordinates.</param>
        /// <param name="color">The color of the line.</param>
        /// <param name="width">The width of the line.</param>
        public static void DrawLineBetter(this SpriteBatch spriteBatch, Vector2 start, Vector2 end, Color color, float width)
        {
            // Draw nothing if the start and end are equal, to prevent division by 0 problems.
            if (start == end)
                return;

            start -= Main.screenPosition;
            end -= Main.screenPosition;

            Texture2D line = ModContent.GetTexture("InfernumMode/ExtraTextures/Line");
            float rotation = (end - start).ToRotation();
            Vector2 scale = new Vector2(Vector2.Distance(start, end) / line.Width, width);

            Main.spriteBatch.Draw(line, start, null, color, rotation, line.Size() * Vector2.UnitY * 0.5f, scale, SpriteEffects.None, 0f);
        }

        /// <summary>
        /// Creates a generic dust explosion at a given position.
        /// </summary>
        /// <param name="spawnPosition">The place to spawn dust at.</param>
        /// <param name="dustType">The dust ID to use.</param>
        /// <param name="dustPerBurst">The amount of dust to spawn per burst.</param>
        /// <param name="burstSpeed">The speed of the dust when exploding.</param>
        /// <param name="baseScale">The scale of the dust</param>
        public static void CreateGenericDustExplosion(Vector2 spawnPosition, int dustType, int dustPerBurst, float burstSpeed, float baseScale)
        {
            // Generate a dust explosion
            float burstDirectionVariance = 3;
            for (int j = 0; j < 10; j++)
            {
                burstDirectionVariance += j * 2;
                for (int k = 0; k < dustPerBurst; k++)
                {
                    Dust burstDust = Dust.NewDustPerfect(spawnPosition, dustType);
                    burstDust.scale = baseScale * Main.rand.NextFloat(0.8f, 1.2f);
                    burstDust.position = spawnPosition + Main.rand.NextVector2Circular(10f, 10f);
                    burstDust.velocity = Main.rand.NextVector2Square(-burstDirectionVariance, burstDirectionVariance).SafeNormalize(Vector2.UnitY) * burstSpeed;
                    burstDust.noGravity = true;
                }
                burstSpeed += 3f;
            }
        }
        /// <summary>
        /// Draws a projectile as a series of afterimages. The first of these afterimages is centered on the center of the projectile's hitbox.<br />
        /// This function is guaranteed to draw the projectile itself, even if it has no afterimages and/or the Afterimages config option is turned off.
        /// </summary>
        /// <param name="proj">The projectile to be drawn.</param>
        /// <param name="mode">The type of afterimage drawing code to use. Vanilla Terraria has three options: 0, 1, and 2.</param>
        /// <param name="lightColor">The light color to use for the afterimages.</param>
        /// <param name="typeOneIncrement">If mode 1 is used, this controls the loop increment. Set it to more than 1 to skip afterimages.</param>
        /// <param name="texture">The texture to draw. Set to <b>null</b> to draw the projectile's own loaded texture.</param>
        /// <param name="drawCentered">If <b>false</b>, the afterimages will be centered on the projectile's position instead of its own center.</param>
        public static void DrawAfterimagesCentered(Projectile proj, Color lightColor, int mode, int typeOneIncrement = 1, Texture2D texture = null, bool drawCentered = true)
        {
            if (texture is null)
                texture = Main.projectileTexture[proj.type];

            int frameHeight = texture.Height / Main.projFrames[proj.type];
            int frameY = frameHeight * proj.frame;
            float scale = proj.scale;
            float rotation = proj.rotation;

            Rectangle rectangle = new Rectangle(0, frameY, texture.Width, frameHeight);
            Vector2 origin = rectangle.Size() / 2f;

            SpriteEffects spriteEffects = SpriteEffects.None;
            if (proj.spriteDirection == -1)
                spriteEffects = SpriteEffects.FlipHorizontally;

            // If no afterimages are drawn due to an invalid mode being specified, ensure the projectile itself is drawn anyway.
            bool failedToDrawAfterimages = false;

            if (CalamityConfig.Instance.Afterimages)
            {
                Vector2 centerOffset = drawCentered ? proj.Size / 2f : Vector2.Zero;
                switch (mode)
                {
                    // Standard afterimages. No customizable features other than total afterimage count.
                    // Type 0 afterimages linearly scale down from 100% to 0% opacity. Their color and lighting is equal to the main projectile's.
                    case 0:
                        for (int i = 0; i < proj.oldPos.Length; ++i)
                        {
                            Vector2 drawPos = proj.oldPos[i] + centerOffset - Main.screenPosition + new Vector2(0f, proj.gfxOffY);
                            // DO NOT REMOVE THESE "UNNECESSARY" FLOAT CASTS. THIS WILL BREAK THE AFTERIMAGES.
                            Color color = proj.GetAlpha(lightColor) * ((float)(proj.oldPos.Length - i) / (float)proj.oldPos.Length);
                            Main.spriteBatch.Draw(texture, drawPos, new Rectangle?(rectangle), color, rotation, origin, scale, spriteEffects, 0f);
                        }
                        break;

                    // Paladin's Hammer style afterimages. Can be optionally spaced out further by using the typeOneDistanceMultiplier variable.
                    // Type 1 afterimages linearly scale down from 66% to 0% opacity. They otherwise do not differ from type 0.
                    case 1:
                        // Safety check: the loop must increment
                        int increment = Math.Max(1, typeOneIncrement);
                        Color drawColor = proj.GetAlpha(lightColor);
                        int afterimageCount = ProjectileID.Sets.TrailCacheLength[proj.type];
                        int k = 0;
                        while (k < afterimageCount)
                        {
                            Vector2 drawPos = proj.oldPos[k] + centerOffset - Main.screenPosition + new Vector2(0f, proj.gfxOffY);
                            // DO NOT REMOVE THESE "UNNECESSARY" FLOAT CASTS EITHER.
                            if (k > 0)
                            {
                                float colorMult = (float)(afterimageCount - k);
                                drawColor *= colorMult / ((float)afterimageCount * 1.5f);
                            }
                            Main.spriteBatch.Draw(texture, drawPos, new Rectangle?(rectangle), drawColor, rotation, origin, scale, spriteEffects, 0f);
                            k += increment;
                        }
                        break;

                    // Standard afterimages with rotation. No customizable features other than total afterimage count.
                    // Type 2 afterimages linearly scale down from 100% to 0% opacity. Their color and lighting is equal to the main projectile's.
                    case 2:
                        for (int i = 0; i < proj.oldPos.Length; ++i)
                        {
                            float afterimageRot = proj.oldRot[i];
                            SpriteEffects sfxForThisAfterimage = proj.oldSpriteDirection[i] == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

                            Vector2 drawPos = proj.oldPos[i] + centerOffset - Main.screenPosition + new Vector2(0f, proj.gfxOffY);
                            // DO NOT REMOVE THESE "UNNECESSARY" FLOAT CASTS. THIS WILL BREAK THE AFTERIMAGES.
                            Color color = proj.GetAlpha(lightColor) * ((float)(proj.oldPos.Length - i) / (float)proj.oldPos.Length);
                            Main.spriteBatch.Draw(texture, drawPos, new Rectangle?(rectangle), color, afterimageRot, origin, scale, sfxForThisAfterimage, 0f);
                        }
                        break;

                    default:
                        failedToDrawAfterimages = true;
                        break;
                }
            }

            // Draw the projectile itself. Only do this if no afterimages are drawn because afterimage 0 is the projectile itself.
            if (!CalamityConfig.Instance.Afterimages || ProjectileID.Sets.TrailCacheLength[proj.type] <= 0 || failedToDrawAfterimages)
            {
                Vector2 startPos = drawCentered ? proj.Center : proj.position;
                Main.spriteBatch.Draw(texture, startPos - Main.screenPosition + new Vector2(0f, proj.gfxOffY), rectangle, proj.GetAlpha(lightColor), rotation, origin, scale, spriteEffects, 0f);
            }
        }

        public static void DisplayText(string text, Color? color = null)
        {
            if (Main.netMode == NetmodeID.SinglePlayer)
                Main.NewText(text, color ?? Color.White);
            else if (Main.netMode == NetmodeID.Server)
                NetMessage.BroadcastChatMessage(NetworkText.FromLiteral(text), color ?? Color.White);
        }
		
		public static void GetCircleVertices(int sideCount, float radius, Vector2 center, out List<short> triangleIndices, out List<VertexPositionColorTexture> vertices)
        {
			vertices = new List<VertexPositionColorTexture>();
			triangleIndices = new List<short>();

            // Use the law of cosines to determine the side length of the triangles that compose the inscribed shape.
            float sideAngle = MathHelper.TwoPi / sideCount;
            float sideLength = (float)Math.Sqrt(2D - Math.Cos(sideAngle) * 2D) * radius;

            // Calculate vertices by approximating a circle with a bunch of triangles.
            for (int i = 0; i < sideCount; i++)
            {
                float completionRatio = i / (float)(sideCount - 1f);
                float nextCompletionRatio = (i + 1) / (float)(sideCount - 1f);
                Vector2 orthogonal = (MathHelper.TwoPi * completionRatio + MathHelper.PiOver2).ToRotationVector2();
                Vector2 radiusOffset = (MathHelper.TwoPi * completionRatio).ToRotationVector2() * radius;
                Vector2 leftEdgeInner = center;
                Vector2 rightEdgeInner = center;
                Vector2 leftEdge = leftEdgeInner + radiusOffset + orthogonal * sideLength * -0.5f;
                Vector2 rightEdge = rightEdgeInner + radiusOffset + orthogonal * sideLength * 0.5f;

				vertices.Add(new VertexPositionColorTexture(new Vector3(leftEdge - Main.screenPosition, 0f), Color.White, new Vector2(completionRatio, 1f)));
				vertices.Add(new VertexPositionColorTexture(new Vector3(rightEdge - Main.screenPosition, 0f), Color.White, new Vector2(nextCompletionRatio, 1f)));
				vertices.Add(new VertexPositionColorTexture(new Vector3(rightEdgeInner - Main.screenPosition, 0f), Color.White, new Vector2(nextCompletionRatio, 0f)));
				vertices.Add(new VertexPositionColorTexture(new Vector3(leftEdgeInner - Main.screenPosition, 0f), Color.White, new Vector2(completionRatio, 0f)));

                triangleIndices.Add((short)(i * 4));
                triangleIndices.Add((short)(i * 4 + 1));
                triangleIndices.Add((short)(i * 4 + 2));
                triangleIndices.Add((short)(i * 4));
                triangleIndices.Add((short)(i * 4 + 2));
                triangleIndices.Add((short)(i * 4 + 3));
            }
        }
		
        public static void CreateShockwave(Vector2 shockwavePosition, int rippleCount = 2, int rippleSize = 8, float rippleSpeed = 75f, bool playSound = true, bool useSecondaryVariant = false)
        {
            DeleteAllProjectiles(false, ModContent.ProjectileType<ScreenShakeProj>());

            if (playSound)
                Main.PlaySound(InfernumMode.Instance.GetLegacySoundSlot(SoundType.Custom, "Sounds/Custom/SonicBoom"), Vector2.Lerp(shockwavePosition, Main.LocalPlayer.Center, 0.84f));

            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                int shockwaveID = NewProjectileBetter(shockwavePosition, Vector2.Zero, ModContent.ProjectileType<ScreenShakeProj>(), 0, 0f, -1, useSecondaryVariant.ToInt());
                if (Main.projectile.IndexInRange(shockwaveID))
                {
                    var shockwave = Main.projectile[shockwaveID].ModProjectile<ScreenShakeProj>();
                    shockwave.RippleCount = rippleCount;
                    shockwave.RippleSize = rippleSize;
                    shockwave.RippleSpeed = rippleSpeed;
                }
            }
        }

        public static void CreateMetaballsFromTexture(this Texture2D texture, ref List<FusableParticle> particleList, Vector2 texturePosition, float textureRotation, float textureScale, float metaballSize, int spawnChance = 35)
        {
            // Leave if this is null, or this is called on the server.
            if (particleList is null || Main.netMode == NetmodeID.Server)
                return;

            // Get the dimensions of the texture.
            int textureWidth = texture.Width;
            int textureHeight = texture.Height;

            // Get the data of every color in the texture.
            Color[] colorData = new Color[textureWidth * textureHeight];
            texture.GetData(colorData);

            // Loop across the texture lengthways, one row at a time.
            for (int h = 0; h < textureHeight; h++)
            {
                for (int w = 0; w < textureWidth; w++)
                {
                    Color color = colorData[w + h * textureWidth];

                    // If the current pixel has any alpha, and the chance is selected (this exists to add variation and prevent having way too many metaballs spawn)
                    if (color.A > 0 && (color.R > 0 && color.G > 0 && color.B > 0) && Main.rand.NextBool(spawnChance))
                    {
                        Vector2 positionOffset = textureScale * new Vector2(textureWidth * 0.5f, textureHeight * 0.5f).RotatedBy(textureRotation);
                        Vector2 metaballSpawnPosition = texturePosition - positionOffset + new Vector2(w, h).RotatedBy(textureRotation);
                        FusableParticle particle = new FusableParticle(metaballSpawnPosition, Main.rand.NextFloat(metaballSize * 0.8f, metaballSize * 1.2f) * color.A / 255);
                        particleList.Add(particle);
                    }
                }
            }
        }

        public static void DrawBloomLineTelegraph(Vector2 drawPosition, BloomLineDrawInfo drawInfo, bool resetSpritebatch = true, Vector2? resolution = null)
        {
            // Claim texture and shader data in easy to use local variables.
            Texture2D invisible = ModContent.GetTexture("InfernumMode/ExtraTextures/Invisible");
            Effect laserScopeEffect = Filters.Scene["Infernum:PixelatedSightLine"].GetShader().Shader;

            // Prepare all parameters for the shader in anticipation that they will go the GPU for shader effects.
            laserScopeEffect.Parameters["sampleTexture2"].SetValue(ModContent.GetTexture("InfernumMode/ExtraTextures/CertifiedCrustyNoise"));
            laserScopeEffect.Parameters["noiseOffset"].SetValue(Main.GameUpdateCount * -0.004f);
            laserScopeEffect.Parameters["mainOpacity"].SetValue(drawInfo.Opacity);
            laserScopeEffect.Parameters["Resolution"].SetValue(resolution ?? Vector2.One * 425f);
            laserScopeEffect.Parameters["laserAngle"].SetValue(drawInfo.LineRotation);
            laserScopeEffect.Parameters["laserWidth"].SetValue(drawInfo.WidthFactor);
            laserScopeEffect.Parameters["laserLightStrenght"].SetValue(drawInfo.LightStrength);
            laserScopeEffect.Parameters["color"].SetValue(drawInfo.MainColor.ToVector3());
            laserScopeEffect.Parameters["darkerColor"].SetValue(drawInfo.DarkerColor.ToVector3());
            laserScopeEffect.Parameters["bloomSize"].SetValue(drawInfo.BloomIntensity);
            laserScopeEffect.Parameters["bloomMaxOpacity"].SetValue(drawInfo.BloomOpacity);
            laserScopeEffect.Parameters["bloomFadeStrenght"].SetValue(3f);

            // Prepare the sprite batch for shader drawing.
            if (resetSpritebatch)
                Main.spriteBatch.EnterShaderRegion(BlendState.Additive);
            laserScopeEffect.CurrentTechnique.Passes[0].Apply();

            // Draw the texture with the shader and flush the results to the GPU, clearing the shader effect for any successive draw calls.
            Main.spriteBatch.Draw(invisible, drawPosition, null, Color.White, 0f, invisible.Size() * 0.5f, drawInfo.Scale, SpriteEffects.None, 0f);
            if (resetSpritebatch)
                Main.spriteBatch.ExitShaderRegion();
        }			
    }
}
