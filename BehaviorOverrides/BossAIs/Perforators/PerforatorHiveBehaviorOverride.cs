using CalamityMod;
using CalamityMod.NPCs;
using CalamityMod.NPCs.Perforator;
using CalamityMod.Particles;
using CalamityMod.Projectiles.Boss;
using InfernumMode.BehaviorOverrides.BossAIs.BoC;
using InfernumMode.OverridingSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.Perforators
{
    public class PerforatorHiveBehaviorOverride : NPCBehaviorOverride
    {
        public enum PerforatorHiveAttackState
        {
            DiagonalBloodCharge,
            HorizontalCrimeraSpawnCharge,
            IchorBlasts,
            IchorSpinDash,
            SmallWormBursts,
            CrimeraWalls,
            MediumWormBursts,
            IchorRain,
            LargeWormBursts,
            IchorFountainCharge
        }

        public const float Phase2LifeRatio = 0.7f;

        public const float Phase3LifeRatio = 0.5f;

        public const float Phase4LifeRatio = 0.25f;

        public override int NPCOverrideType => ModContent.NPCType<PerforatorHive>();

        public override NPCOverrideContext ContentToOverride => NPCOverrideContext.NPCAI | NPCOverrideContext.NPCPreDraw;

        public override float[] PhaseLifeRatioThresholds => new float[]
        {
            Phase2LifeRatio,
            Phase3LifeRatio,
            Phase4LifeRatio
        };

        #region AI

        public override bool PreAI(NPC npc)
        {
            // Set a global whoAmI variable.
            CalamityGlobalNPC.perfHive = npc.whoAmI;

            // Set damage.
            npc.defDamage = 75;
            npc.damage = npc.defDamage;
            npc.dontTakeDamage = false;

            ref float attackState = ref npc.ai[0];
            ref float attackTimer = ref npc.ai[1];
            ref float wormSummonState = ref npc.ai[2];
            ref float finalPhaseTransitionTimer = ref npc.ai[3];
            ref float backafterimageGlowInterpolant = ref npc.localAI[0];
            ref float backgroundStrength = ref npc.localAI[1];

            // Reset certain things.
            npc.Calamity().DR = 0.2f;
            backafterimageGlowInterpolant = MathHelper.Clamp(backafterimageGlowInterpolant - 0.1f, 0f, 1f);

            float lifeRatio = npc.life / (float)npc.lifeMax;

            // Select a new target if an old one was lost.
            npc.TargetClosestIfTargetIsInvalid();
            if (!Main.player.IndexInRange(npc.target) || !Main.player[npc.target].active || Main.player[npc.target].dead || !npc.WithinRange(Main.player[npc.target].Center, 6400f))
            {
                DoDespawnEffects(npc);
                return false;
            }

            bool inPhase2 = lifeRatio <= Phase2LifeRatio;
            bool inPhase3 = lifeRatio <= Phase3LifeRatio;
            bool inPhase4 = lifeRatio <= Phase4LifeRatio;
            Player target = Main.player[npc.target];

            // Prepare worm summon states.
            HandleWormPhaseTriggers(npc, inPhase2, inPhase3, inPhase4, ref attackState, ref wormSummonState);

            // Calculate rotation.
            npc.rotation = MathHelper.Clamp(npc.velocity.X * 0.04f, -MathHelper.Pi / 6f, MathHelper.Pi / 6f);

            // Make the background glow crimson in the form phase, once the large worm is dead.
            if (inPhase4 && attackState != (int)PerforatorHiveAttackState.LargeWormBursts)
            {
                // Handle transition effects.
                if (finalPhaseTransitionTimer < 180f)
                {
                    // Slow down dramatically at first.
                    if (finalPhaseTransitionTimer < 90f)
                        npc.velocity *= 0.93f;

                    // Rise upward and create an explosion sound.
                    if (finalPhaseTransitionTimer == 45f)
                    {
                        Main.PlaySound(SoundID.Roar, (int)npc.Center.X, (int)npc.Center.Y, 0);
                        var explosionSound = Main.PlaySound(SoundID.DD2_BetsyFireballImpact, npc.Center);
                        if (explosionSound != null)
                        {
                            CalamityUtils.SafeVolumeChange(ref explosionSound, 2.2f);
                            explosionSound.Pitch = -0.4f;
                        }

                        if (Main.netMode != NetmodeID.MultiplayerClient)
                            Projectile.NewProjectile(npc.Center, Vector2.Zero, ModContent.ProjectileType<PerforatorWave>(), 0, 0f);

                        npc.velocity = -Vector2.UnitY * 12f;
                        npc.netUpdate = true;
                    }
                }

                finalPhaseTransitionTimer++;
                backgroundStrength = Utils.InverseLerp(45f, 150f, finalPhaseTransitionTimer, true);
            }

            switch ((PerforatorHiveAttackState)attackState)
            {
                case PerforatorHiveAttackState.DiagonalBloodCharge:
                    DoBehavior_DiagonalBloodCharge(npc, target, inPhase2, inPhase3, inPhase4, ref attackTimer);
                    break;
                case PerforatorHiveAttackState.HorizontalCrimeraSpawnCharge:
                    DoBehavior_HorizontalCrimeraSpawnCharge(npc, target, ref attackTimer);
                    break;
                case PerforatorHiveAttackState.IchorBlasts:
                    DoBehavior_IchorBlasts(npc, target, inPhase2, ref attackTimer);
                    break;
                case PerforatorHiveAttackState.IchorSpinDash:
                    DoBehavior_IchorSpinDash(npc, target, inPhase2, inPhase3, inPhase4, ref attackTimer);
                    break;
                case PerforatorHiveAttackState.SmallWormBursts:
                    DoBehavior_SmallWormBursts(npc, target, ref attackTimer);
                    break;
                case PerforatorHiveAttackState.CrimeraWalls:
                    DoBehavior_CrimeraWalls(npc, target, inPhase3, inPhase4, ref attackTimer);
                    break;
                case PerforatorHiveAttackState.MediumWormBursts:
                    DoBehavior_MediumWormBursts(npc, target, ref attackTimer, ref backafterimageGlowInterpolant);
                    break;
                case PerforatorHiveAttackState.IchorRain:
                    DoBehavior_IchorRain(npc, target, inPhase4, ref attackTimer);
                    break;
                case PerforatorHiveAttackState.LargeWormBursts:
                    DoBehavior_LargeWormBursts(npc, target, ref attackTimer);
                    break;
                case PerforatorHiveAttackState.IchorFountainCharge:
                    DoBehavior_IchorFountainCharge(npc, target, ref attackTimer);
                    break;
            }

            attackTimer++;
            return false;
        }

        public static void HandleWormPhaseTriggers(NPC npc, bool inPhase2, bool inPhase3, bool inPhase4, ref float attackState, ref float wormSummonState)
        {
            // Small worm phase.
            if (inPhase2 && wormSummonState == 0f)
            {
                SelectNextAttack(npc);
                CleanUpStrayProjectiles();
                attackState = (int)PerforatorHiveAttackState.SmallWormBursts;
                wormSummonState = 1f;
                return;
            }

            // Medium worm phase.
            if (inPhase3 && wormSummonState == 1f)
            {
                SelectNextAttack(npc);
                CleanUpStrayProjectiles();
                attackState = (int)PerforatorHiveAttackState.MediumWormBursts;
                wormSummonState = 2f;
                return;
            }

            // Medium worm phase.
            if (inPhase4 && wormSummonState == 2f)
            {
                SelectNextAttack(npc);
                CleanUpStrayProjectiles();
                attackState = (int)PerforatorHiveAttackState.LargeWormBursts;
                wormSummonState = 3f;
                return;
            }
        }

        public static void MakeWormEruptFromHive(NPC npc, Vector2 eruptDirection, float splatterIntensity, int wormHeadID)
        {
            Vector2 bloodSpawnPosition = npc.Center + Main.rand.NextVector2Circular(npc.width, npc.height) * 0.04f + eruptDirection * 50f;

            // Create a bunch of blood particles.
            for (int i = 0; i < 21; i++)
            {
                int bloodLifetime = Main.rand.Next(33, 54);
                float bloodScale = Main.rand.NextFloat(0.7f, 0.95f);
                Color bloodColor = Color.Lerp(Color.Red, Color.DarkRed, Main.rand.NextFloat());
                bloodColor = Color.Lerp(bloodColor, new Color(51, 22, 94), Main.rand.NextFloat(0.65f));

                if (Main.rand.NextBool(20))
                    bloodScale *= 2f;

                Vector2 bloodVelocity = eruptDirection.RotatedByRandom(0.81f) * splatterIntensity * Main.rand.NextFloat(11f, 23f);
                bloodVelocity.Y -= splatterIntensity * 12f;
                BloodParticle blood = new BloodParticle(bloodSpawnPosition, bloodVelocity, bloodLifetime, bloodScale, bloodColor);
                GeneralParticleHandler.SpawnParticle(blood);
            }
            for (int i = 0; i < 10; i++)
            {
                float bloodScale = Main.rand.NextFloat(0.35f, 0.4f);
                Color bloodColor = Color.Lerp(Color.Red, Color.DarkRed, Main.rand.NextFloat(0.5f, 1f));
                Vector2 bloodVelocity = eruptDirection.RotatedByRandom(0.9f) * splatterIntensity * Main.rand.NextFloat(9f, 14.5f);
                BloodParticle2 blood = new BloodParticle2(bloodSpawnPosition, bloodVelocity, 35, bloodScale, bloodColor);
                GeneralParticleHandler.SpawnParticle(blood);
            }

            // Spawn the Worm Bosstm.
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                NPC.NewNPC((int)npc.Center.X, (int)npc.Center.Y, wormHeadID, 1);

                // Reel back in pain, indicating that the worm physically burrowed out of the hive.
                npc.velocity = eruptDirection * -8f;
                npc.netUpdate = true;
            }
        }

        public static void DoDespawnEffects(NPC npc)
        {
            npc.damage = 0;
            npc.velocity = Vector2.Lerp(npc.Center, Vector2.UnitY * 21f, 0.08f);
            if (npc.timeLeft > 225)
                npc.timeLeft = 225;
        }

        public static void CleanUpStrayProjectiles()
        {
            Utilities.DeleteAllProjectiles(true,
                ModContent.ProjectileType<FallingIchor>(),
                ModContent.ProjectileType<FlyingIchor>(),
                ModContent.ProjectileType<IchorBlast>(),
                ModContent.ProjectileType<IchorSpit>(),
                ModContent.ProjectileType<ToothBall>(),
                ModContent.ProjectileType<IchorBlob>());
        }

        public static void DoBehavior_DiagonalBloodCharge(NPC npc, Player target, bool inPhase2, bool inPhase3, bool inPhase4, ref float attackTimer)
        {
            int chargeDelay = 55;
            int burstIchorCount = 5;
            int fallingIchorCount = 8;
            int chargeTime = 45;
            int chargeCount = 3;
            float chargeSpeed = 20f;

            if (inPhase2)
            {
                chargeDelay -= 8;
                chargeTime -= 4;
                chargeSpeed += 2.75f;
            }
            if (inPhase3)
            {
                chargeCount--;
                fallingIchorCount += 2;
                chargeSpeed += 1.5f;
            }
            if (inPhase4)
            {
                chargeDelay -= 5;
                fallingIchorCount += 2;
                burstIchorCount += 2;
            }

            ref float attackSubstate = ref npc.Infernum().ExtraAI[0];
            ref float chargeCounter = ref npc.Infernum().ExtraAI[1];

            // Hover into position.
            if (attackSubstate == 0f)
            {
                Vector2 hoverDestination = target.Center + new Vector2((target.Center.X < npc.Center.X).ToDirectionInt() * 375f, -270f);
                Vector2 idealVelocity = npc.SafeDirectionTo(hoverDestination) * 20f;

                npc.SimpleFlyMovement(idealVelocity, idealVelocity.Length() / 12f);
                npc.velocity = Vector2.Lerp(npc.velocity, idealVelocity, 0.08f);

                // Slow down and go to the next attack substate if sufficiently close to the hover destination.
                if (npc.WithinRange(hoverDestination, 75f))
                {
                    attackTimer = 0f;
                    attackSubstate = 1f;
                    npc.velocity *= 0.5f;
                    npc.netUpdate = true;
                }

                // Give up and perform a different attack if unable to reach the hover destination in time.
                if (attackTimer >= 480f)
                    SelectNextAttack(npc);
            }

            // Slow down in anticipation of a charge.
            if (attackSubstate == 1f)
            {
                npc.velocity *= 0.925f;

                // Create blood pulses periodically as an indicator of charging.
                if (attackTimer % 14f == 0f)
                {
                    for (int i = -4; i <= 4; i++)
                    {
                        if (i == 0)
                            continue;
                        Vector2 offsetDirection = Vector2.UnitY.RotatedBy(i * 0.22f + Main.rand.NextFloat(-0.32f, 0.32f));
                        Vector2 baseSpawnPosition = npc.Center + offsetDirection * 180;
                        for (int j = 0; j < 8; j++)
                        {
                            Vector2 dustSpawnPosition = baseSpawnPosition + Main.rand.NextVector2Circular(9f, 9f);
                            Vector2 dustVelocity = (npc.Center - dustSpawnPosition) * 0.07f;

                            Dust blood = Dust.NewDustPerfect(dustSpawnPosition, 5);
                            blood.scale = Main.rand.NextFloat(2.6f, 3f);
                            blood.velocity = dustVelocity;
                            blood.noGravity = true;
                        }
                    }
                }

                // Release ichor into the air that slowly falls and charge at the target.
                if (attackTimer >= chargeDelay)
                {
                    Main.PlaySound(SoundID.NPCHit20, npc.Center);
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        for (int i = 0; i < fallingIchorCount; i++)
                        {
                            float projectileOffsetInterpolant = i / (float)(fallingIchorCount - 1f);
                            float horizontalSpeed = MathHelper.Lerp(-16f, 16f, projectileOffsetInterpolant) + Main.rand.NextFloatDirection() / fallingIchorCount * 5f;
                            float verticalSpeed = Main.rand.NextFloat(-8f, -7f);
                            Vector2 ichorVelocity = new Vector2(horizontalSpeed, verticalSpeed);
                            Utilities.NewProjectileBetter(npc.Top + Vector2.UnitY * 10f, ichorVelocity, ModContent.ProjectileType<FallingIchor>(), 75, 0f);
                        }

                        for (int i = 0; i < burstIchorCount; i++)
                        {
                            float projectileOffsetInterpolant = i / (float)(burstIchorCount - 1f);
                            float offsetAngle = MathHelper.Lerp(-0.55f, 0.55f, projectileOffsetInterpolant);
                            Vector2 ichorVelocity = npc.SafeDirectionTo(target.Center).RotatedBy(offsetAngle) * 6.5f;
                            Utilities.NewProjectileBetter(npc.Center + ichorVelocity * 3f, ichorVelocity, ModContent.ProjectileType<FlyingIchor>(), 75, 0f);
                        }

                        attackTimer = 0f;
                        attackSubstate = 2f;
                        npc.velocity = npc.SafeDirectionTo(target.Center) * chargeSpeed;
                        npc.netUpdate = true;
                    }
                }
            }

            // Charge.
            if (attackSubstate == 2f)
            {
                if (attackTimer >= chargeTime)
                {
                    chargeCounter++;

                    if (chargeCounter >= chargeCount)
                        SelectNextAttack(npc);
                    attackSubstate = 0f;
                    attackTimer = 0f;
                    npc.velocity *= 0.45f;
                    npc.netUpdate = true;
                }
            }
        }

        public static void DoBehavior_HorizontalCrimeraSpawnCharge(NPC npc, Player target, ref float attackTimer)
        {
            int chargeDelay = 20;
            int chargeTime = 60;
            int crimeraSpawnCount = 1;
            int crimeraLimit = 3;
            int crimeraSpawnRate = chargeTime / crimeraSpawnCount;
            float hoverOffset = 500f;
            float chargeSpeed = hoverOffset / chargeTime * 2f;
            ref float attackSubstate = ref npc.Infernum().ExtraAI[0];

            // Hover into position.
            if (attackSubstate == 0f)
            {
                Vector2 hoverDestination = target.Center + new Vector2((target.Center.X < npc.Center.X).ToDirectionInt() * hoverOffset, -300f);
                Vector2 idealVelocity = npc.SafeDirectionTo(hoverDestination) * 20f;

                npc.SimpleFlyMovement(idealVelocity, idealVelocity.Length() / 20f);
                npc.velocity = Vector2.Lerp(npc.velocity, idealVelocity, 0.05f);

                // Slow down and go to the next attack substate if sufficiently close to the hover destination.
                if (npc.WithinRange(hoverDestination, 75f))
                {
                    attackTimer = 0f;
                    attackSubstate = 1f;
                    npc.velocity *= 0.5f;
                    npc.netUpdate = true;
                }

                // Give up and perform a different attack if unable to reach the hover destination in time.
                if (attackTimer >= 480f)
                    SelectNextAttack(npc);
            }

            // Slow down in anticipation of a charge.
            if (attackSubstate == 1f)
            {
                npc.velocity *= 0.925f;

                // Release ichor into the air that slowly falls and charge at the target.
                if (attackTimer >= chargeDelay)
                {
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        attackTimer = 0f;
                        attackSubstate = 2f;
                        npc.velocity = Vector2.UnitX * (target.Center.X > npc.Center.X).ToDirectionInt() * chargeSpeed;
                        npc.netUpdate = true;
                    }
                }
            }

            // Charge.
            if (attackSubstate == 2f)
            {
                // Summon Crimeras.
                bool enoughCrimerasAreAround = NPC.CountNPCS(NPCID.Crimera) >= crimeraLimit;
                if (attackTimer % crimeraSpawnRate == crimeraSpawnRate / 2 && !enoughCrimerasAreAround)
                {
                    Main.PlaySound(SoundID.NPCDeath23, npc.Center);
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                        NPC.NewNPC((int)npc.Center.X, (int)npc.Center.Y, NPCID.Crimera, npc.whoAmI);
                }

                if (attackTimer >= chargeTime)
                {
                    SelectNextAttack(npc);
                    npc.velocity *= 0.45f;
                    npc.netUpdate = true;
                }
            }
        }

        public static void DoBehavior_IchorBlasts(NPC npc, Player target, bool inPhase2, ref float attackTimer)
        {
            int fireDelay = 50;
            int shootRate = 45;
            int blastCount = 12;

            if (inPhase2)
                shootRate -= 4;

            ref float attackSubstate = ref npc.Infernum().ExtraAI[0];
            ref float reboundCoundown = ref npc.Infernum().ExtraAI[1];
            ref float universalTimer = ref npc.Infernum().ExtraAI[2];

            universalTimer++;

            float verticalHoverOffset = (float)Math.Sin(universalTimer / 13f) * 100f - 50f;
            Vector2 hoverDestination = target.Center + new Vector2((target.Center.X < npc.Center.X).ToDirectionInt() * 480f, verticalHoverOffset);
            if (reboundCoundown <= 0f)
            {
                Vector2 idealVelocity = npc.SafeDirectionTo(hoverDestination) * 20f;

                npc.SimpleFlyMovement(idealVelocity, idealVelocity.Length() / 60f);
                npc.velocity = Vector2.Lerp(npc.velocity, idealVelocity, 0.02f);
                if (MathHelper.Distance(npc.Center.X, hoverDestination.X) < 35f)
                {
                    npc.position.X = hoverDestination.X - npc.width * 0.5f;
                    npc.velocity.X = 0f;
                }
            }
            else
            {
                reboundCoundown--;
            }

            // Hover into position.
            if (attackSubstate == 0f)
            {
                // Slow down and go to the next attack substate if sufficiently close to the hover destination.
                if (npc.WithinRange(hoverDestination, 75f))
                {
                    attackTimer = 0f;
                    attackSubstate = 1f;
                    npc.netUpdate = true;
                }

                // Give up and perform a different attack if unable to reach the hover destination in time.
                if (attackTimer >= 480f)
                    SelectNextAttack(npc);
            }

            // Slow down in preparation of firing.
            if (attackSubstate == 1f)
            {
                // Slow down.
                reboundCoundown = 1f;
                npc.velocity = (npc.velocity * 0.95f).MoveTowards(Vector2.Zero, 0.75f);

                if (attackTimer >= fireDelay)
                {
                    attackTimer = 0f;
                    attackSubstate = 2f;
                    npc.netUpdate = true;
                }
            }

            // Fire ichor blasts.
            if (attackSubstate == 2f)
            {
                if (attackTimer % shootRate == shootRate - 1f)
                {
                    Main.PlaySound(SoundID.NPCHit20, npc.Center);
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            float offsetAngle = MathHelper.Lerp(-0.41f, 0.41f, i / 2f);
                            Vector2 shootVelocity = Vector2.UnitX * Math.Sign(target.Center.X - npc.Center.X) * 3.5f;
                            shootVelocity = shootVelocity.RotatedBy(offsetAngle);
                            Utilities.NewProjectileBetter(npc.Center + shootVelocity * 3f, shootVelocity, ModContent.ProjectileType<IchorBlast>(), 75, 0f);
                        }
                        npc.netUpdate = true;
                    }
                }

                if (attackTimer >= blastCount * shootRate)
                    SelectNextAttack(npc);
            }
        }

        public static void DoBehavior_IchorSpinDash(NPC npc, Player target, bool inPhase2, bool inPhase3, bool inPhase4, ref float attackTimer)
        {
            int blobReleaseRate = 10;
            int spinTime = 120;
            int chargeBlobCount = 8;
            int chargeTime = 35;
            int chargeSlowdownTime = 25;
            float spinRadius = 325f;
            float totalSpinArc = MathHelper.TwoPi;
            float chargeSpeed = 16.5f;
            float blobSpeedFactor = -1f;

            if (inPhase2)
            {
                blobSpeedFactor *= -0.7f;
            }

            if (inPhase3)
            {
                spinTime -= 15;
                blobReleaseRate--;
                chargeSpeed += 1.5f;
                blobSpeedFactor *= 1.1f;
            }

            if (inPhase4)
            {
                blobReleaseRate--;
                spinRadius += 75f;
                totalSpinArc *= 1.5f;
                chargeSpeed += 1.5f;
                blobSpeedFactor *= 1.1f;
                chargeBlobCount += 3;
            }

            ref float attackSubstate = ref npc.Infernum().ExtraAI[0];
            ref float spinDirection = ref npc.Infernum().ExtraAI[1];
            ref float spinOffsetAngle = ref npc.Infernum().ExtraAI[2];

            Vector2 spinCenter = target.Center + spinOffsetAngle.ToRotationVector2() * spinRadius;

            // Intiialize the ideal spin offset angle on the first frame.
            if (attackSubstate == 0f && attackTimer == 1f)
            {
                spinOffsetAngle = target.Center.X > npc.Center.X ? MathHelper.Pi : 0f;
                spinDirection = Main.rand.NextBool().ToDirectionInt();
                npc.netUpdate = true;
            }

            // Hover into position for the spin.
            if (attackSubstate == 0f)
            {
                npc.Center = Vector2.Lerp(npc.Center, spinCenter, 0.045f).MoveTowards(spinCenter, 12.5f);
                npc.velocity = Vector2.Zero;

                // Begin spinning once close enough to the ideal position.
                if (npc.WithinRange(spinCenter, 25f))
                {
                    attackSubstate = 1f;
                    attackTimer = 0f;
                    npc.netUpdate = true;
                }
            }

            // Begin spinning.
            if (attackSubstate == 1f)
            {
                npc.Center = spinCenter;
                npc.velocity = Vector2.Zero;

                // Make the spin slow down near the end, to make the impending charge readable.
                float spinSlowdownFactor = Utils.InverseLerp(spinTime - 1f, spinTime * 0.65f, attackTimer, true);
                spinOffsetAngle += totalSpinArc / spinTime * spinDirection * spinSlowdownFactor;

                // Release blobs away from the player periodically. These serve as arena obstacles for successive attacks.
                // Blobs are not fired if there are nearby tiles in the way of the blob's potential path.
                if (attackTimer % blobReleaseRate == blobReleaseRate - 1f)
                {
                    Vector2 blobVelocity = npc.SafeDirectionTo(target.Center) * blobSpeedFactor * 10f;
                    bool lineOfSightIsClear = Collision.CanHit(npc.Center, 1, 1, npc.Center + blobVelocity * 12f, 1, 1);

                    if (lineOfSightIsClear)
                    {
                        Main.PlaySound(SoundID.NPCHit20, npc.Center);
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            int blob = Utilities.NewProjectileBetter(npc.Center + blobVelocity, blobVelocity, ModContent.ProjectileType<IchorBlob>(), 75, 0f);
                            if (Main.projectile.IndexInRange(blob))
                                Main.projectile[blob].ai[1] = target.Center.Y;
                        }
                    }
                }

                // Charge at the target after the spin concludes.
                if (attackTimer >= spinTime)
                {
                    Main.PlaySound(SoundID.NPCDeath23, npc.Center);

                    Vector2 shootDirection = npc.SafeDirectionTo(target.Center) * (blobSpeedFactor > 0f).ToDirectionInt();
                    bool lineOfSightIsClear = Collision.CanHit(npc.Center, 1, 1, npc.Center + shootDirection * 120f, 1, 1);

                    if (Main.netMode != NetmodeID.MultiplayerClient && lineOfSightIsClear)
                    {
                        for (int i = 0; i < chargeBlobCount; i++)
                        {
                            Vector2 blobVelocity = (shootDirection * 14.5f + Main.rand.NextVector2Circular(4f, 4f)) * Math.Abs(blobSpeedFactor);
                            int blob = Utilities.NewProjectileBetter(npc.Center + blobVelocity, blobVelocity, ModContent.ProjectileType<IchorBlob>(), 75, 0f);
                            if (Main.projectile.IndexInRange(blob))
                                Main.projectile[blob].ai[1] = target.Center.Y;
                        }
                    }

                    attackTimer = 0f;
                    attackSubstate = 2f;
                    npc.velocity = npc.SafeDirectionTo(target.Center) * chargeSpeed;
                    npc.netUpdate = true;
                }
            }

            // Post-charge behaviors.
            if (attackSubstate == 2f)
            {
                if (attackTimer >= chargeTime)
                    npc.velocity *= 0.92f;

                if (attackTimer >= chargeTime + chargeSlowdownTime)
                    SelectNextAttack(npc);
            }
        }

        public static void DoBehavior_SmallWormBursts(NPC npc, Player target, ref float attackTimer)
        {
            int wormSummonTime = 150;
            int reelBackTime = 25;
            int chargeRedirectTime = 30;
            int chargeTime = 60;
            float chargeHoverSpeed = 19.5f;
            float chargeSpeed = 26f;
            float maxHoverSpeed = 11f;
            bool doneReelingBack = attackTimer >= wormSummonTime + reelBackTime;

            ref float chargeTimer = ref npc.Infernum().ExtraAI[0];
            ref float chargeDirection = ref npc.Infernum().ExtraAI[1];

            // Disable significant contact damage.
            npc.Calamity().DR = 0.999999f;

            // Hover above the player and slow down.
            if (attackTimer < wormSummonTime)
            {
                npc.damage = 0;

                float hoverSpeed = MathHelper.Lerp(maxHoverSpeed, 0f, Utils.InverseLerp(wormSummonTime - 45f, 0f, attackTimer, true));
                if (doneReelingBack)
                    hoverSpeed = maxHoverSpeed;

                Vector2 hoverDestination = target.Center - Vector2.UnitY * 325f;
                Vector2 idealVelocity = npc.SafeDirectionTo(hoverDestination) * hoverSpeed;

                if (!npc.WithinRange(hoverDestination, 50f))
                {
                    npc.SimpleFlyMovement(idealVelocity, 0.5f);
                    npc.velocity = Vector2.Lerp(npc.velocity, idealVelocity, 0.025f);
                }
            }

            // Do horizontal charges once done reeling back.
            if (doneReelingBack)
            {
                // Initialize the charge direction.
                if (chargeTimer == 1f)
                {
                    chargeDirection = (target.Center.X > npc.Center.X).ToDirectionInt();
                    npc.netUpdate = true;
                }

                // Hover into position before charging.
                if (chargeTimer <= chargeRedirectTime)
                {
                    Vector2 hoverDestination = target.Center + Vector2.UnitX * chargeDirection * -420f;
                    npc.SimpleFlyMovement(npc.SafeDirectionTo(hoverDestination) * chargeHoverSpeed, chargeHoverSpeed * 0.16f);
                    npc.Center = npc.Center.MoveTowards(hoverDestination, 12.5f);
                    if (npc.WithinRange(hoverDestination, 25f))
                        npc.velocity = Vector2.Zero;

                    if (chargeTimer == chargeRedirectTime)
                        npc.velocity *= 0.3f;
                    npc.rotation = 0f;
                }
                else if (chargeTimer <= chargeRedirectTime + chargeTime)
                {
                    npc.velocity = Vector2.Lerp(npc.velocity, Vector2.UnitX * chargeDirection * chargeSpeed, 0.1f);
                    if (chargeTimer == chargeRedirectTime + chargeTime)
                        npc.velocity *= 0.7f;
                }
                else
                    npc.velocity *= 0.92f;

                if (chargeTimer >= chargeRedirectTime + chargeTime + 8f)
                {
                    chargeTimer = 0f;
                    npc.netUpdate = true;
                }

                chargeTimer++;
            }

            // Have the worm erupt from the hive.
            if (attackTimer == wormSummonTime)
                MakeWormEruptFromHive(npc, -Vector2.UnitY, 1f, ModContent.NPCType<PerforatorHeadSmall>());

            // Go to the next attack if the small perforator is dead.
            if (Main.netMode != NetmodeID.MultiplayerClient && attackTimer >= wormSummonTime + 1f && !NPC.AnyNPCs(ModContent.NPCType<PerforatorHeadSmall>()))
                SelectNextAttack(npc);
        }

        public static void DoBehavior_CrimeraWalls(NPC npc, Player target, bool inPhase3, bool inPhase4, ref float attackTimer)
        {
            int riseTime = 45;
            int wallCreationTime = 22;
            int attackSwitchDelay = 120;
            float offsetPerCrimera = 160f;
            float wallSpeed = 8f;
            float aimAtTargetInterpolant = 0f;

            if (inPhase3)
                offsetPerCrimera -= 10f;

            if (inPhase4)
            {
                offsetPerCrimera -= 15f;
                wallSpeed -= 0.95f;
                aimAtTargetInterpolant += 0.125f;
            }

            ref float horizontalWallOffset = ref npc.Infernum().ExtraAI[0];

            // Perform the initial rise.
            if (attackTimer == 1f)
            {
                npc.velocity = Vector2.UnitY * -12f;
                npc.netUpdate = true;
            }

            // Slow down after rising.
            if (attackTimer < riseTime)
                npc.velocity *= 0.95f;

            // Prepare wall attack stuff.
            if (attackTimer == riseTime)
            {
                Main.PlaySound(SoundID.NPCDeath23, npc.Center);

                npc.velocity = Vector2.Zero;
                horizontalWallOffset = Main.rand.NextFloat(-35f, 35f);
                npc.netUpdate = true;
            }

            // Release the walls.
            if (Main.netMode != NetmodeID.MultiplayerClient && attackTimer > riseTime && attackTimer <= riseTime + wallCreationTime)
            {
                horizontalWallOffset += offsetPerCrimera;
                Vector2 wallSpawnOffset = new Vector2(horizontalWallOffset - 1800f, -925f);
                Vector2 wallVelocity = Vector2.Lerp(Vector2.UnitY, -wallSpawnOffset.SafeNormalize(Vector2.UnitY), aimAtTargetInterpolant);
                wallVelocity = wallVelocity.SafeNormalize(Vector2.UnitY) * wallSpeed;

                Utilities.NewProjectileBetter(target.Center + wallSpawnOffset, wallVelocity, ModContent.ProjectileType<Crimera>(), 75, 1f);

                wallSpawnOffset.X += 48f;
                Utilities.NewProjectileBetter(target.Center + wallSpawnOffset * new Vector2(1f, -1f), -wallVelocity, ModContent.ProjectileType<Crimera>(), 75, 1f);
            }

            if (attackTimer > riseTime + attackSwitchDelay)
                SelectNextAttack(npc);
        }

        public static void DoBehavior_MediumWormBursts(NPC npc, Player target, ref float attackTimer, ref float backafterimageGlowInterpolant)
        {
            int wormSummonTime = 150;
            int reelBackTime = 25;
            int burstIchorCount = 3;
            int ichorBurstReleaseRate = 100;
            float maxHoverSpeed = 11f;
            bool doneReelingBack = attackTimer >= wormSummonTime + reelBackTime;
            ref float postWormSummonAttackTimer = ref npc.Infernum().ExtraAI[0];
            ref float hoverOffsetDirection = ref npc.Infernum().ExtraAI[1];

            // Disable contact damage.
            npc.dontTakeDamage = true;

            // Hover above the player and slow down.
            if (attackTimer < wormSummonTime)
            {
                npc.damage = 0;

                float hoverSpeed = maxHoverSpeed;
                Vector2 hoverDestination = target.Center - Vector2.UnitY * 325f;
                Vector2 idealVelocity = npc.SafeDirectionTo(hoverDestination) * hoverSpeed;

                if (!npc.WithinRange(hoverDestination, 50f))
                {
                    npc.SimpleFlyMovement(idealVelocity, 0.5f);
                    npc.velocity = Vector2.Lerp(npc.velocity, idealVelocity, 0.025f);
                }
            }

            // Periodically release bursts of ichor at the target and hover to their side.
            if (doneReelingBack)
            {
                postWormSummonAttackTimer++;

                // Initialize the hover offset direction if necessary.
                if (hoverOffsetDirection == 0f)
                {
                    hoverOffsetDirection = (target.Center.X < npc.Center.X).ToDirectionInt();
                    npc.netUpdate = true;
                }

                if (postWormSummonAttackTimer >= 300f)
                    backafterimageGlowInterpolant = Utils.InverseLerp(300f, 350f, postWormSummonAttackTimer, true);

                // Switch directions after enough time has passed.
                if (postWormSummonAttackTimer >= 380f)
                {
                    hoverOffsetDirection *= -1f;
                    postWormSummonAttackTimer = 0f;
                    npc.netUpdate = true;
                }

                Vector2 hoverDestination = target.Center + new Vector2(hoverOffsetDirection * 560f, -220f) - npc.velocity;
                Vector2 idealVelocity = npc.SafeDirectionTo(hoverDestination) * 12f;
                npc.SimpleFlyMovement(idealVelocity, 0.5f);

                if (attackTimer % ichorBurstReleaseRate == ichorBurstReleaseRate - 1f && npc.WithinRange(hoverDestination, 150f))
                {
                    Main.PlaySound(SoundID.NPCHit20, npc.Center);
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        for (int i = 0; i < burstIchorCount; i++)
                        {
                            float projectileOffsetInterpolant = i / (float)(burstIchorCount - 1f);
                            float offsetAngle = MathHelper.Lerp(-0.49f, 0.49f, projectileOffsetInterpolant);
                            Vector2 ichorVelocity = npc.SafeDirectionTo(target.Center).RotatedBy(offsetAngle) * 5.4f;
                            Utilities.NewProjectileBetter(npc.Center + ichorVelocity * 3f, ichorVelocity, ModContent.ProjectileType<FlyingIchor>(), 80, 0f);
                        }
                        npc.netUpdate = true;
                    }
                }
            }

            // Have the worm erupt from the hive.
            if (attackTimer == wormSummonTime)
                MakeWormEruptFromHive(npc, -Vector2.UnitY, 1f, ModContent.NPCType<PerforatorHeadMedium>());

            // Go to the next attack if the small perforator is dead.
            if (Main.netMode != NetmodeID.MultiplayerClient && attackTimer >= wormSummonTime + 1f && !NPC.AnyNPCs(ModContent.NPCType<PerforatorHeadMedium>()))
                SelectNextAttack(npc);
        }

        public static void DoBehavior_IchorRain(NPC npc, Player target, bool inPhase4, ref float attackTimer)
        {
            int chargeDelay = 20;
            int chargeTime = 60;
            int ichorReleaseRate = 5;
            float hoverOffset = 600f;
            float chargeSpeed = hoverOffset / chargeTime * 2.75f;

            if (inPhase4)
            {
                ichorReleaseRate--;
                chargeSpeed *= 1.225f;
            }

            ref float attackSubstate = ref npc.Infernum().ExtraAI[0];

            // Hover into position.
            if (attackSubstate == 0f)
            {
                Vector2 hoverDestination = target.Center + new Vector2((target.Center.X < npc.Center.X).ToDirectionInt() * hoverOffset, -270f);
                Vector2 idealVelocity = npc.SafeDirectionTo(hoverDestination) * 20f;

                npc.SimpleFlyMovement(idealVelocity, idealVelocity.Length() / 20f);
                npc.velocity = Vector2.Lerp(npc.velocity, idealVelocity, 0.05f);

                // Slow down and go to the next attack substate if sufficiently close to the hover destination.
                if (npc.WithinRange(hoverDestination, 75f))
                {
                    attackTimer = 0f;
                    attackSubstate = 1f;
                    npc.velocity *= 0.5f;
                    npc.netUpdate = true;
                }

                // Give up and perform a different attack if unable to reach the hover destination in time.
                if (attackTimer >= 480f)
                    SelectNextAttack(npc);
            }

            // Slow down in anticipation of a charge.
            if (attackSubstate == 1f)
            {
                npc.velocity *= 0.925f;

                // Release ichor into the air that slowly falls and charge at the target.
                if (attackTimer >= chargeDelay)
                {
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        attackTimer = 0f;
                        attackSubstate = 2f;
                        npc.velocity = Vector2.UnitX * (target.Center.X > npc.Center.X).ToDirectionInt() * chargeSpeed;
                        npc.netUpdate = true;
                    }
                }
            }

            // Charge.
            if (attackSubstate == 2f)
            {
                // Release ichor upward.
                if (attackTimer % ichorReleaseRate == ichorReleaseRate / 2 && attackTimer < chargeTime * 0.67)
                {
                    Main.PlaySound(SoundID.NPCDeath23, npc.Center);
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        Vector2 ichorVelocity = -Vector2.UnitY.RotatedByRandom(0.2f) * 7f;
                        ichorVelocity.X += npc.velocity.X * 0.02f;
                        int blob = Utilities.NewProjectileBetter(npc.Center, ichorVelocity, ModContent.ProjectileType<IchorBlob>(), 80, 0f);
                        if (Main.projectile.IndexInRange(blob))
                            Main.projectile[blob].ai[1] = target.Center.Y;
                    }
                }

                if (attackTimer >= chargeTime)
                {
                    SelectNextAttack(npc);
                    npc.velocity *= 0.45f;
                    npc.netUpdate = true;
                }
            }
        }

        public static void DoBehavior_LargeWormBursts(NPC npc, Player target, ref float attackTimer)
        {
            int wormSummonTime = 150;
            int reelBackTime = 25;
            int chargeRedirectTime = 60;
            int chargeTime = 32;
            int ichorBlobCount = 3;
            float chargeHoverSpeed = 19.5f;
            float chargeSpeed = 29f;
            float maxHoverSpeed = 11f;
            bool doneReelingBack = attackTimer >= wormSummonTime + reelBackTime;

            ref float chargeTimer = ref npc.Infernum().ExtraAI[0];
            ref float chargeDirection = ref npc.Infernum().ExtraAI[1];

            // Disable significant contact damage.
            npc.Calamity().DR = 0.999999f;

            // Hover above the player and slow down.
            if (attackTimer < wormSummonTime)
            {
                npc.damage = 0;

                float hoverSpeed = maxHoverSpeed;
                Vector2 hoverDestination = target.Center - Vector2.UnitY * 325f;
                Vector2 idealVelocity = npc.SafeDirectionTo(hoverDestination) * hoverSpeed;

                if (!npc.WithinRange(hoverDestination, 50f))
                {
                    npc.SimpleFlyMovement(idealVelocity, 0.5f);
                    npc.velocity = Vector2.Lerp(npc.velocity, idealVelocity, 0.025f);
                }
            }

            // Do vertical charges once done reeling back.
            if (doneReelingBack)
            {
                // Hover into position before charging.
                if (chargeTimer <= chargeRedirectTime)
                {
                    Vector2 hoverDestination = target.Center - Vector2.UnitY * 335f;
                    Vector2 idealVelocity = npc.SafeDirectionTo(hoverDestination) * chargeHoverSpeed;

                    // Avoid colliding directly with the target by turning the ideal velocity 90 degrees.
                    // The side at which angular directions happen is dependant on whichever angle has the greatest disparity between the direction to the target.
                    // This means that the direction that gets the hive farther from the player is the one that is favored.
                    if (idealVelocity.AngleBetween(npc.SafeDirectionTo(target.Center)) < MathHelper.PiOver4)
                    {
                        float leftAngleDisparity = idealVelocity.RotatedBy(-MathHelper.PiOver2).AngleBetween(npc.SafeDirectionTo(target.Center));
                        float rightAngleDisparity = idealVelocity.RotatedBy(MathHelper.PiOver2).AngleBetween(npc.SafeDirectionTo(target.Center));
                        idealVelocity = idealVelocity.RotatedBy(leftAngleDisparity > rightAngleDisparity ? -MathHelper.PiOver2 : MathHelper.PiOver2);
                    }

                    npc.SimpleFlyMovement(idealVelocity, chargeHoverSpeed * 0.05f);
                    npc.Center = npc.Center.MoveTowards(hoverDestination, 10f);
                    if (npc.WithinRange(hoverDestination, 25f))
                        npc.velocity = Vector2.Zero;

                    if (chargeTimer == chargeRedirectTime)
                    {
                        // Try again instead of slamming downward if not within the range of the hover destination.
                        if (!npc.WithinRange(hoverDestination, 130f))
                            chargeTimer = 0f;

                        // Otherwise slow down in anticipation of the target and release ichor blobs upward.
                        else
                        {
                            Main.PlaySound(SoundID.NPCDeath23, npc.Center);
                            if (Main.netMode != NetmodeID.MultiplayerClient)
                            {
                                for (int i = 0; i < ichorBlobCount; i++)
                                {
                                    float projectileShootInterpolant = i / (float)(ichorBlobCount - 1f);
                                    float horizontalShootSpeed = MathHelper.Lerp(-10f, 10f, projectileShootInterpolant) + Main.rand.NextFloatDirection() * 0.64f;
                                    Vector2 blobVelocity = new Vector2(horizontalShootSpeed, -7f);
                                    int blob = Utilities.NewProjectileBetter(npc.Center + blobVelocity, blobVelocity, ModContent.ProjectileType<IchorBlob>(), 80, 0f);
                                    if (Main.projectile.IndexInRange(blob))
                                        Main.projectile[blob].ai[1] = target.Center.Y;
                                }
                            }

                            npc.velocity *= 0.25f;
                        }
                        npc.netUpdate = true;
                    }
                    npc.rotation = 0f;
                }
                else if (chargeTimer <= chargeRedirectTime + chargeTime)
                {
                    npc.velocity = Vector2.Lerp(npc.velocity, Vector2.UnitY * chargeSpeed, 0.1f);
                    if (chargeTimer == chargeRedirectTime + chargeTime)
                        npc.velocity *= 0.7f;
                }
                else
                    npc.velocity *= 0.92f;

                if (chargeTimer >= chargeRedirectTime + chargeTime + 8f)
                {
                    chargeTimer = 0f;
                    npc.netUpdate = true;
                }

                chargeTimer++;
            }

            // Have the worm erupt from the hive.
            if (attackTimer == wormSummonTime)
                MakeWormEruptFromHive(npc, -Vector2.UnitY, 1.75f, ModContent.NPCType<PerforatorHeadLarge>());

            // Go to the next attack if the small perforator is dead.
            if (Main.netMode != NetmodeID.MultiplayerClient && attackTimer >= wormSummonTime + 1f && !NPC.AnyNPCs(ModContent.NPCType<PerforatorHeadLarge>()))
            {
                CleanUpStrayProjectiles();
                SelectNextAttack(npc);
            }
        }

        public static void DoBehavior_IchorFountainCharge(NPC npc, Player target, ref float attackTimer)
        {
            int moveDelay = 60;
            int attackDelay = 150;
            int mouthIchorReleaseRate = 5;
            int ichorWallFireRate = 50;
            int ichorWallShotCount = 5;
            int attackTime = 420;
            int attackTransitionDelay = 120;
            float ichorWallSpacing = 40f;
            Vector2 hoverDestination = target.Center - Vector2.UnitY * 300f;
            Vector2 leftMouthPosition = npc.Center + new Vector2(-68f, 6f).RotatedBy(-npc.rotation);
            Vector2 rightMouthPosition = npc.Center + new Vector2(48f, -36f).RotatedBy(npc.rotation);
            Vector2[] mouthPositions = new[]
            {
                leftMouthPosition,
                rightMouthPosition
            };

            if (attackTimer <= moveDelay)
            {
                // Slow down dramatically at first.
                npc.velocity *= 0.935f;

                // Rise upward and create an explosion sound.
                if (attackTimer == moveDelay)
                {
                    Main.PlaySound(SoundID.Roar, (int)npc.Center.X, (int)npc.Center.Y, 0);
                    var explosionSound = Main.PlaySound(SoundID.DD2_BetsyFireballImpact, npc.Center);
                    if (explosionSound != null)
                    {
                        CalamityUtils.SafeVolumeChange(ref explosionSound, 2.2f);
                        explosionSound.Pitch = -0.4f;
                    }

                    if (Main.netMode != NetmodeID.MultiplayerClient)
                        Projectile.NewProjectile(npc.Center, Vector2.Zero, ModContent.ProjectileType<PerforatorWave>(), 0, 0f);

                    npc.velocity = -Vector2.UnitY * 12f;
                    npc.netUpdate = true;
                }
            }

            // Attempt to hover above the target.
            else if (attackTimer < attackDelay)
            {
                if (!npc.WithinRange(hoverDestination, 25f))
                    npc.SimpleFlyMovement(npc.SafeDirectionTo(hoverDestination) * 14.5f, 0.5f);
                npc.Center = npc.Center.MoveTowards(hoverDestination, 8f);
            }

            // Continue hovering over the target, but move at slower horizontal pace.
            // Also spew ichor from the mouths as an effective barrier while creating lines of ichor from the sides that accelerate at the target.
            else
            {
                if (!npc.WithinRange(hoverDestination, 30f))
                {
                    Vector2 idealVelocity = npc.SafeDirectionTo(hoverDestination) * new Vector2(9.5f, 17f);
                    npc.SimpleFlyMovement(idealVelocity, 0.6f);
                    if (npc.Center.Y > hoverDestination.Y + 80f)
                        npc.velocity.Y -= 0.8f;
                }

                // Create walls of ichor
                if (Main.netMode != NetmodeID.MultiplayerClient && attackTimer % ichorWallFireRate == ichorWallFireRate - 1f && attackTimer < attackDelay + attackTime)
                {
                    float ichorOffsetDirection = Main.rand.NextBool().ToDirectionInt();
                    for (int i = 0; i < ichorWallShotCount; i++)
                    {
                        float ichorShootInterpolant = i / (float)(ichorWallShotCount - 1f);
                        float verticalWallOffset = MathHelper.Lerp(-0.5f, 0.5f, ichorShootInterpolant) * ichorWallSpacing * ichorWallShotCount;
                        Vector2 wallOffset = new Vector2(ichorOffsetDirection * 560f, verticalWallOffset);
                        Vector2 wallVelocity = Vector2.UnitX * ichorOffsetDirection * -6.5f;
                        Utilities.NewProjectileBetter(target.Center + wallOffset, wallVelocity, ModContent.ProjectileType<IchorBolt>(), 80, 0f);
                    }
                }

                // Release ichor from the mouth.
                if (attackTimer % mouthIchorReleaseRate == mouthIchorReleaseRate - 1f)
                {
                    Main.PlaySound(SoundID.NPCHit20, npc.Center);
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        foreach (Vector2 mouthPosition in mouthPositions)
                        {
                            Vector2 ichorVelocity = npc.SafeDirectionTo(mouthPosition) * 12f;
                            Utilities.NewProjectileBetter(mouthPosition, ichorVelocity, ModContent.ProjectileType<FallingIchorBlast>(), 85, 0f);
                        }
                    }
                }
            }

            if (attackTimer >= attackDelay + attackTime + attackTransitionDelay)
                SelectNextAttack(npc);
        }

        public static void SelectNextAttack(NPC npc)
        {
            float lifeRatio = npc.life / (float)npc.lifeMax;
            bool inPhase2 = lifeRatio <= Phase2LifeRatio;
            bool inPhase3 = lifeRatio <= Phase3LifeRatio;
            bool inPhase4 = lifeRatio <= Phase4LifeRatio;
            int crimeraAttackType = inPhase2 ? (int)PerforatorHiveAttackState.CrimeraWalls : (int)PerforatorHiveAttackState.HorizontalCrimeraSpawnCharge;
            int ichorBlastAttackType = inPhase3 ? (int)PerforatorHiveAttackState.IchorRain : (int)(int)PerforatorHiveAttackState.IchorBlasts;
            int ichorFromAboveAttackType = inPhase4 ? (int)PerforatorHiveAttackState.IchorFountainCharge : (int)(int)PerforatorHiveAttackState.DiagonalBloodCharge;

            switch ((PerforatorHiveAttackState)npc.ai[0])
            {
                case PerforatorHiveAttackState.HorizontalCrimeraSpawnCharge:
                case PerforatorHiveAttackState.CrimeraWalls:
                    npc.ai[0] = ichorBlastAttackType;
                    break;
                case PerforatorHiveAttackState.IchorBlasts:
                case PerforatorHiveAttackState.IchorRain:
                    npc.ai[0] = (int)PerforatorHiveAttackState.IchorSpinDash;
                    break;
                case PerforatorHiveAttackState.IchorSpinDash:
                    npc.ai[0] = ichorFromAboveAttackType;
                    break;
                default:
                    npc.ai[0] = crimeraAttackType;
                    break;
            }

            npc.ai[1] = 0f;
            for (int i = 0; i < 5; i++)
                npc.Infernum().ExtraAI[i] = 0f;

            npc.netUpdate = true;
        }

        public static void CreateSegments(NPC npc, int wormLength, int bodyType, int tailType)
        {
            int previousIndex = npc.whoAmI;
            for (int i = 0; i < wormLength; i++)
            {
                int nextIndex;
                if (i < wormLength - 1)
                    nextIndex = NPC.NewNPC((int)npc.Center.X, (int)npc.Center.Y, bodyType, npc.whoAmI + 1);
                else
                    nextIndex = NPC.NewNPC((int)npc.Center.X, (int)npc.Center.Y, tailType, npc.whoAmI + 1);

                Main.npc[nextIndex].realLife = npc.whoAmI;
                Main.npc[nextIndex].ai[2] = npc.whoAmI;
                Main.npc[nextIndex].ai[1] = previousIndex;
                Main.npc[previousIndex].ai[0] = nextIndex;

                // Force sync the new segment into existence.
                NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, nextIndex, 0f, 0f, 0f, 0);

                previousIndex = nextIndex;
            }
        }

        #endregion AI

        #region Drawcode

        public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Color lightColor)
        {
            SpriteEffects direction = SpriteEffects.None;
            if (npc.spriteDirection == 1)
                direction = SpriteEffects.FlipHorizontally;

            Texture2D texture = Main.npcTexture[npc.type];
            Vector2 origin = npc.frame.Size() * 0.5f;
            Vector2 baseDrawPosition = npc.Center - Main.screenPosition;
            float backafterimageGlowInterpolant = npc.localAI[0];

            if (backafterimageGlowInterpolant > 0f)
            {
                Color backAfterimageColor = Color.Yellow * backafterimageGlowInterpolant;
                backAfterimageColor.A = 0;
                for (int i = 0; i < 6; i++)
                {
                    Vector2 drawPosition = baseDrawPosition + (MathHelper.TwoPi * i / 6f).ToRotationVector2() * backafterimageGlowInterpolant * 4f;
                    spriteBatch.Draw(texture, drawPosition, npc.frame, npc.GetAlpha(backAfterimageColor), npc.rotation, origin, npc.scale, direction, 0f);
                }
            }

            spriteBatch.Draw(texture, baseDrawPosition, npc.frame, npc.GetAlpha(lightColor), npc.rotation, origin, npc.scale, direction, 0f);

            texture = ModContent.GetTexture("CalamityMod/NPCs/Perforator/PerforatorHiveGlow");
            Color glowmaskColor = Color.Lerp(Color.White, Color.Yellow, 0.5f);

            spriteBatch.Draw(texture, baseDrawPosition, npc.frame, glowmaskColor, npc.rotation, origin, npc.scale, direction, 0f);
            return false;
        }
        #endregion
    }
}
