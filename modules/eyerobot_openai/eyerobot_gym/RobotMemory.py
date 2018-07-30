import numpy as np
from config import MEMORY_SIZE, MEMORY_BIAS, MEMORY_POWER, BATCH_SIZE
import random


# This code is heavily based on the
# https://github.com/jaara/AI-blog/blob/master/Seaquest-DDQN-PER.py

class SumTree:
    write = 0

    def __init__(self, capacity):
        self.capacity = capacity
        self.tree = np.zeros(2 * capacity - 1)
        self.data = np.zeros(capacity, dtype=object)

    def __propagate__(self, idx, change):
        parent = (idx - 1) // 2

        self.tree[parent] += change

        if parent != 0:
            self.__propagate__(parent, change)

    def __fetch__(self, idx, s):
        left = 2 * idx + 1
        right = left + 1

        if left >= len(self.tree):
            return idx

        if s <= self.tree[left]:
            return self.__fetch__(left, s)
        else:
            return self.__fetch__(right, s - self.tree[left])

    def total(self):
        return self.tree[0]

    def add(self, p, data):
        idx = self.write + self.capacity - 1

        self.data[self.write] = data
        self.update(idx, p)

        self.write += 1
        if self.write >= self.capacity:
            self.write = 0

    def update(self, idx, p):
        change = p - self.tree[idx]

        self.tree[idx] = p
        self.__propagate__(idx, change)

    def get(self, s):
        idx = self.__fetch__(0, s)
        dataIdx = idx - self.capacity + 1

        return idx, self.tree[idx], self.data[dataIdx]


class PriorityExperience:
    def __init__(self, memory_size=MEMORY_SIZE):
        self.tree = SumTree(memory_size)

    def add(self, error, sample):
        priority = (error + MEMORY_BIAS) ** MEMORY_POWER
        self.tree.add(priority, sample)

    def sample(self):
        batch = []
        segment = self.tree.total() / BATCH_SIZE
        for i in range(BATCH_SIZE):
            minimum = segment * i
            maximum = segment * (i + 1)
            s = random.uniform(minimum, maximum)
            (idx, _, data) = self.tree.get(s)
            batch.append((idx, data))
        return batch

    def update(self, idx, error):
        priority = (error + MEMORY_BIAS) ** MEMORY_POWER
        self.tree.update(idx, priority)
