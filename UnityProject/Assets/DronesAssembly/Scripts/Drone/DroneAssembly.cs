using System.Collections.Generic;
using System.Linq;
using DotNetGraph;
using DotNetGraph.Edge;
using DotNetGraph.Node;
using UnityEngine;
using DronesAssembly.Math;

public class DroneAssembly : Singleton<DroneAssembly>
{
    static string MIRROR_TAG = "[mirror]";
    public static bool DebugVisual = false;

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

        if (mirror)
        {
            dronePartR.gameObject.name += MIRROR_TAG;
        }

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
            int socketFrom = 0;
            AssemblePart(dotEdge.Right as DotNode, dronePartR, socketTo, socketFrom, edges, parent, mirror);

            if (dotEdge.Mirror is { SocketMirror: true })
            {
                AssemblePart(dotEdge.Right as DotNode, dronePartR, dotEdge.MirrorSocket.SocketIndex, socketFrom, edges, parent, !mirror);
            }
        }

        return dronePartR;
    }

    public static bool CheckDroneSelfCollision(DroneAgent droneAgent)
    {
        List<DronePart> parts = droneAgent.GetComponentsInChildren<DronePart>().ToList();
        //parts = parts.FindAll(x => !x.gameObject.name.Contains(MIRROR_TAG));

        for (int i = 0; i < parts.Count; i++)
        {
            var partL = parts[i];
            Collider[] collidersL = partL.GetComponents<Collider>();
            
            for (int j = i + 1; j < parts.Count; j++)
            {
                var partR = parts[j];
                Collider[] collidersR = partR.GetComponents<Collider>();

                foreach (Collider colliderL in collidersL)
                foreach (Collider colliderR in collidersR)
                {
                    if (partL.transform.parent == partR.transform 
                        || partR.transform.parent == partL.transform)
                        continue;

                    if (BoxSolver.Intersects(
                            colliderL.transform.position, colliderL.bounds.extents, colliderL.transform.rotation,
                            colliderR.transform.position, colliderR.bounds.extents, colliderR.transform.rotation))
                        return true;
                }
            }
        }

        return false;
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
        if (node.SocketCount.SocketIndex <= 1)
            return;

        List<int> ignoreSockets = new List<int>();
        for (int i = 0; i < node.SocketCount.SocketIndex; i++)
        {
            if (ignoreSockets.Contains(i))
                continue;
            
            List<DotEdge> dotEdges = edges.FindAll(x => (x.Left as DotNode)?.Identifier == node.Identifier 
                                                        && (x.Socket != null && x.Socket.SocketIndex == i || x.Socket == null));
            if (dotEdges.Count == 0)
                continue;

            DotEdge edge;
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
            
            PartGenerate(partNode, graph, edges, depth + 1);
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
        
        Queue<DotNode> pendingNodes = new Queue<DotNode>();
        pendingNodes.Enqueue(nodes.Find(x => x.Identifier == "root"));
        
        while (pendingNodes.Count > 0)
        {
            DotNode node = pendingNodes.Dequeue();
            result.Add(DroneGraph.Instance.DirectedGraph.Elements.FindIndex(x => (x as DotNode)?.Identifier == node.Identifier));
            
            GetConnectedNodes(node, edges).ForEach(x => pendingNodes.Enqueue(x));
        }

        return string.Join(" ", result);
    }

    private static List<DotNode> GetConnectedNodes(DotNode node, List<DotEdge> edges)
    {
        List<DotNode> connectedNodes = edges.FindAll(x => (x.Left as DotNode) == node).ConvertAll(x => x.Right as DotNode);
        return connectedNodes;
    }

    public static DotGraph ParseFromArgsOld(string[] args)
    {
        int[] dataArray = args.Select(int.Parse).ToArray();

        if (dataArray.Length == 0)
        {
            Debug.LogError($"Could not parse drone from string");
            return null;
        }

        DotGraph droneGraph = new DotGraph();
        
        int index = 0;
        int partsCount = dataArray[index++];
        
        while (index <= partsCount)
        {
            DotNode node = DroneGraph.Instance.DirectedGraph.Elements[dataArray[index++]] as DotNode;
            node = node.Clone() as DotNode;
            
            droneGraph.Elements.Add(node);
        }
        
        while (index < dataArray.Length - 1)
        {
            int edgeIndex = dataArray[index++];
            int leftIndex = dataArray[index++];
            int rightIndex = dataArray[index++];
            
            DotEdge edge = DroneGraph.Instance.DirectedGraph.Elements[edgeIndex] as DotEdge;
            edge = edge.Clone() as DotEdge;
            edge.Left = droneGraph.Elements[leftIndex];
            edge.Right = droneGraph.Elements[rightIndex];
            
            droneGraph.Elements.Add(edge);
        }

        return droneGraph;
    }
    
    public static DotGraph ParseFromArgs(string[] args)
    {
        int[] dataArray = args.Select(int.Parse).ToArray();

        if (dataArray.Length == 0)
        {
            Debug.LogError($"Could not parse drone from string");
            return null;
        }

        List<DotEdge> edges = DroneGraph.Instance.DirectedGraph.Elements.FindAll(x => x is DotEdge).ConvertAll(x => x as DotEdge);
        
        DotGraph droneGraph = new DotGraph();
        Queue<DotNode> pendingNodes = new Queue<DotNode>();

        int index = 0;
        DotNode nodeRoot = DroneGraph.Instance.DirectedGraph.Elements[dataArray[index++]] as DotNode;
        nodeRoot = nodeRoot.Clone() as DotNode;
        droneGraph.Elements.Add(nodeRoot);
        pendingNodes.Enqueue(nodeRoot);

        while (pendingNodes.Count > 0)
        {
            DotNode node = pendingNodes.Dequeue();
            ProcessDesignArgs(droneGraph, edges, node, dataArray, ref index).ForEach(x => pendingNodes.Enqueue(x));
        }

        return droneGraph;
    }

    private static List<DotNode> ProcessDesignArgs(DotGraph droneGraph, List<DotEdge> edges, DotNode node, int[] dataArray, ref int index)
    {
        List<DotNode> nodes = new List<DotNode>();

        if (!DroneGraph.Instance.AvailableSockets.TryGetValue(node.Identifier, out var sockets))
            return nodes;

        foreach (var socket in sockets)
        {
            DotNode childNode = DroneGraph.Instance.DirectedGraph.Elements[dataArray[index++]] as DotNode;
            childNode = childNode.Clone() as DotNode;
            droneGraph.Elements.Add(childNode);
            
            DotEdge edge = edges.Find(x => (x.Left as DotNode).Identifier == node.Identifier 
                                           && (x.Right as DotNode).Identifier == childNode.Identifier
                                           && x.Socket.SocketIndex == socket);
            
            edge = edge.Clone() as DotEdge;
            edge.Left = node;
            edge.Right = childNode;
            droneGraph.Elements.Add(edge);
            
            nodes.Add(childNode);
        }

        return nodes;
    }
}