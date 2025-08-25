using CalamityMod;
using CalamityMod.NPCs;
using CalamityMod.NPCs.ExoMechs.Apollo;
using CalamityMod.NPCs.ExoMechs.Ares;
using CalamityMod.NPCs.ExoMechs.Artemis;
using CalamityMod.NPCs.ExoMechs.Thanatos;
using CalamityMod.World;
using InfernumMode.OverridingSystem;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static CalamityMod.NPCs.ExoMechs.Draedon;
using DraedonNPC = CalamityMod.NPCs.ExoMechs.Draedon;

namespace InfernumMode.BehaviorOverrides.BossAIs.Draedon
{
    public class DraedonBehaviorOverride : NPCBehaviorOverride
    {
        public override int NPCOverrideType => ModContent.NPCType<DraedonNPC>();

        public override NPCOverrideContext ContentToOverride => NPCOverrideContext.NPCAI | NPCOverrideContext.NPCFindFrame;

        public const int IntroSoundLength = 106;

        public const int PostBattleMusicLength = 5120;

        public override bool PreAI(NPC npc)
        {
            // Set the whoAmI variable.
            CalamityGlobalNPC.draedon = npc.whoAmI;

            // Prevent stupid natural despawns.
            npc.timeLeft = 3600;

            // Define variables.
            ref float talkTimer = ref npc.ai[0];
            ref float hologramEffectTimer = ref npc.localAI[1];
            ref float killReappearDelay = ref npc.localAI[3];
            ref float musicDelay = ref npc.Infernum().ExtraAI[0];

            // Decide an initial target and play a teleport sound on the first frame.
            Player playerToFollow = Main.player[npc.target];
            if (talkTimer == 0f)
            {
                npc.TargetClosest(false);
                playerToFollow = Main.player[npc.target];
                Main.PlaySound(InfernumMode.CalamityMod.GetLegacySoundSlot(SoundType.Custom, "Sounds/Custom/DraedonTeleport"), playerToFollow.Center);
            }

            // Pick someone else to pay attention to if the old target is gone.
            if (playerToFollow.dead || !playerToFollow.active)
            {
                npc.TargetClosest(false);
                playerToFollow = Main.player[npc.target];

                // Fuck off if no living target exists.
                if (playerToFollow.dead || !playerToFollow.active)
                {
                    npc.life = 0;
                    npc.active = false;
                    return false;
                }
            }

            // Stay within the world.
            npc.position.Y = MathHelper.Clamp(npc.position.Y, 150f, Main.maxTilesY * 16f - 150f);

            npc.spriteDirection = (playerToFollow.Center.X < npc.Center.X).ToDirectionInt();

            // Handle delays when re-appearing after being killed.
            if (killReappearDelay > 0f)
            {
                npc.Opacity = 0f;
                killReappearDelay--;
                if (killReappearDelay <= 0f)
                    CalamityUtils.DisplayLocalizedText("Mods.CalamityMod.DraedonEndKillAttemptText", TextColor);
                return false;
            }

            // Synchronize the hologram effect and talk timer at the beginning.
            // Also calculate opacity.
            if (talkTimer <= HologramFadeinTime)
            {
                hologramEffectTimer = talkTimer;
                npc.Opacity = Utils.InverseLerp(0f, 8f, talkTimer, true);
            }

            // Play the stand up animation after teleportation.
            if (talkTimer == HologramFadeinTime + 5f)
                npc.ModNPC<DraedonNPC>().ShouldStartStandingUp = true;

            // Gloss over the arbitrary details and just get to the Exo Mech selection if Draedon has already been talked to.
            if (CalamityWorld.TalkedToDraedon && talkTimer > 70 && talkTimer < TalkDelay * 4f - 25f)
            {
                talkTimer = TalkDelay * 4f - 25f;
                npc.netUpdate = true;
            }

            if (Main.netMode != NetmodeID.MultiplayerClient && talkTimer == TalkDelay)
            {
                CalamityUtils.DisplayLocalizedText("Mods.CalamityMod.DraedonIntroductionText1", TextColor);
                npc.netUpdate = true;
            }

            if (Main.netMode != NetmodeID.MultiplayerClient && talkTimer == TalkDelay + DelayPerDialogLine)
            {
                CalamityUtils.DisplayLocalizedText("Mods.CalamityMod.DraedonIntroductionText2", TextColor);
                npc.netUpdate = true;
            }

            if (Main.netMode != NetmodeID.MultiplayerClient && talkTimer == TalkDelay + DelayPerDialogLine * 2f)
            {
                CalamityUtils.DisplayLocalizedText("Mods.CalamityMod.DraedonIntroductionText3", TextColor);
                npc.netUpdate = true;
            }

            if (Main.netMode != NetmodeID.MultiplayerClient && talkTimer == TalkDelay + DelayPerDialogLine * 3f)
            {
                CalamityUtils.DisplayLocalizedText("Mods.CalamityMod.DraedonIntroductionText4", TextColor);
                npc.netUpdate = true;
            }

            // Inform the player who summoned draedon they may choose the first mech and cause a selection UI to appear over their head.
            if (Main.netMode != NetmodeID.MultiplayerClient && talkTimer == TalkDelay + DelayPerDialogLine * 4f)
            {
                if (CalamityWorld.TalkedToDraedon)
                    CalamityUtils.DisplayLocalizedText("Mods.CalamityMod.DraedonResummonText", TextColorEdgy);
                else
                    Utilities.DisplayText("My creations will not forget your failures. Choose wisely.", TextColorEdgy);

                // Mark Draedon as talked to.
                if (!CalamityWorld.TalkedToDraedon)
                {
                    CalamityWorld.TalkedToDraedon = true;
                    CalamityNetcode.SyncWorld();
                }

                npc.netUpdate = true;
            }

            // Wait for the player to select an exo mech.
            if (talkTimer >= ExoMechChooseDelay && talkTimer < ExoMechChooseDelay + 8f && CalamityWorld.DraedonMechToSummon == ExoMech.None)
            {
                playerToFollow.Calamity().AbleToSelectExoMech = true;
                talkTimer = ExoMechChooseDelay;
            }

            // Fly around once the exo mechs have been spawned.
            if (ExoMechIsPresent || npc.ModNPC<DraedonNPC>().DefeatTimer > 0f)
            {
                npc.ModNPC<DraedonNPC>().FlyAroundInGamerChair();
                npc.ai[3]++;
            }

            // Make the screen rumble and summon the exo mechs.
            if (talkTimer > ExoMechChooseDelay + 8f && talkTimer < ExoMechPhaseDialogueTime)
            {
                Main.LocalPlayer.Calamity().GeneralScreenShakePower = Utils.InverseLerp(4200f, 1400f, Main.LocalPlayer.Distance(playerToFollow.Center), true) * 18f;
                Main.LocalPlayer.Calamity().GeneralScreenShakePower *= Utils.InverseLerp(ExoMechChooseDelay + 5f, ExoMechPhaseDialogueTime, talkTimer, true);
            }

            // Summon the selected exo mech.
            if (talkTimer == ExoMechChooseDelay + 10f)
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                    SummonExoMech(playerToFollow);

                if (Main.netMode != NetmodeID.Server)
                {
                    var sound = Main.PlaySound(InfernumMode.CalamityMod.GetLegacySoundSlot(SoundType.Item, "Sounds/Item/FlareSound"), playerToFollow.Center);
                    if (sound != null)
                        sound.Volume = MathHelper.Clamp(sound.Volume * 1.55f, 0f, 1f);
                    sound = Main.PlaySound(InfernumMode.Instance.GetLegacySoundSlot(SoundType.Custom, "Sounds/Custom/ExoMechIntro"), playerToFollow.Center);
                    if (sound != null)
                        sound.Volume = MathHelper.Clamp(sound.Volume * 1.5f, 0f, 1f);
                }
            }

