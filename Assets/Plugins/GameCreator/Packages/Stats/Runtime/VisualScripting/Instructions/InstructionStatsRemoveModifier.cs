using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

namespace GameCreator.Runtime.Stats
{
    [Version(0, 1, 1)]
    
    [Title("Remove Stat Modifier")]
    [Category("Stats/Remove Stat Modifier")]
    
    [Image(typeof(IconStat), ColorTheme.Type.Yellow, typeof(OverlayMinus))]
    [Description("Removes an equivalent Modifier from the selected Stat on a game object's Traits component.")]

    [Parameter("Target", "The targeted game object with a Traits component")]
    [Parameter("Stat", "The Stat that receives the Modifier")]
    [Parameter("Type", "If the Modifier changes the Stat by a constant value or by a percentage")]
    [Parameter("Value", "The constant or percentage-based value of the Modifier")]
    
    [Keywords("Slot", "Decrease", "Unequip", "Weaken")]
    [Keywords("Vitality", "Constitution", "Strength", "Dexterity", "Defense", "Armor")]
    [Keywords("Magic", "Wisdom", "Intelligence")]

    [Serializable]
    public class InstructionStatsRemoveModifier : Instruction
    {
        [SerializeField] private PropertyGetGameObject m_Target = GetGameObjectPlayer.Create();
        
        [SerializeField] private Stat m_Stat;
        [SerializeField] private ModifierType m_Type = ModifierType.Constant;
        [SerializeField] private PropertyGetDecimal m_Value = new PropertyGetDecimal(15f);

        public override string Title => string.Format(
            "Remove {0} from {1}[{2}]",
            this.m_Value,
            this.m_Target,
            this.m_Stat != null 
                ? this.m_Stat.ID.String 
                : string.Empty
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

            float value = (float) this.m_Value.Get(args);
            stat.RemoveModifier(this.m_Type, value);
            
            return DefaultResult;
        }
    }
}