using CalamityMod;
using CalamityMod.DataStructures;
using InfernumMode.DataStructures;
using InfernumMode.ExtraTextures;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.Draedon.ArtemisAndApollo
{
    public class ArtemisBasicShotLaser : ModProjectile, IAdditiveDrawer
    {
        public NPC ThingToAttachTo => Main.npc.IndexInRange((int)projectile.ai[0]) ? Main.npc[(int)projectile.ai[0]] : null;

        public const int Lifetime = 30;

        public const float LaserLength = 2300f;

        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Exo Flame Laser");
            Main.projFrames[projectile.type] = 4;
        }

        public override void SetDefaults()
        {
            projectile.width = 26;
            projectile.height = 26;
            projectile.hostile = true;
            projectile.ignoreWater = true;
            projectile.tileCollide = false;
            projectile.penetrate = -1;
            projectile.timeLeft = Lifetime;
            projectile.hide = true;
            projectile.MaxUpdates = 5;
            projectile.Calamity().canBreakPlayerDefense = true;
            cooldownSlot = 1;
        }

        public override void AI()
        {
            projectile.scale = CalamityUtils.Convert01To010(projectile.timeLeft / (float)Lifetime) * 1.2f;
            if (projectile.scale > 1f)
                projectile.scale = 1f;
            projectile.rotation = projectile.velocity.ToRotation() + MathHelper.PiOver2;

            // Stick to Artemis.
            float positionOffset = ExoMechManagement.ExoTwinsAreInSecondPhase ? 102f : 70f;
            projectile.Center = ThingToAttachTo.Center + (ThingToAttachTo.rotation - MathHelper.PiOver2).ToRotationVector2() * positionOffset;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float _ = 0f;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), projectile.Center, projectile.Center + projectile.velocity * LaserLength, projectile.width * projectile.scale, ref _);
        }

        public void AdditiveDraw(SpriteBatch spriteBatch)
        {
            // Draw the telegraph line.
            Vector2 start = projectile.Center - Main.screenPosition;
            Texture2D line = InfernumTextureRegistry.BloomLine;

            Vector2 beamOrigin = new Vector2(line.Width / 2f, line.Height);
            Vector2 beamScale = new Vector2(projectile.scale * projectile.width / line.Width * 1.5f, LaserLength / line.Height);
            Main.spriteBatch.Draw(line, start, null, Color.Orange, projectile.rotation, beamOrigin, beamScale, 0, 0f);
            Main.spriteBatch.Draw(line, start, null, Color.Red, projectile.rotation, beamOrigin, beamScale * new Vector2(0.7f, 1f), 0, 0f);
            Main.spriteBatch.Draw(line, start, null, Color.Lerp(Color.OrangeRed, Color.White, 0.6f), projectile.rotation, beamOrigin, beamScale * new Vector2(0.3f, 1f), 0, 0f);

            // Draw the energy focus at the start.
            Texture2D energyFocusTexture = InfernumTextureRegistry.LaserCircle;
            Vector2 drawPosition = projectile.Center - Main.screenPosition;
            spriteBatch.Draw(energyFocusTexture, drawPosition, null, Color.White * projectile.scale, projectile.rotation, energyFocusTexture.Size() * 0.5f, 0.7f, 0, 0f);
        }
    }
}
