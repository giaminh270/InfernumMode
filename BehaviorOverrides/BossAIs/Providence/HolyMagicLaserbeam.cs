using CalamityMod;
using CalamityMod.NPCs;
using CalamityMod.Projectiles.BaseProjectiles;
using InfernumMode.Sounds;
using InfernumMode.Graphics.Interfaces;
using InfernumMode.Graphics.Primitives;
using InfernumMode.BehaviorOverrides.BossAIs.ProfanedGuardians;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.Graphics.Effects;
using Terraria.ModLoader;
using InfernumMode.ExtraTextures;
using InfernumMode.Effects;
using System;

namespace InfernumMode.BehaviorOverrides.BossAIs.Providence
{
    public class HolyMagicLaserbeam : BaseLaserbeamProjectile, IPixelPrimitiveDrawer, ISpecializedDrawRegion
    {
        public bool DrawBeforeNPCs => false;
		
        public PrimitiveTrailCopy LaserDrawer
        {
            get;
            set;
        }

        public int LaserTelegraphTime
        {
            get;
            set;
        } = 45;

        public int LaserShootTime
        {
            get;
            set;
        } = 35;

        public bool FromGuardians
        {
            get;
            set;
        }

        public float SetAngleToMoveTo
        {
            get;
            set;
        }

        public Vector2 InitialVelocity
        {
            get;
            set;
        } = Vector2.Zero;

        public override float Lifetime => LaserTelegraphTime + LaserShootTime;

        public override Color LaserOverlayColor => Color.White;

        public override Color LightCastColor => Color.Wheat;

        public override Texture2D LaserBeginTexture => Main.projectileTexture[projectile.type];

        public override Texture2D LaserMiddleTexture => ModContent.GetTexture("InfernumMode/ExtraTextures/Lasers/OrangeLaserbeamMid");

        public override Texture2D LaserEndTexture => ModContent.GetTexture("InfernumMode/ExtraTextures/Lasers/OrangeLaserbeamEnd");

        public override float MaxLaserLength => 5000f;

        public override float MaxScale => 1f;

        public override string Texture => InfernumTextureRegistry.InvisPath;

        public override void SetStaticDefaults() => DisplayName.SetDefault("Holy Disintegration Deathray");

        public override void SetDefaults()
        {
            projectile.width = projectile.height = 36;
            if (ProvidenceBehaviorOverride.IsEnraged)
                projectile.Size *= 1.5f;

            projectile.hostile = true;
            projectile.alpha = 255;
            projectile.penetrate = -1;
            projectile.tileCollide = false;
            projectile.timeLeft = 600;
            projectile.Calamity().canBreakPlayerDefense = true;
            cooldownSlot = 1;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(projectile.localAI[0]);
            writer.Write(projectile.localAI[1]);
            writer.Write(LaserTelegraphTime);
            writer.Write(LaserShootTime);
            writer.Write(FromGuardians);
            writer.Write(SetAngleToMoveTo);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            projectile.localAI[0] = reader.ReadSingle();
            projectile.localAI[1] = reader.ReadSingle();
            LaserTelegraphTime = reader.ReadInt32();
            LaserShootTime = reader.ReadInt32();
            FromGuardians = reader.ReadBoolean();
            SetAngleToMoveTo = reader.ReadSingle();
        }

        public override void AttachToSomething()
        {
            // Disappear if providence and the commander is not present.
            if (!Main.npc.IndexInRange(CalamityGlobalNPC.holyBoss) && !Main.npc.IndexInRange(CalamityGlobalNPC.doughnutBoss))
            {
                projectile.Kill();
                return;
            }

            Vector2 center;
            if (FromGuardians)
            {
                Projectile fireball = null;

                for (int i = 0; i < Main.maxProjectiles; i++)
                {
                    if (Main.projectile[i].type == ModContent.ProjectileType<HolyDogmaFireball>())
                    {
                        fireball = Main.projectile[i];
                        break;
                    }
                }
                if (fireball is null)
                {
                    projectile.Kill();
                    return;
                }
                else
                    center = fireball.Center;
            }
            else
                center = Main.npc[CalamityGlobalNPC.holyBoss].Center;

            projectile.Center = center + Vector2.UnitY * 24f;
            projectile.Opacity = 1f;

            if (Time == 0)
                InitialVelocity = projectile.velocity;

            // Rotate during the telegraph.
            float telegraphCompletion = Utils.InverseLerp(0f, LaserTelegraphTime, Time, true);
            if (telegraphCompletion < 1f)
            {
                if (SetAngleToMoveTo == 0)
                    projectile.velocity = projectile.velocity.RotatedBy(RotationalSpeed * (float)Math.Pow(CalamityUtils.Convert01To010(telegraphCompletion), 15f));
                else
                    projectile.velocity = InitialVelocity.ToRotation().AngleLerp(SetAngleToMoveTo, telegraphCompletion).ToRotationVector2();
            }
            projectile.velocity = projectile.velocity.RotatedBy(-RotationalSpeed);

            // Play a sound if the telegraph is over.
            if (Time == LaserTelegraphTime)
                Main.PlaySound(InfernumSoundRegistry.ProvidenceDogmaBeamFire, projectile.Center);
        }