            // Increment the music delay.
            if (talkTimer >= ExoMechChooseDelay + 10f)
                musicDelay++;

            // Dialogue lines depending on what phase the exo mechs are at.
            switch ((int)npc.localAI[0])
            {
                case 1:

                    if (Main.netMode != NetmodeID.MultiplayerClient && talkTimer == ExoMechPhaseDialogueTime)
                    {
                        CalamityUtils.DisplayLocalizedText("Mods.CalamityMod.DraedonExoPhase1Text1", TextColor);
                        npc.netUpdate = true;
                    }

                    if (Main.netMode != NetmodeID.MultiplayerClient && talkTimer == ExoMechPhaseDialogueTime + DelayPerDialogLine)
                    {
                        CalamityUtils.DisplayLocalizedText("Mods.CalamityMod.DraedonExoPhase1Text2", TextColor);
                        npc.netUpdate = true;
                    }

                    break;

                case 3:

                    if (Main.netMode != NetmodeID.MultiplayerClient && talkTimer == ExoMechPhaseDialogueTime)
                    {
                        Utilities.DisplayText("Your efforts are very intriguing.", TextColor);
                        npc.netUpdate = true;
                    }

                    if (talkTimer == ExoMechPhaseDialogueTime + DelayPerDialogLine)
                    {
                        Main.PlaySound(InfernumMode.CalamityMod.GetLegacySoundSlot(SoundType.Custom, "Sounds/Custom/DraedonLaugh"), playerToFollow.Center);
                        Utilities.DisplayText("Go on. Continue feeding information to my machines.", TextColorEdgy);
                    }

                    break;

                case 4:

                    if (Main.netMode != NetmodeID.MultiplayerClient && talkTimer == ExoMechPhaseDialogueTime)
                    {
                        CalamityUtils.DisplayLocalizedText("Mods.CalamityMod.DraedonExoPhase5Text1", TextColor);
                        npc.netUpdate = true;
                    }

                    if (Main.netMode != NetmodeID.MultiplayerClient && talkTimer == ExoMechPhaseDialogueTime + DelayPerDialogLine)
                    {
                        CalamityUtils.DisplayLocalizedText("Mods.CalamityMod.DraedonExoPhase5Text2", TextColor);
                        npc.netUpdate = true;
                    }

                    break;

                case 6:

                    if (Main.netMode != NetmodeID.MultiplayerClient && talkTimer == ExoMechPhaseDialogueTime)
                    {
                        CalamityUtils.DisplayLocalizedText("Mods.CalamityMod.DraedonExoPhase6Text1", TextColor);
                        npc.netUpdate = true;
                    }

                    if (Main.netMode != NetmodeID.MultiplayerClient && talkTimer == ExoMechPhaseDialogueTime + DelayPerDialogLine)
                    {
                        CalamityUtils.DisplayLocalizedText("Mods.CalamityMod.DraedonExoPhase6Text2", TextColor);
                        npc.netUpdate = true;
                    }

                    if (talkTimer == ExoMechPhaseDialogueTime + DelayPerDialogLine * 2f)
                    {
                        Main.PlaySound(InfernumMode.CalamityMod.GetLegacySoundSlot(SoundType.Custom, "Sounds/Custom/DraedonLaugh"), playerToFollow.Center);
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            CalamityUtils.DisplayLocalizedText("Mods.CalamityMod.DraedonExoPhase6Text3", TextColor);
                            npc.netUpdate = true;
                        }
                    }

