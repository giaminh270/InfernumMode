using InfernumMode.GlobalInstances;
using Terraria;
using Microsoft.Xna.Framework;

namespace InfernumMode
{
    public static partial class Utilities
    {
        public static PoDPlayer Infernum(this Player player) => player.GetModPlayer<PoDPlayer>();
        public static GlobalNPCOverrides Infernum(this NPC npc) => npc.GetGlobalNPC<GlobalNPCOverrides>();
        public static GlobalProjectileOverrides Infernum(this Projectile projectile) => projectile.GetGlobalProjectile<GlobalProjectileOverrides>();

		public static bool WithinRange(this Vector2 Origin, Vector2 Target, float MaxRange) => (double) Vector2.DistanceSquared(Origin, Target) <= (double) MaxRange * (double) MaxRange;
    }
}

