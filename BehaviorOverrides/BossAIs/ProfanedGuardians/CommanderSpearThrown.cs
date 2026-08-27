using InfernumMode;
using CalamityMod;
using InfernumMode.Sounds;
using InfernumMode.BehaviorOverrides.BossAIs.Providence;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using InfernumMode.Projectiles;
using CalamityMod.Particles;
using InfernumMode.Particles;
using System;

namespace InfernumMode.BehaviorOverrides.BossAIs.ProfanedGuardians
{
    public class CommanderSpearThrown : ModProjectile
    {
        public const int TelegraphTime = 30;

        public const int PassThroughTilesTime = 15;

        public ref float Timer => ref projectile.ai[0];

        public bool ExplodeOnImpact => projectile.ai[1] == 1;

        public override string Texture => "InfernumMode/BehaviorOverrides/BossAIs/ProfanedGuardians/CommanderSpear";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Profaned Spear");
            ProjectileID.Sets.TrailCacheLength[projectile.type] = 4;
            ProjectileID.Sets.TrailingMode[projectile.type] = 0;
        }

        public override void SetDefaults()
        {
            projectile.width = 32;
            projectile.height = 32;
            projectile.hostile = true;
            projectile.tileCollide = false;
            projectile.penetrate = -1;
            projectile.alpha = 255;
            projectile.timeLeft = 180;
            projectile.Calamity().canBreakPlayerDefense = true;
            cooldownSlot = 1;
        }

        public override void AI()
        {
            // Don't do anything if the telegraphs are being drawn.
            if (Timer < TelegraphTime)
            {
                Timer++;
                return;
            }

            if (ExplodeOnImpact)
                projectile.tileCollide = Timer >= TelegraphTime + PassThroughTilesTime;

            projectile.rotation = projectile.velocity.ToRotation() + MathHelper.PiOver4;
            projectile.Opacity = MathHelper.Clamp(projectile.Opacity + 0.08f, 0f, 1f);

            // Accelerate.
            if (projectile.velocity.Length() < 36f)
                projectile.velocity *= 1.05f;

            for (int i = 0; i < (InfernumConfig.Instance.ReducedGraphicsConfig ? 12 : 40); i++)
            {
                // Bias towards lower values. 
                float size = (float)Math.Pow(Main.rand.NextFloat(), 2f);
                InfernumFusableParticleManager.GetParticleSetByType<ProfanedLavaParticleSet>()?.SpawnParticle(projectile.Center - (projectile.velocity * 0.5f) + (Main.rand.NextVector2Circular(projectile.width * 0.5f, projectile.height * 0.5f) * size),
                    Main.rand.NextFloat(10f, 15f));
            }

            Lighting.AddLight(projectile.Center, Vector3.One);
            Timer++;
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            projectile.Kill();
            return false;
        }

        public override bool ShouldUpdatePosition() => Timer >= TelegraphTime;

        public override void Kill(int timeLeft)
        {
            if (!ExplodeOnImpact)
                return;
            ScreenEffectSystem.SetBlurEffect(projectile.Center, 1f, 45);
            Main.PlaySound(SoundID.DD2_LightningBugZap, projectile.Center);
            Main.PlaySound(InfernumSoundRegistry.MyrindaelHitSound, projectile.Center);
            Main.PlaySound(SoundID.DD2_ExplosiveTrapExplode, projectile.Center);

            GuardianComboAttackManager.CreateFireExplosion(projectile.Center, true);
            for (int i = 0; i < 100; i++)
                InfernumFusableParticleManager.GetParticleSetByType<ProfanedLavaParticleSet>()?.SpawnParticle(projectile.Center + Main.rand.NextVector2Circular(100f, 100f), Main.rand.NextFloat(52f, 85f));

            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            int crossWaves = 2;
            int crossCount = 18;
            for (int i = 0; i < crossWaves; i++)
            {
                float speed;
			    switch (i)
			    {
			        case 0:
			            speed = 8.5f;
			            break;
			        case 1:
			            speed = 5f;
			            break;
			        default:
			            speed = 2.5f;
			            break;
			    }

                for (int j = 0; j < crossCount; j++)
                {
                    Vector2 crossVelocity = (MathHelper.TwoPi * j / crossCount + MathHelper.PiOver4 * i).ToRotationVector2() * speed;
                    Utilities.NewProjectileBetter(projectile.Center + crossVelocity, crossVelocity, ModContent.ProjectileType<HolyCross>(), GuardianComboAttackManager.HolyCrossDamage, 0f);
                }
            }
        }

        public override Color? GetAlpha(Color lightColor) => Color.White * projectile.Opacity;

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            if (Timer <= TelegraphTime)
            {
                float opacity = CalamityUtils.Convert01To010(Timer / TelegraphTime);
                BloomLineDrawInfo lineInfo = new BloomLineDrawInfo()
                {
                    LineRotation = -projectile.velocity.ToRotation(),
                    WidthFactor = 0.003f + (float)Math.Pow(opacity, 5f) * ((float)Math.Sin(Main.GlobalTime * 3f) * 0.001f + 0.001f),
                    BloomIntensity = MathHelper.Lerp(0.06f, 0.16f, opacity),
                    Scale = Vector2.One * 1950f,
                    MainColor = WayfinderSymbol.Colors[1],
                    DarkerColor = WayfinderSymbol.Colors[2],
                    Opacity = opacity,
                    BloomOpacity = 0.4f,
                    LightStrength = 5f
                };
                Utilities.DrawBloomLineTelegraph(projectile.Center - Main.screenPosition, lineInfo);
                return false;
            }
            if (ExplodeOnImpact)
            {
                Texture2D texture = ModContent.GetTexture(Texture);

                // Draw the spear as a white hot flame with additive blending before it converge inward to create the actual spear.
                for (int i = 0; i < 5; i++)
                {
                    Vector2 drawOffset = (MathHelper.TwoPi * i / 5f).ToRotationVector2() * 5f;
                    Vector2 drawPosition = projectile.Center - Main.screenPosition + drawOffset;
                    Main.spriteBatch.Draw(texture, drawPosition, null, new Color(Color.LightPink.R, Color.LightPink.G, Color.LightPink.B, 0), projectile.rotation, texture.Size() * 0.5f, projectile.scale, 0, 0);
                }
                Main.spriteBatch.Draw(texture, projectile.Center - Main.screenPosition, null, new Color(Color.White.R, Color.White.G, Color.White.B, 150), projectile.rotation, texture.Size() * 0.5f, projectile.scale, 0, 0);
                return false;
            }
            float alpha = 1f - (float)projectile.alpha / 255;
            CalamityUtils.DrawAfterimagesCentered(projectile, ProjectileID.Sets.TrailingMode[projectile.type], lightColor * alpha, 1);
            projectile.DrawProjectileWithBackglowTemp(new Color(Color.White.R, Color.White.G, Color.White.B, 0), Color.White, 2f);
            return false;
        }
    }
}
