using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

namespace GameCreator.Runtime.Stats
{
    [Version(0, 1, 1)]
    
    [Title("Change Stat")]
    [Category("Stats/Change Stat")]
    
    [Image(typeof(IconStat), ColorTheme.Type.Red)]
    [Description("Changes the base Stat value of a game object's Traits component")]

    [Parameter("Target", "The targeted game object with a Traits component")]
    [Parameter("Stat", "The Stat type that changes its value")]
    [Parameter("Change", "The value changed")]
    
    [Keywords("Vitality", "Constitution", "Strength", "Dexterity", "Defense", "Armor")]
    [Keywords("Magic", "Wisdom", "Intelligence")]
    
    [Serializable]
    public class InstructionStatsChangeStat : Instruction
    {
        [SerializeField] private PropertyGetGameObject m_Target = GetGameObjectPlayer.Create();
        
        [SerializeField] private Stat m_Stat;
        [SerializeField] private ChangeDecimal m_Change = new ChangeDecimal(100f);
        
        public override string Title => string.Format(
            "{0}[{1}] {2}",
            this.m_Target,
            this.m_Stat != null 
                ? this.m_Stat.ID.String 
                : string.Empty,
            this.m_Change
        );
        
        protected override Task Run(Args args)
        {
            GameObject target = this.m_Target.Get(args);
            if (target == null) return DefaultResult;

            Traits traits = target.Get<Traits>();
            if (traits == null) return DefaultResult;
            
            if (this.m_Stat == null) return DefaultResult;
            RuntimeStatData stat = traits.RuntimeStats.Get(this.m_Stat.ID);
            if (stat == null) return DefaultResult;

            stat.Base = (float) this.m_Change.Get(stat.Base, args);
            return DefaultResult;
        }
    }
}