        public override void DetermineScale()
        {
            float lifetimeCompletion = Utils.InverseLerp(LaserTelegraphTime, Lifetime, Time, true);
            projectile.scale = CalamityUtils.Convert01To010(lifetimeCompletion) * MaxScale * 3f;
            if (projectile.scale > MaxScale)
                projectile.scale = MaxScale;
        }

        public override float DetermineLaserLength()
        {
            // Make the laser over time fire outward instead of instantly being full length, for the sake of impact.
            return Utilities.Remap(Time, LaserTelegraphTime, LaserTelegraphTime + 10f, 20f, MaxLaserLength);
        }

        public override bool CanDamage() => Time >= LaserTelegraphTime;

        public float LaserWidthFunction(float _) => projectile.scale * projectile.width * 2;

        public Color LaserColorFunction(float completionRatio)
        {
            float colorInterpolant = (float)Math.Sin(Main.GlobalTime * -3.2f + completionRatio * 23f) * 0.5f + 0.5f;
            Color c = Color.Lerp(Color.Orange, Color.Pink, colorInterpolant * 0.67f);
            if (ProvidenceBehaviorOverride.IsEnraged && !FromGuardians)
                c = Color.Lerp(c, Color.SkyBlue, 0.55f);

            return c;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor) => false;

        public void DrawPixelPrimitives(SpriteBatch spriteBatch)
        {
            // Don't draw until the telegraphs are done being drawn.
            if (Time < LaserTelegraphTime)
                return;

            // This should never happen, but just in case.
            if (projectile.velocity == Vector2.Zero)
                return;

            if (LaserDrawer == null)
            	LaserDrawer = new PrimitiveTrailCopy(LaserWidthFunction, LaserColorFunction, null, true, InfernumEffectsRegistry.ArtemisLaserVertexShader);
            Vector2 laserEnd = projectile.Center + projectile.velocity.SafeNormalize(Vector2.UnitY) * LaserLength;
            Vector2[] baseDrawPoints = new Vector2[20];
            for (int i = 0; i < baseDrawPoints.Length; i++)
                baseDrawPoints[i] = Vector2.Lerp(projectile.Center, laserEnd, i / (float)(baseDrawPoints.Length - 1f));

            // Select textures to pass to the shader, along with the electricity color.
            InfernumEffectsRegistry.ArtemisLaserVertexShader.UseColor(Color.Wheat);
            InfernumEffectsRegistry.ArtemisLaserVertexShader.SetShaderTexture(InfernumTextureRegistry.StreakMagma);
            InfernumEffectsRegistry.ArtemisLaserVertexShader.UseImage("Images/Misc/Perlin");

            LaserDrawer.DrawPixelated(baseDrawPoints, -Main.screenPosition, 54);
        }

        // Draw the telegraphs before the lasers go outward.
        // This technically also draws when the lasers are shot, since the bloom looks super cool.
        public void SpecialDraw(SpriteBatch spriteBatch)
        {
            float opacity = (float)Math.Pow(Time / LaserTelegraphTime, 0.4f);
            Texture2D invisible = InfernumTextureRegistry.Invisible;
            Vector2 drawPosition = projectile.Center - Main.screenPosition;

			Effect laserScopeEffect = InfernumEffectsRegistry.PixelatedSightLine.GetShader().Shader;
            float width = (0.002f + (float)Math.Pow(opacity, 4f) * ((float)Math.Sin(Main.GlobalTime * 3.5f) * 0.001f + 0.001f)) * projectile.width / 36f;
            if (width >= 0.0072f)
                width = 0.0072f;

			laserScopeEffect.Parameters["sampleTexture2"].SetValue(ModContent.GetTexture("InfernumMode/ExtraTextures/GreyscaleGradients/CertifiedCrustyNoise"));
            laserScopeEffect.Parameters["noiseOffset"].SetValue(Main.GameUpdateCount * -0.003f);
            laserScopeEffect.Parameters["mainOpacity"].SetValue((float)Math.Sqrt(opacity));
            laserScopeEffect.Parameters["Resolution"].SetValue(new Vector2(1500f));
            laserScopeEffect.Parameters["laserAngle"].SetValue(-projectile.velocity.ToRotation());
            laserScopeEffect.Parameters["laserWidth"].SetValue(width);
            laserScopeEffect.Parameters["laserLightStrenght"].SetValue(5f);
            laserScopeEffect.Parameters["color"].SetValue(Color.Lerp(Color.Pink, ProvidenceBehaviorOverride.IsEnraged && !FromGuardians ? Color.Cyan : Color.Yellow, projectile.identity / 7f % 1f * 0.84f).ToVector3());
            laserScopeEffect.Parameters["darkerColor"].SetValue(Color.Lerp(Color.Orange, Color.Red, 0.24f).ToVector3());
            laserScopeEffect.Parameters["bloomSize"].SetValue(0.28f + (1f - opacity) * 0.18f);
            laserScopeEffect.Parameters["bloomMaxOpacity"].SetValue(0.4f);
            laserScopeEffect.Parameters["bloomFadeStrenght"].SetValue(3f);
            laserScopeEffect.CurrentTechnique.Passes[0].Apply();
            spriteBatch.Draw(invisible, drawPosition, null, Color.White, 0f, invisible.Size() * 0.5f, opacity * MaxLaserLength * 1.3f, SpriteEffects.None, 0f);
        }

        public void PrepareSpriteBatch(SpriteBatch spriteBatch)
        {
            Main.spriteBatch.EnterShaderRegion(BlendState.Additive);
        }
    }
}
