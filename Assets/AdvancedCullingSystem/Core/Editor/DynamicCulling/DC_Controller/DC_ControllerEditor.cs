using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace NGS.AdvancedCullingSystem.Dynamic
{
    [CustomEditor(typeof(DC_Controller))]
    public class DC_ControllerEditor : Editor
    {
        private static Dictionary<int, DC_SelectionTool> _indexToSelection;

        private static GUIStyle TitleLabelStyle;
        private static GUIStyle ButtonGUIStyle;

        private new DC_Controller target
        {
            get
            {
                return base.target as DC_Controller;
            }
        }
        private DC_SelectionTool _selectionTool;

        private bool _containsReadyToBake;
        private bool _containsBaked;
        private bool _containsNonReadableMeshes;


        private void OnEnable()
        {
            string layer = DC_Controller.GetCullingLayerName();

            if (!LayersHelper.IsLayerExist(layer))
            {
                LayersHelper.CreateLayer(layer);
                LayersHelper.DisableCollisions(LayerMask.NameToLayer(layer));
            }

            UpdateSelectionTool();
            UpdateSceneInfo();
        }

        public override void OnInspectorGUI()
        {
            if (TitleLabelStyle == null)
                CreateGUIStyles();

            EditorGUILayout.Space();
           
            EditorGUILayout.LabelField("Dynamic Culling", TitleLabelStyle);

            EditorHelper.DrawSeparatorLine(1, 2);

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            EditorGUI.BeginChangeCheck();
            EditorGUI.BeginChangeCheck();

            if (Application.isPlaying && target.MergeInGroups)
            {
                target.DrawGizmos = EditorGUILayout.Toggle("Draw Gizmos", target.DrawGizmos);
                EditorGUILayout.Space();
            }

            EditorGUI.BeginDisabledGroup(Application.isPlaying);

            target.ControllerID = EditorGUILayout.IntField("Controller ID", target.ControllerID);

            if (EditorGUI.EndChangeCheck())
                UpdateSelectionTool();

            target.ObjectsLifetime = EditorGUILayout.FloatField("Objects Lifetime", target.ObjectsLifetime);

            EditorGUILayout.Space();

            bool merge = EditorGUILayout.Toggle("Merge In Groups", target.MergeInGroups);
            float cellSize = target.CellSize;

            if (target.MergeInGroups)
                cellSize = EditorGUILayout.FloatField("Cell Size", target.CellSize);

            if (!Application.isPlaying)
            {
                target.MergeInGroups = merge;
                target.CellSize = cellSize;
            }

            EditorGUI.EndDisabledGroup();

            if (EditorGUI.EndChangeCheck() && !Application.isPlaying)
                EditorUtility.SetDirty(base.target);

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            if (_selectionTool.Controller.gameObject != null)
            {
                bool sceneChanged = false;

                _selectionTool.OnInspectorGUI(ref sceneChanged);

                if (sceneChanged)
                    UpdateSceneInfo();
            }

            if (Application.isPlaying)
                return;

            EditorGUILayout.BeginHorizontal();

            if (_containsReadyToBake)
            {
                if (GUILayout.Button("Bake", ButtonGUIStyle))
                {
                    DC_ControllerEditorUtil.BakeScene();
                    UpdateSceneInfo();
                }
            }

            if (_containsBaked)
            {
                if (GUILayout.Button("Clear", ButtonGUIStyle))
                {
                    DC_ControllerEditorUtil.ClearBakedData();
                    UpdateSceneInfo();
                }
            }

            EditorGUILayout.EndHorizontal();

            if (_containsNonReadableMeshes)
            {
                EditorGUILayout.Space();

                if (GUILayout.Button("Make Meshes Readable", ButtonGUIStyle))
                {
                    DC_ControllerEditorUtil.MakeMeshesReadable();
                    UpdateSceneInfo();
                }
            }
        }


        private void UpdateSelectionTool()
        {
            if (_indexToSelection == null)
                _indexToSelection = new Dictionary<int, DC_SelectionTool>();

            if (!_indexToSelection.TryGetValue(target.ControllerID, out _selectionTool))
            {
                _selectionTool = new DC_SelectionTool(target);
                _indexToSelection.Add(target.ControllerID, _selectionTool);
            }

            _selectionTool.Initialize(target, this);
            _selectionTool.Refresh();
        }

        private void UpdateSceneInfo()
        {
            DC_SourceSettings[] settings = FindObjectsOfType<DC_SourceSettings>();

            _containsReadyToBake = DC_ControllerEditorUtil.ContainsReadyToBakeSources(settings);
            _containsBaked = DC_ControllerEditorUtil.ContainsBakedSources(settings);
            _containsNonReadableMeshes = DC_ControllerEditorUtil.ContainsNonReadableMeshes(settings);
        }

        private void CreateGUIStyles()
        {
            if (TitleLabelStyle == null)
            {
                TitleLabelStyle = new GUIStyle();
                TitleLabelStyle.fontSize = 24;
                TitleLabelStyle.fontStyle = FontStyle.Bold;
                TitleLabelStyle.alignment = TextAnchor.MiddleLeft;
                TitleLabelStyle.normal.textColor = Color.white;
            }

            if (ButtonGUIStyle == null)
            {
                ButtonGUIStyle = new GUIStyle(GUI.skin.button);
                ButtonGUIStyle.fontSize = 12;
                ButtonGUIStyle.fixedHeight = 24;
                ButtonGUIStyle.margin = new RectOffset(5, 5, 5, 5);
                ButtonGUIStyle.border = new RectOffset(0, 0, 0, 0);
                ButtonGUIStyle.padding = new RectOffset(5, 5, 5, 5);
            }
        }


        [MenuItem("Tools/NGSTools/Advanced Culling System/Dynamic")]
        private static void CreateDynamicCulling()
        {
            GameObject go = new GameObject("Dynamic Culling");

            go.AddComponent<DC_Controller>();

            Selection.activeGameObject = go;
        }

        [DrawGizmo(GizmoType.Selected | GizmoType.NonSelected)]
        private static void OnDrawGizmos(DC_Controller controller, GizmoType gizmoType)
        {
            if (_indexToSelection == null)
                _indexToSelection = new Dictionary<int, DC_SelectionTool>();

            if (_indexToSelection.TryGetValue(controller.ControllerID, out DC_SelectionTool tool))
                tool.OnDrawGizmos();
        }
    }
}
