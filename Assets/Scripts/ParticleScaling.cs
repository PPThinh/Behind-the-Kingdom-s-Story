using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KingdomProject
{
    public class ParticleScaling : MonoBehaviour
    {
        private ParticleSystem ps;
        public float sliderValue = 1.0F;
        public float parentSliderValue = 1.0F;
        public ParticleSystemScalingMode scaleMode;

        void Start()
        {
            ps = GetComponent<ParticleSystem>();
        }

        void Update()
        {
            ps.transform.localScale = new Vector3(sliderValue, sliderValue, sliderValue);
            if (ps.transform.parent != null)
                ps.transform.parent.localScale = new Vector3(parentSliderValue, parentSliderValue, parentSliderValue);

            var main = ps.main;
            main.scalingMode = scaleMode;
        }

        void OnGUI()
        {
            scaleMode = (ParticleSystemScalingMode)GUI.SelectionGrid(new Rect(25, 25, 300, 30), (int)scaleMode, new GUIContent[] { new GUIContent("Hierarchy"), new GUIContent("Local"), new GUIContent("Shape") }, 3);
            GUI.Label(new Rect(25, 80, 100, 30), "Scale");
            sliderValue = GUI.HorizontalSlider(new Rect(125, 85, 100, 30), sliderValue, 0.0F, 5.0F);
            GUI.Label(new Rect(25, 120, 100, 30), "Parent Scale");
            parentSliderValue = GUI.HorizontalSlider(new Rect(125, 125, 100, 30), parentSliderValue, 0.0F, 5.0F);
        }
    }
}
