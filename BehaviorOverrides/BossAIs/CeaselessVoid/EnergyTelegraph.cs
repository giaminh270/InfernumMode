using CalamityMod;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace InfernumMode.BehaviorOverrides.BossAIs.CeaselessVoid
{
    public class EnergyTelegraph : ModProjectile
    {
        public PrimitiveTrail TelegraphDrawer = null;
        public Vector2[] TelegraphPoints;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Dark Energy Shard");
            ProjectileID.Sets.TrailingMode[projectile.type] = 0;
            ProjectileID.Sets.TrailCacheLength[projectile.type] = 9;
        }

        public override void SetDefaults()
        {
            projectile.width = 2;
            projectile.height = 2;
            projectile.hostile = true;
            projectile.ignoreWater = true;
            projectile.tileCollide = false;
            projectile.penetrate = -1;
            projectile.timeLeft = 32;
            projectile.scale = 2f;
            cooldownSlot = 1;
        }

        public override void AI()
        {
            projectile.Opacity = Utils.InverseLerp(32f, 27f, projectile.timeLeft) * Utils.InverseLerp(0f, 12f, projectile.timeLeft, true) * 0.5f;
            projectile.scale = CalamityUtils.Convert01To010(projectile.timeLeft / 32f);
        }

        public override Color? GetAlpha(Color lightColor)
        {
            return Color.Lerp(Color.Cyan, Color.Fuchsia, projectile.ai[0]) * projectile.Opacity;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            if (TelegraphDrawer == null)
            {
                TelegraphDrawer = new PrimitiveTrail(
                    widthFunction: _ => projectile.scale * 2f,
                    colorFunction: completionRatio =>
                    {
                        float opacity = Utils.InverseLerp(0f, 0.15f, completionRatio, true);
                        return projectile.GetAlpha(Color.White) * opacity;
                    }
                );
            }
            
            TelegraphDrawer.Draw(TelegraphPoints, -Main.screenPosition, 48);
            return false;
        }
    }
}