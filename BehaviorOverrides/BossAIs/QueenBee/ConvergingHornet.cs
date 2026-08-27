using CalamityMod;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.QueenBee
{
    public class ConvergingHornet : ModProjectile
    {
        public enum HornetAttackState
        {
            MoveTowardsQueen,
            HoverAroundQueen,
            FlyOutward
        }

        public HornetAttackState CurrentAttackState
        {
            get => (HornetAttackState)projectile.ai[0];
            set => projectile.ai[0] = (int)value;
        }

        public ref float Time => ref projectile.ai[1];

        public static NPC QueenBee
        {
            get
            {
                int queenIndex = NPC.FindFirstNPC(NPCID.QueenBee);
                if (queenIndex != -1)
                    return Main.npc[queenIndex];

                return null;
            }
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Hornet");
            Main.projFrames[projectile.type] = 3;
        }

        public override void SetDefaults()
        {
            projectile.width = projectile.height = 40;
            projectile.ignoreWater = true;
            projectile.timeLeft = 240;
            projectile.scale = 1f;
            projectile.tileCollide = false;
            projectile.friendly = false;
            projectile.hostile = true;
            projectile.timeLeft = 600;
            projectile.Opacity = 0f;
            projectile.Calamity().canBreakPlayerDefense = true;
            cooldownSlot = 1;
        }

        public override void AI()
        {
            // Disappear if the queen bee is not present.
            if (QueenBee is null)
                return;

            projectile.rotation = MathHelper.Clamp(projectile.velocity.X * 0.15f, -0.7f, 0.7f);
            projectile.spriteDirection = (projectile.velocity.X < 0f).ToDirectionInt();
            projectile.frame = projectile.timeLeft / 4 % Main.projFrames[projectile.type];
            projectile.Opacity = MathHelper.Clamp(projectile.Opacity + 0.03f, 0f, 1f);

            if (projectile.localAI[0] == 0f)
            {
                Main.PlaySound(SoundID.Item17, projectile.Center);
                projectile.localAI[0] = 1f;
            }

            switch (CurrentAttackState)
            {
                case HornetAttackState.MoveTowardsQueen:
                    DoBehavior_MoveTowardsQueen();
                    break;
                case HornetAttackState.HoverAroundQueen:
                    DoBehavior_HoverAroundQueen();
                    break;
                case HornetAttackState.FlyOutward:
                    DoBehavior_FlyOutward();
                    break;
            }

            Time++;
        }

        public void DoBehavior_MoveTowardsQueen()
        {
            // Move with perfect homing towards the queen bee.
            projectile.velocity = projectile.SafeDirectionTo(QueenBee.Center) * 9.6f;

            if (projectile.WithinRange(QueenBee.Center, 150f))
            {
                CurrentAttackState = HornetAttackState.HoverAroundQueen;
                Time = 0f;
                projectile.netUpdate = true;
            }
        }

        public void DoBehavior_HoverAroundQueen()
        {
            float hoverOffset = MathHelper.Lerp(120f, 200f, projectile.identity / 9f % 1f);
            Vector2 hoverDestination = QueenBee.Center + (MathHelper.TwoPi * projectile.identity / 11f + Time / 50f).ToRotationVector2() * hoverOffset;
            Vector2 idealVelocity = projectile.SafeDirectionTo(hoverDestination) * 9f;
            projectile.velocity = Vector2.Lerp(projectile.velocity, idealVelocity, 0.1f);
            projectile.spriteDirection = (QueenBee.Center.X > projectile.Center.X).ToDirectionInt();
        }

        public void DoBehavior_FlyOutward()
        {
            projectile.velocity = Vector2.Lerp(projectile.velocity, projectile.SafeDirectionTo(QueenBee.Center) * -18f, 0.09f);
            projectile.spriteDirection = (QueenBee.Center.X > projectile.Center.X).ToDirectionInt();
        }

        public static void MakeAllBeesFlyOutward()
        {
            foreach (Projectile bee in Utilities.AllProjectilesByID(ModContent.ProjectileType<ConvergingHornet>()))
            {
                bee.ModProjectile<ConvergingHornet>().CurrentAttackState = HornetAttackState.FlyOutward;
                bee.netUpdate = true;
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            projectile.DrawProjectileWithBackglowTemp(Color.White * (float)Math.Pow(projectile.Opacity, 2f), lightColor, projectile.Opacity * 6f);
            return false;
        }

        public override bool CanDamage() => CurrentAttackState != HornetAttackState.HoverAroundQueen;
    }
}
