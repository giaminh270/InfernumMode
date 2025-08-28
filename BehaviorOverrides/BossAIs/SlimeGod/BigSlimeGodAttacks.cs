﻿using CalamityMod.NPCs;
using CalamityMod.NPCs.SlimeGod;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;
using static InfernumMode.BehaviorOverrides.BossAIs.SlimeGod.SlimeGodComboAttackManager;

namespace InfernumMode.BehaviorOverrides.BossAIs.SlimeGod
{
    public static class BigSlimeGodAttacks
    {
        public static void DoBehavior_LongJumps(NPC npc, Player target, bool red, bool alone, ref float attackTimer)
        {
            float lifeRatio = npc.life / (float)npc.lifeMax;

            int globCount = (int)MathHelper.Lerp(7f, 10f, 1f - lifeRatio);
            float globSpeed = MathHelper.Lerp(6.8f, 8.75f, 1f - lifeRatio);
            float jumpDelay = MathHelper.Lerp(48f, 26f, 1f - lifeRatio);
            if (alone)
            {
                globSpeed += 0.8f;
                jumpDelay -= 6f;
            }

            ref float jumpCounter = ref npc.Infernum().ExtraAI[0];
            ref float noTileCollisionCountdown = ref npc.Infernum().ExtraAI[1];
            ref float stuckTimer = ref npc.Infernum().ExtraAI[2];

            // Slow down and prepare to jump if on the ground.
            if ((npc.velocity.Y == 0f && Collision.SolidCollision(npc.BottomLeft - Vector2.UnitY * 8f, npc.width, 16)) || stuckTimer >= 270f)
            {
                npc.velocity.X *= 0.5f;
                attackTimer++;

                if (attackTimer >= jumpDelay)
                {
                    attackTimer = 0f;
                    stuckTimer = 0f;
                    noTileCollisionCountdown = 10f;
                    jumpCounter++;

                    npc.velocity.Y -= 6f;
                    if (target.position.Y + target.height < npc.Center.Y)
                        npc.velocity.Y -= 1.25f;
                    if (target.position.Y + target.height < npc.Center.Y - 40f)
                        npc.velocity.Y -= 1.5f;
                    if (target.position.Y + target.height < npc.Center.Y - 80f)
                        npc.velocity.Y -= 1.75f;
                    if (target.position.Y + target.height < npc.Center.Y - 120f)
                        npc.velocity.Y -= 2.5f;
                    if (target.position.Y + target.height < npc.Center.Y - 160f)
                        npc.velocity.Y -= 3f;
                    if (target.position.Y + target.height < npc.Center.Y - 200f)
                        npc.velocity.Y -= 3f;
                    if (target.position.Y + target.height < npc.Center.Y - 400f)
                        npc.velocity.Y -= 6.1f;
                    if (!Collision.CanHit(npc.Center, 1, 1, target.Center, 1, 1))
                        npc.velocity.Y -= 3.25f;
                    npc.velocity.Y *= 1.35f;

                    // Release a barrage of globs.
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        int globID = red ? ModContent.ProjectileType<DeceleratingCrimulanGlob>() : ModContent.ProjectileType<DeceleratingEbonianGlob>();
                        float offsetAngle = Main.rand.NextFloat(MathHelper.TwoPi);
                        for (int i = 0; i < globCount; i++)
                        {
                            Vector2 globShootVelocity = (MathHelper.TwoPi * i / globCount + offsetAngle).ToRotationVector2() * globSpeed;
                            Utilities.NewProjectileBetter(npc.Bottom, globShootVelocity, globID, 90, 0f);
                        }
                    }

                    npc.velocity.X = (target.Center.X > npc.Center.X).ToDirectionInt() * 16f;
                    npc.netUpdate = true;
                }
            }
            else
            {
                npc.noTileCollide = !Collision.SolidCollision(npc.position, npc.width, npc.height + 16) && npc.Bottom.Y < target.Center.Y;
                npc.noGravity = true;
                npc.velocity.Y = MathHelper.Clamp(npc.velocity.Y + 0.5f, -24f, 28f);
                attackTimer = 0f;
            }

