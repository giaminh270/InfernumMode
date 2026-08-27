using CalamityMod;
using CalamityMod.NPCs.ExoMechs.Ares;
using CalamityMod.Projectiles.BaseProjectiles;
using InfernumMode.ExtraTextures;
using InfernumMode.Graphics.Interfaces;
using InfernumMode.Graphics.Primitives;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.Draedon.Ares
{
    public class AresSpinningDeathBeam : BaseLaserbeamProjectile, IPixelPrimitiveDrawer
    {
        public bool DrawBeforeNPCs => false;
		
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public int OwnerIndex
        {
            get => (int)projectile.ai[1];
            set => projectile.ai[1] = value;
        }

        public PrimitiveTrailCopy BeamDrawer;

        public float InitialSpinDirection = -100f;

        public bool SuperLaser => Main.npc[OwnerIndex].localAI[3] >= 0.1f;

        public float LifetimeThing = 600f;
        public override float MaxScale => 1f;
        public override float MaxLaserLength => AresDeathBeamTelegraph.TelegraphWidth;
        public override float Lifetime => LifetimeThing;
        public override Color LaserOverlayColor => new Color(250, 250, 250, 100);
        public override Color LightCastColor => Color.White;
        public override Texture2D LaserBeginTexture => ModContent.GetTexture("CalamityMod/Projectiles/Boss/AresDeathBeamStart");
        public override Texture2D LaserMiddleTexture => ModContent.GetTexture("CalamityMod/ExtraTextures/Lasers/AresDeathBeamMiddle");
        public override Texture2D LaserEndTexture => ModContent.GetTexture("CalamityMod/ExtraTextures/Lasers/AresDeathBeamEnd");

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Exo Overload Beam");
            Main.projFrames[projectile.type] = 5;
        }

        public override void SetDefaults()
        {
            projectile.width = 85;
            projectile.height = 56;
            projectile.hostile = true;
            projectile.alpha = 255;
            projectile.penetrate = -1;
            projectile.tileCollide = false;
            projectile.timeLeft = 1600;
            projectile.Calamity().canBreakPlayerDefense = true;
            cooldownSlot = 1;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(projectile.localAI[0]);
			writer.Write(projectile.localAI[1]);
            writer.Write(InitialSpinDirection);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            projectile.localAI[0] = reader.ReadSingle();
			projectile.localAI[1] = reader.ReadSingle();
            InitialSpinDirection = reader.ReadSingle();
        }

        public override void AttachToSomething()
        {
            if (InitialSpinDirection == -100f)
                InitialSpinDirection = projectile.velocity.ToRotation();

            // Adjust the size if this is a super laser.
            if (projectile.width != 160 && SuperLaser)
                projectile.width = 160;

            if (Main.npc[OwnerIndex].active && Main.npc[OwnerIndex].type == ModContent.NPCType<AresBody>() && Main.npc[OwnerIndex].Opacity > 0.35f)
            {
                float spinOffset = Main.npc[OwnerIndex].Infernum().ExtraAI[0];
                projectile.velocity = (InitialSpinDirection + spinOffset).ToRotationVector2();
                Vector2 fireFrom = new Vector2(Main.npc[OwnerIndex].Center.X - 1f, Main.npc[OwnerIndex].Center.Y + 23f);
                fireFrom += projectile.velocity.SafeNormalize(Vector2.UnitY) * MathHelper.Lerp(2f, 16f, projectile.scale * projectile.scale);
                projectile.Center = fireFrom;
            }

            // Die of the owner is invalid in some way.
            else
            {
                projectile.Kill();
                return;
            }
        }

        public override float DetermineLaserLength() => MaxLaserLength;

        public override void PostAI()
        {
            // Spawn dust at the end of the beam.
            int dustType = 107;
            Vector2 dustCreationPosition = projectile.Center + projectile.velocity * (LaserLength - 14f);
            for (int i = 0; i < 2; i++)
            {
                float dustDirection = projectile.velocity.ToRotation() + Main.rand.NextBool().ToDirectionInt() * MathHelper.PiOver2;
                Vector2 dustVelocity = dustDirection.ToRotationVector2() * Main.rand.NextFloat(2f, 4f);
                Dust exoEnergy = Dust.NewDustDirect(dustCreationPosition, 0, 0, dustType, dustVelocity.X, dustVelocity.Y, 0, new Color(0, 255, 255), 1f);
                exoEnergy.noGravity = true;
                exoEnergy.scale = 1.7f;
            }

            if (Main.rand.NextBool(5))
            {
                Vector2 dustSpawnOffset = projectile.velocity.RotatedBy(MathHelper.PiOver2) * Main.rand.NextFloatDirection() * projectile.width * 0.5f;
                Dust exoEnergy = Dust.NewDustDirect(dustCreationPosition + dustSpawnOffset - Vector2.One * 4f, 8, 8, dustType, 0f, 0f, 100, new Color(0, 255, 255), 1.5f);
                exoEnergy.velocity *= 0.5f;

                // Ensure that the dust always moves up.
                exoEnergy.velocity.Y = -Math.Abs(exoEnergy.velocity.Y);
            }

            // Determine frames.
            projectile.frameCounter++;
            if (projectile.frameCounter % 5f == 0f)
                projectile.frame = (projectile.frame + 1) % Main.projFrames[projectile.type];
        }

        public float WidthFunction(float completionRatio)
        {
            return MathHelper.Clamp(projectile.width * projectile.scale, 0f, projectile.width);
        }

        public Color ColorFunction(float completionRatio)
        {
            Color color = Main.hslToRgb((completionRatio * 2f + Main.GlobalTime * 0.4f + projectile.identity * 0.27f) % 1f, 1f, 0.6f);
            if (SuperLaser)
            {
                Color laserFireColor = Color.Lerp(Color.Orange, Color.Red, (float)Math.Sin(Main.GlobalTime * 1.7f + completionRatio * 2.3f) * 0.5f + 0.5f);
                color = Color.Lerp(color, laserFireColor, 0.8f);
            }

            color.A = 5;
            return color * 2f;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor) => false;

        public void DrawPixelPrimitives(SpriteBatch spriteBatch)
        {
			if (BeamDrawer is null)
            	BeamDrawer = new PrimitiveTrailCopy(WidthFunction, ColorFunction, null, false, GameShaders.Misc["CalamityMod:Bordernado"]);

            GameShaders.Misc["CalamityMod:Bordernado"].UseSaturation(MathHelper.Lerp(0.23f, 0.29f, projectile.identity / 9f % 1f));
            GameShaders.Misc["CalamityMod:Bordernado"].SetShaderTexture(InfernumTextureRegistry.CultistRayMap);

            List<Vector2> points = new List<Vector2>();
            for (int i = 0; i <= 8; i++)
                points.Add(Vector2.Lerp(projectile.Center, projectile.Center + projectile.velocity * LaserLength, i / 8f));

            if (Time >= 2f)
            {
                for (float offset = 0f; offset < 6f; offset += 0.75f)
                {
                    BeamDrawer.DrawPixelated(points, -Main.screenPosition, 11);
                    BeamDrawer.DrawPixelated(points, (Main.GlobalTime * 1.8f).ToRotationVector2() * offset - Main.screenPosition, 9);
                    BeamDrawer.DrawPixelated(points, -(Main.GlobalTime * 1.8f).ToRotationVector2() * offset - Main.screenPosition, 9);
                }
            }
        }

        public override bool CanHitPlayer(Player target) => projectile.scale >= 0.5f;
    }
}
