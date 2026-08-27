using CalamityMod;
using CalamityMod.Projectiles.BaseProjectiles;
using InfernumMode.Effects;
using InfernumMode.Graphics.Interfaces;
using InfernumMode.Graphics.Primitives;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.Twins
{
    public class RetinazerAimedDeathray : BaseLaserbeamProjectile, IPixelPrimitiveDrawer
    {
        public bool DrawBeforeNPCs => false;
        public PrimitiveTrailCopy LaserDrawer
        {
            get;
            set;
        }

        public const int LifetimeConst = 27;

        public const float LaserLengthConst = 2820f;

        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public override float MaxScale => 1.05f;

        public override float MaxLaserLength => LaserLengthConst;

        public override float Lifetime => LifetimeConst;

        public override Color LaserOverlayColor => Color.Lerp(Color.IndianRed, Color.Red, 0.6f) * 1.2f;

        public override Color LightCastColor => LaserOverlayColor;

        public override Texture2D LaserBeginTexture => ModContent.GetTexture("CalamityMod/ExtraTextures/Lasers/UltimaRayStart");

        public override Texture2D LaserMiddleTexture => ModContent.GetTexture("CalamityMod/ExtraTextures/Lasers/UltimaRayMid");

        public override Texture2D LaserEndTexture => ModContent.GetTexture("CalamityMod/ExtraTextures/Lasers/UltimaRayEnd");

        // To allow easy, static access from different locations.
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Deathray");
        }

        public override void SetDefaults()
        {
            projectile.width = projectile.height = 44;
            projectile.hostile = true;
            projectile.tileCollide = false;
            projectile.penetrate = -1;
            projectile.alpha = 255;
            projectile.Calamity().canBreakPlayerDefense = true;
        }

        public override void AttachToSomething()
        {
            if (!Main.npc.IndexInRange((int)projectile.ai[1]) || !Main.npc[(int)projectile.ai[1]].active)
                projectile.Kill();

            projectile.velocity = (Main.npc[(int)projectile.ai[1]].rotation + MathHelper.PiOver2).ToRotationVector2();
            projectile.Center = Main.npc[(int)projectile.ai[1]].Center + projectile.velocity * 40f;
        }

        public override void Kill(int timeLeft)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            for (float i = 0f; i < LaserLength; i += 180f)
            {
                for (int direction = -1; direction <= 1; direction += 2)
                {
                    Vector2 shootVelocity = projectile.velocity.RotatedBy(MathHelper.PiOver2 * direction) * 4.2f;
                    Utilities.NewProjectileBetter(projectile.Center + projectile.velocity * i, shootVelocity, ModContent.ProjectileType<RetinazerLaser>(), TwinsAttackSynchronizer.SmallLaserDamage, 0f);
                }
            }
        }

        public float LaserWidthFunction(float _) => projectile.scale * projectile.width * projectile.localAI[1] * 0.5f;

        public Color LaserColorFunction(float completionRatio)
        {
            float colorInterpolant = CalamityUtils.Convert01To010(Time / Lifetime) * 0.45f + 0.15f;
            colorInterpolant = MathHelper.Lerp(colorInterpolant, 1f, 1f - 1f / projectile.localAI[1]);

            return Color.Lerp(Color.Red, Color.White, colorInterpolant) * (1f / projectile.localAI[1]);
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor) => false;

        public void DrawPixelPrimitives(SpriteBatch spriteBatch)
        {
            // This should never happen, but just in case.
            if (projectile.velocity == Vector2.Zero)
                return;

            // Initialize the laser drawer.
            if (LaserDrawer is null)
                LaserDrawer = new PrimitiveTrailCopy(LaserWidthFunction, LaserColorFunction, null, true, InfernumEffectsRegistry.ArtemisLaserVertexShader);

            Vector2 laserEnd = projectile.Center + projectile.velocity.SafeNormalize(Vector2.UnitY) * LaserLength;
            Vector2[] baseDrawPoints = new Vector2[8];
            for (int i = 0; i < baseDrawPoints.Length; i++)
                baseDrawPoints[i] = Vector2.Lerp(projectile.Center, laserEnd, i / (float)(baseDrawPoints.Length - 1f));

            // Select textures to pass to the shader, along with the electricity color.
            InfernumEffectsRegistry.ArtemisLaserVertexShader.UseColor(Color.White);
            InfernumEffectsRegistry.ArtemisLaserVertexShader.UseImage("Images/Extra_197");
            InfernumEffectsRegistry.ArtemisLaserVertexShader.UseImage("Images/Misc/Perlin");

            float oldLocalAI = projectile.localAI[1];
            for (float scaleFactor = 3f; scaleFactor >= 1f; scaleFactor -= 0.6f)
            {
                projectile.localAI[1] = scaleFactor;
                LaserDrawer.DrawPixelated(baseDrawPoints, -Main.screenPosition, 54);
            }
            projectile.localAI[1] = oldLocalAI;
        }

        public override void DetermineScale() => projectile.scale = CalamityUtils.Convert01To010(Time / Lifetime);

        public override bool ShouldUpdatePosition() => false;
    }
}
