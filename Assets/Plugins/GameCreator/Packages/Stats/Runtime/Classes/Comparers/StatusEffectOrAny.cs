using System;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace GameCreator.Runtime.Stats
{
    [Serializable]
    public class StatusEffectOrAny
    {
        private enum Option
        {
            Any = 0,
            StatusEffect = 1
        }
        
        // MEMBERS: -------------------------------------------------------------------------------

        [SerializeField] private Option m_Option = Option.Any;
        [SerializeField] private StatusEffect m_StatusEffect;
        
        // PROPERTIES: ----------------------------------------------------------------------------

        public bool Any => this.m_Option == Option.Any;

        // CONSTRUCTORS: --------------------------------------------------------------------------

        public StatusEffectOrAny()
        { }

        public StatusEffectOrAny(StatusEffect statusEffect) : this()
        {
            this.m_Option = Option.StatusEffect;
            this.m_StatusEffect = statusEffect;
        }
        
        // PUBLIC METHODS: ------------------------------------------------------------------------

        public bool Match(IdString statusEffectID)
        {
            if (this.Any) return true;
            
            if (this.m_StatusEffect == null) return false;
            return this.m_StatusEffect.ID.Hash == statusEffectID.Hash;
        }
    }
}
