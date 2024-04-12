using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GameCreator.Editor.Common;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace GameCreator.Editor.Behavior
{
    internal class NodeConditionTool : VisualElement
    {
        // MEMBERS: -------------------------------------------------------------------------------
        
        private readonly Image m_Icon;
        private readonly Label m_Text;
        
        // CONSTRUCTOR: ---------------------------------------------------------------------------
        
        public NodeConditionTool()
        {
            this.m_Icon = new Image();
            this.m_Text = new Label();
            
            this.Add(this.m_Icon);
            this.Add(this.m_Text);
        }
        
        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Refresh(SerializedProperty propertyInstruction)
        {
            Condition instance = propertyInstruction.managedReferenceValue as Condition;
                
            IEnumerable<ImageAttribute> iconAttrs = instance?.GetType()
                .GetCustomAttributes<ImageAttribute>();
            Texture2D icon = iconAttrs?.FirstOrDefault()?.Image;

            this.m_Icon.image = icon != null ? icon : Texture2D.whiteTexture; 
            this.m_Text.text = instance?.Title;
        }
    }
}