using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.AnimatedValues;

namespace NGS.AdvancedCullingSystem.Dynamic
{
    public abstract class DC_BaseSelector
    {
        protected static GUIStyle FoldoutGUIStyle { get; private set; }
        protected static GUIStyle ButtonGUIStyle { get; private set; }
        protected static readonly Color GreenColor = new Color(0.3f, 1f, 0.3f, 1f);
        protected static readonly Color RedColor = new Color(1f, 0.3f, 0.3f, 1f);

        public string Label { get; protected set; }
        public abstract bool IsAvailable { get; }

        protected DC_Controller Controller
        {
            get
            {
                return _parent.Controller;
            }
        }

        private DC_SelectionTool _parent;
        private AnimBool _foldout;


        public DC_BaseSelector(DC_SelectionTool parent)
        {
            Label = "Default Label";

            _parent = parent;

            _foldout = new AnimBool(false);
            _foldout.valueChanged.AddListener(parent.Repaint);

            Refresh();
        }

        public abstract void Refresh();

        public virtual void OnInspectorGUI(ref bool sceneChanged)
        {
            if (FoldoutGUIStyle == null || ButtonGUIStyle == null)
                CreateGUIStyles();

            _foldout.target = EditorGUILayout.Foldout(_foldout.target, Label, true, FoldoutGUIStyle);

            if (EditorGUILayout.BeginFadeGroup(_foldout.faded))
            {
                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
                DrawContent(ref sceneChanged);
            }

            EditorGUILayout.EndFadeGroup();

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        }

        public abstract void OnDrawGizmos();


        protected abstract void DrawContent(ref bool sceneChanged);

        protected void DrawBounds(Bounds[] bounds, Color color)
        {
            Gizmos.color = color;
            for (int i = 0; i < bounds.Length; i++)
                Gizmos.DrawWireCube(bounds[i].center, bounds[i].size);
        }

        private void CreateGUIStyles()
        {
            FoldoutGUIStyle = new GUIStyle(EditorStyles.foldout);
            FoldoutGUIStyle.fontSize = 15;
            FoldoutGUIStyle.fontStyle = FontStyle.Bold;
            FoldoutGUIStyle.normal.textColor = Color.white;

            ButtonGUIStyle = new GUIStyle(GUI.skin.button);
            ButtonGUIStyle.fontSize = 12;
            ButtonGUIStyle.fixedHeight = 30;
            ButtonGUIStyle.margin = new RectOffset(5, 5, 5, 5);
            ButtonGUIStyle.border = new RectOffset(0, 0, 0, 0);
            ButtonGUIStyle.padding = new RectOffset(5, 5, 5, 5);
        }
    }

    public abstract class DC_SelectorTemplate1 : DC_BaseSelector
    {
        protected DC_SelectorTemplate1(DC_SelectionTool parent)
            : base(parent)
        {

        }

        protected override void DrawContent(ref bool sceneChanged)
        {
            Color backColor = GUI.backgroundColor;
            bool picked = Selection.gameObjects.Length > 1 ||
                (Selection.gameObjects.Length == 1 && Selection.activeGameObject != Controller.gameObject);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical();

            GUI.backgroundColor = GreenColor;
            if (GUILayout.Button("Assign Auto", ButtonGUIStyle))
            {
                AssignAuto();
                sceneChanged = true;
            }
            GUI.backgroundColor = backColor;

            EditorGUI.BeginDisabledGroup(!picked);

            if (GUILayout.Button("Assign Selected", ButtonGUIStyle))
            {
                AssignSelectedGameObjects();
                sceneChanged = true;
            }

            EditorGUI.EndDisabledGroup();

            EditorGUILayout.EndVertical();

            if (!ShouldDrawClearOptions())
            {
                EditorGUILayout.EndHorizontal();
                return;
            }

            EditorGUILayout.BeginVertical();

            GUI.backgroundColor = RedColor;
            if (GUILayout.Button("Clear All", ButtonGUIStyle))
            {
                ClearAll();
                sceneChanged = true;
            }
            GUI.backgroundColor = backColor;

            EditorGUI.BeginDisabledGroup(!picked);

            if (GUILayout.Button("Clear Selected", ButtonGUIStyle))
            {
                ClearSelected();
                sceneChanged = true;
            }

            EditorGUI.EndDisabledGroup();

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Select", ButtonGUIStyle))
                Select();
        }


        protected void AssignAuto()
        {
            GameObject[] gos = Object.FindObjectsOfType<GameObject>();

            int count = 0;

            for (int i = 0; i < gos.Length; i++)
            {
                GameObject go = gos[i];

                if (AssignIteration(go))
                    count++;
            }

            Debug.Log("Assigned " + count + " objects");
        }

