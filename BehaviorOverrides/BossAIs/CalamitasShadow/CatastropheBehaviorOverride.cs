using CalamityMod.NPCs.Calamitas;
using CalamityMod;
using InfernumMode.OverridingSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

using CalCloneNPC = CalamityMod.NPCs.Calamitas.CalamitasRun3;

namespace InfernumMode.BehaviorOverrides.BossAIs.CalamitasShadow
{
    public class CatastropheBehaviorOverride : NPCBehaviorOverride
    {

        public override int NPCOverrideType => ModContent.NPCType<CalamitasRun2>();
		
		public override NPCOverrideContext ContentToOverride => NPCOverrideContext.NPCAI | NPCOverrideContext.NPCFindFrame | NPCOverrideContext.NPCPreDraw;

        public override bool PreAI(NPC npc)
        {
			npc.GivenName = CalamitasShadowBehaviorOverride.CustomNameCatastrophe;
            CataclysmBehaviorOverride.DoAI(npc);
            return false;
        }

        #region Frames and Drawcode
        public override void FindFrame(NPC npc, int frameHeight)
        {
            ref float currentFrame = ref npc.localAI[1];
            npc.frameCounter += 0.15;
            if (npc.frameCounter >= 1D)
            {
                currentFrame = (currentFrame + 1f) % 6f;
                npc.frameCounter = 0D;
            }

            npc.frame.Width = 196;
            npc.frame.Height = 198;
            npc.frame.X = 0;
            npc.frame.Y = (int)currentFrame * npc.frame.Height;
        }

        public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Color lightColor) => CataclysmBehaviorOverride.DrawBrother(npc, spriteBatch, lightColor);
        #endregion Frames and Drawcode
    }
}