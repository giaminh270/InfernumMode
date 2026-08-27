using InfernumMode.Graphics.Interfaces;
using InfernumMode.BehaviorOverrides.BossAIs.WallOfFlesh;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.ProfanedGuardians
{
    public class HolyAimedDeathrayTelegraph : FireBeamTelegraph, IScreenCullDrawer
    {
        public override void SetDefaults()
        {
            base.SetDefaults();
            projectile.timeLeft = 72;
        }

        public override void AI()
        {
            // Determine an initial target.
            if (Main.netMode != NetmodeID.MultiplayerClient && projectile.localAI[0] == 0f)
            {
                TargetIndex = Player.FindClosest(projectile.Center, 1, 1);
                projectile.localAI[0] = 1f;
                projectile.netUpdate = true;
            }

            projectile.scale = Utils.InverseLerp(0f, 10f, projectile.timeLeft, true) * Utils.InverseLerp(85f, 75f, projectile.timeLeft, true);
            projectile.scale = MathHelper.SmoothStep(0.04f, 4f, projectile.scale);

            // Try to aim at the target.
            if (projectile.timeLeft > 32f)
                projectile.velocity = Vector2.Lerp(projectile.velocity, projectile.SafeDirectionTo(Target.Center), 0.09f);

            projectile.Center = Owner.Center + projectile.velocity.SafeNormalize(Vector2.UnitY) * 70f;

            if (!Owner.active)
                projectile.Kill();

            if (projectile.timeLeft > 30)
            {
                for (int i = 0; i < 2; i++)
                {
                    Dust fire = Dust.NewDustPerfect(projectile.Center + Main.rand.NextVector2CircularEdge(40f, 40f), 264);
                    fire.color = Color.Orange;
                    fire.velocity = (projectile.Center - fire.position) * 0.08f;
                    fire.fadeIn = 0.5f;
                    fire.noGravity = true;
                    fire.noLight = true;
                }
            }
        }

		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor) => false;

        public override void Kill(int timeLeft)
        {
            if (Target.Center.Y > (Main.maxTilesY - 300f) * 16f)
                Main.PlaySound(SoundID.Item74, Target.Center);
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            Vector2 beamDirection = projectile.velocity.SafeNormalize(Vector2.UnitY);
            Utilities.NewProjectileBetter(projectile.Center, beamDirection, ModContent.ProjectileType<HolyAimedDeathray>(), GuardianComboAttackManager.HolyFireBeamDamage, 0f, -1, 0f, Owner.whoAmI);
        }

        public void CullDraw(SpriteBatch spriteBatch)
        {
            Vector2 aimDirection = projectile.velocity.SafeNormalize(Vector2.UnitY);

            for (int i = 0; i <= 4; i++)
            {
                float lineWidth = MathHelper.SmoothStep(0.25f, 1f, i / 4f) * projectile.scale;
                Color lineColor = Color.Lerp(Color.White, Color.Orange, MathHelper.Lerp(0.15f, 1f, i / 4f));
                lineColor.A = 0;

                Main.spriteBatch.DrawLineBetter(projectile.Center, projectile.Center + aimDirection * 8200f, lineColor, lineWidth);
            }
        }
    }
}
