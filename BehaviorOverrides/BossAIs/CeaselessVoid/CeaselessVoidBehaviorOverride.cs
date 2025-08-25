using CalamityMod;
using CalamityMod.Events;
using CalamityMod.Items.Weapons.Typeless;
using CalamityMod.NPCs;
using CalamityMod.NPCs.CeaselessVoid;
using CalamityMod.Projectiles.Boss;
using CalamityMod.World;
using InfernumMode.BehaviorOverrides.BossAIs.Signus;
using CalamityMod.Items.Weapons.DraedonsArsenal;
using InfernumMode.OverridingSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using System.Collections.Generic;
using Terraria.Graphics.Shaders;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

using CeaselessVoidBoss = CalamityMod.NPCs.CeaselessVoid.CeaselessVoid;

namespace InfernumMode.BehaviorOverrides.BossAIs.CeaselessVoid
{
    public class CeaselessVoidBehaviorOverride : NPCBehaviorOverride
    {
        public override int NPCOverrideType => ModContent.NPCType<CeaselessVoidBoss>();

        public override NPCOverrideContext ContentToOverride => NPCOverrideContext.NPCAI | NPCOverrideContext.NPCSetDefaults;

        public const int BulletHellTime = 900;

        #region Enumerations
        public enum CeaselessVoidAttackType
        {
            DarkEnergySwirl,
            RealityRendCharge,
            ConvergingEnergyBarrages,
            SlowEnergySpirals,
            DarkEnergyBulletHell,
            BlackHoleSuck
        }
        #endregion

        #region Set Defaults
        public const float Phase2LifeRatio = 0.65f;

        public const float Phase3LifeRatio = 0.3f;

        public const float DarkEnergyOffsetRadius = 1120f;

        public override float[] PhaseLifeRatioThresholds => new float[]
        {
            Phase2LifeRatio,
            Phase3LifeRatio
        };
        public override void SetDefaults(NPC npc)
        {
            npc.npcSlots = 36f;
            npc.width = 100;
            npc.height = 100;
            npc.defense = 0;
            npc.lifeMax = 363000;
            Mod calamityModMusic = ModLoader.GetMod("CalamityModMusic");
			if (calamityModMusic != null)
				npc.modNPC.music = calamityModMusic.GetSoundSlot(SoundType.Music, "Sounds/Music/ScourgeofTheUniverse");
			else
				npc.modNPC.music = MusicID.Boss3;
			if (CalamityWorld.DoGSecondStageCountdown <= 0)
			{
				npc.value = Item.buyPrice(0, 35, 0, 0);
				if (calamityModMusic != null)
					npc.modNPC.music = calamityModMusic.GetSoundSlot(SoundType.Music, "Sounds/Music/Void");
				else
					npc.modNPC.music = MusicID.Boss3;
			}
            npc.aiStyle = -1;
            npc.modNPC.aiType = -1;
            npc.knockBackResist = 0f;
            for (int k = 0; k < npc.buffImmune.Length; k++)
                npc.buffImmune[k] = true;

            npc.noGravity = true;
            npc.noTileCollide = true;
            npc.boss = true;
            npc.DeathSound = SoundID.NPCDeath14;
        }
        #endregion Set Defaults
        
        #region AI

