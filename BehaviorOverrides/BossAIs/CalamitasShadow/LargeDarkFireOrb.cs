using CalamityMod;
using CalamityMod.NPCs;
using InfernumMode.Effects;
using InfernumMode.ExtraTextures;
using InfernumMode.Sounds;
using InfernumMode.Graphics.Interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.CalamitasShadow
{
    public class LargeDarkFireOrb : ModProjectile, ISpecializedDrawRegion
    {
        public ref float Time => ref projectile.ai[1];

        public static float MaxFireOrbRadius => 320f;

        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public override void SetStaticDefaults() => DisplayName.SetDefault("Dark Fire Orb");

        public override void SetDefaults()
        {
            projectile.width = projectile.height = 20;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;
            projectile.penetrate = -1;
            projectile.timeLeft = 7200;
            projectile.Opacity = 0f;
            projectile.netImportant = true;
            cooldownSlot = 1;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(projectile.timeLeft);
            writer.Write(projectile.tileCollide);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            projectile.timeLeft = reader.ReadInt32();
            projectile.tileCollide = reader.ReadBoolean();
        }

        public override void AI()
        {
            // Disappear if Calamitas' shadow is not present.
            if (CalamityGlobalNPC.calamitas == -1)
                projectile.Kill();

            projectile.scale = Utilities.Remap(projectile.timeLeft, 25f, 1f, 1f, 10f);
            projectile.Opacity = Utils.InverseLerp(1f, 18f, projectile.timeLeft, true);

            if (Math.Abs(projectile.velocity.Y) >= 1f)
            {
                projectile.velocity.Y *= 1.036f;
                projectile.tileCollide = true;
            }

            Lighting.AddLight(projectile.Center, Color.Yellow.ToVector3() * 0.76f);

            Time++;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor) => false;

        public void SpecialDraw(SpriteBatch spriteBatch)
        {
            float circleFadeinInterpolant = Utils.InverseLerp(0f, 36f, Time, true);
            float colorPulse = ((float)Math.Cos(Main.GlobalTime * 7.2f + projectile.identity) * 0.5f + 0.5f) * 0.6f;
            colorPulse += (float)(Math.Cos(Main.GlobalTime * 6.1f + projectile.identity * 1.3f) * 0.5f + 0.5f) * 0.4f;

            Color explosionTelegraphColor = Color.Lerp(Color.Red, Color.Purple, colorPulse * 0.3f + 0.4f) * circleFadeinInterpolant;

            Texture2D invisible = InfernumTextureRegistry.Invisible;
            Texture2D noise = ModContent.GetTexture("InfernumMode/ExtraTextures/GreyscaleGradients/VoronoiShapes2");
            Effect fireballShader = InfernumEffectsRegistry.FireballShader.GetShader().Shader;

            Vector2 scale = Vector2.One * MaxFireOrbRadius / invisible.Size() * circleFadeinInterpolant * projectile.Opacity * projectile.scale * 2f;
            fireballShader.Parameters["sampleTexture2"].SetValue(noise);
            fireballShader.Parameters["mainColor"].SetValue(explosionTelegraphColor.ToVector3());
            fireballShader.Parameters["resolution"].SetValue(Vector2.One * 250f);
            fireballShader.Parameters["speed"].SetValue(0.76f);
            fireballShader.Parameters["zoom"].SetValue(0.0004f);
            fireballShader.Parameters["dist"].SetValue(60f);
            fireballShader.Parameters["opacity"].SetValue(circleFadeinInterpolant * projectile.Opacity);
            fireballShader.CurrentTechnique.Passes[0].Apply();

            Vector2 drawPosition = projectile.Center - Main.screenPosition;

            float[] scaleFactors = new float[]
            {
                1f, 0.8f, 0.7f, 0.57f, 0.44f, 0.32f, 0.22f
            };

            for (int i = 0; i < scaleFactors.Length; i++)
            {
                fireballShader.Parameters["time"].SetValue(Main.GlobalTime * (i * 0.04f + 0.32f));
                fireballShader.Parameters["mainColor"].SetValue(Color.Lerp(explosionTelegraphColor, Color.White, i / (float)(scaleFactors.Length - 1f)).ToVector3());
                fireballShader.CurrentTechnique.Passes[0].Apply();
                Main.spriteBatch.Draw(invisible, drawPosition, null, Color.White, projectile.rotation, invisible.Size() * 0.5f, scale * scaleFactors[i], 0, 0f);
            }
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (projectile.timeLeft >= 25)
            {
                Main.PlaySound(InfernumSoundRegistry.ProvidenceLavaEruptionSound.WithVolume(0.7f).WithPitchVariance(-0.4f), projectile.Center);
                projectile.timeLeft = 25;
                projectile.netUpdate = true;
            }
            return false;
        }

        public override void Kill(int timeLeft)
        {
            if (CalamityGlobalNPC.calamitas == -1)
                return;

            Main.npc[CalamityGlobalNPC.calamitas].ai[1] = 0f;
            Main.npc[CalamityGlobalNPC.calamitas].Infernum().ExtraAI[2] = 1f;
            Main.npc[CalamityGlobalNPC.calamitas].netUpdate = true;
        }

        public void PrepareSpriteBatch(SpriteBatch spriteBatch)
        {
            spriteBatch.EnterShaderRegion(BlendState.Additive);
        }
    }
}
