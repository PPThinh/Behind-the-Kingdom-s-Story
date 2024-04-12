using System;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace GameCreator.Runtime.Stats
{
    [Title("Attribute Sprite")]
    [Category("Stats/Attribute Sprite")]
    
    [Image(typeof(IconAttr), ColorTheme.Type.Blue)]
    [Description("A reference to the Attribute Sprite value")]

    [Serializable] [HideLabelsInEditor]
    public class GetSpriteAttribute : PropertyTypeGetSprite
    {
        [SerializeField] protected Attribute m_Attribute;

        public override Sprite Get(Args args) => this.m_Attribute != null
            ? this.m_Attribute.GetIcon(args) 
            : null;

        public override string String => this.m_Attribute != null
            ? $"{this.m_Attribute.name} Sprite"
            : "(none)";
    }
}