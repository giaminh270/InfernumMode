using CalamityMod;
using InfernumMode.ExtraTextures;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.Draedon.ArtemisAndApollo
{
    public class ArtemisLaserbeamTelegraph : ModProjectile
    {
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public NPC Artemis => Main.npc.IndexInRange((int)projectile.ai[0]) ? Main.npc[(int)projectile.ai[0]] : null;

        public float ConvergenceRatio => MathHelper.SmoothStep(0f, 1f, Utils.InverseLerp(25f, 120f, Time, true));

        public ref float StartingRotationalOffset => ref projectile.ai[1];

        public ref float ConvergenceAngle => ref projectile.localAI[0];

        public ref float Time => ref projectile.localAI[1];

        public static int TrueLifetime => RawLifetime / TotalUpdates;

        public const int TotalUpdates = 4;

        public const int RawLifetime = 210;

        public const float TelegraphWidth = 3600f;

        public const float BeamPosOffset = 16f;

        public override void SetStaticDefaults() => DisplayName.SetDefault("Laserbeam Telegraph");

        public override void SetDefaults()
        {
            projectile.width = projectile.height = 4;
            projectile.ignoreWater = true;
            projectile.tileCollide = false;
            projectile.Opacity = 0f;
            projectile.penetrate = -1;
            projectile.MaxUpdates = TotalUpdates;
            projectile.timeLeft = RawLifetime;
            cooldownSlot = 1;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(ConvergenceAngle);
            writer.Write(Time);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            ConvergenceAngle = reader.ReadSingle();
            Time = reader.ReadSingle();
        }

        public override void AI()
        {
            // Die if the thing to attach to disappears.
            if (Artemis is null || !Artemis.active)
            {
                projectile.Kill();
                return;
            }

            // Offset to move the beam forward so that it starts at the edge of Artemis' laser firing mechanism.
            float beamStartForwardsOffset = ExoMechManagement.ExoTwinsAreInSecondPhase ? 104f : 72f;

            // Set the starting location of the beam to the center of the NPC.
            projectile.Center = Artemis.Center;

            // Add the forwards offset, measured in pixels.
            float normalizedArtemisDirection = Artemis.rotation - MathHelper.PiOver2;
            projectile.position += normalizedArtemisDirection.ToRotationVector2() * beamStartForwardsOffset;
            projectile.rotation = StartingRotationalOffset.AngleLerp(ConvergenceAngle, ConvergenceRatio) + normalizedArtemisDirection;

            // Fade in.
            projectile.Opacity = MathHelper.Clamp(projectile.Opacity + 0.03f, 0f, 1f);

            Time++;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Main.spriteBatch.SetBlendState(BlendState.Additive);

            Texture2D laserTelegraph = InfernumTextureRegistry.BloomLineSmall;

            float verticalScale = Utils.InverseLerp(0f, 20f, Time, true) * Utils.InverseLerp(0f, 16f, projectile.timeLeft, true) * 0.5f;
            verticalScale += CalamityUtils.Convert01To010(Utils.InverseLerp(20f, 67f, projectile.timeLeft, true)) * 0.5f;

            Vector2 origin = laserTelegraph.Size() * new Vector2(0.5f, 0f);
            Vector2 scaleInner = new Vector2(verticalScale, TelegraphWidth / laserTelegraph.Height);
            Vector2 scaleOuter = scaleInner * new Vector2(2.2f, 1f);

            Color colorOuter = Color.Lerp(Color.Orange, Color.White, Utils.InverseLerp(67f, 0f, projectile.timeLeft, true) * 0.8f);
            Color colorInner = Color.Lerp(colorOuter, Color.White, 0.75f);

            Main.spriteBatch.Draw(laserTelegraph, projectile.Center - Main.screenPosition, null, colorOuter, projectile.rotation - MathHelper.PiOver2, origin, scaleOuter, SpriteEffects.None, 0);
            Main.spriteBatch.Draw(laserTelegraph, projectile.Center - Main.screenPosition, null, colorInner, projectile.rotation - MathHelper.PiOver2, origin, scaleInner, SpriteEffects.None, 0);
            Main.spriteBatch.ResetBlendState();
            return false;
        }
    }
}
