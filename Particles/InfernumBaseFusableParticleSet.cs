using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;

namespace InfernumMode.Particles
{
    public abstract class InfernumBaseFusableParticleSet
    {
        public class FusableParticleRenderCollection
        {
            public InfernumBaseFusableParticleSet ParticleSet;
            public List<RenderTarget2D> BackgroundTargets;
            public FusableParticleRenderCollection(InfernumBaseFusableParticleSet set, List<RenderTarget2D> backgroundTargets)
            {
                ParticleSet = set;
                BackgroundTargets = backgroundTargets;
            }
        }

        public class FusableParticle
        {
            public Vector2 Center;
            public float Size;
            public int GridX, GridY; // Vị trí trong spatial grid
            public Vector2 TopLeft => Center + new Vector2(-1f, -1f) * Size * 0.5f;
            public Vector2 TopRight => Center + new Vector2(1f, -1f) * Size * 0.5f;
            public Vector2 BottomLeft => Center + new Vector2(-1f, 1f) * Size * 0.5f;
            public Vector2 BottomRight => Center + new Vector2(1f, 1f) * Size * 0.5f;

            public FusableParticle(Vector2 center, float size)
            {
                Center = center;
                Size = size;
            }
        }

        public int LayerCount
        {
            get
            {
                if (BackgroundShaders.Count != BackgroundTextures.Count)
                    throw new InvalidOperationException("The number of texture maps and shaders are not equivalent.");
                return BackgroundShaders.Count;
            }
        }

        internal List<DrawData> DrawDataBuffer = new List<DrawData>();
        public List<FusableParticle> Particles = new List<FusableParticle>();

        // Spatial partitioning
        private List<FusableParticle>[,] spatialGrid;
        private int gridWidth, gridHeight;
        private const int GridCellSize = 300; // Tăng cell size để giảm memory

        public FusableParticleRenderCollection RenderCollection => InfernumFusableParticleManager.GetParticleRenderCollectionByType(GetType());
        public List<RenderTarget2D> GetBackgroundTargets => RenderCollection.BackgroundTargets;

        public void PrepareSpecialDrawingForNextFrame(params DrawData[] drawContents) => DrawDataBuffer.AddRange(drawContents);
        
        public virtual float BorderSize => 0f;
        public virtual bool BorderShouldBeSolid => false;
        public virtual Color BorderColor => Color.Transparent;
        public virtual void PrepareOptionalShaderData(Effect effect, int index) { }
        public abstract InfernumFusableParticleRenderLayer RenderLayer { get; }
        public abstract List<Effect> BackgroundShaders { get; }
        public abstract List<Texture2D> BackgroundTextures { get; }
        public abstract FusableParticle SpawnParticle(Vector2 center, float sizeStrength);
        public abstract void UpdateBehavior(FusableParticle particle);
        public abstract void DrawParticles();

        public InfernumBaseFusableParticleSet()
        {
            InitializeSpatialGrid();
        }

        #region Spatial Partitioning Methods
        public void InitializeSpatialGrid()
        {
            if (Main.netMode == NetmodeID.Server) return;

            gridWidth = (Main.maxTilesX * 16) / GridCellSize + 1;
            gridHeight = (Main.maxTilesY * 16) / GridCellSize + 1;
            spatialGrid = new List<FusableParticle>[gridWidth, gridHeight];
            
            for (int x = 0; x < gridWidth; x++)
                for (int y = 0; y < gridHeight; y++)
                    spatialGrid[x, y] = new List<FusableParticle>();
        }

        public void AddParticleToGrid(FusableParticle particle)
        {
            if (spatialGrid == null) return;

            int gridX = (int)(particle.Center.X / GridCellSize);
            int gridY = (int)(particle.Center.Y / GridCellSize);
            
            particle.GridX = gridX;
            particle.GridY = gridY;
            
            if (gridX >= 0 && gridX < gridWidth && gridY >= 0 && gridY < gridHeight)
                spatialGrid[gridX, gridY].Add(particle);
        }

