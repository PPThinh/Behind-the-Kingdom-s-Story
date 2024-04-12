using System;
using System.Collections.Generic;
using UnityEngine;

namespace NGS.AdvancedCullingSystem.Dynamic
{
    public class DC_Controller : MonoBehaviour
    {
        private static Dictionary<int, DC_Controller> _controllersDic;
        private static Dictionary<Collider, IHitable> _hitablesDic;

        public int ControllerID
        {
            get
            {
                return _controllerID;
            }
            set
            {
                _controllerID = value;
            }
        }
        public float ObjectsLifetime
        {
            get
            {
                return _objectsLifetime;
            }
            set
            {
                _objectsLifetime = Mathf.Max(0.1f, value);
            }
        }
        public bool MergeInGroups
        {
            get
            {
                return _mergeInGroups;
            }
            set
            {
                if (_sourcesProvider != null)
                {
                    Debug.Log("You can set 'MergeInGroups' option only before initialized");
                    return;
                }

                _mergeInGroups = value;
            }
        }
        public float CellSize
        {
            get
            {
                return _cellSize;
            }

            set
            {
                if (_sourcesProvider != null)
                {
                    Debug.Log("You can set 'Cell Size' option only before initialized");
                    return;
                }

                _cellSize = Mathf.Max(value, 0.1f);
            }
        }
        public bool DrawGizmos { get; set; }

        [SerializeField]
        private int _controllerID;

        [SerializeField, Min(0.1f)]
        private float _objectsLifetime = 2f;

        [SerializeField]
        private bool _mergeInGroups = true;

        [SerializeField]
        private float _cellSize = 10f;

        private IDC_SourcesProvider _sourcesProvider;
        private BinaryTreeDrawer _treeDrawer;


        private void Awake()
        {
            if (_controllersDic == null)
                _controllersDic = new Dictionary<int, DC_Controller>();

            if (_hitablesDic == null)
                _hitablesDic = new Dictionary<Collider, IHitable>();

            if (!_controllersDic.ContainsKey(_controllerID))
                _controllersDic.Add(_controllerID, this);
            else
                Debug.Log("DynamicCullingController with id : " + _controllerID + " already exists!");

            if (_mergeInGroups)
            {
                _sourcesProvider = new DC_SourcesTree(_cellSize);
                _treeDrawer = new BinaryTreeDrawer();
            }
            else
            {
                _sourcesProvider = new DC_SingleSourcesProvider();
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (!DrawGizmos)
                return;

            if (_treeDrawer == null)
                return;

            DC_SourcesTree tree = _sourcesProvider as DC_SourcesTree;

            if (tree.Root == null)
                return;

            _treeDrawer.Color = Color.white;
            _treeDrawer.DrawTreeGizmos(tree.Root);
        }

        private void OnDestroy()
        {
            _controllersDic.Remove(_controllerID);
        }


        public DC_Camera AddCamera(Camera camera, int raysPerFrame)
        {
            if (camera.TryGetComponent(out DC_Camera cullingCamera))
            {
                Debug.Log(camera.name + " already has DynamicCullingCamera component");
            }
            else
            {
                cullingCamera = camera.gameObject.AddComponent<DC_Camera>();
                cullingCamera.SetRaysCount(raysPerFrame);
            }

            return cullingCamera;
        }

        public DC_SourceSettings AddObjectForCulling(MeshRenderer renderer, 
            CullingMethod cullingMethod = CullingMethod.FullDisable)
        {
            DC_SourceSettings settings = renderer.gameObject.AddComponent<DC_SourceSettings>();

            settings.ControllerID = _controllerID;
            settings.SourceType = SourceType.SingleMesh;
            settings.CullingMethod = cullingMethod;

            return settings;
        }

        public DC_SourceSettings AddObjectForCulling(LODGroup lodGroup, 
            CullingMethod cullingMethod = CullingMethod.FullDisable)
        {
            DC_SourceSettings settings = lodGroup.gameObject.AddComponent<DC_SourceSettings>();

            settings.ControllerID = _controllerID;
            settings.SourceType = SourceType.LODGroup;
            settings.CullingMethod = cullingMethod;

            return settings;
        }

        public void AddObjectForCulling(ICullingTarget cullingTarget, IEnumerable<Collider> colliders)
        {
            DC_Source source = _sourcesProvider.GetSource(cullingTarget);

            source.Lifetime = _objectsLifetime;
            source.transform.parent = transform;

            DC_CullingTargetObserver observer = cullingTarget.GameObject.AddComponent<DC_CullingTargetObserver>();
            observer.Initialize(source, cullingTarget);

            foreach (var collider in colliders)
                _hitablesDic.Add(collider, source);
        }


        public static DC_Controller GetById(int id)
        {
            return _controllersDic[id];
        }

        public static int GetCullingLayer()
        {
            return LayerMask.NameToLayer(GetCullingLayerName());
        }

        public static string GetCullingLayerName()
        {
            return "ACSCulling";
        }

        public static IReadOnlyDictionary<Collider, IHitable> GetHitables()
        {
            return _hitablesDic;
        }
    }
}
