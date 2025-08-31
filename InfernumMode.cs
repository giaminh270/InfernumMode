using CalamityMod.Schematics;
using CalamityMod.Events;
using CalamityMod.NPCs.ExoMechs;
using CalamityMod.NPCs.ExoMechs.Apollo;
using CalamityMod.NPCs.ExoMechs.Ares;
using CalamityMod.NPCs.ExoMechs.Thanatos;
using CalamityMod.NPCs.Providence;
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
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;
using InfernumMode;
using InfernumMode.Particles;
using CalamityMod.World;

using static CalamityMod.CalamityMod;

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
	
        internal static bool CanUseCustomAIs => (!BossRushEvent.BossRushActive || BossRushApplies) && PoDWorld.InfernumMode;

        internal static bool BossRushApplies => false;

        internal static readonly Color HiveMindSkyColor = new Color(53, 42, 82);

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
		internal static Dictionary<string, SchematicMetaTile[,]> TileMaps =>
            typeof(SchematicManager).GetField("TileMaps", Utilities.UniversalBindingFlags).GetValue(null) as Dictionary<string, SchematicMetaTile[,]>;
			
		internal static readonly MethodInfo ImportSchematicMethod = typeof(CalamitySchematicIO).GetMethod("ImportSchematic", Utilities.UniversalBindingFlags);	
			
        public override void Load()
        {
            // However, render targets and certain other graphical objects can only be created on the main thread.
			
            Instance = this;
            CalamityMod = ModLoader.GetMod("CalamityMod");
			
			InfernumFusableParticleManager.LoadParticleRenderSets();
			Main.OnPreDraw += PrepareRenderTargets;		
			


            OverridingListManager.Load();
            BalancingChangesManager.Load();
            HookManager.Load();

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

                Ref<Effect> madnessShader = new Ref<Effect>(GetEffect("Effects/Madness"));
                Filters.Scene["InfernumMode:Madness"] = new Filter(new MadnessScreenShaderData(madnessShader, "DyePass"), EffectPriority.VeryHigh);

                Ref<Effect> aewPsychicEnergyShader = new Ref<Effect>(GetEffect("Effects/AEWPsychicDistortionShader"));
                GameShaders.Misc["Infernum:AEWPsychicEnergy"] = new MiscShaderData(aewPsychicEnergyShader, "DistortionPass");
				
                Ref<Effect> BaseFusableParticleEdgeShader = new Ref<Effect>(GetEffect("Effects/ParticleFusion/InfernumBaseFusableParticleEdgeShader"));
                GameShaders.Misc["Infernum:BaseFusableParticleEdge"] = new MiscShaderData(BaseFusableParticleEdgeShader, "ParticlePass");
				
				Ref<Effect> AdditiveFusableParticleEdgeShader = new Ref<Effect>(GetEffect("Effects/ParticleFusion/InfernumBaseFusableParticleEdgeShader"));
                GameShaders.Misc["Infernum:AdditiveFusableParticleEdge"] = new MiscShaderData(AdditiveFusableParticleEdgeShader, "ParticlePass");

                Ref<Effect> gradientShader = new Ref<Effect>(GetEffect("Effects/GradientWingShader"));
                GameShaders.Misc["Infernum:GradientWingShader"] = new MiscShaderData(gradientShader, "GradientPass");

                Ref<Effect> cyclicHueShader = new Ref<Effect>(GetEffect("Effects/CyclicHueShader"));
                GameShaders.Misc["Infernum:CyclicHueShader"] = new MiscShaderData(cyclicHueShader, "OutlineShader");

                Ref<Effect> pristineArmorShader = new Ref<Effect>(GetEffect("Effects/PristineArmorShader"));
                GameShaders.Misc["Infernum:PristineArmorShader"] = new MiscShaderData(pristineArmorShader, "PristinePass");

                Ref<Effect> dukeTornadoShader = new Ref<Effect>(GetEffect("Effects/DukeTornado"));
                GameShaders.Misc["Infernum:DukeTornado"] = new MiscShaderData(dukeTornadoShader, "TrailPass");

                Ref<Effect> tentacleFleshShader = new Ref<Effect>(GetEffect("Effects/TentacleTexture"));
                GameShaders.Misc["Infernum:WoFTentacleTexture"] = new MiscShaderData(tentacleFleshShader, "TrailPass");

                Ref<Effect> bloodGeyserShader = new Ref<Effect>(GetEffect("Effects/BloodGeyser"));
                GameShaders.Misc["Infernum:WoFGeyserTexture"] = new MiscShaderData(bloodGeyserShader, "TrailPass");

                Ref<Effect> shadowflameShader = new Ref<Effect>(GetEffect("Effects/Shadowflame"));
                GameShaders.Misc["Infernum:Fire"] = new MiscShaderData(shadowflameShader, "TrailPass");

                Ref<Effect> brainPsychicShader = new Ref<Effect>(GetEffect("Effects/BrainPsychicShader"));
                GameShaders.Misc["Infernum:BrainPsychic"] = new MiscShaderData(brainPsychicShader, "TrailPass");

                Ref<Effect> cultistDeathAnimationShader = new Ref<Effect>(GetEffect("Effects/CultistDeathAnimation"));
                GameShaders.Misc["Infernum:CultistDeath"] = new MiscShaderData(cultistDeathAnimationShader, "DeathPass");

                Ref<Effect> flameTrailShader = new Ref<Effect>(GetEffect("Effects/TwinsFlameTail"));
                GameShaders.Misc["Infernum:TwinsFlameTrail"] = new MiscShaderData(flameTrailShader, "TrailPass");

                Ref<Effect> aresLightningArcShader = new Ref<Effect>(GetEffect("Effects/AresLightningArcShader"));
                GameShaders.Misc["Infernum:AresLightningArc"] = new MiscShaderData(aresLightningArcShader, "TrailPass");

                Ref<Effect> ghostlyShader = new Ref<Effect>(GetEffect("Effects/EidolicWailRingShader"));
                GameShaders.Misc["Infernum:PolterghastEctoplasm"] = new MiscShaderData(ghostlyShader, "BurstPass");

                ghostlyShader = new Ref<Effect>(GetEffect("Effects/NecroplasmicRoarShader"));
                GameShaders.Misc["Infernum:NecroplasmicRoar"] = new MiscShaderData(ghostlyShader, "BurstPass");

                Ref<Effect> backgroundShader = new Ref<Effect>(GetEffect("Effects/MoonLordBGDistortionShader"));
                GameShaders.Misc["Infernum:MoonLordBGDistortion"] = new MiscShaderData(backgroundShader, "DistortionPass");

                Ref<Effect> introShader = new Ref<Effect>(GetEffect("Effects/MechIntroLetterShader"));
                GameShaders.Misc["Infernum:MechsIntro"] = new MiscShaderData(introShader, "LetterPass");

                introShader = new Ref<Effect>(GetEffect("Effects/SCalIntroLetterShader"));
                GameShaders.Misc["Infernum:SCalIntro"] = new MiscShaderData(introShader, "LetterPass");

                Ref<Effect> rayShader = new Ref<Effect>(GetEffect("Effects/PrismaticRayShader"));
                GameShaders.Misc["Infernum:PrismaticRay"] = new MiscShaderData(rayShader, "TrailPass");

                Ref<Effect> darkFlamePillarShader = new Ref<Effect>(GetEffect("Effects/DarkFlamePillarShader"));
                GameShaders.Misc["Infernum:DarkFlamePillar"] = new MiscShaderData(darkFlamePillarShader, "TrailPass");

                Ref<Effect> artemisLaserShader = new Ref<Effect>(GetEffect("Effects/ArtemisLaserShader"));
                GameShaders.Misc["Infernum:ArtemisLaser"] = new MiscShaderData(artemisLaserShader, "TrailPass");

                Ref<Effect> realityTearShader = new Ref<Effect>(GetEffect("Effects/RealityTearShader"));
                GameShaders.Misc["Infernum:RealityTear"] = new MiscShaderData(realityTearShader, "TrailPass");

                realityTearShader = new Ref<Effect>(GetEffect("Effects/RealityTear2Shader"));
                GameShaders.Misc["Infernum:RealityTear2"] = new MiscShaderData(realityTearShader, "TrailPass");

                Ref<Effect> hologramShader = new Ref<Effect>(GetEffect("Effects/HologramShader"));
                GameShaders.Misc["Infernum:Hologram"] = new MiscShaderData(hologramShader, "HologramPass");

                Ref<Effect> matrixShader = new Ref<Effect>(GetEffect("Effects/LocalLinearTransformationShader"));
                GameShaders.Misc["Infernum:LinearTransformation"] = new MiscShaderData(matrixShader, "TransformationPass");

				Ref<Effect> cutoutShader = new Ref<Effect>(GetEffect("Effects/CircleCutoutShader"));
                GameShaders.Misc["Infernum:CircleCutout"] = new MiscShaderData(cutoutShader, "CutoutPass");

				cutoutShader = new Ref<Effect>(GetEffect("Effects/CircleCutoutShader2"));
                GameShaders.Misc["Infernum:CircleCutout2"] = new MiscShaderData(cutoutShader, "CutoutPass");

				Ref<Effect> streakShader = new Ref<Effect>(GetEffect("Effects/SideStreakTrail"));
                GameShaders.Misc["Infernum:SideStreak"] = new MiscShaderData(streakShader, "TrailPass");

				Ref<Effect> yharonBurnShader = new Ref<Effect>(GetEffect("Effects/YharonBurnShader"));
                GameShaders.Misc["Infernum:YharonBurn"] = new MiscShaderData(yharonBurnShader, "BurnPass");

                // Screen shaders.

                Filters.Scene["InfernumMode:HiveMind"] = new Filter(new HiveMindScreenShaderData("FilterMiniTower").UseColor(HiveMindSkyColor).UseOpacity(0.6f), EffectPriority.VeryHigh);
                SkyManager.Instance["InfernumMode:HiveMind"] = new HiveMindSky();

                Filters.Scene["InfernumMode:Perforators"] = new Filter(new PerforatorScreenShaderData("FilterMiniTower").UseColor(new Color(255, 60, 30)).UseOpacity(0.445f), EffectPriority.VeryHigh);
                SkyManager.Instance["InfernumMode:Perforators"] = new PerforatorSky();

                Filters.Scene["InfernumMode:Dragonfolly"] = new Filter(new DragonfollyScreenShaderData("FilterMiniTower").UseColor(Color.Red).UseOpacity(0.6f), EffectPriority.VeryHigh);
                SkyManager.Instance["InfernumMode:Dragonfolly"] = new DragonfollySky();

                Filters.Scene["InfernumMode:Deus"] = new Filter(new DeusScreenShaderData("FilterMiniTower").UseColor(Color.Lerp(Color.Purple, Color.Black, 0.75f)).UseOpacity(0.24f), EffectPriority.VeryHigh);
                SkyManager.Instance["InfernumMode:Deus"] = new DeusSky();

                Filters.Scene["InfernumMode:NightProvidence"] = new Filter(new NightProvidenceShaderData("FilterMiniTower").UseOpacity(0.67f), EffectPriority.VeryHigh);
                SkyManager.Instance["InfernumMode:NightProvidence"] = new NightProvidenceSky();

                Filters.Scene["InfernumMode:OldDuke"] = new Filter(new OldDukeScreenShaderData("FilterMiniTower").UseOpacity(0.6f), EffectPriority.VeryHigh);
                SkyManager.Instance["InfernumMode:OldDuke"] = new OldDukeSky();

                Filters.Scene["InfernumMode:DoG"] = new Filter(new PerforatorScreenShaderData("FilterMiniTower").UseColor(0.4f, 0.1f, 1.0f).UseOpacity(0.5f), EffectPriority.VeryHigh);
                SkyManager.Instance["InfernumMode:DoG"] = new DoGSkyInfernum();

                Ref<Effect> scalScreenShader = new Ref<Effect>(GetEffect("Effects/SCalFireBGShader"));
                Filters.Scene["InfernumMode:SCal"] = new Filter(new SCalScreenShaderData(scalScreenShader, "DyePass").UseColor(0.3f, 0f, 0f).UseOpacity(0.5f), EffectPriority.VeryHigh);
                SkyManager.Instance["InfernumMode:SCal"] = new SCalSkyInfernum();

                SkyManager.Instance["InfernumMode:Madness"] = new MadnessSky();
            }

            if (BossRushApplies)
                BossRushChanges.Load();

            if (Main.netMode != NetmodeID.Server)
                GeneralParticleHandler.LoadModParticleInstances(this);
			
            //TileMaps["Profaned Arena"] = LoadInfernumSchematic("Schematics/ProfanedArena.csch");
        }
		
		/*public static SchematicMetaTile[,] LoadInfernumSchematic(string filename)
        {
            SchematicMetaTile[,] ret = null;
            using (Stream st = InfernumMode.Instance.GetFileStream(filename, true))
                ret = (SchematicMetaTile[,])ImportSchematicMethod.Invoke(null, new object[] { st });

            return ret;
        }*/

        public override void PostUpdateEverything()
        {
            // Disable natural GSS spawns.
            if (CanUseCustomAIs)
                sharkKillCount = 0;
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

        public override void PreUpdateEntities()
        {
            InfernumMode.BlackFade = MathHelper.Clamp(InfernumMode.BlackFade - 0.025f, 0f, 1f);
            NetcodeHandler.Update();
            TwinsAttackSynchronizer.DoUniversalUpdate();
            TwinsAttackSynchronizer.PostUpdateEffects();
            if (CalamityWorld.death)
                CalamityWorld.revenge = true;
			if (CalamityWorld.malice)
                CalamityWorld.death = true;

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