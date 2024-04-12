using System;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

namespace GameCreator.Runtime.Stats
{
    [Title("Has Status Effect")]
    [Description("Returns true if the game object has a particular Status Effect active")]

    [Category("Stats/Has Status Effect")]
    
    [Parameter("Target", "The targeted game object with a Traits component")]
    [Parameter("Status Effect", "The type of Status Effect that is checked")]
    [Parameter("Min Amount", "The minimum amount of stacked and active Status Effects")]

    [Keywords("Buff", "Debuff", "Enhance", "Ailment")]
    [Keywords(
        "Blind", "Dark", "Burn", "Confuse", "Dizzy", "Stagger", "Fear", "Freeze", "Paralyze", 
        "Shock", "Silence", "Sleep", "Silence", "Slow", "Toad", "Weak", "Strong", "Poison"
    )]
    [Keywords(
        "Haste", "Protect", "Reflect", "Regenerate", "Shell", "Armor", "Shield", "Berserk",
        "Focus", "Raise"
    )]
    
    [Image(typeof(IconStatusEffect), ColorTheme.Type.Green)]
    
    [Serializable]
    public class ConditionHasStatusEffect : Condition
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [SerializeField] private PropertyGetGameObject m_Target = GetGameObjectPlayer.Create();
        
        [SerializeField] private StatusEffect m_StatusEffect;
        [SerializeField] private int m_MinAmount = 1;

        // PROPERTIES: ----------------------------------------------------------------------------

        protected override string Summary => string.Format(
            "{0} has {1} {2}",
            this.m_Target,
            this.m_StatusEffect != null ? this.m_StatusEffect.ID.String : "(none)",
            this.m_MinAmount > 1 ? $"> {this.m_MinAmount}" : string.Empty
        );

        // RUN METHOD: ----------------------------------------------------------------------------

        protected override bool Run(Args args)
        {
            GameObject target = this.m_Target.Get(args);
            if (target == null) return false;

            Traits traits = target.Get<Traits>();
            if (traits == null) return false;
            
            if (this.m_StatusEffect == null) return false;
            
            int amount = traits.RuntimeStatusEffects.GetActiveStackCount(this.m_StatusEffect.ID);
            return amount >= this.m_MinAmount;
        }
    }
}
