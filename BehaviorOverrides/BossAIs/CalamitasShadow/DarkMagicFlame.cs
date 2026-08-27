using CalamityMod;
using CalamityMod.NPCs;
using InfernumMode.ExtraTextures;
using InfernumMode.Graphics.Interfaces;
using InfernumMode.Graphics.Primitives;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using Terraria;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using static InfernumMode.BehaviorOverrides.BossAIs.CalamitasShadow.CalamitasShadowBehaviorOverride;

namespace InfernumMode.BehaviorOverrides.BossAIs.CalamitasShadow
{
    public class DarkMagicFlame : ModProjectile, IPixelPrimitiveDrawer
    {
		public bool DrawBeforeNPCs => false;
		
        public string HexType;

        public string HexType2;

        public bool FromSeekerHex;

        public PrimitiveTrailCopy TrailDrawer;

        public ref float Time => ref projectile.ai[0];

        public ref float AccelerationRequired => ref projectile.ai[1];

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Dark Magic Flame");
            Main.projFrames[projectile.type] = 6;
            ProjectileID.Sets.TrailCacheLength[projectile.type] = 9;
            ProjectileID.Sets.TrailingMode[projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            projectile.width = projectile.height = 12;
            projectile.scale = 0.8f;
            projectile.hostile = true;
            projectile.ignoreWater = true;
            projectile.tileCollide = false;
            projectile.alpha = 255;
            projectile.penetrate = -1;
            projectile.timeLeft = 150;
            cooldownSlot = 1;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(HexType ?? string.Empty);
            writer.Write(HexType2 ?? string.Empty);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            HexType = reader.ReadString();
            HexType2 = reader.ReadString();
        }

        public bool ImbuedWithHex(string hexName)
        {
            return HexType == hexName || HexType2 == hexName;
        }

        public override void AI()
        {
            // Initialize the hex type(s).
            if (string.IsNullOrEmpty(HexType) && projectile.velocity.Length() < 42f && GetHexNames(out HexType, out HexType2))
                projectile.netUpdate = true;

            // Seeker hex combining with either of these two is very unfun.
            if (!FromSeekerHex)
            {
                float acceleration = 1f;
                float maxSpeed = 36f;
                if (CalamityGlobalNPC.calamitas != -1 && ImbuedWithHex("Zeal"))
                {
                    // Start out slower if acceleration is expected.
                    if (AccelerationRequired == 0f)
                    {
                        projectile.velocity *= 0.37f;
                        AccelerationRequired = 1f;
                        projectile.netUpdate = true;
                    }

                    acceleration = 1.037f;
                }

                // Home in weakly if the shadow's target has the appropriate hex.
                if (CalamityGlobalNPC.calamitas != -1 && ImbuedWithHex("Accentuation"))
                {
                    float idealDirection = projectile.AngleTo(Main.player[Main.npc[CalamityGlobalNPC.calamitas].target].Center);
                    projectile.velocity = projectile.velocity.RotateTowards(idealDirection, 0.012f);
                    if (projectile.velocity.Length() > 18.75f)
                        projectile.velocity *= 0.98f;
                }

                if (acceleration > 1f && projectile.velocity.Length() < maxSpeed)
                    projectile.velocity *= acceleration;
            }

            projectile.Opacity = Utils.InverseLerp(0f, 20f, projectile.timeLeft, true) * Utils.InverseLerp(0f, 8f, Time, true);
            projectile.rotation = projectile.velocity.ToRotation() - MathHelper.PiOver2;

            Time++;
        }

        public float FlameTrailWidthFunction(float completionRatio)
        {
            return MathHelper.SmoothStep(24f, 5f, completionRatio) * projectile.Opacity;
        }

        public Color FlameTrailColorFunction(float completionRatio)
        {
            float trailOpacity = Utils.InverseLerp(0.75f, 0.27f, completionRatio, true) * Utils.InverseLerp(0f, 0.067f, completionRatio, true) * 0.9f;
            Color startingColor = Color.Lerp(Color.White, Color.DarkRed, 0.25f);
            Color middleColor = Color.Lerp(Color.Pink, Color.Red, 0.4f);
            Color endColor = Color.Lerp(Color.Orange, Color.Black, 0.35f);
            Color color = CalamityUtils.MulticolorLerp(completionRatio, startingColor, middleColor, endColor) * trailOpacity;
            color.A = (byte)(trailOpacity * 255);
            return color * projectile.Opacity;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Texture2D texture = Main.projectileTexture[projectile.type];
            Vector2 drawPosition = projectile.Center - Main.screenPosition;
            Rectangle frame = texture.Frame(1, Main.projFrames[projectile.type], 0, projectile.frame);
            Vector2 origin = frame.Size() * 0.5f;
            Color color = projectile.GetAlpha(Color.Lerp(Color.Violet, new Color(1f, 1f, 1f, 1f), projectile.identity / 5f * 0.6f));

            Main.spriteBatch.Draw(texture, drawPosition, frame, color, projectile.rotation, origin, projectile.scale, SpriteEffects.None, 0f);
            return false;
        }

        public void DrawPixelPrimitives(SpriteBatch spriteBatch)
        {
			if (TrailDrawer is null)  
				TrailDrawer = new PrimitiveTrailCopy(FlameTrailWidthFunction, FlameTrailColorFunction, null, true, GameShaders.Misc["CalamityMod:ImpFlameTrail"]);

            // Prepare the flame trail shader with its map texture.
            GameShaders.Misc["CalamityMod:ImpFlameTrail"].SetShaderTexture(InfernumTextureRegistry.StreakMagma);
            TrailDrawer.DrawPixelated(projectile.oldPos, projectile.Size * 0.5f - Main.screenPosition, 30);
        }
    }
}
