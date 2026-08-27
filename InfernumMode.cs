using InfernumMode.Schematics;
using InfernumMode.Effects;
using InfernumMode.Graphics.Primitives;
using InfernumMode.Graphics.Interfaces;
using CalamityMod.Events;
using CalamityMod.CalPlayer;
using CalamityMod.Waters;
using CalamityMod.NPCs;
using CalamityMod.NPCs.ExoMechs;
using CalamityMod.NPCs.ExoMechs.Apollo;
using CalamityMod.NPCs.ExoMechs.Ares;
using CalamityMod.NPCs.ExoMechs.Thanatos;
using CalamityMod.NPCs.ProfanedGuardians;
using CalamityMod.NPCs.Providence;
using CalamityMod.NPCs.SupremeCalamitas;
using CalamityMod.Particles;
using InfernumMode.Balancing;
using InfernumMode.BehaviorOverrides.BossAIs.Cryogen;
using InfernumMode.BehaviorOverrides.BossAIs.DoG;
using InfernumMode.BehaviorOverrides.BossAIs.Draedon;
using InfernumMode.BehaviorOverrides.BossAIs.MoonLord;
using InfernumMode.BehaviorOverrides.BossAIs.Providence;
using InfernumMode.BehaviorOverrides.BossAIs.Twins;
using InfernumMode.BossIntroScreens;
using InfernumMode.BossRush;
using InfernumMode.ILEditingStuff;
using InfernumMode.Items;
using InfernumMode.OverridingSystem;
using InfernumMode.Skies;
using InfernumMode.GlobalInstances;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.IO;
using System;
using System.Linq;
using System.Reflection;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;
using InfernumMode.Projectiles;
using InfernumMode;
using InfernumMode.Particles;
using CalamityMod.World;

using static CalamityMod.CalamityMod;
using InfernumMode.TrackedMusic;
using InfernumMode.UI;
using InfernumMode.BehaviorOverrides.BossAIs.AquaticScourge;

namespace InfernumMode
{
    public class InfernumMode : Mod
    {
		
		private const float Epsilon = 5E-6f;
		private const float OutOfSelectionDimFactor = 0.06f;
		private static readonly Color BaseGridColor = new Color(0.24f, 0.8f, 0.9f, 0.5f);
		private static readonly Rectangle TexUpperHalfRect = new Rectangle(0, 0, 18, 18);
		
        internal static InfernumMode Instance = null;

        internal static Mod CalamityMod = null;

        internal static Mod FargosMutantMod;
        internal static Mod YABHBMod;
		
        internal static bool CanUseCustomAIs => (!BossRushEvent.BossRushActive || BossRushApplies) && PoDWorld.InfernumMode;

        internal static bool BossRushApplies => true;

        internal static readonly Color HiveMindSkyColor = new Color(53, 42, 82);

        internal static List<CustomLavaStyle> CustomLavaStyles
        {
            get => (List<CustomLavaStyle>)typeof(CustomLavaManagement).GetField("CustomLavaStyles", Utilities.UniversalBindingFlags).GetValue(null);
            set => typeof(CustomLavaManagement).GetField("CustomLavaStyles", Utilities.UniversalBindingFlags).SetValue(null, value);
        }
		
        internal static MethodInfo LoadMethod = typeof(CustomLavaStyle).GetMethod("Load", Utilities.UniversalBindingFlags);		
		
        public static float BlackFade
        {
            get;
            set;
        } = 0f;

        public static float DraedonThemeTimer
        {
            get;
            set;
        } = 0f;

