using CalamityMod;
using InfernumMode.OverridingSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.MoonLord
{
    public class TrueEyeOfCthulhuBehaviorOverride : NPCBehaviorOverride
    {
        public override int NPCOverrideType => NPCID.MoonLordFreeEye;

        public override NPCOverrideContext ContentToOverride => NPCOverrideContext.NPCAI | NPCOverrideContext.NPCPreDraw;

        public override bool PreAI(NPC npc)
        {
            // Disappear if the body is not present.
            if (!Main.npc.IndexInRange((int)npc.ai[3]) || !Main.npc[(int)npc.ai[3]].active)
            {
                npc.active = false;
                return false;
            }

            // Define the core NPC.
            NPC core = Main.npc[(int)npc.ai[3]];

            npc.target = core.target;
            npc.damage = 0;
            npc.defDamage = 220;

            Player target = Main.player[npc.target];
            float attackTimer = core.ai[1];
            ref float groupIndex = ref npc.ai[0];
            ref float pupilRotation = ref npc.localAI[0];
            ref float pupilOutwardness = ref npc.localAI[1];
            ref float pupilScale = ref npc.localAI[2];
            ref float enrageTimer = ref npc.Infernum().ExtraAI[5];

            // Define an initial group index.
            if (groupIndex == 0f)
            {
                groupIndex = NPC.CountNPCS(npc.type);
                npc.netUpdate = true;
            }

            // Kill the player if they leave the arena.
            if (MoonLordCoreBehaviorOverride.IsEnraged)
            {
                int enrageBoltCount = 7;
                float enrageBoltSpread = 0.71f;
                float enrageBoltShootSpeed = 12f;

                // Get really, really, really angry if the target leaves the arena.
                pupilRotation = npc.AngleTo(target.Center);
                if (enrageTimer < 60f)
                {
                    pupilOutwardness = MathHelper.Lerp(pupilOutwardness, 0.4f, 0.1f);

                    // Have the pupil dilate to a really large size in shock.
                    pupilScale = MathHelper.Lerp(0.4f, 1.1f, enrageTimer / 60f);
                }
                else if (Main.netMode != NetmodeID.MultiplayerClient && enrageTimer % 8f == 7f)
                {
                    Vector2 boltSpawnPosition = npc.Center + CalculatePupilOffset(npc);
                    for (int i = 0; i < enrageBoltCount; i++)
                    {
                        Vector2 boltShootVelocity = (target.Center - boltSpawnPosition).SafeNormalize(Vector2.UnitY) * enrageBoltShootSpeed;
                        boltShootVelocity = boltShootVelocity.RotatedBy(MathHelper.Lerp(-enrageBoltSpread, enrageBoltSpread, i / (float)(enrageBoltCount - 1f)));
                        Utilities.NewProjectileBetter(boltSpawnPosition, boltShootVelocity, ProjectileID.PhantasmalBolt, 360, 0f);
                    }
                }

                int eyeCount = NPC.CountNPCS(NPCID.MoonLordFreeEye);
                Vector2 hoverOffset = -Vector2.UnitY * 415f;

                if (eyeCount > 1)
                {
                    float hoverOffsetAngle = MathHelper.Lerp(-0.75f, 0.75f, (groupIndex - 1f) / (float)(eyeCount - 1f));
                    hoverOffset = hoverOffset.RotatedBy(hoverOffsetAngle);
                }

                npc.rotation = npc.velocity.X * 0.03f;
                npc.spriteDirection = (target.Center.X < npc.Center.X).ToDirectionInt();
                if (npc.spriteDirection == 1)
                    npc.rotation += MathHelper.Pi;

                npc.Center = npc.Center.MoveTowards(target.Center + hoverOffset, 2f);
                npc.SimpleFlyMovement(npc.SafeDirectionTo(target.Center + hoverOffset) * 19f, 0.75f);

                enrageTimer++;
                return false;
            }
            enrageTimer = 0f;

            switch ((MoonLordCoreBehaviorOverride.MoonLordAttackState)(int)core.ai[0])
            {
                case MoonLordCoreBehaviorOverride.MoonLordAttackState.DeathEffects:
                    npc.life = 0;
                    npc.StrikeNPCNoInteraction(9999, 0f, 0);
                    npc.checkDead();
                    break;
                case MoonLordCoreBehaviorOverride.MoonLordAttackState.PhantasmalSpin:
                    DoBehavior_PhantasmalSpin(npc, target, core, attackTimer, groupIndex, ref pupilRotation, ref pupilOutwardness, ref pupilScale);
                    break;
                case MoonLordCoreBehaviorOverride.MoonLordAttackState.PhantasmalRush:
                    DoBehavior_PhantasmalRush(npc, target, core, attackTimer, groupIndex, ref pupilRotation, ref pupilOutwardness, ref pupilScale);
                    break;
                case MoonLordCoreBehaviorOverride.MoonLordAttackState.PhantasmalDance:
                    DoBehavior_PhantasmalDance(npc, target, core, attackTimer, groupIndex, ref pupilRotation, ref pupilOutwardness, ref pupilScale);
                    break;
                case MoonLordCoreBehaviorOverride.MoonLordAttackState.PhantasmalBarrage:
                    DoBehavior_PhantasmalBarrage(npc, target, core, attackTimer, groupIndex, ref pupilRotation, ref pupilOutwardness, ref pupilScale);
                    break;
                case MoonLordCoreBehaviorOverride.MoonLordAttackState.PhantasmalWrath:
                    DoBehavior_PhantasmalWrath(npc, target, core, attackTimer, groupIndex, ref pupilRotation, ref pupilOutwardness, ref pupilScale);
                    break;
                default:
                    DoBehavior_IdleObserve(npc, target, groupIndex, ref pupilRotation, ref pupilOutwardness, ref pupilScale);
                    break;
            }

            return false;
        }

        public static void DoBehavior_IdleObserve(NPC npc, Player target, float groupIndex, ref float pupilRotation, ref float pupilOutwardness, ref float pupilScale)
        {
            int eyeCount = NPC.CountNPCS(NPCID.MoonLordFreeEye);
            Vector2 hoverOffset = -Vector2.UnitY * 475f;

            // Define pupil variables.
            pupilRotation = pupilRotation.AngleLerp(npc.AngleTo(target.Center), 0.15f);
            pupilOutwardness = MathHelper.Lerp(0.2f, 0.8f, Utils.InverseLerp(150f, 400f, npc.Distance(target.Center), true));
            pupilScale = MathHelper.Lerp(pupilScale, 0.4f, 0.1f);

            if (eyeCount > 1)
            {
                float hoverOffsetAngle = MathHelper.Lerp(-0.75f, 0.75f, (groupIndex - 1f) / (float)(eyeCount - 1f));
                hoverOffset = hoverOffset.RotatedBy(hoverOffsetAngle);
            }

            npc.rotation = npc.velocity.X * 0.03f;
            npc.spriteDirection = (target.Center.X < npc.Center.X).ToDirectionInt();
            if (npc.spriteDirection == 1)
                npc.rotation += MathHelper.Pi;

            npc.Center = npc.Center.MoveTowards(target.Center + hoverOffset, 2f);
            npc.SimpleFlyMovement(npc.SafeDirectionTo(target.Center + hoverOffset) * 19f, 0.75f);
        }

        public static void DoBehavior_PhantasmalSpin(NPC npc, Player target, NPC core, float attackTimer, float groupIndex, ref float pupilRotation, ref float pupilOutwardness, ref float pupilScale)
        {
            int fireDelay = 135;
            int shootTime = 450;
            int attackTransitionDelay = 90;
            int boltSpreadCount = 4;
            int asteroidReleaseRate = 37;
            Vector2 hoverDestination = core.Infernum().Arena.Center.ToVector2();

            // Look at the target.
            pupilRotation = pupilRotation.AngleLerp(npc.AngleTo(target.Center), 0.15f);

            if (groupIndex >= 2f)
            {
                DoBehavior_IdleObserve(npc, target, groupIndex, ref pupilRotation, ref pupilOutwardness, ref pupilScale);
                return;
            }

            // Hover into position.
            if (attackTimer < fireDelay)
            {
                pupilOutwardness = MathHelper.Lerp(pupilOutwardness, 0.2f, 0.15f);
                pupilScale = MathHelper.Lerp(pupilScale, 0.4f, 0.15f);

                npc.rotation = npc.rotation.AngleLerp(MathHelper.Pi, 0.08f);
                npc.spriteDirection = 1;
                npc.velocity = Vector2.Lerp(npc.velocity, Vector2.Zero.MoveTowards(hoverDestination - npc.Center, 45f), 0.6f);
            }
            else if (attackTimer < fireDelay + shootTime)
            {
                if (attackTimer == fireDelay)
                {
                    npc.velocity = Vector2.Zero;
                    npc.netUpdate = true;

                    Main.PlaySound(SoundID.Zombie, target.Center, 100);
                }

                // Release the barrage of eyes.
                if (attackTimer % 5f == 4f)
                {
                    Main.PlaySound(SoundID.Item12, npc.Center);

                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        float eyeAngularVelocity = MathHelper.ToRadians(0.2f);
                        float temporalOffsetAngle = MathHelper.TwoPi * (attackTimer - fireDelay) / 225f;
                        Vector2 eyeSpawnPosition = npc.Center + Vector2.UnitY * 10f;
                        for (int i = 0; i < boltSpreadCount; i++)
                        {
                            Vector2 eyeVelocity = -Vector2.UnitY.RotatedBy(MathHelper.TwoPi * i / boltSpreadCount + temporalOffsetAngle) * 8f;
                            int eye = Utilities.NewProjectileBetter(eyeSpawnPosition, eyeVelocity, ModContent.ProjectileType<NonHomingPhantasmalEye>(), 215, 0f);
                            if (Main.projectile.IndexInRange(eye))
                                Main.projectile[eye].ai[1] = eyeAngularVelocity;
                        }
                    }
                }

                if (attackTimer % asteroidReleaseRate == asteroidReleaseRate - 1f)
                {
                    Vector2 asteroidSpawnPosition = target.Center + Main.rand.NextVector2CircularEdge(700f, 700f);
                    Vector2 asteroidShootVelocity = (core.Center - asteroidSpawnPosition).SafeNormalize(Vector2.UnitY) * 9f;
                    int asteroid = Utilities.NewProjectileBetter(asteroidSpawnPosition, asteroidShootVelocity, ModContent.ProjectileType<LunarAsteroid>(), 220, 0f);
                    if (Main.projectile.IndexInRange(asteroid))
                        Main.projectile[asteroid].ai[0] = core.whoAmI;
                }
            }

            // Terminate the attack early if the target leaves the arena, as this attack is arena-focused.
            if (MoonLordCoreBehaviorOverride.IsEnraged || attackTimer >= fireDelay + shootTime + attackTransitionDelay)
            {
                Utilities.DeleteAllProjectiles(true, ModContent.ProjectileType<NonHomingPhantasmalEye>(), ModContent.ProjectileType<LunarAsteroid>());
                core.Infernum().ExtraAI[5] = 1f;
            }
        }

        public static void DoBehavior_PhantasmalRush(NPC npc, Player target, NPC core, float attackTimer, float groupIndex, ref float pupilRotation, ref float pupilOutwardness, ref float pupilScale)
        {
            int fireDelay = 120;
            int chargeRate = 15;
            int laserLifetime = PressurePhantasmalDeathray.LifetimeConstant;
            int boltCount = 6;
            float pressureLaserStartingAngularOffset = 0.63f;
            float pressureLaserEndingAngularOffset = 0.14f;
            float chargeVerticalOffset = 330f;
            float boltShootSpeed = 2f;

            if (MoonLordCoreBehaviorOverride.InFinalPhase)
            {
                boltCount += 2;
                chargeVerticalOffset += 15f;
            }

            if (MoonLordCoreBehaviorOverride.IsEnraged)
            {
                boltCount = 13;
                boltShootSpeed = 10f;
            }

            int chargeCount = laserLifetime / chargeRate;
            float chargeSpeed = chargeVerticalOffset / chargeRate * 2f;
            ref float telegraphAngularOffset = ref npc.Infernum().ExtraAI[0];
            ref float lineTelegraphInterpolant = ref npc.Infernum().ExtraAI[1];

            lineTelegraphInterpolant = 0f;

            // The left eye shoots pressure lasers while the right eye does zigzag charges.
            if (groupIndex == 1f)
            {
                Vector2 hoverDestination = target.Center + Vector2.UnitX * (target.Center.X < npc.Center.X).ToDirectionInt() * 600f;
                if (attackTimer < fireDelay)
                {
                    float movementSpeedInterpolant = 1f - Utils.InverseLerp(fireDelay * 0.6f, fireDelay * 0.85f, attackTimer, true);
                    npc.rotation = npc.rotation.AngleLerp(npc.AngleTo(target.Center), 0.25f);
                    npc.Center = npc.Center.MoveTowards(hoverDestination, movementSpeedInterpolant * 3f);
                    npc.SimpleFlyMovement(npc.SafeDirectionTo(hoverDestination) * movementSpeedInterpolant * 21f, 0.85f);

                    pupilRotation = npc.rotation;
                    pupilOutwardness = MathHelper.Lerp(pupilOutwardness, 0.45f, 0.15f);
                    pupilScale = MathHelper.Lerp(pupilScale, 0.7f, 0.15f);
                    lineTelegraphInterpolant = attackTimer / fireDelay;
                    telegraphAngularOffset = MathHelper.Lerp(1.5f, 1f, lineTelegraphInterpolant) * pressureLaserStartingAngularOffset;
                }
                else
                {
                    npc.velocity = Vector2.Zero;
                    lineTelegraphInterpolant = 1f;
                    telegraphAngularOffset = -1000f;
                }

                // Release lasers.
                if (attackTimer == fireDelay)
                {
                    Main.PlaySound(SoundID.Zombie, target.Center, 104);
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        for (int i = -1; i <= 1; i += 2)
                        {
                            float angularVelocity = (pressureLaserEndingAngularOffset - pressureLaserStartingAngularOffset) / laserLifetime * i * 0.5f;
                            Vector2 laserDirection = npc.SafeDirectionTo(target.Center).RotatedBy(pressureLaserStartingAngularOffset * i);
                            int telegraph = Utilities.NewProjectileBetter(npc.Center, laserDirection, ModContent.ProjectileType<PressurePhantasmalDeathray>(), 300, 0f);
                            if (Main.projectile.IndexInRange(telegraph))
                            {
                                Main.projectile[telegraph].ai[1] = npc.whoAmI;
                                Main.projectile[telegraph].ModProjectile<PressurePhantasmalDeathray>().AngularVelocity = angularVelocity;
                            }
                        }
                    }
                }
            }
            else if (groupIndex == 2f)
            {
                Vector2 hoverDestination = target.Center + new Vector2((target.Center.X < npc.Center.X).ToDirectionInt() * 600f, chargeVerticalOffset);
                if (attackTimer < fireDelay)
                {
                    float movementSpeedInterpolant = 1f - Utils.InverseLerp(fireDelay * 0.6f, fireDelay * 0.85f, attackTimer, true);
                    npc.Center = npc.Center.MoveTowards(hoverDestination, movementSpeedInterpolant * 3f);
                    npc.rotation = npc.spriteDirection == 1 ? MathHelper.Pi : 0f;
                    npc.velocity = npc.SafeDirectionTo(hoverDestination) * MathHelper.Min(npc.Distance(hoverDestination), 25f);

                    pupilRotation = npc.rotation;
                    pupilOutwardness = MathHelper.Lerp(pupilOutwardness, 0.45f, 0.15f);
                    pupilScale = MathHelper.Lerp(pupilScale, 0.75f, 0.15f);
                }
                else
                {
                    pupilOutwardness = MathHelper.Lerp(pupilOutwardness, 0f, 0.25f);
                    npc.rotation = npc.velocity.ToRotation() + MathHelper.PiOver2;
                    if (npc.spriteDirection == 1)
                        npc.rotation += MathHelper.Pi;

                    // Prepare the charges.
                    if ((attackTimer - fireDelay) % chargeRate == 0f)
                    {
                        // Cast charge telegraph lines and prepare the initial charge.
                        if (attackTimer == fireDelay)
                        {
                            Main.PlaySound(SoundID.Zombie, npc.Center, 100);
                            npc.velocity = new Vector2(Math.Sign(target.Center.X - npc.Center.X), -3.4f).SafeNormalize(Vector2.UnitY) * chargeSpeed;

                            if (Main.netMode != NetmodeID.MultiplayerClient)
                            {
                                Vector2 currentVelocity = npc.velocity;
                                Vector2[] chargePositions = new Vector2[chargeCount + 1];
                                chargePositions[0] = npc.Center;
                                for (int i = 0; i < chargeCount; i++)
                                {
                                    chargePositions[i + 1] = chargePositions[i] + currentVelocity * chargeRate;
                                    currentVelocity = Vector2.Reflect(currentVelocity, Vector2.UnitY) * new Vector2(1f, 0.85f);
                                }
                                int telegraph = Projectile.NewProjectile(npc.Center, Vector2.Zero, ModContent.ProjectileType<TrueEyeChargeTelegraph>(), 0, 0f);
                                if (Main.projectile.IndexInRange(telegraph))
                                {
                                    Main.projectile[telegraph].ModProjectile<TrueEyeChargeTelegraph>().ChargePositions = chargePositions;
                                    Main.projectile[telegraph].netUpdate = true;
                                }
                            }
                            npc.netUpdate = true;
                        }
                        else
                            npc.velocity = Vector2.Reflect(npc.velocity, Vector2.UnitY) * new Vector2(1f, 0.85f);

                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            float circularSpreadOffsetAngle = Main.rand.NextBool() ? MathHelper.Pi / boltCount : 0f;
                            circularSpreadOffsetAngle += npc.AngleTo(target.Center);
                            for (int i = 0; i < boltCount; i++)
                            {
                                Vector2 boltShootVelocity = (MathHelper.TwoPi * i / boltCount + circularSpreadOffsetAngle).ToRotationVector2() * boltShootSpeed;
                                Utilities.NewProjectileBetter(npc.Center, boltShootVelocity, ProjectileID.PhantasmalBolt, 200, 0f);
                            }
                        }

                        npc.netUpdate = true;
                    }
                }
            }
            else
                DoBehavior_IdleObserve(npc, target, groupIndex, ref pupilRotation, ref pupilOutwardness, ref pupilScale);

            npc.spriteDirection = (target.Center.X < npc.Center.X).ToDirectionInt();
            if (attackTimer >= fireDelay + laserLifetime)
                core.Infernum().ExtraAI[5] = 1f;
        }

        public static void DoBehavior_PhantasmalDance(NPC npc, Player target, NPC core, float attackTimer, float groupIndex, ref float pupilRotation, ref float pupilOutwardness, ref float pupilScale)
        {
            int spinTime = 60;
            int chargeTelegraphTime = 39;
            int chargeTime = 36;
            int chargeCount = 4;
            int orbReleaseRate = 5;
            float spinOffset = 500f;
            float chargeSpeed = 37f;
            float chargePredictiveness = 20f;

            if (MoonLordCoreBehaviorOverride.InFinalPhase)
            {
                orbReleaseRate--;
                spinOffset -= 20f;
                chargeSpeed += 3f;
            }

            if (MoonLordCoreBehaviorOverride.IsEnraged)
            {
                chargeTelegraphTime = 30;
                spinOffset = 375f;
                chargeSpeed = 50f;
            }

            int chargeCounter = (int)(attackTimer / (spinTime + chargeTelegraphTime + chargeTime));
            float spinDirection = (chargeCounter % 2f == 0f).ToDirectionInt();
            float wrappedAttackTimer = attackTimer % (spinTime + chargeTelegraphTime + chargeTime);
            ref float telegraphInterpolant = ref npc.Infernum().ExtraAI[0];
            ref float telegraphDirection = ref npc.Infernum().ExtraAI[1];

            // Snap into place for the spin.
            if (wrappedAttackTimer < spinTime)
            {
                float angularOffest = MathHelper.TwoPi * (groupIndex - 1f) / NPC.CountNPCS(npc.type);
                float spinArc = MathHelper.Pi * spinDirection;
                float hoverSlowdown = Utils.InverseLerp(1f, 0.8f, wrappedAttackTimer / spinTime, true);
                Vector2 idealPosition = target.Center + (spinArc * wrappedAttackTimer / spinTime + angularOffest).ToRotationVector2() * spinOffset;
                Vector2 aheadPosition = target.Center + (spinArc * (wrappedAttackTimer + 1f) / spinTime + angularOffest).ToRotationVector2() * spinOffset;
                npc.spriteDirection = (target.Center.X < npc.Center.X).ToDirectionInt();
                npc.rotation = (aheadPosition - idealPosition).ToRotation() + MathHelper.PiOver2;
                npc.velocity = npc.SafeDirectionTo(idealPosition) * MathHelper.Min(npc.Distance(idealPosition), hoverSlowdown * 37f);
                if (npc.spriteDirection == 1)
                    npc.rotation += MathHelper.Pi;

                telegraphInterpolant = 0f;
                pupilRotation = npc.rotation - MathHelper.PiOver2;
                pupilOutwardness = MathHelper.Lerp(pupilOutwardness, 0.7f, 0.15f);
                pupilScale = MathHelper.Lerp(pupilScale, 0.4f, 0.15f);
            }

            // Stop in place and look at the target before charging.
            else if (wrappedAttackTimer < spinTime + chargeTelegraphTime)
            {
                float telegraphCompletion = Utils.InverseLerp(0f, chargeTelegraphTime, wrappedAttackTimer - spinTime, true);
                float pupilDilation = Utils.InverseLerp(0f, 0.6f, telegraphCompletion, true);
                telegraphInterpolant = Utils.InverseLerp(0f, 0.65f, telegraphCompletion, true) * Utils.InverseLerp(1f, 0.75f, telegraphCompletion, true);

                // Define the telegraph direction.
                if (telegraphCompletion < 0.9f)
                    telegraphDirection = npc.AngleTo(target.Center + target.velocity * chargePredictiveness);

                // Scream before charging.
                if (wrappedAttackTimer == spinTime + 8f)
                    Main.PlaySound(SoundID.Zombie, npc.Center, 102);

                // Slow down.
                npc.velocity = (npc.velocity * 0.825f).MoveTowards(Vector2.Zero, 1.5f);
                npc.spriteDirection = (target.Center.X < npc.Center.X).ToDirectionInt();

                float idealRotation = telegraphDirection + MathHelper.PiOver2;
                if (npc.spriteDirection == 1)
                    idealRotation += MathHelper.Pi;
                npc.rotation = npc.rotation.AngleLerp(idealRotation, 0.15f);

                pupilRotation = telegraphDirection;
                pupilOutwardness = MathHelper.Lerp(pupilOutwardness, 0.3f, 0.15f);
                pupilScale = MathHelper.SmoothStep(0.4f, 0.95f, pupilDilation);
            }

            // Do the charge.
            else if (wrappedAttackTimer == spinTime + chargeTelegraphTime)
            {
                if (groupIndex == 1f)
                    Main.PlaySound(SoundID.DD2_WyvernDiveDown, target.Center);

                telegraphInterpolant = 0f;
                npc.velocity = telegraphDirection.ToRotationVector2() * chargeSpeed;
                if (chargeCounter == 0)
                    npc.velocity *= 1.325f;

                npc.rotation = npc.velocity.ToRotation() + MathHelper.PiOver2;
                if (npc.spriteDirection == 1)
                    npc.rotation += MathHelper.Pi;

                npc.netUpdate = true;
            }

            // Do contact damage and release phantasmal orbs when charging.
            else
            {
                npc.damage = npc.defDamage;
                if (chargeCounter % 2f == 1f && wrappedAttackTimer % orbReleaseRate == orbReleaseRate - 1f)
                {
                    Main.PlaySound(SoundID.Item72, npc.Center);
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        Vector2 orbVelocity = npc.velocity.SafeNormalize(Vector2.UnitY) * 6f;
                        Utilities.NewProjectileBetter(npc.Center, orbVelocity, ModContent.ProjectileType<PhantasmalOrb>(), 215, 0f);
                    }
                }
            }

            if (chargeCounter >= chargeCount)
                core.Infernum().ExtraAI[5] = 1f;
        }

        public static void DoBehavior_PhantasmalBarrage(NPC npc, Player target, NPC core, float attackTimer, float groupIndex, ref float pupilRotation, ref float pupilOutwardness, ref float pupilScale)
        {
            int repositionTime = 85;
            int sphereCastCount = 6;
            int sphereCastRate = 5;
            int boltCount = 3;
            int chargeTime = 32;
            int chargeCount = 4;
            float repositionOffset = 610f;
            float chargeSpeed = 38f;
            float sphereBounceSpeed = 4f;
            float boltShootSpeed = 6.1f;
            float boltSpread = 0.21f;

            if (MoonLordCoreBehaviorOverride.InFinalPhase)
            {
                sphereCastCount++;
                boltCount += 2;
                repositionOffset -= 35f;
                boltSpread += 0.12f;
            }

            int sphereCastTime = sphereCastCount * sphereCastRate;
            int chargeCounter = (int)(attackTimer / (repositionTime + sphereCastTime + chargeTime));
            float wrappedAttackTimer = attackTimer % (repositionTime + sphereCastTime + chargeTime);
            Vector2 pupilOffset = CalculatePupilOffset(npc);
            ref float groupIndexToAttack = ref core.Infernum().ExtraAI[0];
            ref float telegraphInterpolant = ref npc.Infernum().ExtraAI[1];

            telegraphInterpolant = 0f;

            // Define the group index that should attack on the first frame. Only the first eye will make the core do this.
            if (wrappedAttackTimer == 1f && groupIndex == 1f)
            {
                groupIndexToAttack = Main.rand.Next(3) + 1;
                npc.netUpdate = true;
            }

            if (wrappedAttackTimer < repositionTime)
            {
                float idealRotation = npc.velocity.X * 0.03f;
                npc.spriteDirection = (target.Center.X < npc.Center.X).ToDirectionInt();
                if (npc.spriteDirection == 1)
                    idealRotation += MathHelper.Pi;
                npc.rotation = npc.rotation.AngleLerp(idealRotation, 0.3f).AngleTowards(idealRotation, 0.1f);

                pupilRotation = npc.AngleTo(target.Center);
                pupilOutwardness = MathHelper.Lerp(pupilOutwardness, 0.45f, 0.15f);
                pupilScale = MathHelper.Lerp(pupilScale, 0.35f, 0.15f);

                // Hover into position before attacking.
                float slowdownFactor = Utils.InverseLerp(0.9f, 0.65f, wrappedAttackTimer / repositionTime, true);
                Vector2 hoverDestination = target.Center + (MathHelper.TwoPi * (groupIndex - 1f + chargeCounter * 0.5f) / 3f).ToRotationVector2() * repositionOffset;
                Vector2 idealVelocity = npc.SafeDirectionTo(hoverDestination) * slowdownFactor * 31f;
                npc.Center = npc.Center.MoveTowards(hoverDestination, 12f);
                npc.SimpleFlyMovement(idealVelocity, slowdownFactor * 0.75f);
                npc.velocity = Vector2.Lerp(npc.velocity, idealVelocity, 0.09f);
                if (npc.WithinRange(hoverDestination, 16f))
                {
                    npc.velocity = Vector2.Zero;
                    npc.Center = hoverDestination;
                }
            }

            // Create phantasmal spheres.
            else if (wrappedAttackTimer < repositionTime + sphereCastTime)
            {
                float sphereCastCompletion = Utils.InverseLerp(0f, sphereCastTime, wrappedAttackTimer - repositionTime, true);
                float continuousSphereOffsetAngle = MathHelper.TwoPi * sphereCastCompletion;
                float idealPupilRotation = npc.AngleTo(target.Center).AngleLerp(continuousSphereOffsetAngle, CalamityUtils.Convert01To010(sphereCastCompletion));
                pupilRotation = pupilRotation.AngleLerp(idealPupilRotation, 0.2f);
                pupilOutwardness = MathHelper.Lerp(pupilOutwardness, 0.5f, 0.15f);
                pupilScale = MathHelper.Lerp(pupilScale, 0.5f, 0.15f);

                if (groupIndex == groupIndexToAttack)
                    telegraphInterpolant = sphereCastCompletion;

                if ((wrappedAttackTimer - repositionTime) % sphereCastRate == 0f)
                {
                    int sphereCreationCounter = (int)((wrappedAttackTimer - repositionTime) / sphereCastRate);

                    // Creates a pattern wherein every second sphere is created opposite to the previous one.
                    // The consequence of this is that the full "circle" is only calculated halfway. The secondary
                    // calculation will allow for the full circle to be completed.
                    float sphereOffsetAngle = MathHelper.Pi * sphereCreationCounter / sphereCastCount;
                    if (sphereCreationCounter % 2 == 1)
                        sphereOffsetAngle = MathHelper.Pi * (sphereCreationCounter - 1f) / sphereCastCount + MathHelper.Pi;
                    Vector2 sphereOffset = -Vector2.UnitY.RotatedBy(sphereOffsetAngle) * 36f;

                    Main.PlaySound(SoundID.Item122, npc.Center + sphereOffset);
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        Vector2 sphereVelocity = sphereOffset.SafeNormalize(Vector2.Zero) * 6f;
                        int sphere = Utilities.NewProjectileBetter(npc.Center + sphereOffset, sphereVelocity, ProjectileID.PhantasmalSphere, 215, 0f);
                        if (Main.projectile.IndexInRange(sphere))
                        {
                            Main.projectile[sphere].Opacity = sphereCastCompletion;
                            Main.projectile[sphere].ai[0] = -1f;
                            Main.projectile[sphere].ai[1] = npc.whoAmI;
                        }
                    }
                }

                if (Main.netMode != NetmodeID.MultiplayerClient &&
                    wrappedAttackTimer == repositionTime + (int)(sphereCastTime * 0.6f) &&
                    groupIndex == groupIndexToAttack)
                {
                    Utilities.NewProjectileBetter(npc.Center, Vector2.Zero, ModContent.ProjectileType<MoonLordWave>(), 0, 0f);
                }
            }

            // Create an explosion as an indicator prior to attacking.
            // Also make the eye that should charge do so.
            if (wrappedAttackTimer == repositionTime + sphereCastTime)
            {
                if (groupIndex == groupIndexToAttack)
                {
                    var explosionSound = Main.PlaySound(SoundID.DD2_BetsyFireballImpact, npc.Center);
                    if (explosionSound != null)
                    {
                        explosionSound.Volume = MathHelper.Clamp(explosionSound.Volume * 1.9f, 0f, 1f);
                        explosionSound.Pitch = -0.4f;
                    }

                    npc.velocity = npc.SafeDirectionTo(target.Center) * chargeSpeed;
                    npc.spriteDirection = (target.Center.X < npc.Center.X).ToDirectionInt();
                    npc.netUpdate = true;

                    // Make all phantasmal spheres move away from the eye that created them.
                    foreach (Projectile sphere in Utilities.AllProjectilesByID(ProjectileID.PhantasmalSphere))
                    {
                        if (sphere.ai[0] == -1f)
                        {
                            sphere.velocity = -sphere.SafeDirectionTo(Main.npc[(int)sphere.ai[1]].Center) * sphereBounceSpeed;
                            sphere.ai[0] = 0f;
                            sphere.netUpdate = true;
                        }
                    }
                }

                // Release phantasmal bolts at the target.
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    Vector2 boltSpawnPosition = npc.Center + pupilOffset;
                    for (int i = 0; i < boltCount; i++)
                    {
                        Vector2 boltVelocity = (target.Center - boltSpawnPosition).SafeNormalize(Vector2.UnitY) * boltShootSpeed;
                        if (boltCount > 1)
                            boltVelocity = boltVelocity.RotatedBy(MathHelper.Lerp(-boltSpread, boltSpread, i / (float)(boltCount - 1f)));
                        Utilities.NewProjectileBetter(boltSpawnPosition, boltVelocity, ProjectileID.PhantasmalBolt, 210, 0f);
                    }
                }
            }

            // Handle charge effects.
            if (wrappedAttackTimer >= repositionTime + sphereCastTime && groupIndex == groupIndexToAttack)
            {
                pupilRotation = npc.velocity.ToRotation();
                npc.rotation = npc.velocity.ToRotation() + MathHelper.PiOver2;
                if (npc.spriteDirection == 1)
                    npc.rotation += MathHelper.Pi;
                npc.damage = npc.defDamage;
            }

            if (chargeCounter >= chargeCount)
            {
                core.Infernum().ExtraAI[5] = 1f;

                // Delete all spheres when the attack ends, to prevent unfair projectile overlap.
                foreach (Projectile sphere in Utilities.AllProjectilesByID(ProjectileID.PhantasmalSphere))
                    sphere.Kill();
            }
        }

        public static void DoBehavior_PhantasmalWrath(NPC npc, Player target, NPC core, float attackTimer, float groupIndex, ref float pupilRotation, ref float pupilOutwardness, ref float pupilScale)
        {
            int attckDelay = 60;
            int slowdownTime = 16;
            int boltCount = 12;
            int chargeTime = 36;
            int chargeCount = 4;
            float spinOffset = 400f;
            float boltShootSpeed = 6.25f;
            float chargeSpeed = 38.5f;

            if (MoonLordCoreBehaviorOverride.InFinalPhase)
            {
                chargeTime -= 3;
                boltCount += 6;
                boltShootSpeed -= 0.5f;
                chargeSpeed += 3f;
            }

            int chargeCounter = (int)(attackTimer / (attckDelay + slowdownTime + chargeTime));
            float wrappedAttackTimer = attackTimer % (attckDelay + slowdownTime + chargeTime);
            ref float telegraphInterpolant = ref npc.Infernum().ExtraAI[1];

            telegraphInterpolant = 0f;

            // Circle around the player before attacking.
            if (wrappedAttackTimer < attckDelay)
            {
                float angularOffest = MathHelper.TwoPi * (groupIndex - 1f) / NPC.CountNPCS(npc.type);
                float spinArc = MathHelper.Pi * 0.666f;
                float hoverSlowdown = Utils.InverseLerp(1f, 0.8f, wrappedAttackTimer / attckDelay, true);
                Vector2 idealPosition = target.Center + (spinArc * wrappedAttackTimer / attckDelay + angularOffest).ToRotationVector2() * spinOffset;
                Vector2 aheadPosition = target.Center + (spinArc * (wrappedAttackTimer + 1f) / attckDelay + angularOffest).ToRotationVector2() * spinOffset;

                pupilRotation = (aheadPosition - idealPosition).ToRotation();
                pupilOutwardness = MathHelper.Lerp(pupilOutwardness, 0.5f, 0.15f);
                pupilScale = MathHelper.Lerp(pupilScale, 0.4f, 0.15f);

                npc.spriteDirection = (target.Center.X < npc.Center.X).ToDirectionInt();
                npc.rotation = pupilRotation + MathHelper.PiOver2;
                npc.velocity = npc.SafeDirectionTo(idealPosition) * MathHelper.Min(npc.Distance(idealPosition), hoverSlowdown * 37f);
                if (npc.spriteDirection == 1)
                    npc.rotation += MathHelper.Pi;
            }

            // Slow down.
            else if (wrappedAttackTimer < attckDelay + slowdownTime)
            {
                float idealRotation = npc.spriteDirection == 1 ? MathHelper.Pi : 0f;
                if (groupIndex != 1f)
                {
                    idealRotation += npc.AngleTo(target.Center) + MathHelper.PiOver2;
                    telegraphInterpolant = Utils.InverseLerp(attckDelay, attckDelay + slowdownTime, attackTimer, true);
                }

                // Scream before charging.
                if (wrappedAttackTimer == attckDelay + (int)(slowdownTime * 0.5f) && groupIndex != 1f)
                    Main.PlaySound(SoundID.Zombie, npc.Center, Main.rand.Next(100, 103));

                pupilRotation = pupilRotation.AngleLerp(npc.AngleTo(target.Center), 0.15f);
                pupilOutwardness = MathHelper.Lerp(pupilOutwardness, 0.4f, 0.15f);
                pupilScale = MathHelper.Lerp(pupilScale, 0.75f, 0.15f);

                npc.rotation = npc.rotation.AngleLerp(idealRotation, 0.12f).AngleTowards(idealRotation, 0.05f);
                npc.velocity = npc.velocity.MoveTowards(Vector2.Zero, 2f) * 0.92f;
            }

            // Have the first eye release a burst of bolts in all directions.
            else if (groupIndex == 1f)
            {
                pupilRotation = pupilRotation.AngleLerp(npc.AngleTo(target.Center), 0.15f);
                if (Main.netMode != NetmodeID.MultiplayerClient && wrappedAttackTimer == attckDelay + slowdownTime + 1f)
                {
                    Vector2 boltSpawnPosition = npc.Center + CalculatePupilOffset(npc);
                    for (int i = 0; i < boltCount; i++)
                    {
                        Vector2 boltShootVelocity = (MathHelper.TwoPi * i / boltCount).ToRotationVector2() * boltShootSpeed;
                        Utilities.NewProjectileBetter(boltSpawnPosition, boltShootVelocity, ProjectileID.PhantasmalBolt, 215, 0f);
                    }
                }
            }

            else
            {
                if (wrappedAttackTimer == attckDelay + slowdownTime + 1f)
                {
                    Main.PlaySound(SoundID.DD2_WyvernDiveDown, npc.Center);
                    npc.spriteDirection = (target.Center.X < npc.Center.X).ToDirectionInt();
                    npc.velocity = npc.SafeDirectionTo(target.Center) * chargeSpeed;
                    npc.netUpdate = true;
                }

                npc.damage = npc.defDamage;
                npc.rotation = npc.velocity.ToRotation() + MathHelper.PiOver2;
                if (npc.spriteDirection == 1)
                    npc.rotation += MathHelper.Pi;

                // Release phantasmal eyes.
                if (Main.netMode != NetmodeID.MultiplayerClient && attackTimer % 8f == 7f && !npc.WithinRange(target.Center, 300f))
                {
                    Vector2 eyeVelocity = -Vector2.UnitY * Main.rand.NextFloat(8f, 11f);
                    Utilities.NewProjectileBetter(npc.Center, eyeVelocity, ProjectileID.PhantasmalEye, 215, 0f);
                }
            }

            if (chargeCounter >= chargeCount)
                core.Infernum().ExtraAI[5] = 1f;
        }

        public static Vector2 CalculatePupilOffset(NPC npc, int? directionOverride = null)
        {
            int direction = directionOverride ?? npc.spriteDirection;
            return npc.localAI[0].ToRotationVector2() * npc.localAI[1] * 25f - Vector2.UnitY.RotatedBy(npc.rotation) * -direction * 20f;
        }

        public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Color lightColor)
        {
            Texture2D texture = Main.npcTexture[npc.type];
            Texture2D pupilTexture = Main.extraTexture[19];
            Vector2 baseDrawPosition = npc.Center - Main.screenPosition - (npc.rotation + MathHelper.PiOver2).ToRotationVector2() * npc.spriteDirection * 32f;
            SpriteEffects direction = npc.spriteDirection == 1 ? SpriteEffects.FlipVertically : SpriteEffects.FlipHorizontally;
            Color color = npc.GetAlpha(Color.Lerp(lightColor, Color.White, 0.3f));
            spriteBatch.Draw(texture, baseDrawPosition, npc.frame, color, npc.rotation, npc.frame.Size() * 0.5f, 1f, direction, 0f);

            Vector2 pupilOffset = CalculatePupilOffset(npc);
            spriteBatch.Draw(pupilTexture, baseDrawPosition + pupilOffset, null, color, npc.rotation, pupilTexture.Size() / 2f, npc.localAI[2], SpriteEffects.None, 0f);

            // Draw line telegraphs as necessary.
            NPC core = Main.npc[(int)npc.ai[3]];
            if (core.ai[0] == (int)MoonLordCoreBehaviorOverride.MoonLordAttackState.PhantasmalRush)
            {
                float lineTelegraphInterpolant = npc.Infernum().ExtraAI[1];
                if (lineTelegraphInterpolant > 0f)
                {
                    spriteBatch.SetBlendState(BlendState.Additive);

                    Texture2D line = ModContent.GetTexture("InfernumMode/ExtraTextures/BloomLineSmall");
                    Texture2D bloomCircle = ModContent.GetTexture("CalamityMod/ExtraTextures/THanosAura");

                    Color outlineColor = Color.Lerp(Color.Turquoise, Color.White, lineTelegraphInterpolant);
                    Vector2 origin = new Vector2(line.Width / 2f, line.Height);
                    Vector2 beamScale = new Vector2(lineTelegraphInterpolant * 0.5f, 2.4f);
                    Vector2 drawPosition = baseDrawPosition + pupilOffset;

                    // Create bloom on the pupil.
                    Vector2 bloomSize = new Vector2(30f) / bloomCircle.Size() * (float)Math.Pow(lineTelegraphInterpolant, 2D);
                    spriteBatch.Draw(bloomCircle, drawPosition, null, Color.Turquoise, 0f, bloomCircle.Size() * 0.5f, bloomSize, 0, 0f);

                    if (npc.Infernum().ExtraAI[0] >= -100f)
                    {
                        for (int i = -1; i <= 1; i += 2)
                        {
                            Vector2 beamDirection = -npc.SafeDirectionTo(Main.player[npc.target].Center).RotatedBy(npc.Infernum().ExtraAI[0] * i);
                            float beamRotation = beamDirection.ToRotation() - MathHelper.PiOver2;
                            spriteBatch.Draw(line, drawPosition, null, outlineColor, beamRotation, origin, beamScale, 0, 0f);
                        }
                    }

                    spriteBatch.ResetBlendState();
                }
            }
            if (core.ai[0] == (int)MoonLordCoreBehaviorOverride.MoonLordAttackState.PhantasmalDance)
            {
                float lineTelegraphInterpolant = npc.Infernum().ExtraAI[0];
                if (lineTelegraphInterpolant > 0f)
                {
                    spriteBatch.SetBlendState(BlendState.Additive);

                    Texture2D line = ModContent.GetTexture("InfernumMode/ExtraTextures/BloomLineSmall");
                    Texture2D bloomCircle = ModContent.GetTexture("CalamityMod/ExtraTextures/THanosAura");

                    Color outlineColor = Color.Lerp(Color.Turquoise, Color.White, lineTelegraphInterpolant);
                    Vector2 origin = new Vector2(line.Width / 2f, line.Height);
                    Vector2 beamScale = new Vector2(lineTelegraphInterpolant * 1.3f, 2.4f);
                    Vector2 drawPosition = baseDrawPosition + pupilOffset;

                    // Create bloom on the pupil.
                    Vector2 bloomSize = new Vector2(30f) / bloomCircle.Size() * (float)Math.Pow(lineTelegraphInterpolant, 2D);
                    spriteBatch.Draw(bloomCircle, drawPosition, null, Color.Turquoise, 0f, bloomCircle.Size() * 0.5f, bloomSize, 0, 0f);

                    Vector2 beamDirection = -npc.Infernum().ExtraAI[1].ToRotationVector2();
                    float beamRotation = beamDirection.ToRotation() - MathHelper.PiOver2;
                    spriteBatch.Draw(line, drawPosition, null, outlineColor, beamRotation, origin, beamScale, 0, 0f);

                    spriteBatch.ResetBlendState();
                }
            }
            if (core.ai[0] == (int)MoonLordCoreBehaviorOverride.MoonLordAttackState.PhantasmalBarrage ||
                core.ai[0] == (int)MoonLordCoreBehaviorOverride.MoonLordAttackState.PhantasmalWrath)
            {
                float lineTelegraphInterpolant = npc.Infernum().ExtraAI[1];
                if (lineTelegraphInterpolant > 0f)
                {
                    spriteBatch.SetBlendState(BlendState.Additive);

                    Texture2D line = ModContent.GetTexture("InfernumMode/ExtraTextures/BloomLineSmall");
                    Color outlineColor = Color.Lerp(Color.Turquoise, Color.White, lineTelegraphInterpolant);
                    Vector2 origin = new Vector2(line.Width / 2f, line.Height);
                    Vector2 beamScale = new Vector2(lineTelegraphInterpolant * 1.3f, 2.4f);
                    Vector2 drawPosition = baseDrawPosition + pupilOffset;

                    Vector2 beamDirection = -npc.SafeDirectionTo(Main.player[npc.target].Center);
                    float beamRotation = beamDirection.ToRotation() - MathHelper.PiOver2;
                    spriteBatch.Draw(line, drawPosition, null, outlineColor, beamRotation, origin, beamScale, 0, 0f);

                    spriteBatch.ResetBlendState();
                }
            }
            return false;
        }
    }
}
