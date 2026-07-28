using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;

using TMLSoundType = Terraria.ModLoader.SoundType;
namespace InfernumMode.Sounds
{
    public static class InfernumSoundRegistry
    {
        public static readonly LegacySoundStyle AresLaughSound = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/AresLaugh");

        public static readonly LegacySoundStyle AresTeslaShotSound = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/AresTeslaShot");

        public static readonly LegacySoundStyle CalThunderStrikeSound = InfernumMode.CalamityMod.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/ThunderStrike");

        public static readonly LegacySoundStyle DoGLaughSound = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/DoGLaugh");

        public static readonly LegacySoundStyle ExoMechFinalPhaseSound = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/ExoMechFinalPhaseChargeup");

        public static readonly LegacySoundStyle ExoMechImpendingDeathSound = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/ExoMechImpendingDeathSound");

        public static readonly LegacySoundStyle ExoMechIntroSound = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/ExoMechIntro");

        public static readonly LegacySoundStyle GolemSansSound = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/BadTime");

        public static readonly LegacySoundStyle GolemSpamtonSound = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/[BIG SHOT]");

        public static readonly LegacySoundStyle HeavyExplosionSound = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/HeavyExplosion");

        public static readonly LegacySoundStyle LeviathanRumbleSound = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/LeviathanSummonBase");

        public static readonly LegacySoundStyle MoonLordIntroSound = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/MoonLordIntro");

        public static readonly LegacySoundStyle PBGMechanicalWarning = InfernumMode.CalamityMod.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/PlagueSounds/PBGNukeWarning");

        public static readonly LegacySoundStyle PoltergastDeathEcho = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/PolterghastDeath");

        public static readonly LegacySoundStyle ProvidenceHolyBlastShootSound = InfernumMode.CalamityMod.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/ProvidenceHolyBlastShoot");

        public static readonly LegacySoundStyle ProvidenceHolyRaySound = InfernumMode.CalamityMod.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/ProvidenceHolyRay");

        public static readonly LegacySoundStyle ProvidenceDoorShimmerSoundLoop = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/ProvidenceDoorSoundLoop");

        public static readonly LegacySoundStyle ProvidenceDoorShatterSound = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/ProvidenceDoorShatter");

        public static readonly LegacySoundStyle ThanatosTransitionSound = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/ThanatosTransition");

        public static readonly LegacySoundStyle WyrmChargeSound = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/WyrmElectricCharge");
		
		public static readonly LegacySoundStyle VassalJumpSound = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/VassalJump");
		
        public static readonly LegacySoundStyle EntropyRayChargeSound = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/EntropyRayCharge");		
		
		public static readonly LegacySoundStyle CalShadowTeleportSound = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/CalamitasShadowTeleport");
		
		public static readonly LegacySoundStyle ProvidenceLavaEruptionSmallSound = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/ProvidenceLavaEruptionSmall");
		
		public static readonly LegacySoundStyle ProvidenceLavaEruptionSound  = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/ProvidenceLavaEruption");
		
		public static readonly LegacySoundStyle GlassmakerFireEndSound  = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Item, "Sounds/Item/GlassmakerOutro");
		
		public static readonly LegacySoundStyle EntropyRayFireSound = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/EntropyRayFire");
		
		public static readonly LegacySoundStyle SCalBrothersSpawnSound = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/SCalBrothersSpawn");
		
        public static readonly LegacySoundStyle SonicBoomSound = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/SonicBoom");
    }                                                          
}