using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using Unity.MLAgents.Sensors;
using MBaske;
using Random = UnityEngine.Random;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody))]
public class DroneAgent : Agent
{
    [SerializeField] private Transform _targetTransform;
    
    private readonly List<PropellerPart> m_PropellerParts = new List<PropellerPart>();
    private Rigidbody m_Rigidbody;
    private Vector3 m_CachedPosition;
    
    const float MaxDist = 5;
    const float DistanceThreshold = 1;

    public override void Initialize()
    {
        m_Rigidbody = GetComponent<Rigidbody>();
        m_CachedPosition = transform.position;

        foreach (PropellerPart propellerPart in GetComponentsInChildren<PropellerPart>())
        {
            propellerPart.agent = this;
            
            m_PropellerParts.Add(propellerPart);
        }
    }
    
    protected override void OnEnable()
    {
        var behaviorParameters = GetComponent<BehaviorParameters>();
        //behaviorParameters.BrainParameters.ActionSpec.NumContinuousActions;
            
        base.OnEnable();
    }

    private void FixedUpdate()
    {
        float angle = transform.up.y;
        //float matchDir = GetMatchingVelocityReward(_targetTransform.position - transform.position, m_Rigidbody.velocity);

        float dist = (_targetTransform.position - transform.position).magnitude;
        float mathDir = dist < DistanceThreshold ? angle * (1 - dist) : -dist / MaxDist * 0.1f - (1 - angle);
        
        AddReward(mathDir);
        AddReward(m_Rigidbody.velocity.magnitude * -0.2f);
        AddReward(m_Rigidbody.angularVelocity.magnitude * -0.1f);

        if (angle < 0)
        {
            AddReward(-1000);
            EndEpisode();
            return;
        }

        if (Vector2.Distance(new Vector2(transform.position.x, transform.position.z), new Vector2(_targetTransform.position.x, _targetTransform.position.z)) > MaxDist
            || transform.position.y - _targetTransform.position.y > MaxDist)
        {
            AddReward(-1000);
            EndEpisode();
            return;
        }
    }
    
    public override void OnEpisodeBegin()
    {
        foreach (PropellerPart propellerPart in m_PropellerParts)
        {
            propellerPart.Reset();
        }
        
        m_Rigidbody.velocity = Vector3.zero;
        m_Rigidbody.angularVelocity = Vector3.zero;
        m_Rigidbody.Sleep();
        
        Vector2 randomPos = Random.insideUnitCircle;
        transform.position = m_CachedPosition + new Vector3(randomPos.x, 0, randomPos.y) * (MaxDist - 1);
        transform.rotation = Quaternion.Euler(Random.Range(-20.0f, 20.0f), Random.Range(0.0f, 360.0f), Random.Range(-20.0f, 20.0f));
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(Normalization.Sigmoid(transform.InverseTransformPoint(_targetTransform.position), 0.25f));
        sensor.AddObservation(Normalization.Sigmoid(transform.InverseTransformDirection(m_Rigidbody.velocity),0.5f));
        sensor.AddObservation(Normalization.Sigmoid(transform.InverseTransformDirection(m_Rigidbody.angularVelocity)));
        
        var rotation = transform.rotation.eulerAngles / 90.0f;
        sensor.AddObservation(rotation.x > 2.0f ? rotation.x - 4 : rotation.x);
        sensor.AddObservation(rotation.z > 2.0f ? rotation.z - 4 : rotation.z);

        foreach (PropellerPart propellerPart in m_PropellerParts)
        {
            sensor.AddObservation(propellerPart.Thrust);
        }
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        for (int i = 0; i < m_PropellerParts.Count; i++)
        {
            m_PropellerParts[i].SetThrust(actions.ContinuousActions[i] / 2.0f + 0.5f);
        }
    }

    private void OnCollisionEnter(Collision col)
    {
        AddReward(-1000);
        EndEpisode();
    }
}
