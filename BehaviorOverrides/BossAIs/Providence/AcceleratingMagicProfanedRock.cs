using CalamityMod;
using CalamityMod.Particles;
using InfernumMode.Graphics;
using InfernumMode.Graphics.Interfaces;
using InfernumMode.Graphics.Primitives;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using System;
using InfernumMode.Particles;
using InfernumMode.Projectiles;
using InfernumMode.ExtraTextures;
using InfernumMode.Effects;

namespace InfernumMode.BehaviorOverrides.BossAIs.Providence
{
    public class AcceleratingMagicProfanedRock : ModProjectile, ISpecializedDrawRegion
    {
        public int CurrentVarient
        {
            get;
            set;
        } = 1;

        public int MagicGlowTimer
        {
            get;
            set;
        } = 30;

        public PrimitiveTrailCopy AfterimageTrail
        {
            get;
            set;
        }

        public ref float Timer => ref projectile.ai[0];

        public override string Texture => "CalamityMod/Projectiles/Typeless/ArtifactOfResilienceShard" + CurrentVarient;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Profaned Rock");
            ProjectileID.Sets.TrailingMode[projectile.type] = 2;
            ProjectileID.Sets.TrailCacheLength[projectile.type] = 12;
        }

        public override void SetDefaults()
        {
            // The size gets changed later, but is this be default.
            projectile.width = 42;
            projectile.height = 36;

            projectile.friendly = false;
            projectile.hostile = true;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;
            projectile.penetrate = -1;
            projectile.Opacity = 1f;
            projectile.timeLeft = 240;
            cooldownSlot = 1;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(CurrentVarient);
            writer.Write(MagicGlowTimer);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            CurrentVarient = reader.ReadInt32();
            MagicGlowTimer = reader.ReadInt32();
        }

        public override void AI()
        {
            // Initialize the rock variant.
            if (projectile.localAI[0] == 0)
            {
                projectile.localAI[0] = 1f;
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    CurrentVarient = Main.rand.Next(1, 7);
                    switch (CurrentVarient)
                    {
                        case 2:
                            projectile.width = 30;
                            projectile.height = 38;
                            break;
                        case 3:
                            projectile.width = 34;
                            projectile.height = 38;
                            break;
                        case 4:
                            projectile.width = 36;
                            projectile.height = 46;
                            break;
                        case 5:
                            projectile.width = 28;
                            projectile.height = 36;
                            break;
                        case 6:
                            projectile.width = 22;
                            projectile.height = 20;
                            break;
                    }
                    projectile.netUpdate = true;
                }
            }

            // Accelerate.
            if (projectile.velocity.Length() < 32f)
                projectile.velocity *= 1.037f;

            // Create rock particles.
            Particle rockParticle = new SandyDustParticle(projectile.Center + Main.rand.NextVector2Circular(projectile.width / 3f, projectile.height / 3f), Vector2.Zero, Color.SandyBrown, Main.rand.NextFloat(0.45f, 0.75f), 30);
            GeneralParticleHandler.SpawnParticle(rockParticle);

            // Emit lava particles.
            if (Main.rand.NextBool() && Main.netMode != NetmodeID.Server)
            {
                Vector2 lavaSpawnPosition = projectile.Center + projectile.velocity * 0.5f;
                ModContent.GetTexture(Texture).CreateMetaballsFromTexture(ref InfernumFusableParticleManager.GetParticleSetByType<ProfanedLavaParticleSet>().Particles, lavaSpawnPosition, 0f, projectile.scale * 0.8f, 12f, 190);
            }

