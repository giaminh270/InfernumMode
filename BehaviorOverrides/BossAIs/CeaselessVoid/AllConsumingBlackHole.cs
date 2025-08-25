using CalamityMod;
using CalamityMod.Items.Weapons.DraedonsArsenal;
using CalamityMod.Items.Weapons.Typeless;
using CalamityMod.NPCs;
using CalamityMod.Sounds;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using System.Collections.Generic;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;


namespace InfernumMode.BehaviorOverrides.BossAIs.CeaselessVoid
{
    public class AllConsumingBlackHole : ModProjectile
    {
        public float Radius => projectile.scale * 360f;

        public static Player Target => Main.player[Main.npc[CalamityGlobalNPC.voidBoss].target];

        public ref float Timer => ref projectile.ai[0];

        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public override void SetStaticDefaults() => DisplayName.SetDefault("All-Consuming Black Hole");

        public override void SetDefaults()
        {
            projectile.width = 240;
            projectile.height = 240;
            projectile.alpha = 255;
            projectile.penetrate = -1;
            projectile.ignoreWater = true;
            projectile.tileCollide = false;
            projectile.hostile = true;
            projectile.timeLeft = projectile.MaxUpdates * 540;
            projectile.hide = true;
        }

        public override void AI()
        {
            // Disappear if the Ceaseless Void is not present.
            if (!Main.npc.IndexInRange(CalamityGlobalNPC.voidBoss))
            {
                projectile.Kill();
                return;
            }

            // Stick to the Ceaseless Void.
            NPC ceaselessVoid = Main.npc[CalamityGlobalNPC.voidBoss];
            projectile.Center = ceaselessVoid.Center;
            projectile.Size = Vector2.One * Radius;

            // Create a slice effect on the first frame.
            if (Timer == 2f)
            {
			Main.PlaySound(mod.GetLegacySoundSlot(SoundType.Item, "Sounds/Item/YanmeiKnifeHit"), (int)Target.Center.X, (int)Target.Center.Y);
			Main.PlaySound(mod.GetLegacySoundSlot(SoundType.Item, "Sounds/Item/TeslaCannonFire"), (int)Target.Center.X, (int)Target.Center.Y);

                Target.Calamity().GeneralScreenShakePower = 20f;
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    float leftOffsetAngle = MathHelper.PiOver2 - MathHelper.Pi * 0.3f;
                    float rightOffsetAngle = MathHelper.PiOver2 + MathHelper.Pi * 0.3f;
                    int slice = Utilities.NewProjectileBetter(projectile.Center, Vector2.Zero, ModContent.ProjectileType<RealitySlice>(), 0, 0f);
                    Main.projectile[slice].ModProjectile<RealitySlice>().Start = projectile.Center - leftOffsetAngle.ToRotationVector2() * 1100f;
                    Main.projectile[slice].ModProjectile<RealitySlice>().End = projectile.Center + leftOffsetAngle.ToRotationVector2() * 1100f;

                    slice = Utilities.NewProjectileBetter(projectile.Center, Vector2.Zero, ModContent.ProjectileType<RealitySlice>(), 0, 0f);
                    Main.projectile[slice].ModProjectile<RealitySlice>().Start = projectile.Center - rightOffsetAngle.ToRotationVector2() * 1100f;
                    Main.projectile[slice].ModProjectile<RealitySlice>().End = projectile.Center + rightOffsetAngle.ToRotationVector2() * 1100f;

                    slice = Utilities.NewProjectileBetter(projectile.Center, Vector2.Zero, ModContent.ProjectileType<RealitySlice>(), 0, 0f);
                    Main.projectile[slice].ModProjectile<RealitySlice>().Start = projectile.Center - Vector2.UnitY * 1400f;
                    Main.projectile[slice].ModProjectile<RealitySlice>().End = projectile.Center + Vector2.UnitY * 1400f;
                }
            }

            // Fade in.
            float disappearInterpolant = Utils.InverseLerp(0f, 24f, projectile.timeLeft / projectile.MaxUpdates, true);
            float scaleGrowInterpolant = (float)Math.Pow(Utils.InverseLerp(0f, 64f, Timer, true), 1.72);
            projectile.Opacity = Utils.InverseLerp(0f, 24f, Timer / projectile.MaxUpdates, true) * disappearInterpolant;
            projectile.scale = MathHelper.Lerp(0.24f, 1f, scaleGrowInterpolant) * disappearInterpolant;
            Timer++;

            // Suck the player in.
            float suckPower = MathHelper.Lerp(0.4f, 0.7f, Timer / 540f);
            for (int i = 0; i < Main.maxPlayers; i++)
            {
                float distance = Main.player[i].Distance(projectile.Center);
                if (distance < 1900f && Main.player[i].grappling[0] == -1)
                {
                    if (Collision.CanHit(projectile.Center, 1, 1, Main.player[i].Center, 1, 1))
                    {
                        float distanceRatio = distance / 1900f;
                        float multiplier = 1f - distanceRatio;

                        if (Main.player[i].Center.X < projectile.Center.X)
                            Main.player[i].velocity.X += suckPower * multiplier;
                        else
                            Main.player[i].velocity.X -= suckPower * multiplier;

                        if (Main.player[i].Center.Y < projectile.Center.Y)
                            Main.player[i].velocity.Y += suckPower * multiplier;
                        else
                            Main.player[i].velocity.Y -= suckPower * multiplier;
                    }
                }
            }

