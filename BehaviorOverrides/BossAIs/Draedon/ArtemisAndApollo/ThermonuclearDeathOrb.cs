using CalamityMod;
using InfernumMode.Effects;
using InfernumMode.ExtraTextures;
using InfernumMode.Graphics.Primitives;
using InfernumMode.BehaviorOverrides.BossAIs.Draedon.Ares;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace InfernumMode.BehaviorOverrides.BossAIs.Draedon.ArtemisAndApollo
{
    public class ThermonuclearDeathOrb : ModProjectile
    {
        public PrimitiveTrailCopy FireDrawer;

        public NPC Owner => Main.npc.IndexInRange((int)projectile.ai[1]) && Main.npc[(int)projectile.ai[1]].active ? Main.npc[(int)projectile.ai[1]] : null;

        public float Radius => Owner.Infernum().ExtraAI[2];

        public ref float Time => ref projectile.ai[0];

        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Thermonuclear Death Orb");
        }

        public override void SetDefaults()
        {
            projectile.width = 164;
            projectile.height = 164;
            projectile.hostile = true;
            projectile.ignoreWater = true;
            projectile.tileCollide = false;
            projectile.timeLeft = 9000;
            projectile.scale = 0.2f;
            cooldownSlot = 1;
        }

        public override void AI()
        {
            if (Owner is null)
            {
                projectile.Kill();
                return;
            }

            // Drift towards the nearest target.
            Player target = Main.player[Player.FindClosest(projectile.Center, 1, 1)];
            if (projectile.velocity.Length() > 0.02f)
            {
                float flySpeed = projectile.Distance(target.Center) * 0.0064f + 5.5f;
                projectile.velocity = Vector2.Lerp(projectile.velocity, projectile.SafeDirectionTo(target.Center) * flySpeed, 0.05f);
                projectile.velocity = projectile.velocity.ClampMagnitude(1f, 26f);
            }

            // Periodically release bursts of plasma bolts.
            if (Main.netMode != NetmodeID.MultiplayerClient && Time % 105f == 104f)
            {
                for (int i = 0; i < 24; i++)
                {
                    Vector2 plasmaVelocity = (MathHelper.TwoPi * i / 24f).ToRotationVector2() * 5f;
                    Vector2 plasmaSpawnPosition = projectile.Center + plasmaVelocity.SafeNormalize(Vector2.UnitY) * Radius * 0.5f;
                    Utilities.NewProjectileBetter(plasmaSpawnPosition, plasmaVelocity, ModContent.ProjectileType<SmallPlasmaSpark>(), DraedonBehaviorOverride.StrongerNormalShotDamage, 0f);
                }
            }

            Time++;
        }

        public float OrbWidthFunction(float completionRatio) => MathHelper.SmoothStep(0f, Radius, (float)Math.Sin(MathHelper.Pi * completionRatio));

        public Color OrbColorFunction(float completionRatio)
        {
            Color c = Color.Lerp(Color.Orange, Color.ForestGreen, MathHelper.Lerp(0.2f, 0.8f, projectile.localAI[0] % 1f));
            c = Color.Lerp(c, Color.White, completionRatio * 0.5f);
            c.A = 0;
            return c;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            if (Owner is null || !Owner.active)
                return false;

            if (FireDrawer == null)
                FireDrawer = new PrimitiveTrailCopy(OrbWidthFunction, OrbColorFunction, null, true, InfernumEffectsRegistry.PrismaticRayVertexShader);

            InfernumEffectsRegistry.PrismaticRayVertexShader.UseOpacity(0.25f);
            InfernumEffectsRegistry.PrismaticRayVertexShader.UseImage("Images/Misc/Perlin");
            Main.instance.GraphicsDevice.Textures[2] = InfernumTextureRegistry.StreakSolid;

            List<Vector2> drawPoints = new List<Vector2>();

            Main.spriteBatch.EnterShaderRegion();
            int pointCount = InfernumConfig.Instance.ReducedGraphicsConfig ? 3 : 8;
            int sampleCount = InfernumConfig.Instance.ReducedGraphicsConfig ? 15 : 30;
            for (float offsetAngle = -MathHelper.PiOver2; offsetAngle <= MathHelper.PiOver2; offsetAngle += 30f)
            {
                projectile.localAI[0] = MathHelper.Clamp((offsetAngle + MathHelper.PiOver2) / MathHelper.Pi, 0f, 1f);

                drawPoints.Clear();

                float adjustedAngle = offsetAngle + CalamityUtils.PerlinNoise2D(offsetAngle, Main.GlobalTime * 0.02f, 3, 185) * 2f;
                Vector2 offsetDirection = adjustedAngle.ToRotationVector2();
                for (int i = 0; i < pointCount; i++)
                    drawPoints.Add(Vector2.Lerp(projectile.Center - offsetDirection * Radius / 2f, projectile.Center + offsetDirection * Radius / 2f, i / (float)(pointCount - 1)));

                FireDrawer.Draw(drawPoints, -Main.screenPosition, sampleCount);
            }
            Main.spriteBatch.ExitShaderRegion();
            return false;
        }

        public override bool CanDamage() => projectile.velocity.Length() > 0.02f;

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => Utilities.CircularCollision(projectile.Center, targetHitbox, Radius * 0.64f);
    }
}
