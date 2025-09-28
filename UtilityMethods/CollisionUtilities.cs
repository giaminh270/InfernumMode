using CalamityMod;
using InfernumMode.Miscellaneous;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.World.Generation;

namespace InfernumMode
{
    public static partial class Utilities
    {
        public static Vector2 GetGroundPositionFrom(Vector2 v, GenSearch search = null)
        {
            Point tileCoordinates = v.ToTileCoordinates();
            if (!WorldGen.InWorld(tileCoordinates.X, tileCoordinates.Y))
                return v;
            
            if (search is null)
                search = new Searches.Down(9001);
            if (!WorldUtils.Find(tileCoordinates, Searches.Chain(search, new Conditions.IsSolid(), new CustomTileConditions.ActiveAndNotActuated()), out Point result))
                return v;
            if (!WorldGen.InWorld(result.X, result.Y))
                return v;
            
            return result.ToWorldCoordinates();
        }

        public static bool RotatingHitboxCollision(this Entity entity, Vector2 targetTopLeft, Vector2 targetHitboxDimensions, Vector2? directionOverride = null)
        {
            Vector2 lineDirection = directionOverride ?? entity.velocity;

            // Ensure that the line direction is a unit vector.
            lineDirection = lineDirection.SafeNormalize(Vector2.UnitY);
            Vector2 start = entity.Center - lineDirection * entity.height * 0.5f;
            Vector2 end = entity.Center + lineDirection * entity.height * 0.5f;

            float _ = 0f;
            return Collision.CheckAABBvLineCollision(targetTopLeft, targetHitboxDimensions, start, end, entity.width, ref _);
        }

        public static bool CircularCollision(Vector2 checkPosition, Rectangle hitbox, float radius)
        {
            float dist1 = Vector2.Distance(checkPosition, hitbox.TopLeft());
            float dist2 = Vector2.Distance(checkPosition, hitbox.TopRight());
            float dist3 = Vector2.Distance(checkPosition, hitbox.BottomLeft());
            float dist4 = Vector2.Distance(checkPosition, hitbox.BottomRight());

            float minDist = dist1;
            if (dist2 < minDist)
                minDist = dist2;
            if (dist3 < minDist)
                minDist = dist3;
            if (dist4 < minDist)
                minDist = dist4;

            return minDist <= radius;
        }

        public static bool EllipseCollision(Vector2 checkPosition, Vector2 focus1, Vector2 focus2, float distanceConstant, out float distance)
        {
            float distance1 = Vector2.Distance(checkPosition, focus1);
            float distance2 = Vector2.Distance(checkPosition, focus2);
            distance = distance1 + distance2;
            return distance <= distanceConstant;
        }
		
        public static bool ActualSolidCollisionTop(Vector2 topLeft, int width, int height)
        {
            int x = (int)(topLeft.X / 16f);
            int y = (int)(topLeft.Y / 16f);
            for (int i = x; i < x + width / 16; i++)
            {
                for (int j = y; j < y + height / 16; j++)
                {
                    Tile t = CalamityUtils.ParanoidTileRetrieval(i, j);
                    if (!t.nactive())
                        continue;

                    if (t.type == TileID.Platforms || TileID.Sets.Platforms[t.type])
                        return true;
                }
            }

            bool halfCorrectCheck = Collision.SolidCollision(topLeft, width, height);
            return halfCorrectCheck;
        }
		
		public static float GetGroundPosition(NPC npc)
		{
			int startX = (int)(npc.position.X / 16);
			int endX = (int)((npc.position.X + npc.width) / 16);
			int y = (int)(npc.position.Y / 16) + 1;
			
			float groundY = npc.position.Y;
			
			// Quét xuống dưới để tìm ground/platform
			for (int j = y; j < Main.maxTilesY; j++)
			{
				bool foundGround = false;
				for (int i = startX; i <= endX; i++)
				{
					if (WorldGen.InWorld(i, j))
					{
						Tile tile = Main.tile[i, j];
						if (tile.active() && (Main.tileSolid[tile.type] || tile.type == TileID.Platforms || Main.tileSolidTop[tile.type]))
						{
							groundY = j * 16 - npc.height;
							foundGround = true;
							break;
						}
					}
				}
				if (foundGround)
					break;
			}
			
			return groundY;
		}

		public static bool HasHitGroundOrPlatform(NPC npc)
		{
			if (Collision.SolidCollision(npc.TopLeft, npc.width, npc.height + 4))
				return true;
			
			int startX = (int)(npc.position.X / 16) - 2;
			int endX = (int)((npc.position.X + npc.width) / 16) + 2;
			int startY = (int)(npc.position.Y / 16) - 2;
			int endY = (int)((npc.position.Y + npc.height + 4) / 16) + 2;
			
			for (int i = startX; i < endX; i++)
			{
				for (int j = startY; j < endY; j++)
				{
					if (WorldGen.InWorld(i, j))
					{
						Tile tile = Main.tile[i, j];
						if (tile.active() && (tile.type == TileID.Platforms || Main.tileSolidTop[tile.type]))
						{
							Rectangle tileRect = new Rectangle(i * 16, j * 16, 16, 16);
							Rectangle npcRect = new Rectangle((int)npc.position.X, (int)npc.position.Y, npc.width, npc.height + 4);
							
							if (npcRect.Intersects(tileRect))
								return true;
						}
					}
				}
			}
			
			return false;
		}		
    }
}
