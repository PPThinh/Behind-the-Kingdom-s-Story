using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AdvancedCullingSystem.Examples
{
    public enum InputType { Mobile, Desktop }

    public class CameraController : MonoBehaviour
    {
        [SerializeField] private float _movementSpeed;
        [SerializeField] private float _rotationSpeed;

        [Space]

        [SerializeField] private Joystick _leftJoystick;
        [SerializeField] private Joystick _rightJoystick;

        [Space]

        [SerializeField] private float _height;

        private float _addRotationX;


        private void ComputeMovement()
        {
            Vector3 direction = new Vector3(_leftJoystick.value.x, 0, _leftJoystick.value.y);

            if (Mathf.Approximately(direction.x, 0) && Mathf.Approximately(direction.z, 0))
                return;

            transform.position += Quaternion.Euler(0, transform.eulerAngles.y, 0) * (direction * _movementSpeed * Time.deltaTime);
        }

        private void ComputeRotation()
        {
            Vector2 value = _rightJoystick.value;

            if (Mathf.Approximately(value.x, 0) && Mathf.Approximately(value.y, 0))
                return;

            Vector3 rotation = transform.eulerAngles;

            if ((_addRotationX > -40 && _addRotationX < 40) || (_addRotationX < -40 && value.y > 0) || (_addRotationX > 40 && value.y < 0))
            {
                rotation.x -= value.y * _rotationSpeed * Time.deltaTime;

                _addRotationX += value.y * _rotationSpeed * Time.deltaTime;
            }

            rotation.y += value.x * _rotationSpeed * Time.deltaTime;

            transform.eulerAngles = rotation;
        }

        private void ComputeHeight()
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, Vector3.down, out hit, 100, LayerMask.GetMask("Water")))
                transform.position = new Vector3(transform.position.x, hit.point.y + _height, transform.position.z);
        }

        private void Update()
        {
            ComputeMovement();
            ComputeRotation();
            ComputeHeight();
        }
    }
}