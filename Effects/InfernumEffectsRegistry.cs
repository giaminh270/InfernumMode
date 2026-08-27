using InfernumMode.Skies;
using InfernumMode;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ID;

namespace InfernumMode.Effects
{
    public static class InfernumEffectsRegistry
    {
        #region Texture Shaders
        public static Ref<Effect> FluidSimulatorShader
        {
            get;
            internal set;
        }
		public static MiscShaderData ExobladeSlash => GameShaders.Misc["Infernum:ExobladeSlash"];
        public static MiscShaderData AEWShadowFormShader => GameShaders.Misc["Infernum:AEWShadowForm"];
        public static MiscShaderData AreaBorderVertexShader => GameShaders.Misc["Infernum:AreaBorder"];
        public static MiscShaderData AresEnergySlashShader => GameShaders.Misc["Infernum:AresEnergySlash"];
        public static MiscShaderData AresLightningVertexShader => GameShaders.Misc["Infernum:AresLightningArc"];
        public static MiscShaderData ArtemisLaserVertexShader => GameShaders.Misc["Infernum:ArtemisLaser"];
        public static MiscShaderData BaseFusableParticleEdge => GameShaders.Misc["Infernum:BaseFusableParticleEdge"];
        public static MiscShaderData BackgroundDistortionShader => GameShaders.Misc["Infernum:BackgroundDistortion"];
        public static MiscShaderData BasicTintShader => GameShaders.Misc["Infernum:BasicTint"];
        public static MiscShaderData BrainPsychicVertexShader => GameShaders.Misc["Infernum:BrainPsychic"];
        public static MiscShaderData CeaselessVoidBackgroundShader => GameShaders.Misc["Infernum:CVBackground"];
        public static MiscShaderData CeaselessVoidCrackShader => GameShaders.Misc["Infernum:CVCrack"];
        public static MiscShaderData CeaselessVoidPortalShader => GameShaders.Misc["Infernum:CVPortal"];
        public static MiscShaderData CircleCutoutShader => GameShaders.Misc["Infernum:CircleCutout"];
        public static MiscShaderData CircleCutout2Shader => GameShaders.Misc["Infernum:CircleCutout2"];
        public static MiscShaderData CloudVertexShader => GameShaders.Misc["Infernum:CloudShader"];
        public static MiscShaderData CosmicBackgroundShader => GameShaders.Misc["Infernum:CosmicBackground"];
        public static MiscShaderData CultistDeathVertexShader => GameShaders.Misc["Infernum:CultistDeath"];
        public static MiscShaderData CultistShieldShader => GameShaders.Misc["Infernum:CultistShield"];
        public static MiscShaderData DarkFlamePillarVertexShader => GameShaders.Misc["Infernum:DarkFlamePillar"];
        public static MiscShaderData DoGDashIndicatorVertexShader => GameShaders.Misc["Infernum:DoGDashIndicatorShader"];
        public static MiscShaderData DukeTornadoVertexShader => GameShaders.Misc["Infernum:DukeTornado"];
        public static MiscShaderData FireVertexShader => GameShaders.Misc["Infernum:Fire"];
        public static MiscShaderData FishEyeShader => GameShaders.Misc["Infernum:Fisheye"];
        public static MiscShaderData FogShaderShader => GameShaders.Misc["Infernum:FogOverlay"];
        public static MiscShaderData GenericLaserVertexShader => GameShaders.Misc["Infernum:GenericLaserShader"];
        public static MiscShaderData GuardiansLaserVertexShader => GameShaders.Misc["Infernum:GuardiansLaserShader"];
        public static MiscShaderData KevinLightningShader => GameShaders.Misc["Infernum:KevinLightning"];
        public static MiscShaderData MechsIntroLetterShader => GameShaders.Misc["Infernum:MechsIntro"];
        public static MiscShaderData NoiseDisplacementShader => GameShaders.Misc["Infernum:NoiseDisplacement"];
        public static MiscShaderData PolterghastEctoplasmVertexShader => GameShaders.Misc["Infernum:PolterghastEctoplasm"];
        public static MiscShaderData PrismaticRayVertexShader => GameShaders.Misc["Infernum:PrismaticRay"];
        public static MiscShaderData ProfanedLavaVertexShader => GameShaders.Misc["Infernum:ProfanedLava"];
        public static MiscShaderData ProfanedPortalShader => GameShaders.Misc["Infernum:ProfanedPortal"];
        public static MiscShaderData ProviLaserVertexShader => GameShaders.Misc["Infernum:ProviLaserShader"];
        public static MiscShaderData PulsatingLaserVertexShader => GameShaders.Misc["Infernum:PulsatingLaserShader"];
        public static MiscShaderData RealityTearVertexShader => GameShaders.Misc["Infernum:RealityTear"];
        public static MiscShaderData RealityTear2Shader => GameShaders.Misc["Infernum:RealityTear2"];
        public static MiscShaderData SCalIntroLetterShader => GameShaders.Misc["Infernum:SCalIntro"];
        public static MiscShaderData SideStreakVertexShader => GameShaders.Misc["Infernum:SideStreak"];
        public static MiscShaderData SignusBackgroundShader => GameShaders.Misc["Infernum:SignusBackground"];
        public static MiscShaderData ScreenInversionMetaballShader => GameShaders.Misc["Infernum:ScreenInversionMetaball"];
        public static MiscShaderData ScrollingCodePrimShader => GameShaders.Misc["Infernum:ScrollingCode"];
        public static MiscShaderData TwinsFlameTrailVertexShader => GameShaders.Misc["Infernum:TwinsFlameTrail"];
        public static MiscShaderData UnderwaterRayShader => GameShaders.Misc["Infernum:UnderwaterRays"];
        public static MiscShaderData WaterVertexShader => InfernumConfig.Instance.ReducedGraphicsConfig ? DukeTornadoVertexShader : GameShaders.Misc["Infernum:WaterShader"];
        public static MiscShaderData WoFGeyserVertexShader => GameShaders.Misc["Infernum:WoFGeyserTexture"];
        public static MiscShaderData WoFTentacleVertexShader => GameShaders.Misc["Infernum:WoFTentacleTexture"];
        public static MiscShaderData YharonBurnShader => GameShaders.Misc["Infernum:YharonBurn"];
        public static MiscShaderData YharonInfernadoShader => GameShaders.Misc["Infernum:YharonInfernado"];
        public static MiscShaderData LightningArc => GameShaders.Misc["Infernum:LightningArc"];
		public static MiscShaderData Trail => GameShaders.Misc["Infernum:Trail"];
        #endregion

