# eyeRobot
A robot that is able to move around the world on its own while detecting/avoiding obstacles, including objects, animals, and people.

See the Developer Documentation and the User Documentation for more information

# Layout
archive_code contains code that is not directly used in the project, but was used to test various components and future additions to the project. Files in the folder are not official and may not work correctly with the main project; they are simply there are reference.

client contains code that is run on the Raspberry Pi, such as connecting to the server and receiving motor commands. server_motor.py is the main script to run to get everything set up on the Pi.

server contains code that is run on the server computer. This is written in Java and connects the AI script, the Unity script, and the client scripts together. 

modules contains the scripts for the artificial intelligence. This includes the setup of the learning environment, the Unity code, and the script to build the neural networks.
