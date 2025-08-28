using CalamityMod;
using CalamityMod.DataStructures;
using CalamityMod.NPCs;
using CalamityMod.Sounds;
using CalamityMod.Particles;
using InfernumMode.Particles;
using InfernumMode;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

using SCalNPC = CalamityMod.NPCs.SupremeCalamitas.SupremeCalamitas;

namespace InfernumMode.BehaviorOverrides.BossAIs.SupremeCalamitas
{
    public class ShadowDemon : ModNPC
    {
        public enum HeadAimState
        {
            LookAtTarget,
            LookInDirectionOfMovement,
            LookUpward
        }

        public class DemonHead
        {
            public Vector2 Center;

            public Vector2 Velocity;

            public Vector2[] OldVelocities = new Vector2[20];

            public int Frame;

            public float HoverOffset;

            public float HoverOffsetAngle;

            public float Rotation;

            public void AdjustOldVelocityArray()
            {
                for (int i = OldVelocities.Length - 1; i > 0; i--)
                    OldVelocities[i] = OldVelocities[i - 1];

                OldVelocities[0] = Velocity;
            }
        }

        public DemonHead[] Heads = new DemonHead[3];

        public HeadAimState AimState
        {
            get => (HeadAimState)npc.localAI[0];
            set => npc.localAI[0] = (int)value;
        }

        public Player Target => Main.player[npc.target];

        public float CongregationDiameter => npc.scale * 250f;

        public ref float GeneralTimer => ref npc.ai[0];

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Shadow Hydra");
        }

        public override void SetDefaults()
        {
            npc.npcSlots = 25f;
            npc.aiStyle = aiType = -1;
            npc.damage = 335;
            npc.width = npc.height = 60;
            npc.lifeMax = 20000;
            npc.knockBackResist = 0f;
            npc.noGravity = true;
            npc.noTileCollide = true;
            npc.dontTakeDamage = true;
        }

        public override void ScaleExpertStats(int numPlayers, float bossLifeScale) => npc.life = 20000;

        public override void AI()
        {
            // Disappear if SCal is not present.
            if (!NPC.AnyNPCs(ModContent.NPCType<SCalNPC>()))
            {
                npc.active = false;
                return;
            }

            // Inherit things from SCal.
            NPC scal = Main.npc[CalamityGlobalNPC.SCal];
            npc.target = scal.target;

            // Reset things.
            npc.damage = 0;

            GeneralTimer++;
            if (GeneralTimer % 160f == 2f)
                RedetermineHeadOffsets();

            // Determine the scale and hitbox size.
            float oldScale = npc.scale;
            npc.scale = MathHelper.Clamp(npc.scale + SupremeCalamitasBehaviorOverride.ShadowDemonCanAttack.ToDirectionInt() * 0.02f, 0.0001f, 1f);
            if (oldScale != npc.scale)
            {
                npc.Center += npc.Size * 0.5f;
                npc.Size = Vector2.One * CongregationDiameter * 0.85f;
                npc.Center -= npc.Size * 0.5f;
            }

            switch ((SupremeCalamitasBehaviorOverride.SCalAttackType)scal.ai[0])
            {
                case SupremeCalamitasBehaviorOverride.SCalAttackType.ShadowDemon_ReleaseExplodingShadowBlasts:
                    DoBehavior_ReleaseExplodingShadowBlasts(scal, ref scal.ai[1]);
                    break;
                case SupremeCalamitasBehaviorOverride.SCalAttackType.ShadowDemon_ShadowGigablastsAndCharges:
                    DoBehavior_ShadowGigablastsAndCharges(scal, ref scal.ai[1]);
                    break;
                default:
                    AimState = HeadAimState.LookAtTarget;
                    npc.SimpleFlyMovement(npc.SafeDirectionTo(Target.Center) * 11f, 0.2f);
                    break;
            }

            // Update heads.
            UpdateHeads();

            // Emit gas.
            EmitShadowGas();
        }

