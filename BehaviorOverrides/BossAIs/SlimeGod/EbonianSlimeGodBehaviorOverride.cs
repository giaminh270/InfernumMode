using CalamityMod.NPCs;
using CalamityMod.NPCs.SlimeGod;
using InfernumMode.OverridingSystem;
using Microsoft.Xna.Framework;
using System;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

using EbonianSlimeGod = CalamityMod.NPCs.SlimeGod.SlimeGod;

namespace InfernumMode.BehaviorOverrides.BossAIs.SlimeGod
{
    public class EbonianSlimeGodBehaviorOverride : NPCBehaviorOverride
    {
        public override int NPCOverrideType => ModContent.NPCType<EbonianSlimeGod>();

        public override NPCOverrideContext ContentToOverride => NPCOverrideContext.NPCAI;

        public override float[] PhaseLifeRatioThresholds => new float[]
        {
            SlimeGodComboAttackManager.SummonSecondSlimeLifeRatio
        };

        #region Enumerations
        public enum EbonianSlimeGodAttackType
        {
            LongLeaps,
            SplitSwarm,
            PowerfulSlam
        }
        #endregion

        #region AI

        public override bool PreAI(NPC npc)
        {
            // Disappear if the core is not present.
            if (!Main.npc.IndexInRange(CalamityGlobalNPC.slimeGod))
            {
                npc.active = false;
                return false;
            }

            // Do targeting.
            npc.target = Main.npc[CalamityGlobalNPC.slimeGod].target;
            Player target = Main.player[npc.target];

            if (target.dead || !target.active)
            {
                npc.active = false;
                return false;
            }

            // This will affect the other gods as well in terms of behavior.
            ref float attackState = ref npc.ai[0];
            ref float attackTimer = ref npc.ai[1];

            // Reset things.
            npc.timeLeft = 3600;
            npc.Opacity = 1f;
            npc.damage = npc.defDamage;
            npc.noGravity = false;
            npc.noTileCollide = false;

            // Set the universal whoAmI variable.
            CalamityGlobalNPC.slimeGodPurple = npc.whoAmI;

            // Summon the second slime.
            if (Main.netMode != NetmodeID.MultiplayerClient && npc.Infernum().ExtraAI[5] == 0f && npc.life < npc.lifeMax * SlimeGodComboAttackManager.SummonSecondSlimeLifeRatio)
            {
                int secondSlime = NPC.NewNPC((int)target.Center.X, (int)target.Center.Y - 750, ModContent.NPCType<SlimeGodRun>(), npc.whoAmI);
                if (Main.npc.IndexInRange(secondSlime))
                {
                    Main.npc[secondSlime].Infernum().ExtraAI[5] = 1f;
                    Main.npc[secondSlime].netUpdate = true;
                }

                npc.Infernum().ExtraAI[5] = 1f;
                npc.netUpdate = true;
            }

            // Inherit attributes from the leader.
            SlimeGodComboAttackManager.InheritAttributesFromLeader(npc);
            SlimeGodComboAttackManager.DoAttacks(npc, target, ref attackTimer);

            if (npc.Opacity <= 0f)
            {
                npc.scale = 0.001f;
                npc.dontTakeDamage = true;
            }
            else
                npc.dontTakeDamage = false;

            while (Collision.SolidCollision(npc.BottomLeft - Vector2.UnitY * 32f, npc.width, 32) && !npc.noTileCollide)
                npc.position.Y -= 4f;

            return false;
        }
        #endregion AI
    }
}
