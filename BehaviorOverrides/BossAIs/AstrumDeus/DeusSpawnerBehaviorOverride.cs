using CalamityMod;
using CalamityMod.NPCs.AstrumDeus;
using CalamityMod.Particles;
using CalamityMod.Projectiles.Boss;
using InfernumMode.OverridingSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.AstrumDeus
{
    public class DeusSpawnerBehaviorOverride : ProjectileBehaviorOverride
    {
        public override int ProjectileOverrideType => ModContent.ProjectileType<DeusRitualDrama>();
        public override ProjectileOverrideContext ContentToOverride => ProjectileOverrideContext.ProjectileAI | ProjectileOverrideContext.ProjectilePreDraw;

        public override bool PreAI(Projectile projectile)
        {

            ref float timer = ref projectile.ai[0];

            // Rise into the sky a bit after oscillating. After even more time has passed, slow down.
            if (timer >= 60f && timer < 350f)
                projectile.velocity.Y = MathHelper.Lerp(projectile.velocity.Y, -3f, 0.06f);
            else
                projectile.velocity *= 0.97f;

            // Summon deus from the sky after enough time has passed.
            if (timer == 374f)
            {
                Main.PlaySound(InfernumMode.CalamityMod.GetLegacySoundSlot(SoundType.Custom, "Sounds/Custom/AstrumDeusSpawn"), projectile.Center);
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    int deus = NPC.NewNPC((int)projectile.Center.X, (int)projectile.Center.Y - 1900, ModContent.NPCType<AstrumDeusHeadSpectral>());
                    CalamityMod.CalamityUtils.BossAwakenMessage(deus);
                }

            }

            if (NPC.AnyNPCs(ModContent.NPCType<AstrumDeusHeadSpectral>()) && timer < 375f)
                timer = 375f;

            if (!NPC.AnyNPCs(ModContent.NPCType<AstrumDeusHeadSpectral>()) && timer >= 375f)
                projectile.Kill();
            projectile.timeLeft = 5;

            timer++;
            return false;
        }

        public override bool PreDraw(Projectile projectile, SpriteBatch spriteBatch, Color lightColor)
        {
            // Draw a beacon into the sky.
            Texture2D borderTexture = ModContent.GetTexture("InfernumMode/BehaviorOverrides/BossAIs/Cultist/Border");

            float left = projectile.Center.X - AstrumDeusHeadBehaviorOverride.EnrageStartDistance;
            float right = projectile.Center.X + AstrumDeusHeadBehaviorOverride.EnrageStartDistance;
            float leftBorderOpacity = Utils.InverseLerp(left + 350f, left, Main.LocalPlayer.Center.X, true) * 0.6f;
            float rightBorderOpacity = Utils.InverseLerp(right - 350f, right, Main.LocalPlayer.Center.X, true) * 0.6f;

            Main.spriteBatch.SetBlendState(BlendState.Additive);

            Color borderColor1 = Color.OrangeRed;
            Color borderColor2 = Color.Cyan;
            if (leftBorderOpacity > 0f)
            {
                Vector2 baseDrawPosition = new Vector2(left, Main.LocalPlayer.Center.Y) - Main.screenPosition;
                float borderOutwardness = Utils.InverseLerp(0f, 0.9f, leftBorderOpacity, true) * MathHelper.Lerp(700f, 755f, (float)Math.Cos(Main.GlobalTime * 4.4f) * 0.5f + 0.5f);
                Color borderColor = Color.Lerp(Color.Transparent, borderColor1, leftBorderOpacity);

                for (int i = 0; i < 150; i++)
                {
                    float fade = 1f - Math.Abs(i - 75f) / 75f;
                    Vector2 drawPosition = baseDrawPosition + Vector2.UnitY * (i - 75f) / 75f * borderOutwardness;
                    Main.spriteBatch.Draw(borderTexture, drawPosition, null, Color.Lerp(borderColor, borderColor2, 1f - fade) * fade, 0f, borderTexture.Size() * 0.5f, new Vector2(0.33f, 1f), 0, 0f);
                }
            }

            if (rightBorderOpacity > 0f)
            {
                Vector2 baseDrawPosition = new Vector2(right, Main.LocalPlayer.Center.Y) - Main.screenPosition;
                float borderOutwardness = Utils.InverseLerp(0f, 0.9f, rightBorderOpacity, true) * MathHelper.Lerp(700f, 755f, (float)Math.Cos(Main.GlobalTime * 4.4f) * 0.5f + 0.5f);
                Color borderColor = Color.Lerp(Color.Transparent, Color.Orange, rightBorderOpacity);

                for (int i = 0; i < 150; i++)
                {
                    float fade = 1f - Math.Abs(i - 75f) / 75f;
                    Vector2 drawPosition = baseDrawPosition + Vector2.UnitY * (i - 75f) / 75f * borderOutwardness;
                    Main.spriteBatch.Draw(borderTexture, drawPosition, null, Color.Lerp(borderColor, borderColor2, 1f - fade) * fade, 0f, borderTexture.Size() * 0.5f, new Vector2(0.33f, 1f), SpriteEffects.FlipHorizontally, 0f);
                }
            }

            Main.spriteBatch.SetBlendState(BlendState.AlphaBlend);
            return projectile.ai[0] <= 374f;
        }
    }
}
