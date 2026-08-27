using CalamityMod;
using InfernumMode.InverseKinematics;
using CalamityMod.NPCs;
using CalamityMod.Particles;
using InfernumMode.Sounds;
using InfernumMode.Graphics.Primitives;
using InfernumMode.GlobalInstances;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using static InfernumMode.BehaviorOverrides.BossAIs.Draedon.Ares.AresBodyBehaviorOverride;
using static InfernumMode.BehaviorOverrides.BossAIs.Draedon.DraedonBehaviorOverride;
using CalamityModClass = CalamityMod.CalamityMod;
using InfernumMode.Effects;

namespace InfernumMode.BehaviorOverrides.BossAIs.Draedon.Ares
{
    public class AresEnergyKatana : ModNPC
    {
        private bool katanaIsInUse;

        public LimbCollection Limbs = new LimbCollection(new ModifiedCyclicCoordinateDescentUpdateRule(0.27f, MathHelper.Pi * 0.75f), 140f, 154f);

        public AresCannonChargeParticleSet EnergyDrawer = new AresCannonChargeParticleSet(-1, 15, 40f, Color.Red);

        public ThanatosSmokeParticleSet SmokeDrawer = new ThanatosSmokeParticleSet(-1, 3, 0f, 16f, 1.5f);

        public Vector2 SlashStart
        {
            get;
            set;
        }

        public PrimitiveTrailCopy SlashDrawer
        {
            get;
            set;
        }

        public bool KatanaIsInUse
        {
            get => katanaIsInUse;
            set
            {
                if (value && !katanaIsInUse)
                    Main.PlaySound(InfernumSoundRegistry.ExoLaserShootSound, npc.Center);

                katanaIsInUse = value;
            }
        }

        public List<Vector2> SlashControlPoints
        {
            get
            {
                Vector2 slashStart = SlashStart;
                Vector2 aimDirection = ((float)Limbs.Limbs[1].Rotation).ToRotationVector2();
                Vector2 slashEnd = npc.Center + aimDirection * npc.scale * 160f;
                Vector2 slashMiddle1 = Vector2.Lerp(slashStart, slashEnd, 0.25f);
                Vector2 slashMiddle2 = Vector2.Lerp(slashStart, slashEnd, 0.5f);
                Vector2 slashMiddle3 = Vector2.Lerp(slashStart, slashEnd, 0.75f);

                return new List<Vector2>()
                {
                    slashEnd,
                    slashMiddle3 + aimDirection * 30f,
                    slashMiddle2,
                    slashMiddle1 - aimDirection * 30f,
                    slashStart,
                };
            }
        }

        public Rectangle ActualHitbox => Utils.CenteredRectangle(npc.Center, Vector2.One * npc.scale * 142f);

        public Player Target => Main.player[npc.target];

        public ref float ArmOffsetDirection => ref npc.ai[2];

        public ref float CurrentDirection => ref npc.ai[3];

        public ref float SlashTrailFadeOut => ref npc.localAI[0];

        public static int DownwardCrossSlicesAnticipationTime
        {
            get
            {
                if (ExoMechManagement.CurrentAresPhase >= 6)
                    return 50;

                if (ExoMechManagement.CurrentAresPhase >= 5)
                    return 70;

                return 84;
            }
        }

        public static int DownwardCrossSlicesSliceTime => 20;

        public static int DownwardCrossSlicesHoldInPlaceTime
        {
            get
            {
                if (ExoMechManagement.CurrentAresPhase >= 6)
                    return 21;

                if (ExoMechManagement.CurrentAresPhase >= 5)
                    return 26;

                return 32;
            }
        }

        public static int ThreeDimensionalSlicesAnticipationTime
        {
            get
            {
                if (ExoMechManagement.CurrentAresPhase >= 6)
                    return 50;

                return 72;
            }
        }

        public static int ThreeDimensionalSlicesSliceTime => 20;

        public static NPC Ares => AresCannonBehaviorOverride.Ares;

        public static float AttackTimer => Ares.ai[1];

