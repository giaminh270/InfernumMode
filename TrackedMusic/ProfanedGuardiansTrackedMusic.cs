using System.Collections.Generic;

namespace InfernumMode.TrackedMusic
{
    public class ProfanedGuardiansTrackedMusic : BaseTrackedMusic
    {
        public override string MusicPath => "CalamityModMusic/Sounds/Music/Guardians";

        public override BPMHeadBobState HeadBobState => BPMHeadBobState.Half;

        public override float BeatsPerMinute => BeatsPerMinuteStatic;

        public override List<SongSection> HeadphonesHighPoints => new List<SongSection>();

        public override List<SongSection> HighPoints => new List<SongSection>();

        public static float BeatsPerMinuteStatic => ProvidenceTrackedMusic.BeatsPerMinuteStatic;
    }
}
