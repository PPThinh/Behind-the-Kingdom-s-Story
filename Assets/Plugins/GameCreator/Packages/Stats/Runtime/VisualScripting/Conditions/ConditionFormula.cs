using System;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

namespace GameCreator.Runtime.Stats
{
    [Title("Check Formula")]
    [Category("Stats/Check Formula")]
    
    [Image(typeof(IconFormula), ColorTheme.Type.Purple)]
    [Description("Returns the comparison between the result of a Formula against another value")]

    [Parameter("Formula", "The Formula used in the operation")]
    [Parameter("Source", "The game object that the Formula identifies as the Source")]
    [Parameter("Target", "The game object that the Formula identifies as the Target")]
    
    [Parameter("Compare To", "The value that the result of the Formula is compared to")]

    [Keywords("Skill", "Throw", "Check", "Dice")]
    [Keywords("Lock", "Pick", "Charisma", "Speech")]

    [Serializable]
    public class ConditionFormula : Condition
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [SerializeField] private Formula m_Formula;
        [SerializeField] private PropertyGetGameObject m_Source = GetGameObjectSelf.Create();
        [SerializeField] private PropertyGetGameObject m_Target = GetGameObjectTarget.Create();

        [Space]
        [SerializeField] private CompareDouble m_CompareTo = new CompareDouble(50f);

        // PROPERTIES: ----------------------------------------------------------------------------

        protected override string Summary => string.Format(
            "{0} {1}", 
            this.m_Formula != null ? this.m_Formula.name : "(none)", 
            this.m_CompareTo
        );

        // RUN METHOD: ----------------------------------------------------------------------------

        protected override bool Run(Args args)
        {
            if (this.m_Formula == null) return false;
            
            GameObject source = this.m_Source.Get(args);
            GameObject target = this.m_Target.Get(args);

            double result = this.m_Formula.Calculate(source, target);
            return this.m_CompareTo.Match(result, args);
        }
    }
}
