from copy import copy, deepcopy

import pydot
import random


class DGraph:
    nodes = []
    edges = []

    def add_node(self, node):
        self.nodes.append(node)

    def add_edge(self, edge):
        self.edges.append(edge)

    def get_nodes(self):
        return self.nodes

    def get_edges(self):
        return self.edges



def generate_design(design_graph):
    root = deepcopy(next(x for x in design_graph.get_nodes() if x.get_name() == "root"))

    dgraph = DGraph()
    dgraph.add_node(root)

    generate_part(dgraph, root, design_graph, 0)

    return dgraph


def generate_part(dgraph, node, design_graph, depth):
    if int(node.get_attributes()["socket"]) <= 1:
        return

    ignore_sockets = []

    for i in range(int(node.get_attributes()["socket"])):
        if i in ignore_sockets:
            continue

        # Getting all possible entropy edges for current node
        entropy_edges = list(filter(lambda x: x.get_source() == node.get_name()
                                              and (x.get_attributes()["socket"] is None or int(x.get_attributes()["socket"]) == i),
                                    design_graph.get_edges()))

        if len(entropy_edges) == 0:
            continue

        # Filtering edge that contain propeller end point
        propeller = next((x for x in entropy_edges if x.get_destination() == "propeller"), None)

        if depth >= 5 and propeller is not None:
            edge = propeller
        else:
            edge = random.choice(entropy_edges)

        edge = deepcopy(edge)

        next_node = deepcopy(next(x for x in design_graph.get_nodes() if x.get_name() == edge.get_destination()))
        # Setting edge as ids for its destination and source nodes
        edge.obj_dict['points_ids'] = [ node.obj_dict, next_node.obj_dict ]

        # We add the mirrored socket, so it won't add part to it twice
        if "socket_mirror" in edge.get_attributes() and "mirror" in edge.get_attributes():
            ignore_sockets.append(int(edge.get_attributes()["socket_mirror"]))

        dgraph.add_node(next_node)
        dgraph.add_edge(edge)

        generate_part(dgraph, next_node, design_graph, depth + 1)


def get_design_pattern(design_graph, dgraph):
    pattern = []
    pattern.append(len(dgraph.get_nodes()))

    # we are getting index of original node in design graph
    for node in dgraph.get_nodes():
        pattern.append(next(i for i, v in enumerate(design_graph.get_nodes()) if node.get_name() == v.get_name()))

    for edge in dgraph.get_edges():
        # we are getting index of original edge in design graph
        pattern.append(next(i for i, v in enumerate(design_graph.get_edges())
                            if edge.get_attributes()["socket"] == v.get_attributes()["socket"]
                            and edge.get_source() == v.get_source()
                            and edge.get_destination() == v.get_destination())
                       # we need this, because in unity DOT edges and nodes are stored in one array
                       # So, we need to offset index on nodes count
                       + len(design_graph.get_nodes()) - 1)

        pattern.append(next(i for i, v in enumerate(dgraph.get_nodes()) if v.obj_dict == edge.obj_dict["points_ids"][0]))
        pattern.append(next(i for i, v in enumerate(dgraph.get_nodes()) if v.obj_dict == edge.obj_dict["points_ids"][1]))

    return [str(i) for i in pattern]