        #region Screen Shaders
        public static Filter AresScreenShader => Filters.Scene["InfernumMode:Ares"];
        public static Filter BasicLightingShader => Filters.Scene["InfernumMode:BasicLighting"];
        public static Filter BossBarShader => Filters.Scene["InfernumMode:BossBar"];
        public static Filter BloomShader => Filters.Scene["InfernumMode:Bloom"];
        public static Filter CalShadowScreenShader => Filters.Scene["InfernumMode:CalShadow"];
        public static Filter CreditShader => Filters.Scene["InfernumMode:Credits"];
        public static Filter DeusScreenShader => Filters.Scene["InfernumMode:Deus"];
        public static Filter DisplacementMap => Filters.Scene["InfernumMode:DisplacementMap"];
        public static Filter DragonfollyScreenShader => Filters.Scene["InfernumMode:Dragonfolly"];
        public static Filter DoGScreenShader => Filters.Scene["InfernumMode:DoG"];
        //public static Filter EoLScreenShader => Filters.Scene["InfernumMode:EmpressOfLight"];
        public static Filter FireballShader => Filters.Scene["Infernum:FireballShader"];
        public static Filter HiveMindScreenShader => Filters.Scene["InfernumMode:HiveMind"];
        public static Filter JumpFloodShader => Filters.Scene["InfernumMode:JumpFlood"];
        public static Filter MadnessScreenShader => Filters.Scene["InfernumMode:Madness"];
        public static Filter NightProviScreenShader => Filters.Scene["InfernumMode:NightProvidence"];
        public static Filter OldDukeScreenShader => Filters.Scene["InfernumMode:OldDuke"];
        public static Filter PerforatorsScreenShader => Filters.Scene["InfernumMode:Perforators"];
        public static Filter RaindropShader => Filters.Scene["InfernumMode:Raindrops"];
        public static Filter RaymarchingShader => Filters.Scene["InfernumMode:Raymarching"];
        public static Filter SCalScreenShader => Filters.Scene["InfernumMode:SCal"];
        public static Filter ScreenDistortionScreenShader => Filters.Scene["InfernumMode:ScreenDistortion"];
        public static Filter ScreenBorderShader => Filters.Scene["InfernumMode:ScreenBorder"];
        public static Filter ScreenSaturationBlurScreenShader => Filters.Scene["InfernumMode:ScreenSaturationBlur"];
        public static Filter ScreenShakeScreenShader => Filters.Scene["InfernumMode:ScreenShake"];
        public static Filter ScreenShakeScreenShader2 => Filters.Scene["InfernumMode:ScreenShake2"];
        public static Filter ShadowShader => Filters.Scene["InfernumMode:ShadowShader"];
        public static Filter TwinsScreenShader => Filters.Scene["InfernumMode:Twins"];
		public static Filter PixelatedSightLine => Filters.Scene["InfernumMode:PixelatedSightLine"];
		public static Filter Shield => Filters.Scene["InfernumMode:Shield"];

