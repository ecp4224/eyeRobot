import random
import sys

import gym

from eyerobot_gym.RobotDQN import RobotDQN
from eyerobot_gym.config import OPEN_CL

if OPEN_CL:
    import plaidml.keras

    plaidml.keras.install_backend()

from keras.models import *
from keras.layers import *
from keras.optimizers import Adam
from keras.callbacks import TensorBoard

devnull = open(os.devnull, 'w')


# Disable
def blockPrint():
    global devnull
    sys.stdout = devnull


# Restore
def enablePrint():
    sys.stdout = sys.__stdout__


env = gym.make('eyerobot-gym-v0')
robot = RobotDQN()

episodes = 1000
for i in range(episodes):

    state = env.reset()
    done = False

    r = raw_input("Press enter to start next episode (or type q to safely quit): ")
    if r == "q":
        break

    # while game still in progress
    while not done:
        # blockPrint()

        action = robot.step(state)

        new_state, reward, done, data = env.step(action)

        # Robot automatically remembers the action it just chose
        robot.save(reward, new_state, done)

        state = new_state

    # Only ever train the robot after an entire episode
    robot.train()

print("Exiting..")
# save model to file
robot.save_model('room1.h5')
env.close()
print("Goodbye")
