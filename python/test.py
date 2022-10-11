from mlagents_envs.environment import UnityEnvironment
from mlagents_envs.side_channel.engine_configuration_channel import EngineConfigurationChannel

from mlagents_envs.environment import UnityEnvironment
from gym_unity.envs import UnityToGymWrapper

channel = EngineConfigurationChannel()
channel.set_configuration_parameters(time_scale=1.0)
# This is a non-blocking call that only loads the environment.
env = UnityEnvironment(file_name="../env/DroneHullAssembly",
                       seed=1,
                       side_channels=[channel],
                       no_graphics=False)

# Start interacting with the environment.
env.reset()
behavior_names = list(env.behavior_specs.keys())
behavior_name = behavior_names[0]
spec = env.behavior_specs[behavior_name]

DecisionSteps, TerminalSteps = env.get_steps(behavior_name)
print(f"{DecisionSteps}/{TerminalSteps}")

while True:
    env.step()