        #endregion

        #region Methods
        public static void LoadEffects()
        {
            if (Main.netMode == NetmodeID.Server)
                return;

            Mod mod = InfernumMode.Instance;
            if (mod == null)
                return;

            LoadRegularShaders(mod);
            LoadScreenShaders(mod);
        }

        public static void LoadRegularShaders(Mod mod)
        {
            FluidSimulatorShader = new Ref<Effect>(mod.GetEffect("Effects/FluidSimulator"));
            GameShaders.Misc["Infernum:DrawFluidResult"] = new MiscShaderData(FluidSimulatorShader, "DrawResultPass");
            GameShaders.Misc["Infernum:FluidUpdateVelocity"] = new MiscShaderData(FluidSimulatorShader, "VelocityUpdatePass");
            GameShaders.Misc["Infernum:FluidUpdateVelocityVorticity"] = new MiscShaderData(FluidSimulatorShader, "VelocityUpdateVorticityPass");
            GameShaders.Misc["Infernum:FluidAdvect"] = new MiscShaderData(FluidSimulatorShader, "AdvectPass");

			Ref<Effect> exobladeSlash = new Ref<Effect>(mod.GetEffect("Effects/ExobladeSlashShader"));
            GameShaders.Misc["Infernum:ExobladeSlash"] = new MiscShaderData(exobladeSlash, "TrailPass");
			
            Ref<Effect> aewShadowShader = new Ref<Effect>(mod.GetEffect("Effects/Overlays/AEWShadowShader"));
            GameShaders.Misc["Infernum:AEWShadowForm"] = new MiscShaderData(aewShadowShader, "BurnPass");

            Ref<Effect> areaBorder = new Ref<Effect>(mod.GetEffect("Effects/Shapes/AreaBorderShader"));
            GameShaders.Misc["Infernum:AreaBorder"] = new MiscShaderData(areaBorder, "TrailPass");

            Ref<Effect> aresEnergySlashShader = new Ref<Effect>(mod.GetEffect("Effects/Primitives/AresEnergySlashShader"));
            GameShaders.Misc["Infernum:AresEnergySlash"] = new MiscShaderData(aresEnergySlashShader, "TrailPass");

            Ref<Effect> aresLightningArcShader = new Ref<Effect>(mod.GetEffect("Effects/Primitives/AresLightningArcShader"));
            GameShaders.Misc["Infernum:AresLightningArc"] = new MiscShaderData(aresLightningArcShader, "TrailPass");

            Ref<Effect> artemisLaserShader = new Ref<Effect>(mod.GetEffect("Effects/Primitives/ArtemisLaserShader"));
            GameShaders.Misc["Infernum:ArtemisLaser"] = new MiscShaderData(artemisLaserShader, "TrailPass");

            Ref<Effect> backgroundDistortionShader = new Ref<Effect>(mod.GetEffect("Effects/Overlays/BackgroundDistortionShader"));
            GameShaders.Misc["Infernum:BackgroundDistortion"] = new MiscShaderData(backgroundDistortionShader, "DistortionPass");

            Ref<Effect> basicTintShader = new Ref<Effect>(mod.GetEffect("Effects/Overlays/BasicTint"));
            GameShaders.Misc["Infernum:BasicTint"] = new MiscShaderData(basicTintShader, "BasicTint");

            Ref<Effect> brainPsychicShader = new Ref<Effect>(mod.GetEffect("Effects/Primitives/BrainPsychicShader"));
            GameShaders.Misc["Infernum:BrainPsychic"] = new MiscShaderData(brainPsychicShader, "TrailPass");

            Ref<Effect> cvBGShader = new Ref<Effect>(mod.GetEffect("Effects/Overlays/CeaselessVoidBackgroundShader"));
            GameShaders.Misc["Infernum:CVBackground"] = new MiscShaderData(cvBGShader, "ScreenPass");

            Ref<Effect> cvCrackShader = new Ref<Effect>(mod.GetEffect("Effects/Cutouts/CeaselessVoidCrackShader"));
            GameShaders.Misc["Infernum:CVCrack"] = new MiscShaderData(cvCrackShader, "CrackPass");

            Ref<Effect> cvPortalShader = new Ref<Effect>(mod.GetEffect("Effects/Shapes/CeaselessVoidPortalShader"));
            GameShaders.Misc["Infernum:CVPortal"] = new MiscShaderData(cvPortalShader, "ScreenPass");

            Ref<Effect> cutoutShader = new Ref<Effect>(mod.GetEffect("Effects/Shapes/CircleCutoutShader"));
            GameShaders.Misc["Infernum:CircleCutout"] = new MiscShaderData(cutoutShader, "CutoutPass");

            cutoutShader = new Ref<Effect>(mod.GetEffect("Effects/Shapes/CircleCutoutShader2"));
            GameShaders.Misc["Infernum:CircleCutout2"] = new MiscShaderData(cutoutShader, "CutoutPass");

            Ref<Effect> cloudShader = new Ref<Effect>(mod.GetEffect("Effects/Primitives/CloudShader"));
            GameShaders.Misc["Infernum:CloudShader"] = new MiscShaderData(cloudShader, "TrailPass");

            Ref<Effect> cosmicBGShader = new Ref<Effect>(mod.GetEffect("Effects/Overlays/CosmicBackgroundShader"));
            GameShaders.Misc["Infernum:CosmicBackground"] = new MiscShaderData(cosmicBGShader, "CosmicPass");

            Ref<Effect> cultistDeathAnimationShader = new Ref<Effect>(mod.GetEffect("Effects/Cutouts/CultistDeathAnimation"));
            GameShaders.Misc["Infernum:CultistDeath"] = new MiscShaderData(cultistDeathAnimationShader, "DeathPass");

            Ref<Effect> cultistShield = new Ref<Effect>(mod.GetEffect("Effects/Shapes/CultistForcefield"));
            GameShaders.Misc["Infernum:CultistShield"] = new MiscShaderData(cultistShield, "ShieldPass");

            Ref<Effect> darkFlamePillarShader = new Ref<Effect>(mod.GetEffect("Effects/Primitives/DarkFlamePillarShader"));
            GameShaders.Misc["Infernum:DarkFlamePillar"] = new MiscShaderData(darkFlamePillarShader, "TrailPass");

            Ref<Effect> dashIndicator = new Ref<Effect>(mod.GetEffect("Effects/Primitives/DoGDashIndicatorShader"));
            GameShaders.Misc["Infernum:DoGDashIndicatorShader"] = new MiscShaderData(dashIndicator, "TrailPass");

            Ref<Effect> dukeTornadoShader = new Ref<Effect>(mod.GetEffect("Effects/Primitives/DukeTornado"));
            GameShaders.Misc["Infernum:DukeTornado"] = new MiscShaderData(dukeTornadoShader, "TrailPass");

            Ref<Effect> shadowflameShader = new Ref<Effect>(mod.GetEffect("Effects/Primitives/Shadowflame"));
            GameShaders.Misc["Infernum:Fire"] = new MiscShaderData(shadowflameShader, "TrailPass");

            Ref<Effect> fishEyeShader = new Ref<Effect>(mod.GetEffect("Effects/SpriteDistortions/FisheyeShader"));
            GameShaders.Misc["Infernum:Fisheye"] = new MiscShaderData(fishEyeShader, "FisheyePass");

            Ref<Effect> fogShader = new Ref<Effect>(mod.GetEffect("Effects/Overlays/FogShader"));
            GameShaders.Misc["Infernum:FogOverlay"] = new MiscShaderData(fogShader, "FogPass");

            Ref<Effect> genericLaserShader = new Ref<Effect>(mod.GetEffect("Effects/Primitives/GenericLaserShader"));
            GameShaders.Misc["Infernum:GenericLaserShader"] = new MiscShaderData(genericLaserShader, "TrailPass");

            Ref<Effect> guardiansShader = new Ref<Effect>(mod.GetEffect("Effects/Primitives/GuardiansLaserShader"));
            GameShaders.Misc["Infernum:GuardiansLaserShader"] = new MiscShaderData(guardiansShader, "TrailPass");

            Ref<Effect> kevinLightningShader = new Ref<Effect>(mod.GetEffect("Effects/Shapes/KevinLightningShader"));
            GameShaders.Misc["Infernum:KevinLightning"] = new MiscShaderData(kevinLightningShader, "UpdatePass");

            Ref<Effect> introShader = new Ref<Effect>(mod.GetEffect("Effects/Shapes/MechIntroLetterShader"));
            GameShaders.Misc["Infernum:MechsIntro"] = new MiscShaderData(introShader, "LetterPass");
			
			Ref<Effect> BaseFusableParticleEdgeShader = new Ref<Effect>(mod.GetEffect("Effects/ParticleFusion/InfernumBaseFusableParticleEdgeShader"));
            GameShaders.Misc["Infernum:BaseFusableParticleEdge"] = new MiscShaderData(BaseFusableParticleEdgeShader, "ParticlePass");			
			
			Ref<Effect> lightningArcShader = new Ref<Effect>(mod.GetEffect("Effects/HeavenlyGaleLightningShader"));
			GameShaders.Misc["Infernum:LightningArc"] = new MiscShaderData(lightningArcShader, "TrailPass");

			Ref<Effect> trailShader = new Ref<Effect>(mod.GetEffect("Effects/HeavenlyGaleTrailShader"));
			GameShaders.Misc["Infernum:Trail"] = new MiscShaderData(trailShader, "PiercePass");

            Ref<Effect> noiseDisplacementShader = new Ref<Effect>(mod.GetEffect("Effects/SpriteDistortions/NoiseDisplacement"));
            GameShaders.Misc["Infernum:NoiseDisplacement"] = new MiscShaderData(noiseDisplacementShader, "GlitchPass");

            Ref<Effect> ghostlyShader = new Ref<Effect>(mod.GetEffect("Effects/Primitives/PolterghastEctoplasmShader"));
            GameShaders.Misc["Infernum:PolterghastEctoplasm"] = new MiscShaderData(ghostlyShader, "BurstPass");

            Ref<Effect> rayShader = new Ref<Effect>(mod.GetEffect("Effects/Primitives/PrismaticRayShader"));
            GameShaders.Misc["Infernum:PrismaticRay"] = new MiscShaderData(rayShader, "TrailPass");

            Ref<Effect> profanedLavaShader = new Ref<Effect>(mod.GetEffect("Effects/Primitives/ProfanedLava"));
            GameShaders.Misc["Infernum:ProfanedLava"] = new MiscShaderData(profanedLavaShader, "TrailPass");

            Ref<Effect> profanedPortal = new Ref<Effect>(mod.GetEffect("Effects/Shapes/ProfanedPortalShader"));
            GameShaders.Misc["Infernum:ProfanedPortal"] = new MiscShaderData(profanedPortal, "PortalPass");

            Ref<Effect> proviLaserShader = new Ref<Effect>(mod.GetEffect("Effects/Primitives/ProviLaserShader"));
            GameShaders.Misc["Infernum:ProviLaserShader"] = new MiscShaderData(proviLaserShader, "TrailPass");

            Ref<Effect> pulsatingLaser = new Ref<Effect>(mod.GetEffect("Effects/Primitives/PulsatingLaser"));
            GameShaders.Misc["Infernum:PulsatingLaserShader"] = new MiscShaderData(pulsatingLaser, "TrailPass");

            Ref<Effect> realityTearShader = new Ref<Effect>(mod.GetEffect("Effects/Primitives/RealityTearShader"));
            GameShaders.Misc["Infernum:RealityTear"] = new MiscShaderData(realityTearShader, "TrailPass");

            realityTearShader = new Ref<Effect>(mod.GetEffect("Effects/Shapes/RealityTear2Shader"));
            GameShaders.Misc["Infernum:RealityTear2"] = new MiscShaderData(realityTearShader, "TrailPass");

            introShader = new Ref<Effect>(mod.GetEffect("Effects/Shapes/SCalIntroLetterShader"));
            GameShaders.Misc["Infernum:SCalIntro"] = new MiscShaderData(introShader, "LetterPass");

            Ref<Effect> streakShader = new Ref<Effect>(mod.GetEffect("Effects/Primitives/SideStreakTrail"));
            GameShaders.Misc["Infernum:SideStreak"] = new MiscShaderData(streakShader, "TrailPass");

            Ref<Effect> signusBGShader = new Ref<Effect>(mod.GetEffect("Effects/Overlays/SignusBackgroundShader"));
            GameShaders.Misc["Infernum:SignusBackground"] = new MiscShaderData(signusBGShader, "ScreenPass");

            Ref<Effect> screenInversionShader = new Ref<Effect>(mod.GetEffect("Effects/Shapes/ScreenInversionMetaballShader"));
            GameShaders.Misc["Infernum:ScreenInversionMetaball"] = new MiscShaderData(screenInversionShader, "UpdatePass");

            Ref<Effect> codeScrollShader = new Ref<Effect>(mod.GetEffect("Effects/Primitives/ScrollingCodePrimShader"));
            GameShaders.Misc["Infernum:ScrollingCode"] = new MiscShaderData(codeScrollShader, "TrailPass");

            Ref<Effect> teleportShader = new Ref<Effect>(mod.GetEffect("Effects/SpriteDistortions/TeleportShader"));
            GameShaders.Misc["Infernum:Teleport"] = new MiscShaderData(teleportShader, "HologramPass");

            Ref<Effect> flameTrailShader = new Ref<Effect>(mod.GetEffect("Effects/Primitives/TwinsFlameTail"));
            GameShaders.Misc["Infernum:TwinsFlameTrail"] = new MiscShaderData(flameTrailShader, "TrailPass");

            Ref<Effect> atThisTimeOfYear = new Ref<Effect>(mod.GetEffect("Effects/Shapes/UnderwaterRayShader"));
            GameShaders.Misc["Infernum:UnderwaterRays"] = new MiscShaderData(atThisTimeOfYear, "RayPass");

            Ref<Effect> waterShader = new Ref<Effect>(mod.GetEffect("Effects/Primitives/WaterShader"));
            GameShaders.Misc["Infernum:WaterShader"] = new MiscShaderData(waterShader, "WaterPass");

            Ref<Effect> bloodGeyserShader = new Ref<Effect>(mod.GetEffect("Effects/Primitives/BloodGeyser"));
            GameShaders.Misc["Infernum:WoFGeyserTexture"] = new MiscShaderData(bloodGeyserShader, "TrailPass");

            Ref<Effect> tentacleFleshShader = new Ref<Effect>(mod.GetEffect("Effects/Primitives/TentacleTexture"));
            GameShaders.Misc["Infernum:WoFTentacleTexture"] = new MiscShaderData(tentacleFleshShader, "TrailPass");

            Ref<Effect> yharonBurnShader = new Ref<Effect>(mod.GetEffect("Effects/Overlays/YharonBurnShader"));
            GameShaders.Misc["Infernum:YharonBurn"] = new MiscShaderData(yharonBurnShader, "BurnPass");

            Ref<Effect> yharonInfernadoShader = new Ref<Effect>(mod.GetEffect("Effects/Primitives/YharonInfernadoShader"));
            GameShaders.Misc["Infernum:YharonInfernado"] = new MiscShaderData(yharonInfernadoShader, "TrailPass");
        }

