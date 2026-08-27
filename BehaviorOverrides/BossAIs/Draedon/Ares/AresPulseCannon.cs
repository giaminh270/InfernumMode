using CalamityMod;
using CalamityMod.Items.Weapons.DraedonsArsenal;
using CalamityMod.NPCs;
using CalamityMod.Particles;
using InfernumMode.Sounds;
using InfernumMode.BehaviorOverrides.BossAIs.Draedon.ComboAttacks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Utilities;
using System;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using static InfernumMode.BehaviorOverrides.BossAIs.Draedon.Ares.AresBodyBehaviorOverride;
using CalamityModClass = CalamityMod.CalamityMod;

namespace InfernumMode.BehaviorOverrides.BossAIs.Draedon.Ares
{
    public class AresPulseCannon : ModNPC
    {
        public AresCannonChargeParticleSet EnergyDrawer = new AresCannonChargeParticleSet(-1, 15, 40f, Color.Fuchsia);

        public ThanatosSmokeParticleSet SmokeDrawer = new ThanatosSmokeParticleSet(-1, 3, 0f, 16f, 1.5f);

        public static NPC Ares => AresCannonBehaviorOverride.Ares;

        public static int TotalPulseBlastsPerBurst
        {
            get
            {
                int totalPulseBlastsPerBurst = 4;

                if (ExoMechManagement.CurrentAresPhase >= 5)
                    totalPulseBlastsPerBurst += 2;
                if (ExoMechManagement.CurrentAresPhase >= 6)
                    totalPulseBlastsPerBurst++;

                return totalPulseBlastsPerBurst;
            }
        }

        public static float AimPredictiveness =>
            ExoMechManagement.CurrentAresPhase >= 5 ? 32.5f : 27f;

        public Vector2 CoreSpritePosition => npc.Center + npc.spriteDirection * npc.rotation.ToRotationVector2() * 35f + (npc.rotation + MathHelper.PiOver2).ToRotationVector2() * 5f;

        // This stores the sound slot of the telegraph sound it makes, so it may be properly updated in terms of position.
        public SlotId TelegraphSoundSlot;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("XF-09 Ares Pulse Cannon");
            Main.npcFrameCount[npc.type] = 12;
            NPCID.Sets.TrailingMode[npc.type] = 3;
            NPCID.Sets.TrailCacheLength[npc.type] = npc.oldPos.Length;
        }

        public override void SetDefaults()
        {
            npc.npcSlots = 5f;
            npc.damage = 0;
            npc.width = 170;
            npc.height = 120;
            npc.defense = 80;
            npc.DR_NERD(0.35f);
            npc.LifeMaxNERB(1250000, 1495000, 500000);
            double HPBoost = CalamityConfig.Instance.BossHealthBoost * 0.01;
            npc.lifeMax += (int)(npc.lifeMax * HPBoost);
            npc.aiStyle = -1;
            aiType = -1;
            npc.Opacity = 0f;
            npc.knockBackResist = 0f;
            npc.canGhostHeal = false;
            npc.noGravity = true;
            npc.noTileCollide = true;
            npc.HitSound = SoundID.NPCHit4;
            npc.DeathSound = SoundID.NPCDeath14;
            npc.netAlways = true;
            npc.hide = true;
            music = (InfernumMode.CalamityMod as CalamityModClass).GetMusicFromMusicMod("ExoMechs") ?? MusicID.Boss3;
        }

