using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace NGS.AdvancedCullingSystem.Dynamic
{
    public class DC_SelectionTool
    {
        public DC_Controller Controller { get; private set; }

        private DC_BaseSelector[] _selectors;
        private DC_ControllerEditor _editor;


        public DC_SelectionTool(DC_Controller controller)
        {
            Controller = controller;

            _selectors = new DC_BaseSelector[]
            {
                new DC_CamerasSelector(this),
                new DC_RenderersSelector(this),
                new DC_LODGroupsSelector(this),
                new DC_OccludersSelector(this),
                new DC_IncompatiblesSelector(this)
            };
        }

        public void Initialize(DC_Controller controller, DC_ControllerEditor editor)
        {
            Controller = controller;
            _editor = editor;
        }

        public void OnInspectorGUI(ref bool sceneChanged)
        {
            for (int i = 0; i < _selectors.Length; i++)
            {
                if (_selectors[i].IsAvailable)
                    _selectors[i].OnInspectorGUI(ref sceneChanged);
            }

            if (sceneChanged) // || GUILayout.Button("Refresh"))
                Refresh();
        }

        public void OnDrawGizmos()
        {
            for (int i = 0; i < _selectors.Length; i++)
            {
                DC_BaseSelector selector = _selectors[i];

                if (selector.IsAvailable)
                    selector.OnDrawGizmos();
            }
        }

        public void Refresh()
        {
            for (int i = 0; i < _selectors.Length; i++)
            {
                DC_BaseSelector selector = _selectors[i];

                if (selector.IsAvailable)
                    selector.Refresh();
            }
        }

        public void Repaint()
        {
            _editor?.Repaint();
        }
    }
}
