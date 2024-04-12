using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NGS.AdvancedCullingSystem.Dynamic
{
    public partial class DC_Camera : MonoBehaviour
    {
        public enum DistributionMethod { Halton, R2 }

        private static class DC_CameraUtil
        {
            private static Dictionary<DC_CameraSettings, Vector3[]> _rayDirsTable;

            private static double _r2a1;
            private static double _r2a2;


            static DC_CameraUtil()
            {
                _rayDirsTable = new Dictionary<DC_CameraSettings, Vector3[]>();

                double g = 1.32471795724474602596;

                _r2a1 = 1.0 / g;
                _r2a2 = 1.0 / (g * g);
            }

            public static Vector3[] GetRaysDirections(Camera camera, DistributionMethod distribution)
            {
                DC_CameraSettings settings = new DC_CameraSettings(camera);

                if (_rayDirsTable.TryGetValue(settings, out Vector3[] result))
                    return result;

                float cameraFov = camera.fieldOfView;
                Matrix4x4 cameraInvTransform = camera.transform.localToWorldMatrix.inverse;

                int count = (Screen.width * Screen.height) / 8;
                Vector3[] dirs = new Vector3[count];

                camera.fieldOfView *= 1.05f;

                for (int i = 0; i < count; i++)
                {
                    Vector2 viewPoint;

                    if (distribution == DistributionMethod.Halton)
                        viewPoint = new Vector2(HaltonSequence(i, 2), HaltonSequence(i, 3));
                    else
                        viewPoint = R2Distribution(i);

                    Ray ray = camera.ViewportPointToRay(viewPoint);

                    dirs[i] = cameraInvTransform.MultiplyVector(ray.direction);
                }

                camera.fieldOfView = cameraFov;

                _rayDirsTable.Add(settings, dirs);

                return dirs;
            }


            private static float HaltonSequence(int index, int b)
            {
                float res = 0f;
                float f = 1f / b;

                int i = index;

                while (i > 0)
                {
                    res = res + f * (i % b);
                    i = Mathf.FloorToInt(i / b);
                    f = f / b;
                }

                return res;
            }

            private static Vector2 R2Distribution(int index)
            {
                float x = (float)((0.5 + _r2a1 * index) % 1);
                float y = (float)((0.5 + _r2a2 * index) % 1);

                return new Vector2(x, y);
            }
        }
    }
}
