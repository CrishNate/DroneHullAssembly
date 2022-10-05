using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody))]
public class PropellerPart : MonoBehaviour
{
    [HideInInspector] public bool rotationCW;

    [SerializeField] private float thrustScale;
    [SerializeField] private float thrustResponse;
    [SerializeField] private float torqueScale;
    
    [HideInInspector] public DroneAgent agent;
    private Rigidbody m_Rigidbody;
    
    private float m_CurrentThrust;
    private float m_Value;
    
    private void Start()
    {
        m_Rigidbody = GetComponent<Rigidbody>();
    }
    
    public void FixedUpdate()
    {
        //m_CurrentThrust = Mathf.Lerp(m_CurrentThrust, m_Value, Time.fixedDeltaTime * thrustResponse);
        m_Rigidbody.AddForce(transform.up * (m_Value * thrustScale), ForceMode.Impulse);
        //Rigidbody.AddRelativeTorque(transform.up * (m_CurrentThrust * torqueScale * (rotationCW ? -1 : 1)), ForceMode.Impulse);
    }

    public void SetThrust(float thrust)
    {
        m_Value = thrust;
    }
    
    public void Reset()
    {
        m_Rigidbody.Sleep();
        m_Rigidbody.velocity = Vector3.zero;
        m_Rigidbody.angularVelocity = Vector3.zero;
    }

    public float CurrentThrust => m_Value;
}
