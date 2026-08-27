using CalamityMod.Particles;
using InfernumMode.Effects;
using InfernumMode.ExtraTextures;
using InfernumMode.Particles;
using InfernumMode.Projectiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.ProfanedGuardians
{
    public class HolyFireRift : ModProjectile
    {
        #region Properties
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public bool SpearRift => projectile.ai[0] == 1;

        public bool MarkedAsDead => projectile.ai[1] == 1;

        public Vector2 RiftSize
        {
            get;
            set;
        } = new Vector2(50f, 50f);

        public float BallSize
        {
            get;
            set;
        } = 55f;
        #endregion

        #region Overrides
        public override void SetStaticDefaults() => DisplayName.SetDefault("Fire Rift");

        public override void SetDefaults()
        {
            projectile.width = 32;
            projectile.height = 32;
            projectile.hostile = true;
            projectile.penetrate = -1;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;
            projectile.timeLeft = CommanderSpearThrown.TelegraphTime;
            projectile.Opacity = 0;
            projectile.scale = 0;
            cooldownSlot = 1;
        }

        public override void AI()
        {
            // Die if the commander is non-existant.
            if (HolySineSpear.Commander is null)
            {
                projectile.Kill();
                return;
            }

            // Emit light.
            Lighting.AddLight(projectile.Center, Color.Gold.ToVector3() * 0.45f);

            if (!SpearRift)
            {
                // Do not die naturally, the commander will manually kill these.
                projectile.timeLeft = 240;
                if (!MarkedAsDead)
                {
                    projectile.Opacity = MathHelper.Clamp(projectile.Opacity + 0.05f, 0f, 1f);
                    projectile.scale = MathHelper.Clamp(projectile.scale + 0.1f, 0f, 1f);
                }
                else
                {
                    projectile.Opacity = MathHelper.Clamp(projectile.Opacity - 0.05f, 0f, 1f);
                    projectile.scale = MathHelper.Clamp(projectile.scale - 0.1f, 0f, 1f);

                    if (projectile.scale == 0f || projectile.Opacity == 0f)
                        projectile.Kill();
                }
            }

            // Spawn a bunch of metaballs.
            if (SpearRift)
            {
                for (int i = 0; i < 3; i++)
                    InfernumFusableParticleManager.GetParticleSetByType<ProfanedLavaParticleSet>()?.SpawnParticle(projectile.Center +
                        Main.rand.NextVector2Circular(RiftSize.X, RiftSize.Y), Main.rand.NextFloat(BallSize * 0.75f, BallSize));
            }
        }

        public override bool CanDamage() => false;

        public override bool ShouldUpdatePosition() => false;

        public void DrawPortal(SpriteBatch spriteBatch)
        {
            Texture2D fireNoise = InfernumTextureRegistry.WavyNoise;
            Texture2D miscNoise = InfernumTextureRegistry.FireNoise;

            Effect portal = InfernumEffectsRegistry.ProfanedPortalShader.Shader;
            portal.Parameters["sampleTexture"].SetValue(fireNoise);
            portal.Parameters["sampleTexture2"].SetValue(miscNoise);
            portal.Parameters["mainColor"].SetValue(WayfinderSymbol.Colors[1].ToVector3());
            portal.Parameters["secondaryColor"].SetValue(WayfinderSymbol.Colors[2].ToVector3());
            portal.Parameters["resolution"].SetValue(new Vector2(120f));
            portal.Parameters["time"].SetValue(Main.GlobalTime);
            portal.Parameters["opacity"].SetValue(projectile.Opacity);
            portal.Parameters["innerGlowAmount"].SetValue(0.8f);
            portal.Parameters["innerGlowDistance"].SetValue(0.15f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, Main.instance.Rasterizer, portal, Main.GameViewMatrix.TransformationMatrix);
            spriteBatch.Draw(fireNoise, projectile.Center - Main.screenPosition, null, Color.White, 0f, fireNoise.Size() * 0.5f, 2f * projectile.scale, SpriteEffects.None, 0f);
            spriteBatch.ExitShaderRegion();
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            if (!SpearRift)
            {
                DrawPortal(Main.spriteBatch);
                return false;
            }
            float scaleInterpolant = Utils.InverseLerp(15f, 30f, projectile.timeLeft, true) * Utils.InverseLerp(240f, 200f, projectile.timeLeft, true) * (1f + 0.1f *
                (float)Math.Cos(Main.GlobalTime % 30f / 0.5f * (MathHelper.Pi * 2f) * 3f)) * 0.225f;

            Texture2D texture = InfernumTextureRegistry.Gleam;
            Vector2 drawPos = projectile.Center - Main.screenPosition + new Vector2(0f, projectile.gfxOffY);
            Color baseColor = WayfinderSymbol.Colors[1];
            baseColor.A = 0;
            Color colorA = baseColor;
            Color colorB = baseColor * 0.5f;
            colorA *= scaleInterpolant;
            colorB *= scaleInterpolant;
            Vector2 origin = texture.Size() / 2f;
            Vector2 scale = new Vector2(0.5f, 2f) * projectile.scale * scaleInterpolant;

            SpriteEffects spriteEffects = SpriteEffects.None;
            if (projectile.spriteDirection == -1)
                spriteEffects = SpriteEffects.FlipHorizontally;

            float upRight = projectile.rotation + MathHelper.PiOver4;
            float up = projectile.rotation + MathHelper.PiOver2;
            float upLeft = projectile.rotation + 3f * MathHelper.PiOver4;
            float left = projectile.rotation + MathHelper.Pi;
            Main.spriteBatch.Draw(texture, drawPos, null, colorA, upLeft, origin, scale, spriteEffects, 0);
            Main.spriteBatch.Draw(texture, drawPos, null, colorA, upRight, origin, scale, spriteEffects, 0);
            Main.spriteBatch.Draw(texture, drawPos, null, colorB, upLeft, origin, scale * 0.6f, spriteEffects, 0);
            Main.spriteBatch.Draw(texture, drawPos, null, colorB, upRight, origin, scale * 0.6f, spriteEffects, 0);
            Main.spriteBatch.Draw(texture, drawPos, null, colorA, up, origin, scale * 0.6f, spriteEffects, 0);
            Main.spriteBatch.Draw(texture, drawPos, null, colorA, left, origin, scale * 0.6f, spriteEffects, 0);
            Main.spriteBatch.Draw(texture, drawPos, null, colorB, up, origin, scale * 0.36f, spriteEffects, 0);
            Main.spriteBatch.Draw(texture, drawPos, null, colorB, left, origin, scale * 0.36f, spriteEffects, 0);

            return false;
        }
        #endregion
    }
}
