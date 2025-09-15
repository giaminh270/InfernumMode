using CalamityMod.NPCs.StormWeaver;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;

using TMLSoundType = Terraria.ModLoader.SoundType;
namespace InfernumMode.BossIntroScreens
{
    public class StormWeaverIntroScreen : BaseIntroScreen
    {
        public override TextColorData TextColor => new TextColorData(completionRatio =>
        {
            float lightningInterpolant = Utils.InverseLerp(0.77f, 1f, (float)Math.Sin(AnimationCompletion * -MathHelper.Pi * 4f + completionRatio * MathHelper.Pi * 4f) * 0.5f + 0.5f);
            Color skinColor = new Color(219, 103, 151);
            Color lightningColor = new Color(190, 233, 215);
            return Color.Lerp(skinColor, lightningColor, lightningInterpolant);
        });

        public override bool TextShouldBeCentered => true;

        public override bool ShouldCoverScreen => false;

        public override string TextToDisplay => "Atmospheric Predator\nThe Storm Weaver";

        public override bool ShouldBeActive() => NPC.AnyNPCs(ModContent.NPCType<StormWeaverHead>());

        public override LegacySoundStyle SoundToPlayWithTextCreation => null;
    }
}