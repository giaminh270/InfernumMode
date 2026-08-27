using CalamityMod.NPCs.HiveMind;
using InfernumMode.OverridingSystem;
using Terraria;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.HiveMind
{
    public class HiveBlobBehaviorOverride : NPCBehaviorOverride
    {
        public override int NPCOverrideType => ModContent.NPCType<HiveBlob2>();

        public override NPCOverrideContext ContentToOverride => NPCOverrideContext.NPCAI;

        public override bool PreAI(NPC npc)
        {
            // How about no?
            npc.active = false;
            return false;
        }
    }
}
