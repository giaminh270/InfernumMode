using CalamityMod;
using CalamityMod.Projectiles;
using InfernumMode.ExtraTextures;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.Twins
{
    public class TwinsShield : ModProjectile
    {
        public NPC Owner => Main.npc[(int)OwnerIndex];

        public ref float OwnerIndex => ref projectile.ai[0];

        public ref float Radius => ref projectile.ai[1];

        public const float MaxRadius = 180f;

        public const int HealTime = 180;

        public const int Lifetime = HealTime + HealTime / 3;

        public override string Texture => "InfernumMode/ExtraTextures/GreyscaleObjects/Gleam";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Shield");
        }

        public override void SetDefaults()
        {
            projectile.width = 72;
            projectile.height = 72;
            projectile.penetrate = -1;
            projectile.tileCollide = false;
            projectile.hostile = true;
            projectile.timeLeft = Lifetime;
            projectile.scale = 0.001f;
            cooldownSlot = 1;
        }

        public override void AI()
        {
            if (!Main.npc.IndexInRange((int)OwnerIndex) || !Main.npc[(int)OwnerIndex].active)
            {
                projectile.Kill();
                return;
            }

            projectile.Center = Owner.Center;

            Radius = (float)Math.Sin(projectile.timeLeft / (float)Lifetime * MathHelper.Pi) * MaxRadius * 4f;
            if (Radius > MaxRadius)
                Radius = MaxRadius;
            projectile.scale = 2f;

            bool alone = !NPC.AnyNPCs(NPCID.Spazmatism) && Owner.type == NPCID.Retinazer || !NPC.AnyNPCs(NPCID.Retinazer) && Owner.type == NPCID.Spazmatism;
            if (!alone && projectile.timeLeft < HealTime)
                projectile.timeLeft = HealTime;

            CalamityGlobalProjectile.ExpandHitboxBy(projectile, (int)(Radius * projectile.scale), (int)(Radius * projectile.scale));
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            if (!Main.npc.IndexInRange((int)OwnerIndex) || !Main.npc[(int)OwnerIndex].active)
                return false;
            Cultist.CultistBehaviorOverride.DrawForcefield(projectile.Center - Main.screenPosition, projectile.Opacity, Owner.type == NPCID.Spazmatism ? Color.LimeGreen : Color.Red, InfernumTextureRegistry.HexagonGrid, false, 1.3f * (Radius / MaxRadius), fresnelScaleFactor: 1.3f, noiseScaleFactor: 1.05f);
            return false;
        }
    }
}