        public static float ProvidenceArenaTimer
        {
            get;
            set;
        }
			
		
        public override void Load()
        {
            Instance = this;
            CalamityMod = ModLoader.GetMod("CalamityMod");
            FargosMutantMod = ModLoader.GetMod("Fargowiltas");
            YABHBMod = ModLoader.GetMod("FKBossHealthBar");
			
			InfernumFusableParticleManager.LoadParticleRenderSets();
			if (!Main.dedServ)			
				Main.OnPreDraw += PrepareRenderTargets;		
			


            OverridingListManager.Load();
            BalancingChangesManager.Load();
            HookManager.Load();	

			ProjectileSpawnManagementSystem.Load();

            // Manually invoke the attribute constructors to get the marked methods cached.
            foreach (var type in typeof(InfernumMode).Assembly.GetTypes())
            {
                foreach (var method in type.GetMethods(Utilities.UniversalBindingFlags))
                    method.GetCustomAttributes(false);
            }

            IntroScreenManager.Load();
            NPCBehaviorOverride.LoadAll();
            ProjectileBehaviorOverride.LoadAll();

            if (Main.netMode != NetmodeID.Server)
            {
                AddBossHeadTexture("InfernumMode/BehaviorOverrides/BossAIs/Cryogen/CryogenMapIcon", -1);
				
                AddBossHeadTexture("InfernumMode/BehaviorOverrides/BossAIs/SupremeCalamitas/SepulcherMapIcon", -1);
				
                // Calamitas' Shadow.
                AddBossHeadTexture("InfernumMode/BehaviorOverrides/BossAIs/CalamitasShadow/CalShadowMapIcon", -1);
                AddBossHeadTexture("InfernumMode/BehaviorOverrides/BossAIs/CalamitasShadow/CataclysmMapIcon", -1);
                AddBossHeadTexture("InfernumMode/BehaviorOverrides/BossAIs/CalamitasShadow/CatastropheMapIcon", -1);	

				AddBossHeadTexture("InfernumMode/BehaviorOverrides/BossAIs/DoG/DoGP1HeadMapIcon", -1);
                AddBossHeadTexture("InfernumMode/BehaviorOverrides/BossAIs/DoG/DoGP1TailMapIcon", -1);
                AddBossHeadTexture("InfernumMode/BehaviorOverrides/BossAIs/DoG/DoGP2HeadMapIcon", -1);
                AddBossHeadTexture("InfernumMode/BehaviorOverrides/BossAIs/DoG/DoGP2BodyMapIcon", -1);
                AddBossHeadTexture("InfernumMode/BehaviorOverrides/BossAIs/DoG/DoGP2TailMapIcon", -1);				

				InfernumEffectsRegistry.LoadEffects();	
				ScreenEffectSystem.Load();				

            }

            if (BossRushApplies)
                BossRushChanges.Load();

            if (Main.netMode != NetmodeID.Server)
                GeneralParticleHandler.LoadModParticleInstances(this);
            InfernumSchematicManager.Load();
			
			On.Terraria.Graphics.Effects.FilterManager.EndCapture += EndCaptureManager;

			if (CustomLavaStyles is null)
				CustomLavaStyles = new List<CustomLavaStyle>();
            foreach (Type type in typeof(InfernumMode).Assembly.GetTypes())
            {
                // Ignore abstract types; they cannot have instances.
                // Also ignore types which do not derive from CustomLavaStyle.
                if (!type.IsSubclassOf(typeof(CustomLavaStyle)) || type.IsAbstract)
                    continue;

                CustomLavaStyles.Add(Activator.CreateInstance(type) as CustomLavaStyle);
                LoadMethod.Invoke(CustomLavaStyles.Last(), new object[0]);
			}
        }
		
