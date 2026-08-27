using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Audio;

namespace InfernumMode.TrackedMusic
{
    /// <summary>
    /// Backported TrackedMusic system for tModLoader 0.11.x / Terraria 1.3.5.3.
    /// 
    /// Original 1.4 system relies on MediaPlayer.PlayPosition for precise timing.
    /// This version keeps MediaPlayer when available (FNA), and falls back to a
    /// Stopwatch-based position tracker so the rest of Infernum (head-bob, high points,
    /// song sections) continues to work.
    /// </summary>
    public static class TrackedMusicManager
    {
        internal static bool PausedBecauseOfUI;

        // Music slot indices that we take complete control of (bypass vanilla/tML music system).
        internal static List<int> TracksThatDontUseTerrariasSystem = new List<int>();

        // slot -> Song (Media) instance
        internal static Dictionary<int, Song> CustomTracks = new Dictionary<int, Song>();

        // slot -> absolute path on disk of the extracted .ogg
        internal static Dictionary<int, string> CustomTrackDiskPositions = new Dictionary<int, string>();

        // slot -> original music path string (for name matching)
        internal static Dictionary<int, string> CustomTrackNames = new Dictionary<int, string>();

        internal static ConstructorInfo SongConstructor;

        // Whether the current runtime actually has a working MediaPlayer/Song.
        internal static bool MediaPlayerAvailable;

        internal static bool UseMediaPlayerPlayback = false;
        // Fallback timing (used when MediaPlayer is missing or fails).
        internal static Stopwatch FallbackStopwatch = new Stopwatch();
        internal static TimeSpan FallbackPausedAccumulated = TimeSpan.Zero;
        internal static bool FallbackIsPaused;

        public static readonly List<string> CustomTrackPaths = new List<string>
        {
            // Grief.
            "CalamityModMusic/Sounds/Music/CalamitasPhase1",
            "CalamityModMusic/Sounds/Music/CalamitasPhase1_FullIntro",

            // Lament.
            "CalamityModMusic/Sounds/Music/CalamitasPhase2",

            // Epiphany.
            "CalamityModMusic/Sounds/Music/CalamitasPhase3",

            // Acceptance.
            "CalamityModMusic/Sounds/Music/CalamitasDefeat",
        };

        public static readonly Dictionary<string, BaseTrackedMusic> TrackInformation = new Dictionary<string, BaseTrackedMusic>();

        // Song instances are expected to be read directly from the disk, not memory.
        public static readonly string MusicDirectory = Path.Combine(Main.SavePath, "TrackedMusic");

        public delegate bool PauseInUIConditionDelegate();
        public static event PauseInUIConditionDelegate PauseInUIConditionEvent;

        public static Song TrackedSong { get; internal set; }

        /// <summary>
        /// Current playback position of the tracked song.
        /// Uses MediaPlayer.PlayPosition when available, otherwise a Stopwatch fallback.
        /// </summary>
        public static TimeSpan SongElapsedTime
        {
            get
            {
                if (Main.netMode == NetmodeID.Server || TrackedSong == null)
                    return TimeSpan.Zero;

                if (FallbackIsPaused)
                    return FallbackPausedAccumulated;

                return FallbackPausedAccumulated + FallbackStopwatch.Elapsed;
            }
        }

        /// <summary>
        /// Call this from InfernumMode.Mod.PostSetupContent() (or equivalent).
        /// Must run after all mods have loaded so that CalamityModMusic / InfernumModeMusic tracks exist.
        /// </summary>
        public static void Initialize()
        {
            TracksThatDontUseTerrariasSystem = new List<int>();
            CustomTracks.Clear();
            CustomTrackDiskPositions.Clear();
            CustomTrackNames.Clear();

            // Detect whether MediaPlayer / Song are usable.
            MediaPlayerAvailable = DetectMediaPlayerSupport();

            if (MediaPlayerAvailable)
            {
                try
                {
                    // Song constructor is internal in XNA/FNA.
                    SongConstructor = typeof(Song).GetConstructor(
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                        null,
                        new Type[] { typeof(string), typeof(string) },
                        null);
                }
                catch
                {
                    MediaPlayerAvailable = false;
                }
            }

            // Load every BaseTrackedMusic subclass present in the Infernum assembly.
            Type baseType = typeof(BaseTrackedMusic);
            Assembly infernumAssembly = typeof(TrackedMusicManager).Assembly;

            foreach (Type musicType in infernumAssembly.GetTypes())
            {
                if (musicType.IsAbstract || !baseType.IsAssignableFrom(musicType))
                    continue;

                try
                {
                    BaseTrackedMusic instance = (BaseTrackedMusic)Activator.CreateInstance(musicType);
                    instance.Load();
                }
                catch (Exception e)
                {
                    Console.WriteLine("[TrackedMusic] Failed to load " + musicType.Name + ": " + e);
                }
            }

            // Extract and register the custom tracks that need precise timing.
            foreach (string path in CustomTrackPaths)
            {
                if (Main.netMode == NetmodeID.Server)
                    break;

                try
                {
                    RegisterCustomTrack(path);
                }
                catch (Exception e)
                {
                    Console.WriteLine("[TrackedMusic] Failed to register track " + path + ": " + e);
                }
            }
        }

