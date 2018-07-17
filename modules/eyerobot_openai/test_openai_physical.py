import gym
import eyerobot_gym

env = gym.make('eyerobot-gym-v0')

while True:
    str_action = raw_input("Enter action to take (W/A/S/D|1/2/3/4): ")
    action = 0
    if str_action.upper() == "W":
        action = 0
    elif str_action.upper() == "A":
        action = 1
    elif str_action.upper() == "S":
        action = 2
    elif str_action.upper() == "D":
        action = 3
    else:
        try:
            action = int(str_action)
        except:
            print("Invalid action!")
            continue

    new_state, reward, done, data = env.step(action)
    print("State:")
    print("\tState: " + str(new_state))
    print("\tReward: " + str(reward))
    print("\tDone?: " + str(done))
    print("\tMeta Data: " + str(data))
    print
    if done:
        env.reset()
