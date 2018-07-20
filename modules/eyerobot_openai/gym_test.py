import random
import sys

import gym

from eyerobot_gym.config import OPEN_CL

if OPEN_CL:
    import plaidml.keras

    plaidml.keras.install_backend()

from keras.models import *
from keras.layers import *

devnull = open(os.devnull, 'w')


# Disable
def blockPrint():
    global devnull
    sys.stdout = devnull


# Restore
def enablePrint():
    sys.stdout = sys.__stdout__


# blockPrint()

######################################
# following methods train network    #
# to navigate the grid               #
######################################

# run the commented section to create the training model
# then comment out the line below for loading the model
# just run the load model line if you already have a trained model saved

input1 = Input(shape=(640, 480, 1))

# https://stats.stackexchange.com/questions/181/how-to-choose-the-number-of-hidden-layers-and-nodes-in-a-feedforward-neural-netw
# we should probably read through this to get a better idea of how many layers to use


conv1 = Convolution2D(200, 8, activation='relu', input_shape=(1, 640, 480, 1), data_format="channels_last")(input1)
conv1 = Convolution2D(80, 2, activation='relu')(conv1)
conv1 = MaxPool2D(pool_size=(2))(conv1)  # same with maxpooling, might want 2d if possible
conv1 = Flatten()(conv1)

# hidden layer 1
input2 = Input(shape=(4,))
model2 = Dense(8, init='lecun_uniform')(input2)
model2 = Activation('tanh')(model2)

merged = Concatenate()([conv1, model2])
output = Dense(10)(merged)
output = Dense(4, activation='softmax')(output)  # softmax usually used in rnn, for probablistic functions
# softmax probably might still work in this case. it gives values between 0 and 1, but divides each output so that
# all the class's probabilities add up to 1. so its the probability that any class is likely to be the case. this
# could be good in figuring out which direction to go

# model = Sequential()
model = Model(input=[input1, input2], output=output)

model.compile(loss='mse', optimizer='adam', metrics=['mae'])

epochs = 3000
gamma = 0.975
epsilon = 0.9
batchSize = 5
buffer = 10
replay = []

env = gym.make('eyerobot-gym-v0')
# stores tuple state, action, reward, new state
h = 0
for i in range(epochs):

    temp = env.reset()
    observation_input = temp[0]
    depth_input = temp[1]
    done = False

    r = raw_input("Press enter to start next episode (or type q to safely quit): ")
    if r == "q":
        break

    # while game still in progress
    while not done:
        blockPrint()
        i2 = np.array(observation_input).reshape(1, 4)
        i1 = np.reshape(depth_input, (1, 640, 480, 1))

        state = [i1, i2]  # The current state is the two inputs combined

        # run q on state s to get all values for each action
        qval = model.predict(state, batch_size=1)

        print("Action: " + str(qval))

        if random.random() < epsilon:  # choose random action
            action = np.random.randint(0, 4)
        else:  # choose best action
            action = (np.argmax(qval))

        # take action, take new state
        temp, reward, done, data = env.step(action)
        new_observation = temp[0]
        new_depth = temp[1]

        new_depth = np.reshape(new_depth, (1, 640, 480, 1))

        enablePrint()
        print("reward: " + str(reward))

        new_observation = np.array(new_observation).reshape(1, 4)

        new_state = [new_depth, new_observation]  # The new state is the two inputs combined

        # Get max_Q(S',a)
        newQ = model.predict(new_state, batch_size=1)
        maxQ = np.max(newQ)
        y = np.zeros((1, 4))
        y[:] = qval[:]

        if reward == -1:  # non-terminal state
            update = (reward + (gamma * maxQ))
        else:  # terminal state
            update = reward

        y[0][action] = update  # target output
        print("Game #: %s" % (i,))
        model.fit(state, y, batch_size=1, nb_epoch=1, verbose=1)

        depth_input = new_state[0]
        observation_input = new_state[1]

    # this was commented out, but dont we need it?
    if epsilon > 0.1:  # slowly decrease epsilon
        epsilon -= (1.0 / epochs)
        print("epsilon: " + str(epsilon))

print("Exiting..")
# save model to file
model.save('room1.h5')
env.close()
print("Goodbye")
