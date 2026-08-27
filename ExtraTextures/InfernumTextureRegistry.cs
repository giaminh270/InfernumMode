using Microsoft.Xna.Framework.Graphics;
using Terraria.ModLoader;

namespace InfernumMode.ExtraTextures
{


    public static class InfernumTextureRegistry
    {
        private static Texture2D Tex(string path)
        {
            return ModContent.GetTexture(path);
        }

        public static Texture2D Arrow => Tex("InfernumMode/ExtraTextures/GreyscaleObjects/ArrowBlack");
        public static Texture2D BigGreyscaleCircle => Tex("InfernumMode/ExtraTextures/GreyscaleObjects/BigGreyscaleCircle");
        public static Texture2D BinaryLine => Tex("InfernumMode/ExtraTextures/Trails/BinaryLine");
        public static Texture2D BloomFlare => Tex("InfernumMode/ExtraTextures/GreyscaleObjects/BloomFlare");
        public static Texture2D BloomLine => Tex("InfernumMode/ExtraTextures/Lines/BloomLine");
        public static Texture2D BloomLineSmall => Tex("InfernumMode/ExtraTextures/Lines/BloomLineSmall");
        public static Texture2D BlurryPerlinNoise => Tex("InfernumMode/ExtraTextures/GreyscaleGradients/BlurryPerlinNoise");
        public static Texture2D Bubble => Tex("InfernumMode/ExtraTextures/GreyscaleObjects/Bubble");
        public static Texture2D Cloud => Tex("InfernumMode/ExtraTextures/GreyscaleObjects/NebulaGas1");
        public static Texture2D Cloud2 => Tex("InfernumMode/ExtraTextures/GreyscaleObjects/NebulaGas2");
        public static Texture2D CrispCircle => Tex("InfernumMode/ExtraTextures/GreyscaleObjects/CrispCircle");
        public static Texture2D CrustyNoise => Tex("InfernumMode/ExtraTextures/GreyscaleGradients/CrustyNoise");
        public static Texture2D CracksNoise => Tex("InfernumMode/ExtraTextures/GreyscaleGradients/CracksNoise");
        public static Texture2D CrystalNoise => Tex("InfernumMode/ExtraTextures/GreyscaleGradients/Crystals");
        public static Texture2D CrystalNoiseNormal => Tex("InfernumMode/ExtraTextures/GreyscaleGradients/CrystalsNormalMap");
        public static Texture2D CultistRayMap => Tex("InfernumMode/ExtraTextures/GreyscaleGradients/CultistRayMap");
        public static Texture2D DayGradient => Tex("InfernumMode/ExtraTextures/Gradients/DayGradient");
        public static Texture2D DiagonalGleam => Tex("InfernumMode/ExtraTextures/GreyscaleGradients/DiagonalGleam");
        public static Texture2D DistortedBloomRing => Tex("InfernumMode/ExtraTextures/GreyscaleObjects/DistortedBloomRing");
        public static Texture2D DistortedCircle => Tex("InfernumMode/ExtraTextures/GreyscaleObjects/DistortedCircle");
        public static Texture2D EmpressStar => Tex("InfernumMode/ExtraTextures/GreyscaleObjects/EmpressStar");
        public static Texture2D FireNoise => Tex("InfernumMode/ExtraTextures/GreyscaleGradients/FireNoise");
        public static Texture2D Gleam => Tex("InfernumMode/ExtraTextures/GreyscaleObjects/Gleam");
        public static Texture2D GreyscalePill => Tex("InfernumMode/ExtraTextures/GreyscaleObjects/GreyscalePill");
        public static Texture2D GrayscaleWater => Tex("InfernumMode/ExtraTextures/ScrollingLayers/GrayscaleWater");
        public static Texture2D GuardianCommanderGlow => Tex("InfernumMode/ExtraTextures/GreyscaleObjects/CommanderGlow");
        public static Texture2D GuardianDefenderGlow => Tex("InfernumMode/ExtraTextures/GreyscaleObjects/DefenderGlow");
        public static Texture2D HarshNoise => Tex("InfernumMode/ExtraTextures/GreyscaleGradients/HarshNoise");
        public static Texture2D HexagonGrid => Tex("InfernumMode/ExtraTextures/GreyscaleGradients/HexagonGrid");
        public static Texture2D HollowCircleSoftEdge => Tex("InfernumMode/ExtraTextures/GreyscaleObjects/HollowCircleSoftEdge");
        public static Texture2D HoneycombNoise => Tex("InfernumMode/ExtraTextures/GreyscaleGradients/HoneycombNoise");
        public static Texture2D HolyCrystalLayer => Tex("InfernumMode/ExtraTextures/ScrollingLayers/HolyCrystalLayer");
        public static Texture2D HolyFireLayer => Tex("InfernumMode/ExtraTextures/ScrollingLayers/HolyFireLayer");
        public static Texture2D HolyFirePixelLayer => Tex("InfernumMode/ExtraTextures/ScrollingLayers/HolyFirePixelLayer");
        public static Texture2D HolyFirePixelLayerNight => Tex("InfernumMode/ExtraTextures/ScrollingLayers/HolyFirePixelLayerNight");
        public static Texture2D HyperplaneMatrixCode => Tex("InfernumMode/ExtraTextures/ScrollingLayers/HyperplaneMatrixCode");
        public static Texture2D Invisible => Tex("InfernumMode/ExtraTextures/Invisible");
        public static Texture2D LargeStar => Tex("InfernumMode/ExtraTextures/GreyscaleObjects/LargeStar");
        public static Texture2D LaserCircle => Tex("InfernumMode/ExtraTextures/GreyscaleObjects/LaserCircle");
        public static Texture2D LavaNoise => Tex("InfernumMode/ExtraTextures/GreyscaleGradients/LavaNoise");
        public static Texture2D LessCrustyNoise => Tex("InfernumMode/ExtraTextures/GreyscaleGradients/LessCrustyNoise");
        public static Texture2D LightningStreak => Tex("InfernumMode/ExtraTextures/Trails/StreakLightning");
        public static Texture2D Line => Tex("InfernumMode/ExtraTextures/Lines/Line");
        public static Texture2D MilkyNoise => Tex("InfernumMode/ExtraTextures/GreyscaleGradients/MilkyNoise");
        public static Texture2D MoonLordBackground => Tex("InfernumMode/ExtraTextures/ScrollingLayers/MoonLordBGLayer");
        public static Texture2D NightGradient => Tex("InfernumMode/ExtraTextures/Gradients/NightGradient");
        public static Texture2D Pixel => Tex("InfernumMode/ExtraTextures/Pixel");
        public static Texture2D SimpleNoise => Tex("InfernumMode/ExtraTextures/GreyscaleGradients/SimpleNoise");
        public static Texture2D Shadow => Tex("InfernumMode/ExtraTextures/ScrollingLayers/Shadow1");
        public static Texture2D Shadow2 => Tex("InfernumMode/ExtraTextures/ScrollingLayers/Shadow2");
        public static Texture2D Smoke => Tex("InfernumMode/ExtraTextures/GreyscaleObjects/Smoke");
        public static Texture2D SmokyNoise => Tex("InfernumMode/ExtraTextures/GreyscaleGradients/SmokyNoise");
        public static Texture2D SolidEdgeGradient => Tex("InfernumMode/ExtraTextures/GreyscaleObjects/SolidEdgeGradient");
        public static Texture2D Smudges => Tex("InfernumMode/ExtraTextures/GreyscaleGradients/Smudges");
        public static Texture2D SquareSmoke => Tex("InfernumMode/ExtraTextures/GreyscaleObjects/SquareSmoke");
        public static Texture2D Stars => Tex("InfernumMode/ExtraTextures/ScrollingLayers/Stars");
        public static Texture2D StreakBigBackground => Tex("InfernumMode/ExtraTextures/Trails/MegaStreakBacking");
        public static Texture2D StreakBigInner => Tex("InfernumMode/ExtraTextures/Trails/MegaStreakInner");
        public static Texture2D StreakBubble => Tex("InfernumMode/ExtraTextures/Trails/Streak3");
        public static Texture2D StreakBubbleGlow => Tex("InfernumMode/ExtraTextures/Trails/Streak4");
        public static Texture2D StreakFaded => Tex("InfernumMode/ExtraTextures/Trails/StreakFaded");
        public static Texture2D StreakFire => Tex("InfernumMode/ExtraTextures/Trails/StreakFire");
        public static Texture2D StreakGeneric => Tex("InfernumMode/ExtraTextures/Trails/GenericStreak");
        public static Texture2D StreakLightning => Tex("InfernumMode/ExtraTextures/Trails/ZapTrail");
        public static Texture2D StreakMagma => Tex("InfernumMode/ExtraTextures/Trails/StreakMagma");
        public static Texture2D StreakSolid => Tex("InfernumMode/ExtraTextures/Trails/StreakSolid");
        public static Texture2D StreakThickGlow => Tex("InfernumMode/ExtraTextures/Trails/Streak2");
        public static Texture2D StreakThinGlow => Tex("InfernumMode/ExtraTextures/Trails/Streak1");
        public static Texture2D TelegraphLine => Tex("CalamityMod/ExtraTextures/LaserWallTelegraphBeam");
        public static Texture2D TrypophobiaNoise => Tex("InfernumMode/ExtraTextures/GreyscaleGradients/TrypophobiaNoise");
        public static Texture2D Void => Tex("InfernumMode/ExtraTextures/ScrollingLayers/Void");
        public static Texture2D VolcanoWarning => Tex("InfernumMode/ExtraTextures/GreyscaleObjects/VolcanoWarningBlack");
        public static Texture2D VoronoiCelluar => Tex("InfernumMode/ExtraTextures/GreyscaleGradients/VoronoiCellular");
        public static Texture2D VoronoiLoop => Tex("InfernumMode/ExtraTextures/GreyscaleGradients/VoronoiLoop");
        public static Texture2D VoronoiShapes => Tex("InfernumMode/ExtraTextures/GreyscaleGradients/VoronoiShapes");
        public static Texture2D Water => Tex("InfernumMode/ExtraTextures/ScrollingLayers/Water");
        public static Texture2D WaterNoise => Tex("InfernumMode/ExtraTextures/GreyscaleGradients/WaterNoise");
        public static Texture2D WavyNeuronsNoise => Tex("InfernumMode/ExtraTextures/GreyscaleGradients/WavyNeurons");
        public static Texture2D WavyNoise => Tex("InfernumMode/ExtraTextures/GreyscaleGradients/WavyNoise");
        public static Texture2D WhiteHole => Tex("InfernumMode/ExtraTextures/GreyscaleObjects/WhiteHole");

        public static string InvisPath => "InfernumMode/ExtraTextures/Invisible";
    }
}
