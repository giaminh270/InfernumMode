using CalamityMod.NPCs.Signus;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;

using TMLSoundType = Terraria.ModLoader.SoundType;
namespace InfernumMode.BossIntroScreens
{
    public class SignusIntroScreen : BaseIntroScreen
    {
        public override TextColorData TextColor => new TextColorData(completionRatio =>
        {
            return Color.Lerp(Color.Violet, Color.Black, 0.67f);
        });

        public override bool TextShouldBeCentered => true;

        public override bool ShouldCoverScreen => false;

        public override string TextToDisplay => "The Fathomless Assassin\nSignus";

        public override bool ShouldBeActive() => NPC.AnyNPCs(ModContent.NPCType<Signus>());

        public override LegacySoundStyle SoundToPlayWithTextCreation => null;
    }
}