        public void DoBehavior_ReleaseExplodingShadowBlasts(NPC scal, ref float attackTimer)
        {
            int shootCount = 4;
            int blastShootDelay = 8;
            int chargeDelay = 20;
            int chargeTime = 40;
            float chargeSpeed = 32f;
            float blastShootSpeed = 23f;
            ref float shootCounter = ref scal.Infernum().ExtraAI[0];
            ref float attackSubstate = ref scal.Infernum().ExtraAI[1];

            // Hover in place near the target before slowing down.
            if (attackSubstate == 0f)
            {
                Vector2 hoverDestination = Target.Center + new Vector2((Target.Center.X < npc.Center.X).ToDirectionInt() * 500f, -275f);
                if (attackTimer >= 60f)
                    npc.velocity *= 0.98f;
                else
                    npc.SimpleFlyMovement(npc.SafeDirectionTo(hoverDestination) * 11f, 0.2f);

                int transitionDelay = shootCounter == 0f ? 150 : 85;
                if (attackTimer >= transitionDelay)
                {
                    attackSubstate = 1f;
                    attackTimer = 0f;
                    scal.netUpdate = true;
                }
                AimState = HeadAimState.LookAtTarget;
                return;
            }

            // Shoot shadow blasts for each of the heads before stopping in place for a bit.
            if (attackSubstate == 1f)
            {
                // Shoot the shadow blasts.
                if (attackTimer == blastShootDelay)
                {
                    foreach (var head in Heads)
                    {
                	Main.PlaySound(InfernumMode.CalamityMod.GetLegacySoundSlot(SoundType.Item, "Sounds/Item/PlasmaBlast"), head.Center);
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            for (int i = 0; i < 3; i++)
                            {
                                float shootOffsetAngle = MathHelper.Lerp(-0.44f, 0.44f, i / 2f) + Main.rand.NextFloatDirection() * 0.04f;
                                Vector2 shadowBlastShootVelocity = (Target.Center - head.Center).SafeNormalize(Vector2.UnitY).RotatedBy(shootOffsetAngle) * blastShootSpeed;
                                Utilities.NewProjectileBetter(head.Center, shadowBlastShootVelocity, ModContent.ProjectileType<ShadowFlameBlast>(), 550, 0f);
                            }
                        }
                    }
                }

                AimState = attackTimer >= blastShootDelay ? HeadAimState.LookUpward : HeadAimState.LookAtTarget;

                if (attackTimer >= chargeDelay)
                {
                    Main.PlaySound(SoundID.Zombie, npc.Center, 93);
                    attackSubstate = 2f;
                    attackTimer = 0f;
                    scal.netUpdate = true;

                    npc.velocity = npc.SafeDirectionTo(Target.Center) * chargeSpeed;
                    npc.netUpdate = true;
                }
                return;
            }

