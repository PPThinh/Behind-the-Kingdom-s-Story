using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AdvancedCullingSystem.Examples
{
    [RequireComponent(typeof(Camera))]
    public class FrustumCullingVisualization : MonoBehaviour
    {
        private Camera _camera;
        private MeshRenderer[] _renderers;

        private void Awake()
        {
            _camera = GetComponent<Camera>();
            _renderers = FindObjectsOfType<MeshRenderer>()
                .Where(r => r.enabled)
                .ToArray();
        }

        private void Update()
        {
            Plane[] planes = GeometryUtility.CalculateFrustumPlanes(_camera);

            foreach (var renderer in _renderers)
                renderer.enabled = GeometryUtility.TestPlanesAABB(planes, renderer.bounds);
        }
    }
}