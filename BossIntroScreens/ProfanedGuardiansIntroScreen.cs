using CalamityMod.NPCs.ProfanedGuardians;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;


using TMLSoundType = Terraria.ModLoader.SoundType;
namespace InfernumMode.BossIntroScreens
{
    public class ProfanedGuardiansIntroScreen : BaseIntroScreen
    {
        public override TextColorData TextColor => Color.Lerp(Color.Orange, Color.Yellow, 0.65f);

        public override bool TextShouldBeCentered => true;

        public override bool ShouldCoverScreen => false;
        
        public override int AnimationTime => 180;

        public override string TextToDisplay => "Disciples of Purity\nThe Profaned Guardians";
        
        public override bool ShouldBeActive() => NPC.AnyNPCs(ModContent.NPCType<ProfanedGuardianBoss>());

		public override LegacySoundStyle SoundToPlayWithTextCreation => InfernumMode.CalamityMod.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/ProvidenceSpawn");
    }
}