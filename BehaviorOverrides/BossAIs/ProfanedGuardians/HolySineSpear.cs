using System;
using CalamityMod;
using CalamityMod.NPCs;
using InfernumMode.ExtraTextures;
using InfernumMode.Graphics.Interfaces;
using InfernumMode.Graphics.Primitives;
using InfernumMode.Projectiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using static System.Math;
using static Microsoft.Xna.Framework.MathHelper;

namespace InfernumMode.BehaviorOverrides.BossAIs.ProfanedGuardians
{
    public class HolySineSpear : ModProjectile, IPixelPrimitiveDrawer
    {
		public bool DrawBeforeNPCs => false;
		
        internal PrimitiveTrailCopy TrailDrawer;

        public float StartingRotation
        {
            get;
            set;
        }

        public Vector2 InitialCenter
        {
            get;
            set;
        } = Vector2.Zero;

        public Vector2 InitialVelocity
        {
            get;
            set;
        } = Vector2.Zero;

        public static NPC Commander
        {
            get
            {
                if (Main.npc.IndexInRange(CalamityGlobalNPC.doughnutBoss))
                    return Main.npc[CalamityGlobalNPC.doughnutBoss];
                return null;
            }
        }

        public float WaveOffset;

        public float TelegraphLength => 20f;

        public float FlyStraightLength => 15f;

        public float SineOffset => (float)Math.Sin((Timer - WaveOffset) / 9f - WaveOffset);

        public ref float Timer => ref projectile.ai[0];

        public ref float SpearDirection => ref projectile.ai[1];

        public float SpearReleaseRate = 3f;

        public override string Texture => "InfernumMode/BehaviorOverrides/BossAIs/ProfanedGuardians/ProfanedSpearInfernum";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Profaned Spear");
            ProjectileID.Sets.TrailCacheLength[projectile.type] = 25;
            ProjectileID.Sets.TrailingMode[projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            projectile.width = 32;
            projectile.height = 32;
            projectile.hostile = true;
            projectile.tileCollide = false;
            projectile.penetrate = -1;
            projectile.alpha = 255;
            projectile.timeLeft = 200;
            projectile.Calamity().canBreakPlayerDefense = true;
            cooldownSlot = 1;
        }

        public override void AI()
        {
            if (Commander is null)
            {
                projectile.Kill();
                return;
            }

            projectile.Opacity = MathHelper.Clamp(projectile.Opacity + 0.08f, 0f, 1f);
            projectile.rotation = projectile.velocity.ToRotation() + MathHelper.PiOver4;

            //Wave up and down over time.
            if (Timer > FlyStraightLength)
            {
                Vector2 moveOffset = (StartingRotation + MathHelper.PiOver2).ToRotationVector2() * SineOffset * 8f;
                projectile.Center += moveOffset;
                projectile.rotation = projectile.velocity.ToRotation() + MathHelper.PiOver4 + (SineOffset * 0.5f * projectile.direction);
                projectile.velocity *= 1.01f;
            }
            //Release spears.
            if (Timer % SpearReleaseRate == SpearReleaseRate - 1 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                Vector2 velocity = projectile.velocity.SafeNormalize(Vector2.UnitY).RotatedBy(MathHelper.PiOver2 * SpearDirection) * 7f;
                SpearDirection *= -1f;
                Utilities.NewProjectileBetter(projectile.Center + velocity.SafeNormalize(Vector2.UnitY), velocity, ModContent.ProjectileType<ProfanedSpearInfernum>(), GuardianComboAttackManager.HolySpearDamage, 0f);
            }

            Lighting.AddLight(projectile.Center, Vector3.One);
            Timer++;
        }

        public override Color? GetAlpha(Color lightColor) => Color.White * projectile.Opacity;

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            projectile.DrawProjectileWithBackglowTemp(Color.White, Color.White, 2f);
            return false;
        }

        internal float TrailWidthFunction(float completionRatio) => projectile.scale * 20f;

        internal Color TrailColorFunction(float completionRatio) => Color.Lerp(WayfinderSymbol.Colors[1], Color.Transparent, completionRatio + 0.2f);

        public void DrawPixelPrimitives(SpriteBatch spriteBatch)
        {
            if (TrailDrawer == null)
                TrailDrawer = new PrimitiveTrailCopy(TrailWidthFunction, TrailColorFunction, null, true, GameShaders.Misc["CalamityMod:ImpFlameTrail"]);

            GameShaders.Misc["CalamityMod:ImpFlameTrail"].SetShaderTexture(InfernumTextureRegistry.HoneycombNoise);

            TrailDrawer.DrawPixelated(projectile.oldPos, projectile.Size * 0.5f - Main.screenPosition, 25);
        }
    }
}