        public override void AI()
        {
            // Die if Ares is not present.
            if (CalamityGlobalNPC.draedonExoMechPrime == -1)
            {
                npc.life = 0;
                npc.active = false;
                return;
            }

            // Update the energy drawers.
            EnergyDrawer.Update();
            SmokeDrawer.Update();

            // Ensure this does not take damage in the desperation attack.
            npc.dontTakeDamage = false;
            if (Ares.ai[0] == (int)AresBodyAttackType.PrecisionBlasts)
                npc.dontTakeDamage = true;

            // Inherit a bunch of attributes such as opacity from the body.
            ExoMechAIUtilities.HaveArmsInheritAresBodyAttributes(npc);

            bool performingDeathAnimation = ExoMechAIUtilities.PerformingDeathAnimation(npc);
            Player target = Main.player[npc.target];

            // Define attack variables.
            int shootTime = 180;
            int shootRate = shootTime / TotalPulseBlastsPerBurst;
            bool currentlyDisabled = ArmIsDisabled(npc);
            ref float attackTimer = ref npc.ai[0];
            ref float chargeDelay = ref npc.ai[1];
            ref float currentDirection = ref npc.ai[3];
            ref float shouldPrepareToFire = ref npc.Infernum().ExtraAI[1];
            ref float telegraphSound = ref npc.Infernum().ExtraAI[2];

            // Initialize delays and other timers.
            shouldPrepareToFire = 0f;
            if (chargeDelay == 0f)
                chargeDelay = Phase1ArmChargeupTime;

            // Don't do anything if this arm should be disabled.
            if (currentlyDisabled)
                attackTimer = 1f;

            // Inherit the attack timer from Ares if he's performing the ultimate attack.
            bool doingUltimateAttack = Ares.ai[0] == (int)AresBodyAttackType.PrecisionBlasts && Ares.Infernum().ExtraAI[9] >= 1f;
            if (doingUltimateAttack)
            {
                chargeDelay = (int)Ares.Infernum().ExtraAI[2];
                attackTimer = Ares.Infernum().ExtraAI[4];
                shootRate = 1;
                shootTime = 1;
            }

            // Hover near Ares.
            bool performingCharge = false;
            Vector2 hoverOffset = PerformHoverMovement(npc, performingCharge);

            // Update the telegraph outline intensity timer.
            npc.Infernum().ExtraAI[0] = MathHelper.Clamp(npc.Infernum().ExtraAI[0] + performingCharge.ToDirectionInt(), 0f, 15f);

            // Check to see if Ares is in the middle of a death animation. If he is, participate in the death animation.
            if (performingDeathAnimation)
            {
                HaveArmPerformDeathAnimation(npc, hoverOffset);
                return;
            }

            // Check to see if this arm should be used for special things in a combo attack.
            if (AresCannonBehaviorOverride.IsInUseByComboAttack(npc))
                return;

            // Calculate the direction and rotation this arm should use.
            Vector2 predictivenessFactor = Vector2.One * AimPredictiveness;
            if (doingUltimateAttack)
            {
                predictivenessFactor.X *= 0.6f;
                predictivenessFactor.Y *= 0.33f;
            }

            Vector2 aimDirection = npc.SafeDirectionTo(target.Center + target.velocity * predictivenessFactor);
            Vector2 endOfCannon = AresCannonBehaviorOverride.GetEndOfCannon(npc, target, aimDirection, currentlyDisabled, performingCharge, ref currentDirection);


            // Create a dust telegraph before firing.
            if (attackTimer > chargeDelay * 0.7f && attackTimer < chargeDelay)
            {
                for (int i = 0; i < 4; i++)
                {
                    Vector2 offset = Main.rand.NextVector2Circular(30f, 30f);
                    Dust.NewDustPerfect(endOfCannon + offset, 234, Main.rand.NextVector2Circular(5f, 5f), 0, default, 1.35f).noGravity = true;
                    Dust.NewDustPerfect(endOfCannon - offset, 234, Main.rand.NextVector2Circular(5f, 5f), 0, default, 1.35f).noGravity = true;
                }
            }

            // Decide the state of the particle drawers.
            if (AresCannonBehaviorOverride.UpdateParticleDrawers(SmokeDrawer, EnergyDrawer, attackTimer, chargeDelay))
                shouldPrepareToFire = 1f;

            // Fire lasers.
            if (attackTimer >= chargeDelay && attackTimer % shootRate == shootRate - 1f)
            {
                Main.PlaySound(InfernumMode.CalamityMod.GetLegacySoundSlot(SoundType.Item, "Sounds/Item/PulseRifleFire"), npc.Center);

                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    if (!doingUltimateAttack)
                    {
                        int blastDamage = ProjectileDamageBoost + DraedonBehaviorOverride.StrongerNormalShotDamage;
                        Vector2 blastShootVelocity = aimDirection * 7.5f;
                        Vector2 blastSpawnPosition = endOfCannon + blastShootVelocity * 8.4f;
                        Utilities.NewProjectileBetter(blastSpawnPosition, blastShootVelocity, ModContent.ProjectileType<AresPulseBlast>(), blastDamage, 0f);
                    }
                    else
                        Utilities.NewProjectileBetter(endOfCannon, aimDirection, ModContent.ProjectileType<AresPrecisionBlast>(), DraedonBehaviorOverride.PowerfulShotDamage, 0f, -1, npc.whoAmI);

                    npc.netUpdate = true;
                }
            }