        public override bool PreAI(NPC npc)
        {
            // Reset DR.
            npc.Calamity().DR = 0.2f;

            // Select a new target if an old one was lost.
            npc.TargetClosestIfTargetIsInvalid();
            Player target = Main.player[npc.target];

            CalamityGlobalNPC.voidBoss = npc.whoAmI;

            if (!target.active || target.dead || !npc.WithinRange(target.Center, 7200f))
            {
                npc.velocity = Vector2.Lerp(npc.velocity, Vector2.UnitY * 38f, 0.08f);
                if (!npc.WithinRange(target.Center, 1450f) || target.dead)
                {
                    npc.life = 0;
                    npc.active = false;
                    npc.netUpdate = true;
                }
                return false;
            }

            npc.timeLeft = 3600;
            npc.chaseable = true;

            float lifeRatio = npc.life / (float)npc.lifeMax;
            bool phase2 = lifeRatio < Phase2LifeRatio;
            bool phase3 = lifeRatio < Phase3LifeRatio;
            bool enraged = target.Center.Y < Main.worldSurface * 16f && !BossRushEvent.BossRushActive;
            ref float attackType = ref npc.ai[0];
            ref float attackTimer = ref npc.ai[1];
            ref float currentPhase = ref npc.ai[2];

            // Do phase transitions.
            if (currentPhase == 0f && phase2)
            {
                currentPhase = 1f;
                SelectNewAttack(npc);
                attackType = (int)CeaselessVoidAttackType.DarkEnergyBulletHell;
            }
            if (currentPhase == 1f && phase3)
            {
                Utilities.DeleteAllProjectiles(true, ModContent.ProjectileType<CelestialBarrage>(), ModContent.ProjectileType<ConvergingCelestialBarrage>(), ModContent.ProjectileType<DarkEnergyBolt>(),
                ModContent.ProjectileType<EnergyTelegraph>(), ModContent.ProjectileType<SpiralEnergyLaser>());
                currentPhase = 2f;
                SelectNewAttack(npc);
            }

            // This debuff is not fun.
            if (target.HasBuff(BuffID.VortexDebuff))
                target.ClearBuff(BuffID.VortexDebuff);

            // Reset things.
            npc.damage = 0;
            npc.dontTakeDamage = enraged;
            npc.Calamity().CurrentlyEnraged = npc.dontTakeDamage;
            if (enraged)
            {
                phase2 = true;
                phase3 = true;
            }
            
            switch ((CeaselessVoidAttackType)(int)attackType)
            {
                case CeaselessVoidAttackType.DarkEnergySwirl:
                    DoBehavior_DarkEnergySwirl(npc, phase2, phase3, target, ref attackTimer);
                    break;
                case CeaselessVoidAttackType.RealityRendCharge:
                    DoBehavior_RealityRendCharge(npc, phase2, phase3, enraged, target, ref attackTimer);
                    break;
                case CeaselessVoidAttackType.ConvergingEnergyBarrages:
                    DoBehavior_ConvergingEnergyBarrages(npc, phase2, phase3, enraged, target, ref attackTimer);
                    break;
                case CeaselessVoidAttackType.SlowEnergySpirals:
                    DoBehavior_SlowEnergySpirals(npc, phase2, phase3, enraged, target, ref attackTimer);
                    break;
                case CeaselessVoidAttackType.DarkEnergyBulletHell:
                    DoBehavior_DarkEnergyBulletHell(npc, target, ref attackTimer);
                    break;
                case CeaselessVoidAttackType.BlackHoleSuck:
                    DoBehavior_BlackHoleSuck(npc, target, ref attackTimer);
                    break;
            }

            attackTimer++;
            return false;
        }

        public static void HandleDeathStuff(NPC npc)
        {
            if (npc.Infernum().ExtraAI[6] == 1f)
                return;

            npc.active = true;
            npc.dontTakeDamage = true;
            npc.Infernum().ExtraAI[6] = 1f;
            npc.life = 1;
            
            SelectNewAttack(npc);
            npc.ai[0] = (int)CeaselessVoidAttackType.BlackHoleSuck;

            npc.netUpdate = true;
        }

