import sys
import gym

from eyerobot_gym.config import OPEN_CL, DEBUG, BATCH_SIZE, MEMORY_SIZE, EPOCH

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


if __name__ == "__main__":
    # Create model object which represents the AI
    robot = RobotDQN(load_from="s.ai",
                     batch_size=BATCH_SIZE,
                     memory_size=MEMORY_SIZE,
                     epochs=EPOCH)
    
    # Create the environment that the robot will run in
    env = gym.make('eyerobot-gym-v0')
    
    episodes = 1000
    model_history = None
    counter = 0
    for i in range(episodes):
        
        state = env.reset()
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
                    counter = 1000
                    break
                else:
                    stop = False
                    break
        else:
            counter -= 1
            
        if stop:
            break
        
        print("Starting episode " + str(i))
        
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
        print("Score: " + str(reward))
        
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
    env.close()
    print("Goodbye")