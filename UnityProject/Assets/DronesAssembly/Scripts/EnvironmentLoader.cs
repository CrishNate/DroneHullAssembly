using System;
using System.Collections.Generic;
using System.Linq;
using DotNetGraph;
using DroneHullAssembly.Tools;
using UnityEngine;
using Object = UnityEngine.Object;

public class EnvironmentLoader : MonoBehaviour
{
    [SerializeField] private GameObject environment;
    [SerializeField] private Vector2 offset;
    [SerializeField] private Vector2Int copy;

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
    
    private void OnEnable()
    {
        ArgumentsParser argumentsParser = new ArgumentsParser();
        argumentsParser.Add("-e", "copies of environments e.g. -e 0,0", x =>
        {
            var split = x.Split(',');
            copy.Set(int.Parse(split[0]), int.Parse(split[1]));
        });

        string[] args = Environment.GetCommandLineArgs();
        int lastInx = argumentsParser.Parse(args);
        
        Copy();

        DotGraph graph = DroneAssembly.ParseFromArgs(args[Range.StartAt(lastInx)]);
        //DotGraph graph = DroneAssembly.ParseFromArgs("11 0 2 2 8 3 1 6 3 3 1 8 9 0 1 22 1 2 23 2 3 13 0 4 11 0 5 16 5 6 27 6 7 14 6 8 26 6 9 19 9 10".Split(' '));
        //var graph = DroneAssembly.Instance.DroneGenerate();
        //print(DroneAssembly.HullModelString(graph));

        DroneAgent[] drones = Object.FindObjectsOfType<DroneAgent>(true);
        foreach (DroneAgent drone in drones)
        {
            drone.Initialize(graph);
        }
    }
}