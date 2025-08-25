using CalamityMod;
using CalamityMod.Events;
using CalamityMod.NPCs.AstrumDeus;
using CalamityMod.Projectiles.BaseProjectiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using Terraria;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.AstrumDeus
{
    public class DarkGodLaser : BaseLaserbeamProjectile
    {
        public int OwnerIndex
        {
            get => (int)projectile.ai[0];
            set => projectile.ai[0] = value;
        }

        public PrimitiveTrail LaserDrawer = null;

        public const int LaserLifetime = 300;
        public override float MaxScale => 1f;
        public override float MaxLaserLength => 3600f;
        public override float Lifetime => LaserLifetime;
        public override Color LaserOverlayColor => new Color(255, 255, 255, 100);
        public override Color LightCastColor => Color.White;
        public override Texture2D LaserBeginTexture => Main.projectileTexture[projectile.type];
        public override Texture2D LaserMiddleTexture => ModContent.GetTexture("CalamityMod/ExtraTextures/Lasers/AresLaserBeamMiddle");
        public override Texture2D LaserEndTexture => ModContent.GetTexture("CalamityMod/ExtraTextures/Lasers/AresLaserBeamEnd");
        public override string Texture => "CalamityMod/Projectiles/Boss/AresLaserBeamStart";

        public override void SetStaticDefaults() => DisplayName.SetDefault("Antimatter Deathray");

        public override void SetDefaults()
        {
            projectile.width = 54;
            projectile.height = 54;
            projectile.hostile = true;
            projectile.alpha = 255;
            projectile.penetrate = -1;
            projectile.tileCollide = false;
            projectile.timeLeft = LaserLifetime;
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
            // Die if Deus is not present.
            if (!NPC.AnyNPCs(ModContent.NPCType<AstrumDeusHeadSpectral>()))
            {
                projectile.Kill();
                return;
            }
        }

        public override float DetermineLaserLength() => MaxLaserLength;

        public override void UpdateLaserMotion()
        {
            float spinSpeed = Utils.InverseLerp(0f, 60f, Time, true) * 0.016f;
            if (BossRushEvent.BossRushActive)
                spinSpeed *= 1.64f;

            projectile.velocity = projectile.velocity.RotatedBy(spinSpeed);
        }

        public override void PostAI()
        {
            // Determine scale.
            Time = Lifetime - projectile.timeLeft;
            projectile.scale = CalamityUtils.Convert01To010(Time / Lifetime) * MaxScale * 3f;
            if (projectile.scale > MaxScale)
                projectile.scale = MaxScale;
        }

        public float LaserWidthFunction(float _) => projectile.scale * projectile.width;

        public static Color LaserColorFunction(float completionRatio)
        {
            float colorInterpolant = (float)Math.Sin(Main.GlobalTime * -1.23f + completionRatio * 23f) * 0.5f + 0.5f;
            return Color.Lerp(Color.Black, Color.Cyan, (float)Math.Pow(colorInterpolant, 3.3) * 0.25f);
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
            GameShaders.Misc["Infernum:ArtemisLaser"].UseColor(Color.Turquoise);
            GameShaders.Misc["Infernum:ArtemisLaser"].SetShaderTexture(ModContent.GetTexture("CalamityMod/ExtraTextures/ScarletDevilStreak"));
            Main.instance.GraphicsDevice.Textures[2] = ModContent.GetTexture("Terraria/Misc/Perlin");

            int pointCount = InfernumConfig.Instance.ReducedGraphicsConfig ? 10 : 25;
            LaserDrawer.Draw(baseDrawPoints, -Main.screenPosition, pointCount);
            return false;
        }

        public override bool CanHitPlayer(Player target) => projectile.scale >= 0.5f;
    }
}
