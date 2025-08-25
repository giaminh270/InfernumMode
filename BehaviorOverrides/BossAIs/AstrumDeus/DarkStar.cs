using CalamityMod;
using CalamityMod.Projectiles;
using InfernumMode.ILEditingStuff;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.AstrumDeus
{
    public class DarkStar : ModProjectile
    {
        public Vector2 AnchorPoint;

        public float InitialOffsetAngle;

        public float FadeToDarkGodColors => Utils.InverseLerp(90f, 150f, Time, true);

        public ref float ConstellationIndex => ref projectile.ai[0];

        public ref float ConstellationIndexToAttachTo => ref projectile.ai[1];

        public ref float ColorSpectrumHue => ref projectile.localAI[0];

        public ref float Time => ref projectile.localAI[1];

        public const int PointsInStar = 6;

        public const float RadiusOfConstellation = 575f;

        public const int Lifetime = 960;
        public const int FadeinTime = 18;
        public const int FadeoutTime = 18;

        public override string Texture => "InfernumMode/ExtraTextures/Gleam";

        public static Vector2 CalculateStarPosition(Vector2 origin, float offsetAngle, float spinAngle)
        {
            // Equations for a generalized form of an asteroid.
            int n = PointsInStar - 1;
            Vector2 starOffset = new Vector2((float)Math.Sin(offsetAngle) * n - (float)Math.Sin(offsetAngle * n), (float)Math.Cos(offsetAngle) * n + (float)Math.Cos(offsetAngle * n)) * RadiusOfConstellation;
            starOffset /= PointsInStar;
            starOffset.Y *= -1f;
            return origin + starOffset.RotatedBy(spinAngle);
        }

        public override void SetStaticDefaults() => DisplayName.SetDefault("Dark Star");

        public override void SetDefaults()
        {
            projectile.width = 72;
            projectile.height = 72;
            projectile.penetrate = -1;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;
            projectile.timeLeft = Lifetime;
            projectile.scale = 0.001f;
            projectile.hide = true;
            cooldownSlot = 1;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(InitialOffsetAngle);
            writer.Write(Time);
            writer.WriteVector2(AnchorPoint);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            InitialOffsetAngle = reader.ReadSingle();
            Time = reader.ReadSingle();
            AnchorPoint = reader.ReadVector2();
        }

        public override void AI()
        {
            if (Time == 1f)
            {
                projectile.scale = 1f;
				CalamityGlobalProjectile.ExpandHitboxBy(projectile, 72 * projectile.scale);
                ColorSpectrumHue = Main.rand.NextFloat(0f, 0.9999f);
                projectile.netUpdate = true;
                projectile.rotation = projectile.velocity.ToRotation() + MathHelper.PiOver2;
            }
            Time++;

            projectile.Center = CalculateStarPosition(AnchorPoint, InitialOffsetAngle, Time / 72f);
            projectile.velocity = Vector2.Zero;
            projectile.rotation += (projectile.identity % 2 == 0).ToDirectionInt() * 0.024f;
            
            projectile.Opacity = Utils.InverseLerp(0f, FadeinTime, Time, true) * Utils.InverseLerp(Lifetime, Lifetime - FadeoutTime, Time, true);
            projectile.velocity = projectile.velocity.RotatedBy(Math.Sin(Time / 20f) * 0.02f);
            projectile.scale = MathHelper.Lerp(0.135f, 0.175f, FadeToDarkGodColors) * projectile.Opacity;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Texture2D sparkleTexture = ModContent.GetTexture("InfernumMode/ExtraTextures/LargeStar");

            // Orange and cyan.
            Color c1 = new Color(255, 63, 39);
            Color c2 = new Color(40, 255, 187);

            // Moon lord cyan and violet.
            c1 = Color.Lerp(c1, new Color(117, 255, 160), FadeToDarkGodColors);
            c2 = Color.Lerp(c2, new Color(88, 55, 172), FadeToDarkGodColors);

            float hue = (float)Math.Sin(MathHelper.Pi * ColorSpectrumHue + Main.GlobalTime * 3f) * 0.5f + 0.5f;
            Color sparkleColor = CalamityUtils.MulticolorLerp(hue, c1, c2) * projectile.Opacity * 0.84f;
            sparkleColor *= MathHelper.Lerp(1f, 1.5f, Utils.InverseLerp(Lifetime * 0.5f - 15f, Lifetime * 0.5f + 15f, Time, true));
            Vector2 origin = sparkleTexture.Size() / 2f;

            Vector2 sparkleScale = Vector2.One * projectile.Opacity * projectile.scale;
            Vector2 orthogonalsparkleScale = Vector2.One * projectile.Opacity * projectile.scale * 1.4f;

            Projectile projectileToConnectTo = null;
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                if (Main.projectile[i].type != projectile.type || !Main.projectile[i].active || Main.projectile[i].ai[0] != ConstellationIndexToAttachTo)
                {
                    continue;
                }

                projectileToConnectTo = Main.projectile[i];
                break;
            }

            // Draw connection lines to the next star in the constellation.
            float scaleFactor = Utils.InverseLerp(0f, 15f, Time, true) + Utils.InverseLerp(30f, 0f, projectile.timeLeft, true) * 2f;
            if (projectileToConnectTo != null)
            {
                Texture2D lineTexture = Main.extraTexture[47];
                Vector2 start = projectile.Center;
                Vector2 end = projectileToConnectTo.Center;
                Vector2 scale = new Vector2(scaleFactor * 1.5f, (start - end).Length() / lineTexture.Height);
                Vector2 lineOrigin = new Vector2(lineTexture.Width * 0.5f, 0f);
                Color drawColor = Color.White * Utils.InverseLerp(1f, 25f, projectileToConnectTo.timeLeft, true);
                float rotation = (end - start).ToRotation() - MathHelper.PiOver2;
                Main.spriteBatch.Draw(lineTexture, start - Main.screenPosition, null, drawColor, rotation, lineOrigin, scale, 0, 0f);
            }

            // Draw the sparkles.
            Vector2 drawCenter = projectile.Center - Main.screenPosition;
            spriteBatch.Draw(sparkleTexture, drawCenter, null, sparkleColor, MathHelper.PiOver2 + projectile.rotation, origin, orthogonalsparkleScale, 0, 0f);
            spriteBatch.Draw(sparkleTexture, drawCenter, null, sparkleColor, projectile.rotation, origin, sparkleScale, 0, 0f);
            spriteBatch.Draw(sparkleTexture, drawCenter, null, sparkleColor, MathHelper.PiOver2 + projectile.rotation, origin, orthogonalsparkleScale * 0.6f, 0, 0f);
            spriteBatch.Draw(sparkleTexture, drawCenter, null, sparkleColor, projectile.rotation, origin, sparkleScale * 0.6f, 0, 0f);
            return false;
        }
    }
}
