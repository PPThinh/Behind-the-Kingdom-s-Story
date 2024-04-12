using System;
using UnityEngine;
using GameCreator.Runtime.Common;

namespace GameCreator.Runtime.Stats
{
    [Title("Stat Name")]
    [Category("Stats/Stat Name")]

    [Image(typeof(IconStat), ColorTheme.Type.Red)]
    [Description("Returns the name of a Stat")]
    
    [Serializable] [HideLabelsInEditor]
    public class GetStringStatName : PropertyTypeGetString
    {
        [SerializeField] protected Stat m_Stat;

        public override string Get(Args args) => this.m_Stat != null 
            ? this.m_Stat.GetName(args) 
            : string.Empty;

        public override string String => this.m_Stat != null ? this.m_Stat.ID.String : "(none)";
    }
}