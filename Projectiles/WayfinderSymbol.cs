using InfernumMode.GlobalInstances;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.Projectiles
{
    public class WayfinderSymbol : ModProjectile
    {

        public ref float Time => ref projectile.ai[0];
        public ref float Initialized => ref projectile.ai[1];
        public float MaxScale;
        public int ColorVariation;
        public float RotationAmount;
        public float Speed;
        public static Color[] Colors => new Color[]
        {
            // Light yellow
            new Color (255 ,255 ,150),
            // Golden
            new Color(255, 191, 73),
            // Orange
            new Color(206, 116, 59)
        };

        public const int Lifetime = 240;

        public override void SetStaticDefaults() => DisplayName.SetDefault("Wayfinder Gate Symbol");

        public override void SetDefaults()
        {
            projectile.width = 48;
            projectile.height = 48;
            projectile.ignoreWater = true;
            projectile.tileCollide = false;
            projectile.netImportant = true;
            projectile.penetrate = -1;
            projectile.timeLeft = Lifetime;
            projectile.Opacity = 0f;
        }

        public override void AI()
        {
            // Die if the gate position is not set.
            if (PoDWorld.WayfinderGateLocation == Vector2.Zero)
            {
                projectile.Kill();
                return;
            }
            // Initialize the random fields, to add variation to each symbol.
            if (Initialized == 0 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                MaxScale = Main.rand.NextFloat(0.3f, 0.5f);
                ColorVariation = Main.rand.Next(0, 3);
                RotationAmount = Main.rand.NextFloat(-0.03f, 0.03f);
                Speed = Main.rand.NextFloat(0.25f, 0.45f);
                projectile.velocity = (-Vector2.UnitY * Speed).RotatedByRandom(0.3f);
                Initialized = 1;
                projectile.netUpdate = true;
            }

            // Fade in and out.
            projectile.Opacity = Utils.InverseLerp(0f, 108f, Time, true) * Utils.InverseLerp(0f, 60f, projectile.timeLeft, true);
            projectile.scale = MathHelper.Clamp(Utils.InverseLerp(0f, 108f, Time, true) * Utils.InverseLerp(0f, 60f, projectile.timeLeft, true), 0, MaxScale);
            projectile.rotation += RotationAmount;
            Time++;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(MaxScale);
            writer.Write(ColorVariation);
            writer.Write(RotationAmount);
            writer.Write(Speed);
        }
        public override void ReceiveExtraAI(BinaryReader reader)
        {
            MaxScale = reader.ReadSingle();
            ColorVariation = reader.ReadInt32();
            RotationAmount = reader.ReadSingle();
            Speed = reader.ReadSingle();
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Texture2D texture = ModContent.GetTexture(Texture);
            Texture2D bloomTexture = ModContent.GetTexture("CalamityMod/Particles/BloomCircle");
            Vector2 drawPos = projectile.position - Main.screenPosition;
            Color color = Colors[ColorVariation];
            Vector2 origin = texture.Size() * 0.5f;

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            Main.spriteBatch.Draw(bloomTexture, drawPos, null, color * projectile.Opacity * 0.55f, 0f, bloomTexture.Size() * 0.5f, 0.7f * MaxScale, 0, 0);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            Main.spriteBatch.Draw(texture, drawPos, null, color * projectile.Opacity * 0.5f, projectile.rotation, origin, projectile.scale, SpriteEffects.None, 0f);
            return false;
        }
    }
}
