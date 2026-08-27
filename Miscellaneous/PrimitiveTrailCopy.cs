using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Graphics.Shaders;

namespace InfernumMode
{

    public class PrimitiveTrailCopy
    {
        public struct VertexPosition2DColor : IVertexType
        {
            public Vector2 Position;
            public Color Color;
            public Vector2 TextureCoordinates;
            public VertexDeclaration VertexDeclaration => _vertexDeclaration;

            private static readonly VertexDeclaration _vertexDeclaration = new VertexDeclaration(new VertexElement[]
            {
                new VertexElement(0, VertexElementFormat.Vector2, VertexElementUsage.Position, 0),
                new VertexElement(8, VertexElementFormat.Color, VertexElementUsage.Color, 0),
                new VertexElement(12, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),
            });

            public VertexPosition2DColor(Vector2 position, Color color, Vector2 textureCoordinates)
            {
                Position = position;
                Color = color;
                TextureCoordinates = textureCoordinates;
            }
        }

        // Dedicated instances — never mutate RasterizerState.CullNone (becomes read-only after bind).
        private static readonly RasterizerState RasterizerCullNone = new RasterizerState
        {
            CullMode = CullMode.None,
            ScissorTestEnable = false
        };

        internal Matrix? PerspectiveMatrixOverride;

        public delegate float VertexWidthFunction(float completionRatio);
        public delegate Vector2 VertexOffsetFunction(float completionRatio);
        public delegate Color VertexColorFunction(float completionRatio);

        public VertexWidthFunction WidthFunction;
        public VertexColorFunction ColorFunction;
        public VertexOffsetFunction OffsetFunction;

        public bool UsesSmoothening;
        public MiscShaderData SpecialShader;

        public static BasicEffect BaseEffect
        {
            get;
            private set;
        }

        public PrimitiveTrailCopy(VertexWidthFunction widthFunction, VertexColorFunction colorFunction, VertexOffsetFunction offsetFunction = null, bool useSmoothening = true, MiscShaderData specialShader = null)
        {
            if (widthFunction is null || colorFunction is null)
                throw new NullReferenceException($"In order to create a primitive trail, a non-null {(widthFunction is null ? "width" : "color")} function must be specified.");

            WidthFunction = widthFunction;
            ColorFunction = colorFunction;
            OffsetFunction = offsetFunction;
            UsesSmoothening = useSmoothening;

            if (specialShader != null)
                SpecialShader = specialShader;

            if (BaseEffect == null)
            {
                BaseEffect = new BasicEffect(Main.instance.GraphicsDevice)
                {
                    VertexColorEnabled = true,
                    TextureEnabled = false
                };
            }
            UpdateBaseEffect(out _, out _);
        }

        public static void UpdateBaseEffect(out Matrix effectProjection, out Matrix effectView)
        {
            int height = Main.instance.GraphicsDevice.Viewport.Height;

            Vector2 zoom = Main.GameViewMatrix.Zoom;
            Matrix zoomScaleMatrix = Matrix.CreateScale(zoom.X, zoom.Y, 1f);

            effectView = Matrix.CreateLookAt(Vector3.Zero, Vector3.UnitZ, Vector3.Up);
            effectView *= Matrix.CreateTranslation(0f, -height, 0f);
            effectView *= Matrix.CreateRotationZ(MathHelper.Pi);

            if (Main.LocalPlayer.gravDir == -1f)
                effectView *= Matrix.CreateScale(1f, -1f, 1f) * Matrix.CreateTranslation(0f, height, 0f);

            effectView *= zoomScaleMatrix;

            effectProjection = Matrix.CreateOrthographicOffCenter(0f, Main.screenWidth * zoom.X, 0f, Main.screenHeight * zoom.Y, 0f, 1f) * zoomScaleMatrix;
            BaseEffect.View = effectView;
            BaseEffect.Projection = effectProjection;
        }

