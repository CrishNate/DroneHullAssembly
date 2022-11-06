from stable_baselines3.a2c import MlpPolicy

# Custom MLP policy of three layers of size 128 each
class CustomPolicy(MlpPolicy):
    def __init__(self, *args, **kwargs):
        super(CustomPolicy, self).__init__(*args, **kwargs,
                                           net_arch=[dict(pi=[2048, 1024, 512, 256, 128],
                                                          vf=[2048, 1024, 512, 256, 128])])