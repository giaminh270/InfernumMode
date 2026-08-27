using CalamityMod;
using CalamityMod.NPCs;
using CalamityMod.NPCs.Providence;
using CalamityMod.Particles;
using InfernumMode.OverridingSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using static InfernumMode.BehaviorOverrides.BossAIs.Providence.ProvidenceBehaviorOverride;
using InfernumMode.Particles;
using System;

namespace InfernumMode.BehaviorOverrides.BossAIs.Providence
{
    public class ProvidenceHealerGuardianBehaviorOverride : NPCBehaviorOverride
    {
        public override int NPCOverrideType => ModContent.NPCType<ProvSpawnHealer>();
        public override NPCOverrideContext ContentToOverride => NPCOverrideContext.NPCAI | NPCOverrideContext.NPCPreDraw;

        #region AI

        public override bool PreAI(NPC npc)
        {
            // Disappear if Providence is not present.
            if (!Main.npc.IndexInRange(CalamityGlobalNPC.holyBoss))
            {
                npc.active = false;
                return false;
            }

            NPC providence = Main.npc[CalamityGlobalNPC.holyBoss];

            npc.target = providence.target;
            Player target = Main.player[npc.target];
            float proviSpinAngularOffset = providence.Infernum().ExtraAI[1];
            float proviRadiusOffset = providence.Infernum().ExtraAI[2];
            HealerGuardianAttackState attackState = (HealerGuardianAttackState)providence.Infernum().ExtraAI[0];
            ref float offsetRadius = ref npc.ai[0];
            ref float offsetAngle = ref npc.ai[1];
            ref float telegraphDirection = ref npc.ai[2];
            ref float previousAngularOffset = ref npc.ai[3];

            // Stick to Providence and look towards her target.
            npc.Center = providence.Bottom - Vector2.UnitY.RotatedBy(offsetAngle) * offsetRadius + Vector2.UnitY * providence.Infernum().ExtraAI[2];

            // Become more transparent the closer the guardian is to Providence.
            npc.Opacity = Utils.InverseLerp(0f, 180f, offsetRadius, true);

            // Disable contact damage.
            npc.damage = 0;

            // Disable HP bar effects since these things die quickly.
            npc.Calamity().ShouldCloseHPBar = true;

            // Stick to Providence and look towards her target.
            npc.Center = providence.Center + (offsetAngle + proviSpinAngularOffset).ToRotationVector2() * (offsetRadius + proviRadiusOffset);
            npc.velocity = Vector2.Zero;
            if (attackState != HealerGuardianAttackState.ShootCrystals)
                npc.spriteDirection = (target.Center.X > npc.Center.X).ToDirectionInt();

            // Release some small lava particles if there's some rotational velocity. This only happens during the idle spin state.
            if (offsetAngle + previousAngularOffset != previousAngularOffset && attackState == HealerGuardianAttackState.SpinInPlace)
            {
                Vector2 particleSpawnOffset = Main.rand.NextVector2Circular(npc.width * 0.5f, npc.height * 0.5f) * (float)Math.Pow(Main.rand.NextFloat(), 2f);
                float particleScale = Main.rand.NextFloat(20f, 35f);
                InfernumFusableParticleManager.GetParticleSetByType<ProfanedLavaParticleSet>()?.SpawnParticle(npc.Center + particleSpawnOffset, particleScale);

                previousAngularOffset = offsetAngle + proviSpinAngularOffset;
            }

            // Secretly aim the telegraph at the target during the spin phase.
            // Doing this ensures that it starts off on top of the target when the telegraphs begin and not in some random starting direction that
            // the guardian needs time to adjust from.
            float idealTelegraphDirection = npc.AngleTo(target.Center);
            if (attackState == HealerGuardianAttackState.SpinInPlace)
                telegraphDirection = idealTelegraphDirection;

            // Aim telegraphs towards the player with limited accuracy if that is what Providence is instructing the guardians to do.
            if (attackState == HealerGuardianAttackState.WaitAndReleaseTelegraph)
                telegraphDirection = telegraphDirection.AngleLerp(idealTelegraphDirection, 0.04f).AngleTowards(idealTelegraphDirection, 0.036f);

            return false;
        }

        public static Vector2 GetCrystalPosition(NPC npc) =>
            npc.Center + new Vector2(npc.spriteDirection, 0.35f).RotatedBy(npc.rotation) * 36f;

