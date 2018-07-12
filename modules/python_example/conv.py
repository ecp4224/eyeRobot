import numpy as np
from keras.models import Sequential
from keras.layers import Conv2D, Dense, Flatten, MaxPooling, Dropout
from keras.optimizers import RMSprop
import skimage
import cv2
import random
from IPython.display import clear_output

#rescale to 160x120
#doing 80x80 for now

proc_img = cv2.VideoCapture(0) #change this to the depth camera later
proc_img = skimage.transform.resize(proc_img,(80,80))
proc_img = skimage.exposure.rescale_intensity(proc_img, out_range=(0,255))
proc_img = proc_img.reshape(1,1,proc_img.shape[0], proc_img.shape[1])
s_img = np.append(proc_img, s_t[:,:3,:,:], axis=1) #stacked image, 4 frames together, not sure if we need this?

batch_size = 40
epochs = 30 #should be about 3000, but keeping a low number for now to test

#need array of depth data from kinect, as well as "labels" (maybe?) of what the colors mean in terms of depth
#or let it figure it out for itself idk
#this tutorial isnt talkin about using data that isnt organized into labels
#theyre using the mnist dataset of images

model = Sequential()
#output size 164, moving window size of 5x5, input shape is size of kinect window

input_shape = (80,80,4) #640 x 480 X 1
'''
model.add(Conv2D(164, kernel_size=(5,5), strides=(1,1), activation='relu', input_shape=input_shape))
model.add(MaxPooling2D(pool_size=(2,2), strides=(2,2))) #pooling is the size in x,y directions
model.add(Conv2D(150, (5,5), activation='relu'))
model.add(MaxPooling2D(pool_size(2,2)))

model.add(Flatten())
model.add(Dense(1000, activation='relu'))
model.add(Dense(10, activation='softmax'))

#so here's the network, but i can't do model.fit (the training) without some kind of dataset
'''

#build model
#using 164, 150, and 150 units, but we need to play around with these numbers
model.add(Conv2D(164, 8, 8, subsample=(4,4), activation='relu', input_shape=input_shape))
model.add(Conv2D(150, 4, 4, subsample=(2,2), activation='relu'))
model.add(Conv2D(150, 3, 3, subsample=(1,1), activation='relu'))
model.add(MaxPooling(pool_size=(2,2))) #dont know what this does
model.add(Dropout(0.25)) #dont know what this does

model.add(Flatten())
model.add(LSTM(20, return_sequences=True)) #what does this do!!!! 
model.add(Dense(512, activation='relu'))
model.add(Dense(4, activation='relu')) #4 is the output
rms = RMSprop() #related to learning rate
model.compile(loss='mse', optimizer=rms)

#train network
epochs = 30 #should be 3000, but start with 30 to make sure no errors occur and all inputs are given correctly before running this bitch for 3 hours
gamma = 0.975
epsilon = 1
batchSize = 40
buffer = 80
replay = []

h = 0
for i in range(epochs):
    
    ret, state = proc_img.read() #this is the camera input, but need to add acceleration, rotational, location, etc data to the state as well if needed
    status = 1
    #while game still in progress
    while(status == 1):
        #run q on state s to get all values for each action
        qval = model.predict(state.reshape(1,64), batch_size=1)
        if (random.random() < epsilon): #choose random action
            action = np.random.randint(0,4)
        else: #choose best action from Q(s,a) values
            action = (np.argmax(qval))
        #take action, observe new state S'
        new_state = '''this has to be done by the gym, the gym takes an action and gives the new_state''' #makeMove(state, action) 
        #observe reward
        reward = '''this also has to be done by the gym, based on the new state''' #getReward(new_state)
        
        #Experience replay storage; deal with catastrophic forgetting
        if (len(replay) < buffer): #if buffer not filled, add to it
            replay.append((state, action, reward, new_state))
        else: #or overwrite old values
            if (h < (buffer-1)):
                h += 1
            else:
                h = 0
            replay[h] = (state, action, reward, new_state)
            #randomly sample our experience replay memory
            minibatch = random.sample(replay, batchSize)
            X_train = [] #holds each state s
            y_train = [] #value updates
            for memory in minibatch:
                #Get max_Q(S',a)
                old_state, action, reward, new_state = memory
                old_qval = model.predict(old_state.reshape(1,64), batch_size=1)
                newQ = model.predict(new_state.reshape(1,64), batch_size=1)
                maxQ = np.max(newQ)
                y = np.zeros((1,4))
                y[:] = old_qval[:]
                if reward == -1: #non-terminal state
                    update = (reward + (gamma * maxQ))
                else: #terminal state
                    update = reward
                y[0][action] = update
                X_train.append(old_state.reshape(64,))
                y_train.append(y.reshape(4,))
            
            X_train = np.array(X_train)
            y_train = np.array(y_train)
            print("Game #: %s" % (i,))
            model.fit(X_train, y_train, batch_size=batchSize, nb_epoch=1, verbose=1)
            state = new_state
        if reward != -1: #if reached terminal state, update game status
            status = 0
        clear_output(wait=True)
    wait = input("Starting new epoch. Place the robot somewhere in the room and press enter")
    if epsilon > 0.1: #decrement epsilon over time
        epsilon -= (1/epochs)
proc_img.release()