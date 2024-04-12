using System;
using UnityEngine;
using GameCreator.Runtime.Common;

namespace GameCreator.Runtime.Stats
{
    [Title("Status Effect Description")]
    [Category("Stats/Status Effect Description")]

    [Image(typeof(IconStatusEffect), ColorTheme.Type.Green)]
    [Description("Returns the description text of a Status Effect")]
    
    [Serializable] [HideLabelsInEditor]
    public class GetStringStatusEffectDescription : PropertyTypeGetString
    {
        [SerializeField] protected StatusEffectSelector m_StatusEffect = new StatusEffectSelector();

        public override string Get(Args args) => this.m_StatusEffect.Get != null 
            ? this.m_StatusEffect.Get.GetDescription(args) 
            : string.Empty;

        public override string String => this.m_StatusEffect.ToString();
    }
}