            if (noTileCollisionCountdown > 0f)
            {
                npc.noTileCollide = true;
                noTileCollisionCountdown--;
            }

            stuckTimer++;
            if (jumpCounter >= 6)
                SelectNextAttack(npc);
        }

        public static void DoBehavior_GroundedGelSlam(NPC npc, Player target, bool red, bool alone, ref float attackTimer)
        {
            int maxHoverTime = 210;
            int maxSlamTime = 150;
            int sitTime = 42;
            int groundBlobCount = 18;
            int blobCount = 5;
            float globSpeed = 15f;

            if (alone)
            {
                sitTime -= 16;
                groundBlobCount += 3;
                blobCount += 2;
                globSpeed += 5.2f;
            }

            ref float hasSlammed = ref npc.Infernum().ExtraAI[0];
            ref float chargeOffsetDirection = ref npc.Infernum().ExtraAI[1];
            ref float slamCounter = ref npc.Infernum().ExtraAI[2];

            // Hover into position.
            if (attackTimer < maxHoverTime)
            {
                // Initialize the offset direction.
                if (chargeOffsetDirection == 0f)
                    chargeOffsetDirection = 1f;
                float hoverSpeed = Utilities.Remap(attackTimer, 0f, maxHoverTime, 23.5f, 38.5f);
                Vector2 hoverDestination = target.Center - Vector2.UnitY * 470f;
                npc.velocity = Vector2.Lerp(npc.velocity, Vector2.Zero.MoveTowards(hoverDestination - npc.Center, hoverSpeed), 0.2f);
                npc.noTileCollide = true;
                npc.damage = 0;

                // Make both slimes slam downward if they are sufficiently close to their hover destination.
                bool bothSlimesAreClose = npc.WithinRange(hoverDestination, 85f);
                if (bothSlimesAreClose)
                {
                    attackTimer = maxHoverTime;
                    npc.netUpdate = true;
                }

                if (attackTimer >= maxHoverTime - 1f)
                    npc.velocity.Y *= 0.4f;

                return;
            }

            // Slam downward.
            if (attackTimer < maxHoverTime + maxSlamTime)
            {
                float gravity = Utilities.Remap(attackTimer - maxHoverTime, 0f, 45f, 1.3f, 2.6f);
                npc.noGravity = true;
                npc.velocity.X *= 0.8f;
                npc.noTileCollide = npc.Bottom.Y < target.Bottom.Y;
                npc.velocity.Y = MathHelper.Clamp(npc.velocity.Y + gravity, -12f, 21f);
                if (Collision.SolidCollision(npc.TopLeft, npc.width, npc.height + 4) && !npc.noTileCollide)
                {
                    // Do collision effects after slamming.
                    if (hasSlammed == 0f)
                    {
                        // Release a bunch of falling slime into the air and towards the target.
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            for (int i = 0; i < groundBlobCount; i++)
                            {
                                float shootOffsetAngle = MathHelper.Lerp(-0.98f, 0.98f, i / (float)(groundBlobCount - 1f)) + Main.rand.NextFloatDirection() * 0.04f;
                                Vector2 globVelocity = -Vector2.UnitY.RotatedBy(shootOffsetAngle) * globSpeed;
                                Utilities.NewProjectileBetter(npc.Bottom, globVelocity, ModContent.ProjectileType<GroundSlimeGlob>(), 90, 0f);
                            }

                            int globID = red ? ModContent.ProjectileType<DeceleratingCrimulanGlob>() : ModContent.ProjectileType<DeceleratingEbonianGlob>();
                            for (int i = 0; i < blobCount; i++)
                            {
                                float shootOffsetAngle = MathHelper.Lerp(-0.98f, 0.98f, i / (float)(blobCount - 1f)) + Main.rand.NextFloatDirection() * 0.03f;
                                Vector2 globVelocity = npc.SafeDirectionTo(target.Center).RotatedBy(shootOffsetAngle) * globSpeed * 0.4f;
                                Utilities.NewProjectileBetter(npc.Bottom, globVelocity, globID, 90, 0f);
                            }
                        }


                        chargeOffsetDirection *= -1f;
                        attackTimer = maxHoverTime + maxSlamTime;
                        hasSlammed = 1f;
                        npc.velocity.Y = 0f;
                        npc.netUpdate = true;
                    }
                }
            }
            else
                hasSlammed = 1f;

