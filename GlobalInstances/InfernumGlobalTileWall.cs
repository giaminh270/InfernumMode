using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.GlobalInstances
{
    public class InfernumGlobalWall : GlobalWall
    {
        public override bool CanExplode(int i, int j, int type)
        {
            if (PoDWorld.ProvidenceArena.Intersects(new Rectangle(i, j, 16, 16)))
                return false;

            return base.CanExplode(i, j, type);
        }
    }
}
