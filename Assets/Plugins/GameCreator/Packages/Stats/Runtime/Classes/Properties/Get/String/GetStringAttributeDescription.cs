using System;
using UnityEngine;
using GameCreator.Runtime.Common;

namespace GameCreator.Runtime.Stats
{
    [Title("Attribute Description")]
    [Category("Stats/Attribute Description")]

    [Image(typeof(IconAttr), ColorTheme.Type.Blue)]
    [Description("Returns the description text of an Attribute")]
    
    [Serializable] [HideLabelsInEditor]
    public class GetStringAttributeDescription : PropertyTypeGetString
    {
        [SerializeField] protected Attribute m_Attribute;

        public override string Get(Args args) => this.m_Attribute != null 
            ? this.m_Attribute.GetDescription(args) 
            : string.Empty;

        public override string String => this.m_Attribute != null 
            ? this.m_Attribute.ID.String 
            : "(none)";
    }
}