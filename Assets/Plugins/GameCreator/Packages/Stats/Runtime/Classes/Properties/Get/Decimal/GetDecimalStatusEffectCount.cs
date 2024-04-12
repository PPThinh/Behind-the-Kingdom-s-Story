using System;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace GameCreator.Runtime.Stats
{
    [Title("Status Effect Count")]
    [Category("Stats/Status Effect Count")]

    [Image(typeof(IconStatusEffect), ColorTheme.Type.Green)]
    [Description("The amount stacked of an active Status Effect on an object's Traits component")]

    [Serializable]
    public class GetDecimalStatusEffectCount : PropertyTypeGetDecimal
    {
        [SerializeField] private PropertyGetGameObject m_Traits = GetGameObjectPlayer.Create();

        [SerializeField] protected StatusEffectSelector m_StatusEffect = new StatusEffectSelector();

        public override double Get(Args args)
        {
            if (this.m_StatusEffect.Get == null) return 0f;
            
            Traits traits = this.m_Traits.Get<Traits>(args);
            if (traits == null) return 0f;

            return traits.RuntimeStatusEffects.GetActiveStackCount(this.m_StatusEffect.Get.ID);
        }

        public static PropertyGetDecimal Create => new PropertyGetDecimal(
            new GetDecimalStatusEffectCount()
        );

        public override string String =>
            $"{this.m_Traits}[{TextUtils.Humanize(this.m_StatusEffect.ToString())}].Count";
    }
}