using CalamityMod;
using CalamityMod.NPCs;
using CalamityMod.NPCs.ProfanedGuardians;
using CalamityMod.Particles;
using InfernumMode.Graphics.Interfaces;
using InfernumMode.Graphics.Primitives;
using InfernumMode;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using static InfernumMode.BehaviorOverrides.BossAIs.ProfanedGuardians.GuardianComboAttackManager;
using InfernumMode.Projectiles;
using InfernumMode.Particles;
using Terraria.Graphics.Shaders;
using InfernumMode.ExtraTextures;
using InfernumMode.Effects;
using System;

namespace InfernumMode.BehaviorOverrides.BossAIs.ProfanedGuardians
{
    internal class HolySpinningFireBeam : ModProjectile, IPixelPrimitiveDrawer
    {
        internal PrimitiveTrailCopy TelegraphDrawer;

        internal PrimitiveTrailCopy BeamDrawer;

        public bool DrawBeforeNPCs => true;

        public ref float Time => ref projectile.ai[0];

        public const int Lifetime = 360;

        public const float MaxLaserLength = 8300f;

        public float CurrentLaserLength;

        public static int TelegraphTime => 45;

        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Holy Fire Beam");
        }

        public override void SetDefaults()
        {
            projectile.width = projectile.height = 60;
            projectile.hostile = true;
            projectile.ignoreWater = true;
            projectile.tileCollide = false;
            projectile.penetrate = -1;
            projectile.timeLeft = Lifetime;
            projectile.Opacity = 0f;
            projectile.scale = 0f;
            projectile.Calamity().canBreakPlayerDefense = true;
            cooldownSlot = 1;
        }

        public override void AI()
        {
            if (!Main.npc.IndexInRange(CalamityGlobalNPC.doughnutBoss) || !Main.npc[CalamityGlobalNPC.doughnutBoss].active)
            {
                projectile.Kill();
                return;
            }
            NPC owner = Main.npc[CalamityGlobalNPC.doughnutBoss];

            if (owner.type != ModContent.NPCType<ProfanedGuardianBoss>() || !owner.active)
            {
                projectile.Kill();
                return;
            }

            projectile.width = 60;

            // Do not naturally die.
            projectile.timeLeft = Lifetime;

            if (Time <= TelegraphTime)
            {
                projectile.Opacity = (float)Math.Sin(Time / TelegraphTime * MathHelper.Pi) * 2f;
                projectile.velocity = (MathHelper.TwoPi * projectile.ai[1] + MathHelper.PiOver2 + projectile.rotation).ToRotationVector2();
                projectile.Center = owner.Center + projectile.velocity;
                Time++;
                return;
            }
            bool fadeOut = owner.Infernum().ExtraAI[CommanderBlenderShouldFadeOutIndex] == 1 && (GuardiansAttackType)owner.ai[0] is GuardiansAttackType.HealerDeathAnimation;
            // Fade in.
            if (!fadeOut)
            {
                projectile.Opacity = MathHelper.Clamp(projectile.Opacity + 0.025f, 0f, 1f);
                projectile.scale = MathHelper.Clamp(projectile.scale + 0.025f, 0f, 1f);
            }
            else
            {
                projectile.Opacity = MathHelper.Clamp(projectile.Opacity - 0.025f, 0f, 1f);
                projectile.scale = MathHelper.Clamp(projectile.scale - 0.025f, 0f, 1f);
                if (projectile.Opacity == 0)
                {
                    projectile.Kill();
                    return;
                }
            }

            // Rotate.
            projectile.rotation += MathHelper.Lerp(0f, 0.014f, projectile.Opacity);
            projectile.velocity = (MathHelper.TwoPi * projectile.ai[1] + MathHelper.PiOver2 + projectile.rotation).ToRotationVector2();
            projectile.Center = owner.Center + projectile.velocity;

            // Sort out length.
            DetermineLaserLength(owner);

            if (Main.myPlayer == projectile.owner && !InfernumConfig.Instance.ReducedGraphicsConfig)
                CreateTileHitEffects();
            Time++;
        }

        public void DetermineLaserLength(NPC owner)
        {
            Player target = Main.player[owner.target];
            float width = projectile.width * projectile.scale * 1.75f;

            // If something is inbetween the npc and the target, go through blocks.
            if (!Collision.CanHitLine(owner.Center, (int)width, (int)1, target.position, target.Hitbox.Width, target.Hitbox.Height))
            {
                CurrentLaserLength = MaxLaserLength;
                return;
            }
            // Else, end at the first tile collision.
            float[] samples = new float[20];
            Collision.LaserScan(owner.Center, projectile.velocity, width, MaxLaserLength, samples);
            CurrentLaserLength = samples.Average();
        }

