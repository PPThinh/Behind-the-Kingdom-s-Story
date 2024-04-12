using System;
using System.Collections.Generic;
using UnityEngine;

namespace NGS.AdvancedCullingSystem.Dynamic
{
    public abstract class DC_Source : MonoBehaviour, IHitable
    {
        public float Lifetime
        {
            get
            {
                return _lifetime;
            }
            set
            {
                _lifetime = Mathf.Max(0.01f, value);
            }
        }

        private float _lifetime;
        private float _currentTime;


        private void Update()
        {
            try
            {
                _currentTime += Time.deltaTime;

                if (_currentTime > _lifetime)
                {
                    OnTimeout();
                    enabled = false;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message + ex.StackTrace);
                enabled = false;
            }
        }

        public void OnHit()
        {
            try
            {
                enabled = true;

                _currentTime = 0;

                OnHitInternal();
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message + ex.StackTrace);
                enabled = false;
            }
        }

        public void Enable()
        {
            OnTimeout();

            enabled = true;
        }

        public void Disable()
        {
            OnHitInternal();

            enabled = false;
        }


        public abstract void SetCullingTarget(ICullingTarget target);

        public abstract void RemoveCullingTarget(ICullingTarget target);


        protected abstract void OnHitInternal();

        protected abstract void OnTimeout();
    }
}
