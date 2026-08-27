using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using InfernumMode.ExtraTextures;
using System;

namespace InfernumMode.BehaviorOverrides.BossAIs.Providence
{
    public class HolyRitual : ModProjectile
    {
        public ref float Time => ref projectile.ai[0];

        public static int Lifetime => 180;

        public override string Texture => InfernumTextureRegistry.InvisPath;

        public override void SetStaticDefaults() => DisplayName.SetDefault("Holy Ritual");

        public override void SetDefaults()
        {
            projectile.width = projectile.height = 2;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;
            projectile.penetrate = -1;
            projectile.timeLeft = Lifetime;
            projectile.hide = false;
        }

        public override void AI()
        {
            projectile.scale = Utils.InverseLerp(0f, 30f, Time, true) * Utils.InverseLerp(0f, 30f, projectile.timeLeft, true);
            projectile.rotation += projectile.scale * 0.05f;

            // Emit fire particles.
            Dust fire = Dust.NewDustPerfect(projectile.Center + Main.rand.NextVector2Circular(225f, 225f) * projectile.scale, 6);
            fire.velocity = -Vector2.UnitY * Main.rand.NextFloat(1f, 5f);
            fire.scale *= 2.7f;
            fire.noGravity = true;

            Time++;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Texture2D ritual1 = ModContent.GetTexture("InfernumMode/ExtraTextures/GreyscaleObjects/Ritual");
            Texture2D ritual2 = ModContent.GetTexture("InfernumMode/ExtraTextures/GreyscaleObjects/Ritual2");
            Vector2 drawPosition = projectile.Center - Main.screenPosition;
            Vector2 scale1 = Vector2.One * projectile.scale * 240f / ritual2.Size() * 2f;

            float colorInterpolant = (float)Math.Cos(MathHelper.TwoPi * Time / 60f) * 0.5f + 0.5f;
            Color color1 = Color.Lerp(Color.Wheat, ProvidenceBehaviorOverride.IsEnraged ? Color.Cyan : Color.Yellow, colorInterpolant * 0.6f) * projectile.scale;

            Main.spriteBatch.SetBlendState(BlendState.Additive);
            Main.spriteBatch.Draw(ritual1, drawPosition, null, color1 * 1.35f, 0f, ritual1.Size() * 0.5f, scale1 * 2.22f, 0, 0);
            Main.spriteBatch.Draw(ritual2, drawPosition, null, color1, projectile.rotation, ritual2.Size() * 0.5f, scale1, 0, 0);
            Main.spriteBatch.ResetBlendState();

            return false;
        }

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers)
        {
            behindNPCs.Add(index);
        }
    }
}
