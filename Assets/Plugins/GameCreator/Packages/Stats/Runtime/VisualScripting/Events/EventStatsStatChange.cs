using System;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

namespace GameCreator.Runtime.Stats
{
    [Title("On Stat Change")]
    [Category("Stats/On Stat Change")]
    [Description("Executed when the value of a specific game object's Stat is modified. Including due to Stat Modifiers")]

    [Image(typeof(IconStat), ColorTheme.Type.Red)]

    [Parameter("Target", "The targeted game object with a Traits component")]
    [Parameter("When", "Determines if the event executes when the Stat increases, decreases or both")]
    [Parameter("Stat", "The Stat from which the event detects its changes")]
    
    [Keywords("Health", "HP", "Mana", "MP", "Stamina")]

    [Serializable]
    public class EventStatsStatChange : VisualScripting.Event
    {
        private enum DetectionType
        {
            OnChange,
            OnIncrease,
            OnDecrease
        }

        // EXPOSED MEMBERS: -----------------------------------------------------------------------

        [SerializeField] private PropertyGetGameObject m_Target = GetGameObjectPlayer.Create();

        [SerializeField] private DetectionType m_When = DetectionType.OnChange;

        [SerializeField] private Stat m_Stat;

        // MEMBERS: -------------------------------------------------------------------------------

        [NonSerialized] private Traits m_TargetTraits;
        [NonSerialized] private double m_LastValue;

        // INITIALIZERS: --------------------------------------------------------------------------

        protected override void OnStart(Trigger trigger)
        {
            base.OnStart(trigger);
            if (this.m_Stat == null) return;

            if (this.m_TargetTraits != null)
            {
                this.m_TargetTraits.EventChange -= this.OnChange;
            }
            
            GameObject target = this.m_Target.Get(trigger.gameObject);
            if (target == null) return;

            this.m_TargetTraits = target.Get<Traits>();
            if (this.m_TargetTraits == null) return;
            this.m_TargetTraits.EventChange += this.OnChange;
            
            if (this.m_Stat == null) return;
            this.m_LastValue = this.m_TargetTraits.RuntimeStats.Get(this.m_Stat.ID).Value;
        }

        protected override void OnEnable(Trigger trigger)
        {
            base.OnEnable(trigger);
            if (this.m_Stat == null) return;

            GameObject target = this.m_Target.Get(trigger.gameObject);
            if (target == null) return;

            this.m_TargetTraits = target.Get<Traits>();
            if (this.m_TargetTraits == null) return;
            this.m_TargetTraits.EventChange += this.OnChange;
            
            if (this.m_Stat == null) return;
            this.m_LastValue = this.m_TargetTraits.RuntimeStats.Get(this.m_Stat.ID).Value;
        }

        protected override void OnDisable(Trigger trigger)
        {
            base.OnDisable(trigger);
            
            if (this.m_TargetTraits == null) return;
            this.m_TargetTraits.EventChange -= this.OnChange;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void OnChange()
        {
            if (this.m_Stat == null) return;
            
            double nextValue = this.m_TargetTraits.RuntimeStats.Get(this.m_Stat.ID).Value;
            double prevValue = this.m_LastValue;
            
            this.m_LastValue = nextValue;
            
            if (Math.Abs(nextValue - prevValue) < float.Epsilon) return;
            if (this.m_When == DetectionType.OnIncrease && nextValue <= prevValue) return;
            if (this.m_When == DetectionType.OnDecrease && nextValue >= prevValue) return;
            
            this.m_Trigger.Execute(this.m_TargetTraits.gameObject);
        }
    }
}