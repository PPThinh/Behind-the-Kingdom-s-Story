using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AdvancedCullingSystem.Examples
{
    public class FPSCounter : MonoBehaviour
    {
        [SerializeField]
        private Text _textField;

        private float _passed;
        private int _frames;

        private void Update()
        {
            _passed += Time.deltaTime;
            _frames++;

            if (_passed > 1)
            {
                _textField.text = _frames.ToString();

                _passed = 0;
                _frames = 0;
            }
        }
    }
}