        public static void LoadScreenShaders(Mod mod)
        {
            // Bossbar shader.
            Ref<Effect> bossBarShader = new Ref<Effect>(mod.GetEffect("Effects/Overlays/BossBarShader"));
            Filters.Scene["InfernumMode:BossBar"] = new Filter(new ScreenShaderData(bossBarShader, "FilterPass"), EffectPriority.VeryHigh);

            // Raindrop shader.
            Ref<Effect> rainShader = new Ref<Effect>(mod.GetEffect("Effects/SpriteDistortions/RaindropShader"));
            Filters.Scene["InfernumMode:Raindrops"] = new Filter(new ScreenShaderData(rainShader, "RainPass"), EffectPriority.VeryHigh);

            // Credits shader.
            Ref<Effect> creditShader = new Ref<Effect>(mod.GetEffect("Effects/Overlays/CreditShader"));
            Filters.Scene["InfernumMode:Credits"] = new Filter(new ScreenShaderData(creditShader, "CreditPass"), EffectPriority.VeryHigh);

            // Raymarching shader.
            Ref<Effect> raymarchingShader = new Ref<Effect>(mod.GetEffect("Effects/Overlays/RaymarchingShader"));
            Filters.Scene["InfernumMode:Raymarching"] = new Filter(new ScreenShaderData(raymarchingShader, "RaymarchPass"), EffectPriority.VeryHigh);

            // Displacement map shader.
            Ref<Effect> displacementMap = new Ref<Effect>(mod.GetEffect("Effects/Overlays/DisplacementMapShader"));
            Filters.Scene["InfernumMode:DisplacementMap"] = new Filter(new ScreenShaderData(displacementMap, "DisplacementPass"), EffectPriority.VeryHigh);

            // Jump flood shader.
            Ref<Effect> jumpFlood = new Ref<Effect>(mod.GetEffect("Effects/Overlays/JumpFloodShader"));
            Filters.Scene["InfernumMode:JumpFlood"] = new Filter(new ScreenShaderData(jumpFlood, "JumpFloodPass"), EffectPriority.VeryHigh);
			
			Ref<Effect> pixelatedSightShader = new Ref<Effect>(mod.GetEffect("Effects/PixelatedSightLine"));
			Filters.Scene["InfernumMode:PixelatedSightLine"] = new Filter(new ScreenShaderData(pixelatedSightShader, "SightLinePass"), EffectPriority.High);
			
			Ref<Effect> shieldShader = new Ref<Effect>(mod.GetEffect("Effects/RoverDriveShield"));
			Filters.Scene["InfernumMode:Shield"] = new Filter(new ScreenShaderData(shieldShader, "ShieldPass"), EffectPriority.High);

            // Bloom shader.
            Ref<Effect> bloomShader = new Ref<Effect>(mod.GetEffect("Effects/Overlays/GaussianBlur"));
            Filters.Scene["InfernumMode:Bloom"] = new Filter(new ScreenShaderData(bloomShader, "BloomPass"), EffectPriority.VeryHigh);

            // Basic fake shadows shader.
            Ref<Effect> shadowShader = new Ref<Effect>(mod.GetEffect("Effects/Overlays/ShadowShader"));
            Filters.Scene["InfernumMode:ShadowShader"] = new Filter(new ScreenShaderData(shadowShader, "GetShadowPass"), EffectPriority.VeryHigh);

            // Basic lighting shader.
            Ref<Effect> lightingShader = new Ref<Effect>(mod.GetEffect("Effects/Overlays/LightingShader"));
            Filters.Scene["InfernumMode:BasicLighting"] = new Filter(new ScreenShaderData(lightingShader, "LightingPass"), EffectPriority.VeryHigh);

            // Flower of the ocean sky. (WIP)
            /*Filters.Scene["InfernumMode:FlowerOfTheOcean"] = new Filter(new PerforatorScreenShaderData("FilterMiniTower").UseColor(0f, 0f, 0f).UseOpacity(0f), EffectPriority.VeryHigh);
            SkyManager.Instance["InfernumMode:FlowerOfTheOcean"] = new FlowerOceanSky();*/

            // Fireball shader.
            Ref<Effect> fireballShader = new Ref<Effect>(mod.GetEffect("Effects/Shapes/FireballShader"));
            Filters.Scene["Infernum:FireballShader"] = new Filter(new ScreenShaderData(fireballShader, "FirePass"), EffectPriority.VeryHigh);

            // Screen Border Shader.
            Ref<Effect> screenBorderShader = new Ref<Effect>(mod.GetEffect("Effects/Overlays/ScreenBorderShader"));
            Filters.Scene["InfernumMode:ScreenBorder"] = new Filter(new ScreenShaderData(screenBorderShader, "ScreenPass"), EffectPriority.VeryHigh);

            Filters.Scene["InfernumMode:GuardianCommander"] = new Filter(new PerforatorScreenShaderData("FilterMiniTower").UseColor(0f, 0f, 0f).UseOpacity(0f), EffectPriority.VeryHigh);
            SkyManager.Instance["InfernumMode:GuardianCommander"] = new ProfanedGuardiansSky();

            // Ares (ultimate attack).
            Filters.Scene["InfernumMode:Ares"] = new Filter(new AresScreenShaderData("FilterMiniTower").UseColor(Color.Red).UseOpacity(0.5f), EffectPriority.VeryHigh);
            SkyManager.Instance["InfernumMode:Ares"] = new AresSky();

            // Astrum Deus.
            Filters.Scene["InfernumMode:Deus"] = new Filter(new DeusScreenShaderData("FilterMiniTower").UseColor(Color.Lerp(Color.Purple, Color.Black, 0.75f)).UseOpacity(0.24f), EffectPriority.VeryHigh);
            SkyManager.Instance["InfernumMode:Deus"] = new DeusSky();

            // Calamitas' Shadow.
            Filters.Scene["InfernumMode:CalShadow"] = new Filter(new CalShadowScreenShaderData("FilterMiniTower").UseOpacity(0f), EffectPriority.VeryHigh);
            SkyManager.Instance["InfernumMode:CalShadow"] = new CalShadowSky();

            // Dragonfolly.
            Filters.Scene["InfernumMode:Dragonfolly"] = new Filter(new DragonfollyScreenShaderData("FilterMiniTower").UseColor(Color.Red).UseOpacity(0.6f), EffectPriority.VeryHigh);
            SkyManager.Instance["InfernumMode:Dragonfolly"] = new DragonfollySky();

            // Devourer of Gods.
            Filters.Scene["InfernumMode:DoG"] = new Filter(new PerforatorScreenShaderData("FilterMiniTower").UseColor(0.4f, 0.1f, 1.0f).UseOpacity(0.5f), EffectPriority.VeryHigh);
            SkyManager.Instance["InfernumMode:DoG"] = new DoGSkyInfernum();
			

            /*// Empress of Light. (NOT IN 1.3)
            Effect screenShader = assets.Request<Effect>("Effects/Overlays/EmpressOfLightScreenShader");
            Filters.Scene["InfernumMode:EmpressOfLight"] = new Filter(new EmpressOfLightScreenShaderData(screenShader, "ScreenPass"), EffectPriority.VeryHigh);
            SkyManager.Instance["InfernumMode:EmpressOfLight"] = new EmpressOfLightSky();*/ // 

            // General screen shake distortion shaders.
            Ref<Effect> screenShakeShader = new Ref<Effect>(mod.GetEffect("Effects/SpriteDistortions/ScreenShakeShader"));
            Filters.Scene["InfernumMode:ScreenShake"] = new Filter(new ScreenShaderData(screenShakeShader, "DyePass"), EffectPriority.VeryHigh);

            screenShakeShader = new Ref<Effect>(mod.GetEffect("Effects/SpriteDistortions/ScreenShockwaveShader2"));
            Filters.Scene["InfernumMode:ScreenShake2"] = new Filter(new ScreenShaderData(screenShakeShader, "DyePass"), EffectPriority.VeryHigh);

            // Heat distortion effect.
            Ref<Effect> screenDistortionShader = new Ref<Effect>(mod.GetEffect("Effects/Overlays/ScreenDistortionShader"));
            Filters.Scene["InfernumMode:ScreenDistortion"] = new Filter(new ScreenShaderData(screenDistortionShader, "ScreenPass"), EffectPriority.VeryHigh);

            // Hive Mind.
            Filters.Scene["InfernumMode:HiveMind"] = new Filter(new HiveMindScreenShaderData("FilterMiniTower").UseColor(HiveMindSky.SkyColor).UseOpacity(0.6f), EffectPriority.VeryHigh);
            SkyManager.Instance["InfernumMode:HiveMind"] = new HiveMindSky();

            // Hyperplane Matrix time change sky. (WIP)
            /*Filters.Scene["InfernumMode:HyperplaneMatrixTimeChange"] = new Filter(new PerforatorScreenShaderData("FilterMiniTower").UseColor(1f, 1f, 1f).UseOpacity(0f), EffectPriority.VeryHigh);
            SkyManager.Instance["InfernumMode:HyperplaneMatrixTimeChange"] = new HyperplaneMatrixTimeChangeSky();*/

            // Deerclops.
            Ref<Effect> madnessShader = new Ref<Effect>(mod.GetEffect("Effects/Overlays/Madness"));
            Filters.Scene["InfernumMode:Madness"] = new Filter(new MadnessScreenShaderData(madnessShader, "DyePass"), EffectPriority.VeryHigh);
            SkyManager.Instance["InfernumMode:Madness"] = new MadnessSky();

            // Moon Lord.
            Ref<Effect> fireBGShader = new Ref<Effect>(mod.GetEffect("Effects/Overlays/SCalFireBGShader"));
            Filters.Scene["InfernumMode:MoonLord"] = new Filter(new MLScreenShaderData(fireBGShader, "DyePass").UseOpacity(0.5f), EffectPriority.VeryHigh);
            SkyManager.Instance["InfernumMode:MoonLord"] = new MLSky();

            // Night Providence.
            Filters.Scene["InfernumMode:NightProvidence"] = new Filter(new NightProvidenceShaderData("FilterMiniTower").UseOpacity(0.67f), EffectPriority.VeryHigh);
            SkyManager.Instance["InfernumMode:NightProvidence"] = new NightProvidenceSky();

            // Old Duke.
            Filters.Scene["InfernumMode:OldDuke"] = new Filter(new OldDukeScreenShaderData("FilterMiniTower").UseOpacity(0.6f), EffectPriority.VeryHigh);
            SkyManager.Instance["InfernumMode:OldDuke"] = new OldDukeSky();

            // Perforators (death animation).
            Filters.Scene["InfernumMode:Perforators"] = new Filter(new PerforatorScreenShaderData("FilterMiniTower").UseColor(new Color(255, 60, 30)).UseOpacity(0.445f), EffectPriority.VeryHigh);
            SkyManager.Instance["InfernumMode:Perforators"] = new PerforatorSky();

            // Screen saturation blur system shader. (WIP)
            /*Ref<Effect> screenSaturationBlurShader = new Ref<Effect>(mod.GetEffect("Effects/Overlays/ScreenSaturationBlurShader"));
            Filters.Scene["InfernumMode:ScreenSaturationBlur"] = new Filter(new ScreenSaturationBlurShaderData(screenSaturationBlurShader, "ScreenPass"), EffectPriority.VeryHigh);*/

            // Supreme Calamitas.
            Filters.Scene["InfernumMode:SCal"] = new Filter(new SCalScreenShaderData(fireBGShader, "DyePass").UseColor(0.3f, 0f, 0f).UseOpacity(0.5f), EffectPriority.VeryHigh);
            SkyManager.Instance["InfernumMode:SCal"] = new SCalSkyInfernum();

            // Twins (desperation phase). (WIP) 
            /*Filters.Scene["InfernumMode:Twins"] = new Filter(new TwinsScreenShaderData("FilterMiniTower").UseColor(Color.Red).UseOpacity(0.5f), EffectPriority.VeryHigh);
            SkyManager.Instance["InfernumMode:Twins"] = new TwinsSky();*/

            // Yharon.
            Filters.Scene["InfernumMode:Yharon"] = new Filter(new PerforatorScreenShaderData("FilterMiniTower").UseColor(0f, 0f, 0f).UseOpacity(0f), EffectPriority.VeryHigh);
            SkyManager.Instance["InfernumMode:Yharon"] = new YharonSky();
        }
        #endregion
    }
}
