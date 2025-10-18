using CalamityMod;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.SupremeCalamitas
{
    public class DemonicExplosion : ModProjectile
    {
        public float MaxRadius;

        public PrimitiveTrailCopy FireDrawer;

        public ref float Time => ref projectile.ai[0];
        
        public ref float Radius => ref projectile.ai[1];
        
        public override void SetStaticDefaults() => DisplayName.SetDefault("Demonic Explosion");

        public override void SetDefaults()
        {
            projectile.width = projectile.height = 8;
            projectile.hostile = true;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;
            projectile.penetrate = -1;
            projectile.timeLeft = 84;
            projectile.MaxUpdates = 2;
            projectile.scale = 1f;
            projectile.hide = true;
            projectile.Calamity().canBreakPlayerDefense = true;
        }

        public override void AI()
        {
            projectile.scale += 0.08f;
            Radius = MathHelper.Lerp(Radius, MaxRadius, 0.1f);
            projectile.Opacity = Utils.InverseLerp(8f, 42f, projectile.timeLeft, true) * 0.55f;

            Time++;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => Utilities.CircularCollision(targetHitbox.Center.ToVector2(), projHitbox, Radius * 0.8f);

        public float SunWidthFunction(float completionRatio) => Radius * (float)Math.Sin(MathHelper.Pi * completionRatio);

        public Color SunColorFunction(float completionRatio)
        {
            Color sunColor = Main.dayTime ? Color.Yellow : Color.Cyan;
            return Color.Lerp(sunColor, Color.White, (float)Math.Sin(MathHelper.Pi * completionRatio) * 0.5f + 0.3f) * projectile.Opacity;
        }

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers)
        {
            behindNPCs.Add(index);
        }

		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
		{
			if (InfernumConfig.Instance.ReducedGraphicsConfig)
			{
				OptimizedDraw();
				return false;
			}

			DefaultDraw();
			return false;
		}

		public void DefaultDraw()
		{
			if (FireDrawer is null)
				FireDrawer = new PrimitiveTrailCopy(SunWidthFunction, SunColorFunction, null, true, GameShaders.Misc["Infernum:Fire"]);

			GameShaders.Misc["Infernum:Fire"].UseSaturation(0.45f);
			GameShaders.Misc["Infernum:Fire"].UseImage("Images/Misc/Perlin");

			List<float> rotationPoints = new List<float>();
			List<Vector2> drawPoints = new List<Vector2>();

			for (float offsetAngle = -MathHelper.PiOver2; offsetAngle <= MathHelper.PiOver2; offsetAngle += MathHelper.Pi / 10f)
			{
				rotationPoints.Clear();
				drawPoints.Clear();

				float adjustedAngle = offsetAngle + MathHelper.Pi * -0.2f;
				Vector2 offsetDirection = adjustedAngle.ToRotationVector2();
				for (int i = 0; i < 16; i++)
				{
					rotationPoints.Add(adjustedAngle);
					drawPoints.Add(Vector2.Lerp(projectile.Center - offsetDirection * Radius / 2f, projectile.Center + offsetDirection * Radius / 2f, i / 16f));
				}

				FireDrawer.Draw(drawPoints, -Main.screenPosition, 14);
			}
		}

		public void OptimizedDraw()
		{
			Texture2D circleTexture = ModContent.GetTexture("InfernumMode/ExtraTextures/LaserCircle");
			Vector2 drawPosition = projectile.Center - Main.screenPosition;
			float scale = Radius * 2f / circleTexture.Width;
			Color color = Color.Lerp(Color.Purple, Color.Red, 0.5f) * projectile.Opacity * 0.7f;
			
			Main.spriteBatch.Draw(circleTexture, drawPosition, null, color, 0f, circleTexture.Size() * 0.5f, scale, SpriteEffects.None, 0f);
			
			Color coreColor = Color.White * projectile.Opacity * 0.8f;
			Main.spriteBatch.Draw(circleTexture, drawPosition, null, coreColor, 0f, circleTexture.Size() * 0.5f, scale * 0.3f, SpriteEffects.None, 0f);
		}
    }
}
