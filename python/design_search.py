# Math & etc.
import random
import os
import path_vars

# Grammar
import pydot
import graph_learning.drone_graph as dg

# Unity Channels
from side_channels.valid_design_side_channel import ValidateDesignSideChannel
from mlagents_envs.side_channel.engine_configuration_channel import EngineConfigurationChannel
from mlagents_envs.side_channel.engine_configuration_channel import EngineConfig

# Unity envs
from mlagents_envs.environment import UnityEnvironment
from gym_unity.envs import UnityToGymWrapper

# ML
from stable_baselines3.common.vec_env.subproc_vec_env import SubprocVecEnv
from stable_baselines3.common.vec_env.dummy_vec_env import DummyVecEnv
from stable_baselines3.common.monitor import Monitor
from stable_baselines3.common.logger import configure
from stable_baselines3 import PPO

valid_design_channel = ValidateDesignSideChannel()

try:
    from mpi4py import MPI
except ImportError:
    MPI = None

def make_unity_env(pattern, rank = 0):
    """
    Create a wrapped, monitored Unity environment.
    """
    engine_channel = EngineConfigurationChannel()
    engine_channel.set_configuration(EngineConfig(600, 600, 1, 20.0, -1, 30))
    unity_env = UnityEnvironment(file_name="../env/DroneHullAssembly",
                                 side_channels=[engine_channel, valid_design_channel],
                                 no_graphics=rank != 1,
                                 worker_id=rank,
                                 additional_args=["-e", "1,1"] + ["-d"] + pattern)

    return unity_env

# https://github.com/Unity-Technologies/ml-agents/blob/56e6d333a52863785e20c34d89faadf0a115d320/gym-unity/README.md
def make_env(pattern, num_env, visual, start_index=0):
    """
    Create a wrapped, monitored Unity environment.
    """
    def _env(rank): # pylint: disable=C0111
        def _thunk():
            unity_env = make_unity_env(pattern, rank)
            env = UnityToGymWrapper(unity_env)
            #env = Monitor(env, log_dir and os.path.join(log_dir, str(rank)))
            return env
        return _thunk

    if visual:
        return SubprocVecEnv([_env(i + start_index) for i in range(num_env)])
    else:
        rank = MPI.COMM_WORLD.Get_rank() if MPI else 0
        return DummyVecEnv([_env(rank + i) for i in range(num_env)])


def search_algo(args, design_graph):
    if args["seed"] is not None:
        random.seed(args["seed"])

    # Load/Generate design graph
    dgraph = dg.generate_design(design_graph)

    design_pattern = dg.get_design_pattern(design_graph, dgraph)
    print("pattern design: ",' '.join(design_pattern))

    # Validating design
    unity_env = make_unity_env(design_pattern)

    unity_env.reset()
    if not valid_design_channel.get_result_blocking():
        unity_env.close()
        return

    unity_env.close()

    # Training design
    env = make_env(design_pattern, args["num_env"], True)

    model = PPO("MlpPolicy", env,
                verbose=1,
                tensorboard_log=path_vars.LOG_DIR,
                learning_rate=1e-3)
    model.learn(total_timesteps=args["total_timesteps"])
    model.save(os.path.join(path_vars.MODELS_DIR, "".join(design_pattern)))
    #del model # remove to demonstrate saving and loading

def main():
    args = {
        "num_env": 8,
        "total_timesteps": 1_000_000,
        "seed": 1
    }

    graphs = pydot.graph_from_dot_file("../data/designs/graph_23oct.dot")
    design_graph = graphs[0]

    search_algo(args, design_graph)


if __name__ == '__main__':
    main()