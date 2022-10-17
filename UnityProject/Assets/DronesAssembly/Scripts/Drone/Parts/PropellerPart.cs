using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class PropellerPart : DronePart
{
    [HideInInspector] public bool rotationCW;

    [SerializeField] private float thrustScale;
    [SerializeField] private float thrustResponse;
    [SerializeField] private float torqueScale;

    private float m_CurrentThrust;
    private float m_Value;

    public void FixedUpdate()
    {
        RigidbodyRef.AddForceAtPosition(transform.up * (m_Value * thrustScale), transform.position, ForceMode.Force);
        //m_CurrentThrust = Mathf.Lerp(m_CurrentThrust, m_Value, Time.fixedDeltaTime * thrustResponse);
        //Rigidbody.AddForce(transform.up * (m_Value * thrustScale), ForceMode.Impulse);
        //Rigidbody.AddRelativeTorque(transform.up * (m_CurrentThrust * torqueScale * (rotationCW ? -1 : 1)), ForceMode.Impulse);
    }

    public void SetThrust(float thrust)
    {
        m_Value = thrust;
    }

    public float CurrentThrust => m_Value;
}
