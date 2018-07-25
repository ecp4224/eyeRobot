from eyerobot_gym.Graph import Graph
import random

g = Graph(title="Testing", xlabel="Iteration", ylabel="Random Value")

g.show()

for i in range(1000):
    g.record(i, random.randrange(0, 10))