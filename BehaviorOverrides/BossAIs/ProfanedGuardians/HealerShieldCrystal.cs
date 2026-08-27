using CalamityMod;
using CalamityMod.NPCs;
using CalamityMod.Particles;
using InfernumMode.Sounds;
using InfernumMode.GlobalInstances;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using InfernumMode.ExtraTextures;
using InfernumMode.Effects;

namespace InfernumMode.BehaviorOverrides.BossAIs.ProfanedGuardians
{
    public class HealerShieldCrystal : ModNPC
    {
        public enum CrystalState
        {
            Alive,
            Shattering
        }

        public ref float CurrentState => ref npc.ai[0];

        public ref float ShatteringTimer => ref npc.ai[1];

        public Vector2 InitialPosition;

        public static Texture2D WallTexture => ModContent.GetTexture("InfernumMode/BehaviorOverrides/BossAIs/ProfanedGuardians/HealerShieldWall");

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Holy Shield");
        }

        public override void SetDefaults()
        {
            npc.width = 42;
            npc.height = 102;
            npc.lifeMax = 25000;
            npc.knockBackResist = 0;
            npc.defense = 50;
            npc.DR_NERD(0.15f);
            npc.canGhostHeal = false;
            npc.noGravity = true;
            npc.HitSound = SoundID.NPCHit52;
            npc.DeathSound = SoundID.NPCDeath55;
            npc.Opacity = 0;
        }

        public override void AI()
        {
            int commanderIndex = GuardianComboAttackManager.FindCommanderIndex();
            if (commanderIndex == -1)
            {
                npc.active = false;
                npc.netUpdate = true;
                return;
            }

            CalamityGlobalNPC.doughnutBoss = commanderIndex;
            if (ShatteringTimer == 0)
                InitialPosition = npc.Center;

            // Declare this as the active crystal.
            GlobalNPCOverrides.ProfanedCrystal = npc.whoAmI;

            // Do not take damage by default.
            npc.dontTakeDamage = true;

            npc.Opacity = MathHelper.Clamp(npc.Opacity + 0.01f, 0f, 1f);

            NPC commander = Main.npc[commanderIndex];

            // Get the commanders target.
            Player target = Main.player[commander.target];

            switch ((CrystalState)CurrentState)
            {
                case CrystalState.Alive:
                    DoBehavior_SitStill(target);
                    break;
                case CrystalState.Shattering:
                    DoBehavior_Shatter(target);
                    break;
            }

            // Force anyone close to it to be to the left.
            foreach (Player player in Main.player)
                if (player.active && !player.dead && Vector2.Distance(player.Center, npc.Center) <= 6000f)
                    if (player.Center.X > npc.Center.X)
                        player.Center = new Vector2(npc.Center.X, player.Center.Y);

            ShatteringTimer++;
        }

        public void DoBehavior_SitStill(Player target)
        {
            // If the target is close enough, take damage.
            if (target.WithinRange(npc.Center * npc.Opacity, 1000))
                npc.dontTakeDamage = false;

            float sparkleRate = 12f;

            // Spawn sparkles.
            if (ShatteringTimer % sparkleRate == 0)
            {
                Vector2 position = npc.Center + Main.rand.NextVector2Circular(npc.width * 1.3f, npc.height);
                Vector2 velocity = -Vector2.UnitY.RotatedBy(Main.rand.NextFloat(-0.1f, 0.1f)) * Main.rand.NextFloat(0.5f, 1f);
                Color color = Color.Lerp(Color.HotPink, Color.LightPink, Main.rand.NextFloat());
                Vector2 scale = new Vector2(0.5f + Main.rand.NextFloat());
                Particle sparkle;
                if (Main.rand.NextBool())
                    sparkle = new GenericSparkle(position, velocity, color, Color.White, Main.rand.NextFloat(0.5f, 0.75f), 75, Main.rand.NextFloat(0.05f), 2f);
                else
                    sparkle = new FlareShine(position, velocity, color, Color.White, -MathHelper.PiOver2, scale, scale * 1.5f, 60, Main.rand.NextFloat(0.05f), 2f);
                GeneralParticleHandler.SpawnParticle(sparkle);
            }
        }

