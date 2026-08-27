using CalamityMod;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Projectiles.BaseProjectiles;
using InfernumMode.Effects;
using InfernumMode.ExtraTextures;
using InfernumMode.Graphics.Interfaces;
using InfernumMode.Graphics.Primitives;
using InfernumMode.GlobalInstances;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using Terraria;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.AstrumAureus
{
    public class OrangeLaserbeam : BaseLaserbeamProjectile, IPixelPrimitiveDrawer
    {
		public bool DrawBeforeNPCs => false;
		
        public PrimitiveTrailCopy LaserDrawer
        {
            get;
            set;
        }

        public int OwnerIndex => (int)projectile.ai[1];
        public const int LaserLifetime = 132;
        public const float FullCircleRotationFactor = 0.84f;
        public override float Lifetime => LaserLifetime;
        public override Color LaserOverlayColor => Color.White;
        public override Color LightCastColor => Color.Orange;
        public override Texture2D LaserBeginTexture => Main.projectileTexture[projectile.type];
        public override Texture2D LaserMiddleTexture => ModContent.GetTexture("InfernumMode/ExtraTextures/Lasers/OrangeLaserbeamMid");
        public override Texture2D LaserEndTexture => ModContent.GetTexture("InfernumMode/ExtraTextures/Lasers/OrangeLaserbeamEnd");
        public override float MaxLaserLength => 3100f;
        public override float MaxScale => 1f;
        public override void SetStaticDefaults() => DisplayName.SetDefault("Astral Deathray");

        public override void SetDefaults()
        {
            projectile.width = projectile.height = 17;
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
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            projectile.localAI[0] = reader.ReadSingle();
            projectile.localAI[1] = reader.ReadSingle();
        }
        public override void AttachToSomething()
        {
            if (!Main.npc.IndexInRange(GlobalNPCOverrides.AstrumAureus))
            {
                projectile.Kill();
                return;
            }

            projectile.Center = Main.npc[GlobalNPCOverrides.AstrumAureus].Center - Vector2.UnitY * 12f;
            projectile.Opacity = 1f;
            RotationalSpeed = MathHelper.Pi / Lifetime * FullCircleRotationFactor;
        }

        public override bool CanDamage() => Time > 35f;
        public float LaserWidthFunction(float _) => projectile.scale * projectile.width * 2;

        public static Color LaserColorFunction(float completionRatio)
        {
            float colorInterpolant = (float)Math.Sin(Main.GlobalTime * -3.2f + completionRatio * 23f) * 0.5f + 0.5f;
            return Color.Lerp(new Color(255, 164, 94), new Color(237, 93, 83), colorInterpolant * 0.67f);
        }

		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor) => false;

        public void DrawPixelPrimitives(SpriteBatch spriteBatch)
        {
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
            InfernumEffectsRegistry.ArtemisLaserVertexShader.UseColor(187, 220, 237);
            InfernumEffectsRegistry.ArtemisLaserVertexShader.SetShaderTexture(InfernumTextureRegistry.StreakThickGlow);
            InfernumEffectsRegistry.ArtemisLaserVertexShader.UseImage("Images/Misc/Perlin");

            LaserDrawer.DrawPixelated(baseDrawPoints, -Main.screenPosition, 54);
        }
		public override void OnHitPlayer(Player target, int damage, bool crit) => target.AddBuff(ModContent.BuffType<AstralInfectionDebuff>(), 300);
    }
}
