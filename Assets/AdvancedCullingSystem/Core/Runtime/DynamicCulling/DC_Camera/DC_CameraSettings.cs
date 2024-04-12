using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NGS.AdvancedCullingSystem.Dynamic
{
    public struct DC_CameraSettings
    {
        public int width;
        public int height;
        public float fov;
        public float farPlane;

        public DC_CameraSettings(Camera camera)
        {
            width = camera.pixelWidth;
            height = camera.pixelHeight;
            fov = camera.fieldOfView;
            farPlane = camera.farClipPlane;
        }
    }
}
