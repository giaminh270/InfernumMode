using CalamityMod.NPCs;
using InfernumMode.Particles;
using InfernumMode.Sounds;
using InfernumMode.Graphics;
using InfernumMode.BehaviorOverrides.BossAIs.ProfanedGuardians;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using static InfernumMode.BehaviorOverrides.BossAIs.Providence.ProvidenceBehaviorOverride;

namespace InfernumMode.BehaviorOverrides.BossAIs.Providence
{
    public class CommanderSpear2 : ModProjectile
    {
        public int OwnerIndex
        {
            get => (int)projectile.ai[0];
            set => projectile.ai[0] = (int)value;
        }

        public NPC Owner => Main.npc[OwnerIndex];

        public static SpearAttackState CurrentBehavior => (SpearAttackState)Main.npc[CalamityGlobalNPC.holyBoss].Infernum().ExtraAI[0];

        public static float CircularSmearInterpolant => Main.npc[CalamityGlobalNPC.holyBoss].Infernum().ExtraAI[1];

        public ref float Time => ref projectile.ai[1];

        public override void SetStaticDefaults() => DisplayName.SetDefault("Holy Spear");

        public override void SetDefaults()
        {
            projectile.width = projectile.height = 124;
            projectile.hostile = true;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;
            projectile.penetrate = -1;
            projectile.timeLeft = 7200;
            projectile.Opacity = 0f;
            projectile.netImportant = true;
            
            cooldownSlot = 1;
        }

        public override void SendExtraAI(BinaryWriter writer) => writer.Write(OwnerIndex);

        public override void ReceiveExtraAI(BinaryReader reader) => OwnerIndex = reader.ReadInt32();

        public override void AI()
        {
            // Disappear if the owner or Providence are not present.
            bool notActuallyCharging = CurrentBehavior == SpearAttackState.Charge && projectile.velocity == Vector2.Zero;
            if (OwnerIndex == -1 || CalamityGlobalNPC.holyBoss == -1 || (!Owner.active && CurrentBehavior != SpearAttackState.Charge) || notActuallyCharging)
            {
                projectile.active = false;
                return;
            }

            bool stickToOwner = true;
            switch (CurrentBehavior)
            {
                case SpearAttackState.LookAtTarget:
                    float idealRotation = projectile.AngleTo(Main.player[Owner.target].Center) + MathHelper.PiOver4;
                    projectile.rotation = projectile.rotation.AngleLerp(idealRotation, 0.11f).AngleTowards(idealRotation, 0.032f);
                    break;
                case SpearAttackState.SpinInPlace:
                    projectile.rotation += 0.1f * MathHelper.Pi * Owner.spriteDirection;
                    break;
                case SpearAttackState.Charge:
                    projectile.rotation = projectile.velocity.ToRotation() + MathHelper.PiOver4;
                    projectile.tileCollide = true;
                    if (projectile.timeLeft >= 240)
                        projectile.timeLeft = 240;

                    if (projectile.velocity.Length() < 38f)
                        projectile.velocity *= 1.0094f;
                    stickToOwner = false;
                    break;
            }

            // Stick to the owner if necessary.
            if (stickToOwner)
            {
                projectile.Center = Owner.Center + projectile.rotation.ToRotationVector2() * 20f;
                projectile.Opacity = MathHelper.Clamp(projectile.Opacity + 0.05f, 0f, 1f);
            }

            Time++;
        }

        public override bool CanDamage() => CurrentBehavior == SpearAttackState.Charge;

        public override void Kill(int timeLeft)
        {
            if (timeLeft <= 5)
                return;

            // Burst into lava metaballs on death.
            if (Main.netMode != NetmodeID.Server)
                ModContent.GetTexture(Texture).CreateMetaballsFromTexture(ref InfernumFusableParticleManager.GetParticleSetByType<ProfanedLavaParticleSet>().Particles, projectile.Center, projectile.rotation, projectile.scale, 20f, 30);

            // Release accelerating spears outward.
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                Utilities.NewProjectileBetter(projectile.Center + projectile.velocity.SafeNormalize(Vector2.UnitY) * 60f, projectile.velocity, ModContent.ProjectileType<StrongProfanedCrack>(), 0, 0f);

                float shootOffsetAngle = Main.rand.NextFloat(MathHelper.TwoPi);
                for (int i = 0; i < 15; i++)
                {
                    Vector2 spearDirection = (MathHelper.TwoPi * i / 15f + shootOffsetAngle).ToRotationVector2();
                    Utilities.NewProjectileBetter(projectile.Center, spearDirection * 0.01f, ModContent.ProjectileType<CrystalTelegraphLine>(), 0, 0f, -1, 0f, 54f);
                    Utilities.NewProjectileBetter(projectile.Center, spearDirection * 8f, ModContent.ProjectileType<ProfanedSpearInfernum>(), HolySpearDamage, 0f);
                }
            }

            Main.LocalPlayer.Infernum().CurrentScreenShakePower = Math.Max(Main.LocalPlayer.Infernum().CurrentScreenShakePower, 8f);
            ScreenEffectSystem.SetFlashEffect(projectile.Center, 1f, 13);

            Main.PlaySound(InfernumSoundRegistry.ProvidenceSpearHitSound, projectile.Center);
        }

        public override Color? GetAlpha(Color lightColor) => Color.White * projectile.Opacity;

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Texture2D texture = ModContent.GetTexture(Texture);
            if (IsEnraged)
                texture = ModContent.GetTexture("InfernumMode/BehaviorOverrides/BossAIs/Providence/CommanderSpear2Night");

            if (CircularSmearInterpolant > 0f)
            {
                Texture2D smear = ModContent.GetTexture("CalamityMod/Particles/SemiCircularSmear");
                float opacity = CircularSmearInterpolant * 0.4f;
                float rotation = projectile.rotation + MathHelper.PiOver2 * 1.1f;
                Main.spriteBatch.Draw(smear, projectile.Center - Main.screenPosition, null, Color.Gold * opacity, rotation, smear.Size() * 0.5f, 1f, SpriteEffects.None, 0);
            }

            float backglowAmount = 12f;
            for (int i = 0; i < backglowAmount; i++)
            {
                Vector2 backglowOffset = (MathHelper.TwoPi * i / backglowAmount).ToRotationVector2() * 4f;
                Color backglowColor = IsEnraged ? Color.Cyan : Color.Gold;
                backglowColor.A = 0;
                Main.spriteBatch.Draw(texture, projectile.Center + backglowOffset - Main.screenPosition, null, backglowColor * MathHelper.Clamp(projectile.Opacity * 2f, 0f, 1f) * Owner.Opacity, projectile.rotation, texture.Size() * 0.5f, projectile.scale, SpriteEffects.None, 0);
            }

            Main.spriteBatch.Draw(texture, projectile.Center - Main.screenPosition, null, lightColor * projectile.Opacity * Owner.Opacity, projectile.rotation, texture.Size() * 0.5f, projectile.scale, SpriteEffects.None, 0);
            return false;
        }
    }
}
