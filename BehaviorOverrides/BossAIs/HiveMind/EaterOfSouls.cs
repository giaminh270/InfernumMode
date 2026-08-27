using CalamityMod;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.HiveMind
{
    public class EaterOfSouls : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Eater of Souls");
            ProjectileID.Sets.TrailingMode[projectile.type] = 0;
            ProjectileID.Sets.TrailCacheLength[projectile.type] = 7;
        }

        public override void SetDefaults()
        {
            projectile.width = 42;
            projectile.height = 32;
            projectile.ignoreWater = true;
            projectile.timeLeft = 490;
            projectile.scale = 1f;
            projectile.tileCollide = false;
            projectile.friendly = false;
            projectile.hostile = true;
            projectile.Calamity().canBreakPlayerDefense = true;
            cooldownSlot = 1;
        }

        public override void AI() => projectile.rotation = projectile.velocity.ToRotation() - MathHelper.PiOver2;

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Color drawColor = Color.MediumPurple;
            drawColor.A = 0;
            drawColor *= 0.5f;

            Utilities.DrawAfterimagesCentered(projectile, drawColor, ProjectileID.Sets.TrailingMode[projectile.type], 3);
            return true;
        }

        // This is mainly for multiplayer, to ensure that walls don't spawn on top of players and cheaply hit them.
        public override bool CanDamage() => projectile.timeLeft < 460;
    }
}
