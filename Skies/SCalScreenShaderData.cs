﻿using CalamityMod.NPCs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;

namespace InfernumMode.Skies
{
    public class SCalScreenShaderData : ScreenShaderData
    {
        public SCalScreenShaderData(Ref<Effect> shader, string passName) : base(shader, passName) { }

        public override void Apply()
        {
            if (CalamityGlobalNPC.SCal < 0)
                return;

            UseTargetPosition(Main.LocalPlayer.Center);
            UseColor(new Color(100, 150, 255));

            // Perform various matrix calculations to transform SCal's arena to UV coordinate space.
            NPC scal = Main.npc[CalamityGlobalNPC.SCal];
            Rectangle arena = scal.Infernum().Arena;
            Vector4 uvScaledArena = new Vector4(arena.X, arena.Y - 6f, arena.Width + 8f, arena.Height + 14f);
            uvScaledArena.X -= Main.screenPosition.X;
            uvScaledArena.Y -= Main.screenPosition.Y;
            Vector2 downscaleFactor = new Vector2(Main.screenWidth, Main.screenHeight);
            Matrix toScreenCoordsTransformation = Main.GameViewMatrix.TransformationMatrix;
            Vector2 coordinatePart = Vector2.Transform(new Vector2(uvScaledArena.X, uvScaledArena.Y), toScreenCoordsTransformation) / downscaleFactor;
            Vector2 areaPart = Vector2.Transform(new Vector2(uvScaledArena.Z, uvScaledArena.W), toScreenCoordsTransformation) / downscaleFactor;
            uvScaledArena = new Vector4(coordinatePart.X, coordinatePart.Y, areaPart.X, areaPart.Y);

            Shader.Parameters["uvArenaArea"].SetValue(uvScaledArena);
            UseImage(ModContent.GetTexture("InfernumMode/ExtraTextures/CultistRayMap"), 0, SamplerState.AnisotropicWrap);

            base.UseOpacity(0.36f);
            base.Apply();
        }
    }
}
