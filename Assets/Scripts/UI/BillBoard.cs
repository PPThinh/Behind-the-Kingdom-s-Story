using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KingdomProject
{
    public class BillBoard : MonoBehaviour
    {
        private Transform _mainCam;
        void Start()
        {
            _mainCam = Camera.main.transform;
        }


        void LateUpdate()
        {
            transform.LookAt(transform.position + _mainCam.transform.rotation * Vector3.forward,_mainCam.transform.rotation * Vector3.up);
        }
    }
}