                    break;
            }

            if (talkTimer > ExoMechChooseDelay + 10f && !ExoMechIsPresent)
            {
                HandleDefeatStuff(npc, playerToFollow, ref npc.ModNPC<DraedonNPC>().DefeatTimer);
                npc.ModNPC<DraedonNPC>().DefeatTimer++;
            }

            if (!ExoMechIsPresent)
            {
                if (npc.ModNPC<DraedonNPC>().DefeatTimer <= 0f)
                {
                    npc.modNPC.music = InfernumMode.CalamityMod.GetSoundSlot(SoundType.Music, "Sounds/Music/DraedonAmbience");
                    InfernumMode.DraedonThemeTimer = 0f;
                }
                else
                {
                    npc.modNPC.music = InfernumMode.Instance.GetSoundSlot(SoundType.Music, "Sounds/Music/Draedon");
                    InfernumMode.DraedonThemeTimer = 1f;
                }
            }

            talkTimer++;
            return false;
        }

        public static void SummonExoMech(Player playerToFollow)
        {
            switch (CalamityWorld.DraedonMechToSummon)
            {
                // Summon Thanatos underground.
                case ExoMech.Destroyer:
                    Vector2 thanatosSpawnPosition = playerToFollow.Center + Vector2.UnitY * 2100f;
                    NPC thanatos = CalamityUtils.SpawnBossBetter(thanatosSpawnPosition, ModContent.NPCType<ThanatosHead>());
                    if (thanatos != null)
                        thanatos.velocity = thanatos.SafeDirectionTo(playerToFollow.Center) * 40f;
                    break;

                // Summon Ares in the sky, directly above the player.
                case ExoMech.Prime:
                    Vector2 aresSpawnPosition = playerToFollow.Center - Vector2.UnitY * 1400f;
                    CalamityUtils.SpawnBossBetter(aresSpawnPosition, ModContent.NPCType<AresBody>());
                    break;

                // Summon Apollo and Artemis above the player to their sides.
                case ExoMech.Twins:
                    Vector2 artemisSpawnPosition = playerToFollow.Center + new Vector2(-1100f, -1600f);
                    Vector2 apolloSpawnPosition = playerToFollow.Center + new Vector2(1100f, -1600f);
                    CalamityUtils.SpawnBossBetter(artemisSpawnPosition, ModContent.NPCType<Artemis>());
                    CalamityUtils.SpawnBossBetter(apolloSpawnPosition, ModContent.NPCType<Apollo>());
                    break;

            }
        }

        public static void HandleDefeatStuff(NPC npc, Player playerToFollow, ref float defeatTimer)
        {
            // Become vulnerable after being defeated after a certain point.
            bool hasBeenKilled = npc.localAI[2] == 1f;
            ref float hologramEffectTimer = ref npc.localAI[1];
            npc.dontTakeDamage = defeatTimer < TalkDelay * 2f + 50f || hasBeenKilled;
            npc.Calamity().CanHaveBossHealthBar = !npc.dontTakeDamage;
            npc.Calamity().ShouldCloseHPBar = hasBeenKilled;

            bool leaving = defeatTimer > DelayBeforeDefeatStandup + TalkDelay * 8f + 200f;

            // Fade away and disappear when leaving.
            if (leaving)
            {
                hologramEffectTimer = MathHelper.Clamp(hologramEffectTimer - 1f, 0f, HologramFadeinTime);
                if (hologramEffectTimer <= 0f)
                    npc.active = false;
            }

            // Fade back in as a hologram if the player tried to kill Draedon.
            else if (hasBeenKilled)
                hologramEffectTimer = MathHelper.Clamp(hologramEffectTimer + 1f, 0f, HologramFadeinTime - 5f);

            // Adjust opacity.
            npc.Opacity = hologramEffectTimer / HologramFadeinTime;
            if (hasBeenKilled)
                npc.Opacity *= 0.67f;

            // Stand up in awe after a small amount of time has passed.
            if (defeatTimer > DelayBeforeDefeatStandup && defeatTimer < TalkDelay * 3f + 50f)
                npc.ModNPC<DraedonNPC>().ShouldStartStandingUp = true;

            if (defeatTimer == DelayBeforeDefeatStandup + 50f)
                Utilities.DisplayText("Intriguing. Truly, intriguing.", TextColor);

            if (defeatTimer == DelayBeforeDefeatStandup + TalkDelay + 50f)
                Utilities.DisplayText("My magnum opera, truly and utterly defeated.", TextColor);

            if (defeatTimer == DelayBeforeDefeatStandup + TalkDelay * 2f + 50f)
                Utilities.DisplayText("This outcome was not what I had expected.", TextColor);

            // After this point Draedon becomes vulnerable.
            // He sits back down as well as he thinks for a bit.
            // Killing him will cause gore to appear but also for Draedon to come back as a hologram.
            if (defeatTimer == DelayBeforeDefeatStandup + TalkDelay * 3f + 50f)
                Utilities.DisplayText("...Excuse my introspection. I must gather my thoughts after that display.", TextColor);

            if (defeatTimer == DelayBeforeDefeatStandup + TalkDelay * 3f + 165f)
                Utilities.DisplayText("It is perhaps not irrational to infer that you are beyond my reasoning.", TextColor);

            if (defeatTimer == DelayBeforeDefeatStandup + TalkDelay * 4f + 165f)
                Utilities.DisplayText("Now.", TextColor);

            if (defeatTimer == DelayBeforeDefeatStandup + TalkDelay * 5f + 165f)
                Utilities.DisplayText("You would wish to reach the Tyrant. I cannot assist you in that.", TextColor);

            if (defeatTimer == DelayBeforeDefeatStandup + TalkDelay * 6f + 165f)
                Utilities.DisplayText("It is not a matter of spite, for I would wish nothing more than to observe such a conflict.", TextColor);

            if (defeatTimer == DelayBeforeDefeatStandup + TalkDelay * 7f + 165f)
                Utilities.DisplayText("But now, I must return to my machinery. You may use the Codebreaker if you wish to face my creations once again.", TextColor);

            if (defeatTimer == DelayBeforeDefeatStandup + TalkDelay * 8f + 165f)
                Utilities.DisplayText("In the meantime, I bid you farewell, and good luck in your future endeavors.", TextColor);
        }

        public override void FindFrame(NPC npc, int frameHeight)
        {
            npc.frame.Width = 100;

            int xFrame = npc.frame.X / npc.frame.Width;
            int yFrame = npc.frame.Y / frameHeight;
            int frame = xFrame * Main.npcFrameCount[npc.type] + yFrame;

            // Prepare to stand up if called for and not already doing so.
            if (npc.ModNPC<DraedonNPC>().ShouldStartStandingUp && frame > 23)
                frame = 0;

            int frameChangeDelay = 7;
            bool shouldNotSitDown = npc.ModNPC<DraedonNPC>().DefeatTimer > DelayBeforeDefeatStandup && npc.ModNPC<DraedonNPC>().DefeatTimer < TalkDelay * 3f + 10f;

            npc.frameCounter++;
            if (npc.frameCounter >= frameChangeDelay)
            {
                frame++;

                if (!npc.ModNPC<DraedonNPC>().ShouldStartStandingUp && (frame < 23 || frame > 47))
                    frame = 23;

                // Do the sit animation infinitely if Draedon should not sit down again.
                if (shouldNotSitDown && frame >= 16)
                    frame = 11;

                if (frame >= 23 && npc.ModNPC<DraedonNPC>().ShouldStartStandingUp)
                {
                    frame = 0;
                    npc.ModNPC<DraedonNPC>().ShouldStartStandingUp = false;
                }

                npc.frameCounter = 0;
            }

            npc.frame.X = frame / Main.npcFrameCount[npc.type] * npc.frame.Width;
            npc.frame.Y = frame % Main.npcFrameCount[npc.type] * frameHeight;
        }
    }
}
