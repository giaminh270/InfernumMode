using CalamityMod;
using InfernumMode.OverridingSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.MoonLord
{
    public class MoonLordHandBehaviorOverride : NPCBehaviorOverride
    {
        public override int NPCOverrideType => NPCID.MoonLordHand;


        public override NPCOverrideContext ContentToOverride => NPCOverrideContext.NPCAI | NPCOverrideContext.NPCPreDraw;

        public override bool PreAI(NPC npc)
        {
            // Disappear if the body is not present.
            if (!Main.npc.IndexInRange((int)npc.ai[3]) || !Main.npc[(int)npc.ai[3]].active)
            {
                npc.active = false;
                return false;
            }

            // Define the core NPC and inherit properties from it.
            NPC core = Main.npc[(int)npc.ai[3]];

            npc.target = core.target;

            int handSide = (npc.ai[2] == 1f).ToDirectionInt();
            bool hasPopped = npc.ai[0] == -2f;
            float attackTimer = core.ai[1];
            Player target = Main.player[npc.target];

            npc.dontTakeDamage = hasPopped;

            ref float pupilRotation = ref npc.localAI[0];
            ref float pupilOutwardness = ref npc.localAI[1];
            ref float pupilScale = ref npc.localAI[2];

            // Hacky workaround to problems with popping.
            if (npc.life < 1000)
                npc.life = 1000;

            int idealFrame = 0;

            switch ((MoonLordCoreBehaviorOverride.MoonLordAttackState)(int)core.ai[0])
            {
                case MoonLordCoreBehaviorOverride.MoonLordAttackState.PhantasmalSphereHandWaves:
                    if (!hasPopped)
                        DoBehavior_PhantasmalSphereHandWaves(npc, core, target, handSide, attackTimer, ref pupilRotation, ref pupilOutwardness, ref pupilScale, ref idealFrame);
                    break;
                case MoonLordCoreBehaviorOverride.MoonLordAttackState.PhantasmalFlareBursts:
                    if (!hasPopped)
                        DoBehavior_PhantasmalFlareBursts(npc, core, target, handSide, attackTimer, ref pupilRotation, ref pupilOutwardness, ref pupilScale, ref idealFrame);
                    break;
                case MoonLordCoreBehaviorOverride.MoonLordAttackState.ExplodingConstellations:
                    DoBehavior_ExplodingConstellations(npc, core, target, handSide, attackTimer, ref idealFrame);
                    break;
                default:
                    DoBehavior_DefaultHandHover(npc, core, handSide, attackTimer, ref idealFrame);
                    if (core.ai[0] == (int)MoonLordCoreBehaviorOverride.MoonLordAttackState.PhantasmalSpin)
                    {
                        idealFrame = 0;
                        npc.dontTakeDamage = false;
                    }
                    break;
            }

            if (hasPopped)
            {
                npc.life = 1;

                DoBehavior_DefaultHandHover(npc, core, handSide, attackTimer, ref idealFrame);
                idealFrame = 0;
            }

            // Handle frames.
            int idealFrameCounter = idealFrame * 7;
            if (idealFrameCounter > npc.frameCounter)
                npc.frameCounter += 1D;
            if (idealFrameCounter < npc.frameCounter)
                npc.frameCounter -= 1D;
            npc.frameCounter = MathHelper.Clamp((float)npc.frameCounter, 0f, 21f);

            return false;
        }

        public static void DoBehavior_DefaultHandHover(NPC npc, NPC core, int handSide, float attackTimer, ref int idealFrame)
        {
            idealFrame = 3;
            npc.dontTakeDamage = true;

            Vector2 idealPosition = core.Center + new Vector2(handSide * 450f, -70f);
            idealPosition += (attackTimer / 32f + npc.whoAmI * 2.3f).ToRotationVector2() * 24f;

            Vector2 idealVelocity = Vector2.Zero.MoveTowards(idealPosition - npc.Center, 15f);
            npc.velocity = Vector2.Lerp(npc.velocity, idealVelocity, 0.125f).MoveTowards(idealVelocity, 2f);
        }

        public static void DoBehavior_PhantasmalSphereHandWaves(NPC npc, NPC core, Player target, int handSide, float attackTimer, ref float pupilRotation, ref float pupilOutwardness, ref float pupilScale, ref int idealFrame)
        {
            int waveTime = 270;
            int sphereShootDelay = 36;
            int sphereShootRate = 12;
            int attackTransitionDelay = 40;
            float sphereShootSpeed = 12f;
            float sphereSlamSpeed = 6f;
            if (MoonLordCoreBehaviorOverride.CurrentActiveArms <= 1)
            {
                sphereShootRate -= 4;
                sphereSlamSpeed += 3f;
            }
            if (MoonLordCoreBehaviorOverride.IsEnraged)
            {
                sphereShootRate /= 2;
                sphereSlamSpeed += 7f;
            }

            float handCloseInterpolant = Utils.InverseLerp(0f, 16f, attackTimer - waveTime, true);

            Vector2 startingIdealPosition = core.Center + new Vector2(handSide * 300f, -125f);
            Vector2 endingIdealPosition = core.Center + new Vector2(handSide * 750f, -70f);
            Vector2 idealPosition = Vector2.SmoothStep(startingIdealPosition, endingIdealPosition, MathHelper.Clamp(attackTimer / waveTime - handCloseInterpolant, 0f, 1f));
            idealPosition += (attackTimer / 16f).ToRotationVector2() * new Vector2(10f, 30f);

            Vector2 idealVelocity = Vector2.Zero.MoveTowards(idealPosition - npc.Center, 15f);
            npc.velocity = Vector2.Lerp(npc.velocity, idealVelocity, 0.1f).MoveTowards(idealVelocity, 1.6f);

            // Open the hand right before firing.
            if (attackTimer < sphereShootDelay - 12f || attackTimer >= waveTime)
            {
                pupilScale = MathHelper.Lerp(pupilScale, 0.3f, 0.1f);
                pupilOutwardness = MathHelper.Lerp(pupilOutwardness, 0f, 0.1f);
                idealFrame = 3;

                // Shut hands faster after the spheres have been released.
                if (attackTimer >= waveTime && attackTimer < waveTime + 16f)
                    npc.frameCounter = MathHelper.Clamp((float)npc.frameCounter + 1f, 0f, 21f);
            }
            else
            {
                pupilScale = MathHelper.Lerp(pupilScale, 0.75f, 0.1f);
                pupilOutwardness = MathHelper.Lerp(pupilOutwardness, 0.5f, 0.1f);
                pupilRotation = pupilRotation.AngleLerp(npc.AngleTo(target.Center), 0.1f);
                idealFrame = 0;
            }

            // Become invulnerable if the hand is closed.
            if (npc.frameCounter > 7)
                npc.dontTakeDamage = true;

            bool canShootPhantasmalSpheres = true;
            if (attackTimer < sphereShootDelay)
                canShootPhantasmalSpheres = false;
            if (attackTimer >= waveTime)
                canShootPhantasmalSpheres = false;

            if (canShootPhantasmalSpheres)
            {
                float attackCompletion = Utils.InverseLerp(0f, waveTime, attackTimer, true);
                float maximumAngularDisparity = MathHelper.TwoPi;
                float angularShootOffset = MathHelper.SmoothStep(0f, maximumAngularDisparity, attackCompletion) * -handSide;
                Vector2 sphereShootVelocity = -Vector2.UnitY.RotatedBy(angularShootOffset) * sphereShootSpeed;
                pupilRotation = sphereShootVelocity.ToRotation();

                if (attackTimer % sphereShootRate == sphereShootRate - 1f)
                {
                    Main.PlaySound(SoundID.Item122, npc.Center);

                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        int sphere = Utilities.NewProjectileBetter(npc.Center, sphereShootVelocity, ProjectileID.PhantasmalSphere, 215, 0f, npc.target);
                        if (Main.projectile.IndexInRange(sphere))
                        {
                            Main.projectile[sphere].ai[1] = npc.whoAmI;
                            Main.projectile[sphere].netUpdate = true;
                        }

                        // Sync the entire moon lord's current state. This will be executed on the frame immediately after this one.
                        core.netUpdate = true;
                    }
                }
            }

            // Slam all phantasmal spheres at the target after they have been fired.
            if (attackTimer == waveTime + 16f && (handSide == 1 || MoonLordCoreBehaviorOverride.CurrentActiveArms == 1))
            {
                if (Main.netMode != NetmodeID.Server)
                {
                    var sound = Main.PlaySound(SoundID.DD2_PhantomPhoenixShot, target.Center);
                    if (sound != null)
                    {
                        sound.Volume = MathHelper.Clamp(sound.Volume * 1.85f, 0f, 1f);
                        sound.Pitch = -0.5f;
                    }
                }

                foreach (Projectile sphere in Utilities.AllProjectilesByID(ProjectileID.PhantasmalSphere))
                {
                    sphere.ai[0] = -1f;
                    sphere.velocity = sphere.SafeDirectionTo(target.Center) * sphereSlamSpeed;
                    sphere.tileCollide = Collision.CanHit(sphere.Center, 0, 0, target.Center, 0, 0);
                    sphere.timeLeft = sphere.MaxUpdates * 270;
                    sphere.netUpdate = true;
                }
            }

            if (attackTimer >= waveTime + attackTransitionDelay)
                core.Infernum().ExtraAI[5] = 1f;
        }

        public static void DoBehavior_PhantasmalFlareBursts(NPC npc, NPC core, Player target, int handSide, float attackTimer, ref float pupilRotation, ref float pupilOutwardness, ref float pupilScale, ref int idealFrame)
        {
            int flareCreationRate = 4;
            int flareTelegraphTime = 150;
            int flareReleaseDelay = 32;
            int flareShootTime = 60;
            float flareSpawnOffsetMax = 900f;
            if (MoonLordCoreBehaviorOverride.IsEnraged)
            {
                flareCreationRate -= 2;
                flareSpawnOffsetMax += 400f;
            }

            idealFrame = 0;
            pupilRotation = pupilRotation.AngleLerp(0f, 0.1f);
            pupilOutwardness = MathHelper.Lerp(pupilOutwardness, 0f, 0.1f);
            pupilScale = MathHelper.Lerp(pupilScale, 0.35f, 0.1f);

            float handCloseInterpolant = Utils.InverseLerp(0f, flareReleaseDelay, attackTimer - flareTelegraphTime, true);
            Vector2 startingIdealPosition = core.Center + new Vector2(handSide * 300f, -100f);
            Vector2 endingIdealPosition = core.Center + new Vector2(handSide * 750f, -150f);
            Vector2 idealPosition = Vector2.SmoothStep(startingIdealPosition, endingIdealPosition, MathHelper.Clamp(attackTimer / flareTelegraphTime - handCloseInterpolant, 0f, 1f));
            idealPosition += (attackTimer / 16f).ToRotationVector2() * 12f;

            Vector2 idealVelocity = Vector2.Zero.MoveTowards(idealPosition - npc.Center, 15f);
            npc.velocity = Vector2.Lerp(npc.velocity, idealVelocity, 0.1f).MoveTowards(idealVelocity, 1.6f);

            // Create flare telegraphs.
            if (attackTimer < flareTelegraphTime && attackTimer % flareCreationRate == flareCreationRate - 1f && (handSide == 1 || MoonLordCoreBehaviorOverride.CurrentActiveArms == 1))
            {
                Main.PlaySound(SoundID.Item72, target.Center);

                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    Vector2 flareSpawnPosition = target.Center + Vector2.UnitX * Main.rand.NextFloatDirection() * flareSpawnOffsetMax;
                    int telegraph = Utilities.NewProjectileBetter(flareSpawnPosition, Vector2.Zero, ModContent.ProjectileType<LunarFlareTelegraph>(), 0, 0f);
                    if (Main.projectile.IndexInRange(telegraph))
                    {
                        Main.projectile[telegraph].ai[0] = flareTelegraphTime - attackTimer + flareReleaseDelay;
                        Main.projectile[telegraph].ai[1] = Main.rand.NextBool(8).ToInt();
                    }
                }
            }

            if (attackTimer >= flareTelegraphTime + flareReleaseDelay + flareShootTime)
                core.Infernum().ExtraAI[5] = 1f;
        }

        public static void DoBehavior_ExplodingConstellations(NPC npc, NPC core, Player target, int handSide, float attackTimer, ref int idealFrame)
        {
            idealFrame = 0;
            int initialAnimationTime = 54;
            int starCreationRate = 4;
            int totalStarsToCreate = 15;
            int explosionTime = 130;
            int constellationCount = 3;

            if (MoonLordCoreBehaviorOverride.InFinalPhase)
            {
                starCreationRate--;
                totalStarsToCreate += 3;
            }

            if (MoonLordCoreBehaviorOverride.IsEnraged)
            {
                starCreationRate = 2;
                explosionTime -= 50;
            }

            int starCreationTime = totalStarsToCreate * starCreationRate;
            float animationCompletionRatio = MathHelper.Clamp(attackTimer / initialAnimationTime, 0f, 1f);
            float wrappedAttackTimer = (attackTimer + (handSide == 0f ? 0f : 36f)) % (initialAnimationTime + starCreationTime + explosionTime);
            Vector2 startingIdealPosition = core.Center + new Vector2(handSide * 300f, -125f);
            Vector2 endingIdealPosition = core.Center + new Vector2(handSide * 450f, -350f);

            ref float constellationPatternType = ref npc.Infernum().ExtraAI[0];
            ref float constellationSeed = ref npc.Infernum().ExtraAI[1];

            // Create charge dust and close hands before the attack begins.
            if (wrappedAttackTimer < initialAnimationTime - 12f)
            {
                float chargePowerup = Utils.InverseLerp(0f, 0.5f, animationCompletionRatio, true);
                int chargeDustCount = (int)Math.Round(MathHelper.Lerp(1f, 3f, chargePowerup));
                float chargeDustOffset = MathHelper.Lerp(30f, 75f, chargePowerup);

                for (int i = 0; i < chargeDustCount; i++)
                {
                    Vector2 chargeDustSpawnPosition = npc.Center + Main.rand.NextVector2CircularEdge(chargeDustOffset, chargeDustOffset) * Main.rand.NextFloat(0.8f, 1f);
                    Vector2 chargeDustVelocity = (npc.Center - chargeDustSpawnPosition) * 0.05f;
                    Dust electricity = Dust.NewDustPerfect(chargeDustSpawnPosition, 229);
                    electricity.velocity = chargeDustVelocity * Main.rand.NextFloat(0.9f, 1.1f);
                    electricity.scale = MathHelper.Lerp(1f, 1.45f, chargePowerup);
                    electricity.alpha = 84;
                    electricity.noGravity = true;
                }

                idealFrame = 3;
            }

            float hoverInterpolant = CalamityUtils.Convert01To010(animationCompletionRatio);
            Vector2 idealPosition = Vector2.SmoothStep(startingIdealPosition, endingIdealPosition, hoverInterpolant);
            idealPosition += (attackTimer / 16f).ToRotationVector2() * new Vector2(10f, 30f);

            Vector2 idealVelocity = Vector2.Zero.MoveTowards(idealPosition - npc.Center, 18f);
            npc.velocity = Vector2.Lerp(npc.velocity, idealVelocity, 0.18f).MoveTowards(idealVelocity, 1.8f);

            // Determine what constellation pattern this arm will use. Each arm has their own pattern that they create.
            if (Main.netMode != NetmodeID.MultiplayerClient && wrappedAttackTimer == initialAnimationTime - 30f)
            {
                constellationSeed = Main.rand.NextFloat();
                constellationPatternType = Main.rand.Next(3);
                npc.netUpdate = true;
            }

            // Create stars.
            if (wrappedAttackTimer >= initialAnimationTime &&
                wrappedAttackTimer < initialAnimationTime + starCreationTime &&
                (wrappedAttackTimer - initialAnimationTime) % starCreationRate == 0f)
            {
                float patternCompletion = Utils.InverseLerp(initialAnimationTime, initialAnimationTime + starCreationTime, wrappedAttackTimer, true);
                Vector2 currentPoint;
                switch ((int)constellationPatternType)
                {
                    // Diagonal stars from top left to bottom right.
                    case 0:
                        Vector2 startingPoint = target.Center + new Vector2(-800f, -600f);
                        Vector2 endingPoint = target.Center + new Vector2(800f, 600f);
                        currentPoint = Vector2.Lerp(startingPoint, endingPoint, patternCompletion);
                        break;

                    // Diagonal stars from top right to bottom left.
                    case 1:
                        startingPoint = target.Center + new Vector2(800f, -600f);
                        endingPoint = target.Center + new Vector2(-800f, 600f);
                        currentPoint = Vector2.Lerp(startingPoint, endingPoint, patternCompletion);
                        break;

                    // Horizontal sinusoid.
                    case 2:
                    default:
                        float horizontalOffset = MathHelper.Lerp(-775f, 775f, patternCompletion);
                        float verticalOffset = (float)Math.Cos(patternCompletion * MathHelper.Pi + constellationSeed * MathHelper.TwoPi) * 420f;
                        currentPoint = target.Center + new Vector2(horizontalOffset, verticalOffset);
                        break;
                }

                Main.PlaySound(SoundID.Item72, currentPoint);
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    int star = Utilities.NewProjectileBetter(currentPoint, Vector2.Zero, ModContent.ProjectileType<StardustConstellation>(), 0, 0f);
                    if (Main.projectile.IndexInRange(star))
                    {
                        Main.projectile[star].ai[0] = (int)(patternCompletion * totalStarsToCreate);
                        Main.projectile[star].ai[1] = npc.whoAmI;
                    }
                }
            }

            // Make all constellations spawned by this hand prepare to explode.
            if (wrappedAttackTimer == initialAnimationTime + starCreationTime)
            {
                foreach (Projectile star in Utilities.AllProjectilesByID(ModContent.ProjectileType<StardustConstellation>()).Where(p => p.ai[1] == npc.whoAmI))
                    star.timeLeft = 50;
            }

            if (attackTimer >= (initialAnimationTime + starCreationTime + explosionTime) * constellationCount - 1f)
                core.Infernum().ExtraAI[5] = 1f;
        }

        public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Color lightColor)
        {
            Texture2D texture = Main.npcTexture[npc.type];
            Vector2 shoulderOffset = new Vector2(220f, -60f);
            Texture2D armTexture = Main.extraTexture[15];
            Vector2 coreCenter = Main.npc[(int)npc.ai[3]].Center;
            Point centerTileCoords = npc.Center.ToTileCoordinates();
            Color color = npc.GetAlpha(Color.Lerp(Lighting.GetColor(centerTileCoords.X, centerTileCoords.Y), Color.White, 0.3f));
            bool isLeftHand = npc.ai[2] == 0f;
            Vector2 directionThing = new Vector2((!isLeftHand).ToDirectionInt(), 1f);
            Vector2 handOrigin = new Vector2(120f, 180f);
            if (!isLeftHand)
                handOrigin.X = texture.Width - handOrigin.X;

            Texture2D scleraTexture = Main.extraTexture[17];
            Texture2D pupilTexture = Main.extraTexture[19];
            Vector2 scleraFrame = new Vector2(26f, 42f);
            if (!isLeftHand)
                scleraFrame.X = scleraTexture.Width - scleraFrame.X;

            Texture2D exposedEyeTexture = Main.extraTexture[26];
            Rectangle exposedEyeFrame = exposedEyeTexture.Frame(1, 1, 0, 0);
            exposedEyeFrame.Height /= 4;
            Vector2 shoulderCenter = coreCenter + shoulderOffset * directionThing;
            Vector2 handBottom = npc.Center + new Vector2(0f, 76f);
            Vector2 v = (shoulderCenter - handBottom) * 0.5f;
            Vector2 armOrigin = new Vector2(60f, 30f);
            SpriteEffects direction = npc.ai[2] != 1f ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            if (!isLeftHand)
                armOrigin.X = armTexture.Width - armOrigin.X;
            
            float armAngularOffset = (float)Math.Acos(MathHelper.Clamp(v.Length() / 340f, 0f, 1f)) * -directionThing.X;
            float armRotation = v.ToRotation() + armAngularOffset - MathHelper.PiOver2;
            spriteBatch.Draw(armTexture, handBottom - Main.screenPosition, null, color, armRotation, armOrigin, 1f, direction, 0f);
            if (npc.ai[0] == -2f)
            {
                int frame = (int)(Main.GlobalTime * 9.3f) % 4;
                exposedEyeFrame.Y += exposedEyeFrame.Height * frame;
                Vector2 exposedEyeDrawPosition = npc.Center - Main.screenPosition;
                spriteBatch.Draw(exposedEyeTexture, exposedEyeDrawPosition, exposedEyeFrame, color, 0f, scleraFrame - new Vector2(4f, 4f), 1f, direction, 0f);
            }
            else
            {
                Vector2 scleraDrawPosition = npc.Center - Main.screenPosition;
                spriteBatch.Draw(scleraTexture, scleraDrawPosition, null, Color.White * npc.Opacity * 0.6f, 0f, scleraFrame, 1f, direction, 0f);
                Vector2 pupilOffset = Utils.Vector2FromElipse(npc.localAI[0].ToRotationVector2(), new Vector2(30f, 66f) * npc.localAI[1]) + new Vector2(-directionThing.X, 3f);
                spriteBatch.Draw(pupilTexture, npc.Center - Main.screenPosition + pupilOffset, null, Color.White * npc.Opacity * 0.6f, 0f, pupilTexture.Size() * 0.5f, 1f, SpriteEffects.None, 0f);
            }
            spriteBatch.Draw(texture, npc.Center - Main.screenPosition, npc.frame, color, 0f, handOrigin, 1f, direction, 0f);
            return false;
        }
    }
}
