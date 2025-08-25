using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Dusts;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.AstrumDeus
{
    public class AstralRubble : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Astral Rubble");
            ProjectileID.Sets.TrailCacheLength[projectile.type] = 3;
            ProjectileID.Sets.TrailingMode[projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            projectile.width = 32;
            projectile.height = 32;
            projectile.hostile = true;
            projectile.ignoreWater = true;
            projectile.tileCollide = false;
            projectile.penetrate = -1;
            projectile.timeLeft = 600;
            projectile.Infernum().FadesAwayWhenManuallyKilled = true;
            cooldownSlot = 1;
        }

        public override void AI()
        {
            Lighting.AddLight(projectile.Center, Vector3.One * projectile.Opacity * 0.5f);

            // Fall downward.
            projectile.velocity.X *= 0.994f;
            if (projectile.Center.Y < 800f)
                projectile.velocity.Y += 3f;
            projectile.velocity.Y = MathHelper.Clamp(projectile.velocity.Y + 0.1f, -36f, 9f);

            // Calculate rotation.
            projectile.rotation += projectile.velocity.Length() * Math.Sign(projectile.velocity.X) * 0.025f;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Utilities.DrawAfterimagesCentered(projectile, lightColor, ProjectileID.Sets.TrailingMode[projectile.type], 1);
            return false;
        }

        public override Color? GetAlpha(Color lightColor) => Color.White * projectile.Opacity;
        
        public override void OnHitPlayer(Player target, int damage, bool crit) => target.AddBuff(ModContent.BuffType<AstralInfectionDebuff>(), 120);

        // Explode into a bunch of astral cinders on death. This is purely visual and does not do damage.
        public override void Kill(int timeLeft)
        {
            projectile.position = projectile.Center;
            projectile.width = projectile.height = 96;
            projectile.position -= projectile.Size * 0.5f;

            for (int i = 0; i < 1; i++)
                Dust.NewDustDirect(projectile.position, projectile.width, projectile.height, ModContent.DustType<AstralOrange>(), 0f, 0f, 50, default, 1f);

            for (int i = 0; i < 5; i++)
            {
                Dust fire = Dust.NewDustDirect(projectile.position, projectile.width, projectile.height, ModContent.DustType<AstralOrange>(), 0f, 0f, 0, default, 0.4f);
                fire.noGravity = true;
                fire.velocity *= 1.5f;
            }
        }
    }
}