        public static void DoBehavior_DarkEnergySwirl(NPC npc, bool phase2, bool phase3, Player target, ref float attackTimer)
        {
            int totalRings = 4;
            int energyCountPerRing = 7;
            int hoverRedirectDelay = 300;
            int portalFireRate = 105;
            int darkEnergyID = ModContent.NPCType<DarkEnergy>();

            if (phase2)
                energyCountPerRing += 2;
            if (phase3)
            {
                energyCountPerRing++;
                totalRings++;
            }

            ref float hasCreatedDarkEnergy = ref npc.Infernum().ExtraAI[0];

            // Initialize by creating the dark energy ring.
            if (Main.netMode != NetmodeID.MultiplayerClient && hasCreatedDarkEnergy == 0f)
            {
                for (int i = 0; i < totalRings; i++)
                {
                    float spinMovementSpeed = MathHelper.Lerp(1.45f, 3f, i / (float)(totalRings - 1f));
                    for (int j = 0; j < energyCountPerRing; j++)
                    {
                        float offsetRadius = MathHelper.Lerp(0f, 150f, CalamityUtils.Convert01To010(j / (float)(energyCountPerRing - 1f)));
                        float offsetAngle = MathHelper.TwoPi * j / energyCountPerRing;
                        NPC.NewNPC((int)npc.Center.X, (int)npc.Center.Y, darkEnergyID, npc.whoAmI, offsetAngle, spinMovementSpeed, offsetRadius);
                    }
                }
                npc.Center = target.Center - Vector2.UnitY * 300f;
                hasCreatedDarkEnergy = 1f;
                npc.netUpdate = true;
            }

            // Redirect to a different offset after a sufficient amount of time has passed.
            if (attackTimer >= hoverRedirectDelay)
            {
                attackTimer = 0f;
                npc.netUpdate = true;
            }

            // Disable damage.
            npc.dontTakeDamage = true;

            // Shoot lasers if moving slowly.
            if (attackTimer % portalFireRate == portalFireRate - 1f && npc.velocity.Length() < 8f)
            {
                Main.PlaySound(SoundID.Item33, npc.Center);
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    for (int i = 0; i < 8; i++)
                    {
                        Vector2 laserShootVelocity = npc.SafeDirectionTo(target.Center).RotatedBy(MathHelper.TwoPi * i / 8f) * 9.6f;
                        int fuckYou = Utilities.NewProjectileBetter(npc.Center, laserShootVelocity, ModContent.ProjectileType<DoGBeam>(), 0, 0f);
                        if (Main.projectile.IndexInRange(fuckYou))
                            Main.projectile[fuckYou].ai[0] = 270 / 4;
                    }
                }
            }
            
            // Calculate the life ratio of all dark energy combined.
            // If it is sufficiently low then all remaining dark energy fades away and CV goes to the next attack.
            int darkEnergyTotalLife = 0;
            int darkEnergyTotalMaxLife = 0;
            List<NPC> darkEnergies = new List<NPC>();
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                if (Main.npc[i].active && Main.npc[i].type == darkEnergyID)
                {
                    darkEnergyTotalLife += Main.npc[i].life;
                    darkEnergyTotalMaxLife = Main.npc[i].lifeMax;
                    darkEnergies.Add(Main.npc[i]);
                }
            }
            darkEnergyTotalMaxLife *= totalRings * energyCountPerRing;

            float darkEnergyLifeRatio = darkEnergyTotalLife / (float)darkEnergyTotalMaxLife;
            if (darkEnergyTotalMaxLife <= 0)
                darkEnergyLifeRatio = 0f;

