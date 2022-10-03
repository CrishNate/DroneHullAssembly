using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using Unity.MLAgents.Sensors;
using UnityEngine.Serialization;
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
    const float MaxWalkingSpeed = 15; 

    [Header("Walk Speed")]
    [Range(0.1f, MaxWalkingSpeed)]
    [SerializeField]
    [Tooltip(
        "The speed the agent will try to match.\n\n" +
        "TRAINING:\n" +
        "For VariableSpeed envs, this value will randomize at the start of each training episode.\n" +
        "Otherwise the agent will try to match the speed set here.\n\n" +
        "INFERENCE:\n" +
        "During inference, VariableSpeed agents will modify their behavior based on this value " +
        "whereas the CrawlerDynamic & CrawlerStatic agents will run at the speed specified during training "
    )]
    private float m_TargetWalkingSpeed = 10.0f;
    public float TargetWalkingSpeed
    {
        get { return m_TargetWalkingSpeed; }
        set { m_TargetWalkingSpeed = Mathf.Clamp(value, .1f, MaxWalkingSpeed); }
    }
    
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
        float angle = Vector3.Dot(transform.up, Vector3.up);
        float matchDir = GetMatchingVelocityReward(_targetTransform.position - transform.position, m_Rigidbody.velocity);

        AddReward(angle * matchDir);

        if (angle < 0.4)
        {
            SetReward(-1);
            EndEpisode();
        }

        if (Vector2.Distance(new Vector2(transform.position.x, transform.position.z), new Vector2(_targetTransform.position.x, _targetTransform.position.z)) > MaxDist
            || transform.position.y - _targetTransform.position.y > MaxDist)
        {
            SetReward(-1);
            EndEpisode();
        }
    }
    
    public float GetMatchingVelocityReward(Vector3 velocityGoal, Vector3 actualVelocity)
    {
        //distance between our actual velocity and goal velocity
        var velDeltaMagnitude = Mathf.Clamp(Vector3.Distance(actualVelocity, velocityGoal), 0, TargetWalkingSpeed);

        //return the value on a declining sigmoid shaped curve that decays from 1 to 0
        //This reward will approach 1 if it matches perfectly and approach zero as it deviates
        return Mathf.Pow(1 - Mathf.Pow(velDeltaMagnitude / TargetWalkingSpeed, 2), 2);
    }
    
    public override void OnEpisodeBegin()
    {
        Vector2 randomPos = Random.insideUnitCircle;
        transform.position = m_CachedPosition + new Vector3(randomPos.x, 0, randomPos.y) * (MaxDist - 1);
        transform.rotation = Quaternion.Euler(Random.Range(-20.0f, 20.0f), Random.Range(0.0f, 360.0f), Random.Range(-20.0f, 20.0f));
        
        m_Rigidbody.velocity = Vector3.zero;
        m_Rigidbody.angularVelocity = Vector3.zero;
        
        foreach (PropellerPart propellerPart in m_PropellerParts)
        {
            propellerPart.Reset();
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.InverseTransformPoint(_targetTransform.position));
        sensor.AddObservation(transform.InverseTransformDirection(m_Rigidbody.velocity));
        sensor.AddObservation(transform.InverseTransformDirection(m_Rigidbody.angularVelocity));
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
        EndEpisode();
    }
}
