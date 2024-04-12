using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

namespace GameCreator.Runtime.Stats.UnityUI
{
    [Version(0, 1, 1)]
    
    [Title("Change StatUI Stat")]
    [Category("Stats/UI/Change StatUI Stat")]
    
    [Image(typeof(IconStat), ColorTheme.Type.Red)]
    [Description("Changes the Stat asset from a Stat UI component")]

    [Parameter("Stat UI", "The game object with the Stat UI component")]
    [Parameter("Stat", "The new Stat asset")]

    [Serializable]
    public class InstructionStatsUIChangeStatUIStat : Instruction
    {
        [SerializeField]
        private PropertyGetGameObject m_StatUI = GetGameObjectInstance.Create();

        [SerializeField] private Stat m_Stat;

        public override string Title => string.Format(
            "Change {0} Stat to {1}", 
            this.m_StatUI, 
            this.m_Stat != null ? this.m_Stat.name : "(none)"
        );
        
        protected override Task Run(Args args)
        {
            if (this.m_Stat == null) return DefaultResult;
            
            GameObject statUIGameObject = this.m_StatUI.Get(args);
            if (statUIGameObject == null) return DefaultResult;

            StatUI statUI = statUIGameObject.Get<StatUI>();
            if (statUI == null) return DefaultResult;

            statUI.Stat = this.m_Stat;
            return DefaultResult;
        }
    }
}