using CalamityMod;
using CalamityMod.Projectiles.BaseProjectiles;
using InfernumMode.Effects;
using InfernumMode.ExtraTextures;
using InfernumMode.Graphics.Interfaces;
using InfernumMode.Graphics.Primitives;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.Prime
{
    public class EvenlySpreadPrimeLaserRay : BaseLaserbeamProjectile, IPixelPrimitiveDrawer
    {
		public bool DrawBeforeNPCs => false;
		
        public PrimitiveTrailCopy BeamDrawer
        {
            get;
            set;
        }

        public float InitialDirection = -100f;
        public int OwnerIndex => (int)projectile.ai[1];
        public override float Lifetime => 260;
        public override Color LaserOverlayColor => Color.White;
        public override Color LightCastColor => Color.White;
        public override Texture2D LaserBeginTexture => ModContent.GetTexture("InfernumMode/ExtraTextures/Lasers/PrimeBeamBegin");
        public override Texture2D LaserMiddleTexture => ModContent.GetTexture("InfernumMode/ExtraTextures/Lasers/PrimeBeamMid");
        public override Texture2D LaserEndTexture => ModContent.GetTexture("InfernumMode/ExtraTextures/Lasers/PrimeBeamEnd");
        public override string Texture => "InfernumMode/ExtraTextures/Lasers/PrimeBeamBegin";
        public override float MaxLaserLength => 3100f;
        public override float MaxScale => 1f;
        public override void SetStaticDefaults() => DisplayName.SetDefault("Deathray");

        public override void SetDefaults()
        {
            projectile.width = projectile.height = 40;
            projectile.hostile = true;
            projectile.alpha = 255;
            projectile.penetrate = -1;
            projectile.tileCollide = false;
            projectile.timeLeft = (int)Lifetime;
            projectile.Calamity().canBreakPlayerDefense = true;
            cooldownSlot = 1;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(projectile.localAI[0]);
            writer.Write(projectile.localAI[1]);
            writer.Write(InitialDirection);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            projectile.localAI[0] = reader.ReadSingle();
            projectile.localAI[1] = reader.ReadSingle();
            InitialDirection = reader.ReadSingle();
        }
        public override void AttachToSomething()
        {
            if (InitialDirection == -100f)
                InitialDirection = projectile.velocity.ToRotation();

            if (!Main.npc.IndexInRange(OwnerIndex))
            {
                projectile.Kill();
                return;
            }

            projectile.velocity = (InitialDirection + Main.npc[OwnerIndex].Infernum().ExtraAI[5]).ToRotationVector2();
            projectile.Center = Main.npc[OwnerIndex].Center - Vector2.UnitY * 16f + projectile.velocity * 2f;
        }
        public float WidthFunction(float completionRatio)
        {
            return MathHelper.Clamp(projectile.width * projectile.scale * 1.7f, 0f, projectile.width * 1.7f);
        }

        public Color ColorFunction(float completionRatio)
        {
            float colorInterpolant = (float)Math.Sin(Main.GlobalTime * -3.2f + completionRatio * 23f) * 0.5f + 0.5f;
            Color color = Color.Lerp(new Color(221, 50, 50), new Color(255, 5, 1), colorInterpolant * 0.67f);
            return color;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Texture2D glowTexture = ModContent.GetTexture("CalamityMod/Particles/BloomCircle");
            Color glowColor = Color.Brown;
            glowColor.A = 0;

            Main.spriteBatch.Draw(glowTexture, projectile.Center - Main.screenPosition, null, glowColor, 0f, glowTexture.Size() * 0.5f, 3f * projectile.scale, SpriteEffects.None, 0);

            InfernumEffectsRegistry.PulsatingLaserVertexShader.UseSaturation(1);
            InfernumEffectsRegistry.PulsatingLaserVertexShader.SetShaderTexture(InfernumTextureRegistry.StreakBigBackground);
            InfernumEffectsRegistry.PulsatingLaserVertexShader.Shader.Parameters["usePulsing"].SetValue(true);
            InfernumEffectsRegistry.PulsatingLaserVertexShader.Shader.Parameters["reverseDirection"].SetValue(false);
            InfernumEffectsRegistry.PulsatingLaserVertexShader.UseColor(ColorFunction(0.1f));

            List<Vector2> points = new List<Vector2>();
            for (int i = 0; i <= 8; i++)
                points.Add(Vector2.Lerp(projectile.Center, projectile.Center + projectile.velocity * LaserLength, i / 8f));

            BeamDrawer.Draw(points, -Main.screenPosition, 28);
            InfernumEffectsRegistry.PulsatingLaserVertexShader.SetShaderTexture(InfernumTextureRegistry.StreakBigInner);
            InfernumEffectsRegistry.PulsatingLaserVertexShader.Shader.Parameters["usePulsing"].SetValue(true);
            InfernumEffectsRegistry.PulsatingLaserVertexShader.UseColor(Color.Lerp(ColorFunction(0.5f), Color.White, 0.5f));
            InfernumEffectsRegistry.PulsatingLaserVertexShader.UseSaturation(1.5f);
            BeamDrawer.Draw(points, -Main.screenPosition, 28);
            return false;
        }

        public void DrawPixelPrimitives(SpriteBatch spriteBatch)
        {
        	if (BeamDrawer is null) 
				BeamDrawer = new PrimitiveTrailCopy(WidthFunction, ColorFunction, null, true, InfernumEffectsRegistry.PulsatingLaserVertexShader);

            //InfernumEffectsRegistry.PulsatingLaserVertexShader.UseSaturation(1);
            //InfernumEffectsRegistry.PulsatingLaserVertexShader.SetShaderTexture(InfernumTextureRegistry.StreakBigBackground);
            //InfernumEffectsRegistry.PulsatingLaserVertexShader.Shader.Parameters["usePulsing"].SetValue(true);
            //InfernumEffectsRegistry.PulsatingLaserVertexShader.UseColor(ColorFunction(0.1f));

            //List<Vector2> points = new();
            //for (int i = 0; i <= 8; i++)
            //    points.Add(Vector2.Lerp(Projectile.Center + Projectile.velocity * 80f, Projectile.Center + Projectile.velocity * LaserLength, i / 8f));

            //BeamDrawer.DrawPixelated(points, -Main.screenPosition, 28);
            //InfernumEffectsRegistry.PulsatingLaserVertexShader.SetShaderTexture(InfernumTextureRegistry.StreakBigInner);
            //InfernumEffectsRegistry.PulsatingLaserVertexShader.Shader.Parameters["usePulsing"].SetValue(true);
            //InfernumEffectsRegistry.PulsatingLaserVertexShader.UseColor(Color.Lerp(ColorFunction(0.5f), Color.White, 0.2f));
            //InfernumEffectsRegistry.PulsatingLaserVertexShader.UseSaturation(3);
            //BeamDrawer.DrawPixelated(points, -Main.screenPosition, 28);
        }
    }
}
