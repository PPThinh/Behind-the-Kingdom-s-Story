using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace NGS.AdvancedCullingSystem.Dynamic
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(DC_SourceSettings))]
    public class DC_SourceSettingsEditor : Editor
    {
        private new DC_SourceSettings target
        {
            get
            {
                return base.target as DC_SourceSettings;
            }
        }

        private SerializedProperty _controllerIdProp;
        private SerializedProperty _sourceTypeProp;
        private SerializedProperty _cullingMethodProp;
        private SerializedProperty _isIncompatibleProp;
        private SerializedProperty _incompatibilityReasonProp;


        private void OnEnable()
        {
            _controllerIdProp = serializedObject.FindAutoProperty(nameof(target.ControllerID));
            _sourceTypeProp = serializedObject.FindAutoProperty(nameof(target.SourceType));
            _cullingMethodProp = serializedObject.FindAutoProperty(nameof(target.CullingMethod));
            _isIncompatibleProp = serializedObject.FindAutoProperty(nameof(target.IsIncompatible));
            _incompatibilityReasonProp = serializedObject.FindAutoProperty(nameof(target.IncompatibilityReason));
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawHelpBox();

            if (DrawProperties())
                ApplyModifiedProperties();

            EditorGUILayout.Space();

            if (GUILayout.Button("Check Compatibility"))
                CheckCompatibilities();
        }


        private void DrawHelpBox()
        {
            if (_isIncompatibleProp.boolValue && !_isIncompatibleProp.hasMultipleDifferentValues)
            {
                string text = _incompatibilityReasonProp.stringValue;

                EditorGUILayout.HelpBox(text, MessageType.Warning);

                EditorGUILayout.Space();
            }
        }

        private bool DrawProperties()
        {
            EditorGUI.BeginDisabledGroup(Application.isPlaying);

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.PropertyField(_controllerIdProp);
            EditorGUILayout.PropertyField(_sourceTypeProp);
            EditorGUILayout.PropertyField(_cullingMethodProp);

            EditorGUI.EndDisabledGroup();

            return EditorGUI.EndChangeCheck();
        }

        private void ApplyModifiedProperties()
        {
            Debug.Log("Applied");

            foreach (var target in targets)
            {
                DC_SourceSettings current = target as DC_SourceSettings;

                if (!_controllerIdProp.hasMultipleDifferentValues)
                    current.ControllerID = _controllerIdProp.intValue;

                if (!_sourceTypeProp.hasMultipleDifferentValues)
                    current.SourceType = (SourceType) _sourceTypeProp.enumValueIndex;

                if (!_cullingMethodProp.hasMultipleDifferentValues)
                    current.CullingMethod = (CullingMethod) _cullingMethodProp.enumValueIndex;
            }
        }

        private void CheckCompatibilities()
        {
            foreach (var target in targets)
            {
                DC_SourceSettings current = target as DC_SourceSettings;

                if (current.CheckCompatibility())
                {
                    Debug.Log(current.name + " is compatible!");
                }
                else
                {
                    Debug.Log(current.name + " is incompatible. Reason : " + current.IncompatibilityReason);
                }
            }
        }
    }
}
