using CalamityMod;
using InfernumMode;
using CalamityMod.Buffs.StatDebuffs;
using CalamityMod.Items.Weapons.Ranged;
using CalamityMod.NPCs.SupremeCalamitas;
using CalamityMod.Projectiles.Boss;
using InfernumMode.OverridingSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

using SCalNPC = CalamityMod.NPCs.SupremeCalamitas.SupremeCalamitas;

namespace InfernumMode.BehaviorOverrides.BossAIs.SupremeCalamitas
{
    public class SepulcherHeadBehaviorOverride : NPCBehaviorOverride
    {
        public enum SepulcherAttackType
        {
            AttackDelay,
            ErraticCharges,
            PerpendicularBoneCharges,
            SoulBombBursts
        }

        public const int MinLength = 39;

        public const int MaxLength = MinLength + 1;

        public override int NPCOverrideType => ModContent.NPCType<SCalWormHead>();

        public override NPCOverrideContext ContentToOverride => NPCOverrideContext.NPCAI | NPCOverrideContext.NPCSetDefaults | NPCOverrideContext.NPCPreDraw;

        public override void SetDefaults(NPC npc)
        {
            npc.damage = 335;
            npc.npcSlots = 5f;
            npc.width = 62;
            npc.height = 64;
            npc.defense = 0;
            npc.lifeMax = 331550;
            npc.aiStyle = -1;
            npc.knockBackResist = 0f;
            npc.scale *= 1.2f;
            npc.alpha = 255;
            npc.chaseable = true;
            npc.behindTiles = true;
            npc.noGravity = true;
            npc.noTileCollide = true;
            npc.canGhostHeal = false;
            npc.netAlways = true;
			npc.DeathSound = InfernumMode.CalamityMod.GetLegacySoundSlot(SoundType.NPCKilled, "Sounds/NPCKilled/SepulcherDeath");
        }

        public override bool PreAI(NPC npc)
        {
            npc.Calamity().CanHaveBossHealthBar = true;

            npc.TargetClosestIfTargetIsInvalid();
            Player target = Main.player[npc.target];
            ref float attackState = ref npc.ai[1];
            ref float attackTimer = ref npc.ai[2];
            ref float hasSummonedSegments = ref npc.Infernum().ExtraAI[5];

            // Don't get stopped by weird freezing debuffs.
            npc.buffImmune[ModContent.BuffType<ExoFreeze>()] = true;
            npc.buffImmune[ModContent.BuffType<GlacialState>()] = true;
            npc.buffImmune[ModContent.BuffType<Eutrophication>()] = true;
            npc.buffImmune[ModContent.BuffType<TemporalSadness>()] = true;

            if (Main.netMode != NetmodeID.MultiplayerClient && hasSummonedSegments == 0f)
            {
                SummonSegments(npc);
                hasSummonedSegments = 1f;
            }

            // Disappear if SCal is not present.
            if (!NPC.AnyNPCs(ModContent.NPCType<SCalNPC>()))
            {
                npc.active = false;
                return false;
            }

            // Become angry.
            npc.Calamity().CurrentlyEnraged = SupremeCalamitasBehaviorOverride.Enraged;

            npc.localAI[0] = MathHelper.Clamp(npc.localAI[0] - 0.1f, 0f, 1f);
            switch ((SepulcherAttackType)attackState)
            {
                case SepulcherAttackType.AttackDelay:
                    DoBehavior_AttackDelay(npc, target, ref attackTimer);
                    break;
                case SepulcherAttackType.ErraticCharges:
                    DoBehavior_ErraticCharges(npc, target, ref attackTimer);
                    break;
                case SepulcherAttackType.PerpendicularBoneCharges:
                    DoBehavior_PerpendicularBoneCharges(npc, target, ref attackTimer);
                    break;
                case SepulcherAttackType.SoulBombBursts:
                    DoBehavior_SoulBombBursts(npc, target, ref attackTimer);
                    break;
            }
            npc.rotation = npc.velocity.ToRotation() + MathHelper.PiOver2;
            attackTimer++;

            return false;
        }

