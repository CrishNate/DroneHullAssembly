import time

import pydot
import design_search.drone_graph as dg

from side_channels.valid_design_side_channel import ValidateDesignSideChannel

from mlagents_envs.side_channel.engine_configuration_channel import EngineConfigurationChannel

from mlagents_envs.environment import UnityEnvironment
from gym_unity.envs import UnityToGymWrapper

engine_channel = EngineConfigurationChannel()
valid_design_channel = ValidateDesignSideChannel()


def MakeEnvironment(design):
    # This is a non-blocking call that only loads the environment.
    unity_env = UnityEnvironment(file_name="../env/DroneHullAssembly",
                                 side_channels=[engine_channel, valid_design_channel],
                                 no_graphics=False,
                                 additional_args=["-d"] + design)

    return unity_env


# Load/Generate design graph
graphs = pydot.graph_from_dot_file("../data/graph/graph_23oct.dot")
design_graph = graphs[0]
dgraph = dg.GenerateDesign(design_graph)

design_pattern = dg.GetDesignPattern(design_graph, dgraph)
print(' '.join(design_pattern))

unity_env = MakeEnvironment(design_pattern)

# Binding validate design
def OnValidateDesign(result: bool):
    print(result)


valid_design_channel.bind_received(OnValidateDesign)

# Start interacting with the environment.
unity_env.reset()
behavior_names = list(unity_env.behavior_specs.keys())
behavior_name = behavior_names[0]
spec = unity_env.behavior_specs[behavior_name]

DecisionSteps, TerminalSteps = unity_env.get_steps(behavior_name)
print(f"{DecisionSteps}/{TerminalSteps}")

while True:
    unity_env.step()