using System;
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

    [SerializeField] private Transform speedVisualObject;
    
    private float m_CurrentThrust;
    private float m_Value;

    public void Start()
    {
        if (DroneAssembly.DebugVisual)
        {
            speedVisualObject.gameObject.SetActive(true);
        }
    }

    public void FixedUpdate()
    {
        m_CurrentThrust = Mathf.Lerp(m_CurrentThrust, m_Value, Time.fixedDeltaTime * thrustResponse);
        
        if (DroneAssembly.DebugVisual)
        {
            float offset = m_CurrentThrust > 0 ? 1 : -1;
            offset *= 0.2f;
            
            speedVisualObject.transform.position = transform.position + transform.up * (m_CurrentThrust * 0.5f + offset);
            
            var localScale = speedVisualObject.transform.localScale;
            localScale = new Vector3(localScale.x, Math.Abs(m_CurrentThrust), localScale.z);
            speedVisualObject.transform.localScale = localScale;
        }
        
        RigidbodyRef.AddForceAtPosition(transform.up * (m_CurrentThrust * thrustScale), transform.position, ForceMode.Force);
        //RigidbodyRef.AddRelativeTorque(transform.up * (m_CurrentThrust * torqueScale * (rotationCW ? -1 : 1)), ForceMode.Force);
    }

    public override void Reset()
    {
        m_CurrentThrust = 0;
        m_Value = 0;
    }

    public void SetThrust(float thrust)
    {
        m_Value = thrust;
    }

    public float CurrentThrust => m_Value;
}
