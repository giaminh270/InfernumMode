using CalamityMod;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Events;
using CalamityMod.Projectiles.BaseProjectiles;
using CalamityMod.World;
using InfernumMode.Effects;
using InfernumMode.ExtraTextures;
using InfernumMode.BaseEntities;
using InfernumMode.Graphics.Primitives;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using BaseLaserbeamProjectile = InfernumMode.BaseEntities.BaseLaserbeamProjectile;
using System;

namespace InfernumMode.BehaviorOverrides.BossAIs.BrimstoneElemental
{
    public class BrimstoneDeathray : BaseLaserbeamProjectile
    {
        public PrimitiveTrailCopy LaserDrawer
        {
            get;
            set;
        }

        public int OwnerIndex => (int)projectile.ai[1];
        public override float Lifetime => 85;
        public override Color LaserOverlayColor => Color.White;
        public override Color LightCastColor => Color.Red;
        public override Texture2D LaserBeginTexture => Main.projectileTexture[projectile.type];
        public override Texture2D LaserMiddleTexture => ModContent.GetTexture("CalamityMod/ExtraTextures/Lasers/BrimstoneRayMid");
        public override Texture2D LaserEndTexture => ModContent.GetTexture("CalamityMod/ExtraTextures/Lasers/BrimstoneRayEnd");
        public override float MaxLaserLength => 3100f;
        public override float MaxScale => 1f;
        public Vector2 OwnerEyePosition => Main.npc[OwnerIndex].Center + new Vector2(Main.npc[OwnerIndex].spriteDirection * 20f, -68f);
        public override void SetStaticDefaults() => DisplayName.SetDefault("Brimstone Deathray");

        public override void SetDefaults()
        {
            projectile.width = projectile.height = 20;
            projectile.hostile = true;
            projectile.alpha = 255;
            projectile.penetrate = -1;
            projectile.tileCollide = false;
            projectile.timeLeft = (int)Lifetime;
            projectile.Calamity().canBreakPlayerDefense = true;
            cooldownSlot = 1;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(projectile.localAI[0]);
            writer.Write(projectile.localAI[1]);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            projectile.localAI[0] = reader.ReadSingle();
            projectile.localAI[1] = reader.ReadSingle();
        }
        public override void AttachToSomething()
        {
            if (!Main.projectile.IndexInRange(OwnerIndex))
            {
                projectile.Kill();
                return;
            }
            projectile.Center = OwnerEyePosition;

            if (projectile.timeLeft == 10)
            {
                Main.PlaySound(InfernumMode.CalamityMod.GetLegacySoundSlot(SoundType.Custom, "Sounds/Custom/SCalSounds/BrimstoneShoot"), projectile.Center);
                if (Main.netMode == NetmodeID.MultiplayerClient)
                    return;

                for (float petalOffset = 20f; petalOffset < LaserLength; petalOffset += 165f)
                {
                    Vector2 petalSpawnPosition = OwnerEyePosition + projectile.velocity * petalOffset;
                    for (int i = -1; i <= 1; i++)
                    {
                    	Vector2 petalVelocity = projectile.velocity.RotatedBy(MathHelper.PiOver2 * i) * 8f;
                        if (BossRushEvent.BossRushActive)
                            petalVelocity *= 1.85f;
                        Utilities.NewProjectileBetter(petalSpawnPosition, petalVelocity, ModContent.ProjectileType<BrimstonePetal2>(), BrimstoneElementalBehaviorOverride.BrimstonePetalDamage, 0f);
                    }
                }
            }
        }

        public float LaserWidthFunction(float completionRatio) => projectile.scale * projectile.width * Utils.InverseLerp(0.02f, 0.05f, completionRatio, true) * 3f;

        public static Color LaserColorFunction(float completionRatio)
        {
            float colorInterpolant = (float)Math.Sin(Main.GlobalTime * -5.2f + completionRatio * 23f) * 0.5f + 0.5f;
            return Color.Lerp(Color.Red, new Color(255, 0, 25), colorInterpolant) * Utils.InverseLerp(0.02f, 0.05f, completionRatio, true);
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            // This should never happen, but just in case.
            if (projectile.velocity == Vector2.Zero)
                return false;

            if (LaserDrawer == null)
    			LaserDrawer = new PrimitiveTrailCopy(LaserWidthFunction, LaserColorFunction, null, true, InfernumEffectsRegistry.GenericLaserVertexShader);
            Vector2 laserEnd = projectile.Center + projectile.velocity.SafeNormalize(Vector2.UnitY) * LaserLength;
            Vector2[] baseDrawPoints = new Vector2[8];
            for (int i = 0; i < baseDrawPoints.Length; i++)
                baseDrawPoints[i] = Vector2.Lerp(projectile.Center, laserEnd, i / (float)(baseDrawPoints.Length - 1f));

            // Select textures to pass to the shader, along with the electricity color.
            Color middleColor = new Color(252, 220, 178);
            Color middleColor2 = new Color(255, 162, 162);
            InfernumEffectsRegistry.GenericLaserVertexShader.UseColor(middleColor2 * 2f);
            InfernumEffectsRegistry.GenericLaserVertexShader.SetShaderTexture(InfernumTextureRegistry.StreakFire);

            LaserDrawer.Draw(baseDrawPoints, -Main.screenPosition, 60);
            return false;
        }

        public override bool CanDamage() => Time >= 10f;
    }
}
