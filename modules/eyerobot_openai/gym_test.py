import sys

import gym

from eyerobot_gym.config import OPEN_CL, DEBUG

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
counter = 0
for i in range(episodes):

    stop = False
    if counter == 0:
        while True:
            r = raw_input("Pick an action:\n\tType r to play the next episode\n\tType g to graph the current model "
                          "accuraccy\n\tType s to autoplay 100 episodes\n\tType q to safely quit\nAction: ")
            if r == "q":
                stop = True
                break
            elif r == "g" and model_history is not None:
                graph(model_history)
            elif r == "s":
                counter = 100
                break
            else:
                stop = False
                break
    else:
        counter -= 1

    if stop:
        break

    print("Starting episode " + str(i))

    state = env.reset()
    done = False
    won = False

    if not DEBUG:
        blockPrint()

    # while game still in progress
    while not done:

        print("What to do..")
        action = robot.step(state)

        print("Pressing " + action_to_letter(action))
        new_state, reward, done, data = env.step(action)

        print("Remembering..")
        # Robot automatically remembers the action it just chose
        robot.save(reward, new_state, done)

        state = new_state

        # For printing
        won = reward == 100

    if not DEBUG:
        enablePrint()

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

name = raw_input("Enter name for AI: ")

robot.save_model(name + ".ai")
senv.close()
print("Goodbye")