        public void UpdateParticleInGrid(FusableParticle particle)
        {
            if (spatialGrid == null) return;

            int newGridX = (int)(particle.Center.X / GridCellSize);
            int newGridY = (int)(particle.Center.Y / GridCellSize);

            // Nếu particle di chuyển sang cell khác
            if (newGridX != particle.GridX || newGridY != particle.GridY)
            {
                // Remove từ cell cũ
                if (particle.GridX >= 0 && particle.GridX < gridWidth && 
                    particle.GridY >= 0 && particle.GridY < gridHeight)
                {
                    spatialGrid[particle.GridX, particle.GridY].Remove(particle);
                }
                
                // Add vào cell mới
                particle.GridX = newGridX;
                particle.GridY = newGridY;
                if (newGridX >= 0 && newGridX < gridWidth && newGridY >= 0 && newGridY < gridHeight)
                    spatialGrid[newGridX, newGridY].Add(particle);
            }
        }

        public List<FusableParticle> GetParticlesInArea(Rectangle area)
        {
            if (spatialGrid == null) return Particles;

            var result = new List<FusableParticle>();
            int startX = Math.Max(0, area.X / GridCellSize);
            int endX = Math.Min(gridWidth - 1, area.Right / GridCellSize);
            int startY = Math.Max(0, area.Y / GridCellSize);
            int endY = Math.Min(gridHeight - 1, area.Bottom / GridCellSize);
            
            for (int x = startX; x <= endX; x++)
                for (int y = startY; y <= endY; y++)
                    result.AddRange(spatialGrid[x, y]);
                    
            return result;
        }
        #endregion

        #region Optimization Methods
        public virtual bool ShouldDrawParticle(FusableParticle particle)
        {
            // Culling: chỉ vẽ hạt trong tầm nhìn
            Rectangle screenRect = new Rectangle((int)Main.screenPosition.X, (int)Main.screenPosition.Y, 
                                               Main.screenWidth, Main.screenHeight);
            Rectangle particleRect = new Rectangle((int)(particle.Center.X - particle.Size), 
                                                 (int)(particle.Center.Y - particle.Size),
                                                 (int)(particle.Size * 2), (int)(particle.Size * 2));
            
            if (!screenRect.Intersects(particleRect))
                return false;
                
            // LOD: bỏ qua hạt quá nhỏ
            float minVisibleSize = 1.5f;
            if (particle.Size * Main.GameViewMatrix.Zoom.X < minVisibleSize)
                return false;
                
            return true;
        }

        public void UpdateAllParticles()
        {
            // Update và xoá hạt chết
            for (int i = Particles.Count - 1; i >= 0; i--)
            {
                var particle = Particles[i];
                UpdateBehavior(particle);
                
                // Xoá hạt ngay lập tức thay vì chờ đến draw
                if (particle.Size <= 1f)
                {
                    // Remove từ spatial grid
                    if (particle.GridX >= 0 && particle.GridX < gridWidth && 
                        particle.GridY >= 0 && particle.GridY < gridHeight)
                    {
                        spatialGrid[particle.GridX, particle.GridY].Remove(particle);
                    }
                    Particles.RemoveAt(i);
                }
            }
        }
        #endregion

        internal void PrepareRenderTargetForDrawing()
        {
            if (Main.netMode == NetmodeID.Server || Particles.Count <= 0)
                return;

            // Update particles trước khi draw
            UpdateAllParticles();

            foreach (RenderTarget2D backgroundTarget in GetBackgroundTargets)
            {
                Main.instance.GraphicsDevice.SetRenderTarget(backgroundTarget);
                Main.instance.GraphicsDevice.Clear(Color.Transparent);

                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, 
                                     Main.DefaultSamplerState, DepthStencilState.None, 
                                     Main.instance.Rasterizer, null, Main.GameViewMatrix.EffectMatrix);

                DrawParticles();

                foreach (DrawData drawData in DrawDataBuffer)
                    drawData.Draw(Main.spriteBatch);

                Main.spriteBatch.End();
            }

            DrawDataBuffer.Clear();
            Main.instance.GraphicsDevice.SetRenderTarget(null);
        }
    }
}