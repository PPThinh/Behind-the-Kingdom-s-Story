using System;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

namespace GameCreator.Runtime.Quests
{
    [Serializable]
    public abstract class TEventOnQuest : VisualScripting.Event
    {
        [SerializeField] private PropertyGetGameObject m_Journal = GetGameObjectPlayer.Create();
        [SerializeField] private CompareQuestOrAny m_Quest = new CompareQuestOrAny();

        // PROPERTIES: ----------------------------------------------------------------------------
        
        protected Journal Journal { get; private set; }
        private Args Args { get; set; }
        
        // INITIALIZERS: --------------------------------------------------------------------------

        protected override void OnStart(Trigger trigger)
        {
            base.OnStart(trigger);
            
            this.Journal = this.m_Journal.Get<Journal>(trigger);
            if (this.Journal == null) return;

            this.Args = new Args(this.Self, this.Journal.gameObject);
            this.Subscribe();
        }

        protected override void OnEnable(Trigger trigger)
        {
            base.OnEnable(trigger);

            this.Journal = this.m_Journal.Get<Journal>(trigger);
            if (this.Journal == null) return;

            this.Args = new Args(this.Self, this.Journal.gameObject);
            this.Subscribe();
        }

        protected override void OnDisable(Trigger trigger)
        {
            base.OnDisable(trigger);
            
            if (this.Journal == null) return;
            this.Unsubscribe();
        }
        
        // PROTECTED METHODS: ---------------------------------------------------------------------
        
        protected void OnChange(Quest quest)
        {
            if (!this.m_Quest.Match(quest, this.Args)) return;
            _ = this.m_Trigger.Execute(this.Args);
        }
        
        // ABSTRACT METHODS: ----------------------------------------------------------------------

        protected abstract void Subscribe();
        protected abstract void Unsubscribe();
    }
}