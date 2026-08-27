﻿using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;

using TMLSoundType = Terraria.ModLoader.SoundType;

namespace InfernumMode.Sounds
{
    public static class InfernumSoundRegistry
    {
        #region Bosses and Enemies - AEW
        public static readonly LegacySoundStyle AEWDeathAnimationSound = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/AEW/AEWDeathAnimation");
        public static readonly LegacySoundStyle AEWEnergyCharge = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/AEW/AEWEnergyCharge");
        public static readonly LegacySoundStyle AEWIceBurst = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/AEW/AEWIceBurst");
        public static readonly LegacySoundStyle AEWThreatenRoar = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/AEW/AEWThreatenRoar");
        public static readonly LegacySoundStyle TerminusLaserbeamSound = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/AEW/TerminusLaserbeam");
        public static readonly LegacySoundStyle TerminusPulseSound = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/AEW/TerminusPulse");
        public static readonly LegacySoundStyle WyrmChargeSound = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/AEW/WyrmElectricCharge");
        #endregion

        #region Bosses and Enemies - Abyss
        public static readonly LegacySoundStyle DevilfishRoarSound = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/Abyss/DevilfishRoar");
        public static readonly LegacySoundStyle EidolistChoirSound = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/Abyss/EidolistChoir");
        public static readonly LegacySoundStyle GulperEelScreamSound = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/Abyss/GulperEelScream");
        public static readonly LegacySoundStyle ReaperSharkIceBreathSound = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/Abyss/ReaperSharkIceBreath");
        #endregion

        #region Bosses and Enemies - Aquatic Scourge
        public static readonly LegacySoundStyle AquaticScourgeAcidHissLoopSound = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/AquaticScourge/AquaticScourgeAcidHissLoop");
        public static readonly LegacySoundStyle AquaticScourgeAppearSound = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/AquaticScourge/AquaticScourgeAppear");
        public static readonly LegacySoundStyle AquaticScourgeChargeSound = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/AquaticScourge/AquaticScourgeCharge");
        public static readonly LegacySoundStyle AquaticScourgeGoreSound = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/AquaticScourge/AquaticScourgeGore");
        public static readonly LegacySoundStyle BubblePop = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/AquaticScourge/BubblePop");
        #endregion

        #region Bosses and Enemies - Astrum Aureus
        public static readonly LegacySoundStyle AstrumAureusStompSound = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/AstrumAureus/AureusStomp");
        public static readonly LegacySoundStyle AstrumAureusLaserSound = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/AstrumAureus/AureusLaser");
        public static readonly LegacySoundStyle AstrumAureusJumpSound = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/AstrumAureus/AureusJump");
        #endregion

