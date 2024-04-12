using System;
using System.Collections.Generic;
using UnityEngine;

using UnityEditor;

namespace NGS.AdvancedCullingSystem.Dynamic
{
    public enum CullingMethod { FullDisable, KeepShadows }
    public enum SourceType { SingleMesh, LODGroup } 

    [DisallowMultipleComponent]
    public class DC_SourceSettings : MonoBehaviour
    {
        [field : SerializeField]
        public int ControllerID { get; set; }

        [field : SerializeField]
        public SourceType SourceType { get; set; }

        [field : SerializeField]
        public CullingMethod CullingMethod { get; set; }

        [field: SerializeField]
        public bool IsIncompatible { get; private set; }

        [field : SerializeField]
        public string IncompatibilityReason { get; private set; }

        public bool Baked
        {
            get
            {
                return _strategy != null && _strategy.Baked;
            }
        }

        [SerializeReference]
        private IDC_SourceSettingsStrategy _strategy;


        private void Reset()
        {
            if (GetComponent<LODGroup>() != null)
                SourceType = SourceType.LODGroup;

            CheckCompatibility();
        }

        private void Start()
        {
            try
            {
                if (_strategy == null)
                    CreateStrategy();

                if (!CheckCompatibility())
                {
                    enabled = false;
                    return;
                }

                if (!_strategy.Baked)
                    _strategy.Bake();

                DC_Controller.GetById(ControllerID).AddObjectForCulling(
                    _strategy.CreateCullingTarget(CullingMethod),
                    _strategy.GetColliders());

                Destroy(this);
            }
            catch (Exception ex)
            {
                IsIncompatible = true;
                IncompatibilityReason = ex.Message + ex.StackTrace;
            }
        }

        private void CreateStrategy()
        {
            if (SourceType == SourceType.SingleMesh)
            {
                _strategy = new DC_RendererSourceSettingsStrategy(gameObject);
            }
            else
            {
                _strategy = new DC_LODGroupSourceSettingsStrategy(gameObject);
            }

            _strategy.Layer = DC_Controller.GetCullingLayer();
        }


        public bool TryGetBounds(ref Bounds bounds)
        {
            if (_strategy == null)
                return false;

            return _strategy.TryGetBounds(ref bounds);
        }

        public bool CheckCompatibility()
        {
            if (_strategy == null)
                CreateStrategy();

            IsIncompatible = !_strategy.CheckCompatibilityAndGetComponents(out string reason);
            IncompatibilityReason = reason;

            return !IsIncompatible;
        }

        public void Bake()
        {
            if (Application.isPlaying)
            {
                Debug.Log("'Bake' can only be called in editor mode");
                return;
            }

            _strategy?.ClearBakedData();

            if (CheckCompatibility())
                _strategy.Bake();
        }

        public void ClearBakedData()
        {
            if (Application.isPlaying)
            {
                Debug.Log("'ClearBakedData' can only be called in editor mode");
                return;
            }

            if (_strategy == null)
                return;

            _strategy.ClearBakedData();
        }
    }
}
