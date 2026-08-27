using CalamityMod;
using InfernumMode.Effects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.Draedon.Ares
{
    public class AresEnergySlash : ModProjectile
    {
        public Vector2[] ControlPoints;

        public PrimitiveTrail SlashDrawer
        {
            get;
            set;
        }

        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Exo Energy Slash");
        }

        public override void SetDefaults()
        {
            projectile.width = 180;
            projectile.height = 180;
            projectile.hostile = true;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;
            projectile.melee = true;
            projectile.penetrate = -1;
            projectile.timeLeft = 60;
            projectile.tileCollide = false;
        }

        public override void AI()
        {
            projectile.Opacity = Utils.InverseLerp(0f, 56f, projectile.timeLeft, true);
            projectile.velocity *= 1.06f;

            if (projectile.timeLeft >= 30)
                projectile.scale *= 1.033f;
        }

        public float SlashWidthFunction(float completionRatio) => Utils.InverseLerp(0f, 0.35f, completionRatio, true) * Utils.InverseLerp(1f, 0.65f, completionRatio, true) * projectile.scale * 35f;

        public Color SlashColorFunction(float completionRatio) => Color.Red* Utils.InverseLerp(0.04f, 0.27f, completionRatio, true) * projectile.Opacity * projectile.localAI[1];

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            // Initialize the drawer.
			if (SlashDrawer == null)
				SlashDrawer = new PrimitiveTrail(SlashWidthFunction, SlashColorFunction, null, InfernumEffectsRegistry.AresEnergySlashShader);

            // Draw the slash effect.
            Main.spriteBatch.EnterShaderRegion();

            List<Vector2> points = new List<Vector2>();
            Vector2 direction = projectile.velocity.SafeNormalize(Vector2.UnitY);
            Vector2 perpendicularDirection = direction.RotatedBy(MathHelper.PiOver2);
            Vector2 left = projectile.Center - perpendicularDirection * projectile.height * projectile.scale * 0.5f;
            Vector2 right = projectile.Center + perpendicularDirection * projectile.height * projectile.scale * 0.5f;
            Vector2 middle = projectile.Center + direction * projectile.height / projectile.scale * 2f;
            for (int i = 0; i < 15; i++)
                points.Add(Utilities.QuadraticBezier(left, middle, right, i / 14f));

            InfernumEffectsRegistry.AresEnergySlashShader.SetShaderTexture(ModContent.GetTexture("CalamityMod/ExtraTextures/VoronoiShapes"));
            InfernumEffectsRegistry.AresEnergySlashShader.SetShaderTexture(ModContent.GetTexture("CalamityMod/ExtraTextures/SwordSlashTexture"));

            for (projectile.localAI[1] = 1f; projectile.localAI[1] > 0f; projectile.localAI[1] -= 0.33f)
                SlashDrawer.Draw(points, direction * -60f - Main.screenPosition, 43);

            Main.spriteBatch.ExitShaderRegion();
            return false;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => projectile.RotatingHitboxCollision(targetHitbox.TopLeft(), targetHitbox.Size());
    }
}
