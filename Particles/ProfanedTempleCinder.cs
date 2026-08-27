using InfernumMode.BaseEntities;
using Microsoft.Xna.Framework;
using Terraria;

namespace InfernumMode.Particles
{
    public class ProfanedTempleCinder : BaseCinderParticle
    {
        public override string Texture => "InfernumMode/Particles/ProfanedTempleCinder";

        public override void Initialize()
        {
            Color = Main.dayTime ? Color.White : Color.Cyan;
            base.Initialize();
        }
    }
}
