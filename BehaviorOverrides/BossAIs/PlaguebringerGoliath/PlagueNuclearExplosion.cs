using CalamityMod;
using CalamityMod.DataStructures;
using InfernumMode.DataStructures;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.PlaguebringerGoliath
{
    public class PlagueNuclearExplosion : ModProjectile, IAdditiveDrawer
    {
        public override string Texture => "CalamityMod/ExtraTextures/XerocLight";

        public override void SetStaticDefaults() => DisplayName.SetDefault("Explosion");

        public override void SetDefaults()
        {
            projectile.width = projectile.height = 14;
            projectile.hostile = true;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;
            projectile.timeLeft = 180;
            projectile.extraUpdates = 1;
            projectile.scale = 0.15f;
            projectile.Calamity().canBreakPlayerDefense = true;
            cooldownSlot = 1;
        }

        public override void AI()
        {
            projectile.scale += 0.1f;
            projectile.Opacity = Utils.InverseLerp(300f, 265f, projectile.timeLeft, true) * Utils.InverseLerp(0f, 50f, projectile.timeLeft, true);
            projectile.rotation = projectile.velocity.ToRotation() + MathHelper.PiOver2;

            if (projectile.velocity.Length() < 18f)
                projectile.velocity *= 1.02f;

            Lighting.AddLight(projectile.Center, Color.Red.ToVector3());
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor) => false;

        public void AdditiveDraw(SpriteBatch spriteBatch)
        {
            int drawCount = InfernumConfig.Instance.ReducedGraphicsConfig ? 1 : 3;
            Texture2D texture = Main.projectileTexture[projectile.type];
            Color explosionColor = Color.LawnGreen * projectile.Opacity * 0.65f;
            Vector2 drawPosition = projectile.Center - Main.screenPosition;

            for (int i = 0; i < drawCount; i++)
                Main.spriteBatch.Draw(texture, drawPosition, null, explosionColor, 0f, texture.Size() * 0.5f, projectile.scale, SpriteEffects.None, 0f);
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            return Utilities.CircularCollision(projectile.Center, targetHitbox, projectile.scale * 135f);
        }

        public override bool CanDamage() => projectile.Opacity > 0.45f;
    }
}
