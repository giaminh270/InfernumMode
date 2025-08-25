using CalamityMod;
using CalamityMod.Projectiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.EyeOfCthulhu
{
    public class SittingBlood : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Tooth Ball");
        }

        public override void SetDefaults()
        {
            projectile.width = projectile.height = 24;
            projectile.hostile = true;
            projectile.ignoreWater = true;
            projectile.alpha = 255;
            projectile.penetrate = -1;
            projectile.tileCollide = true;
            projectile.timeLeft = 330;
            cooldownSlot = 1;
        }

        public override void AI()
        {
            if (projectile.velocity.Y < 14f)
                projectile.velocity.Y += 0.25f;
            projectile.alpha = Utils.Clamp(projectile.alpha - 36, 0, 255);

            projectile.rotation = projectile.velocity.ToRotation() + MathHelper.PiOver2;
            if (projectile.timeLeft < 60)
            {
                projectile.scale *= 0.992f;
                CalamityGlobalProjectile.ExpandHitboxBy(projectile, (int)Math.Ceiling(24 * projectile.scale));
            }
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            projectile.velocity.X *= 0.94f;
            return false;
        }

        public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough)
        {
            fallThrough = false;
            return base.TileCollideStyle(ref width, ref height, ref fallThrough);
        }

        public override void ModifyHitPlayer(Player target, ref int damage, ref bool crit)
        {
            
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Texture2D texture = Main.projectileTexture[projectile.type];
            Vector2 drawPosition = projectile.Center - Main.screenPosition;
            Vector2 origin = texture.Size() * 0.5f;

            for (int i = 0; i < 6; i++)
            {
                Vector2 drawOffset = (MathHelper.TwoPi * i / 6f).ToRotationVector2() * 4f;
                Main.spriteBatch.Draw(texture, drawPosition + drawOffset, null, projectile.GetAlpha(Color.Red) * 0.65f, projectile.rotation, origin, projectile.scale, 0, 0f);
            }
            Main.spriteBatch.Draw(texture, drawPosition, null, projectile.GetAlpha(lightColor), projectile.rotation, origin, projectile.scale, 0, 0f);
            return false;
        }

        public override void Kill(int timeLeft)
        {
            Player closetstPlayer = Main.player[Player.FindClosest(projectile.Center, 1, 1)];
            if (Main.netMode == NetmodeID.MultiplayerClient || MathHelper.Distance(closetstPlayer.Center.X, projectile.Center.X) < 240f)
                return;

            for (int i = 0; i < 2; i++)
            {
                Utilities.NewProjectileBetter(projectile.Center, -Vector2.UnitY.RotatedByRandom(0.92f) * Main.rand.NextFloat(21f, 31f), ModContent.ProjectileType<EoCTooth2>(), 75, 0f);
            }
        }
    }
}
