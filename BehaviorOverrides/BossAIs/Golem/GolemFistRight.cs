using CalamityMod;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.Golem
{
    public class GolemFistRight : ModNPC
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Golem Fist");
        }

        public override void SetDefaults()
        {
            npc.lifeMax = 1;
            npc.defDamage = npc.damage = 125;
            npc.dontTakeDamage = true;
            npc.width = 40;
            npc.height = 40;
            npc.lavaImmune = true;
            npc.noGravity = true;
            npc.noTileCollide = true;
        }

        public override bool PreAI() => GolemFistLeft.DoFistAI(npc, false);

        public override bool PreDraw(SpriteBatch spriteBatch, Color drawColor) => GolemFistLeft.DrawFist(npc, Main.spriteBatch, drawColor, false);

        public override bool CanHitPlayer(Player target, ref int cooldownSlot)
        {
            cooldownSlot = 1;
            return base.CanHitPlayer(target, ref cooldownSlot);
        }
    }
}