        public static void DoBehavior_AttackDelay(NPC npc, Player target, ref float attackTimer)
        {
            // Fade in.
            npc.Opacity = MathHelper.Clamp(npc.Opacity + 0.1f, 0f, 1f);

            float chargeInterpolant = Utils.InverseLerp(40f, 110f, attackTimer, true);
            Vector2 idealVelocity = npc.SafeDirectionTo(target.Center) * new Vector2(7f, -13f);

            if (!npc.WithinRange(target.Center, 180f))
                idealVelocity = Vector2.Lerp(idealVelocity, npc.SafeDirectionTo(target.Center) * 18f, chargeInterpolant);

            npc.velocity = Vector2.Lerp(npc.velocity, idealVelocity, 0.035f);

            if (attackTimer >= 150f)
                SelectNextAttack(npc);
        }

        public static void DoBehavior_ErraticCharges(NPC npc, Player target, ref float attackTimer)
        {
            int chargeCount = 4;
            int erraticMovementTime = 90;
            int chargeRedirectTime = 12;
            int chargeTime = 54;
            int bombReleaseRate = 33;
            float chargeSpeed = 36f;

            if (SupremeCalamitasBehaviorOverride.Enraged)
                chargeSpeed = 76f;

            float bombRadius = 720f;
            float maxBombOffset = 1080f;
            float moveSpeed = chargeSpeed * 0.425f;
            ref float chargeCounter = ref npc.Infernum().ExtraAI[0];
            ref float bombOffsetInterpolant = ref npc.Infernum().ExtraAI[1];
            ref float bombSpawnOffsetAngle = ref npc.Infernum().ExtraAI[2];
            ref float bombSpawnOffsetAngleDirection = ref npc.Infernum().ExtraAI[3];

            // Initialize the bomb angular direction;
            if (bombSpawnOffsetAngleDirection == 0f)
            {
                bombSpawnOffsetAngleDirection = Main.rand.NextBool().ToDirectionInt();
                npc.netUpdate = true;
            }

            // Release bombs around the target.
            if (attackTimer % bombReleaseRate == bombReleaseRate - 1f)
            {
            	Main.PlaySound(InfernumMode.CalamityMod.GetLegacySoundSlot(SoundType.Custom, "Sounds/Custom/SCalSounds/BrimstoneBigShoot"), target.Center);

                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    bombOffsetInterpolant += 0.23f;
                    if (bombOffsetInterpolant >= 1f)
                    {
                        bombSpawnOffsetAngle += bombSpawnOffsetAngleDirection * MathHelper.PiOver2;
                        bombOffsetInterpolant--;
                    }

                    Vector2 bombSpawnPosition = target.Center + new Vector2(MathHelper.Lerp(-maxBombOffset, maxBombOffset, bombOffsetInterpolant % 1f), -maxBombOffset).RotatedBy(bombSpawnOffsetAngle);
                    int bomb = Utilities.NewProjectileBetter(bombSpawnPosition, Vector2.UnitY.RotatedBy(bombSpawnOffsetAngle) * 17f, ModContent.ProjectileType<DemonicBomb>(), 0, 0f);
                    if (Main.projectile.IndexInRange(bomb))
                    {
                        Main.projectile[bomb].timeLeft = 180;
                        Main.projectile[bomb].ModProjectile<DemonicBomb>().ExplosionRadius = bombRadius;
                        Main.projectile[bomb].ModProjectile<DemonicBomb>().ExplodeIntoDarts = true;
                    }
                }
            }

