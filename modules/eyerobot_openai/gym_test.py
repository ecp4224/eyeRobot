import gym
import eyerobot_gym

env = gym.make('eyerobot-gym-v0')

while True:

    for i in range(150):
        env.step(1)

    env.reset()