            // Spin.
            projectile.rotation -= 0.1f;
            Timer++;
        }

        public float PrimitiveWidthFunction(float _) => projectile.scale * 30f;

        public Color PrimitiveColorFunction(float _) => Color.HotPink * projectile.Opacity * 1.3f;

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor) => false;

        public void DrawAfterimageTrail()
        {
            // Initialize the trail.
            var trailShader = InfernumEffectsRegistry.Trail;
            if (AfterimageTrail == null)
                AfterimageTrail = new PrimitiveTrailCopy(PrimitiveWidthFunction, PrimitiveColorFunction, null, true, trailShader);

            float localIdentityOffset = projectile.identity * 0.1372f;
            Color mainColor = CalamityUtils.MulticolorLerp((Main.GlobalTime * 2f + localIdentityOffset) % 1f, Color.Yellow, Color.Pink, Color.HotPink, Color.Goldenrod, Color.Orange);
            Color secondaryColor = CalamityUtils.MulticolorLerp((Main.GlobalTime * 2f + localIdentityOffset + 0.2f) % 1f, Color.Yellow, Color.Pink, Color.HotPink, Color.Goldenrod, Color.Orange);

            mainColor = Color.Lerp(Color.White, mainColor, 0.85f);
            secondaryColor = Color.Lerp(Color.White, secondaryColor, 0.85f);

            Vector2 trailOffset = projectile.Size * 0.5f - Main.screenPosition;
            trailShader.SetShaderTexture(InfernumTextureRegistry.FireNoise);
            trailShader.UseImage("Images/Extra_189");
            trailShader.UseColor(mainColor);
            trailShader.UseSecondaryColor(secondaryColor);
            AfterimageTrail.Draw(projectile.oldPos, trailOffset, 5);
        }

        public void SpecialDraw(SpriteBatch spriteBatch)
        {
            Texture2D texture = ModContent.GetTexture(Texture);
            Vector2 drawPosition = projectile.Center - Main.screenPosition;
            Vector2 origin = texture.Size() * 0.5f;

            Color backglowColor = Color.Lerp(WayfinderSymbol.Colors[0], Color.Pink, 0.5f);
            backglowColor.A = 0;

            // Draw the afterimage trail first.
            DrawAfterimageTrail();

            // Draw the bloom line telegraph.
            if (Timer <= MagicGlowTimer)
            {
                float opacity = CalamityUtils.Convert01To010(Timer / MagicGlowTimer);
                BloomLineDrawInfo lineInfo = new BloomLineDrawInfo()
                {
                    LineRotation = -projectile.velocity.ToRotation(),
                    WidthFactor = 0.003f + (float)Math.Pow(opacity, 5f) * ((float)Math.Sin(Main.GlobalTime * 3f) * 0.001f + 0.001f),
                    BloomIntensity = MathHelper.Lerp(0.06f, 0.16f, opacity),
                    Scale = Vector2.One * 1950f,
                    MainColor = Color.Pink,
                    DarkerColor = Color.Orange,
                    Opacity = opacity,
                    BloomOpacity = 0.4f,
                    LightStrength = 5f
                };
                Utilities.DrawBloomLineTelegraph(drawPosition, lineInfo, false);
            }

            float backglowCount = 12;
            for (int i = 0; i < backglowCount; i++)
            {
                Vector2 backglowOffset = (MathHelper.TwoPi * i / backglowCount).ToRotationVector2() * 4f;
                Main.spriteBatch.Draw(texture, drawPosition + backglowOffset, null, backglowColor * projectile.Opacity, projectile.rotation, texture.Size() * 0.5f, projectile.scale, SpriteEffects.None, 0);
            }
            Main.spriteBatch.Draw(texture, drawPosition, null, projectile.GetAlpha(Color.White) * projectile.Opacity, projectile.rotation, origin, projectile.scale, SpriteEffects.None, 0);
            if (Timer <= MagicGlowTimer)
            {
                backglowColor = Color.HotPink * (1 - Timer / MagicGlowTimer);
                for (int i = 0; i < 3; i++)
                    Main.spriteBatch.Draw(texture, drawPosition, null, backglowColor * projectile.Opacity, projectile.rotation, origin, projectile.scale, SpriteEffects.None, 0);
            }
        }

        public void PrepareSpriteBatch(SpriteBatch spriteBatch)
        {
            spriteBatch.EnforceCutoffRegion(new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Main.GameViewMatrix.TransformationMatrix, SpriteSortMode.Immediate, BlendState.Additive);
        }
    }
}
