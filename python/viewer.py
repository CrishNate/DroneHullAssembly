import argparse
import sys
import pydot

from gym_unity.envs import UnityToGymWrapper
from mlagents_envs.environment import UnityEnvironment
from mlagents_envs.side_channel.engine_configuration_channel import EngineConfig
from mlagents_envs.side_channel.engine_configuration_channel import EngineConfigurationChannel
from stable_baselines3 import PPO


def viewer(design_graph, model_file, pattern):
    args = [
        "-e", "1,1",
        "-v"
    ]

    engine_channel = EngineConfigurationChannel()
    engine_channel.set_configuration(EngineConfig(600, 600, 1, 1.0, 60, 60))
    unity_env = UnityEnvironment(file_name="../env/DroneHullAssembly",
                                 side_channels=[engine_channel],
                                 additional_args=args + ["-d"] + pattern)

    env = UnityToGymWrapper(unity_env)
    obs = env.reset()

    model = PPO.load(model_file)

    while True:
        action, _states = model.predict(obs)
        obs, rewards, dones, info = env.step(action)

        if dones:
            env.reset()


if __name__ == '__main__':
    # parser = argparse.ArgumentParser(description='Drone design viewer.')
    #
    # parser.add_argument('--task', type = str, default = 'FlatTerrainTask', help = 'Task (Python class name')
    # parser.add_argument("grammar_file", type = str, default = "../data/designs/graph_23oct.dot", help="Grammar file (.dot)")
    # parser.add_argument("model_file", type = str, default = "../results/models/results.zip", help = 'Pretrained model to use')
    # parser.add_argument("rule_sequence", nargs="+", help="Drone pattern to use")
    #
    # args = parser.parse_args()

    args = argparse.Namespace()
    setattr(args, "grammar_file", "../data/designs/graph_23oct.dot")
    setattr(args, "model_file", "../results/models/90258183189011812372335241745130611071778.zip")
    setattr(args, "rule_sequence", "9 0 2 5 8 1 8 3 1 8 9 0 1 18 1 2 37 2 3 35 2 4 17 4 5 13 0 6 11 0 7 17 7 8".split(" "))

    graphs = pydot.graph_from_dot_file(args.grammar_file)
    design_graph = graphs[0]

    pattern = [s.strip(" ") for s in args.rule_sequence]

    viewer(design_graph, args.model_file, pattern)

