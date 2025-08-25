using CalamityMod.NPCs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.Polterghast
{
    public class SpinningSoul : ModProjectile
    {
        public bool CounterclockwiseSpin;

        public bool Cyan => projectile.ai[0] == 1f;

        public ref float SpinOffsetAngle => ref projectile.ai[1];

        public ref float SpinSpeedFactor => ref projectile.localAI[0];

        public ref float Radius => ref projectile.localAI[1];

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Soul");
            Main.projFrames[projectile.type] = 4;
            ProjectileID.Sets.TrailingMode[projectile.type] = 2;
            ProjectileID.Sets.TrailCacheLength[projectile.type] = 4;
        }

        public override void SetDefaults()
        {
            projectile.width = projectile.height = 32;
            projectile.hostile = true;
            projectile.friendly = false;
            projectile.ignoreWater = true;
            projectile.tileCollide = false;
            projectile.timeLeft = 270;
            projectile.penetrate = -1;
            cooldownSlot = 1;
        }

        // Sync local AI values.
        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(CounterclockwiseSpin);
            writer.Write(SpinSpeedFactor);
            writer.Write(Radius);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            CounterclockwiseSpin = reader.ReadBoolean();
            SpinSpeedFactor = reader.ReadSingle();
            Radius = reader.ReadSingle();
        }

        public override void AI()
        {
            if (!Main.npc.IndexInRange(CalamityGlobalNPC.ghostBoss))
            {
                projectile.Kill();
                return;
            }

            NPC polterghast = Main.npc[CalamityGlobalNPC.ghostBoss];
            Player target = Main.player[polterghast.target];

            // Spin around the Polterghast.
            if (Radius == 0f)
            {
                Radius = 1520f;
                if (!target.WithinRange(polterghast.Center, Radius))
                    Radius = target.Distance(polterghast.Center) + 100f;

                projectile.netUpdate = true;
            }
            Radius -= SpinSpeedFactor * 6f;
            SpinOffsetAngle -= MathHelper.ToRadians(MathHelper.Lerp(SpinSpeedFactor, 1f, 0.3f) * 1.5f) * CounterclockwiseSpin.ToDirectionInt();
            projectile.Center = polterghast.Center + SpinOffsetAngle.ToRotationVector2() * Radius;
            if (Radius <= 20f)
                projectile.Kill();

            // Handle fade effects and rotate.
            projectile.Opacity = Utils.InverseLerp(270f, 260f, projectile.timeLeft, true) * Utils.InverseLerp(0f, 25f, projectile.timeLeft, true);
            projectile.rotation = (projectile.position - projectile.oldPosition).ToRotation() - MathHelper.PiOver2;

            // Determine frames.
            projectile.frameCounter++;
            if (projectile.frameCounter % 5 == 4)
                projectile.frame = (projectile.frame + 1) % Main.projFrames[projectile.type];

            if (projectile.timeLeft % 18 == 17)
            {
                // Release a circle of dust every so often.
                for (int i = 0; i < 16; i++)
                {
                    Vector2 dustOffset = Vector2.UnitY.RotatedBy(MathHelper.TwoPi * i / 16f) * new Vector2(4f, 1f);
                    dustOffset = dustOffset.RotatedBy(projectile.velocity.ToRotation());

                    Dust ectoplasm = Dust.NewDustDirect(projectile.Center, 0, 0, 175, 0f, 0f);
                    ectoplasm.position = projectile.Center + dustOffset;
                    ectoplasm.velocity = dustOffset.SafeNormalize(Vector2.Zero) * 1.5f;
                    ectoplasm.color = Color.Lerp(Color.Purple, Color.White, 0.5f);
                    ectoplasm.scale = 1.5f;
                    ectoplasm.noGravity = true;
                }
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Texture2D texture = ModContent.GetTexture("InfernumMode/BehaviorOverrides/BossAIs/Polterghast/SoulLarge" + (Cyan ? "Cyan" : string.Empty));
            if (projectile.whoAmI % 2 == 0)
                texture = ModContent.GetTexture("InfernumMode/BehaviorOverrides/BossAIs/Polterghast/SoulMedium" + (Cyan ? "Cyan" : string.Empty));

            Utilities.DrawAfterimagesCentered(projectile, lightColor, ProjectileID.Sets.TrailingMode[projectile.type], 2, texture);
            return false;
        }

        public override void Kill(int timeLeft)
        {
            if (Main.npc.IndexInRange(CalamityGlobalNPC.ghostBoss) && Main.npc[CalamityGlobalNPC.ghostBoss].active)
            {
                Main.npc[CalamityGlobalNPC.ghostBoss].ai[2] = MathHelper.Clamp(Main.npc[CalamityGlobalNPC.ghostBoss].ai[2] - 1f, 0f, 500f);
                Main.npc[CalamityGlobalNPC.ghostBoss].netUpdate = true;
            }
        }

        public override Color? GetAlpha(Color lightColor)
        {
            Color color = Color.White;
            color.A = 0;
            return color * projectile.Opacity;
        }
    }
}
