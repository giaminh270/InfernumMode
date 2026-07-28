using CalamityMod;
using CalamityMod.DataStructures;
using InfernumMode.DataStructures;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static System.Math;
using static Microsoft.Xna.Framework.MathHelper;

namespace InfernumMode.BehaviorOverrides.BossAIs.CalamitasShadow
{
    public class LingeringBrimstoneFlames : ModProjectile, IAdditiveDrawer
    {
        public ref float Time => ref projectile.ai[0];

        public ref float LaserLength => ref projectile.ai[1];

        public override string Texture => "InfernumMode/ExtraTextures/Smoke";

        public override void SetStaticDefaults() => DisplayName.SetDefault("Brimstone Fire Cloud");

        public override void SetDefaults()
        {
            projectile.width = projectile.height = 112;
            projectile.hostile = true;
            projectile.ignoreWater = true;
            projectile.tileCollide = false;
            projectile.penetrate = -1;
            projectile.timeLeft = 32;
            projectile.Opacity = 0f;
            projectile.hide = true;
            projectile.rotation = Main.rand?.NextFloat(MathHelper.TwoPi) ?? 0f;
            cooldownSlot = 1;
        }

        public override void AI()
        {
            // Fade in.
            projectile.scale = CalamityUtils.Convert01To010(projectile.timeLeft / 32f) * 2f;
            if (projectile.scale > 1f)
                projectile.scale = 1f;
            projectile.Opacity = projectile.scale;
            projectile.scale *= Lerp(0.47f, 0.64f, projectile.identity % 9f / 9f);
            projectile.Size = Vector2.One * projectile.scale * 200f;
            projectile.velocity *= 0.98f;
            projectile.rotation += MathHelper.Clamp(projectile.velocity.X * 0.04f, -0.06f, 0.06f) + projectile.identity % 8f / 1200f;

            Time++;
        }

        public Color ColorFunction(float completionRatio)
        {
            Color color = Color.Lerp(Color.Orange, Color.DarkRed, 1f - (float)Pow(completionRatio, 2f));
            color = Color.Lerp(color, Color.Red, 0.5f);
            return color * projectile.Opacity * 0.7f;
        }

        public void AdditiveDraw(SpriteBatch spriteBatch)
        {
            Texture2D texture = ModContent.GetTexture(Texture);
            Color color = projectile.GetAlpha(Color.White);
            Vector2 drawPosition = projectile.Center - Main.screenPosition;
            spriteBatch.Draw(texture, drawPosition, null, color, projectile.rotation, texture.Size() * 0.5f, projectile.scale, 0, 0f);
            spriteBatch.Draw(texture, drawPosition, null, Color.White * projectile.Opacity * 0.7f, projectile.rotation, texture.Size() * 0.5f, projectile.scale, 0, 0f);
        }

        public override Color? GetAlpha(Color lightColor)
        {
            Color c = Color.Lerp(Color.Orange, Color.Red, projectile.identity % 10f / 20f + 0.34f);
            return c * 1.18f;
        }

        public override void Kill(int timeLeft)
        {
            Player target = Main.player[Player.FindClosest(projectile.Center, 1, 1)];
            if (Main.netMode != NetmodeID.MultiplayerClient && !projectile.WithinRange(target.Center, 300f))
            {
                Utilities.NewProjectileBetter(projectile.Center, projectile.SafeDirectionTo(target.Center) * 19f, ModContent.ProjectileType<DarkMagicFlame>(), CalamitasShadowBehaviorOverride.DarkMagicFlameDamage, 0f);
                Utilities.NewProjectileBetter(projectile.Center, Vector2.Zero, ModContent.ProjectileType<BrimstoneBoomExplosion>(), 0, 0f);
            }
        }
    }
}