            // Erratically hover around.
            if (attackTimer < erraticMovementTime)
            {
                if (attackTimer % 30f >= 25f)
                    npc.velocity = Vector2.Lerp(npc.velocity, npc.SafeDirectionTo(target.Center), 0.4f).SafeNormalize(Vector2.UnitY) * moveSpeed;

                else if (!npc.WithinRange(target.Center, 200f))
                    npc.velocity = npc.velocity.RotatedBy(Utilities.AperiodicSin(MathHelper.TwoPi * attackTimer / 100f) * 0.1f);
                return;
            }

            // Charge towards the target.
            if (attackTimer < erraticMovementTime + chargeRedirectTime)
            {
                Vector2 idealVelocity = npc.SafeDirectionTo(target.Center) * chargeSpeed;
                npc.velocity = Vector2.Lerp(npc.velocity, idealVelocity, 0.2f);
                if (npc.velocity.Length() < chargeSpeed * 0.4f)
                    npc.velocity *= 1.32f;
                if (attackTimer == erraticMovementTime + chargeRedirectTime - 1f)
                {
                    npc.velocity = idealVelocity;
                    npc.netUpdate = true;
                }
                return;
            }
            
            if (attackTimer >= erraticMovementTime + chargeTime)
            {
                chargeCounter++;
                if (chargeCounter >= chargeCount)
                {
                    Utilities.DeleteAllProjectiles(true, ModContent.ProjectileType<DemonicBomb>(), ModContent.ProjectileType<BrimstoneBarrage>());
                    SelectNextAttack(npc);
                }

                attackTimer = 0f;
            }
        }

        public static void DoBehavior_PerpendicularBoneCharges(NPC npc, Player target, ref float attackTimer)
        {
            int chargeTime = 40;
            int chargeCount = 3;
            int boneReleaseRate = 4;
            int chargeRedirectTime = 10;
            float chargeSpeed = 36.5f;
            
            if (SupremeCalamitasBehaviorOverride.Enraged)
                chargeSpeed = 78f;

            ref float attackState = ref npc.Infernum().ExtraAI[0];
            ref float chargeCounter = ref npc.Infernum().ExtraAI[1];

            // Hover into position for the charge.
            if (attackState == 0f)
            {
                Vector2 hoverDestination = target.Center + new Vector2((target.Center.X < npc.Center.X).ToDirectionInt() * 500f, -400f);
                Vector2 idealVelocity = npc.SafeDirectionTo(hoverDestination) * (attackTimer / 15f + 25f);
                npc.velocity = npc.velocity.MoveTowards(idealVelocity, 0.4f).RotateTowards(idealVelocity.ToRotation(), 0.05f);

                if (attackTimer == 20f)
                    Main.PlaySound(SoundID.DD2_SkeletonSummoned.WithVolume(3f), target.Center);

                npc.localAI[0] = Utils.InverseLerp(0f, 45f, attackTimer, true);
                if (attackTimer >= 270 || npc.WithinRange(hoverDestination, 95f))
                {
                    attackState = 1f;
                    attackTimer = 0f;
                    npc.netUpdate = true;
                }
                return;
            }

            // Begin charging at the target.
            if (attackState == 1f)
            {
                Vector2 chargeVelocity = npc.SafeDirectionTo(target.Center) * chargeSpeed;
                npc.velocity = Vector2.Lerp(npc.velocity, chargeVelocity, 0.15f);
                npc.localAI[0] = 1f;
                if (attackTimer >= chargeRedirectTime)
                {
                    npc.velocity = chargeVelocity;
                    attackState = 2f;
                    attackTimer = 0f;
                    npc.netUpdate = true;
                }
                return;
            }

            // Charge and release bones perpendicular to the direction Sepulcher is going.
            if (attackTimer % boneReleaseRate == boneReleaseRate - 1f)
            {
                Main.PlaySound(SoundID.NPCHit2, npc.Center);
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    Vector2 leftVelocity = npc.velocity.SafeNormalize(Vector2.UnitY).RotatedBy(-MathHelper.PiOver2) * 4f;
                    Vector2 rightVelocity = npc.velocity.SafeNormalize(Vector2.UnitY).RotatedBy(MathHelper.PiOver2) * 4f;
                    Utilities.NewProjectileBetter(npc.Center, leftVelocity, ModContent.ProjectileType<SepulcherBone>(), 500, 0f);
                    Utilities.NewProjectileBetter(npc.Center, rightVelocity, ModContent.ProjectileType<SepulcherBone>(), 500, 0f);
                }
            }

