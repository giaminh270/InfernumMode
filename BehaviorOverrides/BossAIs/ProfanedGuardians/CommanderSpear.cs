using CalamityMod.NPCs.ProfanedGuardians;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static InfernumMode.BehaviorOverrides.BossAIs.ProfanedGuardians.GuardianComboAttackManager;
using InfernumMode.Projectiles;
using System;

namespace InfernumMode.BehaviorOverrides.BossAIs.ProfanedGuardians
{
    public class CommanderSpear : ModProjectile
    {
        public ref float Timer => ref projectile.ai[0];

        public NPC Owner => Main.npc[(int)projectile.ai[1]];

        // This uses this enum due to needing the same states.
        public DefenderShieldStatus Status => (DefenderShieldStatus)Owner.Infernum().ExtraAI[CommanderSpearStatusIndex];

        public float SpearRotation => Owner.Infernum().ExtraAI[CommanderSpearRotationIndex];

        public static int GlowTime => 30;

        public float PositionOffset => Owner.Infernum().ExtraAI[CommanderSpearPositionOffsetIndex];

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Holy Spear");
            ProjectileID.Sets.TrailingMode[projectile.type] = 2;
            ProjectileID.Sets.TrailCacheLength[projectile.type] = 5;
        }

        public override void SetDefaults()
        {
            projectile.width = 132;
            projectile.height = 132;
            projectile.hostile = true;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;
            projectile.Opacity = 0;
            projectile.timeLeft = 7000;
            projectile.penetrate = -1;
            cooldownSlot = 1;
        }

        public override void AI()
        {
            bool shouldKill = Status is DefenderShieldStatus.MarkedForRemoval;
            if (!Owner.active || Owner.type != ModContent.NPCType<ProfanedGuardianBoss>() || shouldKill)
            {
                // Reset this index.
                Owner.Infernum().ExtraAI[CommanderSpearStatusIndex] = 0;
                projectile.Kill();
                return;
            }
            Vector2 offset = projectile.rotation.ToRotationVector2() * ((AttackerGuardianBehaviorOverride.TotalRemaininGuardians == 1 ? 100f : 20f) + PositionOffset);

            // Move where the commander tells it to.
            if (Status is DefenderShieldStatus.ActiveAndAiming)
            {
                projectile.rotation = SpearRotation;
                projectile.netUpdate = true;
                projectile.netSpam = 0;

                if (AttackerGuardianBehaviorOverride.TotalRemaininGuardians == 1)
                    // Move the aiming hand while being stuck to it.
                    RightHandPosition = offset;
            }
            if (AttackerGuardianBehaviorOverride.TotalRemaininGuardians > 1)
                projectile.Center = Owner.Center + offset;
            else
            {
                Owner.Infernum().ExtraAI[HandsShouldUseNotDefaultPositionIndex] = 1f;
                projectile.Center = Owner.Center + offset;
            }

            projectile.Opacity = MathHelper.Clamp(projectile.Opacity + 0.05f, 0f, 1f);
            projectile.timeLeft = 2000;
        }

        public override bool CanHitPlayer(Player target)
        {
            return base.CanHitPlayer(target);
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Texture2D texture = ModContent.GetTexture(Texture);

            if (Owner.Infernum().ExtraAI[CommanderSpearSmearOpacityIndex] > 0f)
            {
                Texture2D smear = ModContent.GetTexture("CalamityMod/Particles/SemiCircularSmear");
                float opacity = Owner.Infernum().ExtraAI[CommanderSpearSmearOpacityIndex] * 0.4f;
                float rotation = projectile.rotation + MathHelper.PiOver2 * 1.1f;
                Color color = WayfinderSymbol.Colors[1];
				color.A = 0;
				Main.spriteBatch.Draw(smear, projectile.Center - Main.screenPosition, null, color * opacity, rotation, smear.Size() * 0.5f, 1f, SpriteEffects.None, 0);
            }

            float backglowAmount = 12f;
            for (int i = 0; i < backglowAmount; i++)
            {
                Vector2 backglowOffset = (MathHelper.TwoPi * i / backglowAmount).ToRotationVector2() * 4f;
                Color backglowColor = WayfinderSymbol.Colors[1];
                backglowColor.A = 0;
                Main.spriteBatch.Draw(texture, projectile.Center + backglowOffset - Main.screenPosition, null, backglowColor * MathHelper.Clamp(projectile.Opacity * 2f, 0f, 1f) * Owner.Opacity, projectile.rotation + MathHelper.PiOver4, texture.Size() * 0.5f, projectile.scale, SpriteEffects.None, 0);
            }

            Main.spriteBatch.Draw(texture, projectile.Center - Main.screenPosition, null, lightColor * projectile.Opacity * Owner.Opacity, projectile.rotation + MathHelper.PiOver4, texture.Size() * 0.5f, projectile.scale, SpriteEffects.None, 0);
            return false;
        }
    }
}
