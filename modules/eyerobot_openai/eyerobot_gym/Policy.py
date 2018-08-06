from config import MAX_STEPS, GOAL_TOLERANCE, DISTANCE_TOLERANCE


class Policy:
    def __init__(self, starting_score=0):
        self.score = starting_score

    def calculate(self, env, difference):
        done = False

        if env.steps >= MAX_STEPS:
            done = True
            reward = 0
        elif difference <= GOAL_TOLERANCE:
            done = True
            reward = 0
        else:
            reward = MAX_STEPS - env.steps * (1 / difference)

        return reward, done

    def reset(self):
        self.score = 0


class SimpleScorePolicy(Policy):
    def calculate(self, env, difference):
        print(str(env.first_distance) + " - " + str(env.new_distance))
        to_add = (env.first_distance - env.new_distance)
        done = False

        # If the distance is -1, then it fell off the playground
        if env.new_distance == -1:
            done = True
            print "Fell"
            to_add = 0
            self.score = 0
            env.update_observation(-2)
        # If the distance is -2, then it won!
        elif env.new_distance == -2:
            done = True
            to_add = 100
            print "Completed"
            env.update_observation(3)
        # If the difference between the old distance and the new distance
        # is less than the negative of the tolerance
        # then the robot has moved further away
        #
        # i.e
        # prev_distance = 100
        # new_distance = 150
        # difference = 100 - 150
        # difference = -50
        # difference < -1 = TRUE
        elif difference < -DISTANCE_TOLERANCE:
            to_add = 0
            env.update_observation(-1)
        # If the difference between the old distance and the new distance
        # is greater than the tolerance
        # then the robot has moved closer
        elif difference > DISTANCE_TOLERANCE:
            env.update_observation(1)
        # Otherwise the difference is inside the tolerance and therefore
        # hasn't made any progress
        else:
            to_add = 0
            env.update_observation(0)

        if GOAL_TOLERANCE >= env.new_distance > 0:
            done = True
            print "Got to goal"
            to_add = 100
            env.update_observation(3)

        if env.step_count >= MAX_STEPS:
            done = True
            to_add = 0
            print "Too many steps"
            self.score = 0

        self.score += to_add

        reward = self.score

        if env.step_count >= MAX_STEPS:
            done = True
            print 'Too many steps"'
            reward = 0
        elif env.new_distance <= GOAL_TOLERANCE:
            done = True
            print "Got to goal"

        return reward, done


class EagerScorePolicy(SimpleScorePolicy):
    error_counter = 0
    reset_counter = 0
    max_errors = 4
    good_required_reset = 4
    previous_score = 0

    def calculate(self, env, difference):
        reward, done = SimpleScorePolicy.calculate(self, env, difference)

        reward_difference = reward - self.previous_score

        if reward_difference <= 0:
            self.error_counter += 1
            if self.error_counter >= self.max_errors:
                done = True
                self.score = -100
                reward = -100
        else:
            self.reset_counter += 1
            if self.reset_counter >= self.good_required_reset:
                self.error_counter = 0
                self.reset_counter = 0

        self.previous_score = reward

        return reward, done

    def reset(self):
        SimpleScorePolicy.reset(self)
        self.error_counter = 0
        self.reset_counter = 0
        self.previous_score = 0
