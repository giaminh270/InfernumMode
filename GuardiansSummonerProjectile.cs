using CalamityMod;
using CalamityMod.NPCs.ProfanedGuardians;
using CalamityMod.NPCs.Providence;
using InfernumMode.BehaviorOverrides.BossAIs.Providence;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;


namespace InfernumMode
{
    public class GuardiansSummonerProjectile : ModProjectile
    {
        public ref float Time => ref projectile.ai[0];

        public const int Lifetime = 300;

        public override string Texture => "CalamityMod/Items/SummonItems/ProfanedShard";

        public override void SetDefaults()
        {
            projectile.width = 24;
            projectile.height = 24;
            projectile.aiStyle = -1;
            projectile.ignoreWater = true;
            projectile.tileCollide = false;
            projectile.timeLeft = Lifetime;
            projectile.Opacity = 1f;
            projectile.penetrate = -1;
        }

        public override void AI()
        {
            // Play a rumble sound.
            if (Time == 75f)
                Main.PlaySound(InfernumMode.Instance.GetLegacySoundSlot(SoundType.Custom, "Sounds/Custom/LeviathanSummonBase"), projectile.Center);

            if (Time >= 210f)
            {
                // Create screen shake effects.
                Main.LocalPlayer.Calamity().GeneralScreenShakePower = Utils.InverseLerp(2300f, 1300f, Main.LocalPlayer.Distance(projectile.Center), true) * 8f;
            }

            Time++;
        }

        public override void Kill(int timeLeft)
        {
            Main.LocalPlayer.Calamity().GeneralScreenShakePower = Utils.InverseLerp(2300f, 1300f, Main.LocalPlayer.Distance(projectile.Center), true) * 16f;

            // Make the crystal shatter.
            Main.PlaySound(InfernumMode.CalamityMod.GetLegacySoundSlot(SoundType.NPCKilled, "Sounds/NPCKilled/ProvidenceDeath"), projectile.Center);

            // Create an explosion and summon the Guardian Commander.
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                CalamityUtils.SpawnBossBetter(projectile.Center - Vector2.UnitY * 250f, ModContent.NPCType<ProfanedGuardianBoss>());
                
                int explosion = Utilities.NewProjectileBetter(projectile.Center, Vector2.Zero, ModContent.ProjectileType<HolySunExplosion>(), 0, 0f);
                if (Main.projectile.IndexInRange(explosion))
                    Main.projectile[explosion].ModProjectile<HolySunExplosion>().MaxRadius = 600f;
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Texture2D texture = ModContent.GetTexture(Texture);
            Vector2 drawPosition = projectile.Center - Main.screenPosition;

            float glowInterpolant = Utils.InverseLerp(60f, 105f, Time, true);
            if (glowInterpolant > 0f)
            {
                for (int i = 0; i < 8; i++)
                {
                    Color color = Color.Lerp(Color.White, Color.Yellow, glowInterpolant) * projectile.Opacity * glowInterpolant;
                    Vector2 drawOffset = (Time * MathHelper.TwoPi / 50f + MathHelper.TwoPi * i / 8f).ToRotationVector2() * glowInterpolant * 7.5f;
                    Main.spriteBatch.Draw(texture, drawPosition + drawOffset, null, color, projectile.rotation, texture.Size() * 0.5f, projectile.scale, 0, 0f);
                }
            }
            Main.spriteBatch.Draw(texture, drawPosition, null, Color.White * projectile.Opacity, projectile.rotation, texture.Size() * 0.5f, projectile.scale, 0, 0f);

            return false;
        }
    }
}
