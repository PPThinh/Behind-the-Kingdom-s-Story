using System;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace GameCreator.Runtime.Stats
{
    [Title("Stat Modifiers")]
    [Category("Stats/Stat Modifiers")]

    [Image(typeof(IconStat), ColorTheme.Type.Red, typeof(OverlayDot))]
    [Description("The amount all Stat Modifiers contribute to a game object's Stat value")]

    [Serializable]
    public class GetDecimalStatModifiers : PropertyTypeGetDecimal
    {
        [SerializeField] private PropertyGetGameObject m_Traits = GetGameObjectPlayer.Create();

        [SerializeField] private Stat m_Stat;

        public override double Get(Args args)
        {
            if (this.m_Stat == null) return 0f;
            
            Traits traits = this.m_Traits.Get<Traits>(args);
            if (traits == null) return 0f;

            return traits.RuntimeStats.Get(this.m_Stat.ID)?.ModifiersValue ?? 0f;
        }

        public static PropertyGetDecimal Create => new PropertyGetDecimal(
            new GetDecimalStatModifiers()
        );

        public override string String => string.Format(
            "{0}[{1}]", 
            this.m_Traits, 
            this.m_Stat != null ? TextUtils.Humanize(this.m_Stat.ID.String) : ""
        );
    }
}