using CalamityMod.DataStructures;
using InfernumMode.DataStructures;
using CalamityMod.NPCs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.SupremeCalamitas.Symbols
{
    public class SCalSymbol : ModProjectile, IAdditiveDrawer
    {
        public ref float Time => ref projectile.ai[0];

        public const int Lifetime = 300;

        public static readonly string[] AlchemicalNames = new string[]
        {
            "Brimstone",
            "Elements",
            "Fire",
            "Jupiter",
            "Lead",
            "PhilosophersStone",
            "Platinum",
            "Salt"
        };

        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public override void SetStaticDefaults() => DisplayName.SetDefault("Alchemical Symbol");

        public override void SetDefaults()
        {
            projectile.width = 256;
            projectile.height = 256;
            projectile.ignoreWater = true;
            projectile.tileCollide = false;
            projectile.netImportant = true;
            projectile.penetrate = -1;
            projectile.timeLeft = Lifetime;
            projectile.Opacity = 0f;
        }

        // Projectile spawning code is done in SCal's AI.
        public override void AI()
        {
            // Die if SCal is gone.
            if (CalamityGlobalNPC.SCal == -1 || !Main.npc[CalamityGlobalNPC.SCal].active)
            {
                projectile.Kill();
                return;
            }

            // Fade in and out.
            projectile.Opacity = Utils.InverseLerp(0f, 108f, Time, true) * Utils.InverseLerp(0f, 60f, projectile.timeLeft, true);
            projectile.scale = projectile.Opacity * 0.5f + 0.001f;
            projectile.velocity.Y = (float)Math.Sin(Time / 42f + projectile.identity) * 2.1f;

            Time++;
        }

        public void AdditiveDraw(SpriteBatch spriteBatch)
        {
            Texture2D tex = ModContent.GetTexture($"InfernumMode/BehaviorOverrides/BossAIs/SupremeCalamitas/Symbols/{AlchemicalNames[projectile.identity % 8]}");
            Rectangle frame = tex.Frame(1, Main.projFrames[projectile.type], 0, projectile.frame);
            Vector2 drawPosition;
            Vector2 origin = frame.Size() * 0.5f;
            Color glowColor = Color.Lerp(Color.Pink, Color.Red, (float)Math.Cos(Main.GlobalTime * 5f) * 0.5f + 0.5f);

            // Draw an ominous glowing backimage of the book after a bit of time.
            float outwardFade = Main.GlobalTime * 0.4f % 1f;
            for (int i = 0; i < 8; i++)
            {
                float opacity = (1f - outwardFade) * Utils.InverseLerp(0f, 0.15f, outwardFade, true) * 0.6f;
                drawPosition = projectile.Center + (MathHelper.TwoPi * i / 8f).ToRotationVector2() * outwardFade * projectile.scale * 32f - Main.screenPosition;
                Main.spriteBatch.Draw(tex, drawPosition, frame, projectile.GetAlpha(glowColor) * opacity, projectile.rotation, origin, projectile.scale, 0, 0);
            }

            drawPosition = projectile.Center - Main.screenPosition;

            for (int i = 0; i < 3; i++)
                spriteBatch.Draw(tex, drawPosition, frame, projectile.GetAlpha(Color.White), projectile.rotation, origin, projectile.scale, 0, 0);
        }
    }
}
