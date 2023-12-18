using System;
using GameCreator.Editor.Characters;
using GameCreator.Runtime.Inventory;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace GameCreator.Editor.Inventory
{
    [CustomPropertyDrawer(typeof(EquipmentRuntimeSlot))]
    public class EquipmentRuntimeSlotDrawer : PropertyDrawer
    {
        private const string PROP_BASE = "m_Override";
        private const string PROP_HANDLE = "m_OverrideHandle";

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            return CreatePropertyGUI(property, "Bone");
        }

        public static VisualElement CreatePropertyGUI(SerializedProperty property, string label)
        {
            VisualElement root = new VisualElement();

            SerializedProperty propertyBase = property.FindPropertyRelative(PROP_BASE);
            SerializedProperty propertyHandle = property.FindPropertyRelative(PROP_HANDLE);
            
            var fieldBase = new PropertyField(propertyBase, label);
            VisualElement fieldHandle = BoneDrawer.CreatePropertyGUI(propertyHandle, " ");
            
            root.Add(fieldBase);
            root.Add(fieldHandle);

            fieldBase.RegisterValueChangeCallback(changeEvent =>
            {
                fieldHandle.style.display = changeEvent.changedProperty.boolValue
                    ? DisplayStyle.Flex
                    : DisplayStyle.None;
            });
            
            fieldHandle.style.display = propertyBase.boolValue
                ? DisplayStyle.Flex
                : DisplayStyle.None;
            
            return root;
        }
    }
}