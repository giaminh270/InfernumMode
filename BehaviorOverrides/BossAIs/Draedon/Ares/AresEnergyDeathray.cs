using CalamityMod;
using CalamityMod.Projectiles.BaseProjectiles;
using InfernumMode.ExtraTextures;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.Draedon.Ares
{
    public class AresEnergyDeathray : BaseLaserbeamProjectile
    {
        public float LifetimeThing = 50f;
        public override float MaxScale => 1f;
        public override float MaxLaserLength => 4000f;
        public override float Lifetime => LifetimeThing;
        public override Color LaserOverlayColor => Color.White;
        public override Color LightCastColor => Color.White;
        public override Texture2D LaserBeginTexture => ModContent.GetTexture("CalamityMod/Projectiles/Boss/AresLaserBeamStart");
        public override Texture2D LaserMiddleTexture => ModContent.GetTexture("CalamityMod/ExtraTextures/Lasers/AresLaserBeamMiddle");
        public override Texture2D LaserEndTexture => ModContent.GetTexture("CalamityMod/ExtraTextures/Lasers/AresLaserBeamEnd");
        public override string Texture => InfernumTextureRegistry.InvisPath;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Exo Energy Burst");
            Main.projFrames[projectile.type] = 5;
        }

        public override void SetDefaults()
        {
            projectile.width = 36;
            projectile.height = 36;
            projectile.hostile = true;
            projectile.alpha = 255;
            projectile.penetrate = -1;
            projectile.tileCollide = false;
            projectile.timeLeft = 1600;
            projectile.MaxUpdates = 2;
            projectile.Calamity().canBreakPlayerDefense = true;
            cooldownSlot = 1;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(projectile.localAI[0]);
            writer.Write(projectile.localAI[1]);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            projectile.localAI[0] = reader.ReadSingle();
            projectile.localAI[1] = reader.ReadSingle();
        }

        public override float DetermineLaserLength() => MaxLaserLength;

        public override void PostAI()
        {
            // Determine frames.
            projectile.frameCounter++;
            if (projectile.frameCounter % 5f == 0f)
                projectile.frame = (projectile.frame + 1) % Main.projFrames[projectile.type];
        }

        public override bool CanHitPlayer(Player target) => projectile.scale >= 0.5f;

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Color beamColor = LaserOverlayColor;
            Rectangle startFrameArea = LaserBeginTexture.Frame(1, Main.projFrames[projectile.type], 0, projectile.frame);
            Rectangle middleFrameArea = LaserMiddleTexture.Frame(1, Main.projFrames[projectile.type], 0, projectile.frame);
            Rectangle endFrameArea = LaserEndTexture.Frame(1, Main.projFrames[projectile.type], 0, projectile.frame);

            // Start texture drawing.
            Main.spriteBatch.Draw(LaserBeginTexture,
                             projectile.Center - Main.screenPosition,
                             startFrameArea,
                             beamColor,
                             projectile.rotation,
                             LaserBeginTexture.Size() / 2f,
                             projectile.scale,
                             SpriteEffects.None,
                             0);

            // Prepare things for body drawing.
            float laserBodyLength = LaserLength + middleFrameArea.Height;
            Vector2 centerOnLaser = projectile.Center + projectile.velocity * -5f;

            // Body drawing.
            if (laserBodyLength > 0f)
            {
                float laserOffset = middleFrameArea.Height * projectile.scale;
                float incrementalBodyLength = 0f;
                while (incrementalBodyLength + 1f < laserBodyLength)
                {
                    Main.spriteBatch.Draw(LaserMiddleTexture,
                                     centerOnLaser - Main.screenPosition,
                                     middleFrameArea,
                                     beamColor,
                                     projectile.rotation,
                                     LaserMiddleTexture.Size() * 0.5f,
                                     projectile.scale,
                                     SpriteEffects.None,
                                     0);
                    incrementalBodyLength += laserOffset;
                    centerOnLaser += projectile.velocity * laserOffset;
                    middleFrameArea.Y += LaserMiddleTexture.Height / Main.projFrames[projectile.type];
                    if (middleFrameArea.Y + middleFrameArea.Height > LaserMiddleTexture.Height)
                        middleFrameArea.Y = 0;
                }
            }

            Vector2 laserEndCenter = centerOnLaser - Main.screenPosition;
            Main.spriteBatch.Draw(LaserEndTexture,
                             laserEndCenter,
                             endFrameArea,
                             beamColor,
                             projectile.rotation,
                             LaserEndTexture.Size() * 0.5f,
                             projectile.scale,
                             SpriteEffects.None,
                             0);
            return false;
        }
    }
}
