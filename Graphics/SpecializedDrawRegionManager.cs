using InfernumMode.Graphics.Interfaces;
using InfernumMode.ILEditingStuff;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using InfernumMode;

namespace InfernumMode.Graphics
{
    /// <summary>
    /// Groups ISpecializedDrawRegion projectiles by type.
    /// After Main.DrawProjectiles the SpriteBatch is CLOSED — we open a default
    /// batch first so PrepareSpriteBatch (EnterShaderRegion / End+Begin) works.
    /// </summary>
    public static class SpecializedDrawRegionManager
    {
        public static void DrawSpecializedProjectileGroups()
        {
            List<Projectile> specialProjectiles = new List<Projectile>();
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile p = Main.projectile[i];
                if (p.active && p.modProjectile != null && p.modProjectile is ISpecializedDrawRegion)
                    specialProjectiles.Add(p);
            }

            if (specialProjectiles.Count == 0)
                return;

            // DrawProjectiles leaves SpriteBatch ended. EnterShaderRegion / PrepareSpriteBatch
            // expects an OPEN batch (they End then Begin). Open a default one first.
            Main.spriteBatch.Begin(
                SpriteSortMode.Deferred,
                BlendState.AlphaBlend,
                Main.DefaultSamplerState,
                DepthStencilState.None,
                Main.instance.Rasterizer,
                null,
                Main.GameViewMatrix.TransformationMatrix);

            foreach (IGrouping<int, Projectile> projectileGroup in specialProjectiles.GroupBy(p => p.type))
            {
                ISpecializedDrawRegion regionProperties = projectileGroup.First().modProjectile as ISpecializedDrawRegion;
                if (regionProperties == null)
                    continue;

                regionProperties.PrepareSpriteBatch(Main.spriteBatch);

                foreach (Projectile proj in projectileGroup)
                {
                    ISpecializedDrawRegion drawer = proj.modProjectile as ISpecializedDrawRegion;
                    if (drawer != null)
                        drawer.SpecialDraw(Main.spriteBatch);
                }
            }

            // CRITICAL: leave SpriteBatch CLOSED.
            // ExitShaderRegion ends with Begin → would break DrawPlayers (Catalyst/vanilla).
            Main.spriteBatch.End();
        }
    }
}
