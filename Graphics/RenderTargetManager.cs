using System.Collections.Generic;
using InfernumMode.ILEditingStuff;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace InfernumMode.Graphics
{
    /// <summary>
    /// Central registry for ManagedRenderTarget instances.
    /// Recreates targets when the game resolution changes.
    ///
    /// Backport of the role played by Luminance's ManagedRenderTarget system
    /// (Infernum 1.4 depends on Luminance; 1.3 needs this local equivalent).
    /// </summary>
    public class RenderTargetManager : IHookEdit
    {
        private static readonly List<ManagedRenderTarget> tracked = new List<ManagedRenderTarget>();
        private static Vector2 previousScreenSize;

        public void Load()
        {
            previousScreenSize = new Vector2(Main.screenWidth, Main.screenHeight);
            // CheckMonoliths runs early in the frame, before most world drawing.
            On.Terraria.Main.CheckMonoliths += CheckTargets;
            On.Terraria.Main.SetDisplayMode += OnSetDisplayMode;
        }

        public void Unload()
        {
            On.Terraria.Main.CheckMonoliths -= CheckTargets;
            On.Terraria.Main.SetDisplayMode -= OnSetDisplayMode;

            // Dispose all tracked targets.
            for (int i = tracked.Count - 1; i >= 0; i--)
            {
                tracked[i]?.Dispose();
            }
            tracked.Clear();
        }

        internal static void Register(ManagedRenderTarget target)
        {
            if (target != null && !tracked.Contains(target))
                tracked.Add(target);
        }

        internal static void Unregister(ManagedRenderTarget target)
        {
            tracked.Remove(target);
        }

        private static void OnSetDisplayMode(On.Terraria.Main.orig_SetDisplayMode orig, int width, int height, bool fullscreen)
        {
            orig(width, height, fullscreen);
            // Force recreate after resolution change.
            for (int i = 0; i < tracked.Count; i++)
                tracked[i]?.CheckScreenResize();
            previousScreenSize = new Vector2(Main.screenWidth, Main.screenHeight);
        }

        private static void CheckTargets(On.Terraria.Main.orig_CheckMonoliths orig)
        {
            Vector2 current = new Vector2(Main.screenWidth, Main.screenHeight);
            if (current != previousScreenSize)
            {
                for (int i = 0; i < tracked.Count; i++)
                    tracked[i]?.CheckScreenResize();
                previousScreenSize = current;
            }

            orig();
        }
    }
}
