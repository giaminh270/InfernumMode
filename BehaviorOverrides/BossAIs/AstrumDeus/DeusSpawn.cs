using CalamityMod;
using CalamityMod.Dusts;
using CalamityMod.Events;
using CalamityMod.NPCs.AstrumDeus;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.AstrumDeus
{
    public class DeusSpawn : ModNPC
    {
        public bool OrbitAroundDeus
        {
            get => npc.ai[3] == 0f;
            set => npc.ai[3] = value ? 0f : 1f;
        }

        public ref float OrbitOffsetAngle => ref npc.ai[0];

        public ref float OrbitOffsetRadius => ref npc.ai[1];

        public ref float OrbitAngularVelocity => ref npc.ai[2];

        public Player Target => Main.player[npc.target];

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Deus Spawn");
            Main.npcFrameCount[npc.type] = 4;
        }

        public override void SetDefaults()
        {
            npc.damage = 170;
            npc.npcSlots = 0f;
            npc.width = npc.height = 62;
            npc.defense = 0;
            npc.lifeMax = 3700;
            if (BossRushEvent.BossRushActive)
                npc.lifeMax = 35500;

            npc.aiStyle = aiType = -1;
            npc.knockBackResist = 0f;
            npc.noGravity = true;
            npc.noTileCollide = true;
            npc.canGhostHeal = false;
            npc.HitSound = SoundID.NPCHit1;
            npc.Calamity().canBreakPlayerDefense = true;
        }

        public override void AI()
        {
            // Disable contact damage when orbiting.
            npc.damage = 0;

            // Fuck off if Deus is not present.
            int deusIndex = NPC.FindFirstNPC(ModContent.NPCType<AstrumDeusHeadSpectral>());
            if (deusIndex == -1)
            {
                npc.active = false;
                return;
            }

            NPC astrumDeus = Main.npc[deusIndex];

            // Orbit around deus as necessary.
            if (OrbitAroundDeus)
            {
                OrbitOffsetAngle += MathHelper.ToRadians(OrbitAngularVelocity);
                npc.Center = astrumDeus.Center + OrbitOffsetAngle.ToRotationVector2() * OrbitOffsetRadius;
                npc.spriteDirection = (Math.Cos(OrbitOffsetAngle) > 0f).ToDirectionInt();
                npc.rotation = (float)Math.Sin(OrbitOffsetAngle) * 0.11f;
                return;
            }

            // If the spawn shouldn't orbit deus, have it weakly home in on targets and do damage again.
            float flySpeed = BossRushEvent.BossRushActive ? 28f : 19.5f;
            npc.damage = npc.defDamage;
            npc.target = astrumDeus.target;
            npc.velocity = (npc.velocity * 59f + npc.SafeDirectionTo(Target.Center) * flySpeed) / 60f;
            npc.spriteDirection = (npc.velocity.X > 0f).ToDirectionInt();
            npc.rotation = npc.rotation.AngleLerp(npc.velocity.X * 0.02f, 0.25f);

            // Go bye bye and explode if sufficiently close to the target or enough time has passed.
            npc.ai[3]++;
            if (npc.WithinRange(Target.Center, 105f) || npc.ai[3] >= 175f)
            {
                npc.active = false;
                PreNPCLoot();
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color drawColor)
        {
            Texture2D texture = ModContent.GetTexture(Texture);
            Texture2D glowmask = ModContent.GetTexture("InfernumMode/BehaviorOverrides/BossAIs/AstrumDeus/DeusSpawnGlow");
            Vector2 drawPosition = npc.Center - Main.screenPosition;
            Vector2 origin = npc.frame.Size() * 0.5f;
            SpriteEffects direction = npc.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : 0;
            drawColor = Color.Lerp(drawColor, Color.White, 0.5f);

            spriteBatch.Draw(texture, drawPosition, npc.frame, npc.GetAlpha(drawColor), npc.rotation, origin, npc.scale, direction, 0);
            spriteBatch.Draw(glowmask, drawPosition, npc.frame, npc.GetAlpha(Color.White), npc.rotation, origin, npc.scale, direction, 0);
            return false;
        }

        public override bool PreNPCLoot()
        {
            Main.PlaySound(SoundID.DD2_KoboldExplosion, npc.Center);
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return false;

            // Release astral flames.
            for (int i = 0; i < 1; i++)
                Dust.NewDustDirect(npc.position, npc.width, npc.height, ModContent.DustType<AstralOrange>(), 0f, 0f, 50, default, 1f);

            for (int i = 0; i < 5; i++)
            {
                Dust fire = Dust.NewDustDirect(npc.position, npc.width, npc.height, ModContent.DustType<AstralOrange>(), 0f, 0f, 0, default, 0.8f);
                fire.noGravity = true;
                fire.velocity *= 3f;
            }

            // Create a spread of homing astral plasma.
            for (int i = 0; i < 8; i++)
            {
                Vector2 cinderVelocity = (MathHelper.TwoPi * i / 8f).ToRotationVector2() * 5.5f;
                Utilities.NewProjectileBetter(npc.Center, cinderVelocity, ModContent.ProjectileType<AstralPlasmaSpark>(), 180, 0f);
            }
            return false;
        }

        public override void FindFrame(int frameHeight)
        {
            npc.frameCounter++;

            if (npc.frameCounter >= 6D)
            {
                npc.frame.Y += frameHeight;
                if (npc.frame.Y >= frameHeight * Main.npcFrameCount[npc.type])
                    npc.frame.Y = 0;

                npc.frameCounter = 0D;
            }
        }
    }
}
