using InfernumMode.BaseEntities;
using Microsoft.Xna.Framework;
using Terraria;

namespace InfernumMode.BehaviorOverrides.BossAIs.Twins
{
    public class TwinsSpriteExplosion : BaseSpriteExplosionProjectile
    {
        public bool SpazmatismVariant
        {
            get => projectile.ai[1] == 1f;
            set => projectile.ai[1] = value.ToInt();
        }

        public override int GetFrameUpdateRate => 4;

        public override Color ExplosionColor => Color.Lerp(SpazmatismVariant ? Color.Lime : Color.Red, SpazmatismVariant ? Color.Yellow : Color.Orange, projectile.identity / 8f % 0.67f) * 1.5f;
    }
}
