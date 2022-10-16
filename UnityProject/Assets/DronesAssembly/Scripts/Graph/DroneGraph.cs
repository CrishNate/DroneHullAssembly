using System.IO;
using DotNetGraph;
using DotNetGraph.Edge;
using DotNetGraph.Extensions;
using DotNetGraph.Node;

public class DroneGraph : Singleton<DroneGraph>
{
    public readonly DotGraph DirectedGraph = new DotGraph("DroneGraph", true);

    public DroneGraph()
    {
        var rootNode = new DotNode("root") {};
        var limbNode = new DotNode("limb") {};
        var limbForwardNode = new DotNode("limb_forward") {};
        var limbEndNode = new DotNode("limb_end") {};
        var fixedKneeNode = new DotNode("fixed_knee") {};
        var fixedForwardKneeNode = new DotNode("fixed_knee_forward") {};
        var motorKneeNode = new DotNode("motor_knee") {};
        var motorForwardKneeNode = new DotNode("motor_knee_forward") {};
        var propellerNode = new DotNode("propeller") {};
        
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
        DirectedGraph.Elements.Add(new DotEdge(fixedKneeNode, limbEndNode) { Socket = 2, MirrorSocket = 3 });
        DirectedGraph.Elements.Add(new DotEdge(fixedKneeNode, limbEndNode) { Socket = 3, MirrorSocket = 2 });
        

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
        DirectedGraph.Elements.Add(new DotEdge(fixedForwardKneeNode, limbNode) { Socket = 3, MirrorSocket = 2, Mirror = true });
        
        DirectedGraph.Elements.Add(new DotEdge(fixedForwardKneeNode, propellerNode) { Socket = 1 });
        DirectedGraph.Elements.Add(new DotEdge(fixedForwardKneeNode, propellerNode) { Socket = 2, MirrorSocket = 3, Mirror = true });
        DirectedGraph.Elements.Add(new DotEdge(fixedForwardKneeNode, propellerNode) { Socket = 3, MirrorSocket = 2, Mirror = true });
        
        DirectedGraph.Elements.Add(new DotEdge(fixedForwardKneeNode, limbEndNode) { Socket = 1 });
        DirectedGraph.Elements.Add(new DotEdge(fixedForwardKneeNode, limbEndNode) { Socket = 2, MirrorSocket = 3, Mirror = true });
        DirectedGraph.Elements.Add(new DotEdge(fixedForwardKneeNode, limbEndNode) { Socket = 3, MirrorSocket = 2, Mirror = true });
        
        
        DirectedGraph.Elements.Add(new DotEdge(motorForwardKneeNode, limbForwardNode) { Socket = 1 });
        DirectedGraph.Elements.Add(new DotEdge(motorForwardKneeNode, limbNode) { Socket = 2, MirrorSocket = 3, Mirror = true });
        DirectedGraph.Elements.Add(new DotEdge(motorForwardKneeNode, limbNode) { Socket = 3, MirrorSocket = 2, Mirror = true });
        
        DirectedGraph.Elements.Add(new DotEdge(motorForwardKneeNode, propellerNode) { Socket = 1 });
        DirectedGraph.Elements.Add(new DotEdge(motorForwardKneeNode, propellerNode) { Socket = 2, MirrorSocket = 3, Mirror = true });
        DirectedGraph.Elements.Add(new DotEdge(motorForwardKneeNode, propellerNode) { Socket = 3, MirrorSocket = 2, Mirror = true });
        
        DirectedGraph.Elements.Add(new DotEdge(motorForwardKneeNode, limbEndNode) { Socket = 1 });
        DirectedGraph.Elements.Add(new DotEdge(motorForwardKneeNode, limbEndNode) { Socket = 2, MirrorSocket = 3, Mirror = true });
        DirectedGraph.Elements.Add(new DotEdge(motorForwardKneeNode, limbEndNode) { Socket = 3, MirrorSocket = 2, Mirror = true });

        //File.WriteAllText("myFile.dot", DirectedGraph.Compile());
    }
}