using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Utilities;

namespace Microsoft.Xna.Framework
{
    public static class ExtensionMethod
    {
        /// <summary>
        /// Returns the normalized direction vector pointing from <paramref name="from"/> to <paramref name="to"/>.
        /// Safely handles zero-length vectors by returning Vector2.UnitX.
        /// </summary>		
		public static Vector2 DirectionTo(this Vector2 Origin, Vector2 Target)
		{
		  return Vector2.Normalize(Target - Origin);
		}
		
		public static T NextFromList<T>(this UnifiedRandom random, params T[] objs)
		{
		  return objs[random.Next(objs.Length)];
		}
    }
}