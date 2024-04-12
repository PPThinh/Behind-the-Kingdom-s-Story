using System;
using System.Globalization;
using GameCreator.Runtime.Characters;
using UnityEngine;
using GameCreator.Runtime.Common;

namespace GameCreator.Runtime.Stats
{
    [Title("Stat Base")]
    [Category("Stats/Stat Base")]

    [Image(typeof(IconStat), ColorTheme.Type.Red)]
    [Description("Returns the base value of a Stat")]
    
    [Serializable] [HideLabelsInEditor]
    public class GetStringStatBase : PropertyTypeGetString
    {
        [SerializeField] private PropertyGetGameObject m_Traits = GetGameObjectPlayer.Create();
        [SerializeField] protected Stat m_Stat;

        public override string Get(Args args)
        {
            if (this.m_Stat == null) return string.Empty;
            
            Traits traits = this.m_Traits.Get<Traits>(args);
            if (traits == null) return string.Empty;

            return traits.RuntimeStats
                .Get(this.m_Stat.ID)?
                .Base.ToString("0", CultureInfo.InvariantCulture) ?? string.Empty;
        }

        public override string String => string.Format(
            "{0}[{1}].Base", 
            this.m_Traits, 
            this.m_Stat != null ? TextUtils.Humanize(this.m_Stat.ID.String) : ""
        );
    }
}