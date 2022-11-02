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
        
    const float MaxDist = 5;
    const float DistanceThreshold = 0.5f;

    public void Initialize(DotGraph graph)
    {
        m_CachedPosition = transform.position;

        var behaviorParameters = GetComponent<BehaviorParameters>();
        
        m_Rigidbody = GetComponent<Rigidbody>();
        DroneAssembly.AssembleDrone(graph, transform);

        m_Motors.Clear();
        m_PropellerParts.Clear();
        m_Parts.Clear();
        
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
        }
        
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
        
        float dist = dir.magnitude;
        float dir_angle = Vector3.Dot(m_Rigidbody.velocity.normalized * Math.Min(1.0f, m_Rigidbody.velocity.magnitude),
                                        dir.normalized * Math.Min(1.0f, dir.magnitude));
        
        AddReward(dist < DistanceThreshold ? 1 : -dist / MaxDist * 0.5f * (dir_angle * -0.5f + 0.5f));
        //AddReward(Mathf.Min(0.1f, Vector3.Dot(m_Rigidbody.velocity, dir.normalized * Math.Min(10.0f, dir.magnitude)) * 0.05f));
        AddReward(-(1 - angle));
        AddReward(m_Rigidbody.velocity.magnitude * -0.1f);
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
        m_Parts.ForEach(x => x.Reset());
        
        m_Rigidbody.Sleep();
        m_Rigidbody.velocity = Vector3.zero;
        m_Rigidbody.angularVelocity = Vector3.zero;

        transform.position = m_CachedPosition;
        transform.rotation = Quaternion.identity;
        
        //Vector2 randomPos = Random.insideUnitCircle;
        //transform.position = m_CachedPosition + new Vector3(randomPos.x, 0, randomPos.y) * (MaxDist - 1);
        //transform.rotation = Quaternion.Euler(Random.Range(-20.0f, 20.0f), Random.Range(0.0f, 360.0f), Random.Range(-20.0f, 20.0f));
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
        AddReward(-1000);
        EndEpisode();
    }
}
