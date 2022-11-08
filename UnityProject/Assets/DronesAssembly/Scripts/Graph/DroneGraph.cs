using System.Collections.Generic;
using System.IO;
using DotNetGraph;
using DotNetGraph.Attributes;
using DotNetGraph.Edge;
using DotNetGraph.Extensions;
using DotNetGraph.Node;

public class DroneGraph : Singleton<DroneGraph>
{
    public readonly DotGraph DirectedGraph = new DotGraph("drone_graph", true);
    public readonly Dictionary<string, List<int>> AvailableSockets = new Dictionary<string, List<int>>();

    public DroneGraph()
    {
        var rootNode = new DotNode("root") { SocketCount = 4 };
        var limbNode = new DotNode("limb") { SocketCount = 2 };
        var limbForwardNode = new DotNode("limb_forward") { SocketCount = 2 };
        var limbEndNode = new DotNode("limb_end") { SocketCount = 1 };
        var fixedKneeNode = new DotNode("fixed_knee") { SocketCount = 4 };
        var fixedForwardKneeNode = new DotNode("fixed_knee_forward") { SocketCount = 4 };
        var motorKneeNode = new DotNode("motor_knee") { SocketCount = 4 };
        var motorForwardKneeNode = new DotNode("motor_knee_forward") { SocketCount = 4 };
        var propellerNode = new DotNode("propeller") { SocketCount = 1 };
        
        DirectedGraph.Elements.Add(rootNode);
        DirectedGraph.Elements.Add(limbNode);
        DirectedGraph.Elements.Add(limbForwardNode);
        DirectedGraph.Elements.Add(limbEndNode);
        DirectedGraph.Elements.Add(fixedKneeNode);
        DirectedGraph.Elements.Add(fixedForwardKneeNode);
        DirectedGraph.Elements.Add(motorKneeNode);
        DirectedGraph.Elements.Add(motorForwardKneeNode);
        DirectedGraph.Elements.Add(propellerNode);
        
        DirectedGraph.Elements.Add(new DotEdge(rootNode, limbForwardNode) { Socket = 0 });
        DirectedGraph.Elements.Add(new DotEdge(rootNode, limbForwardNode) { Socket = 1 });
        DirectedGraph.Elements.Add(new DotEdge(rootNode, limbNode) { Socket = 2, MirrorSocket = 3, Mirror = true });
        DirectedGraph.Elements.Add(new DotEdge(rootNode, limbEndNode) { Socket = 0});
        DirectedGraph.Elements.Add(new DotEdge(rootNode, limbEndNode) { Socket = 1 });
        

        DirectedGraph.Elements.Add(new DotEdge(limbNode, fixedKneeNode) { Socket = 1 });
        DirectedGraph.Elements.Add(new DotEdge(limbNode, motorKneeNode) { Socket = 1 });
        DirectedGraph.Elements.Add(new DotEdge(limbNode, limbNode) { Socket = 1 });
        DirectedGraph.Elements.Add(new DotEdge(limbNode, propellerNode) { Socket = 1 });
        
        
        DirectedGraph.Elements.Add(new DotEdge(limbForwardNode, fixedForwardKneeNode) { Socket = 1 });
        DirectedGraph.Elements.Add(new DotEdge(limbForwardNode, motorForwardKneeNode) { Socket = 1 });
        DirectedGraph.Elements.Add(new DotEdge(limbForwardNode, limbForwardNode) { Socket = 1 });
        DirectedGraph.Elements.Add(new DotEdge(limbForwardNode, propellerNode) { Socket = 1 });
        
        
        DirectedGraph.Elements.Add(new DotEdge(fixedKneeNode, limbNode) { Socket = 1 });
        DirectedGraph.Elements.Add(new DotEdge(fixedKneeNode, limbNode) { Socket = 2, MirrorSocket = 3 });
        DirectedGraph.Elements.Add(new DotEdge(fixedKneeNode, limbNode) { Socket = 3, MirrorSocket = 2 });
        
        DirectedGraph.Elements.Add(new DotEdge(fixedKneeNode, limbEndNode) { Socket = 1 });
        DirectedGraph.Elements.Add(new DotEdge(fixedKneeNode, limbEndNode) { Socket = 2, MirrorSocket = 3 });
        DirectedGraph.Elements.Add(new DotEdge(fixedKneeNode, limbEndNode) { Socket = 3, MirrorSocket = 2 });
        
        
        DirectedGraph.Elements.Add(new DotEdge(motorKneeNode, limbNode) { Socket = 1 });
        DirectedGraph.Elements.Add(new DotEdge(motorKneeNode, limbNode) { Socket = 2, MirrorSocket = 3 });
        DirectedGraph.Elements.Add(new DotEdge(motorKneeNode, limbNode) { Socket = 3, MirrorSocket = 2 });
        
        DirectedGraph.Elements.Add(new DotEdge(motorKneeNode, limbEndNode) { Socket = 1 });
        DirectedGraph.Elements.Add(new DotEdge(motorKneeNode, limbEndNode) { Socket = 2, MirrorSocket = 3 });
        DirectedGraph.Elements.Add(new DotEdge(motorKneeNode, limbEndNode) { Socket = 3, MirrorSocket = 2 });
        
        
        DirectedGraph.Elements.Add(new DotEdge(fixedForwardKneeNode, limbForwardNode) { Socket = 1 });
        DirectedGraph.Elements.Add(new DotEdge(fixedForwardKneeNode, limbNode) { Socket = 2, MirrorSocket = 3, Mirror = true });
        
        DirectedGraph.Elements.Add(new DotEdge(fixedForwardKneeNode, propellerNode) { Socket = 1 });
        DirectedGraph.Elements.Add(new DotEdge(fixedForwardKneeNode, propellerNode) { Socket = 2, MirrorSocket = 3, Mirror = true });
        
        DirectedGraph.Elements.Add(new DotEdge(fixedForwardKneeNode, limbEndNode) { Socket = 1 });
        DirectedGraph.Elements.Add(new DotEdge(fixedForwardKneeNode, limbEndNode) { Socket = 2, MirrorSocket = 3, Mirror = true });
        
        
        DirectedGraph.Elements.Add(new DotEdge(motorForwardKneeNode, limbForwardNode) { Socket = 1 });
        DirectedGraph.Elements.Add(new DotEdge(motorForwardKneeNode, limbNode) { Socket = 2, MirrorSocket = 3, Mirror = true });
        
        DirectedGraph.Elements.Add(new DotEdge(motorForwardKneeNode, propellerNode) { Socket = 1 });
        DirectedGraph.Elements.Add(new DotEdge(motorForwardKneeNode, propellerNode) { Socket = 2, MirrorSocket = 3, Mirror = true });
        
        DirectedGraph.Elements.Add(new DotEdge(motorForwardKneeNode, limbEndNode) { Socket = 1 });
        DirectedGraph.Elements.Add(new DotEdge(motorForwardKneeNode, limbEndNode) { Socket = 2, MirrorSocket = 3, Mirror = true });

        var edges = DirectedGraph.Elements.FindAll(x => x is DotEdge).ConvertAll(x => x as DotEdge);
        
        foreach (DotNode node in DirectedGraph.Elements.FindAll(x => x is DotNode).ConvertAll(x => x as DotNode))
        {
            foreach (var nodeEdge in edges.FindAll(x => (x.Left as DotNode).Identifier == node.Identifier))
            {
                if (AvailableSockets.ContainsKey(node.Identifier))
                {
                    if (!AvailableSockets[node.Identifier].Contains(nodeEdge.Socket.SocketIndex))
                    {
                        AvailableSockets[node.Identifier].Add(nodeEdge.Socket.SocketIndex);
                    }
                }
                else
                {
                    AvailableSockets.Add(node.Identifier, new List<int>{nodeEdge.Socket.SocketIndex});
                }
            }
        }

        //File.WriteAllText("../data/graph/new_graph.dot", DirectedGraph.Compile());
    }
}