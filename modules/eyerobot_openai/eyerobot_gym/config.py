# Where Unity is running
# This can be on the same machine
# or a remote machine
IP = "127.0.0.1"
PORT = 1338
# How much of a network buffer should be used
# Default is usually good
BUFFER = 2048

# Whether the kinect will be used during training
ENABLE_KINECT = False
# Whether to enable OpenCL during training
# If set to false, then tensorflow will be used
OPEN_CL = True
# How many batches to use during training step
BATCH_SIZE = 200
# Whether to use a priority SumTree for the experience replay or to use a normal deque
PRIORITY_EXPERIENCE_REPLAY = True
# How many actions should the AI remember
MEMORY_SIZE = 10000
MEMORY_BIAS = 0.01
MEMORY_POWER = 0.6
# How many iterations of the batch should be done during training
EPOCH = 3

# Unity KeyCode IDs
# LEAVE DEFAULT (unless keyboard layout changes)
W_KEY = 119
A_KEY = 97
S_KEY = 115
D_KEY = 100

# Environment settings
# How much movement is needed to consider the robot is moving
DISTANCE_TOLERANCE = 0.2
# How close the robot has to be to the goal
GOAL_TOLERANCE = 1
# How many steps is allowed before the robot fails
MAX_STEPS = 50000
# How long to wait before releasing the key
KEY_DELAY = 0.3
# How long to wait before checking if a new score arrived
SCORE_DELAY = 0.1
# Whether to debug log the environment
DEBUG = True
