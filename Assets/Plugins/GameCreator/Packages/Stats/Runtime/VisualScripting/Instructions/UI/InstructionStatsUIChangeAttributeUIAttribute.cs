using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

namespace GameCreator.Runtime.Stats.UnityUI
{
    [Version(0, 1, 1)]
    
    [Title("Change AttributeUI Attribute")]
    [Category("Stats/UI/Change AttributeUI Attribute")]
    
    [Image(typeof(IconAttr), ColorTheme.Type.Blue)]
    [Description("Changes the Attribute from a Attribute UI component")]

    [Parameter("Attribute UI", "The game object with the Attribute UI component")]
    [Parameter("Attribute", "The new Attribute asset")]

    [Serializable]
    public class InstructionStatsUIChangeAttributeUIAttribute : Instruction
    {
        [SerializeField]
        private PropertyGetGameObject m_AttributeUI = GetGameObjectInstance.Create();

        [SerializeField] private Attribute m_Attribute;

        public override string Title => string.Format(
            "Change {0} Attribute to {1}",
            this.m_AttributeUI,
            this.m_Attribute != null ? this.m_Attribute.name : "(none)"
        );
        
        protected override Task Run(Args args)
        {
            if (this.m_Attribute == null) return DefaultResult;
            
            GameObject attributeUIGameObject = this.m_AttributeUI.Get(args);
            if (attributeUIGameObject == null) return DefaultResult;

            AttributeUI attributeUI = attributeUIGameObject.Get<AttributeUI>();
            if (attributeUI == null) return DefaultResult;

            attributeUI.Attribute = this.m_Attribute;
            return DefaultResult;
        }
    }
}