using CalamityMod;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using PolterNPC = CalamityMod.NPCs.Polterghast.Polterghast;

namespace InfernumMode.BehaviorOverrides.BossAIs.Polterghast
{
    public class CirclingEctoplasm : ModProjectile
    {
        public float OrbitRadius;

        public float OrbitOffsetAngle;

        public float OrbitAngularVelocity;

        public Vector2 OrbitCenter;

        public static readonly Color[] ColorSet = new Color[]
        {
            Color.Pink,
            Color.Cyan
        };

        public Color StreakBaseColor => Color.Lerp(CalamityUtils.MulticolorLerp(projectile.ai[1] % 0.999f, ColorSet), Color.White, 0.2f);

        public override string Texture => "CalamityMod/Projectiles/StarProj";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Ectoplasm Wisp");
            ProjectileID.Sets.TrailingMode[projectile.type] = 2;
            ProjectileID.Sets.TrailCacheLength[projectile.type] = 3;
        }

        public override void SetDefaults()
        {
            projectile.width = projectile.height = 12;
            projectile.alpha = 255;
            projectile.penetrate = -1;
            projectile.timeLeft = 1600;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;
            projectile.hostile = true;
            cooldownSlot = 1;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(OrbitRadius);
            writer.Write(OrbitOffsetAngle);
            writer.Write(OrbitAngularVelocity);
            writer.WriteVector2(OrbitCenter);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            OrbitRadius = reader.ReadSingle();
            OrbitOffsetAngle = reader.ReadSingle();
            OrbitAngularVelocity = reader.ReadSingle();
            OrbitCenter = reader.ReadVector2();
        }

        public override void AI()
        {
            // Fade away if Polter is gone or not performing the relevant attack.
            int fadeoutTime = 40;
            int polterghastIndex = NPC.FindFirstNPC(ModContent.NPCType<PolterNPC>());
            if (polterghastIndex == -1 && projectile.timeLeft > fadeoutTime)
                projectile.timeLeft = fadeoutTime;

            if (polterghastIndex >= 0 && Main.npc[polterghastIndex].ai[0] != (int)PolterghastBehaviorOverride.PolterghastAttackType.WispCircleCharges && projectile.timeLeft > fadeoutTime)
                projectile.timeLeft = fadeoutTime;

            if (projectile.timeLeft < fadeoutTime)
                projectile.damage = 0;

            // Spin and orbit in place.
            OrbitOffsetAngle += OrbitAngularVelocity;
            projectile.Center = OrbitCenter + OrbitOffsetAngle.ToRotationVector2() * OrbitRadius;

            // Initialize the hue.
            if (projectile.ai[1] == 0f)
                projectile.ai[1] = Main.rand.NextFloat();

            // Calculate opacity, scale, and rotation.
            projectile.Opacity = Utils.InverseLerp(1600, 1555f, projectile.timeLeft, true) * Utils.InverseLerp(0f, fadeoutTime, projectile.timeLeft, true);
            projectile.scale = projectile.Opacity + 0.01f;
            projectile.rotation = (projectile.position - projectile.oldPosition).ToRotation() + MathHelper.PiOver2;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            // Simply draws a soul that is squished to be the size of the hitbox.
            // This looks a bit funny but visual quality isn't the point when this config is enabled.
            if (InfernumConfig.Instance.ReducedGraphicsConfig)
            {
                OptimizedDraw();
                return false;
            }

            // Draws whispy lights. Slightly more performance intensive due to looping, but also more visually interesting.
            DefaultDraw();
            return false;
        }

        public void DefaultDraw()
        {
            Texture2D streakTexture = Main.projectileTexture[projectile.type];
            for (int i = 1; i < projectile.oldPos.Length; i++)
            {
                if (projectile.oldPos[i - 1] == Vector2.Zero || projectile.oldPos[i] == Vector2.Zero)
                    continue;

                float completionRatio = i / (float)projectile.oldPos.Length;
                float fade = (float)Math.Pow(completionRatio, 2f);
                float scale = projectile.scale * MathHelper.Lerp(1f, 0.56f, Utils.InverseLerp(0f, 0.24f, completionRatio, true)) * MathHelper.Lerp(0.9f, 0.56f, Utils.InverseLerp(0.5f, 0.78f, completionRatio, true));
                Color drawColor = Color.HotPink * (1f - fade) * projectile.Opacity;
                drawColor.A = 0;

                Vector2 drawPosition = projectile.oldPos[i - 1] + projectile.Size * 0.5f - Main.screenPosition;
                Vector2 drawPosition2 = Vector2.Lerp(drawPosition, projectile.oldPos[i] + projectile.Size * 0.5f - Main.screenPosition, 0.5f);
                Main.spriteBatch.Draw(streakTexture, drawPosition, null, drawColor, projectile.oldRot[i], streakTexture.Size() * 0.5f, scale, SpriteEffects.None, 0f);
                Main.spriteBatch.Draw(streakTexture, drawPosition2, null, drawColor, projectile.oldRot[i], streakTexture.Size() * 0.5f, scale, SpriteEffects.None, 0f);
            }
        }

        public void OptimizedDraw()
        {
            Texture2D texture = ModContent.GetTexture("InfernumMode/BehaviorOverrides/BossAIs/Polterghast/SoulMedium");
            Vector2 drawPosition = projectile.Center - Main.screenPosition;
            Rectangle frame = texture.Frame(1, 4, 0, 0);
            Vector2 scale = Vector2.One * projectile.scale * 0.4f;
            Main.spriteBatch.Draw(texture, drawPosition, frame, projectile.GetAlpha(Color.White), projectile.rotation + MathHelper.Pi, frame.Size() * 0.5f, scale, 0, 0);
        }

        public override bool CanDamage() => projectile.timeLeft < 1480;

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