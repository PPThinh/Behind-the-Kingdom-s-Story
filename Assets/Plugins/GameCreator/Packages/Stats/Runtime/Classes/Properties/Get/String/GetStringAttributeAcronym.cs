using System;
using UnityEngine;
using GameCreator.Runtime.Common;

namespace GameCreator.Runtime.Stats
{
    [Title("Attribute Acronym")]
    [Category("Stats/Attribute Acronym")]

    [Image(typeof(IconAttr), ColorTheme.Type.Blue)]
    [Description("Returns the acronym of an Attribute")]
    
    [Serializable] [HideLabelsInEditor]
    public class GetStringAttributeAcronym : PropertyTypeGetString
    {
        [SerializeField] protected Attribute m_Attribute;

        public override string Get(Args args) => this.m_Attribute != null 
            ? this.m_Attribute.GetAcronym(args) 
            : string.Empty;

        public override string String => this.m_Attribute != null ? this.m_Attribute.ID.String : "(none)";
    }
}