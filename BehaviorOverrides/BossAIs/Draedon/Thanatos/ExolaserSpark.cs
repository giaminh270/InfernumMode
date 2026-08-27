using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.Draedon.Thanatos
{
    public class ExolaserSpark : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Exolaser Spark");
            Main.projFrames[projectile.type] = 8;

        }

        public override void SetDefaults()
        {
            projectile.width = projectile.height = 18;
            projectile.scale = 1.2f;
            projectile.penetrate = -1;
            projectile.tileCollide = false;
            projectile.hostile = true;
            projectile.timeLeft = 200;
            projectile.Opacity = 0f;
            projectile.hide = true;
            cooldownSlot = 1;
        }

        public override void SendExtraAI(BinaryWriter writer) => writer.Write(projectile.MaxUpdates);

        public override void ReceiveExtraAI(BinaryReader reader) => projectile.MaxUpdates = reader.ReadInt32();

        public override void AI()
        {
            if (projectile.timeLeft < 30)
            {
                projectile.Opacity = projectile.timeLeft / 30f;
                projectile.damage = 0;
                return;
            }

            projectile.Opacity = MathHelper.Clamp(projectile.Opacity + 0.1f, 0f, 1f);
            projectile.rotation = projectile.velocity.ToRotation() + MathHelper.PiOver2;

            if (projectile.velocity.Length() < 5f)
                projectile.velocity *= 1.0225f;

            // Frames
            projectile.frameCounter++;
            if (projectile.frameCounter >= 8)
            {
                projectile.frame = (projectile.frame + 1) % Main.projFrames[projectile.type];
                projectile.frameCounter = 0;
            }
        }

        public override Color? GetAlpha(Color lightColor)
        {
            return new Color(255, 255, 255, 32) * projectile.Opacity;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Texture2D texture = Main.projectileTexture[projectile.type];
            Rectangle sourceRectangle = texture.Frame(1, Main.projFrames[projectile.type], frameY: projectile.frame);

            Vector2 origin = sourceRectangle.Size() * 0.5f;
            Color frontAfterimageColor = projectile.GetAlpha(lightColor) * 0.45f;
            frontAfterimageColor.A = 120;
            int frontCount = InfernumConfig.Instance.ReducedGraphicsConfig ? 3 : 7;
            for (int i = 0; i < frontCount; i++)
            {
                Vector2 drawOffset = (MathHelper.TwoPi * i / frontCount + projectile.rotation - MathHelper.PiOver2).ToRotationVector2() * projectile.scale * 4f;
                Vector2 afterimageDrawPosition = projectile.Center + drawOffset - Main.screenPosition;
                Main.spriteBatch.Draw(texture, afterimageDrawPosition, sourceRectangle, frontAfterimageColor, projectile.rotation, origin, projectile.scale, SpriteEffects.None, 0f);
            }

            int backCount = InfernumConfig.Instance.ReducedGraphicsConfig ? 6 : 14;
            for (int i = 0; i < backCount; i++)
            {
                Vector2 drawOffset = -projectile.velocity.SafeNormalize(Vector2.Zero) * i * projectile.scale * 4f;
                Vector2 afterimageDrawPosition = projectile.Center + drawOffset - Main.screenPosition;
                Color backAfterimageColor = projectile.GetAlpha(lightColor) * ((backCount - i) / (float)backCount);
                backAfterimageColor.A = 0;
                Main.spriteBatch.Draw(texture, afterimageDrawPosition, sourceRectangle, backAfterimageColor, projectile.rotation, origin, projectile.scale, SpriteEffects.None, 0f);
            }
            return false;
        }

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers)
        {
            behindProjectiles.Add(index);
        }
    }
}
