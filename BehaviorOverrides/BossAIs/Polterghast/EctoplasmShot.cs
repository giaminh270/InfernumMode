using CalamityMod;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.Polterghast
{
    public class EctoplasmShot : ModProjectile
    {
        public static readonly Color[] ColorSet = new Color[]
        {
            Color.Pink,
            Color.Cyan
        };

        public bool ShouldFall => projectile.ai[0] == 1f;

        public ref float Lifetime => ref projectile.ai[1];

        public Color StreakBaseColor => Color.Lerp(CalamityUtils.MulticolorLerp(projectile.ai[1] % 0.999f, ColorSet), Color.White, 0.2f);

        public override string Texture => "CalamityMod/Projectiles/StarProj";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Ectoplasm Blast");
            ProjectileID.Sets.TrailingMode[projectile.type] = 2;
            ProjectileID.Sets.TrailCacheLength[projectile.type] = 7;
        }

        public override void SetDefaults()
        {
            projectile.width = projectile.height = 18;
            projectile.alpha = 255;
            projectile.penetrate = -1;
            projectile.timeLeft = 1200;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;
            projectile.hostile = true;
            cooldownSlot = 1;
        }

        public override void AI()
        {
            if (projectile.timeLeft < 1215f - Lifetime)
                projectile.damage = 0;

            if (projectile.timeLeft < 1200f - Lifetime)
                projectile.Kill();

            // Initialize the hue.
            if (projectile.ai[1] == 0f)
                projectile.ai[1] = Main.rand.NextFloat();

            if (ShouldFall)
            {
                Player target = Main.player[Player.FindClosest(projectile.Center, 1, 1)];
                if (projectile.timeLeft > 1080f)
                    projectile.velocity = Vector2.Lerp(projectile.velocity, projectile.SafeDirectionTo(target.Center) * 18f, 0.032f);
            }
            else
                projectile.velocity *= 0.985f;
            projectile.Opacity = Utils.InverseLerp(1200f, 1180f, projectile.timeLeft, true) * Utils.InverseLerp(1200f - Lifetime, 1220f - Lifetime, projectile.timeLeft, true);
            projectile.rotation = projectile.velocity.ToRotation() + MathHelper.PiOver2;

            // Emit ectoplasm dust.
            if (Main.rand.NextBool())
            {
                Color dustColor = Color.Lerp(Color.Cyan, Color.Pink, Main.rand.NextFloat());
                dustColor = Color.Lerp(dustColor, Color.White, 0.7f);

                Dust ectoplasm = Dust.NewDustPerfect(projectile.Center + Main.rand.NextVector2Circular(9f, 9f) + projectile.velocity, 267, projectile.velocity * -2.6f + Main.rand.NextVector2Circular(0.6f, 0.6f), 0, dustColor);
                ectoplasm.scale = 0.3f;
                ectoplasm.fadeIn = Main.rand.NextFloat() * 1.2f;
                ectoplasm.noGravity = true;
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Texture2D streakTexture = Main.projectileTexture[projectile.type];
            for (int i = 1; i < projectile.oldPos.Length; i += InfernumConfig.Instance.ReducedGraphicsConfig ? 2 : 1)
            {
                if (projectile.oldPos[i - 1] == Vector2.Zero || projectile.oldPos[i] == Vector2.Zero)
                    continue;

                float completionRatio = i / (float)projectile.oldPos.Length;
                float fade = (float)Math.Pow(completionRatio, 2f);
                float scale = projectile.scale * MathHelper.Lerp(1.2f, 0.9f, Utils.InverseLerp(0f, 0.24f, completionRatio, true)) *
                    MathHelper.Lerp(0.9f, 0.56f, Utils.InverseLerp(0.5f, 0.78f, completionRatio, true));
                Color drawColor = Color.HotPink * (1f - fade) * projectile.Opacity;
                drawColor.A = 0;

                Vector2 drawPosition = projectile.oldPos[i - 1] + projectile.Size * 0.5f - Main.screenPosition;
                Main.spriteBatch.Draw(streakTexture, drawPosition, null, drawColor, projectile.oldRot[i], streakTexture.Size() * 0.5f, scale, SpriteEffects.None, 0f);
            }
            return false;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            for (int i = 0; i < 3; i++)
            {
                if (targetHitbox.Intersects(Utils.CenteredRectangle(projectile.oldPos[i] + projectile.Size * 0.5f, projectile.Size)))
                    return true;
            }
            return false;
        }
    }
}