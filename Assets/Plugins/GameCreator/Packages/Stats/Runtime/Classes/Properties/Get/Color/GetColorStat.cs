using System;
using UnityEngine;
using GameCreator.Runtime.Common;

namespace GameCreator.Runtime.Stats
{
    [Title("Stat Color")]
    [Category("Stats/Stat Color")]
    
    [Image(typeof(IconStat), ColorTheme.Type.Red)]
    [Description("Returns the Color value of a Stat")]

    [Serializable] [HideLabelsInEditor]
    public class GetColorStat : PropertyTypeGetColor
    {
        [SerializeField] private Stat m_Stat;

        public override Color Get(Args args) => this.m_Stat != null 
            ? this.m_Stat.Color 
            : Color.black;

        public override string String => this.m_Stat != null
            ? this.m_Stat.ID.String 
            : "(none)";
    }
}