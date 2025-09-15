using CalamityMod;
using InfernumMode.BehaviorOverrides.BossAIs.Cultist;
using Terraria;
using Terraria.Audio;
using Terraria.ID;

using TMLSoundType = Terraria.ModLoader.SoundType;
namespace InfernumMode.BossIntroScreens
{
    public class CultistIntroScreen : BaseIntroScreen
    {
        public override TextColorData TextColor => CalamityUtils.MulticolorLerp(CalamityUtils.Convert01To010(AnimationCompletion), CultistBehaviorOverride.PillarsPallete);

        public override bool TextShouldBeCentered => true;

        public override bool ShouldCoverScreen => false;

        public override string TextToDisplay => "Ancient Doomsayer\nThe Lunatic Cultist";

        public override bool ShouldBeActive() => NPC.AnyNPCs(NPCID.CultistBoss);

        public override LegacySoundStyle SoundToPlayWithTextCreation => null;
    }
}