        private static bool DetectMediaPlayerSupport()
        {
            try
            {
                // Touching the type is enough to know whether the assembly contains it.
                Type songType = typeof(Song);
                Type mediaPlayerType = typeof(MediaPlayer);
                return songType != null && mediaPlayerType != null;
            }
            catch
            {
                return false;
            }
        }

        private static void RegisterCustomTrack(string path)
        {
            // path example: "CalamityModMusic/Sounds/Music/CalamitasPhase1"
            string[] parts = path.Split('/');
            if (parts.Length < 2)
                return;

            string modName = parts[0];
            // Rebuild the relative file name inside the .tmod (everything after mod name + ".ogg")
            string relativeInsideMod = string.Join("/", parts.Skip(1)) + ".ogg";

            Mod mod = ModLoader.GetMod(modName);
            if (mod == null)
                return;

            // Obtain the music slot that tML assigned to this path.
            // In 0.11 the public API is GetSoundSlot(SoundType.Music, pathWithoutModPrefix)
            int musicSlotIndex = -1;
            try
            {
                // Most Calamity / Infernum tracks are registered as "Sounds/Music/XXX"
                string soundPath = string.Join("/", parts.Skip(1));
                musicSlotIndex = mod.GetSoundSlot(SoundType.Music, soundPath);
            }
            catch
            {
                // Some older registrations used the full path; try that as fallback.
                try
                {
                    musicSlotIndex = mod.GetSoundSlot(SoundType.Music, path);
                }
                catch
                {
                    return;
                }
            }

            if (musicSlotIndex < 0)
                return;

            string musicPathOnDisk = Path.Combine(MusicDirectory, path.Replace('/', Path.DirectorySeparatorChar) + ".ogg");
            CustomTrackDiskPositions[musicSlotIndex] = musicPathOnDisk;
            CustomTrackNames[musicSlotIndex] = path;

            // Extract the ogg from the .tmod onto disk if it is not already present.
            if (!File.Exists(musicPathOnDisk) || new FileInfo(musicPathOnDisk).Length == 0)
            {
                string dir = Path.GetDirectoryName(musicPathOnDisk);
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                if (!mod.FileExists(relativeInsideMod))
                    return;

                using (Stream musicStream = mod.GetFileStream(relativeInsideMod))
                using (FileStream saveStream = File.Create(musicPathOnDisk))
                {
                    musicStream.CopyTo(saveStream);
                }
            }

            TracksThatDontUseTerrariasSystem.Add(musicSlotIndex);

            if (MediaPlayerAvailable && SongConstructor != null)
            {
                try
                {
                    // Song(string name, string filename) – name is used for equality checks later.
                    Song song = (Song)SongConstructor.Invoke(new object[] { path, musicPathOnDisk });
                    CustomTracks[musicSlotIndex] = song;
                }
                catch (Exception e)
                {
                    Console.WriteLine("[TrackedMusic] Song construction failed for " + path + ": " + e);
                    MediaPlayerAvailable = false;
                }
            }
        }

        /// <summary>
        /// Call from a PreUpdate / UpdateUI / MidUpdateTimeWorld style hook every frame.
        /// Handles volume, pause-in-UI, starting/stopping the MediaPlayer (or fallback timer).
        /// </summary>
        public static void Update()
        {
            if (Main.netMode == NetmodeID.Server)
                return;

            float volume = 0f;
            int activeSlot = -1;

            if (TrackedSong != null)
            {
                // Find which slot currently owns the tracked song.
                activeSlot = CustomTracks
                    .Where(kv => kv.Value != null && kv.Value.Name == TrackedSong.Name)
                    .Select(kv => kv.Key)
                    .FirstOrDefault();

                if (activeSlot >= 0 && activeSlot < Main.musicFade.Length)
                    volume = Main.musicFade[activeSlot];

                // Pause-in-UI support.
                bool pauseInUI = false;
                if (PauseInUIConditionEvent != null)
                {
                    foreach (Delegate d in PauseInUIConditionEvent.GetInvocationList())
                    {
                        if (((PauseInUIConditionDelegate)d).Invoke())
                            pauseInUI = true;
                    }
                }

                if (pauseInUI && InfernumMode.CanUseCustomAIs)
                {
                    if (!PausedBecauseOfUI && Main.gamePaused)
                    {
                        PausePlayback();
                        PausedBecauseOfUI = true;
                    }
                    else if (PausedBecauseOfUI && !Main.gamePaused)
                    {
                        ResumePlayback();
                        PausedBecauseOfUI = false;
                    }
                }

                if (volume <= 0.0001f && Main.curMusic != activeSlot)
                {
                    StopPlayback();
                    TrackedSong = null;
                    return;
                }
            }

            // Ensure the tracked song is actually playing.
            if (TrackedSong != null)
            {
                string diskPath = null;
                if (activeSlot >= 0 && CustomTrackDiskPositions.ContainsKey(activeSlot))
                    diskPath = CustomTrackDiskPositions[activeSlot];

                if (diskPath == null || !File.Exists(diskPath))
                    return;
                if (!FallbackStopwatch.IsRunning && !FallbackIsPaused)
                    StartFallbackTimer();
            }
        }

