using System;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

namespace GameCreator.Runtime.Stats
{
    [Title("Compare Attribute")]
    [Description("Returns true if the Attribute comparison is successful")]

    [Category("Stats/Compare Attribute")]
    
    [Parameter("Traits", "The targeted game object with a Traits component")]
    [Parameter("Attribute", "The Attribute type value that is compared")]
    [Parameter("Value", "The type of value from the attribute to compare")]
    [Parameter("Comparison", "The comparison operation performed between both values")]
    [Parameter("Compare To", "The decimal value that is compared against")]
    
    [Keywords("Health", "Mana", "Stamina", "Magic", "Life", "HP", "MP")]
    [Image(typeof(IconAttr), ColorTheme.Type.Blue)]
    
    [Serializable]
    public class ConditionCompareAttribute : Condition
    {
        private enum ValueType
        {
            Value,
            MaxValue,
            MinValue,
            Ratio
        }
        
        // MEMBERS: -------------------------------------------------------------------------------

        [SerializeField] private PropertyGetGameObject m_Traits = GetGameObjectPlayer.Create();
        [SerializeField] private Attribute m_Attribute;
        [SerializeField] private ValueType m_Value = ValueType.Value;
        
        [SerializeField] 
        private CompareDouble m_CompareTo = new CompareDouble();

        // PROPERTIES: ----------------------------------------------------------------------------

        protected override string Summary => string.Format(
            "{0}[{1}] {2}", 
            this.m_Traits,
            this.m_Attribute != null ? this.m_Attribute.ID.String : "(none)", 
            this.m_CompareTo
        );

        // RUN METHOD: ----------------------------------------------------------------------------

        protected override bool Run(Args args)
        {
            GameObject target = this.m_Traits.Get(args);
            if (target == null) return false;

            Traits traits = target.Get<Traits>();
            if (traits == null) return false;
            
            if (this.m_Attribute == null) return false;

            double value = this.m_Value switch
            {
                ValueType.Value => traits.RuntimeAttributes.Get(this.m_Attribute.ID).Value,
                ValueType.MaxValue => traits.RuntimeAttributes.Get(this.m_Attribute.ID).MaxValue,
                ValueType.MinValue => traits.RuntimeAttributes.Get(this.m_Attribute.ID).MinValue,
                ValueType.Ratio => traits.RuntimeAttributes.Get(this.m_Attribute.ID).Ratio,
                _ => throw new ArgumentOutOfRangeException()
            };
            
            return this.m_CompareTo.Match(value, args);
        }
    }
}
