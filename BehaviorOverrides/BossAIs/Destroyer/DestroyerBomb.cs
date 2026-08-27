using CalamityMod;
using CalamityMod.Particles;
using InfernumMode.Sounds;
using InfernumMode.Graphics;
using InfernumMode.Graphics.Interfaces;
using InfernumMode.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using CalamityMod.Particles;
using System;
using CalamityMod.Projectiles;

namespace InfernumMode.BehaviorOverrides.BossAIs.Destroyer
{
    public class DestroyerBomb : ModProjectile, ISpecializedDrawRegion
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Explosion");
            Main.projFrames[projectile.type] = 3;
        }

        public override void SetDefaults()
        {
            projectile.width = projectile.height = 20;
            projectile.hostile = true;
            projectile.ignoreWater = true;
            projectile.tileCollide = true;
            projectile.penetrate = -1;
            projectile.timeLeft = 660;
            projectile.Calamity().canBreakPlayerDefense = true;
			cooldownSlot = 1;
        }

        public override void AI()
        {
            projectile.frameCounter++;

            // Flick with red right before death as a telegraph.
            if (projectile.frameCounter % 4 == 3)
                projectile.frame = projectile.frame == 0 ? (projectile.timeLeft < 60 ? 2 : 1) : 0;

            if (projectile.velocity.Y < 20f)
                projectile.velocity.Y += 0.25f;

            // Rotate.
            projectile.rotation = projectile.velocity.ToRotation() - MathHelper.PiOver2;

            // Collide with tiles after enough time has passed.
            projectile.tileCollide = projectile.timeLeft < 420;

            Tile tileAtPosition = CalamityUtils.ParanoidTileRetrieval((int)projectile.Center.X / 16, (int)projectile.Center.Y / 16);
            Player closestPlayer = Main.player[Player.FindClosest(projectile.Center, 1, 1)];
            if ((TileID.Sets.Platforms[tileAtPosition.type] && tileAtPosition.active() && projectile.tileCollide) || (projectile.WithinRange(closestPlayer.Center, 60f) && projectile.timeLeft < 580))
                projectile.Kill();

            Lighting.AddLight(projectile.Center, Vector3.One * 0.85f);
        }

        // Explode on death.
        public override void Kill(int timeLeft)
        {
            CalamityGlobalProjectile.ExpandHitboxBy(projectile, 84);
            projectile.damage = 50;
            projectile.Damage();

            // Create particles and sounds at the explosion point.
            for (int i = 0; i < 5; i++)
            {
                Color fireColor = Main.rand.NextBool() ? Color.Yellow : Color.Red;
                CloudParticle fireCloud = new CloudParticle(projectile.Center, Main.rand.NextVector2Circular(4f, 4f), fireColor, Color.DarkGray, 30, Main.rand.NextFloat(1.67f, 1.85f));
                GeneralParticleHandler.SpawnParticle(fireCloud);
            }
            Utils.PoofOfSmoke(projectile.Center);
            Main.PlaySound(InfernumSoundRegistry.DestroyerBombExplodeSound, projectile.Center);
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            projectile.DrawProjectileWithBackglowTemp(Color.Red * 0.4f, Color.White, projectile.Opacity * 4f);
            return false;
        }

        public void SpecialDraw(SpriteBatch spriteBatch)
        {
            // Draw the bloom laser line telegraph.
            float laserRotation = -projectile.velocity.ToRotation();
            float telegraphInterpolant = Utils.InverseLerp(660f, 630f, projectile.timeLeft, true);

            BloomLineDrawInfo lineInfo = new BloomLineDrawInfo()
            {
                LineRotation = laserRotation,
                WidthFactor = 0.0035f + (float)Math.Pow(telegraphInterpolant, 4f) * ((float)Math.Sin(Main.GlobalTime * 3f) * 0.001f + 0.001f),
                BloomIntensity = MathHelper.Lerp(0.3f, 0.4f, telegraphInterpolant),
                Scale = Vector2.One * telegraphInterpolant * 200f,
                MainColor = Color.Lerp(Color.Orange, Color.Red, telegraphInterpolant * 0.6f + 0.4f),
                DarkerColor = Color.Orange,
                Opacity = (float)Math.Sqrt(telegraphInterpolant),
                BloomOpacity = 0.375f,
                LightStrength = 5f
            };
            Utilities.DrawBloomLineTelegraph(projectile.Center - Main.screenPosition, lineInfo, false);
        }

        public void PrepareSpriteBatch(SpriteBatch spriteBatch)
        {
            spriteBatch.EnforceCutoffRegion(new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Main.GameViewMatrix.TransformationMatrix, SpriteSortMode.Immediate, BlendState.Additive);
        }
    }
}