        public void DoBehavior_Shatter(Player target)
        {
            float attackLength = 180;
            float offsetAmount = MathHelper.Lerp(0, 15, ShatteringTimer / attackLength * npc.Opacity);
            npc.Center = InitialPosition + Main.rand.NextVector2Circular(offsetAmount, offsetAmount);
            npc.netUpdate = true;

            if (ShatteringTimer == 0)
                Main.PlaySound(InfernumSoundRegistry.ProvidenceDoorShatterSound, target.Center * npc.Opacity);

            if (ShatteringTimer >= attackLength)
            {
                // Die
                npc.active = false;

                Vector2 wallBottom = npc.Center + new Vector2(21f, 989f);
                Vector2 wallTop = npc.Center + new Vector2(21f, -989f);

                float crystalAmount = 50f;
                for (int i = 0; i < crystalAmount; i++)
                {
                    Vector2 crystalSpawnPosition = Vector2.Lerp(wallBottom, wallTop, (float)i / crystalAmount) + Main.rand.NextVector2Circular(24f, 24f);
                    Vector2 crystalVelocity = -Vector2.UnitX.RotatedByRandom(1.06f) * Main.rand.NextFloat(2f, 4f);

                    if (!Collision.SolidCollision(crystalSpawnPosition, 1, 1) && Main.netMode != NetmodeID.Server)
                    {
                        int goreType = mod.GetGoreSlot($"Gores/ProvidenceDoor{Main.rand.Next(1, 3)}");
                        Gore.NewGore(crystalSpawnPosition, crystalVelocity, goreType, 1.16f);
                    }
                }

                // Despawn all the fire walls.
                Utilities.DeleteAllProjectiles(true, ModContent.ProjectileType<HolyFireWall>());

                if (!Main.npc.IndexInRange(CalamityGlobalNPC.doughnutBoss))
                    return;

                NPC guard = Main.npc[CalamityGlobalNPC.doughnutBoss];
                // Tell the commander to swap attacks. The other guardians use this.
                GuardianComboAttackManager.SelectNewAttack(guard, ref guard.ai[1], (float)GuardianComboAttackManager.GuardiansAttackType.SoloHealer);
            }
        }


        public override void DrawBehind(int index) => Main.instance.DrawCacheNPCProjectiles.Add(index);

        public override bool PreDraw(SpriteBatch spriteBatch, Color drawColor)
        {
            Texture2D mainTexture = Main.npcTexture[npc.type];
            Vector2 drawPosition = npc.Center - Main.screenPosition;
            Vector2 origin = mainTexture.Size() * 0.5f;

            DrawWall(spriteBatch, drawPosition);
            DrawMovingBackglow(spriteBatch, mainTexture, drawPosition, npc.frame);
            DrawBackglow(spriteBatch, mainTexture, drawPosition, npc.frame);
            spriteBatch.Draw(mainTexture, drawPosition, null, Color.White * npc.Opacity, npc.rotation, origin, npc.scale, SpriteEffects.None, 0f);
            return false;
        }

