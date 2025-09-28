using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.MinibossAIs.CorruptionMimic
{
    public class ChainGuillotine : ModProjectile
    {
        public int NPCIndex => (int)projectile.ai[1];

        public float ChainLength => projectile.Distance(Main.npc[NPCIndex].Center);

        public ref float Time => ref projectile.ai[0];

        public const int PierceTime = 42;

        // This does not account for extra updates.
        public const int ReturnTime = 45;

        public override void SetStaticDefaults() => DisplayName.SetDefault("Chain Guillotine");

        public override void SetDefaults()
        {
            projectile.width = projectile.height = 22;
            projectile.hostile = true;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;
            projectile.timeLeft = PierceTime + ReturnTime;
            projectile.penetrate = -1;
            projectile.MaxUpdates = 2;
        }

        public override void AI()
        {
            Time++;

            if (Time < PierceTime)
                projectile.velocity *= 1.03f;
            else
            {
                projectile.MaxUpdates = 3;
                projectile.velocity = Vector2.Zero;
                projectile.Center = Vector2.Lerp(projectile.Center, Main.npc[NPCIndex].Center, 0.04f).MoveTowards(Main.npc[NPCIndex].Center, 5f);
            }
            projectile.rotation = Main.npc[NPCIndex].AngleTo(projectile.Center) + MathHelper.PiOver2;
        }

        public override Color? GetAlpha(Color lightColor) => Color.White * projectile.Opacity;

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float _ = 0f;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Main.npc[NPCIndex].Center, projectile.Center, projectile.width, ref _);
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Texture2D chainBeginTexture = Main.chain40Texture;
            Texture2D chainMiddleTexture = chainBeginTexture;
            Texture2D chainEndTexture = ModContent.GetTexture(Texture);
            Rectangle startFrameArea = chainBeginTexture.Frame(1, Main.projFrames[projectile.type], 0, 0);
            Rectangle middleFrameArea = chainMiddleTexture.Frame(1, Main.projFrames[projectile.type], 0, 0);
            Rectangle endFrameArea = chainEndTexture.Frame(1, Main.projFrames[projectile.type], 0, 0);

            // Start texture drawing.
            Main.spriteBatch.Draw(chainBeginTexture,
                             projectile.Center - Main.screenPosition,
                             startFrameArea,
                             Color.White,
                             projectile.rotation,
                             chainBeginTexture.Size() / 2f,
                             projectile.scale,
                             SpriteEffects.None,
                             0f);

            // Prepare things for body drawing.
            float chainBodyLength = ChainLength;
            Vector2 centerOnchain = Main.npc[NPCIndex].Center;

            // Body drawing.
            if (chainBodyLength > 0f)
            {
                float chainOffset = middleFrameArea.Height * projectile.scale;
                float incrementalBodyLength = 0f;
                while (incrementalBodyLength + 1f < chainBodyLength)
                {
                    Main.spriteBatch.Draw(chainMiddleTexture,
                                     centerOnchain - Main.screenPosition,
                                     middleFrameArea,
                                     Color.White,
                                     projectile.rotation,
                                     chainMiddleTexture.Width * 0.5f * Vector2.UnitX,
                                     projectile.scale,
                                     SpriteEffects.None,
                                     0f);
                    incrementalBodyLength += chainOffset;
                    centerOnchain += (projectile.rotation - MathHelper.PiOver2).ToRotationVector2() * chainOffset;
                }
            }

            // End texture drawing.
            Vector2 chainEndCenter = centerOnchain - Main.screenPosition;
            Main.spriteBatch.Draw(chainEndTexture,
                             chainEndCenter,
                             endFrameArea,
                             Color.White,
                             projectile.rotation,
                             chainEndTexture.Frame(1, 1, 0, 0).Top(),
                             projectile.scale,
                             SpriteEffects.None,
                             0f);
            return false;
        }
    }
}
