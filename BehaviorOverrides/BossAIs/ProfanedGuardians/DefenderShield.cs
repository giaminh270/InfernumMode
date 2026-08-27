using CalamityMod;
using CalamityMod.NPCs.ProfanedGuardians;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using Terraria;
using Terraria.ModLoader;
using static InfernumMode.BehaviorOverrides.BossAIs.ProfanedGuardians.GuardianComboAttackManager;
using InfernumMode.Projectiles;

namespace InfernumMode.BehaviorOverrides.BossAIs.ProfanedGuardians
{
    public class DefenderShield : ModProjectile
    {
        public ref float Timer => ref projectile.ai[0];

        public NPC Owner => Main.npc[(int)projectile.ai[1]];

        public DefenderShieldStatus Status => (DefenderShieldStatus)Owner.Infernum().ExtraAI[DefenderShieldStatusIndex];

        public static int GlowTime => 30;

        public Vector2 PositionOffset;

        public override void SetStaticDefaults() => DisplayName.SetDefault("Rock Shield");

        public override void SetDefaults()
        {
            projectile.width = 60;
            projectile.height = 110;
            projectile.hostile = true;
            projectile.Opacity = 0;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;
            projectile.penetrate = -1;
            projectile.timeLeft = 2000;
        }

        public override void AI()
        {
            bool shouldKill = Status is DefenderShieldStatus.MarkedForRemoval;
            if (!Owner.active || Owner.type != ModContent.NPCType<ProfanedGuardianBoss2>() || shouldKill)
            {
                // Reset this index.
                Owner.Infernum().ExtraAI[DefenderShieldStatusIndex] = 0;
                projectile.Kill();
                return;
            }

            // Move where the defender is aiming.
            if (Status is DefenderShieldStatus.ActiveAndAiming)
            {
                Vector2 idealOffset = Owner.SafeDirectionTo(Main.player[Owner.target].Center) * 60f;
                PositionOffset = CalamityUtils.MoveTowards(PositionOffset, idealOffset, 7f);
                projectile.rotation = PositionOffset.ToRotation();
                projectile.netUpdate = true;
                projectile.netSpam = 0;
            }
            projectile.Center = Owner.Center + PositionOffset;
            projectile.rotation = PositionOffset.ToRotation();
            projectile.spriteDirection = (projectile.SafeDirectionTo(Main.player[Owner.target].Center).X > 0f).ToDirectionInt();

            projectile.Opacity = MathHelper.Clamp(projectile.Opacity + 0.05f, 0f, 1f);
            projectile.timeLeft = 2000;
            Timer++;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(PositionOffset.X);
            writer.Write(PositionOffset.Y);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            PositionOffset.X = reader.ReadSingle();
            PositionOffset.Y = reader.ReadSingle();
        }

        public override bool CanDamage() => false;

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            SpriteEffects spriteEffects = projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipVertically;
            Texture2D texture = ModContent.GetTexture(Texture);

            float backglowAmount = 12f;
            for (int i = 0; i < backglowAmount; i++)
            {
                Vector2 backglowOffset = (MathHelper.TwoPi * i / backglowAmount).ToRotationVector2() * 4f;
                Color backglowColor = WayfinderSymbol.Colors[1];
                backglowColor.A = 0;
                Main.spriteBatch.Draw(texture, projectile.Center + backglowOffset - Main.screenPosition, null, backglowColor * MathHelper.Clamp(projectile.Opacity * 2f, 0f, 1f) * Owner.Opacity, projectile.rotation, texture.Size() * 0.5f, projectile.scale, spriteEffects, 0);
            }

            Main.spriteBatch.Draw(texture, projectile.Center - Main.screenPosition, null, lightColor * projectile.Opacity * Owner.Opacity, projectile.rotation, texture.Size() * 0.5f, projectile.scale, spriteEffects, 0);
            return false;
        }
    }
}
