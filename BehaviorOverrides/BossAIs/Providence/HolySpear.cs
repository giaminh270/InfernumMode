using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using static System.Math;
using static Microsoft.Xna.Framework.MathHelper;
using InfernumMode.Particles;
using InfernumMode.Sounds;

namespace InfernumMode.BehaviorOverrides.BossAIs.Providence
{
    public class HolySpear : ModProjectile
    {
        public Vector2 CurrentDirectionEdge
        {
            get
            {
                if (InLava)
                    return Vector2.UnitY;

                float bestOrthogonality = -100000f;
                Vector2 aimDirection = (projectile.rotation - MathHelper.PiOver4).ToRotationVector2();
                Vector2 edge = Vector2.Zero;
                Vector2[] edges = new Vector2[]
                {
                    Vector2.UnitX,
                    -Vector2.UnitX,
                    Vector2.UnitY,
                    -Vector2.UnitY
                };

                // Determine which edge the current direction aligns with most based on dot products.
                for (int i = 0; i < edges.Length; i++)
                {
                    float orthogonality = Vector2.Dot(aimDirection, edges[i]);
                    if (orthogonality > bestOrthogonality)
                    {
                        edge = edges[i];
                        bestOrthogonality = orthogonality;
                    }
                }

                return edge;
            }
        }

        public bool SpawnedInBlocks
        {
            get;
            set;
        }

        public bool BeenInBlockSinceStart
        {
            get;
            set;
        }

        public bool InLava
        {
            get
            {
                IEnumerable<Projectile> lavaProjectiles = Utilities.AllProjectilesByID(ModContent.ProjectileType<ProfanedLava>());
                if (!lavaProjectiles.Any())
                    return false;

                Rectangle tipHitbox = Utils.CenteredRectangle(projectile.Center + (projectile.rotation - MathHelper.PiOver4).ToRotationVector2() * 60f, Vector2.One);
                return lavaProjectiles.Any(l => l.Colliding(l.Hitbox, tipHitbox));
            }
        }

        public ref float Time => ref projectile.ai[0];

        public ref float DeathCountdown => ref projectile.ai[1];

        public static int DeathDelay => 90;

        public override string Texture => "InfernumMode/BehaviorOverrides/BossAIs/Providence/CommanderSpear2";

        public override void SetStaticDefaults() => DisplayName.SetDefault("Holy Spear");

        public override void SetDefaults()
        {
            projectile.width = projectile.height = 124;
            projectile.hostile = true;
            projectile.tileCollide = true;
            projectile.ignoreWater = true;
            projectile.penetrate = -1;
            projectile.timeLeft = 360;
            cooldownSlot = 1;
        }

        public override void AI()
        {
            bool tileCollision = Collision.SolidCollision(projectile.Top, projectile.width, projectile.height);
            if (tileCollision && Time <= 1f)
            {
                SpawnedInBlocks = true;
                BeenInBlockSinceStart = false;
            }
            if (SpawnedInBlocks && !tileCollision && Time >= 35f)
                BeenInBlockSinceStart = false;

            // Decide the rotation of the spear based on velocity, if there is any.
            if (projectile.velocity != Vector2.Zero)
                projectile.rotation = projectile.velocity.ToRotation() + MathHelper.PiOver4;

            // Handle death effects.
            if (DeathCountdown >= 1f)
            {
                // Prevent a natural death disrupting the fire wall directions by locking the timeLeft variable in place.
                projectile.timeLeft = 60;

                // Release fire pillars.
                if (DeathCountdown % 5f == 3f)
                {
                    float perpendicularOffset = Utilities.Remap(DeathCountdown, DeathDelay, 0f, 0f, 3600f);
                    Vector2 pillarDirection = -(projectile.rotation - MathHelper.PiOver4).ToRotationVector2();
                    if (InLava)
                        pillarDirection = -Vector2.UnitY;

                    // Make the gaps a bit wider if the pillars will spawn at around a 45-degree inclination, since it's a bit too tight without this.
                    float evenAngle = pillarDirection.ToRotation();
                    if (evenAngle < 0f)
                        evenAngle += MathHelper.TwoPi;
                    bool closeTo45DegreeGap = MathHelper.Distance(evenAngle % MathHelper.PiOver2, MathHelper.PiOver4) < MathHelper.ToRadians(20f);
                    if (closeTo45DegreeGap)
                        perpendicularOffset *= 1.6f;

                    Main.PlaySound(SoundID.Item73, projectile.Center);
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        Vector2 pillarSpawnPosition = projectile.Center + CurrentDirectionEdge.RotatedBy(MathHelper.PiOver2) * perpendicularOffset - pillarDirection * 800f;
                        Utilities.NewProjectileBetter(pillarSpawnPosition, pillarDirection, ModContent.ProjectileType<HolySpearFirePillar>(), 400, 0f);

                        pillarSpawnPosition = projectile.Center - CurrentDirectionEdge.RotatedBy(MathHelper.PiOver2) * perpendicularOffset - pillarDirection * 800f;
                        Utilities.NewProjectileBetter(pillarSpawnPosition, pillarDirection, ModContent.ProjectileType<HolySpearFirePillar>(), 400, 0f);
                    }
                }

                projectile.velocity *= 0.93f;
                DeathCountdown--;
                if (DeathCountdown <= 0f)
                    projectile.Kill();
            }

