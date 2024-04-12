using System;
using System.Collections.Generic;
using UnityEngine;

namespace NGS.AdvancedCullingSystem.Dynamic
{
    [Serializable]
    public class DC_RendererSourceSettingsStrategy : IDC_SourceSettingsStrategy
    {
        [field : SerializeField]
        public int Layer { get; set; }

        [field: SerializeField]
        public bool Baked { get; private set; }

        [SerializeField]
        private GameObject _go;

        [SerializeField]
        private MeshRenderer _renderer;

        [SerializeField]
        private Mesh _mesh;

        [SerializeField]
        private MeshCollider _collider;


        public DC_RendererSourceSettingsStrategy(GameObject go)
        {
            _go = go;
        }

        public bool TryGetBounds(ref Bounds bounds)
        {
            if (_renderer != null)
            {
                bounds = _renderer.bounds;
                return true;
            }

            return false;
        }

        public ICullingTarget CreateCullingTarget(CullingMethod cullingMethod)
        {
            if (cullingMethod == CullingMethod.KeepShadows)
                return new DC_RendererShadowsTarget(_renderer);

            return new DC_RendererTarget(_renderer);
        }

        public IEnumerable<Collider> GetColliders()
        {
            if (_collider == null)
                yield break;

            yield return _collider;
        }


        public bool CheckCompatibilityAndGetComponents(out string incompatibilityReason)
        {
            if (_renderer == null)
            {
                if (!_go.TryGetComponent(out _renderer))
                {
                    incompatibilityReason = "MeshRenderer not found";
                    return false;
                }
            }

            if (_mesh == null)
            {
                MeshFilter filter = _go.GetComponent<MeshFilter>();

                if (filter == null)
                {
                    incompatibilityReason = "MeshFilter not found";
                    return false;
                }

                _mesh = filter.sharedMesh;

                if (_mesh == null)
                {
                    incompatibilityReason = "Mesh not found";
                    return false;
                }
            }

            incompatibilityReason = "";
            return true;
        }

        public void Bake()
        {
            if (Baked)
                return;

            GameObject go = new GameObject("DC_Collider");

            go.transform.parent = _renderer.transform;
            go.layer = Layer;

            go.transform.localPosition = Vector3.zero;
            go.transform.localEulerAngles = Vector3.zero;
            go.transform.localScale = Vector3.one;

            _collider = go.AddComponent<MeshCollider>();
            _collider.sharedMesh = _mesh;

            Baked = true;
        }

        public void ClearBakedData()
        {
            if (!Baked)
                return;

            if (_collider != null)
                UnityEngine.Object.DestroyImmediate(_collider.gameObject);

            _collider = null;

            Baked = false;
        }
    }
}
