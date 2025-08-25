using CalamityMod.NPCs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.Leviathan
{
    public class AnahitaWaterIllusion : ModProjectile
    {
        public static NPC Anahita => Main.npc[CalamityGlobalNPC.siren];

        public ref float Time => ref projectile.ai[0];

        public ref float OffsetAngle => ref projectile.ai[1];

        public const int Lifetime = 270;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Anahita Illusion");
            Main.projFrames[projectile.type] = 6;
        }

        public override void SetDefaults()
        {
            projectile.width = 86;
            projectile.height = 190;
            projectile.timeLeft = Lifetime;
            projectile.friendly = false;
            projectile.hostile = true;
            projectile.Opacity = 0f;
            projectile.penetrate = -1;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;
            cooldownSlot = 1;
        }

        public override void AI()
        {
            // Die if Anahita is not around.
            if (CalamityGlobalNPC.siren == -1 || ComboAttackManager.FightState == LeviAnahitaFightState.LeviathanAlone)
            {
                projectile.Kill();
                return;
            }

            OffsetAngle += MathHelper.ToRadians(0.84f);
            projectile.Center = Anahita.Center + OffsetAngle.ToRotationVector2() * 125f;
            int frameHeight = Anahita.frame.Height;
            if (frameHeight == 0)
                frameHeight = 190;
            projectile.frame = Anahita.frame.Y / frameHeight;
            projectile.spriteDirection = Anahita.spriteDirection;
            Time++;
            projectile.Opacity = Utils.InverseLerp(0f, 24f, projectile.timeLeft, true) * Anahita.Opacity;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Texture2D texture = Main.projectileTexture[projectile.type];
            Rectangle frame = texture.Frame(1, Main.projFrames[projectile.type], 0, projectile.frame);
            Vector2 origin = frame.Size() * 0.5f;
            Vector2 baseDrawPosition = projectile.Center - Main.screenPosition;
            SpriteEffects direction = projectile.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            Color baseDrawColor = projectile.GetAlpha(lightColor) * 0.6f;

            // Create back afterimages.
            for (int i = 0; i < 4; i++)
            {
                Vector2 drawOffset = -Vector2.UnitY.RotatedBy(MathHelper.TwoPi * i / 4f) * 4f;
                Color drawColor = projectile.GetAlpha(Main.hslToRgb((projectile.identity * 0.2875f + i / 4f) % 1f, 1f, 0.75f));
                drawColor.A = (byte)(projectile.Opacity * 64f);
                drawColor *= 0.6f;
                Main.spriteBatch.Draw(texture, baseDrawPosition + drawOffset, frame, drawColor, projectile.rotation, origin, projectile.scale, direction, 0f);
            }

            Main.spriteBatch.Draw(texture, baseDrawPosition, frame, baseDrawColor, projectile.rotation, origin, projectile.scale, direction, 0f);
            return false;
        }
    }
}
