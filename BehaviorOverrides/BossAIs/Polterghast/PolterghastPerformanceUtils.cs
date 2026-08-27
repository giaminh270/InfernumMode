using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;

namespace InfernumMode.BehaviorOverrides.BossAIs.Polterghast
{
    /// <summary>
    /// Low-end rendering helpers for the Polterghast encounter.
    /// Gameplay/projectile counts remain unchanged; only client-side rendering is reduced.
    /// </summary>
    internal static class PolterghastPerformanceUtils
    {
        public static bool ReducedGraphics => InfernumConfig.Instance != null && InfernumConfig.Instance.ReducedGraphicsConfig;

        public static void DrawSoul(Projectile projectile, Color lightColor, Texture2D texture)
        {
            if (!ReducedGraphics)
            {
                Utilities.DrawAfterimagesCentered(
                    projectile,
                    lightColor,
                    ProjectileID.Sets.TrailingMode[projectile.type],
                    2,
                    texture);
                return;
            }

            Rectangle frame = texture.Frame(1, Main.projFrames[projectile.type], 0, projectile.frame);
            Vector2 drawPosition = projectile.Center - Main.screenPosition;
            Vector2 origin = frame.Size() * 0.5f;

            Main.spriteBatch.Draw(
                texture,
                drawPosition,
                frame,
                projectile.GetAlpha(lightColor),
                projectile.rotation,
                origin,
                projectile.scale,
                SpriteEffects.None,
                0f);
        }


    }
}