            npc.localAI[0] = Utils.InverseLerp(chargeTime, chargeTime - 16f, attackTimer, true);
            if (attackTimer >= chargeTime)
            {
                attackState = 0f;
                attackTimer = 0f;
                chargeCounter++;
                if (chargeCounter >= chargeCount)
                    SelectNextAttack(npc);
                npc.netUpdate = true;
            }
        }

        public static void DoBehavior_SoulBombBursts(NPC npc, Player target, ref float attackTimer)
        {
            int bombID = ModContent.ProjectileType<SepulcherSoulBomb>();
            int bombHoverTime = 75;
            int bombExplodeDelay = 48;
            int bombReleaseRate = 172;
            int bombLaunchCount = 3;
            bool bombExists = Utilities.AnyProjectiles(bombID);
            float wrappedAttackTimer = attackTimer % bombReleaseRate;
            float bombFlySpeed = 23f;
            ref float bombLaunchCounter = ref npc.Infernum().ExtraAI[0];

            // Slowly approach the target.
            if (wrappedAttackTimer < bombHoverTime && !npc.WithinRange(target.Center, 600f))
            {
                Vector2 idealVelocity = npc.SafeDirectionTo(target.Center) * 11f;
                npc.velocity = npc.velocity.RotateTowards(idealVelocity.ToRotation(), 0.037f);
                npc.velocity = npc.velocity.SafeNormalize(Vector2.UnitY) * MathHelper.Lerp(npc.velocity.Length(), idealVelocity.Length(), 0.1f);
            }
            if (npc.velocity.Length() > 11.5f)
                npc.velocity = npc.velocity.SafeNormalize(Vector2.UnitY) * 11.49f;
            if (!npc.WithinRange(target.Center, 920f))
                npc.Center = npc.Center.MoveTowards(target.Center, 15f);

            // Create the bomb.
            if (Main.netMode != NetmodeID.MultiplayerClient && wrappedAttackTimer == 1f && !bombExists)
            {
                if (bombLaunchCounter < bombLaunchCount)
                    Utilities.NewProjectileBetter(npc.Center, Vector2.Zero, bombID, 600, 0f);
                else
                {
                    SelectNextAttack(npc);
                    return;
                }
            }

            // Launch the bomb.
            if (wrappedAttackTimer == bombHoverTime)
            {
            	Main.PlaySound(InfernumMode.CalamityMod.GetLegacySoundSlot(SoundType.Item, "Sounds/Item/ScorchedEarthShot3"), npc.Center);
                foreach (Projectile bomb in Utilities.AllProjectilesByID(bombID))
                {
                    bomb.ModProjectile<SepulcherSoulBomb>().ExplodeCountdown = bombExplodeDelay;
                    bomb.velocity = npc.SafeDirectionTo(target.Center) * bombFlySpeed;
                    bomb.netUpdate = true;
                }
                bombLaunchCounter++;
                npc.netUpdate = true;
            }
        }

        public static void SummonSegments(NPC npc)
        {
            int previousSegment = npc.whoAmI;
            float rotationalOffset = 0f;
            float passedVar = 0f;
            for (int i = 0; i < MaxLength; i++)
            {
                int lol;
                if (i >= 0 && i < MinLength && i % 2 == 1)
                {
                    lol = NPC.NewNPC((int)npc.Center.X, (int)npc.Center.Y, ModContent.NPCType<SCalWormBodyWeak>(), npc.whoAmI);
                    Main.npc[lol].localAI[0] += passedVar;
                    passedVar += 36f;
                }
                else if (i >= 0 && i < MinLength)
                {
                    lol = NPC.NewNPC((int)npc.Center.X, (int)npc.Center.Y, ModContent.NPCType<SCalWormBody>(), npc.whoAmI);
                    Main.npc[lol].localAI[3] = i;
                }
                else
                    lol = NPC.NewNPC((int)npc.Center.X, (int)npc.Center.Y, ModContent.NPCType<SCalWormTail>(), npc.whoAmI);

                // Create arms.
                if (i >= 3 && i % 4 == 0)
                {
                    NPC segment = Main.npc[lol];
                    int arm = NPC.NewNPC((int)segment.Center.X, (int)segment.Center.Y, ModContent.NPCType<SCalWormArm>(), lol);
                    if (Main.npc.IndexInRange(arm))
                    {
                        Main.npc[arm].ai[0] = lol;
                        Main.npc[arm].direction = 1;
                        Main.npc[arm].rotation = rotationalOffset;
                    }

                    rotationalOffset += MathHelper.Pi / 6f;

                    arm = NPC.NewNPC((int)segment.Center.X, (int)segment.Center.Y, ModContent.NPCType<SCalWormArm>(), lol);
                    if (Main.npc.IndexInRange(arm))
                    {
                        Main.npc[arm].ai[0] = lol;
                        Main.npc[arm].direction = -1;
                        Main.npc[arm].rotation = rotationalOffset + MathHelper.Pi;
                    }

                    rotationalOffset += MathHelper.Pi / 6f;
                    rotationalOffset = MathHelper.WrapAngle(rotationalOffset);
                }

                Main.npc[lol].realLife = npc.whoAmI;
                Main.npc[lol].ai[2] = npc.whoAmI;
                Main.npc[lol].ai[1] = previousSegment;
                Main.npc[previousSegment].ai[0] = lol;
                previousSegment = lol;
            }
        }

        public static void SelectNextAttack(NPC npc)
        {
            npc.ai[2] = 0f;
            for (int i = 0; i < 5; i++)
                npc.Infernum().ExtraAI[i] = 0f;

            switch ((SepulcherAttackType)npc.ai[1])
            {
                case SepulcherAttackType.AttackDelay:
                    npc.ai[1] = (int)SepulcherAttackType.ErraticCharges;
                    break;
                case SepulcherAttackType.ErraticCharges:
                    npc.ai[1] = (int)SepulcherAttackType.PerpendicularBoneCharges;
                    break;
                case SepulcherAttackType.PerpendicularBoneCharges:
                    npc.ai[1] = (int)SepulcherAttackType.SoulBombBursts;
                    break;
                case SepulcherAttackType.SoulBombBursts:
                    npc.ai[1] = (int)SepulcherAttackType.ErraticCharges;
                    break;
            }

            npc.netUpdate = true;
        }

        public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Color lightColor)
        {
            SpriteEffects direction = SpriteEffects.None;
            if (npc.spriteDirection == 1)
                direction = SpriteEffects.FlipHorizontally;

            Texture2D texture = Main.npcTexture[npc.type];
            Vector2 drawPosition = npc.Center - Main.screenPosition;

            float backglowInterpolant = npc.localAI[0];
            if (backglowInterpolant > 0f)
            {
                for (int i = 0; i < 6; i++)
                {
                    Vector2 drawOffset = (MathHelper.TwoPi * i / 6f).ToRotationVector2() * backglowInterpolant * 5f;
                    spriteBatch.Draw(texture, drawPosition + drawOffset, npc.frame, new Color(255, 0, 0, 0) * backglowInterpolant, npc.rotation, npc.frame.Size() * 0.5f, npc.scale, direction, 0f);
                }
            }
            spriteBatch.Draw(texture, drawPosition, npc.frame, npc.GetAlpha(lightColor), npc.rotation, npc.frame.Size() * 0.5f, npc.scale, direction, 0f);
            return false;
        }
    }
}