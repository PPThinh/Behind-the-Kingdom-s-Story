using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

namespace GameCreator.Runtime.Stats
{
    [Title("On Status Effect Change")]
    [Category("Stats/On Status Effect Change")]
    [Description("Executed when a Status Effect is added or removed from a Traits component")]

    [Image(typeof(IconStatusEffect), ColorTheme.Type.Green)]

    [Parameter("Target", "The targeted game object with a Traits component")]
    [Parameter("Status Effect", "Determines if the event detects any Status Effect change or a specific one")]

    [Keywords("Buff", "Debuff", "Enhance", "Ailment")]
    [Keywords(
        "Blind", "Dark", "Burn", "Confuse", "Dizzy", "Stagger", "Fear", "Freeze", "Paralyze", 
        "Shock", "Silence", "Sleep", "Silence", "Slow", "Toad", "Weak", "Strong", "Poison"
    )]
    [Keywords(
        "Haste", "Protect", "Reflect", "Regenerate", "Shell", "Armor", "Shield", "Berserk",
        "Focus", "Raise"
    )]

    [System.Serializable]
    public class EventStatsStatusEffectChange : VisualScripting.Event
    {
        // EXPOSED MEMBERS: -----------------------------------------------------------------------

        [SerializeField] private PropertyGetGameObject m_Target = GetGameObjectPlayer.Create();

        [SerializeField] private StatusEffectOrAny m_StatusEffect = new StatusEffectOrAny();

        // MEMBERS: -------------------------------------------------------------------------------

        private Traits m_TargetTraits;

        // INITIALIZERS: --------------------------------------------------------------------------

        protected override void OnStart(Trigger trigger)
        {
            base.OnStart(trigger);
            
            if (this.m_TargetTraits != null)
            {
                this.m_TargetTraits.RuntimeStatusEffects.EventChange -= this.OnChange;
            }
            
            GameObject target = this.m_Target.Get(trigger.gameObject);
            if (target == null) return;

            this.m_TargetTraits = target.Get<Traits>();
            if (this.m_TargetTraits == null) return;

            this.m_TargetTraits.RuntimeStatusEffects.EventChange += this.OnChange;
        }

        protected override void OnEnable(Trigger trigger)
        {
            base.OnEnable(trigger);

            GameObject target = this.m_Target.Get(trigger.gameObject);
            if (target == null) return;

            this.m_TargetTraits = target.Get<Traits>();
            if (this.m_TargetTraits == null) return;

            this.m_TargetTraits.RuntimeStatusEffects.EventChange += this.OnChange;
        }

        protected override void OnDisable(Trigger trigger)
        {
            base.OnDisable(trigger);
            if (this.m_TargetTraits == null) return;

            this.m_TargetTraits.RuntimeStatusEffects.EventChange -= this.OnChange;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void OnChange(IdString sourceStatusEffectID)
        {
            if (!this.m_StatusEffect.Match(sourceStatusEffectID)) return;
            this.m_Trigger.Execute(this.m_TargetTraits.gameObject);
        }
    }
}