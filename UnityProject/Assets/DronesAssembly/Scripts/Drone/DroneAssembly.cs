using System.Collections.Generic;
using DotNetGraph;
using DotNetGraph.Edge;
using DotNetGraph.Node;
using UnityEngine;

public class DroneAssembly : Singleton<DroneAssembly>
{
    [SerializeField] 
    private SerializableDictionary<string, DronePart> label2DroneParts;
        
    public static DronePart AssembleDrone(DotGraph drone, Transform parent)
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

        DotNode rootNode = nodes.Find(node => node.Identifier == "root");

        DronePart dronePart = Instance.AssemblePart(rootNode, null, 0, 0, edges, parent, false);

        return dronePart;
    }

    private DronePart AssemblePart(DotNode element, DronePart dronePartL, int socketInxTo, int socketInxFrom, List<DotEdge> edges, Transform parent, bool mirror)
    {
        if (label2DroneParts[element.Identifier] == null)
            return null;
        
        DronePart dronePartR = Instantiate(label2DroneParts[element.Identifier], parent);

        if (dronePartL != null)
        {
            Transform socketFrom = dronePartL.sockets[socketInxTo];
            Transform socketTo = dronePartR.sockets[socketInxFrom];

            dronePartR.transform.rotation = socketFrom.rotation * Quaternion.Inverse(socketTo.localRotation * Quaternion.Euler(0, 180, 0));
            dronePartR.transform.position = socketFrom.position - dronePartR.transform.TransformDirection(socketTo.localPosition * socketTo.lossyScale.x);
            
            dronePartR.transform.SetParent(dronePartL.transform, true);
        }

        List<DotEdge> dotEdges = edges.FindAll(edge => edge.Left == element);

        foreach (DotEdge dotEdge in dotEdges)
        {
            int socketTo = (mirror && dotEdge.MirrorSocket != null ? dotEdge.MirrorSocket.SocketIndex : dotEdge.Socket.SocketIndex);
            int socketFrom = dotEdge.SocketTo?.SocketIndex ?? 0;
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

        DotNode rootNode = nodes.Find(x => x.Identifier == "root").Clone() as DotNode;
        droneGraph.Elements.Add(rootNode);

        PartGenerate(rootNode, droneGraph, edges, 0);
        
        return droneGraph;
    }
    
    public void PartGenerate(DotNode node, DotGraph graph, List<DotEdge> edges, int depth)
    {
        if (label2DroneParts[node.Identifier] == null)
            return;

        List<int> ignoreSockets = new List<int>();
        for (int i = 0; i < label2DroneParts[node.Identifier].sockets.Length; i++)
        {
            if (ignoreSockets.Contains(i))
                continue;
            
            List<DotEdge> dotEdges = edges.FindAll(x => (x.Left as DotNode)?.Identifier == node.Identifier 
                                                        && (x.Socket != null && x.Socket.SocketIndex == i || x.Socket == null));
            if (dotEdges.Count == 0)
                continue;

            DotEdge edge = dotEdges[Random.Range(0, dotEdges.Count)].Clone() as DotEdge;;
            if (depth >= 5 && dotEdges.Find(x => (x.Right as DotNode)?.Identifier == "propeller") is { } edgeFound)
            {
                edge = edgeFound.Clone() as DotEdge;
            }
            else
            {
                edge = dotEdges[Random.Range(0, dotEdges.Count)].Clone() as DotEdge;
            }

            DotNode partNode = (edge.Right as DotNode).Clone() as DotNode;
            edge.Left = node;
            edge.Right = partNode;

            // We add the mirrored socket so it won't add part to it twice
            if (edge.MirrorSocket != null && edge.Mirror is { SocketMirror: true })
            {
                ignoreSockets.Add(edge.MirrorSocket.SocketIndex);
            }
            
            graph.Elements.Add(partNode);
            graph.Elements.Add(edge);
            
            PartGenerate(partNode, graph, edges, ++depth);
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
            result.Add(DroneGraph.Instance.DirectedGraph.Elements.FindIndex(x => (x as DotNode)?.Identifier == node.Identifier));
        }

        foreach (DotEdge edge in edges)
        {
            result.Add(nodes.FindIndex(x=> x == edge.Left));
            result.Add(nodes.FindIndex(x=> x == edge.Right));
        }

        return string.Join(" ", result);
    }
}