        #endregion

        #region Draw Effects

        public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Color lightColor)
        {
            // Don't do anything if Providence is not present, to prevent index errors.
            if (!Main.npc.IndexInRange(CalamityGlobalNPC.holyBoss))
                return false;

            NPC providence = Main.npc[CalamityGlobalNPC.holyBoss];
            float telegraphInterpolant = providence.Infernum().ExtraAI[3];
            float telegraphDirection = npc.ai[2];
            DrawBaseTexture(npc, lightColor, telegraphInterpolant);
            DrawTelegraphEffects(npc, telegraphInterpolant, telegraphDirection);
            return false;
        }

        public static void DrawBaseTexture(NPC npc, Color lightColor, float telegraphInterpolant)
        {
            // Calculate the appropriate direction.
            SpriteEffects direction = SpriteEffects.None;
            if (npc.spriteDirection == 1)
                direction = SpriteEffects.FlipHorizontally;

            // Draw the base texture and backglow.
            Texture2D texture = Main.npcTexture[npc.type];
            Texture2D fireGlowmask = ModContent.GetTexture("CalamityMod/NPCs/ProfanedGuardians/ProfanedGuardianBoss3Glow");
            Texture2D crystalGlowmask = ModContent.GetTexture("CalamityMod/NPCs/ProfanedGuardians/ProfanedGuardianBoss3Glow2");
            if (IsEnraged)
            {
                texture = ModContent.GetTexture("InfernumMode/BehaviorOverrides/BossAIs/Providence/ProvidenceHealerGuardianNight");
                fireGlowmask = ModContent.GetTexture("InfernumMode/BehaviorOverrides/BossAIs/Providence/ProvidenceHealerGuardianNightGlowFire");
                crystalGlowmask = ModContent.GetTexture("InfernumMode/BehaviorOverrides/BossAIs/Providence/ProvidenceHealerGuardianNightGlowCrystal");
            }

            Vector2 drawPosition = npc.Center - Main.screenPosition;
            npc.DrawBackglow(Color.Pink * telegraphInterpolant, 2f * telegraphInterpolant, direction, npc.frame, Main.screenPosition);
            Main.spriteBatch.Draw(texture, drawPosition, npc.frame, npc.GetAlpha(lightColor), npc.rotation, npc.frame.Size() * 0.5f, npc.scale, direction, 0f);

            // Draw the fire and crystal glowmasks.
            Color fireColor = Color.Lerp(Color.White, Color.Yellow, IsEnraged ? 0.05f : 0.5f);
            Color crystalColor = Color.Lerp(Color.White, Color.Violet, 0.5f);
            Main.spriteBatch.Draw(fireGlowmask, drawPosition, npc.frame, fireColor, npc.rotation, npc.frame.Size() * 0.5f, npc.scale, direction, 0f);
            Main.spriteBatch.Draw(crystalGlowmask, drawPosition, npc.frame, crystalColor, npc.rotation, npc.frame.Size() * 0.5f, npc.scale, direction, 0f);
        }

        public static void DrawTelegraphEffects(NPC npc, float telegraphInterpolant, float telegraphDirection)
        {
            // Draw the bloom light line telegraph.
            BloomLineDrawInfo lineInfo = new BloomLineDrawInfo()
            {
                LineRotation = -telegraphDirection,
                WidthFactor = 0.002f + (float)Math.Pow(telegraphInterpolant, 4f) * ((float)Math.Sin(Main.GlobalTime * 3f) * 0.001f + 0.001f),
                BloomIntensity = MathHelper.Lerp(0.3f, 0.4f, telegraphInterpolant),
                Scale = Vector2.One * telegraphInterpolant * 2500f,
                MainColor = Color.Lerp(Color.Yellow, Color.HotPink, telegraphInterpolant * 0.5f + 0.5f),
                DarkerColor = Color.Orange,
                Opacity = (float)Math.Sqrt(telegraphInterpolant),
                BloomOpacity = 0.3f,
                LightStrength = 5f
            };
            Vector2 crystalCenter = GetCrystalPosition(npc) - Main.screenPosition;
            Utilities.DrawBloomLineTelegraph(crystalCenter, lineInfo);
        }
        #endregion Draw Effects
    }
}
