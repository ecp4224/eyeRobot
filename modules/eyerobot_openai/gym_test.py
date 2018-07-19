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

#https://stats.stackexchange.com/questions/181/how-to-choose-the-number-of-hidden-layers-and-nodes-in-a-feedforward-neural-netw
#we should probably read through this to get a better idea of how many layers to use

#most models start off with creating a Sequential()
#conv1 = Sequential()
conv1 = Convolution1D(120, 8, activation='relu')(input1) #i know we had issues with 2d and the shape, but i dont think a conv 1d will work the way we want. we can try it but i think we might need to find another way around the problem
conv1 = Convolution1D(80, 4, activation='relu')(conv1) #why are we putting (conv1) after everything. ive seen it in some tutorials and not others and also i think im tired
conv1 = Convolution1D(50, 2, activation='relu')(conv1)
conv1 = MaxPool1D(pool_size=(2))(conv1) #same with maxpooling, might want 2d if possible
conv1 = Dropout(0.25)(conv1) #sets a random set of activations to 0. as explained below, relu activation can lead to dead neurons if too many are set to 0. using dropout with rely might be a bad idea. however, this can help with overfitting. but overfitting is a problem we may or may not run into (im leaning on the side of 'not'. idk how overfitting works with reinforcement learning, since i cant visualize a graph)
conv1 = Flatten()(conv1)

#if we want LSTM, we would need to define it after conv1, example:
#lstm = Sequential()
#lstm.add(TimeDistributed(conv1, input_shape=()))
#lstm.add(LSTM(activation='relu', return_sequences=False/True, return_state=False/True, etc)) #theres a lot of arguments for the lstm layer, can look at website for more info
#lstm.add(Dense())
#then merge lstm and model2 below

#model2 = Sequential()
model2 = Dense(164, init='lecun_uniform')(input2)
model2 = Activation('tanh')(model2) #pretty much just a scaled sigmoid. would guess its used for probability type things as well?
model2 = Dense(50, init='lecun_uniform')(model2) #why is (model2) after everything. maybe im tired
model2 = Activation('relu')(model2) #linearity, simple, does not saturate, converge quickly; be careful with tuning learning rate, as relu neurons that are 0 , gradients might clip to 0 in backpropagation. so we should monitor the learning rate at first
#can try leaky relu if we have problems with the learning rate and 'dead' neurons. leaky relu tries to fix dead neuron problem, but can be more inconsistent with results
model2 = Dense(4, init='lecun_uniform')(model2)

merged = Concatenate()([conv1,model2])
output = Dense(20)(merged)
output = Dense(10)(output)
output = Dense(4, activation='softmax')(output) #softmax usually used in rnn, for probablistic functions
#softmax probably might still work in this case. it gives values between 0 and 1, but divides each output so that all the class's probabilities add up to 1. so its the probability that any class is likely to be the case. this could be good in figuring out which direction to go

#model = Sequential()
model = Model(input=[input1,input2],output=output)

model.compile(loss='mse', optimizer='adam', metrics=['mae'])

epochs = 3000
gamma = 0.975
epsilon = 0.3
batchSize = 50
buffer = 100
replay = []

env = gym.make('eyerobot-gym-v0')
#stores tuple state, action, reward, new state
h = 0
for i in range(epochs):
    print(env.reset())
    temp = env.reset()
    state = temp[0]
    depth = temp[1]
    done = False
    # while game still in progress
    while not done:
        blockPrint()
        state = np.array(state).reshape(1,4)
        depth = np.reshape(depth, (1, 480, 640))
        #run q on s to get values for action
        qval = model.predict([depth, state], batch_size=1)

        print("Action: " + str(qval))

        if random.random() < epsilon:  #choose random action
            action = np.random.randint(0, 4)
        else:  #choose best action
            action = (np.argmax(qval))
        
        #take action, take new state
        temp, reward, done, data = env.step(action)
        new_state = temp[0]
        new_depth = temp[1]

        new_depth = np.reshape(new_depth, (1, 480, 640))

        enablePrint()
        print("Got reward " + str(reward))

        new_state = np.array(new_state).reshape(1,4)

        #observe reward
        #reward = getReward(new_state)

        #experience replay
        if (len(replay) < buffer):  #if buffer not filled
            replay.append((state, depth, action, reward, new_state, new_depth))
        else:  #overwrite old vals
            if (h < (buffer - 1)):
                h += 1
            else:
                h = 0
            replay[h] = (state, depth, action, reward, new_state, new_depth)
            
            #sample experience replay
            minibatch = random.sample(replay, batchSize)
            X_train = []  #holds each state s
            X2_train = []
            y_train = []  #class updates
            for memory in minibatch: #get max of Q(new_state, a)
                old_state, old_depth, action, reward, new_state, new_depth = memory
                old_qval = model.predict([old_depth, old_state], batch_size=1)
                newQ = model.predict([new_depth, new_state], batch_size=1)
                maxQ = np.max(newQ)
                y = np.zeros((1, 4))
                y[:] = old_qval[:]
                if reward > 1:  #nonterminal state
                    update = (reward + (gamma * maxQ))
                else:  #terminal state
                    update = reward
                y[0][action] = update
                X_train.append(old_state)
                X2_train.append(old_depth.reshape(480, 640))
                y_train.append(y.reshape(4,))

            X_train = np.array(X_train).reshape(batchSize,4)
            X2_train = np.array(X2_train)
            y_train = np.array(y_train)
            print("Test #: %s" % (i,))
            model.fit([X2_train, X_train], y_train, batch_size=batchSize, nb_epoch=1, verbose=1)
            state = new_state
    
    #this was commented out, but dont we need it?
    if epsilon > 0.1:  #slowly decrease epsilon
        epsilon -= (1.0 / epochs)
        print("Epsilon: " + str(epsilon))

#save model to file
model.save('room1.h5')