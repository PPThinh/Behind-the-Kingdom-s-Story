using System;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

namespace GameCreator.Runtime.Stats
{
    [Title("Has Stat Modifiers")]
    [Category("Stats/Has Stat Modifiers")]
    
    [Image(typeof(IconStat), ColorTheme.Type.Yellow)]
    [Description("Returns true if the targeted Stat component has a Stat Modifier")]

    [Parameter("Target", "The targeted game object with a Traits component")]
    [Parameter("Stat", "The Stat that checks if it has a Stat Modifier")]

    [Keywords("Skill", "Throw", "Check", "Dice")]
    [Keywords("Lock", "Pick", "Charisma", "Speech")]

    [Serializable]
    public class ConditionHasStatModifiers : Condition
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [SerializeField] private PropertyGetGameObject m_Target = GetGameObjectPlayer.Create();
        [SerializeField] private Stat m_Stat;

        // PROPERTIES: ----------------------------------------------------------------------------

        protected override string Summary => string.Format(
            "has {0} Modifier {1}",
            this.m_Target,
            this.m_Stat != null 
                ? this.m_Stat.ID.String 
                : "(none)"
        );

        // RUN METHOD: ----------------------------------------------------------------------------

        protected override bool Run(Args args)
        {
            if (this.m_Stat == null) return false;
            Traits target = this.m_Target.Get<Traits>(args);

            return target != null && target.RuntimeStats.Get(this.m_Stat.ID).HasModifiers;
        }
    }
}
