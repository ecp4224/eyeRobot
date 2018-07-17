import gym
import eyerobot_gym

import h5py  # needed to save/load model
from keras.models import Sequential, load_model
from keras.layers.core import Dense, Activation
from keras.optimizers import Nadam
import random
import numpy as np
import sys
import os

devnull = open(os.devnull, 'w')

# Disable
def blockPrint():
    global devnull
    sys.stdout = devnull


# Restore
def enablePrint():
    sys.stdout = sys.__stdout__


env = gym.make('eyerobot-gym-v0')
#blockPrint()

######################################
# following methods train network    #
# to navigate the grid               #
######################################

# run the commented section to create the training model
# then comment out the line below for loading the model
# just run the load model line if you already have a trained model saved


model = Sequential()
model.add(Dense(164, init='lecun_uniform', input_shape=(1,)))  # input layer 64 units (4x4x4 numpy array), hidden
# layer 164 units
model.add(Activation('tanh'))

model.add(Dense(200, init='lecun_uniform'))  # hidden layer 150 units
model.add(Activation('relu'))

model.add(Dense(150, init='lecun_uniform'))  # hidden layer 150 units
model.add(Activation('relu'))

model.add(Dense(100, init='lecun_uniform'))  # hidden layer 150 units
model.add(Activation('relu'))

model.add(Dense(4, init='lecun_uniform'))  # output layer 4 units (up, down, left, right)
model.add(Activation('softmax'))  # linear output so we can have range of real-valued outputs

rms = Nadam(lr=0.002, beta_1=0.9, beta_2=0.999, epsilon=None, schedule_decay=0.004)
model.compile(loss='mse', optimizer=rms)

epochs = 3000
gamma = 0.975
epsilon = 0.3
batchSize = 50
buffer = 100
replay = []
# stores tuples of S, A, R, S'
h = 0
for i in range(epochs):

    state = env.reset()
    done = False
    # while game still in progress
    while not done:
        blockPrint()
        state = np.array(state).reshape((1, 1))
        # run q on state s to get all values for each action
        qval = model.predict(state, batch_size=1)
        if random.random() < epsilon:  # choose random action
            action = np.random.randint(0, 4)
        else:  # choose best action from Q(s,a) values
            action = (np.argmax(qval))
        # take action, observe new state S'

        new_state, reward, done, data = env.step(action)

        enablePrint()
        print("Got reward " + str(reward))

        new_state = np.array(new_state).reshape((1, 1))

        # new_state = makeMove(state, action)
        # observe reward
        # reward = getReward(new_state)

        # Experience replay storage; deal with catastrophic forgetting
        if (len(replay) < buffer):  # if buffer not filled, add to it
            replay.append((state, action, reward, new_state))
        else:  # or overwrite old values
            if (h < (buffer - 1)):
                h += 1
            else:
                h = 0
            replay[h] = (state, action, reward, new_state)
            # randomly sample our experience replay memory
            minibatch = random.sample(replay, batchSize)
            X_train = []  # holds each state s
            y_train = []  # value updates
            for memory in minibatch:
                # Get max_Q(S',a)
                old_state, action, reward, new_state = memory
                old_qval = model.predict(old_state, batch_size=1)
                newQ = model.predict(new_state, batch_size=1)
                maxQ = np.max(newQ)
                y = np.zeros((1, 4))
                y[:] = old_qval[:]
                if reward > 1:  # non-terminal state
                    update = (reward + (gamma * maxQ))
                else:  # terminal state
                    update = reward
                y[0][action] = update
                X_train.append(old_state.reshape((1,)))
                y_train.append(y.reshape(4, ))

            X_train = np.array(X_train)
            y_train = np.array(y_train)
            print("Game #: %s" % (i,))
            model.fit(X_train, y_train, batch_size=batchSize, nb_epoch=1, verbose=1)
            state = new_state
    #if epsilon > 0.1:  # decrement epsilon over time
    #    epsilon -= (1.0 / epochs)
    #    print("Epsilon: " + str(epsilon))

# save model to file
model.save('test_grid.h5')

'''
# load model
model = load_model('test_grid.h5')


def testGrid():
    i = 0
    state = initGridPlayer()

    print("Initial State:")
    print(dispGrid(state))
    status = 1
    # while in progress
    while (status == 1):
        qval = model.predict(state.reshape(1, 64), batch_size=1)
        action = (np.argmax(qval))  # take action with highest Q val
        print('Move #: %s; Taking action: %s' % (i, action))
        state = makeMove(state, action)
        print(dispGrid(state))
        reward = getReward(state)
        if reward != -1:
            status = 0
            print("Reward: %s" % (reward,))
        i += 1  # end game if more than 10 actions taken
        if (i > 10):
            print("Game lost; too many moves.")
            break


print("testing")
testGrid()
testGrid()
testGrid()
testGrid()
testGrid()
testGrid()
'''
