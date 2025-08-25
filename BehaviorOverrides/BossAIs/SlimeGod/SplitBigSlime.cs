using CalamityMod.Events;
using Microsoft.Xna.Framework;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.SlimeGod
{
    public class SplitBigSlime : ModNPC
    {
        public int OwnerIndex => (int)npc.ai[1];

        public ref float RedirectCountdown => ref npc.ai[0];

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Unstable Slime Spawn");
            Main.npcFrameCount[npc.type] = 4;
        }

        public override void SetDefaults()
        {
            npc.aiStyle = aiType = -1;
            npc.damage = 70;
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
            npc.HitSound = SoundID.NPCHit1;
            npc.DeathSound = SoundID.NPCDeath1;
            npc.buffImmune[BuffID.OnFire] = true;
        }

        public override void SendExtraAI(BinaryWriter writer) => writer.Write(npc.lifeMax);

        public override void ReceiveExtraAI(BinaryReader reader) => npc.lifeMax = reader.ReadInt32();

        public override void AI()
        {
            if (!Main.npc.IndexInRange(OwnerIndex) || !Main.npc[OwnerIndex].active)
            {
                npc.active = false;
                return;
            }

            NPC slimeGod = Main.npc[OwnerIndex];
            if (slimeGod.Infernum().ExtraAI[1] == 2f)
                npc.active = false;

            if (!npc.WithinRange(slimeGod.Center, Main.rand.NextFloat(380f, 520f)) || slimeGod.Infernum().ExtraAI[1] == 1f)
                RedirectCountdown = 60f;

            if (RedirectCountdown > 0f && !npc.WithinRange(slimeGod.Center, 50f))
            {
                float flySpeed = BossRushEvent.BossRushActive ? 38f : 14f;
                flySpeed = MathHelper.Max(flySpeed, slimeGod.velocity.Length() * 0.7f);

                Vector2 destinationOffset = (MathHelper.TwoPi * npc.whoAmI / 13f).ToRotationVector2() * 32f;
                npc.velocity = (npc.velocity * 34f + npc.SafeDirectionTo(slimeGod.Center + destinationOffset) * flySpeed) / 35f;
                if (!npc.WithinRange(slimeGod.Center, 175f))
                    npc.Center = Vector2.Lerp(npc.Center, slimeGod.Center, 0.05f);

                RedirectCountdown--;
            }

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
            if (!Main.npc.IndexInRange(OwnerIndex) || !Main.npc[OwnerIndex].active)
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
