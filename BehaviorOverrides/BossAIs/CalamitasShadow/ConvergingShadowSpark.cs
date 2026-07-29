using CalamityMod;
using CalamityMod.NPCs;
using CalamityMod.Particles;
using CalamityMod.Projectiles.Boss;
using InfernumMode;
using InfernumMode.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.CalamitasShadow
{
    public class ConvergingShadowSpark : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Shadow Spark");
            Main.projFrames[projectile.type] = 4;
            ProjectileID.Sets.TrailingMode[projectile.type] = 0;
            ProjectileID.Sets.TrailCacheLength[projectile.type] = 3;
        }

        public override void SetDefaults()
        {
            projectile.width = projectile.height = 24;
            projectile.hostile = true;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;
            projectile.penetrate = -1;
            projectile.timeLeft = 240;
            projectile.Opacity = 0f;
            cooldownSlot = 1;
        }

        public override void AI()
        {
            projectile.Opacity = MathHelper.Clamp(projectile.Opacity + 0.2f, 0f, 1f);

            float maxSpeed = 14f;
            float acceleration = 1.023f;
            if (CalamityGlobalNPC.calamitas != -1 && Main.player[Main.npc[CalamityGlobalNPC.calamitas].target].Infernum_CalShadowHex().HexIsActive("Zeal"))
            {
                maxSpeed = 16f;
                acceleration = 1.029f;
            }

            if (projectile.velocity.Length() < maxSpeed)
                projectile.velocity *= acceleration;

            // Explode if on top of the shadow.
            if (CalamityGlobalNPC.calamitas != -1 && projectile.WithinRange(Main.npc[CalamityGlobalNPC.calamitas].Center, 34f))
            {
                NPC calShadow = Main.npc[CalamityGlobalNPC.calamitas];
                projectile.Center = calShadow.Center + Vector2.UnitX * calShadow.scale * calShadow.spriteDirection * 12f;
                projectile.Kill();
            }

            projectile.frameCounter++;
            projectile.frame = projectile.frameCounter / 5 % Main.projFrames[projectile.type];
            projectile.rotation = projectile.velocity.ToRotation() - MathHelper.PiOver2;
        }

        public override void Kill(int timeLeft)
        {
            // Explode into dark magic clouds and particles.
            for (int i = 0; i < 10; i++)
            {
                Dust darkMagic = Dust.NewDustPerfect(projectile.Center + Main.rand.NextVector2Circular(30f, 30f), 261);
                darkMagic.color = Color.Lerp(Color.DarkBlue, Color.HotPink, Main.rand.NextFloat(0.7f));
                darkMagic.scale = Main.rand.NextFloat(1f, 1.1f);
                darkMagic.velocity = Main.rand.NextVector2Circular(2f, 2f);
                darkMagic.noGravity = true;
            }

            for (int i = 0; i < 8; i++)
            {
                if (!Main.rand.NextBool(5))
                    continue;

                Color fireColor = Main.rand.NextBool() ? Color.HotPink : Color.DarkBlue;
                CloudParticle fireCloud = new CloudParticle(projectile.Center, (MathHelper.TwoPi * i / 8f).ToRotationVector2() * 2f + Main.rand.NextVector2Circular(0.3f, 0.3f), fireColor, Color.DarkGray, 23, Main.rand.NextFloat(1.8f, 2f))
                {
                    Rotation = Main.rand.NextFloat(MathHelper.TwoPi)
                };
                GeneralParticleHandler.SpawnParticle(fireCloud);
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Texture2D texture = Main.projectileTexture[projectile.type];
            Vector2 drawPosition = projectile.Center - Main.screenPosition;
            Rectangle frame = texture.Frame(1, Main.projFrames[projectile.type], 0, projectile.frame);
            Vector2 origin = frame.Size() * 0.5f;

            for (int i = 0; i < 12; i++)
            {
                Vector2 drawOffset = (MathHelper.TwoPi * i / 12f).ToRotationVector2() * 2f;
                Main.spriteBatch.Draw(texture, drawPosition + drawOffset, frame, new Color(1f, 1f, 1f, 0f) * projectile.Opacity * 0.65f, projectile.rotation, origin, projectile.scale, SpriteEffects.None, 0f);
            }
            CalamityUtils.DrawAfterimagesCentered(projectile, ProjectileID.Sets.TrailingMode[projectile.type], lightColor);
            return false;
        }
    }
}
