using InfernumMode.ExtraTextures;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.Twins
{
    public class LaserGroundShock : ModProjectile
    {
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Ground Shock");
        }

        public override void SetDefaults()
        {
            projectile.width = projectile.height = 2;
            projectile.hostile = true;
            projectile.ignoreWater = true;
            projectile.tileCollide = false;
            projectile.penetrate = -1;
            projectile.timeLeft = 27;
            projectile.hide = true;
        }

        public override void AI()
        {
            projectile.Opacity = Utils.InverseLerp(0f, 15f, projectile.timeLeft, true);
            projectile.scale = projectile.Opacity;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Main.spriteBatch.SetBlendState(BlendState.Additive);
            Vector2 drawPosition = projectile.Center - Main.screenPosition + Vector2.UnitY * 24f;
            Texture2D zap = InfernumTextureRegistry.StreakLightning;
            Texture2D backglowTexture = ModContent.GetTexture("CalamityMod/ExtraTextures/XerocLight");

            // Draw an orange backglow.
            Main.spriteBatch.Draw(backglowTexture, drawPosition, null, Color.LightGoldenrodYellow * projectile.Opacity, 0f, backglowTexture.Size() * 0.5f, projectile.scale * 0.56f, 0, 0f);
            Main.spriteBatch.Draw(backglowTexture, drawPosition, null, Color.Orange * projectile.Opacity * 0.67f, 0f, backglowTexture.Size() * 0.5f, projectile.scale * 0.72f, 0, 0f);

            // Draw strong red lightning zaps above the ground.
            ulong lightningSeed = (ulong)projectile.identity * 6342791uL;
            for (int i = 0; i < 5; i++)
            {
                Vector2 lightningScale = new Vector2(1f, projectile.scale) * MathHelper.Lerp(0.3f, 0.5f, Utils.RandomFloat(ref lightningSeed)) * 1.4f;
                float lightningRotation = MathHelper.Lerp(-0.8f, 0.8f, i / 4f + Utils.RandomFloat(ref lightningSeed) * 0.1f) + MathHelper.PiOver2;
                Color lightningColor = Color.Lerp(Color.Red, Color.Yellow, Utils.RandomFloat(ref lightningSeed) * 0.56f) * projectile.Opacity;
                Main.spriteBatch.Draw(zap, drawPosition, null, lightningColor, lightningRotation, zap.Size() * Vector2.UnitY * 0.5f, lightningScale, 0, 0f);
                Main.spriteBatch.Draw(zap, drawPosition, null, lightningColor * 0.5f, lightningRotation, zap.Size() * Vector2.UnitY * 0.5f, lightningScale * new Vector2(1f, 1.3f), 0, 0f);
            }

            Main.spriteBatch.ResetBlendState();
            return false;
        }

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers)
        {
            overPlayers.Add(index);
        }

        public override bool ShouldUpdatePosition() => false;

        public override bool CanDamage() => false;
    }
}