        protected void AssignSelectedGameObjects()
        {
            GameObject[] gos = Selection.gameObjects
                .SelectMany(go => go.GetComponentsInChildren<Transform>()
                .Select(t => t.gameObject))
                .ToArray();

            int count = 0;

            for (int i = 0; i < gos.Length; i++)
            {
                GameObject go = gos[i];

                if (AssignIteration(go))
                    count++;
            }

            Debug.Log("Assigned " + count + " objects");
        }

        protected void ClearSelected()
        {
            GameObject[] gos = Selection.gameObjects
               .SelectMany(go => go.GetComponentsInChildren<Transform>()
               .Select(t => t.gameObject))
               .ToArray();

            int count = 0;

            for (int i = 0; i < gos.Length; i++)
            {
                GameObject go = gos[i];

                if (ClearIteration(go))
                    count++;
            }

            Debug.Log("Cleared " + count + " objects");
        }


        protected abstract bool ShouldDrawClearOptions();

        protected abstract bool AssignIteration(GameObject current);

        protected abstract bool ClearIteration(GameObject current);

        protected abstract void ClearAll();

        protected abstract void Select();
    }

    public class DC_CamerasSelector : DC_SelectorTemplate1
    {
        public override bool IsAvailable
        {
            get
            {
                return !Application.isPlaying;
            }
        }
        private DC_Camera[] _cameras;


        public DC_CamerasSelector(DC_SelectionTool parent) : base(parent)
        {

        }

        public override void Refresh()
        {
            _cameras = Object.FindObjectsOfType<DC_Camera>();

            Label = "Cameras (" + _cameras.Length + ")"; 
        }

        public override void OnDrawGizmos()
        {
            
        }


        protected override bool ShouldDrawClearOptions()
        {
            return _cameras.Length > 0;
        }

        protected override bool AssignIteration(GameObject current)
        {
            if (current.TryGetComponent(out DC_Camera cullingCamera))
                return false;

            if (current.TryGetComponent(out Camera camera))
            {
                Controller.AddCamera(camera, 1500);
                return true;
            }

            return false;
        }

        protected override void ClearAll()
        {
            for (int i = 0; i < _cameras.Length; i++)
            {
                Object.DestroyImmediate(_cameras[i]);
            }
        }

        protected override bool ClearIteration(GameObject current)
        {
            if (current.TryGetComponent(out DC_Camera camera))
            {
                Object.DestroyImmediate(camera);
                return true;
            }

            return false;
        }

        protected override void Select()
        {
            Selection.objects = _cameras.Select(c => c.gameObject).ToArray();
        }
    }

    public class DC_RenderersSelector : DC_SelectorTemplate1
    {
        public override bool IsAvailable
        {
            get
            {
                return !Application.isPlaying;
            }
        }

        private DC_SourceSettings[] _renderersSettings;
        private Bounds[] _bounds;
        private bool _drawGizmos;

        public DC_RenderersSelector(DC_SelectionTool parent) 
            : base(parent)
        {

        }

        public override void Refresh()
        {
            _renderersSettings = Object.FindObjectsOfType<DC_SourceSettings>()
                .Where(s => s.SourceType == SourceType.SingleMesh && s.ControllerID == Controller.ControllerID)
                .ToArray();

            Bounds bounds = default;
            _bounds = _renderersSettings
                .Where(s => s.TryGetBounds(ref bounds))
                .Select(r => bounds)
                .ToArray();

            Label = "Renderers (" + _renderersSettings.Length + ")";
        }

        protected override void DrawContent(ref bool sceneChanged)
        {
            _drawGizmos = EditorGUILayout.Toggle("Draw Gizmos", _drawGizmos);

            base.DrawContent(ref sceneChanged);
        }

        public override void OnDrawGizmos()
        {
            if (!_drawGizmos || _bounds == null)
                return;

            DrawBounds(_bounds, Color.blue);
        }


        protected override bool ShouldDrawClearOptions()
        {
            return _renderersSettings.Length > 0;
        }

        protected override bool AssignIteration(GameObject current)
        {
            if (current.GetComponent<DC_SourceSettings>() != null)
                return false;

            if (current.GetComponent<DC_Occluder>() != null)
                return false;

            MeshRenderer renderer = current.GetComponent<MeshRenderer>();

            if (renderer == null)
                return false;

            LODGroup group = current.GetComponentInParent<LODGroup>();

            if (group != null && group.GetLODs().ContainsAny(r => r == renderer))
                return false;

            Controller.AddObjectForCulling(renderer).CheckCompatibility();

            return true;
        }

