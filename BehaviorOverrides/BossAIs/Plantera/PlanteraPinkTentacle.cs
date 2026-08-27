using System;
using CalamityMod;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.Plantera
{
    public class PlanteraPinkTentacle : ModNPC
    {
        public Player Target => Main.player[npc.target];

        public ref float Time => ref npc.ai[0];

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Plantera's Tentacle");
            Main.npcFrameCount[npc.type] = 4;

            // Ensure that the tentacle always draws, even when far offscreen.
            NPCID.Sets.MustAlwaysDraw[npc.type] = true;
        }

        public override void SetDefaults()
        {
            npc.noGravity = true;
            npc.noTileCollide = true;
            npc.damage = 120;
            npc.width = 28;
            npc.height = 28;
            npc.defense = 5;
            npc.lifeMax = 500;
            npc.aiStyle = aiType = -1;
            npc.dontTakeDamage = true;
            npc.HitSound = SoundID.NPCHit1;
            npc.DeathSound = SoundID.NPCDeath1;
            npc.hide = true;
        }

        public override void AI()
        {
            // Die if Plantera is absent or not using tentacles.
            if (!Main.npc.IndexInRange(NPC.plantBoss) || Main.npc[NPC.plantBoss].ai[0] != (int)PlanteraBehaviorOverride.PlanteraAttackState.TentacleSnap)
            {
                npc.life = 0;
                npc.HitEffect();
                npc.checkDead();
                npc.netUpdate = true;
                return;
            }

            float attachAngle = npc.ai[0];
            ref float attachOffset = ref npc.ai[1];
            ref float wiggleSineAngle = ref npc.Infernum().ExtraAI[0];

            wiggleSineAngle += Utilities.Remap(Time, -85, 10f, 0f, MathHelper.Pi / 8.5f + npc.whoAmI * 0.1f);
            float wingleOffset = (float)Math.Sin(wiggleSineAngle) * 0.016f;

            // Reel inward prior to snapping.
            if (Time > 0f && Time < 45f)
                attachOffset = MathHelper.Lerp(attachOffset, 108f, 0.05f);

            // Reach outward swiftly in hopes of hitting a target.
            if (Time > 180f)
            {
                attachOffset = MathHelper.Lerp(attachOffset, 3900f, 0.021f);
                wingleOffset = 0f;
            }

            if (Time == 180f)
                Main.PlaySound(SoundID.Item74, npc.Center);

            if (Time > 220f)
            {
                npc.scale *= 0.85f;

                // Die once small enough.
                npc.Opacity = npc.scale;
                if (npc.scale < 0.01f)
                {
                    npc.life = 0;
                    npc.HitEffect();
                    npc.checkDead();
                    npc.active = false;
                }
            }

            attachAngle += wingleOffset;
            npc.Center = Main.npc[NPC.plantBoss].Center + attachAngle.ToRotationVector2() * (attachOffset + wingleOffset * 150f);
            npc.rotation = attachAngle + MathHelper.Pi;
            npc.dontTakeDamage = true;

            Time++;
        }

        public override void DrawBehind(int index)
        {
            Main.instance.DrawCacheNPCsOverPlayers.Add(index);
        }

        public override void FindFrame(int frameHeight)
        {
            npc.frameCounter += 0.2f;
            npc.frameCounter %= Main.npcFrameCount[npc.type];
            int frame = (int)npc.frameCounter;
            npc.frame.Y = frame * frameHeight;
        }

        public override void OnHitPlayer(Player player, int damage, bool crit)
        {
            player.AddBuff(BuffID.Venom, 120, true);
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color drawColor)
        {
            if (!Main.npc.IndexInRange(NPC.plantBoss) || Main.npc[NPC.plantBoss].ai[0] != (int)PlanteraBehaviorOverride.PlanteraAttackState.TentacleSnap)
                return true;

            NPC plantera = Main.npc[NPC.plantBoss];

            Vector2 drawPosition = plantera.Center;
            float rotation = npc.AngleFrom(plantera.Center) + MathHelper.PiOver2;
            bool canStillDraw = true;
            while (canStillDraw)
            {
                int moveDistance = 16;
                if (npc.Distance(drawPosition) < 32f)
                {
                    moveDistance = (int)npc.Distance(drawPosition) - 32 + moveDistance;
                    canStillDraw = false;
                }
                drawPosition += plantera.SafeDirectionTo(npc.Center, Vector2.Zero) * moveDistance;
                Color color = Lighting.GetColor((int)(drawPosition.X / 16f), (int)(drawPosition.Y / 16f));
                Rectangle frame = new Rectangle(0, 0, Main.chain27Texture.Width, moveDistance);
                spriteBatch.Draw(Main.chain27Texture, drawPosition - Main.screenPosition, frame, color, rotation, Main.chain27Texture.Size() * 0.5f, 1f, SpriteEffects.None, 0f);
            }
            return true;
        }

        public override void HitEffect(int hitDirection, double damage)
        {
            for (int k = 0; k < 3; k++)
                Dust.NewDust(npc.position, npc.width, npc.height, 2, hitDirection, -1f, 0, default, 1f);

            if (npc.life <= 0)
            {
                for (int k = 0; k < 15; k++)
                    Dust.NewDust(npc.position, npc.width, npc.height, 2, hitDirection, -1f, 0, default, 1f);
            }
        }
    }
}
