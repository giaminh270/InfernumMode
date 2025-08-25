using CalamityMod;
using CalamityMod.NPCs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.CeaselessVoid
{
    public class RealityTear : ModProjectile
    {
        internal PrimitiveTrailCopy LightningDrawer;

        public List<Vector2> TrailCache = new List<Vector2>();

        public float ScaleFactorDelta => projectile.localAI[0];

        public ref float CurrentVerticalOffset => ref projectile.ai[0];

        public ref float Time => ref projectile.ai[1];

        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public override void SetStaticDefaults() => DisplayName.SetDefault("Reality Tear");

        public override void SetDefaults()
        {
            projectile.width = 14;
            projectile.height = 14;
            projectile.alpha = 255;
            projectile.penetrate = -1;
            projectile.ignoreWater = true;
            projectile.tileCollide = false;
            projectile.timeLeft = projectile.MaxUpdates * 135;
        }

        public override void AI()
        {
            // Disappear if the Ceaseless Void is not present.
            if (!Main.npc.IndexInRange(CalamityGlobalNPC.voidBoss))
            {
                projectile.Kill();
                return;
            }

            NPC ceaselessVoid = Main.npc[CalamityGlobalNPC.voidBoss];
            bool stickToVoid = ceaselessVoid.Infernum().ExtraAI[0] == projectile.whoAmI;

            if (stickToVoid)
            {
                TrailCache.Add(projectile.Center);
                projectile.Center = ceaselessVoid.Center + Vector2.UnitY * CurrentVerticalOffset + ceaselessVoid.velocity;
                if (Main.rand.NextBool(4))
                {
                    float newIdealOffset = Main.rand.NextBool().ToDirectionInt() * Main.rand.NextFloat(24f, 76f);
                    CurrentVerticalOffset = MathHelper.Lerp(CurrentVerticalOffset, newIdealOffset, 0.667f);

                    projectile.netUpdate = true;
                }
            }

            // Create barrages of otherwordly magic from the tear.
            else if (Main.netMode != NetmodeID.MultiplayerClient && TrailCache.Count >= 2 && Time % 8f == 7f)
            {
                int barragePointIndex = Main.rand.Next(TrailCache.Count - 1);
                Vector2 barrageVelocity = Main.rand.NextVector2CircularEdge(8f, 8f);
                Vector2 barrageSpawnPosition = Vector2.Lerp(TrailCache[barragePointIndex], TrailCache[barragePointIndex + 1], Main.rand.NextFloat());
                int barrage = Utilities.NewProjectileBetter(barrageSpawnPosition, barrageVelocity, ModContent.ProjectileType<CelestialBarrage>(), 250, 0f);
                if (Main.projectile.IndexInRange(barrage))
                    Main.projectile[barrage].ai[1] = ScaleFactorDelta;
            }
            
            // Fade in.
            float disappearInterpolant = Utils.InverseLerp(0f, 24f, projectile.timeLeft / projectile.MaxUpdates, true);
            float scaleGrowInterpolant = (float)Math.Pow(Utils.InverseLerp(0f, 64f, Time, true), 1.72);
            projectile.Opacity = Utils.InverseLerp(0f, 24f, Time / projectile.MaxUpdates, true) * disappearInterpolant;
            projectile.scale = MathHelper.Lerp(0.24f, 1f, scaleGrowInterpolant) * disappearInterpolant;
            Time++;
        }

        #region Drawing
        internal float WidthFunction(float completionRatio)
        {
            float baseWidth = MathHelper.Lerp(72f, 73f, (float)Math.Sin(MathHelper.Pi * 4f * completionRatio) * 0.5f + 0.5f) * projectile.scale;
            return CalamityUtils.Convert01To010(completionRatio) * baseWidth * (1f + ScaleFactorDelta);
        }

        internal Color ColorFunction(float completionRatio)
        {
            float opacity = CalamityUtils.Convert01To010(completionRatio) * 1.4f;
            if (opacity >= 1f)
                opacity = 1f;
            opacity *= projectile.Opacity;
            return Color.White * opacity;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            if (LightningDrawer is null)
                LightningDrawer = new PrimitiveTrailCopy(WidthFunction, ColorFunction, null, true, GameShaders.Misc["Infernum:RealityTear"]);

            GameShaders.Misc["Infernum:RealityTear"].SetShaderTexture(ModContent.GetTexture("InfernumMode/ExtraTextures/Stars"));
            GameShaders.Misc["Infernum:RealityTear"].Shader.Parameters["useOutline"].SetValue(true);
            LightningDrawer.Draw(TrailCache, projectile.Size * 0.5f - Main.screenPosition, 82);
            return false;
        }
        #endregion
    }
}
