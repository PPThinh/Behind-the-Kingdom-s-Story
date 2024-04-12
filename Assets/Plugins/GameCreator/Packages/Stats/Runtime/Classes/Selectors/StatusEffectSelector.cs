using System;
using UnityEngine;

namespace GameCreator.Runtime.Stats
{
    [Serializable]
    public class StatusEffectSelector
    {
        private enum Option
        {
            StatusEffect = 0,
            LastAdded    = 1,
            LastRemoved  = 2
        }
        
        // MEMBERS: -------------------------------------------------------------------------------

        [SerializeField] private Option m_StatusEffect = Option.StatusEffect;
        [SerializeField] private StatusEffect m_Asset;
        
        // PROPERTIES: ----------------------------------------------------------------------------

        public StatusEffect Get => this.m_StatusEffect switch
        {
            Option.StatusEffect => this.m_Asset,
            Option.LastAdded => StatusEffect.LastAdded,
            Option.LastRemoved => StatusEffect.LastRemoved,
            _ => throw new ArgumentOutOfRangeException()
        };

        // CONSTRUCTORS: --------------------------------------------------------------------------

        public StatusEffectSelector()
        { }

        public StatusEffectSelector(StatusEffect asset) : this()
        {
            this.m_StatusEffect = Option.StatusEffect;
            this.m_Asset = asset;
        }
        
        // STRING: --------------------------------------------------------------------------------

        public override string ToString()
        {
            return this.m_StatusEffect switch
            {
                Option.StatusEffect => this.m_Asset != null ? this.m_Asset.ID.String : "(none)",
                Option.LastAdded => "Last Added",
                Option.LastRemoved => "Last Removed",
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}
