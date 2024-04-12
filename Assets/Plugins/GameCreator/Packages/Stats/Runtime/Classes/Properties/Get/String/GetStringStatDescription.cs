using System;
using UnityEngine;
using GameCreator.Runtime.Common;

namespace GameCreator.Runtime.Stats
{
    [Title("Stat Description")]
    [Category("Stats/Stat Description")]

    [Image(typeof(IconStat), ColorTheme.Type.Red)]
    [Description("Returns the description text of a Stat")]
    
    [Serializable] [HideLabelsInEditor]
    public class GetStringStatDescription : PropertyTypeGetString
    {
        [SerializeField] protected Stat m_Stat;

        public override string Get(Args args) => this.m_Stat != null 
            ? this.m_Stat.GetDescription(args) 
            : string.Empty;

        public override string String => this.m_Stat != null ? this.m_Stat.ID.String : "(none)";
    }
}