            // Release things that fly into the black hole.
            int energyReleaseRate = 3;
            if (Timer >= 135f && Timer % energyReleaseRate == energyReleaseRate - 1f && projectile.scale >= 1f)
            {
                Vector2 asteroidSpawnOffset = Main.rand.NextVector2CircularEdge(640f, 640f);
                Vector2 asteroidSpawnPosition = Target.Center + asteroidSpawnOffset;
                Vector2 asteroidShootVelocity = (ceaselessVoid.Center - asteroidSpawnPosition).SafeNormalize(Vector2.UnitY) * 14f;
                Utilities.NewProjectileBetter(asteroidSpawnPosition, asteroidShootVelocity, ModContent.ProjectileType<DarkEnergyBolt>(), 275, 0f);
            }
        }

        public override void Kill(int timeLeft)
        {
            Utilities.DeleteAllProjectiles(true, ModContent.ProjectileType<DarkEnergyBolt>());
        }

        #region Drawing
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
            int sideCount = 512;
            Utilities.GetCircleVertices(sideCount, Radius, projectile.Center, out var triangleIndices, out var vertices);
            CalamityUtils.CalculatePerspectiveMatricies(out Matrix view, out Matrix projection);
            GameShaders.Misc["Infernum:RealityTear"].SetShaderTexture(ModContent.GetTexture("InfernumMode/ExtraTextures/Stars"));
            GameShaders.Misc["Infernum:RealityTear"].Shader.Parameters["uWorldViewProjection"].SetValue(view * projection);
            GameShaders.Misc["Infernum:RealityTear"].Shader.Parameters["useOutline"].SetValue(false);
            GameShaders.Misc["Infernum:RealityTear"].Apply();

            Main.instance.GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertices.ToArray(), 0, vertices.Count, triangleIndices.ToArray(), 0, sideCount * 2);
            Main.pixelShader.CurrentTechnique.Passes[0].Apply();

            // Draw the vortex.
            Texture2D noiseTexture = ModContent.GetTexture("CalamityMod/ExtraTextures/VoronoiShapes");
            Vector2 drawPosition = projectile.Center - Main.screenPosition;
            Vector2 origin = noiseTexture.Size() * 0.5f;
            Main.spriteBatch.EnterShaderRegion();

            Vector2 diskScale = projectile.scale * new Vector2(1.3f, 1.1f);
            GameShaders.Misc["CalamityMod:DoGPortal"].UseOpacity(projectile.Opacity);
            GameShaders.Misc["CalamityMod:DoGPortal"].UseColor(Color.Fuchsia);
            GameShaders.Misc["CalamityMod:DoGPortal"].UseSecondaryColor(Color.Black);
            GameShaders.Misc["CalamityMod:DoGPortal"].Apply();

            for (int i = 0; i < 4; i++)
                Main.spriteBatch.Draw(noiseTexture, drawPosition, null, Color.White, 0f, origin, diskScale * 2f, SpriteEffects.None, 0f);
            Main.spriteBatch.ExitShaderRegion();

            // Draw the black hole.
            Texture2D blackHoleTexture = ModContent.GetTexture("InfernumMode/ExtraTextures/WhiteHole");
            Vector2 blackHoleScale = Vector2.One * Radius / blackHoleTexture.Size() * 1.2f;
            Main.spriteBatch.SetBlendState(BlendState.Additive);
            for (int i = 0; i < 3; i++)
            {
                Vector2 offset = (CalamityUtils.PerlinNoise2D(i / 3f, i / 8f + Main.GlobalTime * 0.04f, 4, projectile.identity) * 12f).ToRotationVector2() * 8f;
                Main.spriteBatch.Draw(blackHoleTexture, drawPosition + offset, null, Color.Pink, 0f, blackHoleTexture.Size() * 0.5f, blackHoleScale * 1.06f, SpriteEffects.None, 0f);
            }
            Main.spriteBatch.ExitShaderRegion();
            for (int i = 0; i < 3; i++)
                Main.spriteBatch.Draw(blackHoleTexture, drawPosition, null, Color.Black, 0f, blackHoleTexture.Size() * 0.5f, blackHoleScale, SpriteEffects.None, 0f);

            return false;
        }

		public override void DrawBehind(int index, List<int> drawCacheProjsBehindNPCsAndTiles, List<int> drawCacheProjsBehindNPCs, List<int> drawCacheProjsBehindProjectiles, List<int> drawCacheProjsOverWiresUI)
        {
            drawCacheProjsBehindProjectiles.Add(index);
        }
        #endregion
    }
}
