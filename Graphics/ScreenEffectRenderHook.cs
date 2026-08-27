using Microsoft.Xna.Framework.Graphics;
using InfernumMode.ILEditingStuff;
using Terraria;
using Terraria.Graphics.Effects;

namespace InfernumMode.Graphics
{
    public class ScreenEffectRenderHook : IHookEdit
    {
        public void Load()
        {
            On.Terraria.Graphics.Effects.FilterManager.EndCapture += DrawScreenEffectsAfterCapture;
        }

        public void Unload()
        {
            On.Terraria.Graphics.Effects.FilterManager.EndCapture -= DrawScreenEffectsAfterCapture;
        }

        private static void DrawScreenEffectsAfterCapture(On.Terraria.Graphics.Effects.FilterManager.orig_EndCapture orig, FilterManager self)
        {
            // Let vanilla composite every active filter and blit the result to the backbuffer first.
            orig(self);

            if (self != Filters.Scene || !ScreenEffectSystem.AnyBlurOrFlashActive())
                return;

            if (Main.gameMenu || Main.dedServ || Main.mapFullscreen)
                return;

            // Mirrors the gravDir check vanilla EndCapture uses internally to decide which of
            // the two ping-pong buffers holds the final composited frame.
            RenderTarget2D source = Main.player[Main.myPlayer].gravDir == -1f ? Main.screenTargetSwap : Main.screenTarget;

            ScreenEffectSystem.DrawBlurEffect(source);
        }
    }
}
