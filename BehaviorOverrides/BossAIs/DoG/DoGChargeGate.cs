using CalamityMod.NPCs.DevourerofGods;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.DoG
{
    public class DoGChargeGate : ModProjectile
    {
        public int Time;

        public bool TelegraphShouldAim = true;

        public Vector2 Destination;
        public float TelegraphDelay
        {
            get => projectile.localAI[0];
            set => projectile.localAI[0] = value;
        }
        public bool NoTelegraph => projectile.localAI[0] == 1f;

        public ref float TelegraphTotalTime => ref projectile.ai[1];

        public ref float Lifetime => ref projectile.localAI[1];

        public const int DefaultLifetime = 225;

        public const int FadeoutTime = 45;
        public const float TelegraphFadeTime = 18f;

        public const float TelegraphWidth = 6400f;

        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";
        public override void SetStaticDefaults() => DisplayName.SetDefault("Portal");

        public override void SetDefaults()
        {
            projectile.width = 580;
            projectile.height = 580;
            projectile.ignoreWater = true;
            projectile.tileCollide = false;
            projectile.alpha = 255;
            projectile.timeLeft = 600;
            projectile.penetrate = -1;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(Time);
            writer.Write(TelegraphShouldAim);
            writer.WriteVector2(Destination);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            Time = reader.ReadInt32();
            TelegraphShouldAim = reader.ReadBoolean();
            Destination = reader.ReadVector2();
        }

        public override void AI()
        {
            // Use fallbacks for the telegraph time and lifetime if nothing specific is defined.
            if (TelegraphTotalTime == 0f)
                TelegraphTotalTime = 75f;
            if (Lifetime == 0f)
                Lifetime = DefaultLifetime;

            if (!NoTelegraph && !NPC.AnyNPCs(ModContent.NPCType<DevourerofGodsHead>()))
            {
                projectile.Kill();
                return;
            }

            if (Time >= Lifetime)
                projectile.Kill();

            TelegraphDelay++;

            // Make the portal dissipate once ready.
            if (TelegraphDelay > TelegraphTotalTime)
                projectile.alpha = Utils.Clamp(projectile.alpha - 25, 0, 255);

            // Adjust the aim destination such that it approaches the closest player. This stops right before the telegraph line dissipates.
            if (TelegraphDelay < TelegraphTotalTime * 0.8f && TelegraphShouldAim)
            {
                Player target = Main.player[Player.FindClosest(projectile.Center, 1, 1)];
                Vector2 idealDestination = target.Center + target.velocity * new Vector2(36f, 27f);
                if (Destination == Vector2.Zero)
                    Destination = idealDestination;
                Destination = Vector2.Lerp(Destination, idealDestination, 0.1f).MoveTowards(idealDestination, 5f);
            }
            Time++;
        }

        // TODO -- Potentially try to put the portal drawing code into a utility method of sorts?
        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            float fade = Utils.InverseLerp(0f, 35f, Time, true);
            if (Time >= Lifetime - FadeoutTime)
                fade = Utils.InverseLerp(Lifetime, Lifetime - FadeoutTime, Time, true);

            Texture2D noiseTexture = ModContent.GetTexture("CalamityMod/ExtraTextures/VoronoiShapes");
            Vector2 drawPosition = projectile.Center - Main.screenPosition;
            Vector2 origin2 = noiseTexture.Size() * 0.5f;
            if (NoTelegraph)
            {
                spriteBatch.EnterShaderRegion();

                GameShaders.Misc["CalamityMod:DoGPortal"].UseOpacity(fade);
                GameShaders.Misc["CalamityMod:DoGPortal"].UseColor(new Color(0.2f, 1f, 1f, 0f));
                GameShaders.Misc["CalamityMod:DoGPortal"].UseSecondaryColor(new Color(1f, 0.2f, 1f, 0f));
                GameShaders.Misc["CalamityMod:DoGPortal"].Apply();

                spriteBatch.Draw(noiseTexture, drawPosition, null, Color.White, 0f, origin2, projectile.scale * 3.5f, SpriteEffects.None, 0f);
                spriteBatch.ExitShaderRegion();
                return false;
            }

            Texture2D laserTelegraph = ModContent.GetTexture("CalamityMod/ExtraTextures/LaserWallTelegraphBeam");
            float yScale = 4f;
            if (TelegraphDelay < TelegraphFadeTime)
            {
                yScale = MathHelper.Lerp(0f, 2f, TelegraphDelay / 15f);
            }
            if (TelegraphDelay > TelegraphTotalTime - TelegraphFadeTime)
            {
                yScale = MathHelper.Lerp(2f, 0f, (TelegraphDelay - (TelegraphTotalTime - TelegraphFadeTime)) / 15f);
            }
            Vector2 scaleInner = new Vector2(TelegraphWidth / laserTelegraph.Width, yScale);
            Vector2 origin = laserTelegraph.Size() * new Vector2(0f, 0.5f);
            Vector2 scaleOuter = scaleInner * new Vector2(1f, 1.9f);

            Color colorOuter = Color.Lerp(Color.Cyan, Color.Purple, TelegraphDelay / TelegraphTotalTime * 4f % 1f); // Iterate through purple and cyan once and then flash.
            Color colorInner = Color.Lerp(colorOuter, Color.White, 0.85f);

            colorOuter *= 0.7f;
            colorInner *= 0.7f;
            colorInner.A = 72;

            spriteBatch.Draw(laserTelegraph, projectile.Center - Main.screenPosition, null, colorInner, projectile.AngleTo(Destination), origin, scaleInner, SpriteEffects.None, 0f);
            spriteBatch.Draw(laserTelegraph, projectile.Center - Main.screenPosition, null, colorOuter, projectile.AngleTo(Destination), origin, scaleOuter, SpriteEffects.None, 0f);

            spriteBatch.EnterShaderRegion();

            GameShaders.Misc["CalamityMod:DoGPortal"].UseOpacity(fade);
            GameShaders.Misc["CalamityMod:DoGPortal"].UseColor(Color.Cyan);
            GameShaders.Misc["CalamityMod:DoGPortal"].UseSecondaryColor(Color.Fuchsia);
            GameShaders.Misc["CalamityMod:DoGPortal"].Apply();

            spriteBatch.Draw(noiseTexture, drawPosition, null, Color.White, 0f, origin2, 2.7f, SpriteEffects.None, 0f);
            spriteBatch.ExitShaderRegion();
            return false;
        }
    }
}