            if (attackTimer >= maxHoverTime + maxSlamTime + sitTime)
            {
                attackTimer = 0f;
                hasSlammed = 0f;
                npc.netUpdate = true;

                slamCounter++;
                if (slamCounter >= 2f)
                    SelectNextAttack(npc);
            }
        }
        
        public static void DoBehavior_CoreSpinBursts(NPC npc, Player target, ref float attackTimer)
        {
            float lifeRatio = npc.life / (float)npc.lifeMax;

            float burstSpeed = MathHelper.Lerp(5.8f, 7f, 1f - lifeRatio);
            float jumpDelay = 10f;
            float coreChargeSpeed = 24.5f;

            bool touchingGround = Collision.SolidCollision(npc.BottomLeft - Vector2.UnitY * 8f, npc.width, 16);
            ref float jumpCounter = ref npc.Infernum().ExtraAI[0];
            ref float noTileCollisionCountdown = ref npc.Infernum().ExtraAI[1];
            ref float stuckTimer = ref npc.Infernum().ExtraAI[2];
            ref float coreChargeCounter = ref npc.Infernum().ExtraAI[3];
            ref float universalSpinTimer = ref npc.Infernum().ExtraAI[4];

            NPC core = Main.npc[CalamityGlobalNPC.slimeGod];

            // Make the core charge.
            universalSpinTimer++;
            core.ai[0] = (int)SlimeGodCoreBehaviorOverride.SlimeGodCoreAttackType.DoAbsolutelyNothing;
            core.damage = 0;
            core.rotation += core.velocity.X * 0.02f;
            if (coreChargeCounter >= 1f)
            {
                if (touchingGround)
                    npc.velocity.X *= 0.5f;
                core.damage = core.defDamage;
                coreChargeCounter++;
                if (coreChargeCounter >= 50f)
                    core.velocity *= 0.95f;
                if (coreChargeCounter >= 64f)
                {
                    coreChargeCounter = 0f;
                    npc.netUpdate = true;
                }
                return;
            }

            // Make the core spin.
            Vector2 coreHoverDestination = npc.Center + (MathHelper.TwoPi * universalSpinTimer / 45f).ToRotationVector2() * 300f;
            core.Center = Vector2.Lerp(core.Center, coreHoverDestination, 0.05f);
            core.velocity = core.SafeDirectionTo(coreHoverDestination) * 32f;

            // Slow down and prepare to jump if on the ground.
            if ((npc.velocity.Y == 0f && touchingGround) || stuckTimer >= 600f)
            {
                npc.velocity.X *= 0.5f;
                attackTimer++;

                if (attackTimer >= jumpDelay && 
                    npc.SafeDirectionTo(target.Center).RotatedBy(-MathHelper.PiOver2).AngleBetween(core.Center - npc.Center) < 0.28f &&
                    !core.WithinRange(npc.Center, 100f) &&
                    stuckTimer >= 30f)
                {
                    attackTimer = 0f;
                    stuckTimer = 0f;
                    noTileCollisionCountdown = 10f;
                    jumpCounter++;

                    npc.velocity.Y -= 6f;
                    if (target.position.Y + target.height < npc.Center.Y)
                        npc.velocity.Y -= 1.25f;
                    if (target.position.Y + target.height < npc.Center.Y - 40f)
                        npc.velocity.Y -= 1.5f;
                    if (target.position.Y + target.height < npc.Center.Y - 80f)
                        npc.velocity.Y -= 1.75f;
                    if (target.position.Y + target.height < npc.Center.Y - 120f)
                        npc.velocity.Y -= 2.5f;
                    if (target.position.Y + target.height < npc.Center.Y - 160f)
                        npc.velocity.Y -= 3f;
                    if (target.position.Y + target.height < npc.Center.Y - 200f)
                        npc.velocity.Y -= 3f;
                    if (target.position.Y + target.height < npc.Center.Y - 400f)
                        npc.velocity.Y -= 6.1f;
                    if (!Collision.CanHit(npc.Center, 1, 1, target.Center, 1, 1))
                        npc.velocity.Y -= 3.25f;
                    npc.velocity.Y *= 1.35f;

                    // Release a barrage of globs.
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        core.velocity = core.SafeDirectionTo(target.Center) * coreChargeSpeed;
                        int globID = npc.type == ModContent.NPCType<SlimeGodRun>() ? ModContent.ProjectileType<DeceleratingCrimulanGlob>() : ModContent.ProjectileType<DeceleratingEbonianGlob>();
                        for (int i = 0; i < 7; i++)
                        {
                            float shootOffsetAngle = MathHelper.Lerp(-0.63f, 0.63f, i / 7f);
                            Vector2 globShootVelocity = npc.SafeDirectionTo(target.Center).RotatedBy(shootOffsetAngle) * burstSpeed;
                            Utilities.NewProjectileBetter(npc.Bottom, globShootVelocity, globID, 90, 0f);
                        }
                        coreChargeCounter = 1f;
                        npc.netUpdate = true;
                    }

                    npc.velocity.X = (target.Center.X > npc.Center.X).ToDirectionInt() * 16f;
                    npc.netUpdate = true;
                }
            }
            else
            {
                npc.noTileCollide = !Collision.SolidCollision(npc.position, npc.width, npc.height + 16) && npc.Bottom.Y < target.Center.Y;
                npc.noGravity = true;
                npc.velocity.Y = MathHelper.Clamp(npc.velocity.Y + 0.5f, -24f, 28f);
                attackTimer = 0f;
            }

            if (noTileCollisionCountdown > 0f)
            {
                npc.noTileCollide = true;
                noTileCollisionCountdown--;
            }

            stuckTimer++;
            if (jumpCounter >= 5)
            {
                core.ai[0] = (int)SlimeGodCoreBehaviorOverride.SlimeGodCoreAttackType.HoverAndDoNothing;
                core.netUpdate = true;
                SelectNextAttack(npc);
            }
        }

        public static void SelectNextAttack(NPC npc)
        {
            npc.ai[1] = 0f;
            for (int i = 0; i < 5; i++)
                npc.Infernum().ExtraAI[i] = 0f;

            ref float localState = ref npc.ai[0];
            float oldLocalState = localState;

            int tries = 0;
            WeightedRandom<BigSlimeGodAttackType> newStatePicker = new WeightedRandom<BigSlimeGodAttackType>(Main.rand);
            newStatePicker.Add(BigSlimeGodAttackType.LongJumps);
            newStatePicker.Add(BigSlimeGodAttackType.GroundedGelSlam);
            if (SlimeGodComboAttackManager.FightState == SlimeGodFightState.AloneSingleLargeSlimeEnraged)
                newStatePicker.Add(BigSlimeGodAttackType.CoreSpinBursts);

            do
            {
                localState = (int)newStatePicker.Get();
                tries++;
            }
            while (localState == oldLocalState && tries < 1000);
            SlimeGodComboAttackManager.SelectNextAttackSpecific(npc);
            npc.netUpdate = true;
        }
    }
}