        public void CreateTileHitEffects()
        {
            Vector2 endOfLaser = projectile.Center + projectile.velocity * (CurrentLaserLength);
            InfernumFusableParticleManager.GetParticleSetByType<ProfanedLavaParticleSet>()?.SpawnParticle(endOfLaser + Main.rand.NextVector2Circular(15f, 15f), 135f);
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float _ = 0f;
            float width = projectile.width * projectile.scale * 1.75f;
            Vector2 start = projectile.Center;
            Vector2 end = start + projectile.velocity * CurrentLaserLength;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), start, end, width, ref _);
        }

        public override bool CanDamage() => projectile.Opacity >= 0.85f;

        public override bool ShouldUpdatePosition() => false;

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            if (Time > TelegraphTime)
            {
                // Draw a glow effect at the end of the laser.
                Texture2D glowBloom = ModContent.GetTexture("InfernumMode/ExtraTextures/BloomFlare");
                Texture2D glowCircle = ModContent.GetTexture("CalamityMod/Particles/BloomCircle");
                Vector2 glowPosition = projectile.Center + projectile.velocity * CurrentLaserLength;
                Color glowColor = Color.Lerp(WayfinderSymbol.Colors[0], WayfinderSymbol.Colors[1], 0.3f);
                glowColor.A = 0;
                float glowRotation = Main.GlobalTime * 4;
                float scaleInterpolant = (1f + (float)Math.Sin(Main.GlobalTime * 5f)) / 2f;
                float scale = MathHelper.Lerp(1.8f, 2.2f, scaleInterpolant);
                Main.spriteBatch.Draw(glowBloom, glowPosition - Main.screenPosition, null, glowColor, glowRotation, glowBloom.Size() * 0.5f, scale * 0.5f, SpriteEffects.None, 0f);
                Main.spriteBatch.Draw(glowCircle, glowPosition - Main.screenPosition, null, glowColor, glowRotation, glowCircle.Size() * 0.5f, scale, SpriteEffects.None, 0f);
            }
            return false;
        }

        public float WidthFunction(float completionRatio) => projectile.width * projectile.scale * 2f;

        public Color ColorFunction(float completionRatio)
        {
            float interpolant = (1f + (float)Math.Sin(Main.GlobalTime * 2f)) / 2f;
            float colorInterpolant = MathHelper.Lerp(0.3f, 0.5f, interpolant);
            return Color.Lerp(Color.OrangeRed, Color.Gold, colorInterpolant) * projectile.Opacity;
        }

        public float TelegraphWidthFunction(float completionRatio) => projectile.width * 1.5f;

        public Color TelegraphColorFunction(float completionRatio)
        {
            Color orange = Color.Lerp(Color.OrangeRed, WayfinderSymbol.Colors[2], 0.5f);
            return Color.Lerp(orange, WayfinderSymbol.Colors[0], completionRatio) * projectile.Opacity;
        }

        public void DrawPixelPrimitives(SpriteBatch spriteBatch)
        {
            if (Time <= TelegraphTime)
            {
                if (TelegraphDrawer == null)
                    TelegraphDrawer = new PrimitiveTrailCopy(TelegraphWidthFunction, TelegraphColorFunction, null, true, InfernumEffectsRegistry.SideStreakVertexShader);

                bool flipY = projectile.ai[1] == 0.5f;

                InfernumEffectsRegistry.SideStreakVertexShader.SetShaderTexture(InfernumTextureRegistry.CultistRayMap);
                InfernumEffectsRegistry.SideStreakVertexShader.UseOpacity(0.3f);
                InfernumEffectsRegistry.SideStreakVertexShader.Shader.Parameters["flipY"].SetValue(flipY);

                Vector2 telegraphStartPos = projectile.Center - projectile.velocity * 2f;
                Vector2 telegraphEndPos = projectile.Center + projectile.velocity * 1750f;

                Vector2[] telegraphDrawPoints = new Vector2[8];
                for (int i = 0; i < telegraphDrawPoints.Length; i++)
                    telegraphDrawPoints[i] = Vector2.Lerp(telegraphStartPos, telegraphEndPos, (float)i / telegraphDrawPoints.Length);

                TelegraphDrawer.DrawPixelated(telegraphDrawPoints, -Main.screenPosition, 40);

                Texture2D warningSymbol = InfernumTextureRegistry.VolcanoWarning;
                Vector2 drawPosition = (projectile.Center + projectile.velocity * 280f) - Main.screenPosition;
                Color drawColor = Color.Orange * projectile.Opacity;
                drawColor.A = 0;
                float rotation = flipY ? MathHelper.Pi : 0f;
                Vector2 origin = warningSymbol.Size() * 0.5f;

                spriteBatch.Draw(warningSymbol, drawPosition, null, drawColor, rotation, origin, 0.8f, SpriteEffects.None, 0f);
                return;
            }

            if (BeamDrawer == null)
                BeamDrawer = new PrimitiveTrailCopy(WidthFunction, ColorFunction, null, true, InfernumEffectsRegistry.GuardiansLaserVertexShader);

            InfernumEffectsRegistry.GuardiansLaserVertexShader.SetShaderTexture(InfernumTextureRegistry.CrustyNoise);
            InfernumEffectsRegistry.GuardiansLaserVertexShader.SetShaderTexture(InfernumTextureRegistry.CultistRayMap);
            InfernumEffectsRegistry.GuardiansLaserVertexShader.UseColor(new Color(255, 221, 135));
            InfernumEffectsRegistry.GuardiansLaserVertexShader.Shader.Parameters["flipY"].SetValue(false);
            float lengthScalar = CurrentLaserLength / MaxLaserLength;
            InfernumEffectsRegistry.GuardiansLaserVertexShader.Shader.Parameters["stretchAmount"].SetValue(4f * lengthScalar);
            InfernumEffectsRegistry.GuardiansLaserVertexShader.Shader.Parameters["pillarVarient"].SetValue(false);
            InfernumEffectsRegistry.GuardiansLaserVertexShader.Shader.Parameters["scrollSpeed"].SetValue(1.8f);

            Vector2 startPos = projectile.Center - projectile.velocity * 2f;
            Vector2 endPos = projectile.Center + projectile.velocity * CurrentLaserLength * (0.2f * (1f - (lengthScalar * 0.8f)) + 1f);

            Vector2[] drawPoints = new Vector2[54];
            for (int i = 0; i < drawPoints.Length; i++)
                drawPoints[i] = Vector2.Lerp(startPos, endPos, (float)i / drawPoints.Length);

            BeamDrawer.DrawPixelated(drawPoints, -Main.screenPosition, 30);
        }
    }
}
