using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Dusts;
using CalamityMod.Events;
using CalamityMod;
using InfernumMode.Graphics.Interfaces;
using InfernumMode.Graphics.Primitives;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.AstrumAureus
{
    public class AstralMissile : ModProjectile, IPixelPrimitiveDrawer
    {
		public bool DrawBeforeNPCs => false;
		
        public PrimitiveTrailCopy FlameTrailDrawer;

        public ref float Time => ref projectile.ai[0];

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Astral Missile");
            ProjectileID.Sets.TrailCacheLength[projectile.type] = 6;
            ProjectileID.Sets.TrailingMode[projectile.type] = 0;
        }

        public override void SetDefaults()
        {
            projectile.width = projectile.height = 24;
            projectile.hostile = true;
            projectile.ignoreWater = true;
            projectile.tileCollide = false;
            projectile.penetrate = 1;
            projectile.timeLeft = 240;
            cooldownSlot = 1;
        }

        public override void AI()
        {
            projectile.rotation = projectile.velocity.ToRotation() + MathHelper.PiOver2;

            Lighting.AddLight(projectile.Center, 0.5f, 0.5f, 0.5f);

            Player closestPlayer = Main.player[Player.FindClosest(projectile.Center, 1, 1)];

            // Fly towards the closest player.
            projectile.velocity = Vector2.Lerp(projectile.velocity, projectile.SafeDirectionTo(closestPlayer.Center) * projectile.velocity.Length(), 0.033f);

            if (projectile.WithinRange(closestPlayer.Center, 30f))
                projectile.Kill();

            if (Time >= 45f && projectile.velocity.Length() < 24f)
                projectile.velocity *= 1.021f;

            Vector2 backOfMissile = projectile.Center - (projectile.rotation - MathHelper.PiOver2).ToRotationVector2() * 20f;
            Dust.NewDustDirect(backOfMissile, 5, 5, ModContent.DustType<AstralOrange>());

            Time++;
        }

        public static float FlameTrailWidthFunction(float completionRatio) => MathHelper.SmoothStep(21f, 8f, completionRatio);

        public static Color FlameTrailColorFunction(float completionRatio)
        {
            float trailOpacity = Utils.InverseLerp(0.8f, 0.27f, completionRatio, true) * Utils.InverseLerp(0f, 0.067f, completionRatio, true);
            Color startingColor = Color.Lerp(Color.Cyan, Color.White, 0.4f);
            Color middleColor = Color.Lerp(Color.Orange, Color.Yellow, 0.3f);
            Color endColor = Color.Lerp(Color.Orange, Color.Red, 0.67f);
            return CalamityUtils.MulticolorLerp(completionRatio, startingColor, middleColor, endColor) * trailOpacity;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Texture2D texture = ModContent.GetTexture(Texture);
            Texture2D glowmask = ModContent.GetTexture("InfernumMode/BehaviorOverrides/BossAIs/AstrumAureus/AstralMissileGlowmask");
            Rectangle frame = texture.Frame(1, Main.projFrames[projectile.type], 0, projectile.frame);
            Vector2 drawPosition = projectile.Center - Main.screenPosition;
            SpriteEffects direction = projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            // Draw the base sprite and glowmask.
            Main.spriteBatch.Draw(texture, drawPosition, frame, projectile.GetAlpha(lightColor), projectile.rotation, frame.Size() * 0.5f, projectile.scale, direction, 0);
            Main.spriteBatch.Draw(glowmask, drawPosition, frame, projectile.GetAlpha(Color.White), projectile.rotation, frame.Size() * 0.5f, projectile.scale, direction, 0);
            return false;
        }

        public void DrawPixelPrimitives(SpriteBatch spriteBatch)
        {
            // Initialize the flame trail drawer.
			if (FlameTrailDrawer == null)
				FlameTrailDrawer = new PrimitiveTrailCopy(FlameTrailWidthFunction, FlameTrailColorFunction, null, true, GameShaders.Misc["CalamityMod:ImpFlameTrail"]);
            Vector2 trailOffset = projectile.Size * 0.5f;
            trailOffset += (projectile.rotation + MathHelper.PiOver2).ToRotationVector2() * 10f;
            FlameTrailDrawer.DrawPixelated(projectile.oldPos, trailOffset - Main.screenPosition, 61);
        }

        public override void Kill(int timeLeft)
        {
            Main.PlaySound(SoundID.Zombie, projectile.Center, 103);

            projectile.position = projectile.Center;
            projectile.width = projectile.height = 96;
            projectile.position -= projectile.Size * 0.5f;

            for (int i = 0; i < 2; i++)
                Dust.NewDustDirect(projectile.position, projectile.width, projectile.height, ModContent.DustType<AstralBlue>(), 0f, 0f, 50, default, 1f);

            for (int i = 0; i < 20; i++)
            {
                Dust fire = Dust.NewDustDirect(projectile.position, projectile.width, projectile.height, ModContent.DustType<AstralOrange>(), 0f, 0f, 0, default, 1.5f);
                fire.noGravity = true;
                fire.velocity *= 3f;
            }
            projectile.Damage();
        }
		
        public override void OnHitPlayer(Player target, int damage, bool crit) => target.AddBuff(ModContent.BuffType<AstralInfectionDebuff>(), 180);		
    }
}
