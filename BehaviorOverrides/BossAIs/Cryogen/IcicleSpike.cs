using CalamityMod.Events;
using CalamityMod.Particles;
using InfernumMode.Particles;
using InfernumMode.GlobalInstances;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.Cryogen
{
    public class IcicleSpike : ModProjectile
    {
        public float InwardRadiusOffset
        {
            get;
            set;
        }

        public static float SpeedPower => BossRushEvent.BossRushActive ? 1.122f : 0.66f;

        public ref float Time => ref projectile.localAI[0];

        public ref float OffsetRotation => ref projectile.ai[0];

        public NPC Owner => Main.npc[(int)projectile.ai[1]];

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Icicle Spike");
        }

        public override void SetDefaults()
        {
            projectile.width = projectile.height = 28;
            projectile.hostile = true;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;
            projectile.timeLeft = 240;
            projectile.alpha = 255;
            cooldownSlot = 1;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(Time);
            writer.Write(InwardRadiusOffset);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            Time = reader.ReadSingle();
            InwardRadiusOffset = reader.ReadSingle();
        }

        public override void AI()
        {
            if (!Main.npc.IndexInRange((int)projectile.ai[1]) || !Owner.active)
            {
                projectile.Kill();
                return;
            }

            if (projectile.alpha > 0)
                projectile.alpha -= 12;

            if (Time < 65f)
                OffsetRotation += MathHelper.TwoPi * 2f / 55f * Utils.InverseLerp(30f, 60f, Time, true);
            if (Time == 80f)
                projectile.velocity = Owner.SafeDirectionTo(projectile.Center) * SpeedPower * 9f;
            if (Time > 80f && projectile.velocity.Length() < SpeedPower * 33f)
                projectile.velocity *= 1f + SpeedPower * 0.03f;

            if (Time <= 80f)
            {
                // Make the bomb radius fade away if the projectile itself is fading away.
                float offsetRadius = MathHelper.Lerp(110f, 72f, SpeedPower) - InwardRadiusOffset;
                projectile.Center = Owner.Center + OffsetRotation.ToRotationVector2() * offsetRadius;
            }
            else if (Time % 10 == 0)
            {
                // Leave a trail of particles.
                Particle iceParticle = new SnowyIceParticle(projectile.Center, projectile.velocity * 0.5f, Color.White, Main.rand.NextFloat(0.75f, 0.95f), 30);
                GeneralParticleHandler.SpawnParticle(iceParticle);
            }

            projectile.rotation = Time > 80f ? projectile.velocity.ToRotation() : Owner.AngleTo(projectile.Center);
            projectile.rotation -= MathHelper.PiOver2;

            Time++;
        }

        public override bool CanDamage() => projectile.alpha < 20;

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Texture2D texture = Main.projectileTexture[projectile.type];

            // Draw backglow effects.
            for (int i = 0; i < 12; i++)
            {
                Vector2 afterimageOffset = (MathHelper.TwoPi * i / 12f).ToRotationVector2() * 4f;
                Color afterimageColor = new Color(46, 188, 234, 0f) * 0.2f * projectile.Opacity;
                Main.spriteBatch.Draw(texture, projectile.Center - Main.screenPosition + afterimageOffset, null, projectile.GetAlpha(afterimageColor), projectile.rotation, texture.Size() * 0.5f, projectile.scale, SpriteEffects.None, 0f);
            }
            Main.spriteBatch.Draw(texture, projectile.Center - Main.screenPosition, null, Color.White * projectile.Opacity, projectile.rotation, texture.Size() * 0.5f, 1, 0, 0);
            return false;
        }
    }
}
