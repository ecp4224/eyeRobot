import random
from collections import deque

from keras.layers import *
from keras.models import *
from keras.optimizers import Adam
import math


# Represents the robot's AI
class RobotDQN():
    gamma = 0.95
    epsilon = 1.0
    emin = 0.01
    edecay = 0.995
    learning_rate = 0.001
    action_space = 4  # W A S D, there are four buttons
    current_action = None
    current_state = None
    all_loss = []

    def __init__(self, batch_size=100, memory_size=2000, epochs=1, load_from=""):
        self.experiences = deque(maxlen=memory_size)
        self.batch_size = batch_size
        self.epochs = epochs

        self.__build_model__(load_from)

    def __build_model__(self, load_from=""):

        if load_from == "":
            input1 = Input(shape=(640, 480, 1))

            # https://stats.stackexchange.com/questions/181/how-to-choose-the-number-of-hidden-layers-and-nodes-in-a-feedforward-neural-netw
            # we should probably read through this to get a better idea of how many layers to use

            conv1 = Convolution2D(200, 8, activation='relu', input_shape=(1, 640, 480, 1), data_format="channels_last")(
                input1)
            conv1 = Convolution2D(80, 2, activation='relu')(conv1)
            conv1 = MaxPool2D(pool_size=(2))(conv1)  # same with maxpooling, might want 2d if possible
            conv1 = Flatten()(conv1)

            # hidden layer 1
            input2 = Input(shape=(4,))
            model2 = Dense(8, init='lecun_uniform')(input2)
            model2 = Activation('relu')(model2)

            merged = Concatenate()([conv1, model2])
            output = Dense(10)(merged)
            output = Dense(4, activation='linear')(output)  # softmax usually used in rnn, for probablistic functions
            # softmax probably might still work in this case. it gives values between 0 and 1, but divides each output so that
            # all the class's probabilities add up to 1. so its the probability that any class is likely to be the case. this
            # could be good in figuring out which direction to go

            # model = Sequential()
            self.model = Model(input=[input1, input2], output=output)

            self.model.compile(loss='mean_squared_logarithmic_error', optimizer=Adam(lr=0.001))
        else:
            self.epsilon = 0.2
            self.model = load_model(load_from)

    def save(self, reward, next_state, done):
        next_state = [
            np.array(next_state[1]).reshape(1, 640, 480, 1),
            np.array(next_state[0]).reshape(1, 4)
        ]

        self.experiences.append((self.current_state, self.current_action, reward, next_state, done))

    def step(self, state):
        state = [
            np.array(state[1]).reshape(1, 640, 480, 1),
            np.array(state[0]).reshape(1, 4)
        ]
        if np.random.random() <= self.epsilon:
            decided_action = random.randrange(self.action_space)
        else:
            actions = self.model.predict(state)

            # [0] because predict returns a 2d array (where the first dimnension is the sample)
            # We only have one sample whenever we step, so default to 0
            #
            # Use argmax because we want to pick the most likely action (or the action with the highest "best chance" score)
            decided_action = np.argmax(actions[0])

        # Remember these variables for the save() function
        self.current_action = decided_action
        self.current_state = state

        return decided_action

    def train(self):
        bsize = min(self.batch_size, len(self.experiences))

        history = None

        minibatch = random.sample(self.experiences, bsize)
        for (state, action, reward, next_state, done) in minibatch:

            # If the current world state being used to train was not a
            # completed state
            if not done:
                # Apply next state to make the AI remembers what to do next
                next_action = self.model.predict(next_state)[0]
                target = reward + self.gamma * np.amax(next_action)
            # Otherwise it was a completed state
            else:
                target = reward

            # Get current action
            target_state = self.model.predict(state)
            target_state[0][action] = target

            # Finally train model
            history = self.model.fit(state, target_state, epochs=self.epochs, verbose=1)

            self.all_loss.append(history.history['loss'][0])

        if self.epsilon > self.emin:
            self.epsilon *= self.edecay

        return self.all_loss

    def save_model(self, path):
        self.model.save(path)
