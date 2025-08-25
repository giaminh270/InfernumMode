using CalamityMod;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.NPCs.ExoMechs.Artemis;
using CalamityMod.Projectiles.BaseProjectiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.Draedon.ArtemisAndApollo
{
    public class ArtemisSweepLaserbeam : BaseLaserbeamProjectile
    {
        public PrimitiveTrail LaserDrawer
        {
            get;
            set;
        } = null;

        public int OwnerIndex
        {
            get => (int)projectile.ai[0];
            set => projectile.ai[0] = value;
        }

        public const int LifetimeConst = 540;

        public const float MaxLaserRayConst = 6000f;

        public override float MaxScale => 1f;
        public override float MaxLaserLength => MaxLaserRayConst;
        public override float Lifetime => LifetimeConst;
        public override Color LaserOverlayColor => new Color(250, 180, 100, 100);
        public override Color LightCastColor => Color.White;
        public override Texture2D LaserBeginTexture => ModContent.GetTexture(Texture);
        public override Texture2D LaserMiddleTexture => ModContent.GetTexture("CalamityMod/ExtraTextures/Lasers/AresLaserBeamMiddle");
        public override Texture2D LaserEndTexture => ModContent.GetTexture("CalamityMod/ExtraTextures/Lasers/AresLaserBeamEnd");
        public override string Texture => "CalamityMod/Projectiles/Boss/AresLaserBeamStart";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Exothermal Artemis Beam");
            Main.projFrames[projectile.type] = 5;
        }

        public override void SetDefaults()
        {
            projectile.Calamity().canBreakPlayerDefense = true;
            projectile.width = 30;
            projectile.height = 30;
            projectile.hostile = true;
            projectile.alpha = 255;
            projectile.penetrate = -1;
            projectile.tileCollide = false;
            projectile.timeLeft = 600;
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
            if (Main.npc[OwnerIndex].active && Main.npc[OwnerIndex].type == ModContent.NPCType<Artemis>() && Main.npc[OwnerIndex].Opacity > 0.25f)
            {
                Vector2 fireFrom = Main.npc[OwnerIndex].Center + Vector2.UnitY * Main.npc[OwnerIndex].gfxOffY;
                fireFrom += projectile.velocity.SafeNormalize(Vector2.UnitY) * 50f;
                projectile.Center = fireFrom;
            }

            // Die of the owner is invalid in some way.
            else
            {
                projectile.Kill();
                return;
            }
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

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            // This should never happen, but just in case.
            if (projectile.velocity == Vector2.Zero)
                return false;
			if (LaserDrawer is null)
				LaserDrawer = new PrimitiveTrail(LaserWidthFunction, LaserColorFunction, null, GameShaders.Misc["Infernum:ArtemisLaser"]);

            Vector2 laserEnd = projectile.Center + projectile.velocity.SafeNormalize(Vector2.UnitY) * LaserLength;
            Vector2[] baseDrawPoints = new Vector2[8];
            for (int i = 0; i < baseDrawPoints.Length; i++)
                baseDrawPoints[i] = Vector2.Lerp(projectile.Center, laserEnd, i / (float)(baseDrawPoints.Length - 1f));

            // Select textures to pass to the shader, along with the electricity color.
            GameShaders.Misc["Infernum:ArtemisLaser"].UseColor(Color.Cyan);
            GameShaders.Misc["Infernum:ArtemisLaser"].UseImage("Images/Misc/Perlin");

            LaserDrawer.Draw(baseDrawPoints, -Main.screenPosition, 54);
            return false;
        }

        public override void OnHitPlayer(Player target, int damage, bool crit)
        {
            target.AddBuff(ModContent.BuffType<BrimstoneFlames>(), 300);
        }

        public override bool CanHitPlayer(Player target) => projectile.scale >= 0.5f;
    }
}
