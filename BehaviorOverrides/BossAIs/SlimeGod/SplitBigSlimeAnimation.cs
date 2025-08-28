using CalamityMod.NPCs;
using Microsoft.Xna.Framework;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.SlimeGod
{
    public class SplitBigSlimeAnimation : ModNPC
    {
        public static int OwnerIndex => CalamityGlobalNPC.slimeGod;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Unstable Slime Spawn");
            Main.npcFrameCount[npc.type] = 4;
        }

        public override void SetDefaults()
        {
            npc.aiStyle = aiType = -1;
            npc.damage = 0;
            npc.width = 40;
            npc.height = 30;
            npc.defense = 11;
            npc.lifeMax = 320;
            npc.knockBackResist = 0f;
            animationType = 121;
            npc.alpha = 35;
            npc.lavaImmune = true;
            npc.noGravity = false;
            npc.noTileCollide = true;
            npc.canGhostHeal = false;
            npc.dontTakeDamage = true;
            npc.HitSound = SoundID.NPCHit1;
            npc.DeathSound = SoundID.NPCDeath1;
            npc.buffImmune[BuffID.OnFire] = true;
        }

        public override void SendExtraAI(BinaryWriter writer) => writer.Write(npc.lifeMax);

        public override void ReceiveExtraAI(BinaryReader reader) => npc.lifeMax = reader.ReadInt32();

        public override void AI()
        {
            if (!Main.npc.IndexInRange(OwnerIndex))
            {
                npc.active = false;
                return;
            }

            NPC slimeGod = Main.npc[OwnerIndex];
            float flySpeed = 14f;
            Vector2 destinationOffset = (MathHelper.TwoPi * npc.whoAmI / 13f).ToRotationVector2() * 32f;
            npc.velocity = (npc.velocity * 34f + npc.SafeDirectionTo(slimeGod.Center + destinationOffset) * flySpeed) / 35f;

            npc.Opacity = Utilities.Remap(npc.Distance(slimeGod.Center), 240f, 80f, 1f, 0.1f);
            if (npc.Opacity <= 0.1f)
                npc.active = false;

            npc.rotation = MathHelper.Clamp(npc.velocity.X * 0.05f, -0.2f, 0.2f);
            npc.spriteDirection = (npc.velocity.X < 0f).ToDirectionInt();
        }

        public override void ModifyHitByProjectile(Projectile projectile, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        {
            if (projectile.penetrate > 1 || projectile.penetrate > (-1))
                damage = (int)(damage * 0.1);
        }

        public override void HitEffect(int hitDirection, double damage)
        {
            for (int k = 0; k < 3; k++)
                Dust.NewDust(npc.position, npc.width, npc.height, 4, hitDirection, -1f, 0, default, 1f);

            if (npc.life <= 0)
            {
                for (int k = 0; k < 10; k++)
                    Dust.NewDust(npc.position, npc.width, npc.height, 4, hitDirection, -1f, 0, default, 1f);
            }
        }

        public override bool CheckDead()
        {
            if (!Main.npc.IndexInRange(OwnerIndex))
                return base.CheckDead();

            Main.npc[OwnerIndex].life -= npc.lifeMax;
            Main.npc[OwnerIndex].HitEffect(0, npc.lifeMax);
            if (Main.npc[OwnerIndex].life <= 0)
                Main.npc[OwnerIndex].NPCLoot();

            return base.CheckDead();
        }

        public override bool PreNPCLoot() => false;

        public override void OnHitPlayer(Player player, int damage, bool crit)
        {
            player.AddBuff(BuffID.Weak, 90, true);
        }
    }
}
