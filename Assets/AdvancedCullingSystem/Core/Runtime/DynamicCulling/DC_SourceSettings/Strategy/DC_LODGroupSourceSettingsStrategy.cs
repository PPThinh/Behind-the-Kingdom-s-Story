using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace NGS.AdvancedCullingSystem.Dynamic
{
    public class DC_LODGroupSourceSettingsStrategy : IDC_SourceSettingsStrategy
    {
        [field : SerializeField]
        public int Layer { get; set; }

        [field: SerializeField]
        public bool Baked { get; private set; }

        [SerializeField]
        private GameObject _go;

        [SerializeField]
        private LODGroup _group;

        [SerializeField]
        private Renderer[] _renderers;

        [SerializeField]
        private MeshCollider[] _colliders;

        [SerializeField]
        private Bounds _bounds;

        public DC_LODGroupSourceSettingsStrategy(GameObject go)
        {
            _go = go;
        }

        public bool TryGetBounds(ref Bounds bounds)
        {
            if (_renderers != null)
            {
                bounds = _bounds;
                return true;
            }    

            return false;
        }

        public ICullingTarget CreateCullingTarget(CullingMethod cullingMethod)
        {
            if (cullingMethod == CullingMethod.KeepShadows)
                return new DC_LODGroupShadowsTarget(_group, _renderers, _bounds);

            return new DC_LODGroupTarget(_group, _renderers, _bounds);
        }

        public IEnumerable<Collider> GetColliders()
        {
            if (_colliders == null)
                yield break;

            for (int i = 0; i < _colliders.Length; i++)
                yield return _colliders[i];
        }


        public bool CheckCompatibilityAndGetComponents(out string incompatibilityReason)
        {
            if (_group == null)
            {
                if (!_go.TryGetComponent(out _group))
                {
                    incompatibilityReason = "LODGroup not found";
                    return false;
                }
            }

            if (_renderers == null)
            {
                LOD[] lods = _group.GetLODs();

                int count = lods.Count(IsCompatibleRenderer);

                if (count == 0)
                {
                    incompatibilityReason = "Can't find any compatible renderer";
                    return false;
                }

                _renderers = new Renderer[count];
                _bounds = new Bounds(_group.transform.position, Vector3.zero);

                int idx = 0;
                for (int i = 0; i < lods.Length; i++)
                {
                    Renderer[] lodRenderers = lods[i].renderers;

                    for (int c = 0; c < lodRenderers.Length; c++)
                    {
                        Renderer renderer = lodRenderers[c];

                        if (renderer != null && IsCompatibleRenderer(renderer))
                        {
                            _renderers[idx++] = renderer;
                            _bounds.Encapsulate(renderer.bounds);
                        }
                    }
                }
            }
            else
            {
                for (int i = 0; i < _renderers.Length; i++)
                {
                    if (_renderers[i] == null)
                    {
                        incompatibilityReason = "Missing renderer at index : " + i;
                        return false;
                    }
                }
            }

            incompatibilityReason = "";
            return true;
        }

        private bool IsCompatibleRenderer(Renderer renderer)
        {
            MeshFilter filter = renderer.GetComponent<MeshFilter>();

            return filter != null && filter.sharedMesh != null;
        }

        public void Bake()
        {
            if (Baked)
                return;

            _colliders = _group.GetLODs()[0].renderers
                .Where(r => (r != null && IsCompatibleRenderer(r)))
                .Select(r =>
                {
                    GameObject go = new GameObject("DC_Collider");

                    go.transform.parent = r.transform;
                    go.layer = Layer;

                    go.transform.localPosition = Vector3.zero;
                    go.transform.localEulerAngles = Vector3.zero;
                    go.transform.localScale = Vector3.one;

                    MeshCollider collider = go.AddComponent<MeshCollider>();
                    collider.sharedMesh = r.GetComponent<MeshFilter>().sharedMesh;

                    return collider;
                }).ToArray();

            Baked = true;
        }

        public void ClearBakedData()
        {
            if (!Baked)
                return;

            for (int i = 0; i < _colliders.Length; i++)
            {
                Collider collider = _colliders[i];

                if (collider == null || collider.gameObject == null)
                    continue;

                UnityEngine.Object.DestroyImmediate(collider.gameObject);
            }
            _colliders = null;

            Baked = false;
        }
    }
}
