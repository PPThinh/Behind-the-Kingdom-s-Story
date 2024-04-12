using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NGS.AdvancedCullingSystem.Dynamic;

#if UNITY_EDITOR

using UnityEditor;

#endif

namespace AdvancedCullingSystem.Examples
{
    public class SpawnController : MonoBehaviour
    {
        [SerializeField]
        private Transform[] _spawnObjects;

        [SerializeField]
        private Text _objectsCountField;

        [SerializeField]
        private int _objectsPerSpawnCount = 50;

        [SerializeField]
        private Vector3 _minSpawnPos;

        [SerializeField]
        private Vector3 _maxSpawnPos;

        public void Spawn()
        {
            for (int i = 0; i < _objectsPerSpawnCount; i++)
            {
                Transform obj = Instantiate(_spawnObjects[Random.Range(0, _spawnObjects.Length)]);

                Vector3 position = new Vector3(
                    Random.Range(_minSpawnPos.x, _maxSpawnPos.x),
                    Random.Range(_minSpawnPos.y, _maxSpawnPos.y),
                    Random.Range(_minSpawnPos.z, _maxSpawnPos.z));

                Vector3 rotation = new Vector3(
                    obj.eulerAngles.x,
                    Random.Range(0, 360),
                    obj.eulerAngles.z);

                RaycastHit hit;
                if(Physics.Raycast(new Vector3(position.x, 100, position.z), Vector3.down, out hit, 1000, LayerMask.GetMask("Water")))
                    position.y = hit.point.y;

                obj.position = position;
                obj.eulerAngles = rotation;

                foreach (var renderer in obj.GetComponentsInChildren<MeshRenderer>())
                    DC_Controller.GetById(0).AddObjectForCulling(renderer);
            }

            if(_objectsCountField != null)
                _objectsCountField.text = (int.Parse(_objectsCountField.text) + 50).ToString();
        }
    }



#if UNITY_EDITOR

    [CustomEditor(typeof(SpawnController))]
    public class SpawnControllerEditor : Editor
    {
        protected new SpawnController target
        {
            get
            {
                return (SpawnController) base.target;
            }
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (GUILayout.Button("Spawn"))
                target.Spawn();
        }
    }

#endif
}
