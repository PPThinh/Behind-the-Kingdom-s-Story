using System;
using System.Collections.Generic;
using GameCreator.Runtime.Behavior;
using GameCreator.Runtime.Common;
using UnityEditor;
using UnityEngine;

namespace GameCreator.Editor.Behavior
{
    internal class WindowStateMachine : TGraphWindow
    {
        private const string USS_STATE_MACHINE = USS_PATH + "StateMachine";
        
        // PROPERTIES: ----------------------------------------------------------------------------

        protected override string WindowTitle => "State Machine";
        protected override Texture WindowIcon => new IconWindowStateMachine(ColorTheme.Type.TextLight).Texture;
        
        public override string AssetName => "State Machine";
        public override Type AssetType => typeof(StateMachine);

        protected override IEnumerable<string> ExtraStyleSheets => new[]
        {
            USS_STATE_MACHINE
        };

        // INITIALIZERS: --------------------------------------------------------------------------
        
        [MenuItem("Window/Game Creator/Behavior/State Machine")]
        public static void Open()
        {
            Graph asset = UnityEditor.Selection.activeObject as Graph;
            Open(asset as StateMachine);
        }

        public static void Open(StateMachine stateMachine)
        {
            SetupWindow<WindowStateMachine>(stateMachine);
        }
        
        // OVERRIDE METHODS: ----------------------------------------------------------------------
        
        protected override TGraphTool CreateGraphTool(Graph graph)
        {
            return new ToolStateMachine(graph as StateMachine, this);
        }

        protected override Graph CreateAsset()
        {
            return CreateInstance<StateMachine>();
        }
    }
}