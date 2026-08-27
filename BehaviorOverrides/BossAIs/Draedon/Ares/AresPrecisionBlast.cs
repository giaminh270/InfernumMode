using CalamityMod;
using CalamityMod.DataStructures;
using CalamityMod.NPCs.ExoMechs.Ares;
using InfernumMode.DataStructures;
using InfernumMode.ExtraTextures;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.Draedon.Ares
{
    public class AresPrecisionBlast : ModProjectile, IAdditiveDrawer
    {
        public NPC ThingToAttachTo => Main.npc.IndexInRange((int)projectile.ai[0]) ? Main.npc[(int)projectile.ai[0]] : null;

        public Color BlastColor
        {
            get
            {
                int cannonID = ThingToAttachTo.type;
                if (cannonID == ModContent.NPCType<AresLaserCannon>())
                    return Color.Red;
                if (cannonID == ModContent.NPCType<AresTeslaCannon>())
                    return Color.Lerp(Color.Cyan, Color.White, 0.32f);
                if (cannonID == ModContent.NPCType<AresPlasmaFlamethrower>())
                    return Color.ForestGreen;
                if (cannonID == ModContent.NPCType<AresPulseCannon>())
                    return Color.MediumVioletRed;

                return Color.Red;
            }
        }

        public const int Lifetime = 30;

        public const float LaserLength = 2300f;

        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public override void SetStaticDefaults() => DisplayName.SetDefault("Precision Blast");

        public override void SetDefaults()
        {
            projectile.width = 48;
            projectile.height = 48;
            projectile.hostile = true;
            projectile.ignoreWater = true;
            projectile.tileCollide = false;
            projectile.penetrate = -1;
            projectile.timeLeft = Lifetime;
            projectile.hide = true;
            projectile.MaxUpdates = 4;
            projectile.Calamity().canBreakPlayerDefense = true;
            cooldownSlot = 1;
        }

        public override void AI()
        {
            projectile.scale = CalamityUtils.Convert01To010(projectile.timeLeft / (float)Lifetime) * 1.2f;
            if (projectile.scale > 1f)
                projectile.scale = 1f;
            projectile.hide = projectile.timeLeft >= 27;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float _ = 0f;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), projectile.Center, projectile.Center + projectile.velocity * LaserLength, projectile.width * projectile.scale, ref _);
        }

        public override bool ShouldUpdatePosition() => false;

        public void AdditiveDraw(SpriteBatch spriteBatch)
        {
            projectile.rotation = projectile.velocity.ToRotation() + MathHelper.PiOver2;

            // Draw the telegraph line.
            Vector2 start = projectile.Center - Main.screenPosition;
            Texture2D line = InfernumTextureRegistry.BloomLine;

            Vector2 beamOrigin = new Vector2(line.Width / 2f, line.Height);
            Vector2 beamScale = new Vector2(projectile.scale * projectile.width / line.Width * 1.5f, LaserLength / line.Height);
            Main.spriteBatch.Draw(line, start, null, Color.Lerp(BlastColor, Color.DarkGray, 0.27f), projectile.rotation, beamOrigin, beamScale, 0, 0f);
            Main.spriteBatch.Draw(line, start, null, BlastColor, projectile.rotation, beamOrigin, beamScale * new Vector2(0.7f, 1f), 0, 0f);
            Main.spriteBatch.Draw(line, start, null, Color.White, projectile.rotation, beamOrigin, beamScale * new Vector2(0.3f, 1f), 0, 0f);

            // Draw the energy focus at the start.
            Texture2D energyFocusTexture = InfernumTextureRegistry.LaserCircle;
            Vector2 drawPosition = projectile.Center - Main.screenPosition;
            spriteBatch.Draw(energyFocusTexture, drawPosition, null, Color.White * projectile.scale, projectile.rotation, energyFocusTexture.Size() * 0.5f, 0.7f, 0, 0f);
        }
    }
}
