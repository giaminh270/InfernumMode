using CalamityMod;
using CalamityMod.NPCs.ExoMechs.Artemis;
using CalamityMod.Projectiles.BaseProjectiles;
using CalamityMod.Skies;
using InfernumMode.Effects;
using InfernumMode.Graphics.Interfaces;
using InfernumMode.Graphics.Primitives;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.Draedon.ArtemisAndApollo
{
    public class ArtemisSpinLaser : BaseLaserbeamProjectile, IPixelPrimitiveDrawer
    {
		public bool DrawBeforeNPCs => false;
		
        public PrimitiveTrailCopy LaserDrawer
        {
            get;
            set;
        }

        public int OwnerIndex
        {
            get => (int)projectile.ai[0];
            set => projectile.ai[0] = value;
        }

        public const int LaserLifetime = 72;
        public override float MaxScale => 1f;
        public override float MaxLaserLength => 3600f;
        public override float Lifetime => LaserLifetime;
        public override Color LaserOverlayColor => new Color(250, 180, 100, 100);
        public override Color LightCastColor => Color.White;
        public override Texture2D LaserBeginTexture => Main.projectileTexture[projectile.type];
        public override Texture2D LaserMiddleTexture => ModContent.GetTexture("CalamityMod/ExtraTextures/Lasers/AresLaserBeamMiddle");
        public override Texture2D LaserEndTexture => ModContent.GetTexture("CalamityMod/ExtraTextures/Lasers/AresLaserBeamEnd");
        public override string Texture => "CalamityMod/Projectiles/Boss/AresLaserBeamStart";

        // Dude
        // Dude
        // Dude
        // You are going to Ohio
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Ohio Beam");
            Main.projFrames[projectile.type] = 5;
        }

        public override void SetDefaults()
        {
            projectile.width = 60;
            projectile.height = 50;
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
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            projectile.localAI[0] = reader.ReadSingle();
            projectile.localAI[1] = reader.ReadSingle();
        }

        public override void AttachToSomething()
        {
            if (Main.npc[OwnerIndex].active && Main.npc[OwnerIndex].type == ModContent.NPCType<Artemis>())
            {
                Vector2 fireFrom = Main.npc[OwnerIndex].Center + Vector2.UnitY * Main.npc[OwnerIndex].gfxOffY;
                fireFrom += projectile.velocity.SafeNormalize(Vector2.UnitY) * 78f;
                projectile.Center = fireFrom;
            }

            // Die of the owner is invalid in some way.
            else
            {
                projectile.Kill();
                return;
            }

            bool notUsingReleventAttack = Main.npc[OwnerIndex].ai[0] != (int)ApolloBehaviorOverride.TwinsAttackType.ArtemisLaserRay && ExoMechManagement.TotalMechs <= 1;
            if (Main.npc[OwnerIndex].Opacity <= 0f || notUsingReleventAttack)
            {
                projectile.Kill();
                return;
            }

            // Periodically create lightning bolts in the sky.
            int lightningBoltCreateRate = ExoMechManagement.CurrentTwinsPhase >= 6 ? 3 : 6;
            if (Main.netMode != NetmodeID.Server && Time % lightningBoltCreateRate == lightningBoltCreateRate - 1f)
                ExoMechsSky.CreateLightningBolt(6);

            if (ExoMechManagement.TotalMechs <= 1)
                Time = Main.npc[OwnerIndex].ai[1];
        }

        public override float DetermineLaserLength()
        {
            float[] sampledLengths = new float[10];
            Collision.LaserScan(projectile.Center, projectile.velocity, projectile.width * projectile.scale, MaxLaserLength, sampledLengths);

            float newLaserLength = sampledLengths.Average();

            // Fire laser through walls at max length if target is behind tiles.
            if (!Collision.CanHitLine(Main.npc[OwnerIndex].Center, 1, 1, Main.player[Main.npc[OwnerIndex].target].Center, 1, 1))
                newLaserLength = MaxLaserLength;

            return newLaserLength;
        }

        public override void UpdateLaserMotion()
        {
            projectile.rotation = Main.npc[OwnerIndex].rotation;
            projectile.velocity = (projectile.rotation - MathHelper.PiOver2).ToRotationVector2();
        }

        public override void PostAI()
        {
            // Determine frames.
            projectile.frameCounter++;
            if (projectile.frameCounter % 5f == 0f)
                projectile.frame = (projectile.frame + 1) % Main.projFrames[projectile.type];
        }

        public float LaserWidthFunction(float _) => projectile.scale * projectile.width;

        public static Color LaserColorFunction(float completionRatio)
        {
            float colorInterpolant = (float)Math.Sin(Main.GlobalTime * -3.2f + completionRatio * 23f) * 0.5f + 0.5f;
            return Color.Lerp(Color.Orange, Color.Red, colorInterpolant * 0.67f);
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor) => false;

        public void DrawPixelPrimitives(SpriteBatch spriteBatch)
        {
            // This should never happen, but just in case.
            if (projectile.velocity == Vector2.Zero)
                return;
            if (LaserDrawer is null)
            	LaserDrawer = new PrimitiveTrailCopy(LaserWidthFunction, LaserColorFunction, null, true, InfernumEffectsRegistry.ArtemisLaserVertexShader);

            Vector2 laserEnd = projectile.Center + projectile.velocity.SafeNormalize(Vector2.UnitY) * LaserLength;
            Vector2[] baseDrawPoints = new Vector2[8];
            for (int i = 0; i < baseDrawPoints.Length; i++)
                baseDrawPoints[i] = Vector2.Lerp(projectile.Center, laserEnd, i / (float)(baseDrawPoints.Length - 1f));

            // Select textures to pass to the shader, along with the electricity color.
            InfernumEffectsRegistry.ArtemisLaserVertexShader.UseColor(Color.Cyan);
            InfernumEffectsRegistry.ArtemisLaserVertexShader.UseImage("Images/Extra_189");
            InfernumEffectsRegistry.ArtemisLaserVertexShader.UseImage("Images/Misc/Perlin");

            int sampleCount = InfernumConfig.Instance.ReducedGraphicsConfig ? 25 : 54;
            LaserDrawer.DrawPixelated(baseDrawPoints, -Main.screenPosition, sampleCount);
        }

        public override bool CanHitPlayer(Player target) => projectile.scale >= 0.5f;
    }
}