        protected override bool ClearIteration(GameObject current)
        {
            if (current.TryGetComponent(out DC_SourceSettings settings))
            {
                if (settings.SourceType != SourceType.SingleMesh)
                    return false;

                if (settings.ControllerID != Controller.ControllerID)
                    return false;

                settings.ClearBakedData();

                Object.DestroyImmediate(settings);

                return true;
            }

            return false;
        }

        protected override void ClearAll()
        {
            for (int i = 0; i < _renderersSettings.Length; i++)
            {
                _renderersSettings[i].ClearBakedData();

                Object.DestroyImmediate(_renderersSettings[i]);
            }
        }

        protected override void Select()
        {
            Selection.objects = _renderersSettings.Select(s => s.gameObject).ToArray();
        }
    }

    public class DC_LODGroupsSelector : DC_SelectorTemplate1
    {
        public override bool IsAvailable
        {
            get
            {
                return !Application.isPlaying;
            }
        }

        private DC_SourceSettings[] _settings;
        private Bounds[] _bounds;
        private bool _drawGizmos;

        public DC_LODGroupsSelector(DC_SelectionTool parent) : base(parent)
        {

        }

        public override void Refresh()
        {
            _settings = Object.FindObjectsOfType<DC_SourceSettings>()
                .Where(s => s.SourceType == SourceType.LODGroup && s.ControllerID == Controller.ControllerID)
                .ToArray();

            Bounds bounds = default;
            _bounds = _settings
                .Where(s => s.TryGetBounds(ref bounds))
                .Select(r => bounds)
                .ToArray();

            Label = "LODGroups (" + _settings.Length + ")";
        }

        protected override void DrawContent(ref bool sceneChanged)
        {
            _drawGizmos = EditorGUILayout.Toggle("Draw Gizmos", _drawGizmos);

            base.DrawContent(ref sceneChanged);
        }

        public override void OnDrawGizmos()
        {
            if (!_drawGizmos || _bounds == null)
                return;

            DrawBounds(_bounds, Color.yellow);
        }


        protected override bool ShouldDrawClearOptions()
        {
            return _settings.Length > 0;
        }

        protected override bool AssignIteration(GameObject current)
        {
            if (current.GetComponent<DC_SourceSettings>() != null)
                return false;

            if (current.GetComponent<DC_Occluder>() != null)
                return false;

            LODGroup group = current.GetComponent<LODGroup>();

            if (group == null)
                return false;

            Controller.AddObjectForCulling(group).CheckCompatibility();

            return true;
        }

        protected override void ClearAll()
        {
            for (int i = 0; i < _settings.Length; i++)
            {
                _settings[i].ClearBakedData();

                Object.DestroyImmediate(_settings[i]);
            }
        }

        protected override bool ClearIteration(GameObject current)
        {
            if (current.TryGetComponent(out DC_SourceSettings settings))
            {
                if (settings.SourceType != SourceType.LODGroup)
                    return false;

                if (settings.ControllerID != Controller.ControllerID)
                    return false;

                settings.ClearBakedData();

                Object.DestroyImmediate(settings);

                return true;
            }

            return false;
        }

        protected override void Select()
        {
            Selection.objects = _settings.Select(s => s.gameObject).ToArray();
        }
    }

    public class DC_OccludersSelector : DC_BaseSelector
    {
        public override bool IsAvailable
        {
            get
            {
                return !Application.isPlaying;
            }
        }

        private DC_Occluder[] _occluders;
        private Bounds[] _bounds;
        private OccluderType _occluderType;
        private bool _drawGizmos;


        public DC_OccludersSelector(DC_SelectionTool parent) : base(parent)
        {

        }

        public override void Refresh()
        {
            _occluders = Object.FindObjectsOfType<DC_Occluder>();

            Bounds bounds = default;
            _bounds = _occluders
                .Where(o => o.TryGetBounds(ref bounds))
                .Select(o => bounds)
                .ToArray();

            Label = "Occluders (" + _occluders.Length + ")";
        }

