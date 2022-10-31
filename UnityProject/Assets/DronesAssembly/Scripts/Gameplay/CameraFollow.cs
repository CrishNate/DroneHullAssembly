using System;
using UnityEngine;

namespace DronesAssembly.Scripts.Gameplay
{
    public class CameraFollow : MonoBehaviour
    {
        private Transform _followObject;
        
        [SerializeField]
        private float _distanceOffset = 10.0f;

        [SerializeField] 
        private float _sensativity = 1.0f;

        private void Start()
        {
            _followObject = FindObjectOfType<DroneAgent>(true).transform;
        }

        private void Update()
        {
            transform.position = _followObject.position - transform.forward * _distanceOffset;

            if (Input.GetMouseButtonDown(0))
            {
                float yAngle = Input.GetAxis("Mouse X") * _sensativity * Time.deltaTime;
                float pAngle = -Input.GetAxis("Mouse Y") * _sensativity * Time.deltaTime;
                
                transform.Rotate(Vector3.up, Input.GetAxis ("Mouse X"), Space.World);
                transform.Rotate(Vector3.right, Input.GetAxis ("Mouse Y"), Space.Self);
            }
        }
    }
}