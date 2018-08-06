import random
from collections import deque

from keras.layers import *
from keras.models import *
from keras.optimizers import Adam
import logging
from config import ENABLE_KINECT, BATCH_SIZE, PRIORITY_EXPERIENCE_REPLAY
from RobotMemory import PriorityExperience
from Policy import *

class RobotDQN:
    """ Represents the robot's AI """
    
    gamma = 0.78
    epsilon = 0.8 if not ENABLE_KINECT else 1
    emin = 0.01
    edecay = 0.995
    learning_rate = 0.0001
    action_space = 4  # W A S D, there are four buttons
    current_action = None
    current_state = None
    all_loss = []

    def __init__(self, batch_size=100, memory_size=2000, epochs=1, load_from=""):
        self.experiences = PriorityExperience(memory_size) if PRIORITY_EXPERIENCE_REPLAY else deque(maxlen=memory_size)
        self.batch_size = batch_size
        self.epochs = epochs

        self.__build_model__(load_from)
        
    # Neural net simple model
    def __build_simple_model__(self):
        """ Build a model without the depth camera """
        
        input2 = Input(shape=(8,))
        model2 = Dense(20, init='lecun_uniform')(input2)
        model2 = LeakyReLU(alpha=0.3)(model2)
        model2 = Dense(30, init='lecun_uniform')(model2)
        model2 = LeakyReLU(alpha=0.3)(model2)

        output = Dense(4, activation='linear')(model2)
        # output = Dense(12)(model2)
        # output = Dense(4, activation='linear')(output)

        self.model = Model(input=input2, output=output)

        self.model.compile(loss='mean_squared_logarithmic_error', optimizer=Adam(lr=self.learning_rate))

    # Neural net for depth camera
    def __build_kinect_model__(self):
        """ Build a model for use with depth camera """
        
        input1 = Input(shape=(640, 480, 1))
        
        # CNN for learning with video input
        conv1 = Convolution2D(200, 8, input_shape=(1, 640, 480, 1), data_format="channels_last")(
            input1)
        conv1 = LeakyReLU(alpha=0.3)(conv1)
        conv1 = Convolution2D(80, 2)(conv1)
        conv1 = LeakyReLU(alpha=0.3)(conv1)
        conv1 = MaxPool2D(pool_size=(2))(conv1)
        conv1 = Flatten()(conv1)

        # hidden layer 1
        input2 = Input(shape=(8,))
        model2 = Dense(20, init='lecun_uniform')(input2)
        model2 = LeakyReLU(alpha=0.3)(model2)
        model2 = Dense(30, init='lecun_uniform')(model2)
        model2 = LeakyReLU(alpha=0.3)(model2)

        merged = Concatenate()([conv1, model2])
        output = Dense(12)(merged)
        output = Dense(4, activation='linear')(output)

        self.model = Model(input=[input2, input1], output=output)

        self.model.compile(loss='mean_squared_logarithmic_error', optimizer=Adam(lr=self.learning_rate))

    def __build_model__(self, load_from=""):
        """ Build the model with its neural layers """
        
        if load_from == "":
            # choose model to build
            if not ENABLE_KINECT:
                self.__build_simple_model__()
            else:
                self.__build_kinect_model__()
        else:
            # likelihood of model choosing random action
            self.epsilon = 0.2
            # load saved model
            self.model = load_model(load_from)

            inputs = len(self.model.input_layers)

            if inputs == 2 and not ENABLE_KINECT:
                raise TypeError("Loaded AI expects kinect input, but kinect is not enabled!")
            elif inputs == 1 and ENABLE_KINECT:
                logging.warning("Attempting to attach kinect network to already trained virtual network")

                input1 = Input(shape=(640, 480, 1))
                input2 = Input(shape=(8,))

                conv1 = Convolution2D(200, 8, input_shape=(1, 640, 480, 1), data_format="channels_last")(input1)
                conv1 = LeakyReLU(alpha=0.3)(conv1)
                conv1 = Convolution2D(80, 2)(conv1)
                conv1 = LeakyReLU(alpha=0.3)(conv1)
                conv1 = MaxPool2D(pool_size=(2))(conv1)
                conv1 = Flatten()(conv1)

                new_model = self.model(input2)

                merged = Concatenate()([conv1, new_model])
                output = Dense(12)(merged)
                output = Dense(4, activation='linear')(output)

                self.model = Model(input=[input2, input1], output=output)

                self.model.compile(loss='mean_squared_logarithmic_error', optimizer=Adam(lr=self.learning_rate))

    def __shape_state__(self, state):
        """ Decide shape of the numpy arrays based on input given """
        
        if ENABLE_KINECT:
            # reshape to size of depth input
            return [np.array(state[0]).reshape((1, 8)), np.array(state[1]).reshape((1, 640, 480, 1))]
        else:
            return [np.array(state[0]).reshape((1, 8))]

    def save(self, reward, next_state, done):
        """ Save/remember the action taken """
        
        next_state = self.__shape_state__(next_state)

        data = (self.current_state, self.current_action, reward, next_state, done)

        if PRIORITY_EXPERIENCE_REPLAY:
            priority = self.calc_priority(self.current_state, self.current_action, reward, next_state)
            self.experiences.add(priority, data)
        else:
            self.experiences.append(data)

    def step(self, state):
        """ Take an action """
        
        state = self.__shape_state__(state)

        if np.random.random() <= self.epsilon:
            decided_action = random.randrange(self.action_space)
        else:
            actions = self.model.predict(state)

            # [0] because predict returns a 2d array (where the first dimnension is the sample)
            # We only have one sample whenever we step, so default to 0
            #
            # Use argmax because we want to pick the most likely action (or the action with the highest "best chance"
            # score)
            decided_action = np.argmax(actions[0])

        # Remember these variables for the save() function
        self.current_action = decided_action
        self.current_state = state

        return decided_action

    def calc_priority(self, state, action, reward, next_state, next_action=-1):
        """ Calculate priority of an action based on reward """
        
        if next_action == -1:
            # No next action provided, calculate on the fly
            next_action = np.argmax(self.model.predict(next_state)[0])

        confidance = self.model.predict(state)[0][action]
        result = self.model.predict(next_state)
        target = result[0][next_action]
        label = reward + self.gamma * target
        error = abs(label - confidance)
        return error

    def __build_deque_batch__(self, minibatch):
        """ Build batches without experience relay """
        
        batch_state_1 = []
        batch_state_2 = []
        batch_target = []
        for (state, action, reward, next_state, done) in minibatch:
            # for (idx, data) in minibatch:
            # state, action, reward, next_state, done = data

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

            batch_target.append(np.array(target_state).reshape((4,)))
            batch_state_1.append(state[0])
            if ENABLE_KINECT:
                batch_state_2.append(state[1])

        return batch_state_1, batch_state_2, batch_target

    def __build_priority_batch___(self, minibatch):
        """ Prioritize actions with higher rewards in batches with experience relay """
        
        batch_state_1 = []
        batch_state_2 = []
        batch_target = []
        for (idx, data) in minibatch:
            state, action, reward, next_state, done = data

            # If the current world state being used to train was not a
            # completed state
            if not done:
                # Apply next state to make the AI remembers what to do next
                next_action = self.model.predict(next_state)[0]
                target = reward + self.gamma * np.amax(next_action)
                priority = self.calc_priority(state, action, reward, next_state)
                self.experiences.update(idx, priority)
            # Otherwise it was a completed state
            else:
                target = reward

            # Get current action
            target_state = self.model.predict(state)
            target_state[0][action] = target

            batch_target.append(np.array(target_state).reshape((4,)))
            batch_state_1.append(state[0])
            if ENABLE_KINECT:
                batch_state_2.append(state[1])

        return batch_state_1, batch_state_2, batch_target

    def train(self):
        """ Train the model with batches """
        
        # if handling catastrophic forgetting
        # batches must represent the type of memory handling
        if PRIORITY_EXPERIENCE_REPLAY:
            minibatch = self.experiences.sample()
        else:
            minibatch = random.sample(self.experiences, min(BATCH_SIZE, len(self.experiences)))

        bsize = len(minibatch)

        batch_builder = self.__build_priority_batch___ if PRIORITY_EXPERIENCE_REPLAY else self.__build_deque_batch__

        batch_state_1, batch_state_2, batch_target = batch_builder(minibatch)

        # train on batches for faster processing
        batch_state_1 = np.array(batch_state_1).reshape(bsize, 8)
        batch_target = np.array(batch_target)
        if ENABLE_KINECT:
            batch_state_2 = np.array(batch_state_2).reshape(bsize, 640, 480, 1)
            history = self.model.fit([batch_state_1, batch_state_2], batch_target, batch_size=bsize,
                                     epochs=self.epochs, verbose=1)
        else:
            history = self.model.fit([batch_state_1], batch_target, batch_size=bsize, epochs=self.epochs, verbose=1)

        self.all_loss.append(history.history['loss'][0])

        if self.epsilon > self.emin:
            self.epsilon *= self.edecay

        return self.all_loss

    def save_model(self, path):
        """ Save model to file """
        
        self.model.save(path)
