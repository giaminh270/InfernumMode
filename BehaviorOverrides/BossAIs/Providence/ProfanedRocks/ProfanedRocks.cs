using CalamityMod;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Dusts;
using Microsoft.Xna.Framework;
using System;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.Providence
{
    // Calamity 1.3 does not contain that NPC, so Infernum must provide the
    public class ProfanedRocks : ModNPC
    {
        public const int MaxHP = 8000;
        public const int MaxBossRushHP = 20000;

        public override string Texture => "InfernumMode/BehaviorOverrides/BossAIs/Providence/ProfanedRocks/ProfanedRocks1";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Profaned Rocks");
            NPCID.Sets.TrailingMode[npc.type] = 1;
            NPCID.Sets.TrailCacheLength[npc.type] = 6;
        }

        public override void SetDefaults()
        {
            npc.damage = 0;
            npc.aiStyle = -1;
            aiType = -1;

            npc.width = 50;
            npc.height = 50;
            npc.defense = 100;
            npc.lifeMax = 8000;
            npc.life = npc.lifeMax;
            npc.knockBackResist = 0f;

            npc.dontTakeDamage = true;
            npc.noGravity = true;
            npc.noTileCollide = true;
            npc.chaseable = false;
            npc.canGhostHeal = false;
            npc.Opacity = 0f;
            npc.timeLeft = 1800;
            npc.netAlways = true;
            npc.dontCountMe = true;

            npc.HitSound = SoundID.NPCHit52;
            npc.DeathSound = SoundID.NPCDeath55;

            npc.Calamity().canBreakPlayerDefense = true;
            npc.Calamity().VulnerableToHeat = false;
            npc.Calamity().VulnerableToCold = true;
            npc.Calamity().VulnerableToSickness = false;
            npc.Calamity().VulnerableToWater = true;
        }


        public override void ScaleExpertStats(int numPlayers, float bossLifeScale)
        {
            npc.lifeMax = (int)(npc.lifeMax * 0.5f * bossLifeScale);
            npc.life = npc.lifeMax;
        }

        public override bool CheckActive()
        {
            return false;
        }

        public override void OnHitPlayer(Player player, int damage, bool crit)
        {
            if (damage > 0)
                player.AddBuff(ModContent.BuffType<HolyFlames>(), 120, true);
        }

        public override bool CanHitPlayer(Player target, ref int cooldownSlot)
        {
            Rectangle targetHitbox = target.Hitbox;
            float minDist = Vector2.Distance(npc.Center, targetHitbox.TopLeft());
            minDist = Math.Min(minDist, Vector2.Distance(npc.Center, targetHitbox.TopRight()));
            minDist = Math.Min(minDist, Vector2.Distance(npc.Center, targetHitbox.BottomLeft()));
            minDist = Math.Min(minDist, Vector2.Distance(npc.Center, targetHitbox.BottomRight()));

            return minDist <= (npc.ai[2] == 6f ? 16f : 22f);
        }

        public override bool CheckDead()
        {
            return false;
        }

        public override void HitEffect(int hitDirection, double damage)
        {
            int dustCount = InfernumConfig.Instance.ReducedGraphicsConfig ? 1 : 3;
            for (int k = 0; k < dustCount; k++)
            {
                Dust.NewDust(npc.position, npc.width, npc.height, (int)CalamityDusts.ProfanedFire,
                    hitDirection, -1f, 0, default(Color), 1f);
            }

            if (npc.life <= 0)
            {
                int deathDustCount = InfernumConfig.Instance.ReducedGraphicsConfig ? 8 : 30;
                for (int k = 0; k < deathDustCount; k++)
                {
                    Dust.NewDust(npc.position, npc.width, npc.height, (int)CalamityDusts.ProfanedFire,
                        hitDirection, -1f, 0, default(Color), 1f);
                }
            }
        }
    }
}
