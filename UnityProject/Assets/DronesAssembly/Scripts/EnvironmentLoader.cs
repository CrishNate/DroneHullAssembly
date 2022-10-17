using System;
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
        string[] args = Environment.GetCommandLineArgs();
        var graph = DroneAssembly.Instance.DroneGenerate();
        print(DroneAssembly.HullModelString(graph));
        //var graph = DroneAssembly.ParseFromArgs("15 0 2 7 8 1 6 3 3 3 2 8 1 1 1 8 9 0 1 21 1 2 48 2 3 46 2 4 17 4 5 33 5 6 34 5 7 35 5 8 10 0 9 23 9 10 11 0 11 18 11 12 18 12 13 19 13 14".Split(' '));

        DroneAgent[] drones = Object.FindObjectsOfType<DroneAgent>();
        foreach (DroneAgent drone in drones)
        {
            drone.Initialize(graph);
        }
    }
}