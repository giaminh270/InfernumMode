using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Events;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.AstrumDeus
{
    public class AstralPlasmaSpark : ModProjectile
    {
        public bool Cyan => projectile.ai[0] == 1f;
        public ref float Time => ref projectile.ai[0];

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Astral Plasma Spark");
            Main.projFrames[projectile.type] = 4;
            ProjectileID.Sets.TrailCacheLength[projectile.type] = 4;
            ProjectileID.Sets.TrailingMode[projectile.type] = 0;
        }

        public override void SetDefaults()
        {
            projectile.width = 22;
            projectile.height = 22;
            projectile.hostile = true;
            projectile.ignoreWater = true;
            projectile.tileCollide = false;
            projectile.Opacity = 0f;
            projectile.penetrate = -1;
            projectile.timeLeft = 300;
            cooldownSlot = 1;
        }

        public override void AI()
        {
            // Decide frames and rotation.
            projectile.frameCounter++;
            projectile.frame = projectile.frameCounter / 5 % Main.projFrames[projectile.type];
            projectile.rotation = projectile.velocity.ToRotation() - MathHelper.PiOver2;

            // Fade in and out.
            projectile.Opacity = Utils.InverseLerp(0f, 12f, Time, true) * Utils.InverseLerp(0f, 32f, projectile.timeLeft, true);

            // Weakly home in on the target before accelerating.
            if (Time < 135f)
            {
                float flySpeed = BossRushEvent.BossRushActive ? 11f : 9f;
                Player target = Main.player[Player.FindClosest(projectile.Center, 1, 1)];
                if (!projectile.WithinRange(target.Center, 200f))
                    projectile.velocity = (projectile.velocity * 39f + projectile.SafeDirectionTo(target.Center) * flySpeed) / 40f;
            }
            else if (projectile.velocity.Length() < 14.5f)
                projectile.velocity *= 1.015f;

            Time++;
        }

        public override bool CanDamage() => projectile.Opacity > 0.8f;
        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Texture2D texture = ModContent.GetTexture(Texture);
            if (Cyan)
                texture = ModContent.GetTexture("InfernumMode/BehaviorOverrides/BossAIs/AstrumDeus/AstralPlasmaSparkCyan");
            Utilities.DrawAfterimagesCentered(projectile, lightColor, ProjectileID.Sets.TrailingMode[projectile.type], 1, texture);
            return false;
        }

        public override Color? GetAlpha(Color lightColor) => Color.White * projectile.Opacity;

        public override void OnHitPlayer(Player target, int damage, bool crit) => target.AddBuff(ModContent.BuffType<AstralInfectionDebuff>(), 120);
    }
}
