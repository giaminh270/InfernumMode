using CalamityMod.NPCs;
using InfernumMode.Effects;
using InfernumMode.ExtraTextures;
using InfernumMode.Graphics.Interfaces;
using InfernumMode.Graphics.Primitives;
using InfernumMode.GlobalInstances;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.ProfanedGuardians
{
    public class HolyFireWall : ModProjectile, IPixelPrimitiveDrawer
    {
		public bool DrawBeforeNPCs => false;
		
        public PrimitiveTrailCopy FlameDrawer { get; private set; }

        public int Lifetime = BaseLifetime;

        public const int BaseLifetime = 650;

        public bool SlowerFadeIn => projectile.ai[0] == 1;

        public override string Texture => InfernumTextureRegistry.InvisPath;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Fire Wall");
        }

        public override void SetDefaults()
        {
            projectile.width = 200;
            projectile.height = 1000;
            projectile.hostile = true;
            projectile.friendly = false;
            projectile.tileCollide = false;
            projectile.ignoreWater = false;
            projectile.Opacity = 0;
            projectile.scale = 0;
            projectile.timeLeft = Lifetime;
            cooldownSlot = 1;
        }

        public override void AI()
        {
            // Do not exist if the commander does not.
            if (CalamityGlobalNPC.doughnutBoss == -1)
            {
                projectile.Kill();
                return;
            }

            if (projectile.localAI[0] == 0f)
            {
                projectile.timeLeft = Lifetime;
                projectile.localAI[0] = 1f;
            }
            // Rapidly fade in.
            if (projectile.timeLeft >= Lifetime - 60)
            {
                projectile.Opacity = MathHelper.Clamp(projectile.Opacity + (SlowerFadeIn ? 0.01f : 0.025f), 0f, 1f);
                projectile.scale = MathHelper.Clamp(projectile.scale + (SlowerFadeIn ? 0.01f : 0.025f), 0f, 1f);
            }

            // Fade out.
            if (projectile.timeLeft <= 40)
            {
                projectile.Opacity = MathHelper.Clamp(projectile.Opacity - 0.025f, 0f, 1f);
                projectile.scale = MathHelper.Clamp(projectile.scale - 0.025f, 0f, 1f);
            }
        }

        // Only hit the player if faded in enough and the crystal wall is active.
        public override bool CanHitPlayer(Player target) => (projectile.timeLeft >= 40 || projectile.scale >= 0.85f) && GlobalNPCOverrides.ProfanedCrystal != -1;

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float _ = 0f;
            float width = 120f * projectile.scale;
            Vector2 topStart = projectile.Center - new Vector2(0, 130);
            Vector2 topEnd = topStart - Vector2.UnitY * (2000 - 80f);
            if (Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), topStart, topEnd, width, ref _))
                return true;

            Vector2 bottomStart = projectile.Center + new Vector2(0, 130);
            Vector2 bottomEnd = bottomStart + Vector2.UnitY * (2000 - 80f);
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), bottomStart, bottomEnd, width, ref _);
        }

        public float WidthFunction(float completionRatio) => 150f * projectile.scale;

        public Color ColorFunction(float completionRatio) => new Color(255, 191, 73) * MathHelper.Clamp(projectile.Opacity * 2f, 0.1f, 1f);

        public void DrawPixelPrimitives(SpriteBatch spriteBatch)
        {
            if (FlameDrawer == null)
                FlameDrawer = new PrimitiveTrailCopy(WidthFunction, ColorFunction, null, true, InfernumEffectsRegistry.GenericLaserVertexShader);

            // The gap is determined by the projectile center, and thus controlled by the attacking guardian.
            // Draw a set distance above and below the center to give a gap in the wall.
            float laserDistance = 2000f;
            Vector2 topBaseDrawPos = projectile.Center + new Vector2(0f, -75f);
            Vector2[] topDrawPoints = new Vector2[8];
            for (int i = 0; i < topDrawPoints.Length; i++)
                topDrawPoints[i] = Vector2.Lerp(topBaseDrawPos, new Vector2(topBaseDrawPos.X, topBaseDrawPos.Y - laserDistance), (float)i / topDrawPoints.Length);

            Vector2 bottomBaseDrawPos = projectile.Center + new Vector2(0f, 75f);
            Vector2[] bottomDrawPoints = new Vector2[8];
            for (int i = 0; i < bottomDrawPoints.Length; i++)
                bottomDrawPoints[i] = Vector2.Lerp(bottomBaseDrawPos, new Vector2(bottomBaseDrawPos.X, bottomBaseDrawPos.Y + laserDistance), (float)i / bottomDrawPoints.Length);

            InfernumEffectsRegistry.GenericLaserVertexShader.SetShaderTexture(InfernumTextureRegistry.HarshNoise);
            InfernumEffectsRegistry.GenericLaserVertexShader.UseColor(new Color(255, 255, 150) * MathHelper.Clamp(projectile.Opacity * 2f, 0.1f, 1f));
            InfernumEffectsRegistry.GenericLaserVertexShader.Shader.Parameters["strongerFade"].SetValue(true);

            FlameDrawer.DrawPixelated(topDrawPoints, -Main.screenPosition, 20);

            FlameDrawer.DrawPixelated(bottomDrawPoints, -Main.screenPosition, 20);
        }
    }
}