        internal static IDictionary<int, int> SoundLoaderMusicToItem => (IDictionary<int, int>)typeof(SoundLoader).GetField("musicToItem", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
        internal static IDictionary<int, int> SoundLoaderItemToMusic => (IDictionary<int, int>)typeof(SoundLoader).GetField("itemToMusic", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
        internal static IDictionary<int, IDictionary<int, int>> SoundLoaderTileToMusic => (IDictionary<int, IDictionary<int, int>>)typeof(SoundLoader).GetField("tileToMusic", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
		
		
		public override void PostSetupContent()
		{
			TrackedMusicManager.Initialize();
		}
		
		
        public override void UpdateMusic(ref int music, ref MusicPriority priority)
        {
            if (Main.musicVolume != 0)
            {
                if (Main.myPlayer != -1 && !Main.gameMenu && Main.LocalPlayer.active)
                {
                    Player p = Main.LocalPlayer;
                    if (p.InProfaned())
                    {
                        if (!CalamityPlayer.areThereAnyDamnBosses)
                        {
                            music = Instance.GetSoundSlot(SoundType.Music, "Sounds/Music/ProfanedTemple");
                            priority = MusicPriority.Environment;
                        }
                    }
						
					if (NPC.AnyNPCs(NPCID.EyeofCthulhu))
					{
						music = Instance.GetSoundSlot(SoundType.Music, "Sounds/Music/EyeOfCthulhu");
						priority = MusicPriority.BossLow;
					}

					if (NPC.AnyNPCs(NPCID.SkeletronHead))
					{
						music = Instance.GetSoundSlot(SoundType.Music, "Sounds/Music/Boss3");
						priority = MusicPriority.BossLow;
					}

					if (NPC.AnyNPCs(NPCID.SkeletronPrime) || NPC.AnyNPCs(NPCID.Retinazer) || NPC.AnyNPCs(NPCID.Spazmatism) || NPC.AnyNPCs(NPCID.TheDestroyer))
					{
						music = Instance.GetSoundSlot(SoundType.Music, "Sounds/Music/MechBosses");
						priority = MusicPriority.BossLow;
					}

					if (NPC.AnyNPCs(NPCID.DukeFishron))
					{
						music = Instance.GetSoundSlot(SoundType.Music, "Sounds/Music/DukeFishron");
						priority = MusicPriority.BossMedium;
					}

					if (NPC.AnyNPCs(NPCID.CultistBoss))
					{
						music = Instance.GetSoundSlot(SoundType.Music, "Sounds/Music/LunaticCultist");
						priority = MusicPriority.BossMedium;
					}

					int moonLordIndex = NPC.FindFirstNPC(NPCID.MoonLordCore);
					if (moonLordIndex != -1)
					{
						NPC moonLord = Main.npc[moonLordIndex];

						music = Instance.GetSoundSlot(SoundType.Music, "Sounds/Music/MoonLord");
						if (moonLord.Infernum().ExtraAI[10] < MoonLordCoreBehaviorOverride.IntroSoundLength)
							music = 0;
						Main.musicFade[Main.curMusic] = 1f;
						priority = MusicPriority.BossHigh;
					}

					if (DoGPhase2HeadBehaviorOverride.InPhase2)
					{
						music = (CalamityMod as CalamityMod.CalamityMod).GetMusicFromMusicMod("DevourerOfGodsP2") ?? MusicID.LunarBoss;
                        priority = MusicPriority.BossHigh;
					}

					bool areExoMechsAround = NPC.AnyNPCs(ModContent.NPCType<AresBody>()) ||
						NPC.AnyNPCs(ModContent.NPCType<ThanatosHead>()) ||
						NPC.AnyNPCs(ModContent.NPCType<Apollo>());

					if (areExoMechsAround)
					{
						int draedon = NPC.FindFirstNPC(ModContent.NPCType<Draedon>());
						if (draedon >= 0 && Main.npc[draedon].Infernum().ExtraAI[0] < DraedonBehaviorOverride.IntroSoundLength)
							music = 0;
						else
							music = Instance.GetSoundSlot(SoundType.Music, "Sounds/Music/ExoMechBosses");
						priority = MusicPriority.BossHigh;
					}
					
					bool areGuardiansAround = NPC.AnyNPCs(ModContent.NPCType<ProfanedGuardianBoss>()) ||
						NPC.AnyNPCs(ModContent.NPCType<ProfanedGuardianBoss2>()) ||
						NPC.AnyNPCs(ModContent.NPCType<ProfanedGuardianBoss3>());
						
					if (areGuardiansAround)
					{
						music = (CalamityMod as CalamityMod.CalamityMod).GetMusicFromMusicMod("Guardians") ?? MusicID.LunarBoss;
                        priority = MusicPriority.BossHigh;
					}
                    int providenceIndex = NPC.FindFirstNPC(ModContent.NPCType<Providence>());
                    if (providenceIndex >= 0 && CanUseCustomAIs)
                    {
                        NPC providence = Main.npc[providenceIndex];
                        bool providencePhase2 = providence.life < providence.lifeMax * ProvidenceBehaviorOverride.Phase2LifeRatio;

                        music = providencePhase2
                            ? (CalamityMod as CalamityMod.CalamityMod).GetMusicFromMusicMod("Providence") ?? MusicID.LunarBoss
                            : (CalamityMod as CalamityMod.CalamityMod).GetMusicFromMusicMod("Guardians") ?? MusicID.Boss1;
                        priority = MusicPriority.BossHigh;
                    }

					if (DraedonThemeTimer > 0f)
					{
						DraedonThemeTimer++;
						if (DraedonThemeTimer >= DraedonBehaviorOverride.PostBattleMusicLength)
							DraedonThemeTimer = 0f;
						else
							music = Instance.GetSoundSlot(SoundType.Music, "Sounds/Music/Draedon");
						priority = MusicPriority.BossHigh;
					}
					
				}
			}
        }		
		
        public override void PostUpdateEverything()
        {
            // Disable natural GSS spawns.
            if (CanUseCustomAIs)
                sharkKillCount = 0;
            BossRushChanges.HandleTeleports();
            if (!NPC.AnyNPCs(ModContent.NPCType<Draedon>()))
                CalamityGlobalNPC.draedon = -1;
            ScreenEffectSystem.Update();

			// ---- TrackedMusic ----
			if (Main.netMode != NetmodeID.Server)
			{
				if (Main.gameMenu)
				{
					TrackedMusicManager.OnEnterMainMenu();
				}
				else
				{
					TrackedMusicManager.Update();

					int cur = Main.curMusic;
					if (cur >= 0 && cur < Main.musicFade.Length)
					{
						float fade = Main.musicFade[cur];
						TrackedMusicManager.OnMusicSlotActive(cur, ref fade);
						Main.musicFade[cur] = fade;
					}
				}
			}
			
        }
        
        public override void HandlePacket(BinaryReader reader, int whoAmI) => NetcodeHandler.ReceivePacket(this, reader, whoAmI);

        public override void AddRecipes() => RecipeUpdates.Update();

        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
			layers.Insert(0, new LegacyGameInterfaceLayer("Prov Arena Selection Grid", RenderSchematicSelectionGrid));
			
            int mouseIndex = layers.FindIndex(layer => layer.Name == "Vanilla: Mouse Text");
            if (mouseIndex != -1)
            {
                layers.Insert(mouseIndex, new LegacyGameInterfaceLayer("Boss Introduction Screens", () =>
                {
                    IntroScreenManager.Draw();
                    return true;
                }, InterfaceScaleType.None));
				
				layers.Insert(mouseIndex, new LegacyGameInterfaceLayer("Guardians Plaque UI", () =>
                {
                    GuardiansPlaqueUIManager.Draw(Main.spriteBatch);
                    return true;
                }, InterfaceScaleType.UI));
            }
        }
		
		private static bool RenderSchematicSelectionGrid()
		{
			Texture2D gridSquareTex = Main.extraTexture[68];
			Rectangle? rectNull = Main.LocalPlayer.Infernum().SelectedProvidenceArena;
			if (!rectNull.HasValue)
				return true;
			Rectangle selection = rectNull.Value;

			Vector2 topLeftScreenTile = (Main.screenPosition / 16f).Floor();
			for (int i = 0; i <= Main.screenWidth; i += 16)
			{
				for (int j = 0; j <= Main.screenHeight; j += 16)
				{
					Vector2 offset = new Vector2(i >> 4, j >> 4);
					Vector2 gridTilePos = topLeftScreenTile + offset;
					Point gridTilePoint = new Point((int)(gridTilePos.X + Epsilon), (int)(gridTilePos.Y + Epsilon));
					bool inSelection = selection.Contains(gridTilePoint);
					Color gridColor = BaseGridColor * (inSelection ? 1f : OutOfSelectionDimFactor);
					Main.spriteBatch.Draw(gridSquareTex, gridTilePos * 16f - Main.screenPosition, TexUpperHalfRect, gridColor, 0f, Vector2.Zero, 1f, 0, 0f);
				}
			}
			return true;
		}

		private void EndCaptureManager(On.Terraria.Graphics.Effects.FilterManager.orig_EndCapture orig, FilterManager self)
        {
            orig(self);
        }
		
        public override void PreUpdateEntities()
        {
            InfernumMode.BlackFade = MathHelper.Clamp(InfernumMode.BlackFade - 0.025f, 0f, 1f);
            NetcodeHandler.Update();
            TwinsAttackSynchronizer.DoUniversalUpdate();
            TwinsAttackSynchronizer.PostUpdateEffects();
            if (CalamityWorld.death)
                CalamityWorld.revenge = true;

            bool arenaShouldApply = Utilities.AnyProjectiles(ModContent.ProjectileType<ProvidenceSummonerProjectile>()) || NPC.AnyNPCs(ModContent.NPCType<Providence>());
            InfernumMode.ProvidenceArenaTimer = MathHelper.Clamp(InfernumMode.ProvidenceArenaTimer + arenaShouldApply.ToDirectionInt(), 0f, 120f);
            if (Main.netMode != NetmodeID.MultiplayerClient && InfernumMode.ProvidenceArenaTimer > 0 && !Utilities.AnyProjectiles(ModContent.ProjectileType<ProvidenceArenaBorder>()))
                Utilities.NewProjectileBetter(Vector2.One * 9999f, Vector2.Zero, ModContent.ProjectileType<ProvidenceArenaBorder>(), 0, 0f);
        }

        public override bool HijackSendData(int whoAmI, int msgType, int remoteClient, int ignoreClient, NetworkText text, int number, float number2, float number3, float number4, int number5, int number6, int number7)
        {
            if (msgType == MessageID.SyncNPC)
            {
                NPC npc = Main.npc[number];
                if (!npc.active)
                    return base.HijackSendData(whoAmI, msgType, remoteClient, ignoreClient, text, number, number2, number3, number4, number5, number6, number7);

                ModPacket packet = InfernumMode.Instance.GetPacket();
                packet.Write((short)InfernumPacketType.SendExtraNPCData);
                packet.Write(npc.whoAmI);
                packet.Write(npc.realLife);
                packet.Write(npc.Infernum().TotalAISlotsInUse);
                packet.Write(npc.Infernum().Arena.X);
                packet.Write(npc.Infernum().Arena.Y);
                packet.Write(npc.Infernum().Arena.Width);
                packet.Write(npc.Infernum().Arena.Height);
                for (int i = 0; i < npc.Infernum().ExtraAI.Length; i++)
                {
                    if (!npc.Infernum().HasAssociatedAIBeenUsed[i])
                        continue;
                    packet.Write(i);
                    packet.Write(npc.Infernum().ExtraAI[i]);
                }
                packet.Send();
            }
            return base.HijackSendData(whoAmI, msgType, remoteClient, ignoreClient, text, number, number2, number3, number4, number5, number6, number7);
        }

        public override object Call(params object[] args)
        {
            return InfernumModCalls.Call(args);
        }

        public override void Unload()
        {
            IntroScreenManager.Unload();
            OverridingListManager.Unload();
            BalancingChangesManager.Unload();	
            HookManager.Unload();
            Instance = null;
            CalamityMod = null;
			InfernumFusableParticleManager.UnloadParticleRenderSets();
			Main.OnPreDraw -= PrepareRenderTargets;	
            InfernumSchematicManager.Unload();		
            Primitive3DStrip.Dispose();
			ProjectileSpawnManagementSystem.Unload();			
			ScreenEffectSystem.Unload();
			PrimitiveTrailCopy.Dispose();	
			On.Terraria.Graphics.Effects.FilterManager.EndCapture -= EndCaptureManager;
			On.Terraria.GameContent.Liquid.LiquidRenderer.InternalDraw += WaterClearingBubble.PrepareWater;
        }
		
        #region Fusable Particle Updating
        public override void MidUpdateProjectileItem()
        {
            // Update all fusable particles.
            // These are really only visual and as such don't really need any complex netcode.
            foreach (InfernumBaseFusableParticleSet.FusableParticleRenderCollection particleSet in InfernumFusableParticleManager.ParticleSets)
            {
                foreach (InfernumBaseFusableParticleSet.FusableParticle particle in particleSet.ParticleSet.Particles)
                    particleSet.ParticleSet.UpdateBehavior(particle);
            }
        }
        #endregion
		
		#region Render Target Management
        public static void PrepareRenderTargets(GameTime gameTime)
        {
            InfernumFusableParticleManager.PrepareFusableParticleTargets();
            DeathAshParticle.PrepareRenderTargets();
        }
        #endregion Render Target Management
    }
}