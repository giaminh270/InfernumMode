using InfernumMode.BaseEntities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.Polterghast
{
    public class PolterghastWave : BaseWaveExplosionProjectile
    {
        public override int Lifetime => 80;

        public override float MaxRadius => 1700f;

        public override float RadiusExpandRateInterpolant => 0.15f;

        public override string Texture => "InfernumMode/ExtraTextures/GreyscaleObjects/Gleam";

        public override float DetermineScreenShakePower(float lifetimeCompletionRatio, float distanceFromPlayer)
        {
            float baseShakePower = MathHelper.Lerp(1f, 5f, (float)Math.Sin(MathHelper.Pi * lifetimeCompletionRatio));
            return baseShakePower * Utils.InverseLerp(2200f, 1050f, distanceFromPlayer, true);
        }

        public override Color DetermineExplosionColor(float lifetimeCompletionRatio)
        {
            return Color.Lerp(Color.Cyan, Color.Pink * 0.8f, MathHelper.Clamp(lifetimeCompletionRatio * 1.75f, 0f, 1f));
        }
    }
}
