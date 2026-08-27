using CalamityMod.DataStructures;
using InfernumMode.ExtraTextures;
using InfernumMode;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using InfernumMode.DataStructures;

namespace InfernumMode.BehaviorOverrides.BossAIs.Draedon.Ares
{
    public class AresEnergyDeathrayTelegraph : ModProjectile, IAdditiveDrawer
    {
        public float LifetimeCompletion => 1f - projectile.timeLeft / (float)Lifetime;

        public static int Lifetime => 27;

        public override string Texture => InfernumTextureRegistry.InvisPath;

        public override void SetStaticDefaults() => DisplayName.SetDefault("Exo Energy Burst Telegraph");

        public override void SetDefaults()
        {
            projectile.width = 2;
            projectile.height = 2;
            projectile.hostile = true;
            projectile.ignoreWater = true;
            projectile.tileCollide = false;
            projectile.Opacity = 0f;
            projectile.penetrate = -1;
            projectile.timeLeft = Lifetime;
            cooldownSlot = 1;
        }

        public override void AI()
        {
            // Rapidly fade in.
            projectile.Opacity = MathHelper.Clamp(projectile.Opacity + 0.15f, 0f, 1f);
        }

        public override void Kill(int timeLeft)
        {
            Main.LocalPlayer.Infernum().CurrentScreenShakePower = 12f;
            ScreenEffectSystem.SetBlurEffect(projectile.Center, 0.3f, 16);
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            Utilities.NewProjectileBetter(projectile.Center + projectile.velocity * 80f, projectile.velocity, ModContent.ProjectileType<AresEnergyDeathray>(), DraedonBehaviorOverride.PowerfulShotDamage, 0f);
        }

        public override bool ShouldUpdatePosition() => false;

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor) => false;

        public void AdditiveDraw(SpriteBatch spriteBatch)
        {
            float opacity = Utils.InverseLerp(1f, 0.75f, LifetimeCompletion, true) * projectile.Opacity;
            Vector2 start = projectile.Center;
            Vector2 end = start + projectile.velocity * 4000f;
            spriteBatch.DrawBloomLine(start, end, Color.Lerp(Color.Red, Color.Wheat, LifetimeCompletion) * opacity, LifetimeCompletion * 15f + 20f);
        }
    }
}
