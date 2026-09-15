using CalamityMod;
using InfernumMode;
using InfernumMode.Sounds;
using InfernumMode.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Utilities;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using WayfinderItem = InfernumMode.Items.Wayfinder;
using System;

namespace InfernumMode.Projectiles
{
    public class WayfinderItemProjectile : ModProjectile
    {
        #region Fields + Properties

        public PrimitiveTrailCopy LightDrawer;

        public SlotId SoundID;

        public Player Owner => Main.player[projectile.owner];

        public ref float Time => ref projectile.ai[0];

        public const int RayCreationTime = 120;

        public const int RayExpandTime = 50;

        public const int IdleDrawTime = 72;

        public const int VisualEffectsDissipateTime = 60;

        public const int Lifetime = RayCreationTime + RayExpandTime + IdleDrawTime + VisualEffectsDissipateTime; // 302

        #endregion

        #region Overrides
        public override string Texture => "InfernumMode/Items/Wayfinder";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Wayfinder");
            Main.projFrames[projectile.type] = 8;
        }

        public override void SetDefaults()
        {
            projectile.width = 56;
            projectile.height = 60;
            projectile.friendly = true;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;
            projectile.netImportant = true;
            projectile.timeLeft = Lifetime;
            projectile.penetrate = -1;
            projectile.hide = true;
        }

        public override void AI()
        {
            // Slow down after flying upward for long enough.
            if (Time >= 45f)
                projectile.velocity *= 0.9f;

            // Decide frames.
            projectile.frameCounter++;
            projectile.frame = projectile.frameCounter / 5 % Main.projFrames[projectile.type];

            if (Time == 0) // 170
                Main.PlaySound(InfernumSoundRegistry.WayfinderObtainSound, projectile.Center);


            if (Time < RayCreationTime + RayExpandTime + IdleDrawTime && Time > 10f)
            {
                float interpolant = (Time - 10f) / (RayCreationTime + RayExpandTime + IdleDrawTime - 10f);
                int amount = (int)MathHelper.Lerp(0, 6f, interpolant);
                float offsetAmount = MathHelper.Lerp(0f, 25f, interpolant);
                float scale = MathHelper.Lerp(0f, 1.3f, interpolant);
                WayfinderHoldout.CreateFlameExplosion(projectile.Center, offsetAmount, offsetAmount, amount, scale, 30);
            }

            if (Time >= RayCreationTime + RayExpandTime + IdleDrawTime)
            {
                int fireLifetime = 30;
                if (Time is Lifetime)
                    fireLifetime = 60;

                WayfinderHoldout.CreateFlameExplosion(projectile.Center, 25f, 25f, 30, 1.3f, fireLifetime);
            }

            projectile.rotation = -MathHelper.PiOver4;

            Time++;
        }

