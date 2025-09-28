using InfernumMode;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.MinibossAIs.HallowedMimic
{
    public class PiercingCrystalShard : ModProjectile
    {
        public int NPCIndex => (int)projectile.ai[1];

        public float CrystalLength => projectile.Distance(Main.npc[NPCIndex].Center);

        public ref float Time => ref projectile.ai[0];

        public const int PierceTime = 42;

        public const int FadeOutTime = 54;

        public override void SetStaticDefaults() => DisplayName.SetDefault("Crystal Shard");

        public override void SetDefaults()
        {
            projectile.width = projectile.height = 32;
            projectile.hostile = true;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;
            projectile.timeLeft = PierceTime + FadeOutTime;
            projectile.penetrate = -1;
            projectile.MaxUpdates = 2;
        }

        public override void AI()
        {
            Time++;

            if (Time < PierceTime)
                projectile.velocity *= 1.05f;
            else
            {
                projectile.MaxUpdates = 1;
                projectile.velocity = Vector2.Zero;
                projectile.Opacity = Utils.InverseLerp(0f, FadeOutTime, projectile.timeLeft, true);
            }
            projectile.rotation = Main.npc[NPCIndex].AngleTo(projectile.Center) + MathHelper.PiOver2;
        }

        public override Color? GetAlpha(Color lightColor) => Color.White * projectile.Opacity;

        public override bool CanDamage() => projectile.Opacity > 0.67f;

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float _ = 0f;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Main.npc[NPCIndex].Center, projectile.Center, projectile.width, ref _);
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Main.instance.LoadProjectile(ProjectileID.CrystalVileShardShaft);
			Texture2D crystalBeginTexture = Main.projectileTexture[ProjectileID.CrystalVileShardShaft];
            Texture2D crystalMiddleTexture = crystalBeginTexture;
            Texture2D crystalEndTexture = ModContent.GetTexture(Texture);
            Rectangle startFrameArea = crystalBeginTexture.Frame(1, Main.projFrames[projectile.type], 0, 0);
            Rectangle middleFrameArea = crystalMiddleTexture.Frame(1, Main.projFrames[projectile.type], 0, 0);
            Rectangle endFrameArea = crystalEndTexture.Frame(1, Main.projFrames[projectile.type], 0, 0);

            // Start texture drawing.
            Main.spriteBatch.Draw(crystalBeginTexture,
                             projectile.Center - Main.screenPosition,
                             startFrameArea,
                             Color.White * projectile.Opacity,
                             projectile.rotation,
                             crystalBeginTexture.Size() / 2f,
                             projectile.scale,
                             SpriteEffects.None,
                             0f);

            // Prepare things for body drawing.
            float crystalBodyLength = CrystalLength + crystalBeginTexture.Height * projectile.scale;
            Vector2 centerOncrystal = Main.npc[NPCIndex].Center;

            // Body drawing.
            if (crystalBodyLength > 0f)
            {
                float crystalOffset = middleFrameArea.Height * projectile.scale;
                float incrementalBodyLength = 0f;
                while (incrementalBodyLength + 1f < crystalBodyLength)
                {
                    Main.spriteBatch.Draw(crystalMiddleTexture,
                                     centerOncrystal - Main.screenPosition,
                                     middleFrameArea,
                                     Color.White * projectile.Opacity,
                                     projectile.rotation,
                                     crystalMiddleTexture.Width * 0.5f * Vector2.UnitX,
                                     projectile.scale,
                                     SpriteEffects.None,
                                     0f);
                    incrementalBodyLength += crystalOffset;
                    centerOncrystal += (projectile.rotation - MathHelper.PiOver2).ToRotationVector2() * crystalOffset;
                }
            }

            // End texture drawing.
            Vector2 crystalEndCenter = centerOncrystal - Main.screenPosition;
            Main.spriteBatch.Draw(crystalEndTexture,
                             crystalEndCenter,
                             endFrameArea,
                             Color.White * projectile.Opacity,
                             projectile.rotation,
                             crystalEndTexture.Frame(1, 1, 0, 0).Top(),
                             projectile.scale,
                             SpriteEffects.None,
                             0f);
            return false;
        }
    }
}
