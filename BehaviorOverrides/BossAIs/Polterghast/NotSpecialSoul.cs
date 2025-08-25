using CalamityMod.NPCs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.Polterghast
{
    public class NotSpecialSoul : ModProjectile
    {
        public bool Cyan => projectile.ai[0] == 1f;

        public bool AcceleratesAndHomes => projectile.ai[1] == 1f;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Soul");
            Main.projFrames[projectile.type] = 4;
            ProjectileID.Sets.TrailingMode[projectile.type] = 2;
            ProjectileID.Sets.TrailCacheLength[projectile.type] = 4;
        }

        public override void SetDefaults()
        {
            projectile.width = projectile.height = 32;
            projectile.hostile = true;
            projectile.friendly = false;
            projectile.ignoreWater = true;
            projectile.tileCollide = false;
            projectile.timeLeft = 200;
            projectile.penetrate = -1;
            cooldownSlot = 1;
        }

        public override void SendExtraAI(BinaryWriter writer) => writer.Write(projectile.timeLeft);

        public override void ReceiveExtraAI(BinaryReader reader) => projectile.timeLeft = reader.ReadInt32();
        public override void AI()
        {
            if (!Main.npc.IndexInRange(CalamityGlobalNPC.ghostBoss))
            {
                projectile.Kill();
                return;
            }

            NPC polterghast = Main.npc[CalamityGlobalNPC.ghostBoss];

            // Fade in/out and rotate.
            projectile.Opacity = Utils.InverseLerp(300f, 295f, projectile.timeLeft, true) * Utils.InverseLerp(0f, 25f, projectile.timeLeft, true);
            projectile.rotation = projectile.velocity.ToRotation() - MathHelper.PiOver2;

            // Handle frames.
            projectile.frameCounter++;
            if (projectile.frameCounter % 5 == 4)
                projectile.frame = (projectile.frame + 1) % Main.projFrames[projectile.type];

            // Create periodic puffs of dust.
            if (projectile.timeLeft % 30 == 29 && Main.rand.NextBool(2))
            {
                for (int i = 0; i < 8; i++)
                {
                    Vector2 dustOffset = Vector2.UnitY.RotatedBy(MathHelper.TwoPi * i / 16f) * new Vector2(4f, 1f);
                    dustOffset = dustOffset.RotatedBy(projectile.velocity.ToRotation());

                    Dust ectoplasm = Dust.NewDustDirect(projectile.Center, 0, 0, 175, 0f, 0f);
                    ectoplasm.position = projectile.Center + dustOffset;
                    ectoplasm.velocity = dustOffset.SafeNormalize(Vector2.Zero) * 1.5f;
                    ectoplasm.color = Color.Lerp(Color.Purple, Color.White, 0.5f);
                    ectoplasm.scale = 1.5f;
                    ectoplasm.noGravity = true;
                }
            }

            // Return to the polterghast.
            if (projectile.timeLeft < 3)
            {
                projectile.Center = Vector2.Lerp(projectile.Center, polterghast.Center, 0.06f);
                projectile.velocity = (projectile.velocity * 11f + projectile.SafeDirectionTo(polterghast.Center) * 36f) / 12f;
                projectile.damage = 0;
                if (projectile.Hitbox.Intersects(polterghast.Hitbox))
                {
                    polterghast.ai[2]--;
                    projectile.Kill();
                }
                projectile.timeLeft = 2;
            }

            // Accelerate and home.
            else if (projectile.timeLeft < 135f && AcceleratesAndHomes)
            {
                float WHATTHEFUCKDOYOUWANTFROMME = projectile.velocity.Length() + 0.01f;
                Player target = Main.player[polterghast.target];
                projectile.velocity = (projectile.velocity * 169f + projectile.SafeDirectionTo(target.Center) * WHATTHEFUCKDOYOUWANTFROMME) / 170f;
                projectile.velocity = projectile.velocity.SafeNormalize(Vector2.UnitY) * WHATTHEFUCKDOYOUWANTFROMME;
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Texture2D texture = ModContent.GetTexture("InfernumMode/BehaviorOverrides/BossAIs/Polterghast/SoulLarge" + (Cyan ? "Cyan" : ""));
            if (projectile.whoAmI % 2 == 0)
                texture = ModContent.GetTexture("InfernumMode/BehaviorOverrides/BossAIs/Polterghast/SoulMedium" + (Cyan ? "Cyan" : ""));

            Utilities.DrawAfterimagesCentered(projectile, lightColor, ProjectileID.Sets.TrailingMode[projectile.type], 2, texture);
            return false;
        }

        public override Color? GetAlpha(Color lightColor)
        {
            Color color = Color.White;
            color.A = 0;
            return color * projectile.Opacity;
        }
    }
}
