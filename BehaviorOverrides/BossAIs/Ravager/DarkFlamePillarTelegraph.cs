using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using System.Collections.Generic;

namespace InfernumMode.BehaviorOverrides.BossAIs.Ravager
{
    public class DarkFlamePillarTelegraph : ModProjectile
    {
        public ref float Countdown => ref projectile.ai[0];

        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public override void SetStaticDefaults() => DisplayName.SetDefault("Telegraph");

        public override void SetDefaults()
        {
            projectile.width = 8;
            projectile.height = 8;
            projectile.hostile = true;
            projectile.ignoreWater = true;
            projectile.tileCollide = false;
            projectile.penetrate = -1;
            projectile.hide = true;
        }

        public override void AI()
        {
            projectile.Opacity = Utils.InverseLerp(0f, 12f, 3600f - projectile.timeLeft, true) * Utils.InverseLerp(0f, 16f, Countdown, true);
            projectile.scale = projectile.Opacity * 5f;
            Countdown--;

            if (Countdown <= 0f)
                projectile.Kill();
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Vector2 top = projectile.Center - Vector2.UnitY * 1500f;
            Vector2 bottom = projectile.Center + Vector2.UnitY * 1500f;
            Color color = Color.SkyBlue * projectile.Opacity;
            Main.spriteBatch.DrawLineBetter(top, bottom, color, projectile.scale);
            return false;
        }

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers)
        {
            behindNPCsAndTiles.Add(index);
        }
    }
}