            // Reset the attack and laser counter after an attack cycle ends.
            if (attackTimer >= chargeDelay + shootTime)
            {
                attackTimer = 0f;
                npc.netUpdate = true;
            }
            attackTimer++;
        }

        public static Vector2 PerformHoverMovement(NPC npc, bool performingCharge)
        {
            float backArmDirection = (Ares.Infernum().ExtraAI[ExoMechManagement.Ares_BackArmsAreSwappedIndex] != 1f).ToDirectionInt();
            Vector2 hoverOffset = new Vector2(backArmDirection * 575f, 0f);
            if (performingCharge)
                hoverOffset = new Vector2(backArmDirection * 380f, 150f);

            Vector2 hoverDestination = Ares.Center + hoverOffset * Ares.scale;
            ExoMechAIUtilities.DoSnapHoverMovement(npc, hoverDestination, 64f, 115f);

            return hoverOffset;
        }

        public override void DrawBehind(int index)
        {
            Main.instance.DrawCacheNPCProjectiles.Add(index);
        }

        public override void FindFrame(int frameHeight)
        {
            int currentFrame = (int)Math.Round(MathHelper.Lerp(0f, 35f, npc.ai[0] / npc.ai[1]));

            if (npc.ai[0] > npc.ai[1])
            {
                npc.frameCounter++;
                if (npc.frameCounter >= 66f)
                    npc.frameCounter = 0D;
                currentFrame = (int)Math.Round(MathHelper.Lerp(36f, 47f, (float)npc.frameCounter / 66f));
            }
            else
                npc.frameCounter = 0D;

            if (ExoMechComboAttackContent.ArmCurrentlyBeingUsed(npc))
                currentFrame = (int)Math.Round(MathHelper.Lerp(0f, 35f, npc.ai[0] % 72f / 72f));

            npc.frame = new Rectangle(currentFrame / 12 * 150, currentFrame % 12 * 148, 150, 148);
        }

        public override void HitEffect(int hitDirection, double damage)
        {
            if (npc.soundDelay == 1)
            {
                npc.soundDelay = 3;
                Main.PlaySound(InfernumSoundRegistry.ExoHit, npc.Center);
            }

            for (int k = 0; k < 3; k++)
                Dust.NewDust(npc.position, npc.width, npc.height, DustID.TerraBlade, 0f, 0f, 100, new Color(0, 255, 255), 1f);

            if (npc.life <= 0)
            {
                for (int i = 0; i < 2; i++)
                    Dust.NewDust(npc.position, npc.width, npc.height, DustID.TerraBlade, 0f, 0f, 100, new Color(0, 255, 255), 1.5f);

                for (int i = 0; i < 20; i++)
                {
                    Dust exoEnergy = Dust.NewDustDirect(npc.position, npc.width, npc.height, DustID.TerraBlade, 0f, 0f, 0, new Color(0, 255, 255), 2.5f);
                    exoEnergy.noGravity = true;
                    exoEnergy.velocity *= 3f;

                    exoEnergy = Dust.NewDustDirect(npc.position, npc.width, npc.height, DustID.TerraBlade, 0f, 0f, 100, new Color(0, 255, 255), 1.5f);
                    exoEnergy.velocity *= 2f;
                    exoEnergy.noGravity = true;
                }

                if (Main.netMode != NetmodeID.Server)
                {
	                Gore.NewGore(npc.position, npc.velocity, mod.GetGoreSlot("AresPulseCannon1"), npc.scale);
	                Gore.NewGore(npc.position, npc.velocity, InfernumMode.CalamityMod.GetGoreSlot("AresHandBase1"), npc.scale);
	                Gore.NewGore(npc.position, npc.velocity, InfernumMode.CalamityMod.GetGoreSlot("AresHandBase2"), npc.scale);
	                Gore.NewGore(npc.position, npc.velocity, InfernumMode.CalamityMod.GetGoreSlot("AresHandBase3"), npc.scale);
                }
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            string glowmaskTexturePath = "InfernumMode/BehaviorOverrides/BossAIs/Draedon/Ares/AresPulseCannonGlow";
            AresCannonBehaviorOverride.DrawCannon(npc, glowmaskTexturePath, Color.Violet, lightColor, CoreSpritePosition, EnergyDrawer, SmokeDrawer);
            return false;
        }

		public override bool CheckDead() => ExoMechManagement.HandleDeathEffects(npc);
	
        public override bool CheckActive() => false;
    }
}