            // Stick to lava.
            else if (InLava)
                PrepareForDeath(projectile.velocity);

            // Wait a little bit before interacting with tiles.
            int collideDelay = SpawnedInBlocks ? 65 : 24;
            projectile.tileCollide = Time >= collideDelay && !BeenInBlockSinceStart;
            Time++;
        }

        public void PrepareForDeath(Vector2 oldVelocity)
        {
            if (DeathCountdown > 0f)
                return;

            if (Main.netMode != NetmodeID.Server)
                Utilities.NewProjectileBetter(projectile.Center + oldVelocity.SafeNormalize(Vector2.UnitY) * 60f, oldVelocity, ModContent.ProjectileType<StrongProfanedCrack>(), 0, 0f);

            Main.LocalPlayer.Infernum().CurrentScreenShakePower = 9f;
            ScreenEffectSystem.SetBlurEffect(projectile.Center, 0.2f, 18);

            Main.PlaySound(InfernumSoundRegistry.ProvidenceSpearHitSound, projectile.Center);
            projectile.velocity *= InLava ? 0.6f : 0f;
            projectile.Center += oldVelocity.SafeNormalize(Vector2.Zero) * 50f;
            DeathCountdown = DeathDelay;
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            PrepareForDeath(oldVelocity);
            return false;
        }

        public override void Kill(int timeLeft)
        {
            // Burst into lava metaballs on death.
            if (Main.netMode != NetmodeID.Server)
                ModContent.GetTexture(Texture).CreateMetaballsFromTexture(ref InfernumFusableParticleManager.GetParticleSetByType<ProfanedLavaParticleSet>().Particles, projectile.Center, projectile.rotation, projectile.scale, 20f, 30);
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            float burnInterpolant = Utils.InverseLerp(45f, 0f, Time, true);
            float drawOffsetRadius = burnInterpolant * 16f;
            Color color = projectile.GetAlpha(Color.Lerp(Color.White, Color.Yellow * 0.6f, burnInterpolant));
            Texture2D texture = ModContent.GetTexture(Texture);
            if (ProvidenceBehaviorOverride.IsEnraged)
                texture = ModContent.GetTexture("InfernumMode/BehaviorOverrides/BossAIs/Providence/CommanderSpear2Night");

            // Draw the spear as a white hot flame with additive blending before it converge inward to create the actual spear.
            for (int i = 0; i < 10; i++)
            {
                float rotation = projectile.rotation + MathHelper.Lerp(-0.16f, 0.16f, i / 9f) * burnInterpolant;
                Vector2 drawOffset = (MathHelper.TwoPi * i / 10f).ToRotationVector2() * drawOffsetRadius;
                Vector2 drawPosition = projectile.Center - Main.screenPosition + drawOffset;
                Main.spriteBatch.Draw(texture, drawPosition, null, color, rotation, texture.Size() * 0.5f, projectile.scale, 0, 0);
            }
            return false;
        }
    }
}
