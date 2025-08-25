using CalamityMod;
using CalamityMod.Events;
using CalamityMod.NPCs;
using CalamityMod.NPCs.Calamitas;
using CalamityMod.Projectiles.Boss;
using CalamityMod.World;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.CalamitasClone
{
    public class SoulSeeker2 : ModNPC
    {
        public Player Target => Main.player[npc.target];
        public float RingRadius => Main.npc[CalamityGlobalNPC.calamitas].Infernum().ExtraAI[6];
        public ref float RingAngle => ref npc.ai[0];
        public ref float AngerTimer => ref npc.ai[1];
        public ref float AttackTimer => ref npc.ai[2];

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
            npc.Center = calamitas.Center + RingAngle.ToRotationVector2() * RingRadius;
            npc.Opacity = 1f - calamitas.Opacity;
            bool shouldBeBuffed = CalamityWorld.downedProvidence && !BossRushEvent.BossRushActive && CalamitasCloneBehaviorOverride.ReadyToUseBuffedAI;

            float idealRotation = RingAngle;
            if (!Target.WithinRange(calamitas.Center, RingRadius + 60f))
            {
                idealRotation = npc.AngleTo(Target.Center);

                AngerTimer++;
                AttackTimer++;
                if (AttackTimer >= MathHelper.Lerp(90f, 30f, Utils.InverseLerp(30f, 520f, AngerTimer, true)))
                {
                    int dartDamage = shouldBeBuffed ? 350 : 150;
                    Vector2 shootVelocity = npc.SafeDirectionTo(Target.Center + Target.velocity * 15f) * (shouldBeBuffed ? 30f : 21f);
                    int dart = Utilities.NewProjectileBetter(npc.Center + shootVelocity, shootVelocity, ModContent.ProjectileType<BrimstoneBarrage>(), dartDamage, 0f);
                    if (Main.projectile.IndexInRange(dart))
                    {
                        Main.projectile[dart].ai[0] = 1f;
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
            npc.rotation = npc.rotation.AngleLerp(idealRotation, 0.05f).AngleTowards(idealRotation, 0.1f);
        }

        public override bool CheckActive() => false;

        public override void FindFrame(int frameHeight)
        {
            npc.frameCounter++;
            npc.frame.Y = (int)(npc.frameCounter / 5D + RingAngle / MathHelper.TwoPi * 50f) % Main.npcFrameCount[npc.type] * frameHeight;
        }

        public override bool PreNPCLoot() => false;
    }
}
