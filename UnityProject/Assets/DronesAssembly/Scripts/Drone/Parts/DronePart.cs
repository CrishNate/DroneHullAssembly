using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public abstract class DronePart : MonoBehaviour
{
    public Transform[] sockets;
    public float mass;
    [HideInInspector] public DroneAgent agent;

    private void Start()
    { }
    
    public void Reset()
    { }
}
