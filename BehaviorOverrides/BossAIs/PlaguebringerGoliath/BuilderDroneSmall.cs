using CalamityMod.Events;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.PlaguebringerGoliath
{
    public class BuilderDroneSmall : ModNPC
    {
        public Vector2 GeneralHoverPosition;
        public Player Target => Main.player[npc.target];
        public ref float GeneralTimer => ref npc.ai[0];

        public override string Texture => "InfernumMode/BehaviorOverrides/BossAIs/PlaguebringerGoliath/SmallDrone";
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Small Builder Drone");
            Main.npcFrameCount[npc.type] = 5;
        }

        public override void SetDefaults()
        {
            npc.damage = 100;
            npc.npcSlots = 0f;
            npc.width = npc.height = 42;
            npc.defense = 15;
            npc.lifeMax = 1200;
            if (BossRushEvent.BossRushActive)
                npc.lifeMax = 11256;

            npc.aiStyle = aiType = -1;
            npc.knockBackResist = 0f;
            npc.noGravity = true;
            npc.noTileCollide = true;
            npc.canGhostHeal = false;
            npc.HitSound = SoundID.NPCHit4;
            npc.DeathSound = SoundID.NPCDeath14;
        }

        public override void AI()
        {
            Lighting.AddLight(npc.Center, 0.03f, 0.2f, 0f);

            // Handle despawn stuff.
            if (!Target.active || Target.dead)
            {
                npc.TargetClosest(false);
                if (!Target.active || Target.dead)
                {
                    if (npc.timeLeft > 10)
                        npc.timeLeft = 10;
                    return;
                }
            }
            else if (npc.timeLeft > 600)
                npc.timeLeft = 600;

            npc.dontTakeDamage = GeneralTimer < 60f;

            Vector2 continousHoverPosition = Target.Center + new Vector2(-280f, -225f);
            continousHoverPosition += (npc.whoAmI * 1.58436f).ToRotationVector2() * (float)Math.Cos(GeneralTimer / 17f) * 42f;
            if (Vector2.Distance(GeneralHoverPosition, continousHoverPosition) > 325f)
                GeneralHoverPosition = continousHoverPosition;

            // Move in the general area of the hover position if not noticeably close or movement is very low.
            if (!npc.WithinRange(continousHoverPosition, 95f) || npc.velocity.Length() < 2.25f)
                npc.SimpleFlyMovement(npc.SafeDirectionTo(GeneralHoverPosition) * 11f, 0.9f);

            // Explode into rockets if the big builder is gone or the nuke has been launched.
            if (!NPC.AnyNPCs(ModContent.NPCType<BuilderDroneBig>()) || GeneralTimer >= PlagueNuke.BuildTime)
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    Vector2 rocketVelocity = Main.rand.NextVector2Unit() * Main.rand.NextFloat(9f, 13f);
                    Vector2 rocketSpawnPosition = npc.Center + rocketVelocity * 4f;
                    Utilities.NewProjectileBetter(rocketSpawnPosition, rocketVelocity, ModContent.ProjectileType<RedirectingPlagueMissile>(), 160, 0f);
                }

                npc.life = 0;
                npc.checkDead();
                npc.active = false;
                return;
            }

            // Randomly play sounds to indicate building.
            if (Main.rand.NextBool(45))
            {
                NPC nuke = Main.npc[NPC.FindFirstNPC(ModContent.NPCType<PlagueNuke>())];
                Vector2 end = nuke.Center + Main.rand.NextVector2Circular(8f, 8f);
                Dust.QuickDust(npc.Center, Color.Lime).scale = 1.4f;
                Dust.QuickDust(end, Color.Lime).scale = 1.4f;
                for (float num2 = 0f; num2 < 1f; num2 += 0.01f)
                    Dust.QuickDust(Vector2.Lerp(npc.Center, end, num2), Color.Lime).scale = 0.95f;

                switch (Main.rand.Next(4))
                {
                    case 0:
                        Main.PlaySound(SoundID.Item12, npc.Center);
                        break;
                    case 1:
                        Main.PlaySound(SoundID.Item15, npc.Center);
                        break;
                    case 2:
                        Main.PlaySound(SoundID.Item22, npc.Center);
                        break;
                    case 3:
                        Main.PlaySound(SoundID.Item23, npc.Center);
                        break;
                }
            }

            GeneralTimer++;
        }

        public override bool PreNPCLoot() => false;

        public override bool CheckDead()
        {
            Main.PlaySound(SoundID.DD2_KoboldExplosion, npc.position);

            npc.position = npc.Center;
            npc.width = npc.height = 84;
            npc.Center = npc.position;

            for (int i = 0; i < 15; i++)
            {
                Dust plague = Dust.NewDustDirect(npc.position, npc.width, npc.height, 89, 0f, 0f, 100, default, 1.4f);
                if (Main.rand.NextBool(2))
                {
                    plague.scale = 0.5f;
                    plague.fadeIn = Main.rand.NextFloat(1f, 2f);
                }
                plague.velocity *= 3f;
                plague.noGravity = true;
            }

            for (int i = 0; i < 30; i++)
            {
                Dust plague = Dust.NewDustDirect(npc.position, npc.width, npc.height, 89, 0f, 0f, 100, default, 1.85f);
                plague.noGravity = true;
                plague.velocity *= 5f;

                plague = Dust.NewDustDirect(npc.position, npc.width, npc.height, 89, 0f, 0f, 100, default, 2f);
                plague.velocity *= 2f;
                plague.noGravity = true;
            }

            return true;
        }

        public override void FindFrame(int frameHeight)
        {
            npc.frameCounter++;

            if (npc.frameCounter >= 5D)
            {
                npc.frame.Y += frameHeight;
                if (npc.frame.Y >= frameHeight * Main.npcFrameCount[npc.type])
                    npc.frame.Y = 0;

                npc.frameCounter = 0D;
            }
        }
    }
}