        public static Vector2 ActiveHitboxSize => new Vector2(450f);

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("XF-09 Ares Energy Katana");
            NPCID.Sets.TrailingMode[npc.type] = 3;
            NPCID.Sets.TrailCacheLength[npc.type] = 50;
        }

        public override void SetDefaults()
        {
            npc.npcSlots = 5f;
            npc.damage = AresEnergyKatanaContactDamage / 2;
            npc.Size = Vector2.One * 60f;
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
            npc.HitSound = null;
            npc.DeathSound = SoundID.NPCDeath14;
            npc.netAlways = true;
            npc.boss = true;
            npc.hide = true;
            npc.Calamity().canBreakPlayerDefense = true;
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

            // Update limbs.
            UpdateLimbs();

            // Close the HP bar.
            npc.boss = false;
            npc.Calamity().ShouldCloseHPBar = true;

            // Inherit a bunch of attributes such as opacity from the body.
            ExoMechAIUtilities.HaveArmsInheritAresBodyAttributes(npc);

            // Ensure this does not take damage in the desperation attack.
            npc.dontTakeDamage = false;
            if (Ares.ai[0] == (int)AresBodyAttackType.PrecisionBlasts)
                npc.dontTakeDamage = true;

            bool currentlyDisabled = ArmIsDisabled(npc);

            // Inherit a bunch of attributes such as opacity from the body.
            ExoMechAIUtilities.HaveArmsInheritAresBodyAttributes(npc);

            AresCannonBehaviorOverride.UpdateParticleDrawers(SmokeDrawer, EnergyDrawer, 0f, 100f);

            // Check to see if this arm should be used for special things in a combo attack.
            if (AresCannonBehaviorOverride.IsInUseByComboAttack(npc))
            {
                npc.Size = ActiveHitboxSize;
                return;
            }

            // Hover in place below Ares if disabled.
            if (currentlyDisabled)
            {
                PerformDisabledHoverMovement();
                return;
            }

            // Unlike projectiles, NPCs have no Colliding hook to use for general-purpose collision logic.
            // As such, a roundabout hack is required, where the hurt box is so large that it triggers for everything, but a CanHitPlayer check culls invalid hits.
            npc.Size = ActiveHitboxSize;

            switch ((AresBodyAttackType)Ares.ai[0])
            {
                case AresBodyAttackType.EnergyBladeSlices:
                    DoBehavior_EnergyBladeSlices();
                    break;
                case AresBodyAttackType.DownwardCrossSlices:
                    DoBehavior_DownwardCrossSlices();
                    break;
                case AresBodyAttackType.ThreeDimensionalSuperslashes:
                    DoBehavior_ThreeDimensionalSuperslashes();
                    break;
            }
        }

        public void UpdateLimbs()
        {
			Vector2 connectPosition = Ares.Center + new Vector2(ArmOffsetDirection * 70f, -108f).RotatedBy(Ares.rotation * -Ares.spriteDirection);
			Vector2 endPosition = npc.Center;

			for (int i = 0; i < 12; i++)
			{
				float lockedRotation;
				if (ArmOffsetDirection == 1f)
					lockedRotation = 0.23f;
				else
					lockedRotation = MathHelper.Pi - 0.23f;

				// Properly clamp the angular difference to avoid large jumps when the angle representation wraps around 0/2π.
				// This prevents the katana on one side from continuously "bending" or flipping by ~90-180 degrees.
				float currentRotation = (float)Limbs[0].Rotation;
				float angularDifference = MathHelper.WrapAngle(currentRotation - lockedRotation);
				Limbs[0].Rotation = lockedRotation + MathHelper.Clamp(angularDifference, -0.45f, 0.45f);

				Limbs.Update(connectPosition, endPosition);
			}
		}

