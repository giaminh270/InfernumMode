using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace InfernumMode.BehaviorOverrides.BossAIs.AquaticScourge
{
    public static class SulphuricWaterSafeZoneSystem
    {
        public static Dictionary<Point, float> NearbySafeTiles
        {
            get;
            private set;
        } = new Dictionary<Point, float>();
    }
}