        public override bool CanDamage() => false;

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers)
        {
            overPlayers.Add(index);
        }

        public override void Kill(int timeLeft)
        {
            if (Main.myPlayer != projectile.owner)
                return;

            // If server-side, then the item must be spawned for each client individually.
            int itemID = ModContent.ItemType<WayfinderItem>();
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                int item = Item.NewItem(projectile.Center, itemID, 1, true, -1);
                for (int i = 0; i < Main.maxPlayers; ++i)
                {
                    if (Main.player[i].active)
                        NetMessage.SendData(MessageID.InstancedItem, i, -1, null, item);
                }

                Main.item[item].active = false;
            }

            // Otherwise just drop the item.
            else
                Item.NewItem(projectile.Center, itemID, 1, true, -1);
        }
        #endregion

        #region Drawing
        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Main.spriteBatch.EnterShaderRegion(BlendState.Additive);

            float dissipateInterpolant = Utils.InverseLerp(Lifetime, Lifetime - VisualEffectsDissipateTime, Time, true);
            float rayExpandFactor = MathHelper.Lerp(1f, 2f, MathHelper.Clamp((Time - RayCreationTime - RayExpandTime) / 90f, 0f, 1000f)) * dissipateInterpolant;
            DrawBloomCircle(rayExpandFactor);
            DrawLightRays();

            Main.spriteBatch.ExitShaderRegion();
            DrawWayfinder();
            return false;
        }

        public float DrawLightRays()
        {
            // Draw a bunch of god rays.
            float dissipateInterpolant = Utils.InverseLerp(Lifetime, Lifetime - VisualEffectsDissipateTime, Time, true);
            float totalDeathRays = MathHelper.Lerp(0f, 8f, Utils.InverseLerp(0f, RayCreationTime, Time, true)) * dissipateInterpolant;
            float rayExpandFactor = MathHelper.Lerp(1f, 2f, MathHelper.Clamp((Time - RayCreationTime - RayExpandTime) / 90f, 0f, 1000f)) * dissipateInterpolant;

            for (int i = 0; i < (int)totalDeathRays; i++)
            {
                float rayAnimationCompletion = 1f;
                if (i == (int)totalDeathRays - 1f)
                    rayAnimationCompletion = totalDeathRays - (int)totalDeathRays;
                rayAnimationCompletion *= rayExpandFactor;

                ulong seed = (ulong)(i + 1) * 3141592uL;
                float rayDirection = MathHelper.TwoPi * i / 8f + (float)Math.Sin(Main.GlobalTime * (i + 1f) * 0.3f) * 0.51f;
                rayDirection += Main.GlobalTime * 0.48f;
                DrawLightRay(seed, rayDirection, rayAnimationCompletion, projectile.Center);
            }

            return rayExpandFactor;
        }

        public void DrawWayfinder()
        {
            Texture2D texture = ModContent.GetTexture(Texture);
            Vector2 drawPosition = projectile.Center - Main.screenPosition;
            Rectangle frame = texture.Frame(1, Main.projFrames[projectile.type], 0, projectile.frame);
            Vector2 origin = frame.Size() * 0.5f;
            Main.spriteBatch.Draw(texture, drawPosition, frame, Color.White * 0.7f, projectile.rotation, origin, projectile.scale, 0, 0f);
        }

        public void DrawBloomCircle(float rayExpandFactor)
        {
            // Create bloom over the waypoint.
            float dissipateInterpolant = Utils.InverseLerp(Lifetime, Lifetime - VisualEffectsDissipateTime, Time, true);
            float bloomInterpolant = Utils.InverseLerp(0f, RayCreationTime * 0.67f, Time, true) * dissipateInterpolant;
            if (bloomInterpolant > 0f)
            {
                Texture2D bloomCircle = ModContent.GetTexture("CalamityMod/ExtraTextures/THanosAura");
                Vector2 drawPosition = projectile.Center - Main.screenPosition;
                Vector2 bloomSize = new Vector2(200f) / bloomCircle.Size() * (float)Math.Pow(bloomInterpolant, 2f);
                bloomSize *= 1f + (rayExpandFactor - 1f) * 2f;

                Main.spriteBatch.Draw(bloomCircle, drawPosition, null, Color.Orange * bloomInterpolant, 0f, bloomCircle.Size() * 0.5f, bloomSize, 0, 0f);
                Main.spriteBatch.Draw(bloomCircle, drawPosition, null, Color.Orange * bloomInterpolant, 0f, bloomCircle.Size() * 0.5f, bloomSize * 0.8f, 0, 0f);
                Main.spriteBatch.Draw(bloomCircle, drawPosition, null, Color.Yellow * bloomInterpolant, 0f, bloomCircle.Size() * 0.5f, bloomSize * 0.55f, 0, 0f);
                Main.spriteBatch.Draw(bloomCircle, drawPosition, null, Color.Wheat * bloomInterpolant, 0f, bloomCircle.Size() * 0.5f, bloomSize * 0.5f, 0, 0f);
            }
        }

        public void DrawLightRay(ulong seed, float initialRayRotation, float rayBrightness, Vector2 rayStartingPoint)
        {
            // Parameters are not correctly passed into the delegates after the primitive drawer is created.
            // As a substitute, a direct NPC variable is used as storage to allow for access.
            projectile.Infernum().ExtraAI[8] = rayBrightness;

            float rayWidthFunction(float completionRatio, float rayBrightness2)
            {
                return MathHelper.Lerp(2f, 14f, completionRatio) * (1f + (rayBrightness2 - 1f) * 1.6f);
            }
            Color rayColorFunction(float completionRatio, float rayBrightness2)
            {
                float dissipateInterpolant = Utils.InverseLerp(Lifetime, Lifetime - VisualEffectsDissipateTime, Time, true);
                return Color.White * projectile.Opacity * Utils.InverseLerp(0.8f, 0.5f, completionRatio, true) * MathHelper.Clamp(0f, 0.65f, rayBrightness2) * dissipateInterpolant;
            }

            if (LightDrawer is null)
                LightDrawer = new PrimitiveTrailCopy(c => rayWidthFunction(c, projectile.Infernum().ExtraAI[8]), c => rayColorFunction(c, projectile.Infernum().ExtraAI[8]), null, false);

            Vector2 currentRayDirection = initialRayRotation.ToRotationVector2();
            float length = MathHelper.Lerp(125f, 220f, Utils.RandomFloat(ref seed)) * rayBrightness;
            List<Vector2> points = new List<Vector2>();
            for (int i = 0; i <= 12; i++)
                points.Add(Vector2.Lerp(rayStartingPoint, rayStartingPoint + initialRayRotation.ToRotationVector2() * length, i / 12f));

            LightDrawer.Draw(points, -Main.screenPosition, 47);
        }
        #endregion
    }
}