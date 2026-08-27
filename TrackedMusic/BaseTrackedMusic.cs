using System;
using System.Collections.Generic;

namespace InfernumMode.TrackedMusic
{
    public abstract class BaseTrackedMusic
    {
        public abstract string MusicPath { get; }

        public abstract float BeatsPerMinute { get; }

        public abstract BPMHeadBobState HeadBobState { get; }

        public abstract List<SongSection> HeadphonesHighPoints { get; }

        public abstract List<SongSection> HighPoints { get; }

        public virtual Dictionary<SongSection, int> SongSections
        {
            get { return null; }
        }

        internal void Load()
        {
            if (!TrackedMusicManager.CustomTrackPaths.Contains(MusicPath))
                TrackedMusicManager.CustomTrackPaths.Add(MusicPath);

            TrackedMusicManager.TrackInformation[MusicPath] = this;
        }

        public static TimeSpan TimeFormat(int minutes, int seconds, int milliseconds)
        {
            return new TimeSpan(0, 0, minutes, seconds, milliseconds);
        }

        public static SongSection WithMSDelay(int minutes, int seconds, int milliseconds, int delayInMilliseconds)
        {
            TimeSpan start = new TimeSpan(0, 0, minutes, seconds, milliseconds);
            TimeSpan end = start.Add(new TimeSpan(0, 0, 0, 0, delayInMilliseconds));
            return new SongSection(start, end);
        }
    }
}
