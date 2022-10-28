# Math & etc.
import random
import os

# Grammar
import pydot
import design_search.drone_graph as dg

# Unity Channels
from side_channels.valid_design_side_channel import ValidateDesignSideChannel
from mlagents_envs.side_channel.engine_configuration_channel import EngineConfigurationChannel

# Unity envs
from mlagents_envs.environment import UnityEnvironment
from gym_unity.envs import UnityToGymWrapper

# ML
import tensorflow.compat.v1 as tf
tf.disable_v2_behavior()

from baselines.common.vec_env.subproc_vec_env import SubprocVecEnv
from baselines.common.vec_env.dummy_vec_env import DummyVecEnv
from baselines.bench import Monitor
from baselines import logger
import baselines.ppo2.ppo2 as ppo2
from baselines.ppo2.policies import MlpPolicy


engine_channel = EngineConfigurationChannel()
valid_design_channel = ValidateDesignSideChannel()


try:
    from mpi4py import MPI
except ImportError:
    MPI = None

def make_unity_env(design, rank = 0):
    """
    Create a wrapped, monitored Unity environment.
    """
    unity_env = UnityEnvironment(file_name="../env/DroneHullAssembly",
                                 side_channels=[engine_channel, valid_design_channel],
                                 no_graphics=True,
                                 base_port=5000 + rank,
                                 additional_args=["-e", "1,1"] + ["-d"] + design)

    return unity_env

# https://github.com/Unity-Technologies/ml-agents/blob/56e6d333a52863785e20c34d89faadf0a115d320/gym-unity/README.md
def make_env(design, num_env, visual, start_index=0):
    """
    Create a wrapped, monitored Unity environment.
    """
    def _env(rank, use_visual=True): # pylint: disable=C0111
        def _thunk():
            unity_env = make_unity_env(design, rank)
            env = UnityToGymWrapper(unity_env, uint8_visual=True, allow_multiple_obs=(num_env > 0))
            env = Monitor(env, logger.get_dir() and os.path.join(logger.get_dir(), str(rank)))
            return env
        return _thunk

    if visual:
        return SubprocVecEnv([_env(i + start_index) for i in range(num_env)])
    else:
        rank = MPI.COMM_WORLD.Get_rank() if MPI else 0
        return DummyVecEnv([_env(rank, use_visual=False)])

def search_algo(args):
    # Load/Generate design graph
    graphs = pydot.graph_from_dot_file("../data/graph/graph_23oct.dot")
    design_graph = graphs[0]
    dgraph = dg.generate_design(design_graph)

    design_pattern = dg.get_design_pattern(design_graph, dgraph)
    print("pattern design: ",' '.join(design_pattern))

    unity_env = make_unity_env(design_pattern)

    unity_env.reset()
    if not valid_design_channel.get_result_blocking():
        unity_env.close()
        return

    unity_env.close()

    env = make_env(design_pattern, 4, False)
    ppo2.learn(
        policy=MlpPolicy,
        env=env,
        total_timesteps=100000,
        lr=3e-3,
        ent_coef=0.01,
        nsteps=1000
    )

def main():
    search_algo([])

if __name__ == '__main__':
    main()