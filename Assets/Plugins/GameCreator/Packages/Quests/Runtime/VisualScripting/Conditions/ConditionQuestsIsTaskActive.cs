using System;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

namespace GameCreator.Runtime.Quests
{
    [Title("Is Task Active")]
    [Description("Returns true if a Task from a Journal is active")]

    [Category("Quests/Is Task Active")]

    [Keywords("Journal", "Mission")]
    
    [Image(typeof(IconTaskOutline), ColorTheme.Type.Blue)]
    [Serializable]
    public class ConditionQuestsIsTaskActive : Condition
    {
        [SerializeField] private PropertyGetGameObject m_Journal = GetGameObjectPlayer.Create();
        [SerializeField] private PickTask m_Task = new PickTask();
        
        // PROPERTIES: ----------------------------------------------------------------------------
        
        protected override string Summary => $"is {this.m_Task} Active";
        
        // RUN METHOD: ----------------------------------------------------------------------------

        protected override bool Run(Args args)
        {
            Journal journal = this.m_Journal.Get<Journal>(args);
            if (journal == null) return false;

            if (!this.m_Task.IsValid) return false;
            Quest quest = this.m_Task.Quest;
            int taskId = this.m_Task.TaskId;
            
            return quest != null && journal.IsTaskActive(quest, taskId);
        }
    }
}
