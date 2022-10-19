from mlagents_envs.environment import UnityEnvironment
from mlagents_envs.side_channel.engine_configuration_channel import EngineConfigurationChannel

from mlagents_envs.environment import UnityEnvironment
from gym_unity.envs import UnityToGymWrapper

channel = EngineConfigurationChannel()

def MakeEnvironment(design):
    # This is a non-blocking call that only loads the environment.
    unity_env = UnityEnvironment(file_name="../env/DroneHullAssembly",
                           side_channels=[channel],
                           no_graphics=False,
                           additional_args=["-d"] + design)

    return unity_env


design = "11 0 2 2 8 3 1 6 3 3 1 8 9 0 1 22 1 2 23 2 3 13 0 4 11 0 5 16 5 6 27 6 7 14 6 8 26 6 9 19 9 10".split(" ")
env = MakeEnvironment(design)

# Start interacting with the environment.
env.reset()
behavior_names = list(env.behavior_specs.keys())
behavior_name = behavior_names[0]
spec = env.behavior_specs[behavior_name]

DecisionSteps, TerminalSteps = env.get_steps(behavior_name)
print(f"{DecisionSteps}/{TerminalSteps}")

while True:
    env.step()
