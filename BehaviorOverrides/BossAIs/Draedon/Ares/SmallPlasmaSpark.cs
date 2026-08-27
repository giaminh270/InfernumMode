using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.Draedon.Ares
{
    public class SmallPlasmaSpark : ModProjectile
    {
        public override void SetStaticDefaults() => DisplayName.SetDefault("Plasma Spark");

        public override void SetDefaults()
        {
            projectile.width = projectile.height = 14;
            projectile.penetrate = -1;
            projectile.tileCollide = false;
            projectile.hostile = true;
            projectile.timeLeft = 360;
            projectile.Opacity = 0f;
            projectile.hide = true;
            cooldownSlot = 1;
        }

        public override void AI()
        {
            // Set correct lifetime.
            if (projectile.ai[1] == 0f)
            {
                // This is 1 if being fired during the ares cage.
                if (projectile.ai[0] == 1f)
                    projectile.timeLeft = 200;

                projectile.ai[1] = 1f;
            }

            // Fade in and out.
            if (projectile.timeLeft <= 10)
                projectile.Opacity = MathHelper.Clamp(projectile.Opacity + 0.1f, 0f, 1f);
            else
                projectile.Opacity = MathHelper.Clamp(projectile.Opacity + 0.1f, 0f, 1f);

            projectile.rotation += projectile.velocity.X * 0.025f;
            if (projectile.velocity.Length() < 16f)
                projectile.velocity *= 1.0225f;

            // Emit dust.
            for (int i = 0; i < 2; i++)
            {
                Dust plasma = Dust.NewDustDirect(projectile.position, projectile.width, projectile.height, 107);
                plasma.scale *= 0.7f;
                plasma.velocity = plasma.velocity * 0.4f + Main.rand.NextVector2Circular(0.4f, 0.4f);
                plasma.fadeIn = 0.4f;
                plasma.noGravity = true;
            }
        }

        public override void OnHitPlayer(Player target, int damage, bool crit)
        {
            if (projectile.Opacity != 1f)
                return;

            target.AddBuff(BuffID.CursedInferno, 120);
        }

        public override Color? GetAlpha(Color lightColor)
        {
            return new Color(255, 255, 255, 48) * projectile.Opacity;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Texture2D texture = Main.projectileTexture[projectile.type];
            Vector2 origin = texture.Size() * 0.5f;

            int backCount = InfernumConfig.Instance.ReducedGraphicsConfig ? 1 : 4;
            for (int i = 0; i < backCount; i++)
            {
                Vector2 drawOffset = -projectile.velocity.SafeNormalize(Vector2.Zero) * i * 12f;
                Vector2 afterimageDrawPosition = projectile.Center + drawOffset - Main.screenPosition;
                Color backAfterimageColor = projectile.GetAlpha(lightColor) * ((backCount - i) / (float)backCount);
                Main.spriteBatch.Draw(texture, afterimageDrawPosition, null, backAfterimageColor, projectile.rotation, origin, projectile.scale, SpriteEffects.None, 0f);
            }

            Color frontAfterimageColor = projectile.GetAlpha(lightColor) * 0.2f;
            int frontCount = InfernumConfig.Instance.ReducedGraphicsConfig ? 3 : 9;
            for (int i = 0; i < frontCount; i++)
            {
                Vector2 drawOffset = (MathHelper.TwoPi * i / frontCount + projectile.rotation - MathHelper.PiOver2).ToRotationVector2() * 2f;
                Vector2 afterimageDrawPosition = projectile.Center + drawOffset - Main.screenPosition;
                Main.spriteBatch.Draw(texture, afterimageDrawPosition, null, frontAfterimageColor, projectile.rotation, origin, projectile.scale, SpriteEffects.None, 0f);
            }
            return false;
        }

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers)
        {
            behindProjectiles.Add(index);
        }
    }
}
