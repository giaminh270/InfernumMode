using CalamityMod;
using CalamityMod.CalPlayer;
using CalamityMod.Items.SummonItems;
using CalamityMod.Items.TreasureBags;
using CalamityMod.NPCs.DevourerofGods;
using InfernumMode.Balancing;
using InfernumMode.BehaviorOverrides.BossAIs.DoG;
using InfernumMode.Items;
using InfernumMode.Projectiles;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode
{
    public class PoDItems : GlobalItem
    {
        public override void SetDefaults(Item item)
        {
            if (item.type == ItemID.CelestialSigil)
            {
                item.consumable = false;
                item.maxStack = 1;
            }

            if (ItemDamageValues.DamageValues.TryGetValue(item.type, out int newDamage))
                item.damage = newDamage;
        }

        public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
        {
            if (item.type == ItemID.CelestialSigil)
            {
                var tooltip0 = tooltips.FirstOrDefault(x => x.Name == "Tooltip0" && x.mod == "Terraria");
                if (tooltip0 != null)
                {
                    tooltip0.text = 
                        "Summons the Moon Lord immediately\n" +
                        "Creates an arena at the player's position\n" +
                        "Not consumable.";
                }
            }

            if (InfernumMode.CanUseCustomAIs && item.type == ModContent.ItemType<ProfanedShard>())
            {
                var tooltip1 = tooltips.FirstOrDefault(x => x.Name == "Tooltip1" && x.mod == "Terraria");
                if (tooltip1 != null)
                {
                    tooltip1.text = "Summons the Profaned Guardians when used in the profaned garden at the far right of the underworld";

                    tooltips.RemoveAt(tooltips.IndexOf(tooltip1) + 1);
                    if (!PoDWorld.HasGeneratedProfanedShrine)
                    {
                        TooltipLine warningTooltip = new TooltipLine(mod, "Warning",
                            "Your world does not currently have a garden. kill the Moon Lord again to generate it\n" +
                            "Be sure to grab the Hell schematic first if you do this, as the garden might destroy the lab");
                        warningTooltip.overrideColor = Color.Orange;
                        tooltips.Insert(tooltips.IndexOf(tooltip1) + 1, warningTooltip);
                    }
                }
            }

            if (InfernumMode.CanUseCustomAIs && item.type == ModContent.ItemType<ProfanedCoreUnlimited>())
            {
                var tooltip1 = tooltips.FirstOrDefault(x => x.Name == "Tooltip1" && x.mod == "Terraria");
                if (tooltip1 != null)
                    tooltip1.text = "Summons Providence when used at the alter in the profaned temple at the far right of the underworld";
            }

            if (InfernumMode.CanUseCustomAIs && item.type == ItemID.LihzahrdPowerCell)
            {
                var tooltip0 = tooltips.FirstOrDefault(x => x.Name == "Tooltip0" && x.mod == "Terraria");
                if (tooltip0 != null)
                    tooltip0.text += "\nCreates a rectangular arena around the altar. If the altar is inside of the temple solid tiles within the arena are broken";
            }
        }

        internal static void DoGTeleportDenialText(Player player)
        {
            if (!player.chaosState)
            {
                player.AddBuff(BuffID.ChaosState, CalamityPlayer.chaosStateDurationBoss, true);
                Projectile.NewProjectile(Main.MouseWorld, Vector2.Zero, ModContent.ProjectileType<RoDFailPulse>(), 0, 0f, player.whoAmI);

                string[] possibleEdgyShitToSay = new string[]
                {
                        "YOU CANNOT EVADE ME SO EASILY!",
                        "YOU CANNOT HOPE TO OUTSMART A MASTER OF DIMENSIONS!",
                        "NOT SO FAST!"
                };
                Utilities.DisplayText(Main.rand.Next(possibleEdgyShitToSay), Color.Cyan);
            }
        }

        public override bool CanUseItem(Item item, Player player)
        {
            if (InfernumMode.CanUseCustomAIs && item.type == ItemID.RodofDiscord && NPC.AnyNPCs(ModContent.NPCType<DevourerofGodsHead>()))
            {
                if (PoDWorld.InfernumMode)
                {
                    DoGTeleportDenialText(player);
                    return false;
                }
            }
            if (InfernumMode.CanUseCustomAIs && (item.type == ModContent.ItemType<ProfanedShard>() || item.type == ModContent.ItemType<ProfanedCoreUnlimited>()))
                return false;
            return base.CanUseItem(item, player);
        }
        public override bool UseItem(Item item, Player player)
        {
            if (item.type == ItemID.CelestialSigil && !NPC.AnyNPCs(NPCID.MoonLordCore))
            {
                NPC.NewNPC((int)player.Center.X, (int)player.Center.Y, NPCID.MoonLordCore);
            }
            return base.UseItem(item, player);
        }

        public override void RightClick(Item item, Player player)
        {
            if (item.type == ModContent.ItemType<StarterBag>())
                DropHelper.DropItemCondition(player, ModContent.ItemType<Death2>(), Main.expertMode);
        }

        public override void OpenVanillaBag(string context, Player player, int arg)
        {
            // Only apply bag drop contents in Infernum Mode and on boss bags.
            if (context != "bossBag" || !InfernumMode.CanUseCustomAIs)
                return;

            if (arg == ItemID.EaterOfWorldsBossBag)
            {
                int itemCount = Main.rand.Next(30, 60);
                player.QuickSpawnItem(ItemID.DemoniteOre, itemCount);
                itemCount = Main.rand.Next(10, 20);
                player.QuickSpawnItem(ItemID.ShadowScale, itemCount);
            }
            if (arg == ItemID.BrainOfCthulhuBossBag)
            {
                int itemCount = Main.rand.Next(30, 60);
                player.QuickSpawnItem(ItemID.CrimtaneOre, itemCount);
                itemCount = Main.rand.Next(10, 20);
                player.QuickSpawnItem(ItemID.TissueSample, itemCount);
            }
        }
    }
}
