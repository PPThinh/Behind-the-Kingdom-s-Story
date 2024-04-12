using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

namespace GameCreator.Runtime.Stats
{
    [Version(0, 1, 1)]
    
    [Title("Change Attribute")]
    [Category("Stats/Change Attribute")]
    
    [Image(typeof(IconAttr), ColorTheme.Type.Blue)]
    [Description("Changes the current Attribute value of a game object's Traits component")]

    [Parameter("Target", "The targeted game object with a Traits component")]
    [Parameter("Attribute", "The Attribute type that changes its value")]
    [Parameter("Change", "The value changed")]
    
    [Keywords("Health", "HP", "Mana", "MP", "Stamina")]

    [Serializable]
    public class InstructionStatsChangeAttribute : Instruction
    {
        [SerializeField] private PropertyGetGameObject m_Target = GetGameObjectPlayer.Create();
        
        [SerializeField] private Attribute m_Attribute;
        
        [SerializeField] private ChangeDecimal m_Change = new ChangeDecimal(100f);
        
        public override string Title => string.Format(
            "{0}[{1}] {2}",
            this.m_Target,
            this.m_Attribute != null 
                ? this.m_Attribute.ID.String  
                : string.Empty,
            this.m_Change
        );
        
        protected override Task Run(Args args)
        {
            GameObject target = this.m_Target.Get(args);
            if (target == null) return DefaultResult;

            Traits traits = target.Get<Traits>();
            if (traits == null) return DefaultResult;
            
            if (this.m_Attribute == null) return DefaultResult;
            RuntimeAttributeData attribute = traits.RuntimeAttributes.Get(this.m_Attribute.ID);
            if (attribute == null) return DefaultResult;

            attribute.Value = (float) this.m_Change.Get(attribute.Value, args);
            return DefaultResult;
        }
    }
}