using CalamityMod.DataStructures;
using CalamityMod.Sounds;
using InfernumMode.ExtraTextures;
using CalamityMod.Projectiles.Boss;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using InfernumMode.GlobalInstances;
using Terraria.ID;
using Terraria.ModLoader;
using InfernumMode.DataStructures;

namespace InfernumMode.BehaviorOverrides.BossAIs.DoG
{
    public class RealityBreakPortalLaserWall : ModProjectile, IAdditiveDrawer
    {
        public ref float Time => ref projectile.ai[0];
        public override void SetStaticDefaults() => DisplayName.SetDefault("Portal");

        public override void SetDefaults()
        {
            projectile.width = 90;
            projectile.height = 90;
            projectile.ignoreWater = true;
            projectile.tileCollide = false;
            projectile.alpha = 255;
            projectile.penetrate = -1;
            projectile.timeLeft = 100;
            projectile.hide = true;
            cooldownSlot = 1;
        }

        public override void AI()
        {
            Time++;

            // Release the laser burst a second after spawning.
            if (Time == 60f)
            {
				Main.PlaySound(InfernumMode.CalamityMod.GetLegacySoundSlot(SoundType.Item, "Sounds/Item/LaserCannon"), projectile.Center);

                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    Player target = Main.player[Player.FindClosest(projectile.Center, 1, 1)];
                    float shootInterpolant = Utils.InverseLerp(600f, 1450f, projectile.Distance(target.Center), true);

                    int laserCount = (int)MathHelper.Lerp(5f, 12f, shootInterpolant);
                    float shootSpeed = MathHelper.Lerp(15f, 25f, shootInterpolant);
                    for (int i = 0; i < laserCount; i++)
                    {
                        Vector2 shootVelocity = projectile.SafeDirectionTo(target.Center).RotatedBy(MathHelper.Lerp(-0.6f, 0.6f, i / (float)(laserCount - 1f))) * shootSpeed;

                        ProjectileSpawnManagementSystem.PrepareProjectileForSpawning(laser =>
                        {
                            laser.MaxUpdates = 2;
                        });
                        Utilities.NewProjectileBetter(projectile.Center, shootVelocity, ModContent.ProjectileType<DoGDeathInfernum>(), DoGPhase1HeadBehaviorOverride.DeathLaserDamage, 0f, projectile.owner);
                    }
                }
            }

            projectile.Opacity = Utils.InverseLerp(0f, 50f, Time, true) * Utils.InverseLerp(0f, 30f, projectile.timeLeft, true);
            projectile.rotation += projectile.Opacity * 0.15f;
        }

        public void AdditiveDraw(SpriteBatch spriteBatch)
        {
            Texture2D portalTexture = Main.projectileTexture[projectile.type];
            Texture2D lightTexture = InfernumTextureRegistry.LaserCircle;
            Vector2 drawPosition = projectile.Center - Main.screenPosition;
            Vector2 origin = portalTexture.Size() * 0.5f;
            Color baseColor = Color.White;

            // Black portal.
            Color portalColor = baseColor * projectile.Opacity;

            for (int i = 0; i < 2; i++)
            {
                spriteBatch.Draw(portalTexture, drawPosition, null, portalColor, projectile.rotation, origin, projectile.scale, 0, 0f);
                spriteBatch.Draw(portalTexture, drawPosition, null, portalColor, -projectile.rotation, origin, projectile.scale, 0, 0f);
            }

            // Point of light.
            spriteBatch.Draw(lightTexture, drawPosition, null, baseColor * 0.8f, -projectile.rotation, lightTexture.Size() * 0.5f, projectile.scale * projectile.Opacity * 0.85f, 0, 0f);
        }
    }
}
