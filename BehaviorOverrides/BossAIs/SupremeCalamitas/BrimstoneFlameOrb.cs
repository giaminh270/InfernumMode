using CalamityMod;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.SupremeCalamitas
{
    public class BrimstoneFlameOrb : ModProjectile
    {
        public PrimitiveTrailCopy FireDrawer;

        public NPC Owner => Main.npc.IndexInRange((int)projectile.ai[1]) && Main.npc[(int)projectile.ai[1]].active ? Main.npc[(int)projectile.ai[1]] : null;

        public static int LaserCount => 5;

        public float TelegraphInterpolant => Utils.InverseLerp(20f, LaserReleaseDelay, Time, true);

        public float Radius => Owner.Infernum().ExtraAI[0] * (1f - Owner.Infernum().ExtraAI[1]);

        public ref float Time => ref projectile.ai[0];

        public const int OverloadBeamLifetime = 300;

        public const int LaserReleaseDelay = 125;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Brimstone Flame Orb");
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
        }

        public override void AI()
        {
            if (Owner is null)
            {
                projectile.Kill();
                return;
            }

            // Die after sufficiently shrunk.
            if (Owner.Infernum().ExtraAI[1] >= 1f)
            {
                projectile.Kill();
                return;
            }

            // Release beams outward once ready.
            if (Time == LaserReleaseDelay)
            {
                Main.PlaySound(InfernumMode.Instance.GetLegacySoundSlot(SoundType.Custom, "Sounds/Custom/WyrmElectricCharge"), projectile.Center);

                for (int i = 0; i < LaserCount; i++)
                {
                    Vector2 laserDirection = (MathHelper.TwoPi * i / LaserCount + 0.8f).ToRotationVector2();
                    int laser = Utilities.NewProjectileBetter(projectile.Center, laserDirection, ModContent.ProjectileType<FlameOverloadBeam>(), 900, 0f);
                    if (Main.projectile.IndexInRange(laser))
                        Main.projectile[laser].ai[0] = Owner.whoAmI;
                }
            }
            
            Time++;
        }

        public float OrbWidthFunction(float completionRatio) => MathHelper.SmoothStep(0f, Radius, (float)Math.Sin(MathHelper.Pi * completionRatio));

        public Color OrbColorFunction(float completionRatio)
        {
            Color c = Color.Lerp(Color.Yellow, Color.Red, MathHelper.Lerp(0.2f, 0.8f, projectile.localAI[0] % 1f));
            c = Color.Lerp(c, Color.White, completionRatio * 0.5f);
            c.A = 0;
            return c;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            if (Owner is null || !Owner.active)
                return false;

            if (FireDrawer is null)
                FireDrawer = new PrimitiveTrailCopy(OrbWidthFunction, OrbColorFunction, null, true, GameShaders.Misc["Infernum:PrismaticRay"]);

            GameShaders.Misc["Infernum:PrismaticRay"].UseOpacity(0.25f);
            GameShaders.Misc["Infernum:PrismaticRay"].UseImage("Images/Misc/Perlin");
            Main.instance.GraphicsDevice.Textures[2] = ModContent.GetTexture("InfernumMode/ExtraTextures/PrismaticLaserbeamStreak");

            List<float> rotationPoints = new List<float>();
            List<Vector2> drawPoints = new List<Vector2>();

            // Draw telegraphs.
            if (TelegraphInterpolant >= 0 && TelegraphInterpolant < 1)
            {
                float telegraphWidth = MathHelper.Lerp(1f, 6f, TelegraphInterpolant);
                for (int i = 0; i < LaserCount; i++)
                {
                    Vector2 laserDirection = (MathHelper.TwoPi * i / LaserCount + 0.8f).ToRotationVector2();
                    Vector2 start = projectile.Center;
                    Vector2 end = projectile.Center + laserDirection * 4200f;
                    Color telegraphColor = Color.Orange * (float)Math.Pow(TelegraphInterpolant, 0.67);
                    Main.spriteBatch.DrawLineBetter(start, end, telegraphColor, telegraphWidth);
                }
            }

            Main.spriteBatch.EnterShaderRegion();
            for (float offsetAngle = -MathHelper.PiOver2; offsetAngle <= MathHelper.PiOver2; offsetAngle += MathHelper.Pi / 30f)
            {
                projectile.localAI[0] = MathHelper.Clamp((offsetAngle + MathHelper.PiOver2) / MathHelper.Pi, 0f, 1f);

                rotationPoints.Clear();
                drawPoints.Clear();

                float adjustedAngle = offsetAngle + CalamityUtils.PerlinNoise2D(offsetAngle, Main.GlobalTime * 0.02f, 3, 185) * 3f;
                Vector2 offsetDirection = adjustedAngle.ToRotationVector2();
                for (int i = 0; i < 4; i++)
                {
                    rotationPoints.Add(adjustedAngle);
                    drawPoints.Add(Vector2.Lerp(projectile.Center - offsetDirection * Radius / 2f, projectile.Center + offsetDirection * Radius / 2f, i / 3f));
                }

                FireDrawer.Draw(drawPoints, -Main.screenPosition, 30);
            }
            Main.spriteBatch.ExitShaderRegion();
            return false;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => Utilities.CircularCollision(projectile.Center, targetHitbox, Radius * 0.85f);
    }
}
