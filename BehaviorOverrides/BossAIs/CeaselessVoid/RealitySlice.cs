using CalamityMod;
using CalamityMod.NPCs;
using InfernumMode.BehaviorOverrides.BossAIs.DoG;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.CeaselessVoid
{
    public class RealitySlice : ModProjectile
    {
        internal PrimitiveTrailCopy LightningDrawer;

        public bool Cosmilite;

        public Vector2 Start;

        public Vector2 End;

        public List<Vector2> TrailCache = new List<Vector2>();
        
        public ref float Time => ref projectile.ai[0];

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
            projectile.timeLeft = 84;
            projectile.MaxUpdates = 2;
            cooldownSlot = 1;
        }
        public override void AI()
        {
            // Disappear if neither the Ceaseless Void nor DoG not present.
            if (!Main.npc.IndexInRange(CalamityGlobalNPC.voidBoss) && !Main.npc.IndexInRange(CalamityGlobalNPC.DoGHead))
            {
                projectile.Kill();
                return;
            }			
            float sliceInterpolant = (float)Math.Pow(Utils.InverseLerp(0f, 27f, Time, true), 1.6f);
            projectile.Center = Vector2.Lerp(Start, End, sliceInterpolant);
            if (Time <= 27f)
                TrailCache.Add(projectile.Center);

            // Fade in.
            float disappearInterpolant = Utils.InverseLerp(0f, 16f, projectile.timeLeft / projectile.MaxUpdates, true);
            float scaleGrowInterpolant = (float)Math.Pow(Utils.InverseLerp(0f, 15f, Time, true), 1.72f);
            projectile.Opacity = Utils.InverseLerp(0f, 24f, Time / projectile.MaxUpdates, true) * disappearInterpolant;
            projectile.scale = MathHelper.Lerp(0.24f, 1f, scaleGrowInterpolant) * disappearInterpolant;
            Time++;
        }

        #region Drawing
        internal float WidthFunction(float completionRatio)
        {
            float width = Cosmilite ? 80f : 40f;
            return CalamityUtils.Convert01To010(completionRatio) * projectile.scale * width;
        }

        internal Color ColorFunction(float completionRatio)
        {
            Color baseColor = Color.White;
            if (Cosmilite)
                baseColor = (projectile.localAI[0] == 0f ? Color.Cyan : Color.Fuchsia);

            float opacity = CalamityUtils.Convert01To010(completionRatio) * 1.4f;
            if (opacity >= 1f)
                opacity = 1f;
            opacity *= projectile.Opacity;
            return baseColor * opacity;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            if (LightningDrawer is null)
                LightningDrawer = new PrimitiveTrailCopy(WidthFunction, ColorFunction, null, true, GameShaders.Misc["Infernum:RealityTear"]);

            GameShaders.Misc["Infernum:RealityTear"].SetShaderTexture(ModContent.GetTexture("InfernumMode/ExtraTextures/Stars"));
            GameShaders.Misc["Infernum:RealityTear"].Shader.Parameters["useOutline"].SetValue(true);
            projectile.localAI[0] = 0f;
            LightningDrawer.Draw(TrailCache, projectile.Size * 0.5f - Main.screenPosition, 50);
            if (Cosmilite)
            {
                projectile.localAI[0] = 1f;
                LightningDrawer.Draw(TrailCache, projectile.Size * 0.5f - Main.screenPosition, 50);
            }

            return false;
        }
        #endregion
    }
}
