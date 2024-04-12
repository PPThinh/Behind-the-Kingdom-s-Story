using System;
using GameCreator.Runtime.Characters;
using UnityEngine;
using GameCreator.Runtime.Common;

namespace GameCreator.Runtime.Stats
{
    [Title("Stat Base")]
    [Category("Stats/Stat Base")]
    
    [Description("Sets the base value of a Stat on a game object's Traits component")]
    [Image(typeof(IconStat), ColorTheme.Type.Red)]

    [Serializable]
    public class SetNumberStat : PropertyTypeSetNumber
    {
        [SerializeField] private PropertyGetGameObject m_Traits = GetGameObjectPlayer.Create();

        [SerializeField] private Stat m_Stat;

        public override void Set(double value, Args args)
        {
            if (this.m_Stat == null) return;
            
            GameObject gameObject = this.m_Traits.Get(args);
            if (gameObject == null) return;

            Traits traits = gameObject.Get<Traits>();
            if (traits == null) return;

            traits.RuntimeStats.Get(this.m_Stat.ID).Base = (float) value;
        }
        
        public override double Get(Args args)
        {
            if (this.m_Stat == null) return 0f;
            
            GameObject gameObject = this.m_Traits.Get(args);
            if (gameObject == null) return 0f;

            Traits traits = gameObject.Get<Traits>();
            return traits != null ? traits.RuntimeStats.Get(this.m_Stat.ID).Value : 0f;
        }

        public static PropertySetNumber Create => new PropertySetNumber(
            new SetNumberStat()
        );
        
        public override string String => string.Format(
            "{0}[{1}].Base", 
            this.m_Traits, 
            this.m_Stat != null ? TextUtils.Humanize(this.m_Stat.ID.String) : ""
        );
    }
}