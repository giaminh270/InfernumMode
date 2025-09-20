using CalamityMod.Dusts;
using CalamityMod.World;
using CalamityMod.NPCs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using ProvidenceBoss = CalamityMod.NPCs.Providence.Providence;

namespace InfernumMode.BehaviorOverrides.BossAIs.Providence
{
    public class GroundCrystalSpike : ModProjectile
    {
        public bool SpikesShouldExtendOutward;

        public ref float SpikeReach => ref projectile.ai[0];

        public ref float SpikeDirection => ref projectile.ai[1];

        public override void SetStaticDefaults() => DisplayName.SetDefault("Crystal Spike");

        public override void SetDefaults()
        {
            projectile.width = 16;
            projectile.height = 16;
            projectile.ignoreWater = true;
            projectile.tileCollide = false;
            projectile.hostile = true;
            projectile.penetrate = -1;
            projectile.timeLeft = 900000;
            projectile.netImportant = true;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(SpikesShouldExtendOutward);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            SpikesShouldExtendOutward = reader.ReadBoolean();
        }

        public override void AI()
        {
            if (CalamityGlobalNPC.holyBoss == -1 || !Main.npc[CalamityGlobalNPC.holyBoss].active || Main.npc[CalamityGlobalNPC.holyBoss].type != ModContent.NPCType<ProvidenceBoss>())
            {
                projectile.Kill();
                return;
            }

            if (SpikesShouldExtendOutward)
                SpikeReach = MathHelper.Clamp(SpikeReach + 8f, 0f, 125f);


			if (SpikesShouldExtendOutward && SpikeReach >= 125f)
			{
				for (int i = 12; i < 125; i++)
				{
					if (Collision.SolidCollision(projectile.Center + SpikeDirection.ToRotationVector2() * i, 1, 1))
					{
						projectile.Kill();
						return;
					}
				}
			}
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            if (SpikeReach <= 0f)
                return false;

            float _ = 0f;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), projectile.Center, projectile.Center + SpikeDirection.ToRotationVector2() * SpikeReach, 4f, ref _);
        }

        public override Color? GetAlpha(Color lightColor)
        {
            Color c = (Main.dayTime && !CalamityWorld.malice) ? Color.Lerp(Color.Orange, Color.Yellow, 0.8f) : Color.Lerp(Color.Cyan, Color.Lime, 0.15f);
            c = Color.Lerp(c, Color.White, 0.4f);

            c.A = 0;
            return c * projectile.Opacity;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Texture2D spikeChain = ModContent.GetTexture("InfernumMode/BehaviorOverrides/BossAIs/Providence/GroundCrystalSpikePillar");

            // Draw the spike.
            Texture2D spikeTipTexture = Main.projectileTexture[projectile.type];
            Vector2 spikeTip = projectile.Center + Vector2.UnitY * SpikeDirection * SpikeReach;
            float frameHeight = Vector2.Distance(projectile.Center + SpikeDirection.ToRotationVector2() * 5f, spikeTip) - projectile.velocity.Length();
            float frameTop = spikeChain.Height - frameHeight;
            if (frameHeight > 0f)
            {
                float spikeRotation = SpikeDirection + MathHelper.PiOver2;
                Rectangle spikeFrame = new Rectangle(0, (int)frameTop, spikeChain.Width, (int)frameHeight);

                for (int i = 0; i < 1; i++)
                {
                    Main.spriteBatch.Draw(spikeChain, spikeTip - Main.screenPosition, spikeFrame, projectile.GetAlpha(Color.White), spikeRotation, new Vector2(spikeChain.Width / 2f, 0f), 1f, 0, 0f);
                    Main.spriteBatch.Draw(spikeTipTexture, spikeTip - Main.screenPosition, null, projectile.GetAlpha(Color.White), spikeRotation + MathHelper.Pi, new Vector2(spikeTipTexture.Width / 2f, 0f), 1f, 0, 0f);
                }
            }
            return false;
        }
    }
}