        /// <summary>
        /// Full Main.screenWidth/Height ortho (not the half-res RT size) so full-res
        /// coordinates compress into the half-res buffer (pixelation on upscale x2).
        ///
        /// Zoom (BetterZoom etc.) is applied HERE around the screen centre so trails
        /// stay aligned with the zoomed world. The RT is later presented with
        /// Matrix.Identity (no second zoom) to avoid edge clipping.
        /// </summary>
        public static void UpdatePixelatedBaseEffect(out Matrix effectProjection, out Matrix effectView)
        {
            effectProjection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

            Vector2 zoom = Main.GameViewMatrix.Zoom;
            // Fast path: no zoom mods / default zoom → pure Identity (original 1.4 behaviour).
            if (zoom.X == 1f && zoom.Y == 1f)
            {
                effectView = Matrix.Identity;
            }
            else
            {
                // Scale around screen centre — same idea as GameViewMatrix zoom.
                Vector2 center = new Vector2(Main.screenWidth * 0.5f, Main.screenHeight * 0.5f);
                effectView =
                    Matrix.CreateTranslation(-center.X, -center.Y, 0f) *
                    Matrix.CreateScale(zoom.X, zoom.Y, 1f) *
                    Matrix.CreateTranslation(center.X, center.Y, 0f);

                // Match inverted gravity if needed.
                if (Main.LocalPlayer.gravDir == -1f)
                {
                    effectView *= Matrix.CreateScale(1f, -1f, 1f) *
                                  Matrix.CreateTranslation(0f, Main.screenHeight, 0f);
                }
            }

            BaseEffect.Projection = effectProjection;
            BaseEffect.View = effectView;
        }

        public List<Vector2> GetTrailPoints(IEnumerable<Vector2> originalPositions, Vector2 generalOffset, int totalTrailPoints)
        {
            if (!UsesSmoothening)
            {
                List<Vector2> basePoints = originalPositions.Where(p => p != Vector2.Zero).ToList();
                List<Vector2> endPoints = new List<Vector2>();

                if (basePoints.Count <= 2)
                    return endPoints;

                for (int i = 0; i < basePoints.Count; i++)
                {
                    Vector2 offset = generalOffset;
                    if (OffsetFunction != null)
                        offset += OffsetFunction(i / (float)(basePoints.Count - 1f));

                    endPoints.Add(basePoints[i] + offset);
                }
                return endPoints;
            }

            List<Vector2> controlPoints = new List<Vector2>();
            int count = originalPositions.Count();
            for (int i = 0; i < count; i++)
            {
                Vector2 pos = originalPositions.ElementAt(i);
                if (pos == Vector2.Zero)
                    continue;

                float completionRatio = i / (float)count;
                Vector2 offset = generalOffset;
                if (OffsetFunction != null)
                    offset += OffsetFunction(completionRatio);
                controlPoints.Add(pos + offset);
            }

            if (controlPoints.Count <= 4)
                return controlPoints;

            List<Vector2> points = new List<Vector2>();

            for (int j = 0; j < totalTrailPoints; j++)
            {
                float splineInterpolant = j / (float)totalTrailPoints;
                float localSplineInterpolant = splineInterpolant * (controlPoints.Count - 1f) % 1f;
                int localSplineIndex = (int)(splineInterpolant * (controlPoints.Count - 1f));

                Vector2 left = controlPoints[localSplineIndex];
                Vector2 right = controlPoints[localSplineIndex + 1];
                Vector2 farLeft;
                Vector2 farRight;

                if (localSplineIndex <= 0)
                    farLeft = left * 2f - right;
                else
                    farLeft = controlPoints[localSplineIndex - 1];

                if (localSplineIndex >= controlPoints.Count - 2)
                    farRight = right * 2f - left;
                else
                    farRight = controlPoints[localSplineIndex + 2];

                points.Add(Vector2.CatmullRom(farLeft, left, right, farRight, localSplineInterpolant));
            }

            points.Insert(0, controlPoints.First());
            points.Add(controlPoints.Last());

            return points;
        }

        public List<VertexPosition2DColor> GetVerticesFromTrailPoints(List<Vector2> trailPoints, float? directionOverride = null)
        {
            List<VertexPosition2DColor> vertices = new List<VertexPosition2DColor>();

            for (int i = 0; i < trailPoints.Count - 1; i++)
            {
                float completionRatio = i / (float)trailPoints.Count;
                float widthAtVertex = WidthFunction(completionRatio);
                Color vertexColor = ColorFunction(completionRatio);

                Vector2 currentPosition = trailPoints[i];
                Vector2 positionAhead = trailPoints[i + 1];
                Vector2 directionToAhead = (positionAhead - trailPoints[i]).SafeNormalize(Vector2.Zero);
                if (directionOverride.HasValue)
                    directionToAhead = directionOverride.Value.ToRotationVector2();

                Vector2 leftCurrentTextureCoord = new Vector2(completionRatio, 0f);
                Vector2 rightCurrentTextureCoord = new Vector2(completionRatio, 1f);

                Vector2 sideDirection = new Vector2(-directionToAhead.Y, directionToAhead.X);

                vertices.Add(new VertexPosition2DColor(currentPosition - sideDirection * widthAtVertex, vertexColor, leftCurrentTextureCoord));
                vertices.Add(new VertexPosition2DColor(currentPosition + sideDirection * widthAtVertex, vertexColor, rightCurrentTextureCoord));
            }

            return vertices;
        }

