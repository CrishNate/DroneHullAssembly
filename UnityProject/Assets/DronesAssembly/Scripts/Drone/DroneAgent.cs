using System;
using System.Collections.Generic;
using DotNetGraph;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using Unity.MLAgents.Sensors;
using MBaske;
using Random = UnityEngine.Random;

[DisallowMultipleComponent]
public class DroneAgent : Agent
{
    [SerializeField] private Transform _targetTransform;
    [SerializeField] private List<DronePart> m_Parts = new List<DronePart>();
    [SerializeField] private List<PropellerPart> m_PropellerParts = new List<PropellerPart>();
    [SerializeField] private List<MotorKneePart> m_Motors = new List<MotorKneePart>();
    
    private Rigidbody m_Rigidbody;
    private Vector3 m_CachedPosition;
    private Vector3 m_WindForce = Vector3.forward * 0.0f;

    const float MaxDist = 10;
    const float TargetWalkingSpeed = 5.0f;
    

    public void Initialize(DotGraph graph)
    {
        m_CachedPosition = transform.position;

        var behaviorParameters = GetComponent<BehaviorParameters>();
        
        m_Rigidbody = GetComponent<Rigidbody>();
        DroneAssembly.AssembleDrone(graph, transform);

        m_Motors.Clear();
        m_PropellerParts.Clear();
        m_Parts.Clear();

        float totalMass = 0;
        
        foreach (DronePart part in GetComponentsInChildren<DronePart>())
        {
            part.Init(this, m_Rigidbody);
            m_Parts.Add(part);
            
            switch (part)
            {
                case PropellerPart propellerPart:
                    m_PropellerParts.Add(propellerPart);
                    break;
                
                case MotorKneePart motorKneePart:
                    m_Motors.Add(motorKneePart);
                    break;
            }

            totalMass += part.mass;
        }

        m_Rigidbody.mass = totalMass;
        
        // 3 - target position offset
        // 3 - velocity
        // 3 - angular velocity
        // 2 - rotation local
        behaviorParameters.BrainParameters.VectorObservationSize = 11 + m_PropellerParts.Count + m_Motors.Count * 2;
        behaviorParameters.BrainParameters.ActionSpec = ActionSpec.MakeContinuous(m_PropellerParts.Count + m_Motors.Count);
        
        gameObject.SetActive(true);
    }

    private void FixedUpdate()
    {
        float angle = transform.up.y;
        //float matchDir = GetMatchingVelocityReward(_targetTransform.position - transform.position, m_Rigidbody.velocity);

        Vector3 dir = (_targetTransform.position - transform.position);

        AddReward(GetMatchingVelocityReward(dir.normalized * Math.Min(dir.magnitude, TargetWalkingSpeed), m_Rigidbody.velocity));
        AddReward(angle * 0.2f);
        AddReward(m_Rigidbody.velocity.magnitude * -0.05f);
        AddReward(m_Rigidbody.angularVelocity.magnitude * -0.05f);

        if (Vector2.Distance(new Vector2(transform.position.x, transform.position.z), new Vector2(_targetTransform.position.x, _targetTransform.position.z)) > MaxDist
            || transform.position.y - _targetTransform.position.y > MaxDist)
        {
            AddReward(-10);
            EndEpisode();
            return;
        }
        
        //m_Rigidbody.AddForce(m_WindForce, ForceMode.Force);
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
        m_Parts.ForEach(x => x.Reset());
        
        m_Rigidbody.Sleep();
        m_Rigidbody.velocity = Vector3.zero;
        m_Rigidbody.angularVelocity = Vector3.zero;

        transform.position = m_CachedPosition;
        transform.rotation = Quaternion.identity;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        if (sensor == null)
            return;
        
        sensor.AddObservation(Normalization.Sigmoid(transform.InverseTransformPoint(_targetTransform.position), 0.25f));
        sensor.AddObservation(Normalization.Sigmoid(transform.InverseTransformDirection(m_Rigidbody.velocity),0.5f));
        sensor.AddObservation(Normalization.Sigmoid(transform.InverseTransformDirection(m_Rigidbody.angularVelocity)));
        
        var rotation = transform.rotation.eulerAngles / 90.0f;
        sensor.AddObservation(rotation.x > 2.0f ? rotation.x - 4 : rotation.x);
        sensor.AddObservation(rotation.z > 2.0f ? rotation.z - 4 : rotation.z);
        
        foreach (PropellerPart propellerPart in m_PropellerParts)
        {
            sensor.AddObservation(propellerPart.CurrentThrust);
        }
        
        foreach (MotorKneePart motorKneePart in m_Motors)
        {
            sensor.AddObservation(motorKneePart.CurrentTorque);
            sensor.AddObservation(motorKneePart.CurrentTorqueSpeed);
        }
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        int index = 0;
        for (int i = 0; i < m_PropellerParts.Count; i++)
        {
            m_PropellerParts[i].SetThrust(actions.ContinuousActions[index++] / 2.0f + 0.5f);
        }
        
        for (int i = 0; i < m_Motors.Count; i++)
        {
            m_Motors[i].SetTorqueSpeed(actions.ContinuousActions[index++]);
        }
    }

    private void OnCollisionEnter(Collision col)
    {
        AddReward(-10);
        EndEpisode();
    }
}