        protected override void DrawContent(ref bool sceneChanged)
        {
            _drawGizmos = EditorGUILayout.Toggle("Draw Gizmos", _drawGizmos);
            _occluderType = (OccluderType) EditorGUILayout.EnumPopup("Occluder Type", _occluderType);

            Color backColor = GUI.backgroundColor;

            EditorGUILayout.BeginHorizontal();

            GUI.backgroundColor = GreenColor;
            if (GUILayout.Button("Assign Selected", ButtonGUIStyle))
            {
                AssignSelected();
                sceneChanged = true;
            }
            GUI.backgroundColor = backColor;

            if (_occluders.Length == 0)
            {
                EditorGUILayout.EndHorizontal();
                return;
            }

            GUI.backgroundColor = RedColor;
            if (GUILayout.Button("Clear Selected", ButtonGUIStyle))
            {
                ClearSelected();
                sceneChanged = true;
            }
            GUI.backgroundColor = backColor;

            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Clear All", ButtonGUIStyle))
            {
                ClearAll();
                sceneChanged = true;
            }

            if (GUILayout.Button("Select", ButtonGUIStyle))
                Select();
        }

        public override void OnDrawGizmos()
        {
            if (!_drawGizmos || _bounds == null)
                return;

            DrawBounds(_bounds, Color.green);
        }


        private void AssignSelected()
        {
            GameObject[] gos = Selection.gameObjects
                .SelectMany(go => go.GetComponentsInChildren<Transform>()
                .Select(t => t.gameObject))
                .ToArray();

            int count = 0;

            for (int i = 0; i < gos.Length; i++)
            {
                GameObject go = gos[i];

                if (go.TryGetComponent(out DC_SourceSettings settings))
                    continue;

                if (_occluderType == OccluderType.Collider)
                {
                    if (go.GetComponent<Collider>() != null)
                    {
                        go.AddComponent<DC_Occluder>();
                        count++;
                    }
                }
                else if (_occluderType == OccluderType.Mesh)
                {
                    if (go.GetComponent<MeshRenderer>() != null)
                    {
                        go.AddComponent<DC_Occluder>();
                        count++;
                    }
                }
                else
                {
                    if (go.GetComponent<LODGroup>() != null)
                    {
                        go.AddComponent<DC_Occluder>();
                        count++;
                    }
                }
            }

            Debug.Log("Assigned " + count + " objects");
        }

        private void ClearSelected()
        {
            GameObject[] gos = Selection.gameObjects
               .SelectMany(go => go.GetComponentsInChildren<Transform>()
               .Select(t => t.gameObject))
               .ToArray();

            int count = 0;

            for (int i = 0; i < gos.Length; i++)
            {
                GameObject go = gos[i];

                if (go.TryGetComponent(out DC_Occluder occluder))
                {
                    Object.DestroyImmediate(occluder);
                    count++;
                }
            }

            Debug.Log("Clear " + count + " objects");
        }

        private void ClearAll()
        {
            for (int i = 0; i < _occluders.Length; i++)
                Object.DestroyImmediate(_occluders[i]);
        }

        private void Select()
        {
            Selection.objects = _occluders.Select(o => o.gameObject).ToArray();
        }
    }
    
    public class DC_IncompatiblesSelector : DC_BaseSelector
    {
        public override bool IsAvailable
        {
            get
            {
                return true;
            }
        }

        private DC_SourceSettings[] _settings;

        public DC_IncompatiblesSelector(DC_SelectionTool parent) 
            : base(parent)
        {

        }

        public override void Refresh()
        {
            _settings = Object.FindObjectsOfType<DC_SourceSettings>()
                .Where(s => s.IsIncompatible)
                .ToArray();

            Label = "Incompatible Sources (" + _settings.Length + ")";
        }

        public override void OnInspectorGUI(ref bool sceneChanged)
        {
            if (_settings.Length == 0)
                return;

            base.OnInspectorGUI(ref sceneChanged);
        }

        protected override void DrawContent(ref bool sceneChanged)
        {
            if (_settings.Length == 0)
            {
                EditorGUILayout.HelpBox("Incompatible sources not found :)", MessageType.Info);
                return;
            }

            if (!Application.isPlaying)
            {
                Color back = GUI.backgroundColor;

                GUI.backgroundColor = GreenColor;

                if (GUILayout.Button("Clear All", ButtonGUIStyle))
                {
                    ClearAll();
                    sceneChanged = true;
                }

                GUI.backgroundColor = back;
            }

            if (GUILayout.Button("Select", ButtonGUIStyle))
                Select();

            if (GUILayout.Button("Print", ButtonGUIStyle))
                Print();
        }

        public override void OnDrawGizmos()
        {
            
        }


        private void ClearAll()
        {
            for (int i = 0; i < _settings.Length; i++)
                Object.DestroyImmediate(_settings[i]);
        }

        private void Select()
        {
            Selection.objects = _settings.Select(o => o.gameObject).ToArray();
        }

        private void Print()
        {
            for (int i = 0; i < _settings.Length; i++)
                Debug.Log(_settings[i].gameObject.name + " " + _settings[i].IncompatibilityReason);
        }
    }
}