using CalamityMod.DataStructures;
using InfernumMode.DataStructures;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.Yharon
{
    public class YharonFlameExplosion : ModProjectile, IAdditiveDrawer
    {
        public override string Texture => "CalamityMod/ExtraTextures/XerocLight";

        public override void SetStaticDefaults() => DisplayName.SetDefault("Hyperthermal Explosion");

        public override void SetDefaults()
        {
            projectile.width = projectile.height = 16;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;
            projectile.MaxUpdates = 2;
            projectile.timeLeft = projectile.MaxUpdates * 210;
            projectile.scale = 0.15f;
            cooldownSlot = 1;
        }

        public override void AI()
        {
            projectile.scale += 0.16f;
            projectile.Opacity = Utils.InverseLerp(projectile.MaxUpdates * 300f, projectile.MaxUpdates * 265f, projectile.timeLeft, true) * Utils.InverseLerp(0f, projectile.MaxUpdates * 50f, projectile.timeLeft, true);
            projectile.rotation = projectile.velocity.ToRotation() + MathHelper.PiOver2;

            if (projectile.velocity.Length() < 18f)
                projectile.velocity *= 1.02f;

            Lighting.AddLight(projectile.Center, Color.Orange.ToVector3());
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor) => false;

        public void AdditiveDraw(SpriteBatch spriteBatch)
        {
            Texture2D texture = Main.projectileTexture[projectile.type];
            Color explosionColor = Color.Lerp(Color.Orange, Color.Yellow, 0.5f);
            explosionColor = Color.Lerp(explosionColor, Color.White, projectile.Opacity * 0.2f);
            explosionColor *= projectile.Opacity * 0.5f;
            Vector2 drawPosition = projectile.Center - Main.screenPosition;

            for (int i = 0; i < (int)MathHelper.Lerp(3f, 6f, projectile.Opacity); i++)
                spriteBatch.Draw(texture, drawPosition, null, explosionColor, 0f, texture.Size() * 0.5f, projectile.scale, SpriteEffects.None, 0f);
        }
    }
}
