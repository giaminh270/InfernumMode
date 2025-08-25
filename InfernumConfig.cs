using System.ComponentModel;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

namespace InfernumMode
{
    [Label("Config")]
    [BackgroundColor(96, 30, 53, 216)]
    public class InfernumConfig : ModConfig
    {
        public static InfernumConfig Instance => ModContent.GetInstance<InfernumConfig>();
        public override ConfigScope Mode => ConfigScope.ClientSide;

        [Label("Boss Introduction Animations")]
        [BackgroundColor(224, 127, 180, 192)]
        [DefaultValue(true)]
        [Tooltip("Enables boss introduction animations. They only activate when Infernum Mode is active.")]
        public bool BossIntroductionAnimationsAreAllowed { get; set; }
		
		[Label("Reduced Graphical Settings")]
        [BackgroundColor(224, 127, 180, 192)]
        [DefaultValue(false)]
        [Tooltip("Enables reduced graphics mode. Use this if lag is an issue.")]
        public bool ReducedGraphicsConfig { get; set; }

        [Label("Saturation Bloom Intensity")]
        [BackgroundColor(224, 127, 180, 192)]
        [SliderColor(224, 165, 56, 128)]
        [Range(0f, 1f)]
        [DefaultValue(0f)]
        [Tooltip("How intense color saturation bloom effects should be. Such effects are disabled when this value is zero. Be warned that high values may be overwhelming.")]
        public float SaturationBloomIntensity { get; set; }

        public override bool AcceptClientChanges(ModConfig pendingConfig, int whoAmI, ref string message) => false;
    }
}