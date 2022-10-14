using System.Collections.Generic;
using System.IO;
using DotNetGraph;
using DotNetGraph.Core;
using DotNetGraph.Edge;
using DotNetGraph.Node;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

public class DroneAssembly : Singleton<DroneAssembly>
{
    [SerializeField] 
    private SerializableDictionary<string, DronePart> label2DroneParts;
        
    public static DronePart AssembleDrone(DotGraph drone, Transform parent)
    {
        print(HullModelString(drone));
        List<DotNode> nodes = new List<DotNode>();
        List<DotEdge> edges = new List<DotEdge>();
        
        foreach (var element in drone.Elements)
        {
            switch (element)
            {
                case DotNode node:
                    nodes.Add(node);
                    break;
                
                case DotEdge edge:
                    edges.Add(edge);
                    break;
            }
        }

        DotNode rootNode = nodes.Find(node => node.Label.Text == "root");

        DronePart dronePart = Instance.AssemblePart(rootNode, null, 0, 0, edges, parent, false);
        foreach (Rigidbody child in parent.GetComponentsInChildren<Rigidbody>())
        {
            child.isKinematic = false;
        }

        return dronePart;
    }

    private DronePart AssemblePart(DotNode element, DronePart dronePartL, int socketInxTo, int socketInxFrom, List<DotEdge> edges, Transform parent, bool mirror)
    {
        if (label2DroneParts[element.Label.Text] == null)
            return null;
        
        DronePart dronePartR = Instantiate(label2DroneParts[element.Label.Text], parent);
        dronePartR.GetComponent<Rigidbody>().isKinematic = true;

        if (dronePartL != null)
        {
            Transform socketFrom = dronePartL.sockets[socketInxTo];
            Transform socketTo = dronePartR.sockets[socketInxFrom];

            dronePartR.transform.rotation = socketFrom.rotation * Quaternion.Inverse(socketTo.localRotation * Quaternion.Euler(0, 180, 0));
            dronePartR.transform.position = socketFrom.position - dronePartR.transform.TransformDirection(socketTo.localPosition * socketTo.lossyScale.x);

            var joint = dronePartL.AddComponent<FixedJoint>();
            joint.enableCollision = false;
            joint.connectedBody = dronePartR.GetComponent<Rigidbody>();
        }

        List<DotEdge> dotEdges = edges.FindAll(edge => edge.Left == element);

        foreach (DotEdge dotEdge in dotEdges)
        {
            int socketTo = (mirror && dotEdge.MirrorSocket != null ? dotEdge.MirrorSocket.SocketIndex : dotEdge.Socket.SocketIndex);
            int socketFrom = dotEdge.SocketTo == null ? 0 : dotEdge.SocketTo.SocketIndex;
            AssemblePart(dotEdge.Right as DotNode, dronePartR, socketTo, socketFrom, edges, parent, mirror);

            if (dotEdge.Mirror is { SocketMirror: true })
            {
                AssemblePart(dotEdge.Right as DotNode, dronePartR, dotEdge.MirrorSocket.SocketIndex, socketFrom, edges, parent, !mirror);
            }
        }

        return dronePartR;
    }

    public DotGraph DroneGenerate()
    {
        List<DotNode> nodes = new List<DotNode>();
        List<DotEdge> edges = new List<DotEdge>();
        
        foreach (var element in DroneGraph.Instance.DirectedGraph.Elements)
        {
            switch (element)
            {
                case DotNode node:
                    nodes.Add(node);
                    break;
                
                case DotEdge edge:
                    edges.Add(edge);
                    break;
            }
        }

        DotGraph droneGraph = new DotGraph();

        DotNode rootNode = nodes.Find(x => x.Label.Text == "root").Clone() as DotNode;
        droneGraph.Elements.Add(rootNode);

        PartGenerate(rootNode, droneGraph, nodes, edges, 0);
        
        return droneGraph;
    }
    
    public void PartGenerate(DotNode node, DotGraph graph, List<DotNode> nodes, List<DotEdge> edges, int depth)
    {
        if (label2DroneParts[node.Label.Text] == null)
            return;
        
        for (int i = 0; i < label2DroneParts[node.Label.Text].sockets.Length; i++)
        {
            DotNode baseNode = nodes.Find(x => x.Label == node.Label);
            List<DotEdge> dotEdges = edges.FindAll(x => x.Left == baseNode && x.Socket != null && x.Socket.SocketIndex == i);
            if (dotEdges.Count == 0)
                continue;

            DotEdge edge = dotEdges[Random.Range(0, dotEdges.Count)].Clone() as DotEdge;
            DotNode partNode = (edge.Right as DotNode).Clone() as DotNode;
            edge.Left = node;
            edge.Right = partNode;
            
            graph.Elements.Add(partNode);
            graph.Elements.Add(edge);

            if (depth < 4)
            {
                PartGenerate(partNode, graph, nodes, edges, ++depth);
            }
        }
    }

    public static string HullModelString(DotGraph drone)
    {
        List<DotNode> nodes = new List<DotNode>();
        List<DotEdge> edges = new List<DotEdge>();
        
        foreach (var element in drone.Elements)
        {
            switch (element)
            {
                case DotNode node:
                    nodes.Add(node);
                    break;
                
                case DotEdge edge:
                    edges.Add(edge);
                    break;
            }
        }

        List<int> result = new List<int>();
        result.Add(nodes.Count);

        foreach (DotNode node in nodes)
        {
            result.Add(DroneGraph.Instance.DirectedGraph.Elements.FindIndex(x => (x as DotNode)?.Label == node.Label));
        }

        foreach (DotEdge edge in edges)
        {
            result.Add(nodes.FindIndex(x=> x == edge.Left));
            result.Add(nodes.FindIndex(x=> x == edge.Right));
        }

        return string.Join(" ", result);
    }
}