using CalamityMod;
using CalamityMod.DataStructures;
using InfernumMode.DataStructures;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.CalamitasShadow
{
    public class BrimstoneBoomExplosion : ModProjectile, IAdditiveDrawer
    {
        public override string Texture => "CalamityMod/ExtraTextures/XerocLight";

        public override void SetStaticDefaults() => DisplayName.SetDefault("Explosion");

        public override void SetDefaults()
        {
            projectile.width = projectile.height = 520;
            projectile.ignoreWater = false;
            projectile.tileCollide = false;
            projectile.penetrate = -1;
            projectile.timeLeft = 150;
            projectile.MaxUpdates = 3;
            projectile.scale = 0.2f;
            projectile.hide = true;
            cooldownSlot = 1;
        }

        public override void AI()
        {
            // Emit a strong white light.
            Lighting.AddLight(projectile.Center, Color.White.ToVector3() * 1.5f);

            // Determine frames. Once the maximum frame is reached the projectile dies.
            projectile.frameCounter++;
            if (projectile.frameCounter % 8 == 7)
                projectile.frame++;
            if (projectile.frame >= 18)
                projectile.Kill();

            // Exponentially expand.
            projectile.scale *= 1.013f;
            projectile.Opacity = Utils.InverseLerp(5f, 36f, projectile.timeLeft, true);
        }

        public void AdditiveDraw(SpriteBatch spriteBatch)
        {
            Texture2D texture = ModContent.GetTexture("InfernumMode/ExtraTextures/TerratomereExplosion");
            Texture2D lightTexture = ModContent.GetTexture(Texture);
            Rectangle frame = texture.Frame(3, 6, projectile.frame / 6, projectile.frame % 6);
            Vector2 drawPosition = projectile.Center - Main.screenPosition;
            Vector2 origin = frame.Size() * 0.5f;

            for (int i = 0; i < 36; i++)
            {
                Vector2 lightDrawPosition = drawPosition + (MathHelper.TwoPi * i / 36f + Main.GlobalTime * 5f).ToRotationVector2() * projectile.scale * 12f;
                Color lightBurstColor = CalamityUtils.MulticolorLerp(projectile.timeLeft / 144f, Color.OrangeRed, Color.Yellow);
                lightBurstColor = Color.Lerp(lightBurstColor, Color.White, 0.4f) * projectile.Opacity * 0.184f;
                Main.spriteBatch.Draw(lightTexture, lightDrawPosition, null, lightBurstColor, 0f, lightTexture.Size() * 0.5f, projectile.scale * 1.32f, SpriteEffects.None, 0);
            }
            Main.spriteBatch.Draw(texture, drawPosition, frame, Color.Yellow, 0f, origin, 1.4f, SpriteEffects.None, 0);
        }
    }
}
