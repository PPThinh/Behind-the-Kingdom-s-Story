using System;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace GameCreator.Runtime.Stats
{
    [Title("Attribute Ratio")]
    [Category("Stats/Attribute Ratio")]

    [Image(typeof(IconAttr), ColorTheme.Type.Blue, typeof(OverlayDot))]
    [Description(
        "A value between 0 and 1 that indicates the ratio between the Attribute's current " +
        "value and its maximum"
    )]

    [Serializable]
    public class GetDecimalAttributeRatio : PropertyTypeGetDecimal
    {
        [SerializeField] private PropertyGetGameObject m_Traits = GetGameObjectPlayer.Create();

        [SerializeField] private Attribute m_Attribute;

        public override double Get(Args args)
        {
            if (this.m_Attribute == null) return 0f;
            
            Traits traits = this.m_Traits.Get<Traits>(args);
            if (traits == null) return 0f;

            return traits.RuntimeAttributes.Get(this.m_Attribute.ID)?.Ratio ?? 0f;
        }

        public static PropertyGetDecimal Create => new PropertyGetDecimal(
            new GetDecimalAttributeRatio()
        );

        public override string String => string.Format(
            "{0}[{1}].Ratio", 
            this.m_Traits, 
            this.m_Attribute != null ? TextUtils.Humanize(this.m_Attribute.ID.String) : ""
        );
    }
}