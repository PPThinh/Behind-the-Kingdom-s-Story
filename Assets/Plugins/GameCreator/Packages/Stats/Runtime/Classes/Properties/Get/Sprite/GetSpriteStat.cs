using System;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace GameCreator.Runtime.Stats
{
    [Title("Stat Sprite")]
    [Category("Stats/Stat Sprite")]
    
    [Image(typeof(IconStat), ColorTheme.Type.Red)]
    [Description("A reference to the Stat Sprite value")]

    [Serializable] [HideLabelsInEditor]
    public class GetSpriteStat : PropertyTypeGetSprite
    {
        [SerializeField] protected Stat m_Stat;

        public override Sprite Get(Args args) => this.m_Stat != null
            ? this.m_Stat.GetIcon(args) 
            : null;

        public override string String => this.m_Stat != null
            ? $"{this.m_Stat.name} Sprite"
            : "(none)";
    }
}