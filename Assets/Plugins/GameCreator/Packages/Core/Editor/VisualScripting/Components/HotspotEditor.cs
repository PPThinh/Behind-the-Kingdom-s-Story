using GameCreator.Editor.Common;
using UnityEditor;
using GameCreator.Runtime.VisualScripting;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEngine;

namespace GameCreator.Editor.VisualScripting
{
    [CustomEditor(typeof(Hotspot))]
    public class HotspotEditor : UnityEditor.Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            VisualElement container = new VisualElement();

            container.Add(new PropertyField(this.serializedObject.FindProperty("m_Target")));

            SerializedProperty withFocus = this.serializedObject.FindProperty("m_WithFocus");
            PropertyField fieldWithFocus = new PropertyField(withFocus);
            
            container.Add(new SpaceSmaller());
            container.Add(fieldWithFocus);
            
            container.Add(new SpaceSmall());
            container.Add(new PropertyField(this.serializedObject.FindProperty("m_Radius")));
            container.Add(new PropertyField(this.serializedObject.FindProperty("m_Offset")));

            container.Add(new SpaceSmaller());
            container.Add(new PropertyField(this.serializedObject.FindProperty("m_Spots")));
            
            fieldWithFocus.SetEnabled(!EditorApplication.isPlayingOrWillChangePlaymode);

            return container;
        }
        
        // CREATION MENU: -------------------------------------------------------------------------
        
        [MenuItem("GameObject/Game Creator/Visual Scripting/Hotspot", false, 0)]
        public static void CreateElement(MenuCommand menuCommand)
        {
            GameObject instance = new GameObject("Hotspot");
            instance.AddComponent<Hotspot>();
            
            GameObjectUtility.SetParentAndAlign(instance, menuCommand?.context as GameObject);

            Undo.RegisterCreatedObjectUndo(instance, $"Create {instance.name}");
            Selection.activeObject = instance;
        }
    }
}