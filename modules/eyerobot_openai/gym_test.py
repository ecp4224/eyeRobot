import gym
import eyerobot_gym

import h5py  # needed to save/load model

from keras.models import *
from keras.layers import *
from keras.optimizers import RMSprop, Nadam
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


#blockPrint()

######################################
# following methods train network    #
# to navigate the grid               #
######################################

# run the commented section to create the training model
# then comment out the line below for loading the model
# just run the load model line if you already have a trained model saved

input1 = Input(shape=(480,640))
input2 = Input(shape=(4,))

conv1 = Convolution1D(120, 8, activation='relu')(input1)
conv1 = Convolution1D(80, 4, activation='relu')(conv1)
conv1 = Convolution1D(50, 2, activation='relu')(conv1)
conv1 = MaxPool1D(pool_size=(2))(conv1)
conv1 = Dropout(0.25)(conv1)
conv1 = Flatten()(conv1)

model2 = Dense(164, init='lecun_uniform')(input2)
model2 = Activation('tanh')(model2)
model2 = Dense(50, init='lecun_uniform')(model2)
model2 = Activation('relu')(model2)
model2 = Dense(4, init='lecun_uniform')(model2)

merged = Concatenate()([conv1,model2])
output = Dense(20)(merged)
output = Dense(10)(output)
output = Dense(4, activation='softmax')(output)

model = Model(input=[input1,input2],output=output)

model.compile(loss='mse', optimizer='adam', metrics=['mae'])

epochs = 3000
gamma = 0.975
epsilon = 0.3
batchSize = 50
buffer = 100
replay = []

env = gym.make('eyerobot-gym-v0')

# stores tuples of S, A, R, S'
h = 0
for i in range(epochs):

    temp = env.reset()
    state = temp[0]
    depth = temp[1]
    done = False
    # while game still in progress
    while not done:
        blockPrint()
        state = np.array(state).reshape(1,4)
        depth = np.reshape(depth, (1, 480, 640))
        # run q on state s to get all values for each action
        qval = model.predict([depth, state], batch_size=1)

        print("Action: " + str(qval))

        if random.random() < epsilon:  # choose random action
            action = np.random.randint(0, 4)
        else:  # choose best action from Q(s,a) values
            action = (np.argmax(qval))
        # take action, observe new state S'

        temp, reward, done, data = env.step(action)
        new_state = temp[0]
        new_depth = temp[1]

        new_depth = np.reshape(new_depth, (1, 480, 640))

        enablePrint()
        print("Got reward " + str(reward))

        new_state = np.array(new_state).reshape(1,4)

        # new_state = makeMove(state, action)
        # observe reward
        # reward = getReward(new_state)

        # Experience replay storage; deal with catastrophic forgetting
        if (len(replay) < buffer):  # if buffer not filled, add to it
            replay.append((state, depth, action, reward, new_state, new_depth))
        else:  # or overwrite old values
            if (h < (buffer - 1)):
                h += 1
            else:
                h = 0
            replay[h] = (state, depth, action, reward, new_state, new_depth)
            # randomly sample our experience replay memory
            minibatch = random.sample(replay, batchSize)
            X_train = []  # holds each state s
            X2_train = []  # holds each state s
            y_train = []  # value updates
            for memory in minibatch:
                # Get max_Q(S',a)
                old_state, old_depth, action, reward, new_state, new_depth = memory
                old_qval = model.predict([old_depth, old_state], batch_size=1)
                newQ = model.predict([new_depth, new_state], batch_size=1)
                maxQ = np.max(newQ)
                y = np.zeros((1, 4))
                y[:] = old_qval[:]
                if reward > 1:  # non-terminal state
                    update = (reward + (gamma * maxQ))
                else:  # terminal state
                    update = reward
                y[0][action] = update
                X_train.append(old_state)
                X2_train.append(old_depth.reshape(480, 640))
                y_train.append(y.reshape(4,))

            X_train = np.array(X_train).reshape(batchSize,4)
            X2_train = np.array(X2_train)
            y_train = np.array(y_train)
            print("Game #: %s" % (i,))
            model.fit([X2_train, X_train], y_train, batch_size=batchSize, nb_epoch=1, verbose=1)
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
