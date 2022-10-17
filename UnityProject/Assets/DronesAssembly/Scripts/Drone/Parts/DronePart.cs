using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public abstract class DronePart : MonoBehaviour
{
    public Transform[] sockets;
    public float mass;
    
    public DroneAgent Agent
    {
        get;
        private set;
    }

    protected Rigidbody RigidbodyRef
    {
        get;
        private set;
    }

    public void Init(DroneAgent inAgent, Rigidbody inRigidbody)
    {
        Agent = inAgent;
        RigidbodyRef = inRigidbody;
    }
    
    private void Start()
    { }
    
    public void Reset()
    { }
}