        #region Bosses and Enemies - Bereft Vassal
        public static readonly LegacySoundStyle GreatSandSharkChargeRoarSound = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/BereftVassal/GreatSandSharkChargeRoar");
        public static readonly LegacySoundStyle GreatSandSharkMiscRoarSound = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/BereftVassal/GreatSandSharkMiscRoar");
        public static readonly LegacySoundStyle GreatSandSharkHitSound = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.NPCHit, "Sounds/NPCHit/GreatSandSharkHit");
        public static readonly LegacySoundStyle GreatSandSharkSpawnSound = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/BereftVassal/GreatSandSharkSpawnSound");
        public static readonly LegacySoundStyle GreatSandSharkSuddenRoarSound = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/BereftVassal/GreatSandSharkSuddenRoar");
        public static readonly LegacySoundStyle MyrindaelHitSound = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/BereftVassal/MyrindaelHit");
        public static readonly LegacySoundStyle MyrindaelLightningSound = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/BereftVassal/MyrindaelLightning");
        public static readonly LegacySoundStyle MyrindaelSpinSound = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/BereftVassal/MyrindaelSpin");
        public static readonly LegacySoundStyle MyrindaelThrowSound = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/BereftVassal/MyrindaelThrow");
        public static readonly LegacySoundStyle VassalAngerSound = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/BereftVassal/VassalAnger");
        public static readonly LegacySoundStyle VassalHitSound = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.NPCHit, "Sounds/NPCHit/VassalHit");
        public static readonly LegacySoundStyle VassalHornSound = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/BereftVassal/VassalHornSound");
        public static readonly LegacySoundStyle VassalJumpSound = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/BereftVassal/VassalJump");
        public static readonly LegacySoundStyle VassalSlashSound = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/BereftVassal/VassalSlash");
        public static readonly LegacySoundStyle VassalTeleportSound = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/BereftVassal/VassalTeleport");
        public static readonly LegacySoundStyle VassalWaterBeamSound = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/BereftVassal/VassalWaterBeam");
        #endregion

        #region Bosses and Enemies - Brain of Cthulhu
        public static readonly LegacySoundStyle BrainLightningSound = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/BrainOfCthulhu/BrainLightning");
        #endregion

        #region Bosses and Enemies - Brimstone Elemental
        public static readonly LegacySoundStyle BrimstoneLaser = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/BrimstoneElemental/BrimstoneLaser");
        public static readonly LegacySoundStyle BrimstoneElementalShellGroundHit = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/DeathAnimations/BrimstoneElementalShellGroundHit");
        #endregion

        #region Bosses and Enemies - Calamitas Shadow
        public static readonly LegacySoundStyle CalShadowDissipateSound = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/CalShadow/CalamitasShadowDissipate");
        public static readonly LegacySoundStyle CalShadowTeleportSound = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/CalShadow/CalamitasShadowTeleport");
        public static readonly LegacySoundStyle EntropyRayChargeSound = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/CalShadow/EntropyRayCharge");
        public static readonly LegacySoundStyle EntropyRayFireSound = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/CalShadow/EntropyRayFire");
        #endregion

        #region Bosses and Enemies - Ceaseless Void
        public static readonly LegacySoundStyle CeaselessVoidChainSound = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/CeaselessVoid/CeaselessVoidChain");
        public static readonly LegacySoundStyle CeaselessVoidEnergyTorrentSound = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/CeaselessVoid/CeaselessVoidEnergyTorrent");
        public static readonly LegacySoundStyle CeaselessVoidStrikeSound = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/CeaselessVoid/CeaselessVoidStrike");
        public static readonly LegacySoundStyle CeaselessVoidSwirlSound = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/CeaselessVoid/CeaselessVoidSwirl");
        public static readonly LegacySoundStyle CeaselessVoidTeleportSound = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/CeaselessVoid/CeaselessVoidTeleport");
        public static readonly LegacySoundStyle CeaselessVoidMetalBreakSound = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/CeaselessVoid/CeaselessVoidMetalBreak");
        #endregion

        #region Bosses and Enemies - Cryogen
        public static readonly LegacySoundStyle CryogenPhaseTransitionCrack = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/Cryogen/CryogenPhaseTransitionCrack");
        public static readonly LegacySoundStyle CryogenShieldRegenerate = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/Cryogen/CryogenShieldRegenerate");
        #endregion

        #region Bosses and Enemies - Death Animations
        public static readonly LegacySoundStyle KingSlimeDeathAnimation = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/DeathAnimations/KingSlimeDeathAnimation");
        public static readonly LegacySoundStyle PerforatorDeathAnimation = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/DeathAnimations/PerforatorDeathAnimation");
        public static readonly LegacySoundStyle PolterghastDeathEchoSound = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/DeathAnimations/PolterghastDeath");
        #endregion

        #region Bosses and Enemies - Deerclops
        public static readonly LegacySoundStyle DeerclopsRubbleAttackDistortedSound = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/Deerclops/DeerclopsRubbleAttackDistorted");
        #endregion

        #region Bosses and Enemies - Desert Scourge
        public static readonly LegacySoundStyle DesertScourgeSandstormWindSound = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/DesertScourge/DesertScourgeSandstormWind");
        public static readonly LegacySoundStyle DesertScourgeShortRoar = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/DesertScourge/DesertScourgeShortRoar");
        #endregion

        #region Bosses and Enemies - Destroyer
        public static readonly LegacySoundStyle DestroyerBombExplodeSound = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/Destroyer/DestroyerBombExplode");
        public static readonly LegacySoundStyle DestroyerChargeImpactSound = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/Destroyer/DestroyerChargeImpact");
        public static readonly LegacySoundStyle DestroyerChargeUpSound = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/Destroyer/DestroyerChargeUp");
        public static readonly LegacySoundStyle DestroyerLaserTelegraphSound = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/Destroyer/DestroyerLaserTelegraph");
        public static readonly LegacySoundStyle DestroyerProbeReleaseSound = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/Destroyer/ProbeRelease");
        #endregion

        #region Bosses and Enemies - Devourer of Gods
        public static readonly LegacySoundStyle DoGLaughSound = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/DevourerOfGods/DoGLaugh");
        public static readonly LegacySoundStyle DevourerSegmentBreak1 = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/DevourerOfGods/DevourerSegmentBreak1");
        public static readonly LegacySoundStyle DevourerSegmentBreak2 = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/DevourerOfGods/DevourerSegmentBreak2");
        public static readonly LegacySoundStyle DevourerSegmentBreak3 = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/DevourerOfGods/DevourerSegmentBreak3");
        public static readonly LegacySoundStyle DevourerSegmentBreak4 = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/DevourerOfGods/DevourerSegmentBreak4");
        public static readonly LegacySoundStyle DoGAttack = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/DevourerOfGods/DoGAttack");
        
        // Random Devourer Segment Break
        public static LegacySoundStyle DevourerSegmentBreak => _devourerSegmentBreaks[Main.rand.Next(4)];
        #endregion

        #region Bosses and Enemies - ExoMechs
        public static readonly LegacySoundStyle AresCircleLaserStart = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/ExoMechs/AresCircleLaserStart");
		public static readonly LegacySoundStyle AresCircleLaserEnd = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/ExoMechs/AresCircleLaserEnd");
        public static readonly LegacySoundStyle AresLaserArmShoot = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/ExoMechs/AresLaserArmShoot");
        public static readonly LegacySoundStyle AresGaussNukeArmCharge = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/ExoMechs/AresGaussNukeArmCharge");
        public static readonly LegacySoundStyle AresLaughSound = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/ExoMechs/AresLaugh");
        public static readonly LegacySoundStyle AresSlashSound = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/ExoMechs/AresSlash");
        public static readonly LegacySoundStyle AresTeslaShotSound = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/ExoMechs/AresTeslaShot");
        public static readonly LegacySoundStyle AresPulseCannonChargeSound = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/ExoMechs/AresPulseCannonCharge");
        public static readonly LegacySoundStyle ArtemisSpinLaserbeamSound = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/ExoMechs/ArtemisSpinLaserbeam");
        public static readonly LegacySoundStyle ApolloMissileLaunch = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/ExoMechs/ApolloMissileLaunch");
        public static readonly LegacySoundStyle ArtemisApolloDashTelegraph = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/ExoMechs/ArtemisApolloDashTelegraph");
        public static readonly LegacySoundStyle ArtemisApolloDash = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/ExoMechs/ArtemisApolloDash");
        public static readonly LegacySoundStyle ExoMechFinalPhaseSound = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/ExoMechs/ExoMechFinalPhaseChargeup");
        public static readonly LegacySoundStyle ExoMechImpendingDeathSound = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/ExoMechs/ExoMechImpendingDeathSound");
        public static readonly LegacySoundStyle ExoMechIntroSound = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/ExoMechs/ExoMechIntro");
        public static readonly LegacySoundStyle ExoLaserShootSound = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/ExoMechs/ExoLaserShoot");
        public static readonly LegacySoundStyle ExoPlasmaExplosion1 = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/ExoMechs/ExoPlasmaExplosion1");
        public static readonly LegacySoundStyle ExoPlasmaExplosion2 = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/ExoMechs/ExoPlasmaExplosion2");
        public static readonly LegacySoundStyle ExoHit1 = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/ExoMechs/ExoHit1");
        public static readonly LegacySoundStyle ExoHit2 = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/ExoMechs/ExoHit2");
        public static readonly LegacySoundStyle ExoHit3 = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/ExoMechs/ExoHit3");
        public static readonly LegacySoundStyle ExoHit4 = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/ExoMechs/ExoHit4");
        public static readonly LegacySoundStyle ThanatosLightRay = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/ExoMechs/ThanatosLightRay");
        public static readonly LegacySoundStyle ThanatosTransitionSound = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/ExoMechs/ThanatosTransition");
        
        // Random Exo Sounds
        public static LegacySoundStyle ExoPlasmaExplosion => _exoPlasmaExplosion[Main.rand.Next(2)];
        public static LegacySoundStyle ExoHit => _exoHit[Main.rand.Next(4)];
        #endregion

        #region Bosses and Enemies - Golem
        public static readonly LegacySoundStyle GolemGroundHitSound = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/Golem/GolemGroundHit");
        public static readonly LegacySoundStyle GolemSansSound = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/Golem/BadTime");
        public static readonly LegacySoundStyle GolemSpamtonSound = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/Golem/[BIG SHOT]");
        #endregion

        #region Bosses and Enemies - Leviathan
        public static readonly LegacySoundStyle AnahitaSingSound = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/Leviathan/AnahitaSing");
        public static readonly LegacySoundStyle LeviathanRumbleSound = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/Leviathan/LeviathanRumble");
        #endregion

        #region Bosses and Enemies - Misc
        public static readonly LegacySoundStyle BirbCrySound = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/Misc/BirbCry");
        public static readonly LegacySoundStyle CloudElementalWindSound = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/Misc/CloudElementalWind");
        public static readonly LegacySoundStyle HatGirlPeckVASound = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/Misc/HatGirlPeck");
        public static readonly LegacySoundStyle SonicBoomSound = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/Misc/SonicBoom");
        #endregion

        #region Bosses and Enemies - Moon Lord
        public static readonly LegacySoundStyle MoonLordIntroSound = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/MoonLord/MoonLordIntro");
        #endregion

        #region Bosses and Enemies - Nuclear Terror
        public static readonly LegacySoundStyle NuclearTerrorGroundSlamSound = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/NuclearTerror/NuclearTerrorGroundSlam");
        public static readonly LegacySoundStyle NuclearTerrorJumpSound = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/NuclearTerror/NuclearTerrorJump");
        public static readonly LegacySoundStyle NuclearTerrorTeleportSound = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/NuclearTerror/NuclearTerrorTeleport");
        #endregion

        #region Bosses and Enemies - Plaguebringer Goliath
        public static readonly LegacySoundStyle PBGMissileLaunchSound = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/PlaguebringerGoliath/PBGMissileLaunch");
        public static readonly LegacySoundStyle PBGNukeExplosionSound = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/PlaguebringerGoliath/PBGNukeExplosion");
        #endregion

        #region Bosses and Enemies - Polterghast
        public static readonly LegacySoundStyle PolterghastDashSound = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/Polterghast/PolterDash");
        public static readonly LegacySoundStyle PolterghastShortDashSound = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/Polterghast/PolterDashShort");
        public static readonly LegacySoundStyle PolterghastSoulSound = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/Polterghast/PolterSoulVortexShoot");
        public static readonly LegacySoundStyle PolterSoulVortexShootSound = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/Polterghast/PolterSoulShoot");
        public static readonly LegacySoundStyle PolterghastP2Transition = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/Polterghast/PolterghastP2Transition");
        public static readonly LegacySoundStyle PolterghastP3Transition = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/Polterghast/PolterghastP3Transition");
        #endregion

        #region Bosses and Enemies - Profaned Guardians
        public static readonly LegacySoundStyle GuardiansPhaseThreeTransition = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/ProfanedGuardians/GuardiansPhaseThreeTransition");
        public static readonly LegacySoundStyle GuardiansPhaseTwoTransition = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/ProfanedGuardians/GuardiansPhaseTwoTransition");
        public static readonly LegacySoundStyle GuardianSpawnSound = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/ProfanedGuardians/GuardiansSpawn");
        public static readonly LegacySoundStyle GuardianRockShieldDeactivate = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/ProfanedGuardians/GuardianShieldDeactivate");
        public static readonly LegacySoundStyle GuardianRockShieldActivate = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/ProfanedGuardians/GuardianRockShieldActivate");
        #endregion

        #region Bosses and Enemies - Providence
        public static readonly LegacySoundStyle ProvidenceBlenderSound = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/Providence/ProvidenceBlender");
        public static readonly LegacySoundStyle ProvidenceBurnSound = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/Providence/ProvidenceBurn");
        public static readonly LegacySoundStyle ProvidenceCrystalPillarShatterSound = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/Providence/ProvidenceCrystalPillarShatter");
        public static readonly LegacySoundStyle ProvidenceDogmaBeamFire = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/Providence/DogmaLasersFire");
        public static readonly LegacySoundStyle ProvidenceDoorShimmerSoundLoop = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/Providence/ProvidenceDoorSoundLoop");
        public static readonly LegacySoundStyle ProvidenceDoorShatterSound = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/Providence/ProvidenceDoorShatter");
        public static readonly LegacySoundStyle ProvidenceLavaEruptionSound = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/Providence/ProvidenceLavaEruption");
        public static readonly LegacySoundStyle ProvidenceLavaEruptionSmallSound = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/Providence/ProvidenceLavaEruptionSmall");
        public static readonly LegacySoundStyle ProvidenceScreamSound = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/Providence/ProvidenceScream");
        public static readonly LegacySoundStyle ProvidenceSpawnSuspenseSound = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/Providence/ProvidenceSpawnSuspense");
        public static readonly LegacySoundStyle ProvidenceSpearHitSound = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/Providence/ProvidenceSpearHit");
        public static readonly LegacySoundStyle SizzleSound = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/Providence/ProvidenceSizzle");
        #endregion

        #region Bosses and Enemies - Queen Slime
        public static readonly LegacySoundStyle QueenSlimeExplosionSound = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/QueenSlime/QueenSlimeExplosion");
        #endregion

        #region Bosses and Enemies - Ravager
        public static readonly LegacySoundStyle RavagerFlamePillarEruptSound = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/Ravager/RavagerFlamePillarErupt");
		public static readonly LegacySoundStyle RavagerPunch1 = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/Ravager/RavagerPunch1");
		public static readonly LegacySoundStyle RavagerPunch2 = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/Ravager/RavagerPunch2");
		
		public static LegacySoundStyle RavagerPunch => _ravagerPunch[Main.rand.Next(2)];
        #endregion

        #region Bosses and Enemies - Signus
        public static readonly LegacySoundStyle SignusChargeSound = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/Signus/SignusCharge");
        public static readonly LegacySoundStyle SignusFlameBombShootSound = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/Signus/SignusFlameBombShoot");
        public static readonly LegacySoundStyle SignusKunaiExplosionSound = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/Signus/SignusKunaiExplosion");
        public static readonly LegacySoundStyle SignusSlashSound = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/Signus/SignusSlash");
        public static readonly LegacySoundStyle SignusWeaponFireSound = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/Signus/SignusWeaponFire");
        #endregion

        #region Bosses and Enemies - Skeletron
        public static readonly LegacySoundStyle DarkMagicSkullShootDamage = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/Skeletron/DarkMagicSkullShoot");
        public static readonly LegacySoundStyle SkeletronHeadBonkSound = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/Skeletron/SkeletronHeadBonk");
        #endregion

        #region Bosses and Enemies - Skeletron Prime
        public static readonly LegacySoundStyle PrimeChargeSound = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/SkeletronPrime/PrimeCharge");
        public static readonly LegacySoundStyle PrimeSawSound = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/SkeletronPrime/PrimeSaw");
        #endregion

        #region Bosses and Enemies - Storm Weaver
        public static readonly LegacySoundStyle StormWeaverWindSound = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/StormWeaver/StormWeaverWind");
        public static readonly LegacySoundStyle StormWeaverElectricDischargeSound = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/StormWeaver/ElectricDischarge");
        #endregion

        #region Bosses and Enemies - Supreme Calamitas
        public static readonly LegacySoundStyle CalamitousEnergyBurstSound = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/SupremeCalamitas/CalamitousEnergyBurst");
        public static readonly LegacySoundStyle SCalBrothersSpawnSound = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/SupremeCalamitas/SCalBrothersSpawn");
        public static readonly LegacySoundStyle ShadowHydraCharge = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/SupremeCalamitas/HydraCharge");
        public static readonly LegacySoundStyle ShadowHydraSpawn = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/SupremeCalamitas/HydraSpawn");
        #endregion

        #region Bosses and Enemies - Twins
        public static readonly LegacySoundStyle TwinsForcefieldExplosionSound = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/Twins/TwinsForcefieldExplosion");
        #endregion

        #region Items
        public static readonly LegacySoundStyle GlassmakerFireStartSound = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Item, "Sounds/Item/GlassmakerIntro");
        public static readonly LegacySoundStyle GlassmakerFireSound = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Item, "Sounds/Item/GlassmakerFire");
        public static readonly LegacySoundStyle GlassmakerFireEndSound = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Item, "Sounds/Item/GlassmakerOutro");
        public static readonly LegacySoundStyle HalibutSpotlight = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Item, "Sounds/Item/HalibutSpotlight");
        public static readonly LegacySoundStyle HyperplaneMatrixActivateSound = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Item, "Sounds/Item/HyperplaneMatrixActivate");
        public static readonly LegacySoundStyle KevinElectricitySound = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Item, "Sounds/Item/KevinElectricity");
        public static readonly LegacySoundStyle WayfinderCreateSound = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Item, "Sounds/Item/WayfinderCreate");
        public static readonly LegacySoundStyle WayfinderDestroySound = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Item, "Sounds/Item/WayfinderDestroy");
        public static readonly LegacySoundStyle WayfinderFail = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Item, "Sounds/Item/WayfinderFail");
        public static readonly LegacySoundStyle WayfinderTeleport = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Item, "Sounds/Item/WayfinderTeleport");
        public static readonly LegacySoundStyle AsterBarkSound = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Item, "Sounds/Item/AsterBark");
        #endregion
		
		public static readonly LegacySoundStyle YharonInfernado = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/Yharon/YharonInfernado");
		public static readonly LegacySoundStyle YharonDeath = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/Yharon/YharonDeath");
        public static readonly LegacySoundStyle YharonFireOrb = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/Yharon/YharonFireOrb");

        #region Miscellaneous
        public static readonly LegacySoundStyle InfernumAchievementCompletionSound = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/Misc/InfernumAchievementComplete");
        public static readonly LegacySoundStyle ModeToggleLaugh = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/Misc/ModeToggleLaugh");
        public static readonly LegacySoundStyle RainLoop = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/MainMenu/RainLoop");
        public static readonly LegacySoundStyle ThunderRumble = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/MainMenu/ThunderRumble");
        public static readonly LegacySoundStyle ThunderRumble2 = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/MainMenu/ThunderRumble2");
        public static readonly LegacySoundStyle ThunderRumble3 = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/MainMenu/ThunderRumble3");
        public static readonly LegacySoundStyle WayfinderGateLoop = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/Wayfinder/WayfinderGateLoop");
        public static readonly LegacySoundStyle WayfinderObtainSound = InfernumMode.Instance.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/Wayfinder/WayfinderObtainSound");
        #endregion

        #region Calamity Mod Sounds (from CalamityMod)
        public static readonly LegacySoundStyle CalThunderStrikeSound = InfernumMode.CalamityMod.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/ThunderStrike");
        public static readonly LegacySoundStyle PBGMechanicalWarning = InfernumMode.CalamityMod.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/PlagueSounds/PBGNukeWarning");
        public static readonly LegacySoundStyle ProvidenceHolyBlastShootSound = InfernumMode.CalamityMod.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/ProvidenceHolyBlastShoot");
        public static readonly LegacySoundStyle ProvidenceHolyRaySound = InfernumMode.CalamityMod.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/ProvidenceHolyRay");
        #endregion
        #region Random Sound Arrays
        private static readonly LegacySoundStyle[] _devourerSegmentBreaks = new LegacySoundStyle[]
        {
            DevourerSegmentBreak1,
            DevourerSegmentBreak2,
            DevourerSegmentBreak3,
            DevourerSegmentBreak4
        };

        private static readonly LegacySoundStyle[] _exoPlasmaExplosion = new LegacySoundStyle[]
        {
            ExoPlasmaExplosion1,
            ExoPlasmaExplosion2
        };

        private static readonly LegacySoundStyle[] _exoHit = new LegacySoundStyle[]
        {
            ExoHit1,
            ExoHit2,
            ExoHit3,
            ExoHit4
        };
		
        private static readonly LegacySoundStyle[] _ravagerPunch = new LegacySoundStyle[]
        {
            RavagerPunch1,
            RavagerPunch2
        };		
        #endregion
    }
}