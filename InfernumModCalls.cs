using CalamityMod.NPCs.ExoMechs;
using InfernumMode.BehaviorOverrides.BossAIs.MoonLord;
using InfernumMode;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode
{
    public class InfernumModCalls
    {
        public static object Call(params object[] args)
        {
            if (args is null || args.Length <= 0)
                return new ArgumentNullException("ERROR: No function name specified. First argument must be a function name.");
            if (!(args[0] is string))
                return new ArgumentException("ERROR: First argument must be a string function name.");

            string methodName = args[0].ToString();
            switch (methodName)
            {
                case "GetInfernumActive":
                    return PoDWorld.InfernumMode;
                case "SetInfernumActive":
                    PoDWorld.InfernumMode = (bool)args[1];
                    break;
				case "CanPlayMusicForNPC":
                    int npcID = (int)args[1];
                    return CanPlayMusicForNPC(npcID);
            }
            return null;
        }
		
		public static bool CanPlayMusicForNPC(int npcID)
        {
            if (npcID == NPCID.EyeofCthulhu)
                return NPC.AnyNPCs(npcID);
            if (npcID == NPCID.SkeletronHead)
                return NPC.AnyNPCs(npcID);
            if (npcID == NPCID.Retinazer || npcID == NPCID.Spazmatism || npcID == NPCID.SkeletronPrime || npcID == NPCID.TheDestroyer)
                return NPC.AnyNPCs(npcID);
            if (npcID == NPCID.Plantera)
                return NPC.AnyNPCs(npcID);
            if (npcID == NPCID.DukeFishron)
                return NPC.AnyNPCs(npcID);
            if (npcID == NPCID.CultistBoss)
                return NPC.AnyNPCs(npcID);
            if (npcID == NPCID.MoonLordCore)
            {
                int npcIndex = NPC.FindFirstNPC(npcID);
                if (npcIndex < 0)
                    return false;
                return Main.npc[npcIndex].Infernum().ExtraAI[10] >= MoonLordCoreBehaviorOverride.IntroSoundLength;
            }
            if (npcID == ModContent.NPCType<Draedon>())
                return NPC.AnyNPCs(npcID) || InfernumMode.DraedonThemeTimer > 0;

            return false;
        }
    }
}
