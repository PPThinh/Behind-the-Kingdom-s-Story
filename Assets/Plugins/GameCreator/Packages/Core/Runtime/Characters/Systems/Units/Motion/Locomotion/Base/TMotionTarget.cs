﻿using System;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace GameCreator.Runtime.Characters
{
	public abstract class TMotionTarget<T> : TMotion
	{
		private const float MIN_THRESHOLD = 0.15f;
		
		// MEMBERS: -------------------------------------------------------------------------------
		
		protected T m_Target;

		protected float m_Threshold;

		protected bool m_HasFinished;
		protected Action<Character> m_OnFinish;

        private int m_FacingLayerKey = -1;

        // PROPERTIES: ----------------------------------------------------------------------------

        protected abstract bool HasTarget { get; }
        protected abstract Vector3 Position { get; }
        protected abstract Vector3 Direction { get; }

        // INITIALIZERS: --------------------------------------------------------------------------

        public virtual Character.MovementType Setup(T target, float threshold, 
	        Action<Character> onFinish)
		{
            this.Setup();

			this.m_HasFinished = false;
			this.m_Target = target;

			this.m_Threshold = Math.Max(threshold, MIN_THRESHOLD);
			this.m_OnFinish = onFinish;

			this.Motion.MoveDirection = (this.Position - this.Transform.position).normalized;
			this.Motion.MovePosition = this.Position;

			return Character.MovementType.MoveToPosition;
		}

        // INTERFACE METHODS: ---------------------------------------------------------------------

        public override Character.MovementType Update()
		{
			if (this.m_HasFinished) return Character.MovementType.None;
			
			Vector3 source = this.Character.Feet;
			Vector3 target = this.Position;
			
			float distance = this.HasTarget ? Vector3.Distance(source, target) : 0f;
			float slowdownDistance = this.m_Threshold + this.Motion.Radius * 2f;

			if (this.Direction != Vector3.zero && distance <= slowdownDistance)
            {
                IUnitFacing facing = this.Character.Facing;
                this.m_FacingLayerKey = facing.SetLayerDirection(
                    this.m_FacingLayerKey,
                    this.Direction,
                    true
                );
            }
            
			if (distance <= this.m_Threshold)
			{
				return this.Stop();
			}

            Vector3 direction = target - source;
            if (distance < this.m_Threshold)
            {
                direction = Vector3.zero;
            }

            direction = this.CalculateSpeed(direction);
            direction = this.CalculateAcceleration(direction);
            
            this.Motion.MoveDirection = direction;
			this.Motion.MovePosition = this.Position;

			return direction.sqrMagnitude > float.Epsilon
                ? Character.MovementType.MoveToPosition
                : Character.MovementType.None;
		}

        public override Character.MovementType Stop()
        {
	        Character.MovementType movementType = base.Stop();
	        this.m_OnFinish?.Invoke(this.Character);
	        this.m_HasFinished = true;
	        
	        return movementType;
        }

        public override void OnDrawGizmos()
        {
            base.OnDrawGizmos();

            if (this.m_HasFinished || this.m_Threshold < MIN_THRESHOLD) return;

            Gizmos.color = Color.yellow;
            GizmosExtension.Circle(this.Position, this.m_Threshold);
        }
    }
}