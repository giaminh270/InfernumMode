using InfernumMode;
using System.IO;
using CalamityMod;
using CalamityMod.NPCs;
using CalamityMod.NPCs.Providence;
using CalamityMod.Particles;
using CalamityMod.Projectiles;
using InfernumMode.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using System;
using ProvidenceBoss = CalamityMod.NPCs.Providence.Providence;

namespace InfernumMode.BehaviorOverrides.BossAIs.Providence
{
    public class HolyBasicFireball : ModProjectile
    {
        public bool GuardiansType;

        public static int Variant => (int)(ProvidenceBehaviorOverride.IsEnraged ? -1 : 0);

        public override string Texture => "CalamityMod/Projectiles/StarProj";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Holy Fireball");
        }

        public override void SetDefaults()
        {
            projectile.width = projectile.height = 36;
            projectile.hostile = true;
            projectile.ignoreWater = true;
            projectile.tileCollide = false;
            projectile.Opacity = 0f;
            projectile.penetrate = -1;
            projectile.timeLeft = 240;
            projectile.scale = 0f;
            projectile.Calamity().canBreakPlayerDefense = true;
            cooldownSlot = 1;
        }

        public override void SendExtraAI(BinaryWriter writer) => writer.Write(projectile.timeLeft);

        public override void ReceiveExtraAI(BinaryReader reader) => projectile.timeLeft = reader.ReadInt32();

        public override void AI()
        {
            Lighting.AddLight(projectile.Center, 0.45f, 0.35f, 0f);

            if (projectile.ai[1] == 1f && CalamityGlobalNPC.holyBoss != -1 && projectile.WithinRange(Main.npc[CalamityGlobalNPC.holyBoss].Center, projectile.velocity.Length() * 1.96f + 28f))
                projectile.Kill();

            // Release fire particles.
            for (int i = 0; i < 3; i++)
            {
                Color fireColor = Color.Lerp(Color.Orange, Color.Red, Main.rand.NextFloat(0.2f, 0.4f));
                if (ProvidenceBehaviorOverride.IsEnraged)
                    fireColor = Color.Lerp(fireColor, Color.SkyBlue, 0.7f);

                fireColor = Color.Lerp(fireColor, Color.White, Main.rand.NextFloat(0.4f));
                float angularVelocity = Main.rand.NextFloat(0.035f, 0.08f);
                FireballParticle fire = new FireballParticle(projectile.Center, projectile.velocity * 0.6f, fireColor, 10, Main.rand.NextFloat(0.52f, 0.68f) * projectile.scale, 1f, true, Main.rand.NextBool().ToDirectionInt() * angularVelocity);
                GeneralParticleHandler.SpawnParticle(fire);
            }

            // Make the fire grow in size.
            projectile.scale = MathHelper.Clamp(projectile.scale + 0.067f, 0f, 1.2f);
            Vector2 newScale = Vector2.One * projectile.scale * 36f;
            if (projectile.Size != newScale)
                projectile.Size = newScale;

            if (GuardiansType)
            {
                if (projectile.velocity.Length() < 20f)
                    projectile.velocity *= 1.015f;
            }
            else
            {
                if (projectile.velocity.Length() < 16f)
                    projectile.velocity *= 1.01f;
            }
            projectile.rotation = projectile.velocity.ToRotation();
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            float scaleInterpolant = Utils.InverseLerp(15f, 30f, projectile.timeLeft, true) * Utils.InverseLerp(240f, 200f, projectile.timeLeft, true) * (1f + 0.1f * (float)Math.Cos(Main.GlobalTime % 30f / 0.5f * (MathHelper.Pi * 2f) * 3f)) * 0.225f;

            Texture2D texture = ModContent.GetTexture(Texture);
            Vector2 drawPos = projectile.Center - Main.screenPosition + new Vector2(0f, projectile.gfxOffY);
            Color baseColor = Utilities.GetProjectileColor(Variant, 255);
            baseColor.A = 0;
            Color colorA = baseColor;
            Color colorB = baseColor * 0.5f;
            colorA *= scaleInterpolant;
            colorB *= scaleInterpolant;
            Vector2 origin = texture.Size() / 2f;
            Vector2 scale = new Vector2(0.5f, 2f) * projectile.scale * scaleInterpolant;

            SpriteEffects spriteEffects = SpriteEffects.None;
            if (projectile.spriteDirection == -1)
                spriteEffects = SpriteEffects.FlipHorizontally;

            float upRight = projectile.rotation + MathHelper.PiOver4;
            float up = projectile.rotation + MathHelper.PiOver2;
            float upLeft = projectile.rotation + 3f * MathHelper.PiOver4;
            float left = projectile.rotation + MathHelper.Pi;
            Main.spriteBatch.Draw(texture, drawPos, null, colorA, upLeft, origin, scale, spriteEffects, 0);
            Main.spriteBatch.Draw(texture, drawPos, null, colorA, upRight, origin, scale, spriteEffects, 0);
            Main.spriteBatch.Draw(texture, drawPos, null, colorB, upLeft, origin, scale * 0.6f, spriteEffects, 0);
            Main.spriteBatch.Draw(texture, drawPos, null, colorB, upRight, origin, scale * 0.6f, spriteEffects, 0);
            Main.spriteBatch.Draw(texture, drawPos, null, colorA, up, origin, scale * 0.6f, spriteEffects, 0);
            Main.spriteBatch.Draw(texture, drawPos, null, colorA, left, origin, scale * 0.6f, spriteEffects, 0);
            Main.spriteBatch.Draw(texture, drawPos, null, colorB, up, origin, scale * 0.36f, spriteEffects, 0);
            Main.spriteBatch.Draw(texture, drawPos, null, colorB, left, origin, scale * 0.36f, spriteEffects, 0);

            return false;
        }

        public override void Kill(int timeLeft)
        {
            Main.PlaySound(SoundID.Item14, projectile.Center);

            CalamityGlobalProjectile.ExpandHitboxBy(projectile, 50);
            int dustType = Utilities.GetDustID(Variant);
            if (ProvidenceBehaviorOverride.IsEnraged)
                dustType = 187;

            for (int d = 0; d < 5; d++)
            {
                int holy = Dust.NewDust(projectile.position, projectile.width, projectile.height, dustType, 0f, 0f, 100, default, 2f);
                Main.dust[holy].velocity *= 3f;
                Main.dust[holy].noGravity = true;
                if (Main.rand.NextBool(2))
                {
                    Main.dust[holy].scale = 0.5f;
                    Main.dust[holy].fadeIn = 1f + Main.rand.Next(10) * 0.1f;
                }
            }
            for (int d = 0; d < 8; d++)
            {
                int fire = Dust.NewDust(projectile.position, projectile.width, projectile.height, dustType, 0f, 0f, 100, default, 3f);
                Main.dust[fire].noGravity = true;
                Main.dust[fire].velocity *= 5f;
                fire = Dust.NewDust(projectile.position, projectile.width, projectile.height, dustType, 0f, 0f, 100, default, 2f);
                Main.dust[fire].velocity *= 2f;
                Main.dust[fire].noGravity = true;
            }
        }

        public override void ModifyHitPlayer(Player target, ref int damage, ref bool crit)
        {
            // If the player is dodging, don't apply debuffs.
            if (damage <= 0)
                return;

            Utilities.ApplyHitEffects(target, Variant, 180, 0);
            projectile.Kill();
        }
    }
}
