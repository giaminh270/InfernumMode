using CalamityMod;
using CalamityMod.DataStructures;
using InfernumMode.DataStructures;
using InfernumMode.ExtraTextures;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.AquaticScourge
{
    public class SulphuricGas : ModProjectile, IAdditiveDrawer
    {
        public ref float LightPower => ref projectile.ai[0];

        public ref float IdealScale => ref projectile.ai[1];

        public static int Lifetime => 120;

        public override string Texture => "InfernumMode/ExtraTextures/GreyscaleObjects/NebulaGas1";

        public override void SetStaticDefaults() => DisplayName.SetDefault("Sulphuric Acid Gas");

        public override void SetDefaults()
        {
            projectile.width = projectile.height = 50;
            projectile.penetrate = -1;
            projectile.tileCollide = false;
            projectile.timeLeft = Lifetime;
            projectile.scale = 0.03f;
            projectile.hide = true;
            projectile.hostile = true;
            projectile.ignoreWater = true;
            projectile.Calamity().canBreakPlayerDefense = true;
            
            cooldownSlot = 1;
        }

        public override void AI()
        {
            // Decide scale and initial rotation on the first frame this projectile exists.
            if (IdealScale == 0f)
            {
                IdealScale = Main.rand.NextFloat(4f, 5.5f);
                projectile.rotation = Main.rand.NextFloat(MathHelper.TwoPi);
                projectile.netUpdate = true;
            }

            // Grow in scale.
            float idealScale = IdealScale + Utilities.Remap(projectile.timeLeft, 60f, 0f, 0f, 10f);
            projectile.scale = MathHelper.Lerp(projectile.scale, idealScale, 0.064f);

            // Calculate light power. This checks below the position of the fog to check if this fog is underground.
            // Without this, it may render over the fullblack that the game renders for obscured tiles.
            float lightPowerBelow = Lighting.GetColor((int)projectile.Center.X / 16, (int)projectile.Center.Y / 16 + 6).ToVector3().Length() / (float)Math.Sqrt(3f);
            if (CalamityUtils.ParanoidTileRetrieval((int)projectile.Center.X / 16, (int)projectile.Center.Y / 16).liquid >= 25)
                lightPowerBelow = 1f;

            LightPower = MathHelper.Lerp(LightPower, lightPowerBelow, 0.15f);
            projectile.Opacity = Utils.InverseLerp(Lifetime, Lifetime - 20f, projectile.timeLeft, true) * Utils.InverseLerp(0f, 40f, projectile.timeLeft, true) * 0.675f;
            projectile.rotation += projectile.velocity.X * 0.002f;
            projectile.velocity *= 0.985f;
        }

        public override bool CanDamage() => projectile.Opacity > 0.56f;

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor) => false;

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            return Utilities.CircularCollision(projectile.Center, targetHitbox, projectile.scale * 30f);
        }

        public void AdditiveDraw(SpriteBatch spriteBatch)
        {
            Vector2 screenArea = new Vector2(Main.screenWidth, Main.screenHeight);
            Rectangle screenRectangle = Utils.CenteredRectangle(Main.screenPosition + screenArea * 0.5f, screenArea * 1.33f);

            if (!projectile.Hitbox.Intersects(screenRectangle))
                return;

            // Decide which gas texture to use.
            Texture2D texture = Main.projectileTexture[projectile.type];
            if (projectile.identity % 2 == 1)
                texture = InfernumTextureRegistry.Cloud2;

            // Calculate drawing variables for the mist.
            Vector2 origin = texture.Size() * 0.5f;
            Vector2 drawPosition = projectile.Center - Main.screenPosition;
            float opacity = Utils.InverseLerp(0f, 0.08f, LightPower, true) * projectile.Opacity;

            int b = 160 + (int)(Math.Sin(MathHelper.Pi * projectile.identity / 8f + Main.GlobalTime * 10f) * 80f);
            Color drawColor = new Color(141, 255, b) * opacity;
            Vector2 scale = Vector2.One * 50f / texture.Size() * projectile.scale * 1.35f;
            spriteBatch.Draw(texture, drawPosition, null, drawColor, projectile.rotation, origin, scale, SpriteEffects.None, 0f);
        }
    }
}
