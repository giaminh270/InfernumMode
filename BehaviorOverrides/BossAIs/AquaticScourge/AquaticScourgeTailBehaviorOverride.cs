using CalamityMod.NPCs.AquaticScourge;
using InfernumMode.OverridingSystem;
using Terraria;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.AquaticScourge
{
    public class AquaticScourgeTailBehaviorOverride : NPCBehaviorOverride
    {
        public override int NPCOverrideType => ModContent.NPCType<AquaticScourgeTail>();

        public override NPCOverrideContext ContentToOverride => NPCOverrideContext.NPCAI;

        public override bool PreAI(NPC npc)
        {
            AquaticScourgeBodyBehaviorOverride.DoAI(npc);
            return false;
        }
    }
}