        /// <summary>
        /// Call this from the place that decides which music should play
        /// (normally inside a detour of Main.UpdateAudio or a music selection hook).
        /// When the active music index is one of our special tracks we silence the
        /// normal tML/vanilla player and switch to our MediaPlayer / fallback.
        /// </summary>
        public static void OnMusicSlotActive(int slot, ref float fade)
        {
            if (!TracksThatDontUseTerrariasSystem.Contains(slot))
                return;

            if (CustomTracks.ContainsKey(slot))
            {
                Song desired = CustomTracks[slot];
                if (TrackedSong == null || TrackedSong.Name != desired.Name)
                {
                    StopPlayback();
                    TrackedSong = desired;

                    // Start timing from the exact frame Terraria selected the track.
                    StartFallbackTimer();
                }
            }
            else if (CustomTrackNames.ContainsKey(slot))
            {
                if (TrackedSong == null || TrackedSong.Name != CustomTrackNames[slot])
                {
                    StopPlayback();
                    TrackedSong = CreateSentinelSong(CustomTrackNames[slot]);
                    StartFallbackTimer();
                }
            }

            // Never force fade = 0 here. In tModLoader 0.11 this hook runs before
            // Main.UpdateAudio(), which is responsible for advancing the native fade.
        }

        private static Song CreateSentinelSong(string name)
        {
            if (!MediaPlayerAvailable || SongConstructor == null)
                return null;

            try
            {
                // Dummy path – we never actually Play this Song when Media is missing.
                return (Song)SongConstructor.Invoke(new object[] { name, "" });
            }
            catch
            {
                return null;
            }
        }

        #region Playback helpers

        private static void EnsurePlaying()
        {
            if (!UseMediaPlayerPlayback)
            {
                if (!FallbackStopwatch.IsRunning && !FallbackIsPaused)
                    StartFallbackTimer();
                return;
            }

            if (MediaPlayerAvailable && TrackedSong != null)
            {
                try
                {
                    if (MediaPlayer.State == MediaState.Stopped)
                    {
                        MediaPlayer.IsRepeating = true;
                        MediaPlayer.IsMuted = false;
                        MediaPlayer.Play(TrackedSong);
                    }
                    return;
                }
                catch
                {
                    MediaPlayerAvailable = false;
                }
            }

            // Fallback path.
            if (!FallbackStopwatch.IsRunning && !FallbackIsPaused)
                StartFallbackTimer();
        }

        private static void StopPlayback()
        {
            if (MediaPlayerAvailable)
            {
                try
                {
                    if (MediaPlayer.State != MediaState.Stopped)
                        MediaPlayer.Stop();
                }
                catch { }
            }

            FallbackStopwatch.Reset();
            FallbackPausedAccumulated = TimeSpan.Zero;
            FallbackIsPaused = false;
        }

        private static void PausePlayback()
        {
            if (FallbackStopwatch.IsRunning)
            {
                FallbackPausedAccumulated += FallbackStopwatch.Elapsed;
                FallbackStopwatch.Reset();
            }
            FallbackIsPaused = true;
        }

        private static void ResumePlayback()
        {
            if (FallbackIsPaused)
            {
                FallbackIsPaused = false;
                FallbackStopwatch.Restart();
            }
        }

        private static void SetVolume(float volume)
        {
            if (!UseMediaPlayerPlayback)
                return;

            if (MediaPlayerAvailable)
            {
                try
                {
                    if (MediaPlayer.Volume != volume)
                        MediaPlayer.Volume = volume;
                    return;
                }
                catch { }
            }
            // Fallback has no independent volume control; the normal musicFade already
            // gates whether we keep the track alive.
        }

        private static void StartFallbackTimer()
        {
            FallbackPausedAccumulated = TimeSpan.Zero;
            FallbackIsPaused = false;
            FallbackStopwatch.Restart();
        }

        #endregion

        public static bool TryGetSongInformation(out BaseTrackedMusic information)
        {
            information = null;

            if (TrackedSong == null)
                return false;

            string name = TrackedSong.Name;
            if (string.IsNullOrEmpty(name))
                return false;

            return TrackInformation.TryGetValue(name, out information);
        }

        /// <summary>
        /// Optional helper: force-stop when the game is on the main menu.
        /// Call from a menu-related hook if desired.
        /// </summary>
        public static void OnEnterMainMenu()
        {
            if (TrackedSong != null)
            {
                StopPlayback();
                TrackedSong = null;
            }
        }
    }
}
