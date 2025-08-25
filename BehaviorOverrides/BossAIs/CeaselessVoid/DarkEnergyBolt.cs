using CalamityMod.Events;
using CalamityMod.Dusts;
using CalamityMod.NPCs;
using InfernumMode.BehaviorOverrides.BossAIs.MoonLord;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;
using Terraria;
using CalamityMod;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.CeaselessVoid
{
    public class DarkEnergyBolt : ModProjectile
    {
        public PrimitiveTrailCopy TrailDrawer = null;
        public ref float Time => ref projectile.ai[1];

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Dark Energy Bolt");
            Main.projFrames[projectile.type] = 2;
            ProjectileID.Sets.TrailingMode[projectile.type] = 0;
            ProjectileID.Sets.TrailCacheLength[projectile.type] = 9;
        }

        public override void SetDefaults()
        {
            projectile.width = projectile.height = 50;
            projectile.hostile = true;
            projectile.ignoreWater = true;
            projectile.tileCollide = false;
            projectile.penetrate = -1;
            projectile.timeLeft = 360;
            cooldownSlot = 1;
        }

        public override void AI()
        {
            ProjectileID.Sets.TrailCacheLength[projectile.type] = 16;

            // Thin out and disappear once collision has happened.
            if (!Main.npc.IndexInRange(CalamityGlobalNPC.voidBoss))
            {
                projectile.Kill();
                return;
            }
            NPC ceaselessVoid = Main.npc[CalamityGlobalNPC.voidBoss];
            float distanceToVoid = projectile.Distance(ceaselessVoid.Center);
            projectile.scale = Utils.InverseLerp(0f, 240f, distanceToVoid, true);
            projectile.rotation += (projectile.velocity.X > 0f).ToDirectionInt() * 0.007f;
            if (distanceToVoid < 360f)
                projectile.velocity = (projectile.velocity * 29f + projectile.SafeDirectionTo(ceaselessVoid.Center) * 14.5f) / 30f;
            
            if (distanceToVoid < Main.rand.NextFloat(64f, 90f))
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                    Utilities.NewProjectileBetter(projectile.Center, Vector2.Zero, ModContent.ProjectileType<MoonLordExplosion>(), 0, 0f);

                projectile.Kill();
            }



            Time++;
        }

        internal float WidthFunction(float completionRatio)
        {
            float arrowheadCutoff = 0.33f;
            float width = projectile.width;
            if (completionRatio <= arrowheadCutoff)
                width = MathHelper.Lerp(0.02f, width, Utils.InverseLerp(0f, arrowheadCutoff, completionRatio, true));
            return width * projectile.scale + 1f;
        }

        internal Color ColorFunction(float completionRatio)
        {
            Color shaderColor1 = Color.Lerp(Color.Black, Color.Purple, 0.35f);
            Color shaderColor2 = Color.Lerp(Color.Black, Color.Cyan, 0.7f);

            float endFadeRatio = 0.9f;

            float endFadeTerm = Utils.InverseLerp(0f, endFadeRatio * 0.5f, completionRatio, true) * 3.2f;
            float sinusoidalTime = completionRatio * 2.7f - Main.GlobalTime * 2.3f + endFadeTerm;
            float startingInterpolant = (float)Math.Cos(sinusoidalTime) * 0.5f + 0.5f;

            float colorLerpFactor = 0.6f;
            Color startingColor = Color.Lerp(shaderColor1, shaderColor2, startingInterpolant * colorLerpFactor) * projectile.Opacity;
            return Color.Lerp(startingColor, Color.Transparent, MathHelper.SmoothStep(0f, 1f, Utils.InverseLerp(0f, endFadeRatio, completionRatio, true)));
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            if (TrailDrawer is null)
                TrailDrawer = new PrimitiveTrailCopy(WidthFunction, ColorFunction, null, true, GameShaders.Misc["Infernum:TwinsFlameTrail"]);

            GameShaders.Misc["Infernum:TwinsFlameTrail"].UseImage("Images/Misc/Perlin");
			TrailDrawer.Draw(projectile.oldPos, projectile.Size * 0.5f - Main.screenPosition, 39);
            return false;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            return CalamityUtils.CircularHitboxCollision(projectile.Center, projectile.scale * 17f, targetHitbox);
        }
        public override void Kill(int timeLeft)
        {
            Main.PlaySound(SoundID.Item20, projectile.Center);
            for (int dust = 0; dust < 4; dust++)
                Dust.NewDust(projectile.position + projectile.velocity, projectile.width, projectile.height, (int)CalamityDusts.BlueCosmilite, 0f, 0f);
        }

        public override void ModifyHitPlayer(Player target, ref int damage, ref bool crit)
        {
            
        }

    }
}
