using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NGS.AdvancedCullingSystem.Dynamic;

namespace AdvancedCullingSystem.Examples
{
    public class CullingSettingsController : MonoBehaviour
    {
        private DC_Camera _camera;

        private void Awake()
        {
            _camera = FindObjectOfType<DC_Camera>();
        }

        public void EnableCulling()
        {
            _camera.enabled = true;

            foreach (var source in FindObjectsOfType<DC_Source>())
                source.Enable();
        }

        public void DisableCulling()
        {
            _camera.enabled = true;

            foreach (var source in FindObjectsOfType<DC_Source>())
                source.Disable();
        }

        public void OnSliderChanged(float value)
        {
            _camera.SetRaysCount((int)value);
        }
    }
}
