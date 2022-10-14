using DotNetGraph;
using DotNetGraph.Edge;
using DotNetGraph.Node;

public class DroneGraph : Singleton<DroneGraph>
{
    public readonly DotGraph DirectedGraph = new DotGraph("DroneGraph", true);

    public DroneGraph()
    {
        var rootNode = new DotNode("root")
        {
            Label = "root",
        };
        
        var bodyNode = new DotNode("body")
        {
            Label = "body",
        };
        
        var limbNode = new DotNode("limb")
        {
            Label = "limb"
        };
        
        var limbEndNode = new DotNode("limb_end")
        {
            Label = "limb_end"
        };
        
        var fixedKneeNode = new DotNode("fixed_knee")
        {
            Label = "fixed_knee"
        };
        
        var propellerNode = new DotNode("propeller")
        {
            Label = "propeller",
        };
        
        DirectedGraph.Elements.Add(rootNode);
        DirectedGraph.Elements.Add(bodyNode);
        DirectedGraph.Elements.Add(limbNode);
        DirectedGraph.Elements.Add(limbEndNode);
        DirectedGraph.Elements.Add(fixedKneeNode);
        DirectedGraph.Elements.Add(propellerNode);
        
        
        DirectedGraph.Elements.Add(new DotEdge(rootNode, limbNode) { Socket = 0, MirrorSocket = 1, Mirror = true });
        DirectedGraph.Elements.Add(new DotEdge(rootNode, limbEndNode));
        
        DirectedGraph.Elements.Add(new DotEdge(rootNode, bodyNode) { Socket = 2 });
        DirectedGraph.Elements.Add(new DotEdge(rootNode, bodyNode) { Socket = 3 });
        
        DirectedGraph.Elements.Add(new DotEdge(rootNode, limbNode) { Socket = 1 });
        DirectedGraph.Elements.Add(new DotEdge(rootNode, limbNode) { Socket = 2 });
        DirectedGraph.Elements.Add(new DotEdge(rootNode, limbNode) { Socket = 3 });
        

        DirectedGraph.Elements.Add(new DotEdge(bodyNode, limbNode) { Socket = 0, MirrorSocket = 1, Mirror = true });
        DirectedGraph.Elements.Add(new DotEdge(bodyNode, limbEndNode));
        
        DirectedGraph.Elements.Add(new DotEdge(bodyNode, bodyNode) { Socket = 2 });
        
        DirectedGraph.Elements.Add(new DotEdge(bodyNode, limbNode) { Socket = 1 });
        DirectedGraph.Elements.Add(new DotEdge(bodyNode, limbNode) { Socket = 2 });
        DirectedGraph.Elements.Add(new DotEdge(bodyNode, limbNode) { Socket = 3 });

        
        DirectedGraph.Elements.Add(new DotEdge(limbNode, fixedKneeNode) { Socket = 1 });
        DirectedGraph.Elements.Add(new DotEdge(limbNode, limbNode) { Socket = 1 });
        DirectedGraph.Elements.Add(new DotEdge(limbNode, propellerNode) { Socket = 1 });
        
        
        DirectedGraph.Elements.Add(new DotEdge(fixedKneeNode, limbNode) { Socket = 1 });
        DirectedGraph.Elements.Add(new DotEdge(fixedKneeNode, limbNode) { Socket = 2, MirrorSocket = 3 });
        DirectedGraph.Elements.Add(new DotEdge(fixedKneeNode, limbNode) { Socket = 3, MirrorSocket = 2 });
        
        DirectedGraph.Elements.Add(new DotEdge(fixedKneeNode, limbEndNode) { Socket = 1 });
        DirectedGraph.Elements.Add(new DotEdge(fixedKneeNode, limbEndNode) { Socket = 2, MirrorSocket = 3 });
        DirectedGraph.Elements.Add(new DotEdge(fixedKneeNode, limbEndNode) { Socket = 3, MirrorSocket = 2 });

        //File.WriteAllText("myFile.dot", m_DirectedGraph.Compile());
    }
}