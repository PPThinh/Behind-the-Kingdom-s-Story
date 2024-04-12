using GameCreator.Runtime.Stats;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace GameCreator.Editor.Stats
{
    [CustomPropertyDrawer(typeof(StatusEffectSelector))]
    public class StatusEffectSelectorDrawer : PropertyDrawer
    {
        private const string EMPTY_LABEL = " ";
        
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            VisualElement root = new VisualElement();
            VisualElement head = new VisualElement();
            VisualElement body = new VisualElement();

            SerializedProperty statusEffect = property.FindPropertyRelative("m_StatusEffect");
            SerializedProperty asset = property.FindPropertyRelative("m_Asset");
            
            PropertyField fieldStatusEffect = new PropertyField(statusEffect, property.displayName);
            PropertyField fieldAsset = new PropertyField(asset, EMPTY_LABEL);
            
            head.Add(fieldStatusEffect);
            body.Add(fieldAsset);
            
            fieldStatusEffect.RegisterValueChangeCallback(changeEvent =>
            {
                body.style.display = changeEvent.changedProperty.intValue == 0
                    ? DisplayStyle.Flex
                    : DisplayStyle.None;
            });

            body.style.display = statusEffect.intValue == 0
                ? DisplayStyle.Flex
                : DisplayStyle.None;

            root.Add(head);
            root.Add(body);
            
            return root;
        }
    }
}