using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.MinibossAIs.CorruptionMimic
{
    public class CursedFlamePillar : ModProjectile
    {
        public ref float Time => ref projectile.ai[0];

        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public override void SetStaticDefaults() => DisplayName.SetDefault("Cursed Flame Pillar");

        public override void SetDefaults()
        {
            projectile.width = 36;
            projectile.height = 440;
            projectile.hostile = true;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;
            projectile.timeLeft = 120;
            projectile.penetrate = -1;
        }

        public override void AI()
        {
            Time++;

            if (Time >= 30f)
                projectile.velocity = (projectile.velocity * 1.025f).ClampMagnitude(5f, 30f);

            float dustCreationChance = Utils.InverseLerp(0f, 30f, Time, true);
            for (int i = 0; i < 20; i++)
            {
                if (Main.rand.NextFloat() > dustCreationChance)
                    continue;

                Dust fire = Dust.NewDustDirect(projectile.TopLeft, projectile.width, projectile.height, 267);
                fire.color = Color.Lerp(Color.Yellow, Color.Lime, Main.rand.NextFloat(0.1f, 1f));
                fire.velocity = projectile.velocity * 0.25f;
                fire.scale = 1.15f;
                fire.noGravity = true;
            }
        }

        public override Color? GetAlpha(Color lightColor) => Color.White * projectile.Opacity;

        public override bool ShouldUpdatePosition() => Time >= 30f;

        public override bool CanDamage() => Time >= 30f;

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Utilities.DrawAfterimagesCentered(projectile, lightColor, ProjectileID.Sets.TrailingMode[projectile.type], 3);
            return false;
        }
    }
}