        public void DrawWall(SpriteBatch spriteBatch, Vector2 centerPosition)
        {
            // Draw variables.
            Vector2 drawPosition = centerPosition + new Vector2(21, 0);
            Rectangle frame = new Rectangle(0, 0, WallTexture.Width, WallTexture.Height);

            // Draw the initial wall.
            DrawBackglow(spriteBatch, WallTexture, drawPosition, frame);
            spriteBatch.Draw(WallTexture, drawPosition, frame, Color.White * npc.Opacity, 0f, WallTexture.Size() * 0.5f, 1f, SpriteEffects.None, 0f);

            // More variables
            spriteBatch.EnterShaderRegion();
            float lifeRatio = (float)npc.life / npc.lifeMax;
            float opacity = MathHelper.Lerp(0f, 0.125f, lifeRatio);
            float interpolant = CurrentState == (float)CrystalState.Shattering ? ShatteringTimer / 120f : 0f;
            float shaderWallOpacity = MathHelper.Lerp(0.02f, 0f, interpolant);

            // Initialize the shader.
            Texture2D shaderLayer = InfernumTextureRegistry.HolyCrystalLayer;
            InfernumEffectsRegistry.RealityTear2Shader.SetShaderTexture(shaderLayer);
            InfernumEffectsRegistry.RealityTear2Shader.Shader.Parameters["fadeOut"].SetValue(true);

            // Draw a large overlay behin the wall, as if they are being protected by it.
            Texture2D magicPixel = Main.magicPixel;
            Vector2 scale = new Vector2(53.6f, 2010f) / Main.magicPixel.Size();
            Rectangle overlayFrame = new Rectangle(0, 0, (int)(magicPixel.Width * scale.X), (int)(magicPixel.Height * scale.Y));
            DrawData overlay = new DrawData(magicPixel, centerPosition + new Vector2(1290, 0), overlayFrame, Color.White * shaderWallOpacity * npc.Opacity, 0f, overlayFrame.Size() * 0.5f, scale, SpriteEffects.None, 0);
            InfernumEffectsRegistry.RealityTear2Shader.Apply(overlay);
            overlay.Draw(spriteBatch);

            // Draw the wall overlay.
            DrawData wall = new DrawData(WallTexture, drawPosition, frame, Color.White * opacity * 0.5f * npc.Opacity, 0f, WallTexture.Size() * 0.5f, 1f, SpriteEffects.None, 0);
            InfernumEffectsRegistry.RealityTear2Shader.Apply(wall);
            wall.Draw(spriteBatch);

            spriteBatch.ExitShaderRegion();
        }

        public void DrawBackglow(SpriteBatch spriteBatch, Texture2D npcTexture, Vector2 drawPosition, Rectangle frame)
        {
            float backglowAmount = 12;
            for (int i = 0; i < backglowAmount; i++)
            {
                Vector2 backglowOffset = (MathHelper.TwoPi * i / backglowAmount).ToRotationVector2() * 2f;
                Color backglowColor = MagicSpiralCrystalShot.ColorSet[0];
                backglowColor.A = 0;
                spriteBatch.Draw(npcTexture, drawPosition + backglowOffset, frame, backglowColor * npc.Opacity, npc.rotation, frame.Size() * 0.5f, 1f, SpriteEffects.None, 0);
            }
        }

        public void DrawMovingBackglow(SpriteBatch spriteBatch, Texture2D npcTexture, Vector2 drawPosition, Rectangle frame)
        {
            float backglowAmount = 5;
            for (int i = 0; i < backglowAmount; i++)
            {
                Vector2 backglowOffset = (MathHelper.TwoPi * i / backglowAmount + Main.GlobalTime * 2f).ToRotationVector2() * 7f;
                Color backglowColor = Color.Lerp(MagicSpiralCrystalShot.ColorSet[0], MagicSpiralCrystalShot.ColorSet[1], 0.5f);
                backglowColor.A = 0;
                spriteBatch.Draw(npcTexture, drawPosition + backglowOffset, frame, backglowColor * npc.Opacity, npc.rotation, frame.Size() * 0.5f, 1f, SpriteEffects.None, 0);
            }
        }

        public override bool CheckActive() => false;

        public override bool CheckDead()
        {
            CurrentState = (int)CrystalState.Shattering;
            ShatteringTimer = 0;
            npc.life = npc.lifeMax;
            npc.netUpdate = true;

            return false;
        }
    }
}
