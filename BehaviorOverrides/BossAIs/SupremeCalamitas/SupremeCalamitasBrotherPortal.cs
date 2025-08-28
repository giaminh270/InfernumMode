using CalamityMod;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.SupremeCalamitas
{
    public class SupremeCalamitasBrotherPortal : ModProjectile
    {
        public int NPCIDToSpawn => (int)projectile.ai[0];

        public ref float Time => ref projectile.ai[1];

        public const int Lifetime = 150;

        public override void SetStaticDefaults() => DisplayName.SetDefault("Dark Portal");

        public override void SetDefaults()
        {
            projectile.width = projectile.height = 34;
            projectile.hostile = true;
            projectile.ignoreWater = true;
            projectile.tileCollide = false;
            projectile.penetrate = -1;
            projectile.timeLeft = Lifetime;
            projectile.scale = 0f;
            cooldownSlot = 1;
        }

        public override void AI()
        {
            projectile.scale = Utils.InverseLerp(0f, Lifetime * 0.4f, Time, true) * Utils.InverseLerp(1f, Lifetime * 0.9f, Time, true);
            projectile.Opacity = projectile.scale;

            // Create a lot of light particles around the portal.
            float particleSpawnChance = Utilities.Remap(Time, 0f, 60f, 0.1f, 0.9f);
            for (int i = 0; i < 2; i++)
            {
                if (Main.rand.NextFloat() > particleSpawnChance)
                    continue;

                float scale = Main.rand.NextFloat(0.5f, 0.66f);
                Color particleColor = Color.Lerp(Color.Red, Color.Yellow, Main.rand.NextFloat(0.1f, 0.9f));
                Vector2 particleSpawnOffset = Main.rand.NextVector2Unit() * Main.rand.NextFloat(0.15f, 1f) * projectile.scale * 512f;
                Vector2 particleVelocity = particleSpawnOffset * -0.05f;
                SquishyLightParticle light = new SquishyLightParticle(projectile.Center + particleSpawnOffset, particleVelocity, scale, particleColor, 40, 1f, 7f);
                GeneralParticleHandler.SpawnParticle(light);
            }

            // Summon the brother and create a massive explosion before having the portal close.
            if (Time == (int)(Lifetime * 0.8f))
            {
                Main.PlaySound(InfernumMode.Instance.GetLegacySoundSlot(SoundType.Custom, "Sounds/Custom/WyrmElectricCharge"), projectile.Center);
                Main.PlaySound(InfernumMode.Instance.GetLegacySoundSlot(SoundType.Custom, "Sounds/Custom/HeavyExplosion"), projectile.Center);
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    int explosion = Utilities.NewProjectileBetter(projectile.Center, Vector2.Zero, ModContent.ProjectileType<DemonicExplosion>(), 0, 0f);
                    if (Main.projectile.IndexInRange(explosion))
                        Main.projectile[explosion].ModProjectile<DemonicExplosion>().MaxRadius = 800f;

                    NPC.NewNPC((int)projectile.Center.X, (int)projectile.Center.Y, NPCIDToSpawn);
                }
            }

            Time++;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Main.spriteBatch.EnterShaderRegion();

            float fade = Utils.InverseLerp(0f, 45f, Time, true) * Utils.InverseLerp(0f, 45f, projectile.timeLeft, true);
            Texture2D noiseTexture = ModContent.GetTexture("CalamityMod/ExtraTextures/VoronoiShapes");
            Vector2 drawPosition2 = projectile.Center - Main.screenPosition;
            Vector2 origin = noiseTexture.Size() * 0.5f;
            GameShaders.Misc["CalamityMod:DoGPortal"].UseOpacity(fade);
            GameShaders.Misc["CalamityMod:DoGPortal"].UseColor(Color.Violet);
            GameShaders.Misc["CalamityMod:DoGPortal"].UseSecondaryColor(Color.Red);
            GameShaders.Misc["CalamityMod:DoGPortal"].Apply();

            Main.spriteBatch.Draw(noiseTexture, drawPosition2, null, Color.White, 0f, origin, new Vector2(projectile.scale * 0.6f, 1f) * 4f, SpriteEffects.None, 0f);
            Main.spriteBatch.ExitShaderRegion();
            return false;
        }
    }
}
