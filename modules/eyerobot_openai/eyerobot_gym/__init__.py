from gym.envs.registration import register

register(
    id='eyerobot-gym-v0',
    entry_point='eyerobot_gym.envs:EyeRobotEnv',
)