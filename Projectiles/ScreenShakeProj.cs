using CalamityMod;
using System.IO;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.Projectiles
{
    public class ScreenShakeProj : ModProjectile
    {
        public int RippleCount;

        public int RippleSize;

        public float RippleSpeed;

        public bool UseSecondaryVariant
        {
            get => projectile.ai[0] == 1f;
            set => projectile.ai[0] = value.ToInt();
        }

        public Filter ScreenShader => UseSecondaryVariant ? Filters.Scene["InfernumMode:ScreenShake2"] : Filters.Scene["InfernumMode:ScreenShake"];

        public const int Lifetime = 105;

        public override string Texture => "InfernumMode/ExtraTextures/Invisible";

        public override void SetStaticDefaults() => DisplayName.SetDefault("Screen Shake");

        public override void SetDefaults()
        {
            projectile.width = 2;
            projectile.height = 2;
            projectile.penetrate = -1;
            projectile.friendly = true;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;
            projectile.MaxUpdates = 1;
            projectile.timeLeft = Lifetime;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(RippleCount);
            writer.Write(RippleSize);
            writer.Write(RippleSpeed);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            RippleCount = reader.ReadInt32();
            RippleSize = reader.ReadInt32();
            RippleSpeed = reader.ReadSingle();
        }

        public override void AI()
        {
            // Don't do anything if running server-side or if screen shake effects are disabled in the config.
            if (Main.netMode == NetmodeID.Server || CalamityConfig.Instance.DisableScreenShakes)
                return;

            if (!ScreenShader.IsActive())
            {
                string screenShaderKey = UseSecondaryVariant ? "InfernumMode:ScreenShake2" : "InfernumMode:ScreenShake";
                Filters.Scene.Activate(screenShaderKey, projectile.Center).GetShader().UseColor(RippleCount, RippleSize, RippleSpeed).UseTargetPosition(projectile.Center);
            }
            else
            {
                float progress = Utilities.Remap(projectile.timeLeft, Lifetime, 0f, 0f, 1f);
                ScreenShader.GetShader().UseProgress(progress).UseOpacity((1f - progress) * 30f);
            }
        }

        public override void Kill(int timeLeft)
        {
            if (Main.netMode != NetmodeID.Server && ScreenShader.IsActive())
                ScreenShader.Deactivate();
        }
    }
}
