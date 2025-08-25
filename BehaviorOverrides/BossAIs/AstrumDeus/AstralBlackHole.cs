using CalamityMod;
using CalamityMod.Events;
using CalamityMod.Items.Weapons.DraedonsArsenal;
using CalamityMod.NPCs.AstrumDeus;
using CalamityMod.Particles;
using InfernumMode.BehaviorOverrides.BossAIs.Cultist;
using InfernumMode.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.AstrumDeus
{
    public class AstralBlackHole : ModProjectile
    {
        public const int LaserCount = 6;

        public ref float Timer => ref projectile.ai[0];
        public ref float Owner => ref projectile.ai[1];
        public Player Target => Main.player[projectile.owner];
        public override string Texture => "InfernumMode/ExtraTextures/WhiteHole";

        public override void SetStaticDefaults() => DisplayName.SetDefault("Astral Black Hole");

        public override void SetDefaults()
        {
            projectile.width = projectile.height = 160;
            projectile.hostile = true;
            projectile.ignoreWater = true;
            projectile.tileCollide = false;
            projectile.penetrate = -1;
            projectile.timeLeft = 900000;
            projectile.scale = 1f;
            cooldownSlot = 1;
        }

        public override void AI()
        {
            // Die if Deus is not present.
            if (!NPC.AnyNPCs(ModContent.NPCType<AstrumDeusHeadSpectral>()))
            {
                projectile.Kill();
                return;
            }

            projectile.scale = Utilities.UltrasmoothStep(Timer / 60f) * 2.5f + Utilities.UltrasmoothStep(Timer / 32f) * 0.34f;
            projectile.scale = MathHelper.Lerp(projectile.scale, 0f, Utils.InverseLerp(30f, 0f, projectile.timeLeft, true));
            projectile.Opacity = MathHelper.Clamp(projectile.scale * 0.87f, 0f, 1f);
            Timer++;

            // Prepare for death if the lasers are gone.
            if (Timer > 90f && !Utilities.AnyProjectiles(ModContent.ProjectileType<DarkGodLaser>()) && projectile.timeLeft > 30)
            {
                projectile.damage = 0;
                projectile.timeLeft = 30;
            }

            // Create the lasers.
            if (Timer == 90f)
            {
				Main.PlaySound(InfernumMode.CalamityMod.GetLegacySoundSlot(Terraria.ModLoader.SoundType.Item, "Sounds/Item/TeslaCannonFire"), projectile.Center);

                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    for (int i = 0; i < LaserCount; i++)
                    {
                        Vector2 laserDirection = -Vector2.UnitY.RotatedBy(MathHelper.TwoPi * i / LaserCount);
                        Utilities.NewProjectileBetter(projectile.Center, laserDirection, ModContent.ProjectileType<DarkGodLaser>(), 300, 0f);
                    }
                }
            }

            // Idly release sparks.
            if (Timer >= 90f && Timer % 10f == 9f)
            {
                Main.PlaySound(SoundID.Item28, projectile.Center);
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    Player target = Main.player[Player.FindClosest(projectile.Center, 1, 1)];
                    Vector2 flyVelocity = projectile.SafeDirectionTo(target.Center) * (BossRushEvent.BossRushActive ? 28f : 19.5f);
                    Utilities.NewProjectileBetter(projectile.Center + flyVelocity * 10f, flyVelocity, ModContent.ProjectileType<DarkBoltLarge>(), 200, 0f);
                }
            }
        }

        public override bool CanDamage() => Timer >= 96f;

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            return CalamityUtils.CircularHitboxCollision(projectile.Center, projectile.scale * 80f, targetHitbox);
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Texture2D blackHoleTexture = Main.projectileTexture[projectile.type];
            Texture2D noiseTexture = ModContent.GetTexture("CalamityMod/ExtraTextures/VoronoiShapes");
            Vector2 drawPosition = projectile.Center - Main.screenPosition;
            Vector2 origin = noiseTexture.Size() * 0.5f;

            if (Timer < 90f)
            {
                float width = Utils.InverseLerp(0f, 8f, Timer, true) * Utils.InverseLerp(45f, 38f, Timer, true) * 3f;
                for (int i = 0; i < LaserCount; i++)
                {
                    Vector2 lineDirection = -Vector2.UnitY.RotatedBy(MathHelper.TwoPi * i / LaserCount);
                    Main.spriteBatch.DrawLineBetter(projectile.Center, projectile.Center + lineDirection * 4500f, Color.Violet, width);
                }
            }

            // Draw a vortex blackglow effect if the reduced graphics config is not enabled.
            if (!InfernumConfig.Instance.ReducedGraphicsConfig)
            {
                Main.spriteBatch.EnterShaderRegion();

            	Vector2 diskScale = projectile.scale * new Vector2(0.925f, 0.85f);
            	GameShaders.Misc["CalamityMod:DoGPortal"].UseOpacity(projectile.Opacity);
                GameShaders.Misc["CalamityMod:DoGPortal"].UseColor(Color.Turquoise);
                GameShaders.Misc["CalamityMod:DoGPortal"].UseSecondaryColor(Color.Turquoise);
                GameShaders.Misc["CalamityMod:DoGPortal"].Apply();

                for (int i = 0; i < 2; i++)
                    Main.spriteBatch.Draw(noiseTexture, drawPosition, null, Color.White, 0f, origin, diskScale, SpriteEffects.None, 0f);
                Main.spriteBatch.ExitShaderRegion();
            }

            Vector2 blackHoleScale = projectile.Size / blackHoleTexture.Size() * projectile.scale;
            for (int i = 0; i < 2; i++)
                Main.spriteBatch.Draw(blackHoleTexture, drawPosition, null, Color.White, 0f, blackHoleTexture.Size() * 0.5f, blackHoleScale * 1.0024f, SpriteEffects.None, 0f);
            for (int i = 0; i < 3; i++)
                Main.spriteBatch.Draw(blackHoleTexture, drawPosition, null, Color.Black, 0f, blackHoleTexture.Size() * 0.5f, blackHoleScale, SpriteEffects.None, 0f);

            return false;
        }

        public override void Kill(int timeLeft)
        {
            Utilities.DeleteAllProjectiles(false, ModContent.ProjectileType<DarkStar>());

			Main.PlaySound(InfernumMode.Instance.GetLegacySoundSlot(SoundType.Custom, "Sounds/Custom/WyrmElectricCharge"), projectile.Center);
            Color[] explosionColors = new Color[]
            {
                new Color(250, 90, 74, 127),
                new Color(76, 255, 194, 127)
            };
            GeneralParticleHandler.SpawnParticle(new ElectricExplosionRing(projectile.Center, Vector2.Zero, explosionColors, 3f, 180, 1.4f));
        }
    }
}
