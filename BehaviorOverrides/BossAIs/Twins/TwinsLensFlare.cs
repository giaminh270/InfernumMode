using CalamityMod;
using CalamityMod.DataStructures;
using InfernumMode.DataStructures;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.Twins
{
    public class TwinsLensFlare : ModProjectile, IAdditiveDrawer
    {
        public bool SpazmatismVariant
        {
            get => projectile.ai[1] == 1f;
            set => projectile.ai[1] = value.ToInt();
        }

        public const int Lifetime = 45;

        public override string Texture => "InfernumMode/ExtraTextures/GreyscaleObjects/LargeStar";

        public override void SetStaticDefaults() => DisplayName.SetDefault("Lens Flare");

        public override void SetDefaults()
        {
            projectile.width = projectile.height = 6;
            projectile.ignoreWater = true;
            projectile.tileCollide = false;
            projectile.penetrate = -1;
            projectile.timeLeft = Lifetime;
            cooldownSlot = 1;
        }

        public override void AI()
        {
            projectile.scale = CalamityUtils.Convert01To010(projectile.timeLeft / (float)Lifetime) * 1.67f;
        }

        public void AdditiveDraw(SpriteBatch spriteBatch)
        {
            Texture2D texture = ModContent.GetTexture(Texture);
            Vector2 drawPosition = projectile.Center - Main.screenPosition;

            for (float scale = 1f; scale > 0.3f; scale -= 0.1f)
            {
                Color c = Color.Lerp(projectile.GetAlpha(Color.White), Color.White, 1f - scale);
                spriteBatch.Draw(texture, drawPosition, null, c, projectile.rotation, texture.Size() * 0.5f, projectile.scale * scale, 0, 0f);
                spriteBatch.Draw(texture, drawPosition, null, c, projectile.rotation, texture.Size() * 0.5f, projectile.scale * new Vector2(4f, 0.2f) * scale, 0, 0f);
            }
        }

        public override Color? GetAlpha(Color lightColor) => SpazmatismVariant ? Color.Lime : Color.Red;

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor) => false;
    }
}