            // Charge.
            if (attackSubstate == 2f)
            {
                npc.velocity *= 0.984f;
                AimState = HeadAimState.LookInDirectionOfMovement;
                if (attackTimer >= chargeTime)
                {
                    attackSubstate = 0f;
                    attackTimer = 0f;
                    scal.netUpdate = true;
                    shootCounter++;
                    if (shootCounter >= shootCount)
                        SupremeCalamitasBehaviorOverride.SelectNextAttack(scal);
                }
            }
        }

        public void DoBehavior_ShadowGigablastsAndCharges(NPC scal, ref float attackTimer)
        {
            int hoverTime = 35;
            int chargeTime = 56;
            int chargeCount = 5;
            int boltReleaseRate = 8;
            float chargeSpeed = 36f;
            ref float chargeCounter = ref scal.Infernum().ExtraAI[0];
            ref float attackSubstate = ref scal.Infernum().ExtraAI[1];

            if (chargeCounter == 0f)
                hoverTime += 180;

            // Briefly hover into position.
            if (attackSubstate == 0f)
            {
                Vector2 hoverDestination = Target.Center + new Vector2((Target.Center.X < npc.Center.X).ToDirectionInt() * 500f, -120f);
                Vector2 idealVelocity = Vector2.Zero.MoveTowards(hoverDestination - npc.Center, 30f);
                npc.SimpleFlyMovement(idealVelocity, 0.4f);
                npc.velocity = Vector2.Lerp(npc.velocity, idealVelocity, 0.12f);

                // Charge and release a shadow gigablast if close to the destination or enough time has passed.
                if (attackTimer >= hoverTime || (npc.WithinRange(hoverDestination, 75f) && chargeCounter >= 1f))
                {
                    attackTimer = 0f;
                    attackSubstate = 1f;
                    npc.velocity = npc.SafeDirectionTo(Target.Center) * chargeSpeed;

                    // Shoot the gigablast.
                    Main.PlaySound(InfernumMode.CalamityMod.GetLegacySoundSlot(SoundType.Custom, "Sounds/Custom/SCalSounds/BrimstoneBigShoot"), npc.Center);
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        Vector2 gigablastShootVelocity = (Target.Center - Heads[1].Center).SafeNormalize(Vector2.UnitY) * 12.5f;
                        Utilities.NewProjectileBetter(Heads[1].Center, gigablastShootVelocity, ModContent.ProjectileType<ShadowGigablast>(), 550, 0f);
                    }

                    npc.netUpdate = true;
                    scal.netUpdate = true;
                }
                return;
            }

            // Charge and release shadow bolts.
            if (attackSubstate == 1f)
            {
                // Deal contact damage. This only applies to the body.
                npc.damage = npc.defDamage;

                int headToShoot = Main.rand.Next(Heads.Length);
                if (!Heads[headToShoot].Center.WithinRange(Target.Center, 300f) && attackTimer % boltReleaseRate == boltReleaseRate - 1f)
                {
					Main.PlaySound(InfernumMode.CalamityMod.GetLegacySoundSlot(SoundType.Custom, "Sounds/Custom/SCalSounds/BrimstoneShoot"), Heads[headToShoot].Center);
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        Vector2 boltVelocity = (Target.Center - Heads[headToShoot].Center).SafeNormalize(Vector2.UnitY) * 14.5f;
                        Utilities.NewProjectileBetter(Heads[headToShoot].Center, boltVelocity, ModContent.ProjectileType<ShadowBolt>(), 500, 0f);
                    }
                }

                if (attackTimer >= chargeTime)
                {
                    attackTimer = 0f;
                    attackSubstate = 0f;
                    chargeCounter++;
                    if (chargeCounter >= chargeCount)
                        SupremeCalamitasBehaviorOverride.SelectNextAttack(scal);

                    npc.netUpdate = true;
                    scal.netUpdate = true;
                }
            }
        }

        public void RedetermineHeadOffsets()
        {
            for (int i = 0; i < Heads.Length; i++)
            {
                Heads[i].HoverOffset = Main.rand.NextFloat(180f, 300f);
                Heads[i].HoverOffsetAngle = MathHelper.Lerp(-0.87f, 0.87f, i / (float)(Heads.Length - 1f));
                Heads[i].HoverOffsetAngle += Main.rand.NextFloatDirection() * 0.17f;
            }
        }

        public void UpdateHeads()
        {
            for (int i = 0; i < Heads.Length; i++)
            {
                if (Heads[i] is null)
                {
                    Heads[i] = new DemonHead()
                    {
                        Center = npc.Center
                    };
                }

                Vector2 hoverDestination = npc.Center - Vector2.UnitY.RotatedBy(Heads[i].HoverOffsetAngle) * Heads[i].HoverOffset * npc.scale;
                Vector2 idealHeadVelocity = Vector2.Zero.MoveTowards(hoverDestination - Heads[i].Center, npc.velocity.Length() + 20f);
                Heads[i].Velocity = Vector2.Lerp(Heads[i].Velocity, idealHeadVelocity, 0.085f).MoveTowards(idealHeadVelocity, 0.8f);
                Heads[i].Center += Heads[i].Velocity;

                float idealRotation = (Target.Center - Heads[i].Center).ToRotation();
                if (AimState == HeadAimState.LookUpward)
                    idealRotation = -MathHelper.PiOver2 * 0.95f;
                if (AimState == HeadAimState.LookInDirectionOfMovement)
                    idealRotation = npc.velocity.ToRotation();

                Heads[i].Rotation = Heads[i].Rotation.AngleLerp(idealRotation, 0.04f).AngleTowards(idealRotation, 0.04f);
                Heads[i].Frame = (int)(GeneralTimer / 6f + i * 4) % 6;
                Heads[i].AdjustOldVelocityArray();
            }
        }

        public void EmitShadowGas()
        {
            float particleSize = CongregationDiameter;
            if (npc.oldPosition != npc.position && GeneralTimer >= 3f)
                particleSize += (npc.oldPosition - npc.position).Length() * 4.2f;

            // Place a hard limit on particle sizes.
            if (particleSize > 500f)
                particleSize = 500f;
			var particleSet = InfernumFusableParticleManager.GetParticleSetByType<ShadowDemonParticleSet>();
			if (particleSet == null)
				return;

            int particleSpawnCount = Main.rand.NextBool(8) ? 3 : 1;
            for (int i = 0; i < particleSpawnCount; i++)
            {
                // Summon a base particle.
                Vector2 spawnPosition = npc.Center + Main.rand.NextVector2Circular(1f, 1f) * particleSize / 26f;
				particleSet.SpawnParticle(spawnPosition, particleSize);

                // And an "ahead" particle that spawns based on current movement.
                // This causes the "head" of the overall thing to have bumps when moving.
                spawnPosition += npc.velocity.RotatedByRandom(1.38f) * particleSize / 105f;
				particleSet.SpawnParticle(spawnPosition, particleSize * 0.4f);
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color drawColor)
        {
            float headScale = npc.scale * 1.6f;
            for (int i = 0; i < Heads.Length; i++)
            {
                int maxFrame = 6;
                Texture2D texture = ModContent.GetTexture("CalamityMod/Projectiles/Magic/SpiritCongregation");
                Texture2D backTexture = ModContent.GetTexture("CalamityMod/Projectiles/Magic/SpiritCongregationBack");
                Texture2D auraTexture = ModContent.GetTexture("CalamityMod/Projectiles/Magic/SpiritCongregationAura");

                float offsetFactor = npc.scale * ((CongregationDiameter - 54f) / 90f + 1.5f);
                offsetFactor *= texture.Width / 90f;
                Vector2 teethOffset = Heads[i].Rotation.ToRotationVector2() * offsetFactor * 4f;
                Vector2 drawPosition = Heads[i].Center - Main.screenPosition;
                Rectangle frame = texture.Frame(1, maxFrame, 0, Heads[i].Frame);
                Vector2 origin = frame.Size() * 0.5f;
                SpriteEffects direction = Math.Cos(Heads[i].Rotation) > 0f ? SpriteEffects.None : SpriteEffects.FlipVertically;

                // Draw the neck.
                Texture2D neckTexture = ModContent.GetTexture("CalamityMod/ExtraTextures/PhotovisceratorLight");

                Vector2 start = drawPosition;
                Vector2 end = npc.Center - Main.screenPosition;
                List<Vector2> controlPoints = new List<Vector2>()
                {
                    start
                };
                for (int j = 0; j < Heads[i].OldVelocities.Length; j++)
                {
                    // Incorporate the past movement into neck turns, giving it rubber band-like movment.
                    // Become less responsive at the neck ends. Having the ends have typical movement can look strange sometimes.
                    float swayResponsiveness = Utils.InverseLerp(0f, 6f, j, true) * Utils.InverseLerp(Heads[i].OldVelocities.Length, Heads[i].OldVelocities.Length - 6f, j, true);
                    swayResponsiveness *= 2.5f;
                    Vector2 swayTotalOffset = Heads[i].OldVelocities[j] * swayResponsiveness;
                    controlPoints.Add(Vector2.Lerp(start, end, j / (float)Heads[i].OldVelocities.Length) + swayTotalOffset);
                }
                controlPoints.Add(end);

                int chainPointCount = (int)(Vector2.Distance(controlPoints.First(), controlPoints.Last()) / 12f);
                if (chainPointCount < 12)
                    chainPointCount = 12;
                BezierCurve bezierCurve = new BezierCurve(controlPoints.ToArray());
                List<Vector2> chainPoints = bezierCurve.GetPoints(chainPointCount);

                for (int j = 0; j < chainPoints.Count; j++)
                {
                    Vector2 positionAtPoint = chainPoints[j];
                    if (Vector2.Distance(positionAtPoint, end) < 10f)
                        continue;

                    DrawData neckDraw = new DrawData(neckTexture, positionAtPoint, null, Color.White, 0f, neckTexture.Size() / 2f, npc.scale * 1.6f, 0, 0);
                    InfernumFusableParticleManager.GetParticleSetByType<ShadowDemonParticleSet>()?.PrepareSpecialDrawingForNextFrame(neckDraw);
                }

                // Draw the head.
                DrawData backTextureDraw = new DrawData(backTexture, drawPosition + npc.position - npc.oldPosition, frame, Color.White, Heads[i].Rotation, origin, headScale, direction, 0);
                DrawData auraTextureDraw = new DrawData(auraTexture, drawPosition + npc.position - npc.oldPosition, frame, Color.White, Heads[i].Rotation, origin, headScale, direction, 0);
                InfernumFusableParticleManager.GetParticleSetByType<ShadowDemonParticleSet>()?.PrepareSpecialDrawingForNextFrame(backTextureDraw, auraTextureDraw);
                Main.spriteBatch.Draw(texture, drawPosition + teethOffset, frame, Color.White, Heads[i].Rotation, origin, headScale, direction, 0);
            }
            return false;
        }

        public override bool CheckActive() => false;
    }
}
