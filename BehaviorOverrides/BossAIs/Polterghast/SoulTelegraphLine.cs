using CalamityMod;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.Polterghast
{
    public class SoulTelegraphLine : ModProjectile
    {
        public PrimitiveTrailCopy TelegraphDrawer = null;

        public ref float Time => ref projectile.ai[0];

        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public override void SetStaticDefaults() => DisplayName.SetDefault("Telegraph");

        public override void SetDefaults()
        {
            projectile.width = 10;
            projectile.height = 10;
            projectile.alpha = 0;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;
            projectile.timeLeft = 24;
            projectile.penetrate = -1;
        }

        public override void AI()
        {
            Time++;
            projectile.Opacity = CalamityUtils.Convert01To010(projectile.timeLeft / 24f);
        }

        public static float TelegraphWidthFunction(float _) => 70f;

        public Color TelegraphColorFunction(float completionRatio)
        {
            float endFadeOpacity = Utils.InverseLerp(0f, 0.15f, completionRatio, true) * Utils.InverseLerp(1f, 0.8f, completionRatio, true);
            return Color.LightCyan * endFadeOpacity * projectile.Opacity * 0.4f;
        }

        public override bool ShouldUpdatePosition() => false;

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            TelegraphDrawer = new PrimitiveTrailCopy(TelegraphWidthFunction, TelegraphColorFunction, null, true, GameShaders.Misc["Infernum:SideStreak"]);
            
            Vector2 telegraphStart = projectile.Center;
            Vector2 telegraphEnd = projectile.Center + projectile.velocity * 5000f;
            Vector2[] telegraphPoints = new Vector2[]
            {
                telegraphStart,
                (telegraphStart + telegraphEnd) * 0.5f,
                telegraphEnd
            };
            TelegraphDrawer.Draw(telegraphPoints, -Main.screenPosition, 72);
            return false;
        }

        public override void Kill(int timeLeft)
        {
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                for (int i = 0; i < 10; i++)
                {
                    Vector2 soulSpawnPosition = projectile.Center + Main.rand.NextVector2Circular(100f, 100f);
                    Vector2 soulVelocity = projectile.velocity * 24.5f;
                    Utilities.NewProjectileBetter(soulSpawnPosition, soulVelocity, ModContent.ProjectileType<NonReturningSoul>(), PolterghastBehaviorOverride.SoulDamage, 0f);
                }
            }
        }
    }
}
