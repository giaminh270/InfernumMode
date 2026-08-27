using CalamityMod;
using CalamityMod.NPCs;
using InfernumMode.Sounds;
using InfernumMode.BehaviorOverrides.BossAIs.Providence;
using InfernumMode.GlobalInstances;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using InfernumMode.Projectiles;
using InfernumMode.ExtraTextures;
using InfernumMode.Effects;

namespace InfernumMode.BehaviorOverrides.BossAIs.ProfanedGuardians
{
    public class HolyDogmaFireball : ModProjectile
    {
        public enum StateType
        {
            Growing,
            InitialFiring,
            Flinging,
            SlowdownMovement,
            FinalFiring
        }

        public ref float Timer => ref projectile.ai[0];

        public StateType CurrentState
        {
            get
            {
                return (StateType)projectile.ai[1];
            }
            set
            {
                projectile.ai[1] = (float)value;
            }
        }

        public static NPC Commander
        {
            get
            {
                if (Main.npc.IndexInRange(CalamityGlobalNPC.doughnutBoss))
                {
                    if (Main.npc[CalamityGlobalNPC.doughnutBoss].type == GuardianComboAttackManager.CommanderType)
                        return Main.npc[CalamityGlobalNPC.doughnutBoss];
                }
                return null;
            }
        }

        public static Player Target
        {
            get
            {
                if (Main.player.IndexInRange(Commander.target))
                    return Main.player[Commander.target];
                return null;
            }
        }

        public float GrowTime = 30f;

        public int InitialLaserTelegraphTime = 60;

        public int InitialLaserShootTime = 35;

        public float FlingSpeed = 15f;

        public float BeamAmount => CurrentState is StateType.InitialFiring ? 12f : 8f;

        public override string Texture => InfernumTextureRegistry.InvisPath;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Large Fireball");
        }

        public override void SetDefaults()
        {
            projectile.width = projectile.height = 400;
            projectile.hostile = true;
            projectile.Opacity = 0;
            projectile.scale = 0;
            projectile.penetrate = -1;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;
            projectile.timeLeft = 7000;
            cooldownSlot = 1;
        }

