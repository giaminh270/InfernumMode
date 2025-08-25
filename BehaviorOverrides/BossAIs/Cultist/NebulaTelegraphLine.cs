using CalamityMod;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework.Graphics;
using System.IO;

namespace InfernumMode.BehaviorOverrides.BossAIs.Cultist
{
    public class NebulaTelegraphLine : ModProjectile
    {
        public ref float Time => ref projectile.ai[0];

        public ref float Lifetime => ref projectile.ai[1];
        
        public override void SetStaticDefaults() => DisplayName.SetDefault("Telegraph");

        public override void SetDefaults()
        {
            projectile.width = projectile.height = 2;
            projectile.ignoreWater = true;
            projectile.tileCollide = false;
            projectile.penetrate = -1;
            projectile.timeLeft = 900;
        }

        public override void AI()
        {
            int cultistIndex = NPC.FindFirstNPC(NPCID.CultistBoss);
            if (cultistIndex < 0)
            {
                projectile.Kill();
                return;
            }

            NPC cultist = Main.npc[cultistIndex];

            projectile.Center = cultist.Center;
            projectile.Opacity = CalamityUtils.Convert01To010(Time / Lifetime) * 2f;
            if (projectile.Opacity > 1f)
                projectile.Opacity = 1f;
            projectile.Opacity *= projectile.localAI[0];
            if (Time >= Lifetime)
                projectile.Kill();

            Time++;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            float telegraphWidth = MathHelper.Lerp(0.8f, 5f, CalamityUtils.Convert01To010(Time / Lifetime));

            // Draw a telegraph line outward.
            Vector2 start = projectile.Center;
            Vector2 end = projectile.Center + projectile.velocity.SafeNormalize(Vector2.UnitY) * 4200f;
            Main.spriteBatch.DrawLineBetter(start, end, Color.MediumPurple * projectile.Opacity, telegraphWidth);
            return false;
        }

        public override bool ShouldUpdatePosition() => false;
    }
}