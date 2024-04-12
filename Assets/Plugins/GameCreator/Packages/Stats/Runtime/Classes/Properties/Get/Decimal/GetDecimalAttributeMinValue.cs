using System;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace GameCreator.Runtime.Stats
{
    [Title("Attribute Min Value")]
    [Category("Stats/Attribute Min Value")]

    [Image(typeof(IconAttr), ColorTheme.Type.Blue, typeof(OverlayArrowLeft))]
    [Description("The Attribute's min range value of a game object's Traits component")]

    [Serializable]
    public class GetDecimalAttributeMinValue : PropertyTypeGetDecimal
    {
        [SerializeField]
        private PropertyGetGameObject m_Traits = GetGameObjectPlayer.Create();

        [SerializeField] private Attribute m_Attribute;

        public override double Get(Args args)
        {
            if (this.m_Attribute == null) return 0;
            
            Traits traits = this.m_Traits.Get<Traits>(args);
            if (traits == null) return 0f;

            return traits.RuntimeAttributes.Get(this.m_Attribute.ID)?.MinValue ?? 0f;
        }

        public static PropertyGetDecimal Create => new PropertyGetDecimal(
            new GetDecimalAttributeMinValue()
        );

        public override string String => string.Format(
            "{0}[{1}].Min", 
            this.m_Traits, 
            this.m_Attribute != null ? TextUtils.Humanize(this.m_Attribute.ID.String) : ""
        );
    }
}