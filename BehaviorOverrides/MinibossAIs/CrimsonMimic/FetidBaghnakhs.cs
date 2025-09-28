using InfernumMode;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.MinibossAIs.CrimsonMimic
{
    public class FetidBaghnakhs : ModProjectile
    {
        public float SpinOffsetAngle;

        public int OwnerIndex => (int)projectile.ai[1];

        public ref float Time => ref projectile.ai[0];

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Fetid Baghnakhs");
            ProjectileID.Sets.TrailingMode[projectile.type] = 2;
            ProjectileID.Sets.TrailCacheLength[projectile.type] = 3;
        }

        public override void SetDefaults()
        {
            projectile.width = projectile.height = 22;
            projectile.hostile = true;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;
            projectile.netImportant = true;
            projectile.timeLeft = 900;
            projectile.penetrate = -1;
        }

        public override void SendExtraAI(BinaryWriter writer) => writer.Write(SpinOffsetAngle);

        public override void ReceiveExtraAI(BinaryReader reader) => SpinOffsetAngle = reader.ReadSingle();

        public override void AI()
        {
            Time++;
            SpinOffsetAngle += MathHelper.TwoPi / 60f;
            projectile.Opacity = Utils.InverseLerp(0f, 30f, projectile.timeLeft, true);
            projectile.Center = Main.npc[OwnerIndex].Center + SpinOffsetAngle.ToRotationVector2() * projectile.Opacity * 120f;

            if (!Main.npc[OwnerIndex].active)
                projectile.Kill();
        }

        public override Color? GetAlpha(Color lightColor) => Color.White * projectile.Opacity;

        public override bool CanDamage() => projectile.Opacity >= 1f;

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Utilities.DrawAfterimagesCentered(projectile, lightColor, ProjectileID.Sets.TrailingMode[projectile.type], 3);
            return false;
        }
    }
}
