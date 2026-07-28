using Microsoft.Xna.Framework;

namespace InfernumMode
{
    public struct BloomLineDrawInfo
    {
        public float LineRotation { get; set; }
        public float WidthFactor { get; set; }
        public float Opacity { get; set; }
        public float BloomIntensity { get; set; }
        public Color MainColor { get; set; }
        public Color DarkerColor { get; set; }
        public Vector2 Scale { get; set; }
        public float BloomOpacity { get; set; }
        public float LightStrength { get; set; }

        public BloomLineDrawInfo(float rotation, float width, float opacity, float bloom, Color main, Color darker, Vector2 scale, float bloomOpacity = 0.425f, float lightStrength = 5f)
        {
            LineRotation = rotation;
            WidthFactor = width;
            Opacity = opacity;
            BloomIntensity = bloom;
            MainColor = main;
            DarkerColor = darker;
            Scale = scale;
            BloomOpacity = bloomOpacity;
            LightStrength = lightStrength;
        }
    }
}