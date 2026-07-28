using CalamityMod;
using CalamityMod.NPCs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using InfernumMode;

namespace InfernumMode.BehaviorOverrides.BossAIs.CalamitasShadow
{
    public class ArcingBrimstoneDart : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Brimstone Dart");
            Main.projFrames[projectile.type] = 4;
            ProjectileID.Sets.TrailingMode[projectile.type] = 0;
            ProjectileID.Sets.TrailCacheLength[projectile.type] = 3;
        }

        public override void SetDefaults()
        {
            projectile.width = projectile.height = 24;
            projectile.hostile = true;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;
            projectile.penetrate = -1;
            projectile.timeLeft = 240;
            projectile.Opacity = 0f;
            cooldownSlot = 1;
        }

        public override void AI()
        {
            projectile.Opacity = MathHelper.Clamp(projectile.Opacity + 0.2f, 0f, 1f);

            projectile.velocity = projectile.velocity.RotatedBy(projectile.ai[0]);

            float maxSpeed = 16f;
            float acceleration = 1.034f;
            if (CalamityGlobalNPC.calamitas != -1 && Main.player[Main.npc[CalamityGlobalNPC.calamitas].target].Infernum_CalShadowHex().HexIsActive("Zeal"))
            {
                maxSpeed = 21.5f;
                acceleration = 1.031f;
            }

            // Home in weakly if the shadow's target has the appropriate hex.
            if (CalamityGlobalNPC.calamitas != -1 && Main.player[Main.npc[CalamityGlobalNPC.calamitas].target].Infernum_CalShadowHex().HexIsActive("Accentuation"))
            {
                float idealDirection = projectile.AngleTo(Main.player[Main.npc[CalamityGlobalNPC.calamitas].target].Center);
                projectile.velocity = projectile.velocity.RotateTowards(idealDirection, 0.006f);
            }

            if (projectile.velocity.Length() < maxSpeed)
                projectile.velocity *= acceleration;

            projectile.frameCounter++;
            projectile.frame = projectile.frameCounter / 5 % Main.projFrames[projectile.type];
            projectile.rotation = projectile.velocity.ToRotation() + MathHelper.PiOver2;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            //Utilities.DrawBackglow(Color.HotPink, projectile.Opacity * 2f);
            CalamityUtils.DrawAfterimagesCentered(projectile, ProjectileID.Sets.TrailingMode[projectile.type], lightColor);
            return false;
        }
    }
}
