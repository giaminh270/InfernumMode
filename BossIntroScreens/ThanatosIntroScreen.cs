using CalamityMod.NPCs.ExoMechs.Thanatos;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

using TMLSoundType = Terraria.ModLoader.SoundType;
namespace InfernumMode.BossIntroScreens
{
    public class ThanatosIntroScreen : BaseIntroScreen
    {
        public override TextColorData TextColor => new Color(115, 124, 139);

        public override int AnimationTime => 210;

        public override bool TextShouldBeCentered => true;

        public override string TextToDisplay => "The Perfect Annihilator\nThanatos";

        public override Effect ShaderToApplyToLetters => GameShaders.Misc["Infernum:MechsIntro"].Shader;

        public override void PrepareShader(Effect shader)
        {
            shader.Parameters["uColor"].SetValue(new Vector3(0.02f, 0.54f, 0.91f));
            shader.GraphicsDevice.Textures[1] = ModContent.GetTexture("InfernumMode/ExtraTextures/DiagonalGleam");
        }

        public override bool ShouldBeActive() => NPC.AnyNPCs(ModContent.NPCType<ThanatosHead>());

        public override LegacySoundStyle SoundToPlayWithTextCreation => InfernumMode.CalamityMod.GetLegacySoundSlot(TMLSoundType.Custom, "Sounds/Custom/ThanatosTransition");

        public override LegacySoundStyle SoundToPlayWithLetterAddition => SoundID.NPCHit4;

        public override bool CanPlaySound => LetterDisplayCompletionRatio(AnimationTimer) >= 1f;

        public override float LetterDisplayCompletionRatio(int animationTimer)
        {
            float completionRatio = Utils.InverseLerp(TextDelayInterpolant, 0.92f, animationTimer / (float)AnimationTime, true);

            // If the completion ratio exceeds the point where the name is displayed, display all letters.
            int startOfLargeTextIndex = TextToDisplay.IndexOf('\n');
            int currentIndex = (int)(completionRatio * TextToDisplay.Length);
            if (currentIndex >= startOfLargeTextIndex)
                completionRatio = 1f;

            return completionRatio;
        }
    }
}