        public static List<short> GetIndicesFromTrailPoints(int pointCount)
        {
            // One quad (2 triangles = 6 indices) per consecutive vertex-pair strip element.
            // Vertex buffer has 2 vertices per trail point except the last → 2*(pointCount-1) verts.
            // Number of quads = pointCount - 2.
            if (pointCount < 3)
                return new List<short>();

            int quadCount = pointCount - 2;
            short[] indices = new short[quadCount * 6];

            for (int i = 0; i < quadCount; i++)
            {
                int startingTriangleIndex = i * 6;
                int connectToIndex = i * 2;
                indices[startingTriangleIndex] = (short)connectToIndex;
                indices[startingTriangleIndex + 1] = (short)(connectToIndex + 1);
                indices[startingTriangleIndex + 2] = (short)(connectToIndex + 2);
                indices[startingTriangleIndex + 3] = (short)(connectToIndex + 2);
                indices[startingTriangleIndex + 4] = (short)(connectToIndex + 1);
                indices[startingTriangleIndex + 5] = (short)(connectToIndex + 3);
            }

            return indices.ToList();
        }

        public void SpecifyPerspectiveMatrix(Matrix m) => PerspectiveMatrixOverride = m;

        public void Draw(IEnumerable<Vector2> originalPositions, Vector2 generalOffset, int totalTrailPoints, float? directionOverride = null)
            => DrawPrims(originalPositions, generalOffset, totalTrailPoints, false, directionOverride);

        public void DrawPixelated(IEnumerable<Vector2> originalPositions, Vector2 generalOffset, int totalTrailPoints, float? directionOverride = null)
            => DrawPrims(originalPositions, generalOffset, totalTrailPoints, true, directionOverride);

        private void DrawPrims(IEnumerable<Vector2> originalPositions, Vector2 generalOffset, int totalTrailPoints, bool pixelated, float? directionOverride = null)
        {
            if (originalPositions.Count() <= 2)
                return;

            originalPositions = originalPositions.Where(p => p != Vector2.Zero);
            List<Vector2> trailPoints = GetTrailPoints(originalPositions, generalOffset, totalTrailPoints);

            if (trailPoints.Count <= 2)
                return;

            if (trailPoints.Any(point => point.HasNaNs()))
                return;

            if (trailPoints.All(point => point == trailPoints[0]))
                return;

            DrawPrimsFromVertexData(
                GetVerticesFromTrailPoints(trailPoints, directionOverride),
                GetIndicesFromTrailPoints(trailPoints.Count),
                pixelated);
        }

