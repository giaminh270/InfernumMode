using InfernumMode.Effects;
using InfernumMode.ExtraTextures;
using InfernumMode.Graphics.Interfaces;
using InfernumMode.Graphics.Primitives;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.ProfanedGuardians
{
    public class HolyPushbackWall : ModProjectile, IPixelPrimitiveDrawer
    {
		public bool DrawBeforeNPCs => false;
		
        public PrimitiveTrailCopy FlameDrawer { get; private set; }

        public ref float Timer => ref projectile.ai[0];

        public const float LaserDistance = 6000;

        public static int Lifetime => 660;

        public int SpearReleaseRate => 15;

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
            // Rapidly fade in.
            if (projectile.timeLeft >= Lifetime - 100)
            {
                projectile.Opacity = MathHelper.Clamp(projectile.Opacity + 0.025f, 0f, 1f);
                projectile.scale = MathHelper.Clamp(projectile.scale + 0.025f, 0f, 1f);
            }

            // Fade out.
            if (projectile.timeLeft <= 40)
            {
                projectile.Opacity = MathHelper.Clamp(projectile.Opacity - 0.025f, 0f, 1f);
                projectile.scale = MathHelper.Clamp(projectile.scale - 0.025f, 0f, 1f);
            }

            // Force anyone close to it to be to the left.
            foreach (Player player in Main.player)
                if (player.active && !player.dead && Vector2.Distance(player.Center, projectile.Center) <= 6000f)
                    if (player.Center.X > projectile.Center.X)
                        player.Center = new Vector2(projectile.Center.X, player.Center.Y);

            if (projectile.Opacity == 1f)
            {
                if (Timer % SpearReleaseRate == 0f && Main.netMode != NetmodeID.MultiplayerClient)
                {
                    Vector2 velocity = -Vector2.UnitX * 10f;
                    Vector2 position = new Vector2(projectile.Center.X, projectile.Center.Y + Main.rand.NextFloat(-800f, 800f));
                    Utilities.NewProjectileBetter(position, velocity, ModContent.ProjectileType<TelegraphedProfanedSpearInfernum>(), 200, 0f, ai1: projectile.whoAmI);
                }
            }
            Timer++;
        }

        public override bool CanHitPlayer(Player target) => projectile.Opacity >= 0.75f;

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            Vector2 drawPos = projectile.Center - new Vector2(0f, LaserDistance / 2);
            Vector2 endPos = projectile.Center + new Vector2(0f, LaserDistance / 2);
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), drawPos, endPos);

        }

        public float WidthFunction(float completionRatio) => 200 * projectile.scale;

        public Color ColorFunction(float completionRatio) => new Color(255, 191, 73) * projectile.Opacity;

        public void DrawPixelPrimitives(SpriteBatch spriteBatch)
        {
            if (FlameDrawer == null)
                FlameDrawer = new PrimitiveTrailCopy(WidthFunction, ColorFunction, null, true, InfernumEffectsRegistry.GenericLaserVertexShader);

            // The gap is determined by the projectile center, and thus controlled by the attacking guardian.
            // Draw a set distance above and below the center to give a gap in the wall.
            Vector2 drawPos = projectile.Center - new Vector2(0f, LaserDistance / 2);
            Vector2 endPos = projectile.Center + new Vector2(0f, LaserDistance / 2);
            Vector2[] topDrawPoints = new Vector2[8];
            for (int i = 0; i < topDrawPoints.Length; i++)
                topDrawPoints[i] = Vector2.Lerp(drawPos, endPos, (float)i / topDrawPoints.Length);

            InfernumEffectsRegistry.GenericLaserVertexShader.SetShaderTexture(InfernumTextureRegistry.CrustyNoise);
            InfernumEffectsRegistry.GenericLaserVertexShader.UseColor(new Color(255, 255, 150) * projectile.Opacity);
            InfernumEffectsRegistry.GenericLaserVertexShader.Shader.Parameters["strongerFade"].SetValue(true);

            FlameDrawer.DrawPixelated(topDrawPoints, -Main.screenPosition, 40);
        }
    }
}
