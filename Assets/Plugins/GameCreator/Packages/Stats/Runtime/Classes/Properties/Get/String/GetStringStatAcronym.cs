using System;
using UnityEngine;
using GameCreator.Runtime.Common;

namespace GameCreator.Runtime.Stats
{
    [Title("Stat Acronym")]
    [Category("Stats/Stat Acronym")]

    [Image(typeof(IconStat), ColorTheme.Type.Red)]
    [Description("Returns the acronym of a Stat")]
    
    [Serializable] [HideLabelsInEditor]
    public class GetStringStatAcronym : PropertyTypeGetString
    {
        [SerializeField] protected Stat m_Stat;

        public override string Get(Args args) => this.m_Stat != null 
            ? this.m_Stat.GetAcronym(args) 
            : string.Empty;

        public override string String => this.m_Stat != null ? this.m_Stat.ID.String : "(none)";
    }
}