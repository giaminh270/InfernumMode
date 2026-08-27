using System;
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
        [DefaultValue(true)]
        [Tooltip("Enables reduced graphics mode.")]
        public bool ReducedGraphicsConfig { get; set; }

        /// <summary>
        /// Convenience: skip spawning heavy particles entirely under reduced graphics.
        /// </summary>
        public static bool SkipHeavyParticle
        {
            get
            {
                var cfg = Instance;
                return cfg != null && cfg.ReducedGraphicsConfig;
            }
        }

        public override bool AcceptClientChanges(ModConfig pendingConfig, int whoAmI, ref string message) => false;
    }
}
