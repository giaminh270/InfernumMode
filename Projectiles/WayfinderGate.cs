using CalamityMod.Particles;
using InfernumMode.Sounds;
using InfernumMode.GlobalInstances;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Utilities;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;
using System;

namespace InfernumMode.Projectiles
{
    public class WayfinderGate : ModProjectile
    {
        public SlotId LoopSlot;

        public ref float Timer => ref projectile.ai[0];

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Wayfinder Gate");
        }

        public override void SetDefaults()
        {
            projectile.hostile = false;
            projectile.friendly = false;
            projectile.width = projectile.height = 40;
            projectile.timeLeft = 2;
            projectile.penetrate = -1;
            projectile.tileCollide = false;
            projectile.Opacity = 0f;
            projectile.hide = true;
        }

        public override void AI()
        {
            // Only exist if the position is set.
            if (PoDWorld.WayfinderGateLocation == Vector2.Zero)
                projectile.Kill();
            projectile.active = true;

            // Fade in.
            projectile.Opacity = MathHelper.Clamp(projectile.Opacity + 0.015f, 0f, 1f);

            // Ensure the position remains accurate.
            projectile.Center = PoDWorld.WayfinderGateLocation;

            // Never die naturally.
            projectile.timeLeft = 2;

            // Handle the loop sound.
            if (Timer % 115f == 0f)
            {
                var soundInstance = Main.PlaySound(InfernumSoundRegistry.WayfinderGateLoop, projectile.Center);
            }


            // Periodically emit particles if any player is nearby.
            bool nearbyPlayer = false;
            for (int i = 0; i < Main.player.Length; i++)
            {
                Player player = Main.player[i];
                if (player.active && player.WithinRange(projectile.Center, 1500f))
                {
                    nearbyPlayer = true;
                    break;
                }
            }

            if (nearbyPlayer)
            {
                if (Main.rand.NextBool(8))
                {
                    Vector2 position = projectile.Center + Main.rand.NextVector2Circular(40f, 10f) + new Vector2(-7f, -11f);
                    Dust fire = Dust.NewDustDirect(position, 16, 16, Main.rand.NextBool() ? 267 : 6, 0f, 0f, 254, Color.White, 1.4f);
                    fire.velocity = -Vector2.UnitY.RotatedByRandom(0.5f) * Main.rand.Next(3, 6);
                    fire.color = Color.Lerp(Color.Yellow, Color.Red, Main.rand.NextFloat(0.7f));
                    fire.noGravity = true;
                }
                if (Main.rand.NextBool(20))
                {
                    Vector2 position = projectile.Center + Main.rand.NextVector2Circular(40f, 10f) + new Vector2(-7f, -11f);
                    Vector2 velocity = -Vector2.UnitY.RotatedByRandom(0.5f) * 5;
                    Particle particle = new CritSpark(position, velocity, Main.rand.NextBool() ? Color.Orange : Color.Gold, Color.LightGoldenrodYellow, Main.rand.NextFloat(0.35f, 0.6f), 60, 0.2f);
                    GeneralParticleHandler.SpawnParticle(particle);
                }
                if (Main.rand.NextBool(80))
                {
                    Vector2 position = projectile.Center + Main.rand.NextVector2Circular(30f, 10f) + new Vector2(22f, 20f);
                    Projectile.NewProjectile(position, Vector2.Zero, ModContent.ProjectileType<WayfinderSymbol>(), 0, 0, Main.myPlayer);
                }

                // Spawn a symbol every 90 frames, due to the low chance of spawning often leading to empty patches of spawns.
                if (Timer % 90f == 0f)
                {
                    Vector2 position = projectile.Center + Main.rand.NextVector2Circular(30f, 10f) + new Vector2(22f, 20f);
                    Projectile.NewProjectile(position, Vector2.Zero, ModContent.ProjectileType<WayfinderSymbol>(), 0, 0, Main.myPlayer);
                }
            }
            Timer++;
        }
        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers)
        {
            behindNPCsAndTiles.Add(index);
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Texture2D bloomTexture = ModContent.GetTexture("CalamityMod/Particles/BloomCircle");
            Texture2D outerTexture = ModContent.GetTexture("InfernumMode/Projectiles/WayfinderGateOuter");
            Texture2D innerTexture = ModContent.GetTexture("InfernumMode/Projectiles/WayfinderGateInner");

            Vector2 drawPos = projectile.Center - Main.screenPosition;
            Color outerColor = CalamityMod.CalamityUtils.ColorSwap(WayfinderSymbol.Colors[1], WayfinderSymbol.Colors[2], 10f);
            Color innerColor = Color.Lerp(Color.White, outerColor, 0.5f) * 0.6f;

            float rotOuter = Main.GlobalTime * 0.1f;
            float rotInner = Main.GlobalTime * 0.133f;

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            int initialGateCreationTime = 120;
            if (Timer > initialGateCreationTime)
                Main.spriteBatch.Draw(bloomTexture, drawPos, null, outerColor * projectile.Opacity * 0.55f, 0f, bloomTexture.Size() * 0.5f, 1f, 0, 0);
            else
            {
                float interpolant = Timer / initialGateCreationTime;
                float opacity = MathHelper.Lerp(1, 0.55f, interpolant);
                float scale2 = MathHelper.Lerp(2, 1, interpolant);

                Main.spriteBatch.Draw(bloomTexture, drawPos, null, outerColor * opacity, 0, bloomTexture.Size() * 0.5f, scale2, SpriteEffects.None, 0f);
            }
            Main.spriteBatch.Draw(outerTexture, drawPos, null, outerColor * projectile.Opacity, rotOuter, outerTexture.Size() * 0.5f, 1, 0, 0);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            Main.spriteBatch.Draw(innerTexture, drawPos, null, innerColor * projectile.Opacity, rotInner, innerTexture.Size() * 0.5f, 1, 0, 0);

            float scale = (float)Math.Sin(Main.GlobalTime * MathHelper.TwoPi / 2f) * 0.3f + 0.7f;
            innerColor.A = 0;
            innerColor = innerColor * 0.1f * scale;
            for (float i = 0f; i < 1f; i += 1f / 16f)
                Main.spriteBatch.Draw(innerTexture, drawPos + (MathHelper.TwoPi * i).ToRotationVector2() * (6f + 2f), null, innerColor * projectile.Opacity, rotInner, innerTexture.Size() * 0.5f, 1f, 0, 0f);
            return false;
        }


    }
}
