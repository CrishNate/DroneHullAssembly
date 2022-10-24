import pydot
import design_search.drone_graph as dg

from mlagents_envs.environment import UnityEnvironment
from mlagents_envs.side_channel.engine_configuration_channel import EngineConfigurationChannel

from mlagents_envs.environment import UnityEnvironment
from gym_unity.envs import UnityToGymWrapper

channel = EngineConfigurationChannel()


def MakeEnvironment(pattern):
    # This is a non-blocking call that only loads the environment.
    unity_env = UnityEnvironment(file_name="../env/DroneHullAssembly",
                                 side_channels=[channel],
                                 no_graphics=False,
                                 additional_args=["-d"] + pattern)

    return unity_env


# Load graph
graphs = pydot.graph_from_dot_file("../data/graph/graph_23oct.dot")
design_graph = graphs[0]
dgraph = dg.GenerateDesign(design_graph)

pattern = dg.GetDesignPattern(design_graph, dgraph)

print(' '.join(pattern))
unity_env = MakeEnvironment(pattern)

#env = UnityToGymWrapper(unity_env, uint8_visual=True)

# Start interacting with the environment.
unity_env.reset()
behavior_names = list(unity_env.behavior_specs.keys())
behavior_name = behavior_names[0]
spec = unity_env.behavior_specs[behavior_name]

DecisionSteps, TerminalSteps = unity_env.get_steps(behavior_name)
print(f"{DecisionSteps}/{TerminalSteps}")

while True:
    unity_env.step()