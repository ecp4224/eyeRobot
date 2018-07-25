import sys

import gym

from eyerobot_gym.config import OPEN_CL

if OPEN_CL:
    import plaidml.keras

    plaidml.keras.install_backend()

from eyerobot_gym.RobotDQN import RobotDQN
from eyerobot_gym.Graph import graph
import os

devnull = open(os.devnull, 'w')


# Disable
def blockPrint():
    global devnull
    sys.stdout = devnull


# Restore
def enablePrint():
    sys.stdout = sys.__stdout__


def action_to_letter(a):
    actions = ["W", "A", "S", "D"]
    return actions[a]


env = gym.make('eyerobot-gym-v0')
robot = RobotDQN(batch_size=40)

episodes = 1000
model_history = None
for i in range(episodes):

    stop = False
    while True:
        r = raw_input("Pick an action:\n\tType r to play the next episode\n\tType g to graph the current model "
                      "accuraccy\n\tType q to safely quit\nAction: ")
        if r == "q":
            stop = True
            break
        elif r == "g" and model_history is not None:
            graph(model_history)
        else:
            stop = False
            break

    if stop:
        break

    print("Starting episode " + str(i))

    state = env.reset()
    done = False
    won = False

    # while game still in progress
    while not done:
        # blockPrint()

        print("What to do..")
        action = robot.step(state)

        print("Pressing " + action_to_letter(action))
        new_state, reward, done, data = env.step(action)

        print("Remembering..")
        # Robot automatically remembers the action it just chose
        robot.save(reward, new_state, done)

        state = new_state

        # For printing
        won = reward == 1

    print("Episode completed!")

    if won:
        print("I won!")
    else:
        print("I lost :(")

    print("Training..")

    # Only ever train the robot after an entire episode
    model_history = robot.train()

print("Exiting..")
# save model to file
robot.save_model('hallway3.ai')
env.close()
print("Goodbye")
