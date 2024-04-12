using System;
using System.Collections.Generic;
using GameCreator.Runtime.Behavior;
using GameCreator.Runtime.Common;
using UnityEditor;
using UnityEngine;

namespace GameCreator.Editor.Behavior
{
    internal class WindowActionPlan : TGraphWindow
    {
        private const string USS_ACTION_PLAN = USS_PATH + "ActionPlan";
        
        // PROPERTIES: ----------------------------------------------------------------------------

        protected override string WindowTitle => "Action Planner";
        protected override Texture WindowIcon => new IconWindowActionPlan(ColorTheme.Type.TextLight).Texture;
        
        public override string AssetName => "Action Plan";
        public override Type AssetType => typeof(ActionPlan);

        protected override IEnumerable<string> ExtraStyleSheets => new[]
        {
            USS_ACTION_PLAN
        };

        // INITIALIZERS: --------------------------------------------------------------------------
        
        [MenuItem("Window/Game Creator/Behavior/Action Plan")]
        public static void Open()
        {
            Graph asset = UnityEditor.Selection.activeObject as Graph;
            Open(asset as ActionPlan);
        }

        public static void Open(ActionPlan actionPlan)
        {
            SetupWindow<WindowActionPlan>(actionPlan);
        }
        
        // OVERRIDE METHODS: ----------------------------------------------------------------------
        
        protected override TGraphTool CreateGraphTool(Graph graph)
        {
            return new ToolActionPlan(graph as ActionPlan, this);
        }

        protected override Graph CreateAsset()
        {
            return CreateInstance<ActionPlan>();
        }
    }
}