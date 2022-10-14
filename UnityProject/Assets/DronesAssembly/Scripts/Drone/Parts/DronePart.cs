using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody))]
public abstract class DronePart : MonoBehaviour
{
    public Transform[] sockets;
    [HideInInspector] public DroneAgent agent;
    private Rigidbody m_Rigidbody;
    
    private void Start()
    {
        m_Rigidbody = GetComponent<Rigidbody>();
    }
    
    public void Reset()
    {
        m_Rigidbody.Sleep();
        m_Rigidbody.velocity = Vector3.zero;
        m_Rigidbody.angularVelocity = Vector3.zero;
    }

    protected Rigidbody Rigidbody => m_Rigidbody;
}
