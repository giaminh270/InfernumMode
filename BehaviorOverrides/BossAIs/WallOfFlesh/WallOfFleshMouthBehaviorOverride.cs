using CalamityMod;
using CalamityMod.Events;
using InfernumMode.OverridingSystem;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.WallOfFlesh
{
    public class WallOfFleshMouthBehaviorOverride : NPCBehaviorOverride
    {
        public override int NPCOverrideType => NPCID.WallofFlesh;

        public override NPCOverrideContext ContentToOverride => NPCOverrideContext.NPCAI;

        public const float Phase2LifeRatio = 0.45f;

        public override float[] PhaseLifeRatioThresholds => new float[]
        {
            Phase2LifeRatio
        };

        #region AI

        public override bool PreAI(NPC npc)
        {
            int totalAttachedEyes = 0;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                if (!Main.npc[i].active || Main.npc[i].type != NPCID.WallofFleshEye || Main.npc[i].Infernum().ExtraAI[2] != 0f)
                    continue;

                totalAttachedEyes++;
            }

            npc.Calamity().DR = MathHelper.Lerp(0.225f, 0.725f, Utils.InverseLerp(0f, 3f, totalAttachedEyes, true));

            ref float initialized01Flag = ref npc.localAI[0];
            ref float attackTimer = ref npc.ai[3];

            // Select a new target if an old one was lost.
            npc.TargetClosestIfTargetIsInvalid();
            Player target = Main.player[npc.target];
            float lifeRatio = MathHelper.Clamp(npc.life / (float)npc.lifeMax, 0f, 1f);

            // If the target isn't in the underworld check to see if anyone else is. If not, despawn.
            if (target.Center.Y <= (Main.maxTilesY - 300f) * 16f)
            {
                int newTarget = -1;
                for (int i = 0; i < Main.maxPlayers; i++)
                {
                    if (!Main.player[i].active || Main.player[i].dead)
                        continue;

                    if (Main.player[i].Center.Y > (Main.maxTilesY - 300f) * 16f)
                    {
                        newTarget = i;
                        break;
                    }
                }

                if (newTarget >= 0f)
                {
                    npc.target = newTarget;
                    npc.netUpdate = true;
                }
                else
                    npc.active = false;
            }

            Main.wof = npc.whoAmI;

            if (npc.Center.X <= 160f || npc.Center.X >= Main.maxTilesX * 16f - 160f)
            {
                npc.active = false;
                npc.netUpdate = true;
            }

            if (initialized01Flag == 0f)
            {
                Main.wofB = -1;
                Main.wofT = -1;

                SetEyePositions(npc);
                SummonEyes(npc);
                initialized01Flag = 1f;
            }

            SetEyePositions(npc);
            PerformMouthMotion(npc, lifeRatio);
            AngerEffects(npc, target);
            DetermineMouthRotation(npc, target);

            attackTimer++;

            // Have high DR when eyes are attached.
            npc.Calamity().DR = totalAttachedEyes > 0 ? 0.98f : 0.3f;
            npc.chaseable = totalAttachedEyes <= 0;

            int beamShootRate = 380;
            if (totalAttachedEyes <= 0 && attackTimer % beamShootRate == beamShootRate - 1f)
                PrepareFireBeam(npc, target);

            int miscEnemyCount = NPC.CountNPCS(NPCID.LeechHead) + NPC.CountNPCS(NPCID.TheHungryII);
            if (Main.netMode != NetmodeID.MultiplayerClient && miscEnemyCount < 3 && totalAttachedEyes <= 0 && attackTimer % 180f == 179f)
            {
                int leech = NPC.NewNPC((int)npc.Center.X, (int)npc.Center.Y, Main.rand.NextBool() ? NPCID.LeechHead : NPCID.TheHungryII);
                if (Main.npc.IndexInRange(leech))
                    Main.npc[leech].velocity = npc.velocity * 1.25f;
            }

            return false;
        }

        internal static void SetEyePositions(NPC npc)
        {
            int left = (int)(npc.Left.X / 16f);
            int right = (int)(npc.Right.X / 16f);
            int tries = 0;
            int y = (int)(npc.Center.Y / 16f) + 7;
            while (tries < 15 && y > Main.maxTilesY - 200)
            {
                y++;
                for (int x = left; x <= right; x++)
                {
                    try
                    {
                        if (WorldGen.SolidTile(x, y) || Main.tile[x, y].liquid > 0)
                            tries++;
                    }
                    catch
                    {
                        tries += 15;
                    }
                }
            }
            y += 4;
            if (Main.wofB == -1)
                Main.wofB = y * 16;
            else if (Main.wofB > y * 16)
            {
                Main.wofB--;
                if (Main.wofB < y * 16)
                    Main.wofB = y * 16;
            }
            else if (Main.wofB < y * 16)
            {
                Main.wofB++;
                if (Main.wofB > y * 16)
                    Main.wofB = y * 16;
            }

            tries = 0;
            y = (int)(npc.Center.Y / 16f) - 7;
            while (tries < 15 && y < Main.maxTilesY - 10)
            {
                y--;
                for (int x = left; x <= right; x++)
                {
                    try
                    {
                        if (WorldGen.SolidTile(x, y) || Main.tile[x, y].liquid > 0)
                            tries++;
                    }
                    catch
                    {
                        tries += 15;
                    }
                }
            }
            y -= 4;

            if (Main.wofT == -1)
                Main.wofT = y * 16;
            else if (Main.wofT > y * 16)
            {
                Main.wofT--;
                if (Main.wofT < y * 16)
                    Main.wofT = y * 16;
            }
            else if (Main.wofT < y * 16)
            {
                Main.wofT++;
                if (Main.wofT > y * 16)
                    Main.wofT = y * 16;
            }
        }

        internal static void PerformMouthMotion(NPC npc, float lifeRatio)
        {
            float verticalDestination = (Main.wofB + Main.wofT) / 2 - npc.height / 2;
            float horizontalSpeed = MathHelper.Lerp(4.35f, 7.4f, 1f - lifeRatio);
            if (verticalDestination < (Main.maxTilesY - 180) * 16f)
                verticalDestination = (Main.maxTilesY - 180) * 16f;

            if (BossRushEvent.BossRushActive)
                horizontalSpeed *= 2.4f;

            npc.position.Y = verticalDestination;

            if (npc.velocity.X == 0f)
                npc.velocity.X = npc.direction;

            if (npc.velocity.X < 0f)
            {
                npc.velocity.X = -horizontalSpeed;
                npc.direction = -1;
            }
            else
            {
                npc.velocity.X = horizontalSpeed;
                npc.direction = 1;
            }
        }

        internal static void SummonEyes(NPC npc)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            for (int i = 0; i < 5; i++)
            {
                int hungry = NPC.NewNPC((int)npc.position.X, (int)npc.Center.Y, NPCID.TheHungry, npc.whoAmI, 0f, 0f, 0f, 0f, 255);
                Main.npc[hungry].ai[0] = i * 0.2f - 0.05f;
            }

            List<float> offsetFactors = new List<float>();
            for (int i = 0; i < 4; i++)
            {
                float potentialOffsetFactor = Main.rand.NextFloat();
                while (offsetFactors.Any(factor => MathHelper.Distance(factor, potentialOffsetFactor) < 0.25f))
                {
                    i--;
                    continue;
                }

                NPC.NewNPC((int)npc.Center.X, (int)npc.Center.Y, NPCID.WallofFleshEye, ai0: potentialOffsetFactor);
            }
        }

        internal static void AngerEffects(NPC npc, Player target)
        {
            ref float angerStrength = ref npc.ai[0];
            ref float timeSpentRunning = ref npc.ai[1];
            ref float enrageAttackCountdown = ref npc.ai[2];
            ref float roarTimer = ref npc.localAI[2];

            float idealAngerStrength = MathHelper.Lerp(0f, 0.8f, Utils.InverseLerp(1150f, 2300f, Math.Abs(target.Center.X - npc.Center.X), true));

            // Check if the player is running in one direction.
            if (Math.Abs(Vector2.Dot(target.velocity.SafeNormalize(Vector2.Zero), Vector2.UnitX)) > 0.74f && Math.Abs(target.Center.X - npc.Center.X) > 500f)
                timeSpentRunning++;
            else
                timeSpentRunning--;

            if (roarTimer > 0)
                roarTimer--;

            timeSpentRunning = MathHelper.Clamp(timeSpentRunning, 0f, 1200f);
            idealAngerStrength += MathHelper.Lerp(0f, 0.2f, Utils.InverseLerp(240f, 420f, timeSpentRunning, true));
            idealAngerStrength = MathHelper.Clamp(idealAngerStrength, 0f, 1f);
            angerStrength = MathHelper.Lerp(angerStrength, idealAngerStrength, 0.03f);

            bool targetInHell = target.Center.Y > (Main.maxTilesY - 320f) * 16f;
            if (enrageAttackCountdown > 0)
            {
                // Summon tentacles near the player.
                if (Main.netMode != NetmodeID.MultiplayerClient && targetInHell && enrageAttackCountdown % 30f == 29f)
                {
                    Vector2 spawnPosition = target.Center + Main.rand.NextVector2CircularEdge(320f, 320f);
                    for (int tries = 0; tries < 2500; tries++)
                    {
                        int checkArea = 30 + tries / 20;
                        Vector2 potentialSpawnPosition = target.Center + target.velocity * 10f + Main.rand.NextVector2CircularEdge(checkArea, checkArea) * 16f;
                        Tile spawnTile = CalamityUtils.ParanoidTileRetrieval((int)potentialSpawnPosition.X / 16, (int)potentialSpawnPosition.Y / 16);
                        Tile aboveTile = CalamityUtils.ParanoidTileRetrieval((int)potentialSpawnPosition.X / 16, (int)potentialSpawnPosition.Y / 16 - 2);
                        Tile belowTile = CalamityUtils.ParanoidTileRetrieval((int)potentialSpawnPosition.X / 16, (int)potentialSpawnPosition.Y / 16 + 2);

                        bool aboveTileInvalid = aboveTile.active() && Main.tileSolid[aboveTile.type];
                        bool bottomTileInvalid = belowTile.active() && Main.tileSolid[belowTile.type];

                        if (spawnTile.nactive() && Main.tileSolid[spawnTile.type] && !aboveTileInvalid && !bottomTileInvalid)
                        {
                            spawnPosition = potentialSpawnPosition;
                            break;
                        }
                    }

                    spawnPosition = spawnPosition.ToTileCoordinates().ToWorldCoordinates();
                    Vector2 tentacleDirection = target.DirectionFrom(spawnPosition);
                    Utilities.NewProjectileBetter(spawnPosition, tentacleDirection, ModContent.ProjectileType<TileTentacle>(), 105, 0f);
                }
                enrageAttackCountdown--;
            }

            // Roaring and preparing for enrage attack.
            if (roarTimer <= 0f && targetInHell && enrageAttackCountdown <= 0f && idealAngerStrength >= 0.5f && angerStrength < idealAngerStrength)
            {
                if (Main.netMode != NetmodeID.Server)
                {
                    roarTimer = 660f;

                    if (Main.LocalPlayer.Center.Y > (Main.maxTilesY - 300f) * 16f)
                    {
                        // Scream.
                        Main.PlaySound(SoundID.Roar, (int)target.Center.X, (int)target.Center.Y, 1, 1f, 0.3f);

                        // Roar.
                        Main.PlaySound(SoundID.NPCKilled, (int)target.Center.X, (int)target.Center.Y, 10, 1.2f, 0.3f);
                    }
                }

                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    enrageAttackCountdown = 300f;
                    npc.netUpdate = true;
                }
            }
        }

        internal static void DetermineMouthRotation(NPC npc, Player target)
        {
            npc.spriteDirection = npc.direction;
            if (Utilities.AnyProjectiles(ModContent.ProjectileType<FireBeamWoF>()))
                return;

            if (npc.direction > 0)
            {
                if (target.Center.X > npc.Center.X)
                    npc.rotation = npc.AngleTo(target.Center);
                else
                    npc.rotation = MathHelper.Pi;
            }
            else if (target.Center.X < npc.Center.X)
                npc.rotation = npc.AngleTo(target.Center) + MathHelper.Pi;
            else
                npc.rotation = 0f;
        }

        internal static void PrepareFireBeam(NPC npc, Player target)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            Vector2 aimDirection = npc.SafeDirectionTo(target.Center);
            int fire = Utilities.NewProjectileBetter(npc.Center, aimDirection, ModContent.ProjectileType<FireBeamTelegraph>(), 0, 0f);
            if (Main.projectile.IndexInRange(fire))
                Main.projectile[fire].ai[1] = npc.whoAmI;
        }

        #endregion
    }
}
