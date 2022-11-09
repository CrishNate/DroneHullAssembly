import argparse

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
                                 worker_id=50,
                                 additional_args=args + ["-d"] + pattern)

    env = UnityToGymWrapper(unity_env)
    obs = env.reset()

    # view model
    while model_file is None:
        action = env.action_space.sample()
        obs, rewards, dones, info = env.step(action)

        if dones:
            env.reset()


    # view trained model
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
    # parser.add_argument("model_file", type = str, default = "../results/models/.zip", help = 'Pretrained model to use')
    # parser.add_argument("rule_sequence", nargs="+", help="Drone pattern to use")
    #
    # args = parser.parse_args()

    args = argparse.Namespace()
    setattr(args, "grammar_file", "../data/designs/graph_23oct.dot")
    setattr(args, "model_file", "../results/models/023158288.zip")
    #setattr(args, "model_file", None)
    setattr(args, "rule_sequence", "0 2 3 1 5 8 2 8 8".split(" "))

    graphs = pydot.graph_from_dot_file(args.grammar_file)
    design_graph = graphs[0]

    pattern = [s.strip(" ") for s in args.rule_sequence]

    viewer(design_graph, args.model_file, pattern)

