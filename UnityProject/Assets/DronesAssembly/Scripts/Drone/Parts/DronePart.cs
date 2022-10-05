using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody))]
public abstract class DronePart : MonoBehaviour
{
    [HideInInspector] public DroneAgent agent;
    private Rigidbody m_Rigidbody;
    
    private void Start()
    {
        m_Rigidbody = GetComponent<Rigidbody>();
    }
    
    public virtual void Reset()
    { }

    protected Rigidbody Rigidbody => m_Rigidbody;
}
