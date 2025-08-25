using CalamityMod;
using CalamityMod.NPCs;
using InfernumMode.InverseKinematics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.Polterghast
{
    public class PolterghastLeg : ModNPC
    {
        public Vector2 IdealPosition;

        public LimbCollection Limbs;

        public PrimitiveTrailCopy LimbDrawer = null;

        public Player Target => Main.player[npc.target];

        public int Direction => (npc.ai[0] >= 2f).ToDirectionInt();

        public ref float IdealPositionTimer => ref npc.ai[1];
		
		public ref float FadeToRed => ref npc.localAI[1];

        public static PolterghastBehaviorOverride.PolterghastAttackType CurrentAttack => (PolterghastBehaviorOverride.PolterghastAttackType)(int)Polterghast.ai[0];

        public static float AttackTimer => Polterghast.ai[1];

        public static bool Enraged => Polterghast.ai[3] == 1f;

        public static NPC Polterghast => Main.npc[CalamityGlobalNPC.ghostBoss];

        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public override void SetStaticDefaults() => DisplayName.SetDefault("Ghostly Leg");

        public override void SetDefaults()
        {
            npc.npcSlots = 1f;
            npc.aiStyle = aiType = -1;
            npc.width = npc.height = 1800;
            npc.damage = 245;
            npc.lifeMax = 5000;
            npc.dontTakeDamage = true;
            npc.knockBackResist = 0f;
            npc.noGravity = true;
            npc.noTileCollide = true;
            npc.netAlways = true;
            npc.scale = 1.15f;
        }

        public override void SendExtraAI(BinaryWriter writer) => writer.WriteVector2(IdealPosition);

        public override void ReceiveExtraAI(BinaryReader reader) => IdealPosition = reader.ReadVector2();

        public override void AI()
        {
            if (!Main.npc.IndexInRange(CalamityGlobalNPC.ghostBoss) || !Main.npc[CalamityGlobalNPC.ghostBoss].active)
            {
                npc.active = false;
                npc.netUpdate = true;
                return;
            }

            if (npc.localAI[0] == 0f)
            {
                Limbs = new LimbCollection(new ModifiedCyclicCoordinateDescentUpdateRule(0.15f, MathHelper.PiOver2), 160f, 160f, 160f);
                DecideNewPositionToStickTo();
                npc.localAI[0] = 1f;
            }

            Limbs.Update(Polterghast.Center, npc.Center);

            bool isBeingControlled = Polterghast.Infernum().ExtraAI[PolterghastBehaviorOverride.LegToManuallyControlIndexIndex] == npc.whoAmI;
            float moveSpeed = 28f + Polterghast.velocity.Length() * 1.2f;

            // Reposition and move to the ideal position if not being manually controlled in Polter's AI.
            npc.damage = npc.defDamage;
            if (!isBeingControlled)
            {
                npc.damage = 0;

                if (!npc.WithinRange(IdealPosition, moveSpeed + 4f))
                    npc.velocity = npc.SafeDirectionTo(IdealPosition) * moveSpeed;
                else
                {
                    npc.Center = IdealPosition;
                    npc.velocity = Vector2.Zero;
                }

                if (CurrentAttack == PolterghastBehaviorOverride.PolterghastAttackType.CloneSplit && AttackTimer % 180f == 30f)
                    IdealPositionTimer = 40f;

                if (!npc.WithinRange(Polterghast.Center, 650f))
                    IdealPositionTimer++;
                float repositionRate = 30f - Polterghast.velocity.Length() - Utils.InverseLerp(350f, 600f, IdealPositionTimer, true) * 10f;
                repositionRate = MathHelper.Clamp(repositionRate, 5f, 35f);
                if (IdealPositionTimer >= repositionRate)
                {
                    DecideNewPositionToStickTo();
                    IdealPositionTimer = 0f;
                    npc.netUpdate = true;
                }
            }

            // Fade to red based on whether the leg is being controlled.
            FadeToRed = MathHelper.Clamp(FadeToRed + isBeingControlled.ToDirectionInt(), 0.3f, 1f);

            npc.target = Polterghast.target;
        }

        public void DecideNewPositionToStickTo()
        {
            for (int tries = 0; tries < 20000; tries++)
            {
                int checkArea = (int)(40f * (tries / 20000f)) + 25;
                Point limbTilePosition = (Polterghast.Center / 16f + (MathHelper.TwoPi * npc.ai[0] / 4f).ToRotationVector2().RotatedByRandom(0.69f) * Main.rand.NextFloat(checkArea)).ToPoint();

                if (!WorldGen.InWorld(limbTilePosition.X, limbTilePosition.Y, 4))
                    continue;

                // Only stick to a tile if it has at least 5 exposed tiles.
                int exposedTiles = 0;
                for (int dx = -1; dx <= 1; dx++)
                {
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        if (dx == 0 && dy == 0)
                            continue;

                        int tileType = Main.tile[limbTilePosition.X + dx, limbTilePosition.Y + dy].type;
                        if (!Main.tile[limbTilePosition.X + dx, limbTilePosition.Y + dy].active() || !Main.tileSolid[tileType])
                            exposedTiles++;
                    }
                }

                if (exposedTiles < 3)
                    continue;

                Vector2 endPosition = limbTilePosition.ToWorldCoordinates(8, 16);

                if (!Polterghast.WithinRange(endPosition, 720f))
                    continue;

                if (Math.Abs(MathHelper.WrapAngle(Polterghast.AngleTo(endPosition) - MathHelper.Pi * Direction)) > 0.5f)
                    continue;

                bool outOfGeneralDirection = Polterghast.SafeDirectionTo(endPosition).AngleBetween((MathHelper.TwoPi * npc.ai[0] / 4f).ToRotationVector2()) > 1.05f; 
                if (tries < 15000 && (!Collision.CanHitLine(Polterghast.Center, 16, 16, endPosition, 16, 16) || outOfGeneralDirection))
                    continue;

                bool farFromOtherLimbs = false;
                for (int j = 0; j < Main.maxNPCs; j++)
                {
                    if (Main.npc[j].type != npc.type || !Main.npc[j].active || j == npc.whoAmI)
                        continue;
                    if (!Main.npc[j].WithinRange(endPosition, 650f) || Main.npc[j].WithinRange(endPosition, 140f))
                    {
                        farFromOtherLimbs = true;
                        break;
                    }
                }

                if (farFromOtherLimbs)
                    continue;

                if (WorldGen.SolidTile(limbTilePosition.X, limbTilePosition.Y) || (tries >= 17500 && Main.tile[limbTilePosition.X, limbTilePosition.Y].wall > 0))
                {
                    IdealPosition = endPosition;
                    npc.netUpdate = true;
                    return;
                }
            }

            // If no position was found, go to a default position.
            IdealPosition = Polterghast.Center + (MathHelper.TwoPi * npc.ai[0] / 4f).ToRotationVector2() * 570f * Main.rand.NextFloat(0.7f, 1.3f);
            if (!Polterghast.WithinRange(IdealPosition, 400f))
                IdealPosition = Polterghast.Center + Polterghast.SafeDirectionTo(IdealPosition) * 400f;

            npc.netUpdate = true;
        }

        internal float PrimitiveWidthFunction(float completionRatio)
        {
            float fadeToMax = MathHelper.Lerp(0f, 1f, (float)Math.Sin(MathHelper.Pi * completionRatio) * (npc.localAI[2] == 1f ? 0.5f : 1f));
            float pulse = MathHelper.Lerp(-0.4f, 2f, (float)Math.Sin(Main.GlobalTime * 4.6f) * 0.5f + 0.5f);
            return MathHelper.Lerp(9.5f, 12f + pulse, fadeToMax) * MathHelper.Clamp(Polterghast.scale, 0.75f, 1.5f);
        }

        internal Color PrimitiveColorFunction(float completionRatio)
        {
            Color baseColor = Color.Lerp(Color.Cyan, Color.HotPink, FadeToRed) * Utils.InverseLerp(54f, 45f, Polterghast.ai[2], true);
            return baseColor * Utils.InverseLerp(0.02f, 0.1f, completionRatio, true) * Utils.InverseLerp(0.98f, 0.9f, completionRatio, true);
        }

        public override bool CanHitPlayer(Player target, ref int cooldownSlot)
        {
            if (Limbs is null)
                return false;

            if (Limbs.Limbs is null)
                return false;

            for (int i = 0; i < Limbs.Limbs.Length; i++)
            {
                if (Collision.CheckAABBvLineCollision(target.TopLeft, target.Size, Limbs.Limbs[i].ConnectPoint, Limbs.Limbs[i].EndPoint))
                    return true;
            }
            return false;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color drawColor)
        {
            NPCID.Sets.MustAlwaysDraw[npc.type] = true;
            if (LimbDrawer is null)
                LimbDrawer = new PrimitiveTrailCopy(PrimitiveWidthFunction, PrimitiveColorFunction, null, true, GameShaders.Misc["Infernum:PolterghastEctoplasm"]);

            GameShaders.Misc["Infernum:PolterghastEctoplasm"].SetShaderTexture(ModContent.GetTexture("Terraria/Misc/Perlin"));

            if (Limbs is null)
                return false;

            Main.spriteBatch.SetBlendState(BlendState.Additive);
            for (int i = 0; i < Limbs.Limbs.Length; i++)
            {
                npc.localAI[2] = 0f;
                if (Limbs.Limbs[i] is null)
                    return false;

                Vector2 offsetToNext = Vector2.Zero;
                if (i < Limbs.Limbs.Length - 1)
                {
                    offsetToNext = Limbs.Limbs[i + 1].EndPoint - Limbs.Limbs[i].EndPoint;
                    npc.localAI[2] = 0f;
                }

                Vector2 directionToNext = offsetToNext.SafeNormalize(Vector2.Zero);

                for (int j = 4; j >= 0; j--)
                {
                    GameShaders.Misc["Infernum:PolterghastEctoplasm"].UseOpacity((float)Math.Pow(MathHelper.Lerp(0.9f, 0.05f, j / 4f), 4D));
                    GameShaders.Misc["Infernum:PolterghastEctoplasm"].UseSaturation(i);

                    if (j > 0 && npc.velocity == Vector2.Zero)
                        continue;

                    Vector2 end = i == Limbs.Limbs.Length - 1 ? npc.Center - Vector2.UnitY * 10f : Limbs.Limbs[i + 1].ConnectPoint;
                    if (i == Limbs.Limbs.Length - 1)
                        end -= npc.velocity * j * 0.85f;
                    end += directionToNext * Utils.InverseLerp(15f, 175f, offsetToNext.Length(), true) * 20f;

                    List<Vector2> drawPositions = new List<Vector2>();
                    for (int k = 0; k < 10; k++)
                        drawPositions.Add(Vector2.Lerp(Limbs.Limbs[i].ConnectPoint, end, k / 9f));

                    LimbDrawer.Draw(drawPositions, -Main.screenPosition, 40);
                }
            }
            Main.spriteBatch.ResetBlendState();
            return false;
        }

        public override bool CheckActive() => false;
    }
}