        internal void DrawPrimsFromVertexData(List<VertexPosition2DColor> vertices, List<short> triangleIndices, bool pixelated)
        {
            if (triangleIndices == null || vertices == null)
                return;
            if (triangleIndices.Count < 6 || triangleIndices.Count % 3 != 0 || vertices.Count <= 3)
                return;

            // Reject any index that would be out of range (GPU "unexpected error").
            int maxIndex = vertices.Count - 1;
            for (int i = 0; i < triangleIndices.Count; i++)
            {
                if (triangleIndices[i] < 0 || triangleIndices[i] > maxIndex)
                    return;
            }

            // Reject NaN / Infinity positions — Intel drivers often hard-crash on these
            // instead of quietly clipping (InvalidOperationException: unexpected error).
            for (int i = 0; i < vertices.Count; i++)
            {
                Vector2 p = vertices[i].Position;
                if (float.IsNaN(p.X) || float.IsNaN(p.Y) || float.IsInfinity(p.X) || float.IsInfinity(p.Y))
                    return;
            }

            int primitiveCount = triangleIndices.Count / 3;
            if (primitiveCount <= 0)
                return;

            // Cap batch size — some integrated GPUs reject very large immediate draws.
            if (vertices.Count > 16384 || primitiveCount > 8192)
                return;

            Matrix projection;
            Matrix view;

            if (pixelated)
                UpdatePixelatedBaseEffect(out projection, out view);
            else
                UpdateBaseEffect(out projection, out view);

            GraphicsDevice device = Main.instance.GraphicsDevice;
            if (device == null || device.IsDisposed)
                return;

            // Save states we are about to mutate so we can restore them even if the draw throws.
            // Leaving the device in a bad state after a failed DrawUserIndexedPrimitives is what
            // causes the later SpriteBatch.End crash in Main.DoDraw.
            var oldRasterizer = device.RasterizerState;
            var oldBlend = device.BlendState;
            var oldDepth = device.DepthStencilState;
            var oldSampler0 = device.SamplerStates[0];

            VertexBufferBinding[] oldVertexBindings = null;
            IndexBuffer oldIndexBuffer = null;
            try
            {
                oldVertexBindings = device.GetVertexBuffers();
                oldIndexBuffer = device.Indices;
            }
            catch { }

            device.RasterizerState = RasterizerCullNone;
            device.BlendState = BlendState.AlphaBlend;
            device.DepthStencilState = DepthStencilState.None;

            // Intentionally do NOT set ScissorRectangle.
            // Full-screen scissor is invalid while a half-res RT is bound.

            try
            {
                if (SpecialShader != null && SpecialShader.Shader != null)
                {
                    var param = SpecialShader.Shader.Parameters["uWorldViewProjection"];
                    if (param != null)
                        param.SetValue(PerspectiveMatrixOverride ?? (view * projection));
                    SpecialShader.Apply();
                    PerspectiveMatrixOverride = null;
                }
                else if (BaseEffect != null)
                {
                    BaseEffect.CurrentTechnique.Passes[0].Apply();
                }
                else
                {
                    return;
                }

                device.DrawUserIndexedPrimitives(
                    PrimitiveType.TriangleList,
                    vertices.ToArray(),
                    0,
                    vertices.Count,
                    triangleIndices.ToArray(),
                    0,
                    primitiveCount);
            }
            catch
            {
                // Swallow GPU/state errors so one bad trail cannot crash the client.
                // Intel UHD and similar integrated GPUs periodically throw
                // InvalidOperationException ("An unexpected error has occurred") here.
            }
            finally
            {
                PerspectiveMatrixOverride = null;

                // Restore device state so SpriteBatch / the rest of the frame is not corrupted.
                try
                {
                    device.RasterizerState = oldRasterizer ?? RasterizerState.CullCounterClockwise;
                    device.BlendState = oldBlend ?? BlendState.AlphaBlend;
                    device.DepthStencilState = oldDepth ?? DepthStencilState.None;
                    if (oldSampler0 != null)
                        device.SamplerStates[0] = oldSampler0;
                }
                catch { }

                // Explicitly detach the vertex/index buffers used by DrawUserIndexedPrimitives above.
                // GraphicsDevice caches "currently bound vertex/index buffer" by reference and skips
                // redundant rebinds. SpriteBatch relies on that same caching to avoid rebinding its own
                // internal buffer every draw call. Because DrawUserIndexedPrimitives changes the actual
                // bound buffers on the GPU without SpriteBatch's knowledge, the device's cached reference
                // can end up out of sync with reality, so a later spriteBatch.Draw() thinks its buffer is
                // still bound and skips rebinding it, which throws "A valid vertex buffer ... must be set
                // on the device" (this is what was crashing during the Profaned Guardians fight). Setting
                // both to null invalidates the cached reference, forcing SpriteBatch to properly rebind
                // its own buffer the next time it draws, regardless of what ran in between.
                try
                {
                    if (oldVertexBindings != null && oldVertexBindings.Length > 0)
                        device.SetVertexBuffers(oldVertexBindings);
                    else
                        device.SetVertexBuffer(null);
                    device.Indices = oldIndexBuffer;
                }
                catch { }

                try
                {
                    if (Main.pixelShader != null)
                        Main.pixelShader.CurrentTechnique.Passes[0].Apply();
                }
                catch { }
            }
        }

        public static void Dispose()
        {
            BaseEffect?.Dispose();
            BaseEffect = null;
        }
    }
}
