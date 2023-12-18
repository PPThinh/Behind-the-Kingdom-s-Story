using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

namespace GameCreator.Runtime.Melee
{
    [Version(0, 0, 1)]
    
    [Title("Try Cancel Skill")]
    [Description("Attempts to cancel an ongoing Skill being executed by a character")]

    [Category("Melee/Skills/Try Cancel Skill")]
    
    [Parameter("Character", "The Character reference using a Skill")]

    [Keywords("Melee", "Combat", "Skill", "Stop")]
    [Image(typeof(IconMeleeSkill), ColorTheme.Type.Red, typeof(OverlayCross))]
    
    [Serializable]
    public class InstructionMeleeTryCancel : Instruction
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [SerializeField] private PropertyGetGameObject m_Character = GetGameObjectPlayer.Create();

        // PROPERTIES: ----------------------------------------------------------------------------

        public override string Title => $"Try Cancel Skill on {this.m_Character}";

        // RUN METHOD: ----------------------------------------------------------------------------

        protected override Task Run(Args args)
        {
            Character character = this.m_Character.Get<Character>(args);
            if (character == null) return DefaultResult;

            character.Combat
                .RequestStance<MeleeStance>()
                .TryToCancel();
            
            return DefaultResult;
        }
    }
}