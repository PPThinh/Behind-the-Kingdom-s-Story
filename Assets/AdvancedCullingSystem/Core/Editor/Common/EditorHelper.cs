using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace NGS.AdvancedCullingSystem
{
    public static class EditorHelper
    {
        public static void DrawSeparatorLine(float thickness, float padding)
        {
            Rect previousRect = GUILayoutUtility.GetLastRect();
            GUILayout.Space(padding);
            EditorGUILayout.LabelField("", GUILayout.Height(thickness));
            Rect lineRect = GUILayoutUtility.GetLastRect();
            lineRect.x = previousRect.x;
            lineRect.width = previousRect.width;
            EditorGUI.DrawRect(lineRect, Color.gray);
            GUILayout.Space(padding);
        }
    }
}
