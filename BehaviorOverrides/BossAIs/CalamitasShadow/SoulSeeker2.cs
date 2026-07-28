using CalamityMod;
using CalamityMod.NPCs;
using CalamityMod.NPCs.Calamitas;
using CalamityMod.Projectiles.Boss;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using static System.Math;
using static Microsoft.Xna.Framework.MathHelper;

namespace InfernumMode.BehaviorOverrides.BossAIs.CalamitasShadow
{
    public class SoulSeeker2 : ModNPC
    {
        public Player Target => Main.player[npc.target];

        public ref float RingAngle => ref npc.ai[0];

        public ref float AngerTimer => ref npc.ai[1];

        public ref float AttackTimer => ref npc.ai[2];

        public override string Texture => "CalamityMod/NPCs/Calamitas/SoulSeeker"; 

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Soul Seeker");
            Main.npcFrameCount[npc.type] = 5;
        }

        public override void SetDefaults()
        {
            npc.aiStyle = aiType = -1;
            npc.damage = 0;
            npc.width = 40;
            npc.height = 30;
            npc.defense = 0;
            npc.lifeMax = 100;
            npc.dontTakeDamage = true;
            npc.knockBackResist = 0f;
            npc.lavaImmune = true;
            npc.noGravity = false;
            npc.noTileCollide = false;
            npc.canGhostHeal = false;
        }

        public override void AI()
        {
            bool brotherIsPresent = NPC.AnyNPCs(ModContent.NPCType<CalamitasRun>()) || NPC.AnyNPCs(ModContent.NPCType<CalamitasRun2>());
            brotherIsPresent |= (Main.npc.IndexInRange(CalamityGlobalNPC.calamitas) && Main.npc[CalamityGlobalNPC.calamitas].ai[3] > 0f && Main.npc[CalamityGlobalNPC.calamitas].ai[3] < 50f);			
            if (!Main.npc.IndexInRange(CalamityGlobalNPC.calamitas) || !brotherIsPresent)
            {
                npc.active = false;
                return;
            }

            NPC calamitas = Main.npc[CalamityGlobalNPC.calamitas];
            npc.target = calamitas.target;
            npc.Center = calamitas.Center + RingAngle.ToRotationVector2() * 950f;
            npc.Opacity = 1f - calamitas.Opacity;

            npc.spriteDirection = (calamitas.Center.X > npc.Center.X).ToDirectionInt();
            if (!Target.WithinRange(calamitas.Center, 1000f))
            {
                npc.spriteDirection = (Target.Center.X > npc.Center.X).ToDirectionInt();

                AngerTimer++;
                AttackTimer++;
                if (AttackTimer >= Lerp(90f, 30f, Utils.InverseLerp(30f, 520f, AngerTimer, true)))
                {
                    Vector2 shootVelocity = npc.SafeDirectionTo(Target.Center + Target.velocity * 15f) * 21f;
                    int dart = Utilities.NewProjectileBetter(npc.Center + shootVelocity, shootVelocity, ModContent.ProjectileType<BrimstoneBarrage>(), CalamitasShadowBehaviorOverride.BrimstoneDartDamage, 0f, -1, 1f);
                    if (Main.projectile.IndexInRange(dart))
                    {
                        Main.projectile[dart].tileCollide = false;
                        Main.projectile[dart].netUpdate = true;
                    }
                    AttackTimer = 0f;
                    npc.netUpdate = true;
                }
            }
            else
            {
                AngerTimer = 0f;
                AttackTimer = 0f;
            }
        }

        public override bool CheckActive() => false;

        public override void FindFrame(int frameHeight)
        {
            npc.frameCounter++;
            npc.frame.Width = 88;
            npc.frame.Height = 105;
            npc.frame.Y = (int)(npc.frameCounter / 5D + RingAngle / MathHelper.TwoPi * 50f) % 5 * npc.frame.Height;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color drawColor)
        {
            Texture2D texture = ModContent.GetTexture("CalamityMod/NPCs/Other/CalamitasEnchantDemon");
            Vector2 drawPosition = npc.Center - Main.screenPosition;
            Vector2 origin = npc.frame.Size() * 0.5f;
            SpriteEffects direction = npc.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            Main.spriteBatch.Draw(texture, drawPosition, npc.frame, npc.GetAlpha(Color.White), npc.rotation, origin, npc.scale, direction, 0f);
            return false;
        }

        public override bool PreNPCLoot() => false;
    }
}