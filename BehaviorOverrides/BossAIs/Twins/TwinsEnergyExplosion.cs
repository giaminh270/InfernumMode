using CalamityMod;
using CalamityMod.Projectiles;
using InfernumMode.Sounds;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.Twins
{
    public class TwinsEnergyExplosion : ModProjectile
    {
        public ref float OwnerType => ref projectile.ai[0];

        public ref float Radius => ref projectile.ai[1];

        public const int Lifetime = 80;

        public override string Texture => "InfernumMode/ExtraTextures/GreyscaleObjects/Gleam";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Boom");
        }

        public override void SetDefaults()
        {
            projectile.width = 72;
            projectile.height = 72;
            projectile.penetrate = -1;
            projectile.tileCollide = false;
            projectile.timeLeft = Lifetime;
            projectile.usesLocalNPCImmunity = true;
            projectile.localNPCHitCooldown = 10;
            projectile.scale = 0.001f;
            cooldownSlot = 1;
        }

        public override void AI()
        {
            if (projectile.localAI[0] == 0f)
            {
                Main.PlaySound(InfernumSoundRegistry.TwinsForcefieldExplosionSound, projectile.Center);
                projectile.localAI[0] = 1f;
            }
            Main.LocalPlayer.Infernum().CurrentScreenShakePower = (float)Math.Sin(MathHelper.Pi * projectile.timeLeft / Lifetime) * 14f + 2f;

            Radius = MathHelper.Lerp(Radius, 1516f, 0.15f);
            projectile.scale = MathHelper.Lerp(1.2f, 5f, Utils.InverseLerp(Lifetime, 0f, projectile.timeLeft, true));
            CalamityGlobalProjectile.ExpandHitboxBy(projectile, (int)(Radius * projectile.scale), (int)(Radius * projectile.scale));
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.LinearWrap, DepthStencilState.None, Main.instance.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

            float pulseCompletionRatio = Utils.InverseLerp(Lifetime, 0f, projectile.timeLeft, true);
            Vector2 scale = new Vector2(1.5f, 1f);
            DrawData drawData = new DrawData(ModContent.GetTexture("InfernumMode/ExtraTextures/GreyscaleGradients/TechyNoise"),
                projectile.Center - Main.screenPosition + projectile.Size * scale * 0.5f,
                new Rectangle(0, 0, projectile.width, projectile.height),
                new Color(new Vector4(1f - (float)Math.Sqrt(pulseCompletionRatio))) * projectile.Opacity,
                projectile.rotation,
                projectile.Size,
                scale,
                SpriteEffects.None, 0);

            Color pulseColor = OwnerType == NPCID.Spazmatism ? Color.LimeGreen : Color.Red;
            GameShaders.Misc["ForceField"].UseColor(pulseColor);
            GameShaders.Misc["ForceField"].Apply(drawData);

            for (int i = 0; i < 5; i++)
                drawData.Draw(Main.spriteBatch);

            Main.spriteBatch.ExitShaderRegion();
            return false;
        }
    }
}
