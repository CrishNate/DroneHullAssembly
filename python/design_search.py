# Math & etc.
import argparse
import random
import os
import time
import csv

import path_vars

# Grammar
import pydot
import graph_learning.drone_graph as dg
from DesignEvalCallback import DesignEvalCallback
from graph_learning.drone_graph import DNode

# Unity Channels
from side_channels.valid_design_side_channel import ValidateDesignSideChannel
from mlagents_envs.side_channel.engine_configuration_channel import EngineConfigurationChannel
from mlagents_envs.side_channel.engine_configuration_channel import EngineConfig

# Unity envs
from mlagents_envs.environment import UnityEnvironment
from gym_unity.envs import UnityToGymWrapper
from gym import logger
from mlagents_envs import logging_util

# ML
from stable_baselines3.common.vec_env.subproc_vec_env import SubprocVecEnv
from stable_baselines3.common.vec_env.dummy_vec_env import DummyVecEnv
from stable_baselines3.common.monitor import Monitor
from stable_baselines3.common.evaluation import evaluate_policy
from stable_baselines3.common.logger import configure
from stable_baselines3.common.callbacks import EvalCallback, StopTrainingOnRewardThreshold, StopTrainingOnNoModelImprovement
from stable_baselines3 import PPO

# MCTS
from mcts import MCTS

valid_design_channel = ValidateDesignSideChannel()

try:
    from mpi4py import MPI
except ImportError:
    MPI = None

logger.set_level(40)
logging_util.set_log_level(logging_util.ERROR)

# assigning empty dirs
if not os.path.exists(path_vars.DESIGNS_DIR):
    os.mkdir(path_vars.DESIGNS_DIR)


def make_unity_env(pattern, rank = 0):
    """
    Create a wrapped, monitored Unity environment.
    """
    engine_channel = EngineConfigurationChannel()
    engine_channel.set_configuration(EngineConfig(600, 600, 1, 5.0, -1, 30))
    unity_env = UnityEnvironment(file_name="../env/DroneHullAssembly",
                                 side_channels=[engine_channel, valid_design_channel],
                                 no_graphics=True,
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
            env = UnityToGymWrapper(unity_env, allow_multiple_obs=False)
            env = Monitor(env, path_vars.LOG_DIR)
            return env
        return _thunk

    if visual:
        return SubprocVecEnv([_env(i + start_index) for i in range(num_env)])
    else:
        rank = MPI.COMM_WORLD.Get_rank() if MPI else 0
        return DummyVecEnv([_env(rank + i) for i in range(num_env)])

def simulate(args, csv_designs_writer, dnode):
    t_start = time.time()

    # Load/Generate design graph
    design_pattern = dnode.get_pattern()

    # Validating design
    valid_design_channel.reset()
    unity_env = make_unity_env(design_pattern)
    unity_env.reset()

    if not valid_design_channel.get_result_blocking():
        print("Invalid design: ", ' '.join(design_pattern))
        unity_env.close()
        return 0 # possible error with search

    unity_env.close()

    # Training design
    print("Begin eval pattern design: ", ' '.join(design_pattern))

    env = make_env(design_pattern, args.num_envs, True)

    model = PPO("MlpPolicy", env,
                tensorboard_log=path_vars.LOG_DIR if args.log else None,
                n_steps=100,
                batch_size=200,
                policy_kwargs={'net_arch': [dict(pi=[256, 256], vf=[256, 256])]},
                learning_rate=1e-3,
                device="auto")

    callback_on_best = StopTrainingOnRewardThreshold(reward_threshold=800, verbose=0)
    stop_train_callback = StopTrainingOnNoModelImprovement(max_no_improvement_evals=20, min_evals=20, verbose=0)
    eval_callback = DesignEvalCallback(env,
                                 callback_on_new_best=callback_on_best,
                                 callback_after_eval=stop_train_callback,
                                 eval_freq=model.n_steps,
                                 verbose=1)

    model.learn(total_timesteps=args.num_steps,
                tb_log_name="flying_base_with_wind/base/",
                callback=eval_callback)

    pattern_code = "".join(design_pattern)
    model.save(os.path.join(path_vars.MODELS_DIR, pattern_code))

    print("=======[Pattern evaluated]=======")
    print("Eval time: ", time.time() - t_start)
    print("Eval pattern: ", ' '.join(design_pattern))

    mean_reward, std_reward = evaluate_policy(model, env, n_eval_episodes=10, deterministic=True)
    print(f"Mean Reward={mean_reward:.2f} +/- {std_reward}")
    print("=================================")

    model.env.close()
    del model

    data = [' '.join(design_pattern), mean_reward]
    csv_designs_writer.writerow(data)

    return mean_reward


def search_algo(args, design_graph):
    if args.seed is not None:
        random.seed(args.seed)

    sockets_dict = dg.get_sockets_dict(design_graph)

    mcts = MCTS(exploration_weight=args.exploration_weight)

    root_node = next(x for x in design_graph.get_nodes() if x.get_name() == "root")

    # managing csv for design logs
    csv_designs_file = open(os.path.join(path_vars.DESIGNS_DIR, "drone_stay_middle.csv"), 'w', newline='')
    csv_designs_writer = csv.writer(csv_designs_file)

    root_dnode = DNode(sockets_dict, root_node, None, None, None)
    root_dnode.design_graph = design_graph
    root_dnode.simulation = lambda x: simulate(args, csv_designs_writer, x)

    for _ in range(args.num_eval):
        # we run MCTS simulation for many times
        for _ in range(args.num_iterations):
            mcts.run(root_dnode, num_rollout=args.num_rollout)

        # we choose the best greedy action based on simulation results
        root_dnode = mcts.choose(root_dnode)

    csv_designs_file.close()


def main(args):
    graphs = pydot.graph_from_dot_file(args.grammar_file)
    design_graph = graphs[0]

    search_algo(args, design_graph)


if __name__ == '__main__':
    # parser = argparse.ArgumentParser(description='Design search.')
    #
    # parser.add_argument("grammar_file", type = str, default = "../data/designs/graph_23oct.dot", help="Grammar file (.dot)")
    # parser.add_argument('--task', type = str, default = 'StayMiddle', help = 'Task (Python class name')
    # parser.add_argument('--num-steps', type = int, default=1_000_000, help='Number of steps for each design')
    # parser.add_argument('-i', '--num-iterations', type = int, default=2000, help='Number of iterations')
    # parser.add_argument('-j', '--num-envs', type = int, default=8, help='Number of environments')
    # parser.add_argument('-s', '--seed', type = int, default=None, help='seed')
    # parser.add_argument('-l', '--log', type = bool, default=False, help='enable logging')
    #
    # args = parser.parse_args()

    args = argparse.Namespace()
    setattr(args, "grammar_file", "../data/designs/graph_23oct.dot")
    setattr(args, "num_envs", 20)
    setattr(args, "num_eval", 30)
    setattr(args, "num_iterations", 10)
    setattr(args, "num_rollout", 1)
    setattr(args, "exploration_weight", 51)
    setattr(args, "num_steps", 500_000)
    setattr(args, "seed", 1)
    setattr(args, "log", False)

    main(args)