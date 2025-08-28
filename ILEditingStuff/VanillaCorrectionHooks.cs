using CalamityMod;
using CalamityMod.Balancing;
using CalamityMod.NPCs.AstrumAureus;
using CalamityMod.NPCs.DesertScourge;
using CalamityMod.World;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using static InfernumMode.ILEditingStuff.HookManager;
using InfernumBalancingManager = InfernumMode.Balancing.BalancingChangesManager;

namespace InfernumMode.ILEditingStuff
{
    public class ReplaceGoresHook : IHookEdit
    {
        internal static int AlterGores(On.Terraria.Gore.orig_NewGore orig, Vector2 Position, Vector2 Velocity, int Type, float Scale)
        {
            if (InfernumMode.CanUseCustomAIs && Type >= GoreID.Cultist1 && Type <= GoreID.CultistBoss2)
                return Main.maxDust;

            if (InfernumMode.CanUseCustomAIs && Type == 573)
                Type = InfernumMode.Instance.GetGoreSlot("Gores/DukeFishronGore1");
            if (InfernumMode.CanUseCustomAIs && Type == 574)
                Type = InfernumMode.Instance.GetGoreSlot("Gores/DukeFishronGore3");
            if (InfernumMode.CanUseCustomAIs && Type == 575)
                Type = InfernumMode.Instance.GetGoreSlot("Gores/DukeFishronGore2");
            if (InfernumMode.CanUseCustomAIs && Type == 576)
                Type = InfernumMode.Instance.GetGoreSlot("Gores/DukeFishronGore4");

            return orig(Position, Velocity, Type, Scale);
        }

        public void Load() => On.Terraria.Gore.NewGore += AlterGores;

        public void Unload() => On.Terraria.Gore.NewGore -= AlterGores;
    }

    public class AureusPlatformWalkingHook : IHookEdit
    {
        internal static bool LetAureusWalkOnPlatforms(On.Terraria.NPC.orig_Collision_DecideFallThroughPlatforms orig, NPC npc)
        {
            if (npc.type == ModContent.NPCType<AstrumAureus>())
            {
                if (Main.player[npc.target].position.Y > npc.Bottom.Y)
                    return true;
                return false;
            }
            return orig(npc);
        }

        public void Load() => On.Terraria.NPC.Collision_DecideFallThroughPlatforms += LetAureusWalkOnPlatforms;

        public void Unload() => On.Terraria.NPC.Collision_DecideFallThroughPlatforms -= LetAureusWalkOnPlatforms;
    }

    public class FishronSkyDistanceLeniancyHook : IHookEdit
    {
        internal static void AdjustFishronScreenDistanceRequirement(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);
            cursor.GotoNext(i => i.MatchLdcR4(3000f));
            cursor.Remove();
            cursor.Emit(OpCodes.Ldc_R4, 6000f);
        }

        public void Load() => IL.Terraria.GameContent.Events.ScreenDarkness.Update += AdjustFishronScreenDistanceRequirement;

        public void Unload() => IL.Terraria.GameContent.Events.ScreenDarkness.Update -= AdjustFishronScreenDistanceRequirement;
    }

    /*public class UseCustomShineParticlesForInfernumParticlesHook : IHookEdit
    {
        internal static void EmitFireParticles(On.Terraria.GameContent.Drawing.TileDrawing.orig_DrawTiles_EmitParticles orig, TileDrawing self, int j, int i, Tile tileCache, ushort typeCache, short tileFrameX, short tileFrameY, Color tileLight)
        {
            ModTile mt = TileLoader.GetTile(tileCache.TileType);
            if ((tileLight.R > 20 || tileLight.B > 20 || tileLight.G > 20) && Main.rand.NextBool(12) && mt != null)
            {
                Dust fire = Dust.NewDustDirect(new Vector2(i * 16, j * 16), 16, 16, Main.rand.NextBool() ? 267 : 6, 0f, 0f, 254, Color.White, 1.4f);
                fire.velocity = -Vector2.UnitY.RotatedByRandom(0.5f);
                fire.color = Color.Lerp(Color.Yellow, Color.Red, Main.rand.NextFloat(0.7f));
                fire.noGravity = true;
            }

            // I LOVE RANDOM ERRORS IN VANILLA METHODS THAT DISRUPT MY GODDAMN DEBUGGING ENVIRONMENT.
            // It's so FUN!
            try
            {
                orig(self, i, j, tileCache, typeCache, tileFrameX, tileFrameY, tileLight);
            }
            catch (IndexOutOfRangeException) { }
        }

        public void Load() => On.Terraria.GameContent.Drawing.TileDrawing.DrawTiles_EmitParticles += EmitFireParticles;

        public void Unload() => On.Terraria.GameContent.Drawing.TileDrawing.DrawTiles_EmitParticles -= EmitFireParticles;
    }*/

    public class LessenDesertTileRequirementsHook : IHookEdit
    {
        internal static void MakeDesertRequirementsMoreLenient(On.Terraria.Player.orig_UpdateBiomes orig, Player self)
        {
            orig(self);
            self.ZoneDesert = Main.sandTiles > 300;
        }

        public void Load() => On.Terraria.Player.UpdateBiomes += MakeDesertRequirementsMoreLenient;

        public void Unload() => On.Terraria.Player.UpdateBiomes -= MakeDesertRequirementsMoreLenient;
    }

    public class SepulcherOnHitProjectileEffectRemovalHook : IHookEdit
    {
        internal static void EarlyReturn(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);
            cursor.Emit(OpCodes.Ret);
        }

        public void Load()
        {
            SepulcherHeadModifyProjectile += EarlyReturn;
            SepulcherBodyModifyProjectile += EarlyReturn;
            SepulcherTailModifyProjectile += EarlyReturn;
        }

        public void Unload()
        {
            SepulcherHeadModifyProjectile -= EarlyReturn;
            SepulcherBodyModifyProjectile -= EarlyReturn;
            SepulcherTailModifyProjectile -= EarlyReturn;
        }
    }

    public class GetRidOfDesertNuisancesHook : IHookEdit
    {
        internal static void GetRidOfDesertNuisances(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);
            cursor.Emit(OpCodes.Ldarg_1);
            cursor.EmitDelegate<Action<Player>>(player =>
            {
                Main.PlaySound(SoundID.Roar, player.position, 0);
                if (Main.netMode != NetmodeID.MultiplayerClient)
                    NPC.SpawnOnPlayer(player.whoAmI, ModContent.NPCType<DesertScourgeHead>());
                else
                    NetMessage.SendData(MessageID.SpawnBoss, -1, -1, null, player.whoAmI, ModContent.NPCType<DesertScourgeHead>());
            });
            cursor.Emit(OpCodes.Ldc_I4_1);
            cursor.Emit(OpCodes.Ret);
        }

        public void Load() => DesertScourgeItemUseItem += GetRidOfDesertNuisances;

        public void Unload() => DesertScourgeItemUseItem -= GetRidOfDesertNuisances;
    }

    public class LetAresHitPlayersHook : IHookEdit
    {
        internal static void LetAresHitPlayer(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);
            cursor.Emit(OpCodes.Ldc_I4_1);
            cursor.Emit(OpCodes.Ret);
        }

        public void Load() => AresBodyCanHitPlayer += LetAresHitPlayer;

        public void Unload() => AresBodyCanHitPlayer -= LetAresHitPlayer;
    }
	
}