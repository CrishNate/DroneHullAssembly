using System;
using System.Linq;
using DotNetGraph;
using DroneHullAssembly.Tools;
using Unity.MLAgents;
using Unity.MLAgents.SideChannels;
using UnityEngine;

public class EnvironmentLoader : MonoBehaviour
{
    [SerializeField] private GameObject environment;
    [SerializeField] private Vector2 offset;
    [SerializeField] private Vector2Int copy;

    private ValidDesignSideChannel validDesignSideChannel;
    
    void Copy()
    {
        for (int x = 0; x < copy.x; x++)
        for (int y = 0; y < copy.y; y++)
        {
            if (x == 0 && y == 0)
                continue;
            
            Instantiate(environment, environment.transform.position + new Vector3(offset.x * x, 0, offset.y * y), environment.transform.rotation);
        }
    }
    
    public void Awake()
    {
        validDesignSideChannel = new ValidDesignSideChannel();
        SideChannelManager.RegisterSideChannel(validDesignSideChannel);
    }
    
    public void OnDestroy()
    {
        if (Academy.IsInitialized)
        {
            SideChannelManager.UnregisterSideChannel(validDesignSideChannel);
        }
    }
    
    private void OnEnable()
    {
        ArgumentsParser argumentsParser = new ArgumentsParser();
        argumentsParser.Add("-e", "copies of environments e.g. -e 0,0", x =>
        {
            var split = x.Split(',');
            copy.Set(int.Parse(split[0]), int.Parse(split[1]));
        });

        string[] args = Environment.GetCommandLineArgs();
        argumentsParser.Parse(args);
        
        Copy();

        int index = args.ToList().FindIndex(x => x.Equals("-d"));
        DotGraph graph = DroneAssembly.ParseFromArgs(args[Range.StartAt(index + 1)]);
        
        //DotGraph graph = DroneAssembly.Instance.DroneGenerate();
        //print(DroneAssembly.HullModelString(graph));
        DroneAgent[] drones = FindObjectsOfType<DroneAgent>(true);
        
        foreach (DroneAgent drone in drones)
        {
            drone.Initialize(graph);

            if (drone == drones[0])
            {
                bool validate = !DroneAssembly.CheckDroneSelfCollision(drone);
                validDesignSideChannel.SendMessage(validate);
            }
        }
    }
}