        public void DoBehavior_EnergyBladeSlices()
        {
            int anticipationTime = 54;
            int sliceTime = 16;
            int hoverTime = 8;
            float slashShootSpeed = 4f;

            if (ExoMechManagement.CurrentAresPhase >= 3)
                slashShootSpeed += 0.5f;
            if (ExoMechManagement.CurrentAresPhase >= 5)
                anticipationTime -= 6;
            if (ExoMechManagement.CurrentAresPhase >= 6)
                anticipationTime -= 5;

            float wrappedAttackTimer = (AttackTimer + (int)ArmOffsetDirection * anticipationTime / 3) % (anticipationTime + sliceTime + hoverTime);
            float flySpeedBoost = Ares.velocity.Length() * 0.51f;

            // Anticipate the slash.
            if (wrappedAttackTimer <= anticipationTime)
            {
                SlashTrailFadeOut = 1f;
                float minHoverSpeed = Utilities.Remap(wrappedAttackTimer, 7f, anticipationTime * 0.5f, 2f, 42f);
                Vector2 startingOffset = new Vector2(ArmOffsetDirection * 470f, 0f);
                Vector2 endingOffset = new Vector2(ArmOffsetDirection * 172f, -175f);
                Vector2 hoverOffset = Vector2.Lerp(startingOffset, endingOffset, Utils.InverseLerp(0f, anticipationTime, wrappedAttackTimer, true));
                ExoMechAIUtilities.DoSnapHoverMovement(npc, Ares.Center + hoverOffset.SafeNormalize(Vector2.Zero) * 450f, flySpeedBoost + minHoverSpeed, 115f);
            }

            // Do the slash.
            else if (wrappedAttackTimer <= anticipationTime + sliceTime)
            {
                SlashTrailFadeOut = 0f;
                Vector2 startingOffset = new Vector2(ArmOffsetDirection * 172f, -175f);
                Vector2 endingOffset = new Vector2(ArmOffsetDirection * -260f, 400f);
                Vector2 hoverOffset = Vector2.Lerp(startingOffset, endingOffset, Utils.InverseLerp(anticipationTime, anticipationTime + sliceTime, wrappedAttackTimer, true));
                ExoMechAIUtilities.DoSnapHoverMovement(npc, Ares.Center + hoverOffset.SafeNormalize(Vector2.Zero) * 400f, flySpeedBoost + 49f, 115f);
            }

            // Drift for a short time after the slash.
            else
            {
                npc.velocity.X *= 0.6f;
                npc.velocity.Y *= 0.1f;
                SlashTrailFadeOut = MathHelper.Clamp(SlashTrailFadeOut + 0.5f, 0f, 1f);
            }

            // Prepare the slash.
            if (wrappedAttackTimer == anticipationTime)
            {
                // Reset the position cache, so that the trail can be drawn with a fresh set of points.
                npc.oldPos = new Vector2[npc.oldPos.Length];

                // Calculate the starting position of the slash. This is used for determining the orientation of the trail.
                SlashStart = npc.Center + ((float)Limbs.Limbs[1].Rotation).ToRotationVector2() * npc.scale * 160f;
                npc.netUpdate = true;

                // Play a slice sound.
                Main.PlaySound(InfernumSoundRegistry.AresSlashSound, npc.Center);
            }

            // Create an energy slash.
            if (wrappedAttackTimer == anticipationTime + sliceTime / 2 + 4)
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    Vector2 energySlashVelocity = Vector2.Lerp(((float)Limbs[1].Rotation).ToRotationVector2(), npc.SafeDirectionTo(Target.Center), 0.6f) * slashShootSpeed;

                    ProjectileSpawnManagementSystem.PrepareProjectileForSpawning(slash =>
                    {
                        slash.ModProjectile<AresEnergySlash>().ControlPoints = SlashControlPoints.ToArray();
                    });
                    Utilities.NewProjectileBetter(npc.Center, energySlashVelocity, ModContent.ProjectileType<AresEnergySlash>(), AresEnergySlashDamage, 0f);
                }
            }

            // Rotate based on the direction of the arm.
            npc.rotation = (float)Limbs[1].Rotation;
            npc.spriteDirection = (int)ArmOffsetDirection;
            if (ArmOffsetDirection == 1)
                npc.rotation += MathHelper.Pi;

            // Ensure that the katanas are drawn.
            KatanaIsInUse = true;
        }

        public void DoBehavior_DownwardCrossSlices()
        {
            int anticipationTime = DownwardCrossSlicesAnticipationTime;
            int sliceTime = DownwardCrossSlicesSliceTime;
            int holdInPlaceTime = DownwardCrossSlicesHoldInPlaceTime;
            float wrappedAttackTimer = AttackTimer % (anticipationTime + sliceTime + holdInPlaceTime);
            float flySpeedBoost = Ares.velocity.Length() * 1.1f;

            // Anticipate the slash.
            if (wrappedAttackTimer <= anticipationTime)
            {
                SlashTrailFadeOut = 1f;
                float minHoverSpeed = Utilities.Remap(wrappedAttackTimer, 7f, anticipationTime * 0.5f, 2f, 42f);
                Vector2 startingOffset = new Vector2(ArmOffsetDirection * 470f, 0f);
                Vector2 endingOffset = new Vector2(ArmOffsetDirection * 140f, -192f);
                Vector2 hoverOffset = Vector2.Lerp(startingOffset, endingOffset, Utils.InverseLerp(0f, anticipationTime, wrappedAttackTimer, true));
                ExoMechAIUtilities.DoSnapHoverMovement(npc, Ares.Center + hoverOffset.SafeNormalize(Vector2.Zero) * 450f, flySpeedBoost + minHoverSpeed, 115f);
            }

            // Do the slash.
            else if (wrappedAttackTimer <= anticipationTime + sliceTime)
            {
                SlashTrailFadeOut = 0f;
                Vector2 startingOffset = new Vector2(ArmOffsetDirection * 140f, -192f);
                Vector2 endingOffset = new Vector2(ArmOffsetDirection * -260f, 450f);
                Vector2 hoverOffset = Vector2.Lerp(startingOffset, endingOffset, Utils.InverseLerp(anticipationTime, anticipationTime + sliceTime, wrappedAttackTimer, true));
                ExoMechAIUtilities.DoSnapHoverMovement(npc, Ares.Center + hoverOffset.SafeNormalize(Vector2.Zero) * 400f, flySpeedBoost + 67f, 115f);
            }

            // Hold the blades in place.
            else
            {
                npc.velocity *= 0.27f;
                SlashTrailFadeOut = MathHelper.Clamp(SlashTrailFadeOut + 0.25f, 0f, 1f);
            }

            // Prepare the slash.
            if (wrappedAttackTimer == anticipationTime)
            {
                // Reset the position cache, so that the trail can be drawn with a fresh set of points.
                npc.oldPos = new Vector2[npc.oldPos.Length];
                Target.Infernum().CurrentScreenShakePower = 6f;
                ScreenEffectSystem.SetFlashEffect(Target.Center, 2f, 30);

                // Calculate the starting position of the slash. This is used for determining the orientation of the trail.
                SlashStart = npc.Center + ((float)Limbs.Limbs[1].Rotation).ToRotationVector2() * npc.scale * 160f;
                npc.netUpdate = true;

                // Play a slice sound.
                Main.PlaySound(InfernumSoundRegistry.AresSlashSound, npc.Center);
            }

            // Rotate based on the direction of the arm.
            if (wrappedAttackTimer <= anticipationTime + sliceTime)
            {
                npc.rotation = (float)Limbs[1].Rotation;
                npc.spriteDirection = (int)ArmOffsetDirection;
                if (ArmOffsetDirection == 1)
                    npc.rotation += MathHelper.Pi;
            }

            // Ensure that the katanas are drawn.
            KatanaIsInUse = true;
        }

        public void DoBehavior_ThreeDimensionalSuperslashes()
        {
            int anticipationTime = ThreeDimensionalSlicesAnticipationTime;
            int sliceTime = ThreeDimensionalSlicesSliceTime;
            float wrappedAttackTimer = AttackTimer % (anticipationTime + sliceTime);
            float flySpeedBoost = Vector2.Distance(Ares.position, Ares.oldPosition);

            // Anticipate the slash.
            if (wrappedAttackTimer <= anticipationTime)
            {
                SlashTrailFadeOut = 1f;
                float minHoverSpeed = Utilities.Remap(wrappedAttackTimer, 7f, anticipationTime * 0.5f, 9f, 66f);
                Vector2 startingOffset = new Vector2(ArmOffsetDirection * 470f, 0f);
                Vector2 endingOffset = new Vector2(ArmOffsetDirection * 172f, -175f);
                Vector2 hoverOffset = Vector2.Lerp(startingOffset, endingOffset, Utils.InverseLerp(0f, anticipationTime, wrappedAttackTimer, true));
                ExoMechAIUtilities.DoSnapHoverMovement(npc, Ares.Center + hoverOffset.SafeNormalize(Vector2.Zero) * Ares.scale * 880f, flySpeedBoost + minHoverSpeed, 115f);
            }

            // Do the slash.
            else if (wrappedAttackTimer <= anticipationTime + sliceTime)
            {
                SlashTrailFadeOut = 0f;
                Vector2 startingOffset = new Vector2(ArmOffsetDirection * 172f, -175f);
                Vector2 endingOffset = new Vector2(ArmOffsetDirection * -260f, 400f);
                Vector2 hoverOffset = Vector2.Lerp(startingOffset, endingOffset, Utils.InverseLerp(anticipationTime, anticipationTime + sliceTime, wrappedAttackTimer, true));
                ExoMechAIUtilities.DoSnapHoverMovement(npc, Ares.Center + hoverOffset.SafeNormalize(Vector2.Zero) * Ares.scale * 400f, flySpeedBoost + 67f, 115f);
            }

            // Prepare the slash.
            if (wrappedAttackTimer == anticipationTime)
            {
                // Reset the position cache, so that the trail can be drawn with a fresh set of points.
                npc.oldPos = new Vector2[npc.oldPos.Length];

                // Calculate the starting position of the slash. This is used for determining the orientation of the trail.
                SlashStart = npc.Center + ((float)Limbs.Limbs[1].Rotation).ToRotationVector2() * npc.scale * 160f;
                npc.netUpdate = true;

                // Play a slice sound.
                Main.PlaySound(InfernumSoundRegistry.AresSlashSound, npc.Center);
            }

            // Rotate based on the direction of the arm.
            npc.rotation = (float)Limbs[1].Rotation;
            npc.spriteDirection = (int)ArmOffsetDirection;
            if (ArmOffsetDirection == 1)
                npc.rotation += MathHelper.Pi;

            // Ensure that the katanas are drawn.
            KatanaIsInUse = true;
        }

        public Vector2 PerformDisabledHoverMovement()
        {
            // The katana should by default not be in use.
            KatanaIsInUse = false;

            // Reset the hit/hurtbox.
            npc.Size = Vector2.One * 60f;

            ExoMechAIUtilities.PerformAresArmDirectioning(npc, Ares, Target, Vector2.UnitY, true, false, ref CurrentDirection);

            Vector2 hoverOffset = new Vector2(ArmOffsetDirection * 470f, 0f);
            Vector2 hoverDestination = Ares.Center + hoverOffset * Ares.scale;
            ExoMechAIUtilities.DoSnapHoverMovement(npc, hoverDestination, 64f, 115f);

            return hoverOffset;
        }

        public override void DrawBehind(int index)
        {
            Main.instance.DrawCacheNPCProjectiles.Add(index);
        }

        public override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position) => false;

        public override bool? CanBeHitByProjectile(Projectile projectile)
        {
            if (projectile.Colliding(projectile.Hitbox, ActualHitbox))
                return null;
            return false;
        }

        public override bool CanHitPlayer(Player target, ref int cooldownSlot)
        {
            // Use the boss cooldown slot.
            cooldownSlot = 1;

            // Don't do damage if Ares is in the background.
            if (Ares.ai[2] >= 0.25f)
                return false;

            // If the player is colliding with the katana, they take damage.
            float _ = 0f;
            Vector2 katanaStart = npc.Center - npc.rotation.ToRotationVector2() * ArmOffsetDirection * npc.scale * 14f;
            Vector2 katanaEnd = npc.Center - npc.rotation.ToRotationVector2() * ArmOffsetDirection * npc.scale * 264f;
            bool playerIsCollidingWithKatana = Collision.CheckAABBvLineCollision(target.TopLeft, target.Hitbox.Size(), katanaStart, katanaEnd, npc.scale * 50f, ref _);
            if (KatanaIsInUse && playerIsCollidingWithKatana)
                return true;

            return false;
        }

        public override bool? CanHitNPC(NPC target) => false;

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

        public float SlashWidthFunction(float completionRatio) => npc.scale * 100f;

        public Color SlashColorFunction(float completionRatio) => Color.White * Utils.InverseLerp(0.9f, 0.4f, completionRatio, true) * (1f - SlashTrailFadeOut) * npc.Opacity * npc.scale;

        public void DrawSlash()
        {
            PrepareSlashShader();
            SlashDrawer.Draw(SlashControlPoints, -Main.screenPosition, 20, (float)Limbs.Limbs[1].Rotation + MathHelper.PiOver2);
        }

        public static void PrepareSlashShader()
        {
            var slashShader = InfernumEffectsRegistry.ExobladeSlash;
            slashShader.SetShaderTexture(ModContent.GetTexture("CalamityMod/ExtraTextures/VoronoiShapes"));
            slashShader.UseColor(new Color(237, 148, 54));
            slashShader.UseSecondaryColor(new Color(104, 24, 38));
            slashShader.Shader.Parameters["fireColor"].SetValue(Color.Wheat.ToVector3());
            slashShader.Shader.Parameters["flipped"].SetValue(false);
            slashShader.Apply();
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color drawColor)
        {
            // Draw the cannon.
            string glowmaskTexturePath = "InfernumMode/BehaviorOverrides/BossAIs/Draedon/Ares/AresEnergyKatanaGlow";
            AresCannonBehaviorOverride.DrawCannon(npc, glowmaskTexturePath, Color.Red, drawColor, npc.Center - Main.screenPosition, EnergyDrawer, SmokeDrawer);

            if (KatanaIsInUse)
            {
                // Prepare the slash drawer.
                var slashShader = InfernumEffectsRegistry.ExobladeSlash;
                if (SlashDrawer == null)
                    SlashDrawer = new PrimitiveTrailCopy(SlashWidthFunction, SlashColorFunction, null, true, slashShader);

                // Draw the zany slash effect.
                Main.spriteBatch.EnterShaderRegion();

                int slashLayers = (InfernumConfig.Instance != null && InfernumConfig.Instance.ReducedGraphicsConfig) ? 2 : 6;
                for (int i = 0; i < slashLayers; i++)
                    DrawSlash();
                Main.spriteBatch.ExitShaderRegion();

                // Draw the energy katana.
                int bladeFrameNumber = (int)((Main.GlobalTime * 16f + npc.whoAmI * 7.13f) % 9f);
                Texture2D bladeTexture = ModContent.GetTexture("CalamityMod/Projectiles/DraedonsArsenal/PhaseslayerBlade");
                Rectangle bladeFrame = bladeTexture.Frame(3, 7, bladeFrameNumber / 7, bladeFrameNumber % 7);
                Vector2 bladeOrigin = bladeFrame.Size() * new Vector2(0.5f, 1f);
                Vector2 bladeDrawPosition = npc.Center - Main.screenPosition - npc.rotation.ToRotationVector2() * ArmOffsetDirection * npc.scale * 14f;
                Vector2 bladeScale = Vector2.One * npc.scale;
                float squish = Vector2.Distance(npc.position, npc.oldPosition) * 0.006f;
                bladeScale.X -= squish;

                Main.spriteBatch.Draw(bladeTexture, bladeDrawPosition, bladeFrame, npc.GetAlpha(Color.White), npc.rotation - ArmOffsetDirection * MathHelper.PiOver2, bladeOrigin, bladeScale, 0, 0);
            }

            return false;
        }

        public override bool CheckActive() => false;
    }
}
