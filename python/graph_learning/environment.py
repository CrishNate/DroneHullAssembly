import gym
from gym_unity.envs import UnityToGymWrapper


class DroneEnvironment(gym.Env):
    def __init__(self, unity_env):
        super(DroneEnvironment, self).__init__()

        env = UnityToGymWrapper(unity_env, allow_multiple_obs=False, uint8_visual=True)

        self.env = env
        self.action_space = self.env.action_space
        self.action_size = self.env.action_size
        self.observation_space = self.env.action_space

    @staticmethod
    def tuple_to_dict(s):
        obs = {
            0: s[0],
            1: s[1],
            2: s[2]
        }
        return obs

    def reset(self):
        #         print("LOG: returning reset" + self.tuple_to_dict(self.env.reset()))
        #         print("LOG: returning reset" + (self.env.reset()))
        #          np.array(self._observation)
        return self.tuple_to_dict(self.env.reset())

    def step(self, action):
        s, r, d, info = self.env.step(action)
        return self.tuple_to_dict(s), float(r), d, info

    def close(self):
        self.env.close()
        global rank
        rank -= 1

    def render(self, mode="human"):
        self.env.render()
