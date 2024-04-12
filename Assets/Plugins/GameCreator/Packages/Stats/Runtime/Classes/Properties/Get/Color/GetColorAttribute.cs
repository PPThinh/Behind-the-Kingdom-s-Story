using System;
using UnityEngine;
using GameCreator.Runtime.Common;

namespace GameCreator.Runtime.Stats
{
    [Title("Attribute Color")]
    [Category("Stats/Attribute Color")]
    
    [Image(typeof(IconAttr), ColorTheme.Type.Blue)]
    [Description("Returns the Color value of an Attribute")]

    [Serializable] [HideLabelsInEditor]
    public class GetColorAttribute : PropertyTypeGetColor
    {
        [SerializeField] private Attribute m_Attribute;

        public override Color Get(Args args) => this.m_Attribute != null 
            ? this.m_Attribute.Color 
            : Color.black;

        public override string String => this.m_Attribute != null
            ? this.m_Attribute.ID.String 
            : "(none)";
    }
}