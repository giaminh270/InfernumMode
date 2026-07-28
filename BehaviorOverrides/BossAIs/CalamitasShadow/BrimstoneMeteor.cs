using CalamityMod.NPCs;
using CalamityMod.Particles;
using CalamityMod.Projectiles.Boss;
using InfernumMode.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.CalamitasShadow
{
    public class BrimstoneMeteor : ModProjectile
    {
        public ref float Time => ref projectile.ai[0];

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Brimstone Meteor");
            Main.projFrames[projectile.type] = 4;
            ProjectileID.Sets.TrailingMode[projectile.type] = 2;
            ProjectileID.Sets.TrailCacheLength[projectile.type] = 4;
        }

        public override void SetDefaults()
        {
            projectile.width = projectile.height = 36;
            projectile.hostile = true;
            projectile.ignoreWater = true;
            projectile.tileCollide = false;
            projectile.penetrate = -1;
            projectile.timeLeft = 360;
            cooldownSlot = 1;
        }

        public override void AI()
        {
            projectile.frameCounter++;
            projectile.frame = projectile.frameCounter / 5 % Main.projFrames[projectile.type];

            projectile.Opacity = Utils.InverseLerp(0f, 12f, projectile.timeLeft, true);
            projectile.rotation = projectile.velocity.ToRotation() - MathHelper.PiOver2;

            float acceleration = 1.01f;
            float maxSpeed = 17f;
            if (CalamityGlobalNPC.calamitas != -1 && Main.player[Main.npc[CalamityGlobalNPC.calamitas].target].Infernum_CalShadowHex().HexIsActive("Zeal"))
            {
                // Start out slower if acceleration is expected.
                if (projectile.ai[1] == 0f)
                {
                    projectile.velocity *= 0.3f;
                    projectile.ai[1] = 1f;
                    projectile.netUpdate = true;
                }

                acceleration = 1.02f;
            }

            if (acceleration > 1f && projectile.velocity.Length() < maxSpeed)
                projectile.velocity *= acceleration;

            // Explode once past the tile collision line.
            projectile.tileCollide = projectile.Top.Y >= projectile.ai[1];

            Time++;
        }

        public override Color? GetAlpha(Color lightColor) => Color.White * projectile.Opacity;

        public override bool CanDamage() => projectile.Opacity >= 1f;

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            lightColor = Color.Lerp(lightColor, Color.White, 0.5f);
            lightColor.A /= 3;
            Utilities.DrawAfterimagesCentered(projectile, lightColor, ProjectileID.Sets.TrailingMode[projectile.type], 1);
            return false;
        }

        public override void Kill(int timeLeft)
        {
            Main.PlaySound(InfernumMode.CalamityMod.GetLegacySoundSlot(SoundType.Custom, "Sounds/Custom/ProvidenceHolyBlastImpact"), projectile.Center);

            for (int i = 0; i < 6; i++)
            {
                Color fireColor = Main.rand.NextBool() ? Color.HotPink : Color.Red;
                CloudParticle fireCloud = new CloudParticle(projectile.Center, (MathHelper.TwoPi * i / 6f).ToRotationVector2() * 2f + Main.rand.NextVector2Circular(0.3f, 0.3f), fireColor, Color.DarkGray, 33, Main.rand.NextFloat(1.8f, 2f))
                {
                    Rotation = Main.rand.NextFloat(MathHelper.TwoPi)
                };
                GeneralParticleHandler.SpawnParticle(fireCloud);
            }
        }

        public override bool ShouldUpdatePosition() => true;
    }
}
