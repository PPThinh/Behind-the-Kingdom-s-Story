using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Common.Audio;
using UnityEngine;

namespace GameCreator.Runtime.VisualScripting
{
    [Version(0, 1, 1)]

    [Title("Play Music")]
    [Description(
        "Plays a looped Audio Clip. Useful for background music or persistent sounds."
    )]

    [Category("Audio/Play Music")]
    
    [Parameter("Audio Clip", "The Audio Clip to be played")]
    [Parameter("Transition In", "Time it takes for the sound to fade in")]
    [Parameter("Spatial Blending", "Whether the sound is placed in a 3D space or not")]
    [Parameter("Target", "A Game Object reference that the sound follows as the source")]

    [Keywords("Audio", "Ambience", "Background")]
    [Image(typeof(IconHeadset), ColorTheme.Type.Yellow)]
    
    [Serializable]
    public class InstructionCommonAudioMusicPlay : Instruction
    {
        [SerializeField] private AudioClip m_AudioClip = null;
        [SerializeField] private AudioConfigMusic m_Config = new AudioConfigMusic();

        public override string Title => string.Format(
            "Play Music: {0}",
            this.m_AudioClip != null ? this.m_AudioClip.name : "(none)"
        );

        protected override Task Run(Args args)
        {
            if (!AudioManager.Instance.Music.IsPlaying(this.m_AudioClip))
            {
                _ = AudioManager.Instance.Music.Play(
                    this.m_AudioClip, 
                    this.m_Config,
                    args
                );   
            }

            return DefaultResult;
        }
    }
}