        public override void AI()
        {
            projectile.rotation = projectile.velocity.ToRotation();
            if (Commander is null)
            {
                projectile.Kill();
                return;
            }

            float telegraphMaxAngularVelocity = MathHelper.ToRadians(1.2f);

            // Handle setting the telegraph opacities.

            switch (CurrentState)
            {
                case StateType.Growing:
                    projectile.Opacity = MathHelper.Clamp(projectile.Opacity + 0.1f, 0f, 1f);
                    projectile.scale = MathHelper.Clamp(projectile.scale += 1f / GrowTime, 0f, 1f);
                    if (Timer >= GrowTime)
                    {
                        CurrentState++;
                        Timer = 0;
                        return;
                    }
                    break;

                case StateType.InitialFiring:
                    if (Timer == 0)
                    {
                        Main.PlaySound(InfernumSoundRegistry.ProvidenceBurnSound, projectile.Center);

                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            for (int i = 0; i < BeamAmount; i++)
                            {
                                float angularVelocity = Main.rand.NextFloat(0.65f, 1f) * Main.rand.NextFromList(-1f, 1f) * telegraphMaxAngularVelocity;
                                Vector2 laserDirection = (MathHelper.TwoPi * i / BeamAmount + Main.rand.NextFloatDirection() * 0.16f).ToRotationVector2();

                                ProjectileSpawnManagementSystem.PrepareProjectileForSpawning(laser =>
                                {
                                    laser.ModProjectile<HolyMagicLaserbeam>().LaserTelegraphTime = InitialLaserTelegraphTime;
                                    laser.ModProjectile<HolyMagicLaserbeam>().LaserShootTime = InitialLaserShootTime;
                                    laser.ModProjectile<HolyMagicLaserbeam>().FromGuardians = true;
                                });
                                Utilities.NewProjectileBetter(projectile.Center, laserDirection, ModContent.ProjectileType<HolyMagicLaserbeam>(), ProvidenceBehaviorOverride.MagicLaserbeamDamage, 0f, -1, angularVelocity);
                            }
                        }
                    }
                    else if (Timer >= InitialLaserTelegraphTime + InitialLaserShootTime)
                    {
                        CurrentState++;
                        Timer = 0;
                        return;
                    }
                    break;

                case StateType.Flinging:
                    float flingSpeedScalar = 1f;
                    float distanceToTarget = projectile.Distance(Target.Center);
                    float minDistance = 500f;
                    float maxDistance = 1000f;
                    if (distanceToTarget > minDistance)
                    {
                        flingSpeedScalar += MathHelper.Clamp(Utils.InverseLerp(500f, maxDistance, distanceToTarget, false), 0f, 2.5f);
                    }
                    projectile.velocity = projectile.SafeDirectionTo(Target.Center) * (FlingSpeed * flingSpeedScalar);
                    CurrentState++;
                    Timer = 0;
                    return;

                case StateType.SlowdownMovement:
                    projectile.velocity *= 0.98f;
                    if (projectile.velocity.Length() <= 10)
                    {
                        // Inform the commander that this has been launched.
                        Commander.Infernum().ExtraAI[GuardianComboAttackManager.CommanderDogmaFireballHasBeenYeetedIndex] = 1f;
                        CurrentState++;
                        Timer = 0;
                        return;
                    }
                    break;


                case StateType.FinalFiring:
                    projectile.velocity *= 0.98f;
                    if (Timer == 0)
                    {

                        Main.PlaySound(InfernumSoundRegistry.ProvidenceBurnSound, projectile.Center);

                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            for (int i = 1; i <= BeamAmount; i++)
                            {
                                float angularVelocity = Main.rand.NextFloat(0f, MathHelper.TwoPi) * Main.rand.NextFromList(-1f, 1f) * telegraphMaxAngularVelocity;
                                float setAngle = MathHelper.TwoPi * i / BeamAmount;
                                Vector2 laserDirection = (setAngle + Main.rand.NextFloat(0f, MathHelper.Pi) * Main.rand.NextFromList(-1f, 1f)).ToRotationVector2();

                                ProjectileSpawnManagementSystem.PrepareProjectileForSpawning(laser =>
                                {
                                    laser.ModProjectile<HolyMagicLaserbeam>().LaserTelegraphTime = 90;
                                    laser.ModProjectile<HolyMagicLaserbeam>().LaserShootTime = InitialLaserShootTime;
                                    laser.ModProjectile<HolyMagicLaserbeam>().FromGuardians = true;
                                    laser.ModProjectile<HolyMagicLaserbeam>().SetAngleToMoveTo = MathHelper.TwoPi * i / BeamAmount;
                                });
                                Utilities.NewProjectileBetter(projectile.Center, laserDirection, ModContent.ProjectileType<HolyMagicLaserbeam>(), ProvidenceBehaviorOverride.MagicLaserbeamDamage, 0f, -1, angularVelocity);
                            }
                        }
                    }
                    int minimumTime = InitialLaserTelegraphTime + InitialLaserShootTime;

                    if (Timer >= minimumTime && Timer <= minimumTime + GrowTime)
                        projectile.scale = MathHelper.Clamp(projectile.scale -= 1f / GrowTime, 0f, 1f);
                    else if (projectile.scale == 0 || Timer >= minimumTime + GrowTime + 2)
                    {
                        projectile.Kill();
                        return;
                    }
                    break;
            }

            Timer++;
        }

        // This is manually updated when needed.
        public override bool ShouldUpdatePosition()
        {
            return CurrentState == StateType.Flinging ||
                   CurrentState == StateType.SlowdownMovement ||
                   CurrentState == StateType.FinalFiring;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => Utils.CenteredRectangle(projectile.Center, new Vector2(300f) * projectile.scale).Intersects(targetHitbox);

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Texture2D invis = InfernumTextureRegistry.Invisible;
            Texture2D noise = InfernumTextureRegistry.HarshNoise;
            Effect fireball = InfernumEffectsRegistry.FireballShader.GetShader().Shader;

            fireball.Parameters["sampleTexture2"].SetValue(noise);
            fireball.Parameters["mainColor"].SetValue(Color.Lerp(WayfinderSymbol.Colors[1], WayfinderSymbol.Colors[2], 0.3f).ToVector3());
            fireball.Parameters["resolution"].SetValue(new Vector2(250f, 250f));
            fireball.Parameters["speed"].SetValue(0.76f);
            fireball.Parameters["time"].SetValue(Main.GlobalTime);
            fireball.Parameters["zoom"].SetValue(0.0004f);
            fireball.Parameters["dist"].SetValue(60f);
            fireball.Parameters["opacity"].SetValue(projectile.Opacity);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.instance.Rasterizer, fireball, Main.GameViewMatrix.TransformationMatrix);
            Main.spriteBatch.Draw(invis, projectile.Center - Main.screenPosition, null, Color.White, projectile.rotation, invis.Size() * 0.5f, 400f * projectile.scale, SpriteEffects.None, 0f);
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.instance.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            return false;
        }
    }
}