            if (darkEnergyLifeRatio <= 0.3f)
            {
                foreach (NPC darkEnergy in darkEnergies)
                {
                    if (darkEnergy.Infernum().ExtraAI[1] == 0f)
                    {
                        darkEnergy.Infernum().ExtraAI[1] = 1f;
                        darkEnergy.netUpdate = true;
                    }
                }
                
                SelectNewAttack(npc);
            }
        }
        
        public static void DoBehavior_RealityRendCharge(NPC npc, bool phase2, bool phase3, bool enraged, Player target, ref float attackTimer)
        {
            int chargeTime = 37;
            int repositionTime = 210;
            int chargeCount = 3;
            float hoverOffset = 640f;
            float chargeDistance = hoverOffset + 975f;
            float scaleFactorDelta = 0f;
            if (phase2)
            {
                chargeTime -= 6;
                repositionTime -= 20;
                scaleFactorDelta += 0.2f;
            }
            if (phase3)
            {
                chargeTime -= 8;
                scaleFactorDelta += 0.8f;
                chargeDistance += 900f;
            }
            if (BossRushEvent.BossRushActive)
            {
                chargeTime -= 6;
                scaleFactorDelta += 0.6f;
            }

            if (enraged)
                scaleFactorDelta = 1.45f;

            float chargeSpeed = chargeDistance / chargeTime;
            ref float tearProjectileIndex = ref npc.Infernum().ExtraAI[0];
            ref float attackState = ref npc.Infernum().ExtraAI[1];
            ref float chargeDirection = ref npc.Infernum().ExtraAI[2];
            ref float chargeCounter = ref npc.Infernum().ExtraAI[3];
            ref float verticalHoverOffset = ref npc.Infernum().ExtraAI[4];

            switch ((int)attackState)
            {
                // Get into position for the horizontal charge.
                case 0:
                    if (attackTimer == 1f)
                        verticalHoverOffset = Main.rand.NextFloat(-0.84f, 0.84f) * hoverOffset;

                    Vector2 hoverDestination = target.Center + Vector2.UnitX * (target.Center.X < npc.Center.X).ToDirectionInt() * hoverOffset;
                    hoverDestination.Y += verticalHoverOffset;
                    npc.SimpleFlyMovement(npc.SafeDirectionTo(hoverDestination) * 23f, 0.9f);

                    // Begin the charge if either enough time has passed or within sufficient range of the hover destination.
                    if ((attackTimer >= repositionTime || npc.WithinRange(hoverDestination, 85f)) && attackTimer >= 30f)
                    {
                        attackTimer = 0f;
                        attackState = 1f;
                        chargeDirection = npc.AngleTo(target.Center);
                        npc.velocity *= 0.372f;
                        npc.netUpdate = true;

                        // Create the reality tear.
						Main.PlaySound(InfernumMode.Instance.GetLegacySoundSlot(SoundType.Item, "Sounds/Item/YanmeiKnifeHit"), target.Center);
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            tearProjectileIndex = Utilities.NewProjectileBetter(npc.Center, Vector2.Zero, ModContent.ProjectileType<RealityTear>(), 0, 0f);
                            if (Main.projectile.IndexInRange((int)tearProjectileIndex))
                                Main.projectile[(int)tearProjectileIndex].localAI[0] = scaleFactorDelta;
                        }
                    }
                    break;

                // Do the charge.
                case 1:
                    npc.damage = npc.defDamage;
                    npc.velocity = Vector2.Lerp(npc.velocity, chargeDirection.ToRotationVector2() * chargeSpeed, 0.125f);
                    if (attackTimer >= chargeTime)
                    {
                        attackState = 0f;
                        attackTimer = 0f;
                        tearProjectileIndex = -1f;
                        chargeCounter++;
                        npc.netUpdate = true;

                        if (chargeCounter >= chargeCount)
                            SelectNewAttack(npc);
                        else
                            npc.velocity *= 0.3f;
                    }
                    break;
            }
        }

        public static void DoBehavior_ConvergingEnergyBarrages(NPC npc, bool phase2, bool phase3, bool enraged, Player target, ref float attackTimer)
        {
            int hoverTime = 20;
            int barrageBurstCount = 4;
            int barrageTelegraphTime = 18;
            int barrageShootRate = 28;
            int barrageCount = 13;
            int attackTransitionDelay = 40;
            float maxShootOffsetAngle = 1.49f;
            float initialBarrageSpeed = 16f;
            if (phase2)
            {
                initialBarrageSpeed += 1.8f;
                barrageTelegraphTime -= 5;
                barrageShootRate -= 6;
            }
            if (phase3)
            {
                initialBarrageSpeed += 3f;
                barrageShootRate -= 6;
                barrageTelegraphTime -= 6;
                attackTransitionDelay -= 14;
            }
            if (enraged)
                initialBarrageSpeed += 7.5f;
            if (BossRushEvent.BossRushActive)
                initialBarrageSpeed += 8f;

            ref float hoverOffsetAngle = ref npc.Infernum().ExtraAI[0];
            ref float playerShootDirection = ref npc.Infernum().ExtraAI[1];
            ref float barrageBurstCounter = ref npc.Infernum().ExtraAI[2];
            if (barrageBurstCounter == 0f)
                hoverTime += 64;

            // Hover before firing.
            if (attackTimer < hoverTime)
            {
                Vector2 hoverDestination = target.Center - Vector2.UnitY.RotatedBy(hoverOffsetAngle) * 640f;
                npc.Center = Vector2.Lerp(npc.Center, hoverDestination, 0.025f);

                Vector2 idealVelocity = npc.SafeDirectionTo(hoverDestination) * 25f;
                npc.SimpleFlyMovement(idealVelocity, 1.9f);
                if (npc.WithinRange(hoverDestination, 100f))
                    npc.velocity *= 0.85f;
            }
            else
                npc.velocity *= 0.9f;

            // Prepare particle line telegraphs.
            if (attackTimer == hoverTime + barrageShootRate - barrageTelegraphTime)
            {
                Main.PlaySound(SoundID.Item8, npc.Center);

                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    playerShootDirection = npc.AngleTo(target.Center);
                    for (int i = 0; i < barrageCount; i++)
                    {
                        float offsetAngle = MathHelper.Lerp(-maxShootOffsetAngle, maxShootOffsetAngle, i / (float)(barrageCount - 1f));

                        List<Vector2> telegraphPoints = new List<Vector2>();
                        for (int frames = 1; frames < 84; frames += 4)
                        {
                            Vector2 linePosition = ConvergingCelestialBarrage.SimulateMotion(npc.Center, (offsetAngle + playerShootDirection).ToRotationVector2() * initialBarrageSpeed, playerShootDirection, frames);
                            telegraphPoints.Add(linePosition);
                        }

                        int shard = Utilities.NewProjectileBetter(npc.Center, Vector2.Zero, ModContent.ProjectileType<EnergyTelegraph>(), 0, 0f);
                        if (Main.projectile.IndexInRange(shard))
                        {
                            Main.projectile[shard].ai[0] = i / (float)barrageCount;
                            Main.projectile[shard].ModProjectile<EnergyTelegraph>().TelegraphPoints = telegraphPoints.ToArray();
                        }
                    }
                    npc.velocity = Vector2.Zero;
                    npc.netUpdate = true;
                }
            }

            // Shoot.
            if (attackTimer == hoverTime + barrageShootRate)
            {
                Main.PlaySound(SoundID.Item28, npc.Center);
                for (int i = 0; i < barrageCount; i++)
                {
                    float offsetAngle = MathHelper.Lerp(-maxShootOffsetAngle, maxShootOffsetAngle, i / (float)(barrageCount - 1f));
                    Vector2 shootVelocity = (offsetAngle + playerShootDirection).ToRotationVector2() * initialBarrageSpeed;
                    int barrage = Utilities.NewProjectileBetter(npc.Center, shootVelocity, ModContent.ProjectileType<ConvergingCelestialBarrage>(), 250, 0f);
                    if (Main.projectile.IndexInRange(barrage))
                        Main.projectile[barrage].ai[1] = playerShootDirection;
                }
            }

            if (attackTimer >= hoverTime + barrageShootRate + attackTransitionDelay)
            {
                attackTimer = 0f;
                hoverOffsetAngle += MathHelper.TwoPi / barrageBurstCount + Main.rand.NextFloatDirection() * 0.36f;
                barrageBurstCounter++;
                if (barrageBurstCounter >= barrageBurstCount)
                    SelectNewAttack(npc);

                npc.netUpdate = true;
            }

        }

        public static void DoBehavior_SlowEnergySpirals(NPC npc, bool phase2, bool phase3, bool enraged, Player target, ref float attackTimer)
        {
            int shootDelay = 96;
            int burstShootRate = 26;
            int laserBurstCount = 12;
            float burstShootSpeed = 13f;
            if (phase2)
                burstShootRate -= 4;
            if (phase3)
            {
                burstShootRate -= 4;
                laserBurstCount += 2;
                burstShootSpeed -= 1.6f;
            }
            if (enraged)
            {
                burstShootRate -= 8;
                laserBurstCount += 3;
                burstShootSpeed += 7.5f;
            }
            if (BossRushEvent.BossRushActive)
            {
                laserBurstCount += 2;
                burstShootSpeed += 8.4f;
            }

            // Disable contact damage.
            npc.damage = 0;

            ref float spinOffsetAngle = ref npc.Infernum().ExtraAI[1];

            // Make Ceaseless Void circle the target.
            npc.velocity *= 0.9f;
            npc.Center = npc.Center.MoveTowards(target.Center - Vector2.UnitY.RotatedBy(spinOffsetAngle) * 540f, 30f);
            spinOffsetAngle += MathHelper.ToRadians(1.8f);

            // Release lasers.
            if (attackTimer % burstShootRate == burstShootRate - 1f && attackTimer >= shootDelay && attackTimer < 400f)
            {
                Main.PlaySound(SoundID.Item28, npc.Center);
                float shootOffsetAngle = Main.rand.NextFloat(MathHelper.TwoPi);
                for (int i = 0; i < laserBurstCount; i++)
                {
                    for (int j = -1; j <= 1; j += 2)
                    {
                        Vector2 shootVelocity = (MathHelper.TwoPi * i / laserBurstCount + shootOffsetAngle).ToRotationVector2() * burstShootSpeed;
                        Vector2 laserSpawnPosition = npc.Center + shootVelocity.SafeNormalize(Vector2.UnitY).RotatedBy(MathHelper.PiOver2) * j * 8f;
                        int laser = Utilities.NewProjectileBetter(laserSpawnPosition, shootVelocity, ModContent.ProjectileType<SpiralEnergyLaser>(), 250, 0f);
                        if (Main.projectile.IndexInRange(laser))
                            Main.projectile[laser].localAI[1] = j * 0.5f;
                    }
                }
            }

            if (attackTimer >= 480f)
                SelectNewAttack(npc);

        }

        public static void DoBehavior_DarkEnergyBulletHell(NPC npc, Player target, ref float attackTimer)
        {
            int burstFireRate = 17;
            int circleFireRate = 42;
            int energyPerBurst = 12;
            int energyPerCircle = 20;
            int shootTime = 780;
            float burstBaseSpeed = 8.4f;
            if (BossRushEvent.BossRushActive)
                burstBaseSpeed *= 1.3f + npc.Distance(target.Center) * 0.00174f;

            // Slow down and use more DR.
            npc.velocity *= 0.965f;
            npc.Calamity().DR = 0.7f;

            // Make a pulse sound before firing.
            if (attackTimer == 45f)
                Main.PlaySound(SoundID.DD2_WitherBeastAuraPulse, target.Center);

            if (attackTimer >= shootTime)
                SelectNewAttack(npc);

            // Don't fire near the start/end of the attack.
            if (attackTimer < 90f || attackTimer > shootTime - 90f)
                return;

            // Create bursts.
            if (attackTimer % burstFireRate == burstFireRate - 1f)
            {
                Main.PlaySound(SoundID.Item103, npc.Center);
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    float burstAngleOffset = Main.rand.NextFloat(MathHelper.TwoPi);
                    for (int i = 0; i < energyPerBurst; i++)
                    {
                        float burstInterpolant = i / (float)(energyPerBurst - 1f);
                        float burstAngle = burstAngleOffset + burstInterpolant * (i + i * i) / 2f + 32f * i;
                        Vector2 burstVelocity = burstAngle.ToRotationVector2() * burstBaseSpeed * Main.rand.NextFloat(0.7f, 1f);
                        Utilities.NewProjectileBetter(npc.Center, burstVelocity, ModContent.ProjectileType<DarkEnergyBulletHellProj>(), 260, 0f);
                    }
                }
            }

            // Create circles of energy.
            if (attackTimer % circleFireRate == circleFireRate - 1f)
            {
                Main.PlaySound(SoundID.Item103, npc.Center);
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    for (int i = 0; i < energyPerCircle; i++)
                    {
                        Vector2 burstVelocity = (MathHelper.TwoPi * i / energyPerCircle).ToRotationVector2() * burstBaseSpeed * 1.1f;
                        Utilities.NewProjectileBetter(npc.Center, burstVelocity, ModContent.ProjectileType<DarkEnergyBulletHellProj>(), 260, 0f);
                    }
                }
            }
        }
        public static void DoBehavior_BlackHoleSuck(NPC npc, Player target, ref float attackTimer)
        {
            npc.life = 1;
            npc.dontTakeDamage = true;
            npc.Calamity().ShouldCloseHPBar = true;

            int blackHoleDamage = 750;
            ref float moveTowardsTarget = ref npc.Infernum().ExtraAI[0];
            ref float hasCreatedBlackHole = ref npc.Infernum().ExtraAI[1];

            // Create the black hole on the first frame.
            if (hasCreatedBlackHole == 0f)
            {
                Utilities.DeleteAllProjectiles(true, ModContent.ProjectileType<ConvergingCelestialBarrage>(), ModContent.ProjectileType<SpiralEnergyLaser>(), ModContent.ProjectileType<CelestialBarrage>());

                if (Main.netMode != NetmodeID.MultiplayerClient)
                    Utilities.NewProjectileBetter(npc.Center, Vector2.Zero, ModContent.ProjectileType<AllConsumingBlackHole>(), blackHoleDamage, 0f);



                hasCreatedBlackHole = 1f;
            }

            // Disable damage.
            npc.dontTakeDamage = true;

            // Redirect quickly towards the target if necessary.
            if (moveTowardsTarget == 1f)
            {
                if (npc.WithinRange(target.Center, 480f))
                {
                    npc.velocity *= 0.8f;
                    npc.damage = 0;
                    if (npc.velocity.Length() < 4f)
                    {
                        npc.velocity = Vector2.Zero;
                        moveTowardsTarget = 0f;
                        npc.netUpdate = true;
                    }
                    return;
                }

				CalamityGlobalNPC.SmoothMovement(npc, 0f, target.Center - Vector2.UnitY * 360f - npc.Center, 40f);
                return;
            }

            // Make Ceaseless Void move quickly towards the target if they go too far away.
            if (!npc.WithinRange(target.Center, 1320f))
            {
                moveTowardsTarget = 1f;
                npc.netUpdate = true;
                return;
            }

            // Slow down.
            npc.velocity *= 0.9f;

            if (attackTimer >= 560f)
            {
				Main.PlaySound(SoundID.DD2_WitherBeastAuraPulse, target.Center);
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    int explosion = Utilities.NewProjectileBetter(npc.Center, Vector2.Zero, ModContent.ProjectileType<CosmicExplosion>(), 0, 0f);
                    if (Main.projectile.IndexInRange(explosion))
                        Main.projectile[explosion].ModProjectile<CosmicExplosion>().MaxRadius = 1250f;
                }

                npc.life = 1;
                npc.StrikeNPCNoInteraction(9999, 0f, 0, true);
                npc.HitEffect();
                npc.NPCLoot();
            }
        }
        public static void SelectNewAttack(NPC npc)
        {
            // Select a new target.
            npc.TargetClosest();
            
            List<CeaselessVoidAttackType> possibleAttacks = new List<CeaselessVoidAttackType>()
            {
                CeaselessVoidAttackType.RealityRendCharge,
                CeaselessVoidAttackType.ConvergingEnergyBarrages,
                CeaselessVoidAttackType.SlowEnergySpirals
            };
            
            if (possibleAttacks.Count >= 2)
                possibleAttacks.Remove((CeaselessVoidAttackType)(int)npc.ai[0]);

            for (int i = 0; i < 5; i++)
                npc.Infernum().ExtraAI[i] = 0f;

            npc.ai[0] = (int)Main.rand.Next(possibleAttacks);
            npc.ai[1] = 0f;
            npc.netUpdate = true;
        }
        #endregion AI

        #region Drawing
        public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Color lightColor)
        {
            Texture2D texture = Main.npcTexture[npc.type];
            Texture2D glowmask = ModContent.GetTexture("CalamityMod/NPCs/CeaselessVoid/CeaselessVoidGlow");
            Texture2D voidTexture = ModContent.GetTexture("InfernumMode/BehaviorOverrides/BossAIs/CeaselessVoid/CeaselessVoidVoidStuff");
            spriteBatch.Draw(texture, npc.Center - Main.screenPosition, npc.frame, npc.GetAlpha(lightColor), npc.rotation, npc.frame.Size() * 0.5f, npc.scale, 0, 0f);
            spriteBatch.Draw(glowmask, npc.Center - Main.screenPosition, npc.frame, npc.GetAlpha(Color.White), npc.rotation, npc.frame.Size() * 0.5f, npc.scale, 0, 0f);

            Main.spriteBatch.EnterShaderRegion();

            Main.spriteBatch.Draw(voidTexture, (npc.Center - Main.screenPosition), npc.frame, npc.GetAlpha(Color.White), npc.rotation, npc.frame.Size() * 0.5f, npc.scale, 0, 0);
            GameShaders.Misc["Infernum:RealityTear2"].SetShaderTexture(ModContent.GetTexture("InfernumMode/ExtraTextures/Stars"));
            //GameShaders.Misc["Infernum:RealityTear2"].Apply(drawData);
            //drawData.Draw(Main.spriteBatch);
            Main.spriteBatch.ExitShaderRegion();
            return false;
        }
        #endregion Drawing
    }
}
