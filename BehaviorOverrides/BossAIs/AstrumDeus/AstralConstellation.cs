using CalamityMod.NPCs.AstrumDeus;
using InfernumMode.BehaviorOverrides.BossAIs.MoonLord;
using InfernumMode.ILEditingStuff;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.AstrumDeus
{
    public class AstralConstellation : ModProjectile
    {
        public ref float Index => ref projectile.ai[0];
        public ref float Time => ref projectile.localAI[1];
        public override string Texture => "InfernumMode/ExtraTextures/LaserCircle";
        public override void SetStaticDefaults() => DisplayName.SetDefault("Star");

        public override void SetDefaults()
        {
            projectile.scale = Main.rand?.NextFloat(0.8f, 1f) ?? 1f;
            projectile.width = projectile.height = (int)(projectile.scale * 64f);
            projectile.hostile = true;
            projectile.ignoreWater = true;
            projectile.tileCollide = false;
            projectile.hide = true;
            projectile.timeLeft = 900;
            cooldownSlot = 1;
        }

        public override void AI()
        {
            if (!NPC.AnyNPCs(ModContent.NPCType<AstrumDeusHeadSpectral>()))
                projectile.active = false;

            if (projectile.timeLeft < 60)
                projectile.Opacity = MathHelper.Lerp(projectile.Opacity, 0.002f, 0.1f);

            Time++;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Projectile projectileToConnectTo = null;
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                if (Main.projectile[i].type != projectile.type || !Main.projectile[i].active ||
                    Main.projectile[i].timeLeft < 25f || Main.projectile[i].ai[0] != Index - 1f ||
                    Main.projectile[i].ai[1] != projectile.ai[1])
                {
                    continue;
                }

                projectileToConnectTo = Main.projectile[i];
                break;
            }

            float fadeToOrange = Utils.InverseLerp(50f, 0f, projectile.timeLeft, true);
            Color cyanColor = new Color(76, 255, 194);
            Color orangeColor = new Color(250, 90, 74);
            Color starColor = Color.Lerp(cyanColor, orangeColor, fadeToOrange);

            Texture2D starTexture = Main.projectileTexture[projectile.type];
            float scaleFactor = Utils.InverseLerp(0f, 15f, Time, true) + Utils.InverseLerp(30f, 0f, projectile.timeLeft, true) * 2f;

            // Draw stars.
            Vector2 drawPosition = projectile.Center - Main.screenPosition;
            for (int i = 0; i < 16; i++)
            {
                float drawOffsetFactor = ((float)Math.Cos(Main.GlobalTime * 40f) * 0.5f + 0.5f) * scaleFactor * fadeToOrange * 8f + 1f;
                Vector2 drawOffset = (MathHelper.TwoPi * i / 16f).ToRotationVector2() * drawOffsetFactor;
                Main.spriteBatch.Draw(starTexture, drawPosition + drawOffset, null, starColor * 0.4f, 0f, starTexture.Size() * 0.5f, projectile.scale * scaleFactor, SpriteEffects.None, 0f);
            }
            Main.spriteBatch.Draw(starTexture, drawPosition, null, starColor * 4f, 0f, starTexture.Size() * 0.5f, projectile.scale * scaleFactor, SpriteEffects.None, 0f);

            // Draw connection lines to the next star in the constellation.
            if (projectileToConnectTo != null)
            {
                Texture2D lineTexture = Main.extraTexture[47];
                Vector2 connectionDirection = projectile.SafeDirectionTo(projectileToConnectTo.Center);
                Vector2 start = projectile.Center + connectionDirection * projectile.scale * 24f;
                Vector2 end = projectileToConnectTo.Center - connectionDirection * projectile.scale * 24f;
                Vector2 scale = new Vector2(scaleFactor * 1.5f, (start - end).Length() / lineTexture.Height);
                Vector2 origin = new Vector2(lineTexture.Width * 0.5f, 0f);
                Color drawColor = Color.White;
                float rotation = (end - start).ToRotation() - MathHelper.PiOver2;

                Main.spriteBatch.Draw(lineTexture, start - Main.screenPosition, null, drawColor, rotation, origin, scale, SpriteEffects.None, 0f);
            }
            return false;
        }

        public override void Kill(int timeLeft)
        {
            Main.PlaySound(SoundID.Item91, projectile.Center);
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            Vector2 initialVelocity = Vector2.UnitY * 6f;
            if (projectile.identity % 2f == 1f)
                initialVelocity = initialVelocity.RotatedBy(MathHelper.PiOver2);

            Utilities.NewProjectileBetter(projectile.Center, -initialVelocity, ModContent.ProjectileType<AstralPlasmaSpark>(), 200, 0f);
            Utilities.NewProjectileBetter(projectile.Center, initialVelocity, ModContent.ProjectileType<AstralPlasmaSpark>(), 200, 0f);
            Utilities.NewProjectileBetter(projectile.Center, Vector2.Zero, ModContent.ProjectileType<MoonLordExplosion>(), 0, 0f);
        }
    }
}
