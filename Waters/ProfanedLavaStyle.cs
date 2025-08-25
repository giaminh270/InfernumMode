﻿using CalamityMod.Waters;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace InfernumMode.Waters
{
    public class ProfanedLavaflow : ModWaterfallStyle { }

    public class ProfanedLavaStyle : CustomLavaStyle
    {
        public override string LavaTexturePath => "InfernumMode/Waters/ProfanedLava";

        public override string BlockTexturePath => LavaTexturePath + "_Block";

        public override bool ChooseLavaStyle() => Main.LocalPlayer.Infernum().ZoneProfaned || Main.LocalPlayer.Infernum().ProfanedLavaFountain;

        public override int ChooseWaterfallStyle() => InfernumMode.CalamityMod.GetWaterfallStyleSlot("InfernumMode/ProfanedLavaflow");

        public override int GetSplashDust() => 0;

        public override int GetDropletGore() => 0;

        public override void SelectLightColor(ref Color initialLightColor)
        {
            initialLightColor = Color.White;
        }
    }
}
