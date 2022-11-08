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


class TreeNode:
    childs = {}
    parent = None

    graph_node = None

    def add_child(self, socket, node):
        self.childs[socket] = node


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
        edge.obj_dict['points_ids'] = [ node, next_node ]

        # We add the mirrored socket, so it won't add part to it twice
        if "socket_mirror" in edge.get_attributes() and "mirror" in edge.get_attributes():
            ignore_sockets.append(int(edge.get_attributes()["socket_mirror"]))

        dgraph.add_node(next_node)
        dgraph.add_edge(edge)

        generate_part(dgraph, next_node, design_graph, depth + 1)


# Parse block
def get_design_pattern(design_graph, dgraph):
    pattern = []

    pending_nodes = []

    root = next(x for x in dgraph.get_nodes() if x.get_name() == "root")
    pending_nodes.append(root)

    while len(pending_nodes) > 0:
        node = pending_nodes.pop(0)

        pattern.append(next(i for i, v in enumerate(design_graph.get_nodes()) if node.get_name() == v.get_name()))

        for n in get_connected_nodes(node, dgraph.get_edges()):
            pending_nodes.append(n)

    return [str(i) for i in pattern]


def get_connected_nodes(node, edges):
    node_edges = list(filter(lambda v: v.obj_dict['points_ids'][0] == node, edges))
    return [n.obj_dict['points_ids'][1] for n in node_edges]


# Block pattern from dgraph
def dgraph_from_pattern(design_graph, pattern):
    if len(pattern) == 0:
        print("[Error]: pattern is empty")
        return

    pattern = [int(n) for n in pattern]

    dgraph = DGraph()
    index = 0

    nodes = design_graph.get_nodes()
    edges = design_graph.get_edges()
    sockets = get_sockets_dict(design_graph)

    pending_nodes = []
    root_node = deepcopy(next(v for v in nodes if v.get_name() == "root"))
    dgraph.add_node(root_node)
    pending_nodes.append(root_node)

    while len(pending_nodes) > 0:
        node = pending_nodes.pop(0)
        child_nodes, index = process_design_args(dgraph, edges, sockets, node, pattern, index)

        for x in child_nodes:
            pending_nodes.append(x)

    return dgraph


def get_sockets_dict(design_graph):
    sockets = {}

    edges = design_graph.get_edges()

    for node in design_graph.get_nodes():
        for edge in filter(lambda v: v.get_source() == node.get_name(), edges):
            socket = edge.get_attributes()["socket"]
            if node.get_name() in sockets:
                if socket not in sockets[node.get_name()]:
                    sockets[node.get_name()].append(socket)
            else:
                sockets[node.get_name()] = [socket]

    return sockets


def process_design_args(dgraph, edges, sockets, node, pattern, index):
    nodes = []

    if node.get_name() not in sockets:
        return nodes

    for socket in sockets[node.get_name()]:
        child_node = deepcopy(nodes[pattern[index]])
        dgraph.add_node(child_node)
        index += 1

        edge = next(v for v in edges if v.get_source() == node.get_name()
             and v.get_destination() == child_node.get_name()
             and v.get_attributes()["socket"] == socket)
        edge = deepcopy(edge)

        edge.obj_dict['points_ids'] = [node, child_node]
        dgraph.add_edge(edge)
        nodes.append(node)

    return nodes, index


def dgraph_from_pattern_old(design_graph, pattern):
    if len(pattern) == 0:
        print("[Error]: pattern is empty")
        return

    pattern = [int(n) for n in pattern]

    dgraph = DGraph()
    index = 0

    partsCount = pattern[index]
    index += 1

    nodes = design_graph.get_nodes()
    edges = design_graph.get_edges()
    while index <= partsCount:
        node = nodes[pattern[index]]
        node = deepcopy(node)
        index += 1

        dgraph.add_node(node)

    while index < len(pattern) - 1:
        edgeIndex = pattern[index] - len(nodes) + 1
        index += 1

        leftIndex = pattern[index]
        leftNode = dgraph.get_nodes()[leftIndex]
        index += 1

        rightIndex = pattern[index]
        rightNode = dgraph.get_nodes()[rightIndex]
        index += 1

        edge = edges[edgeIndex]
        edge = deepcopy(edge)
        edge.obj_dict['points_ids'] = [ leftNode, rightNode ]

        dgraph.add_edge(edge)

    return dgraph

