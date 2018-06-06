package io.edkek.eyerobot.world;

import java.util.ArrayList;
import java.util.Collections;
import java.util.HashMap;
import java.util.List;

public class World {

    private HashMap<String, WorldModule> modules = new HashMap<>();
    private HashMap<String, Robot> robots = new HashMap<>();

    /**
     * Add a module to the World. If the module is already in the world,
     * then a {@link IllegalStateException} is thrown
     * @param module The module to add
     */
    public void addModule(WorldModule module) {
        if (hasModule(module.getName()) || module.getWorld() != null)
            throw new IllegalStateException("Module already added!");

        modules.put(module.getName(), module);
        module.setWorld(this);
    }

    /**
     * Add a robot to the World. If the robot is already in the world,
     * then a {@link IllegalStateException} is thrown
     * @param robot The robot to add
     */
    public void addRobot(Robot robot) {
        if (hasRobot(robot.getName()))
            throw new IllegalStateException("Robot already added!");

        robots.put(robot.getName(), robot);
        robot.setWorld(this);
    }

    /**
     * Remove a module from the World. If the module is not in the world, then nothing happens
     * @param module The module to remove
     */
    public void removeModule(WorldModule module) {
        if (!hasModule(module.getName()))
            return;

        modules.remove(module.getName());
        module.setWorld(null);
    }

    /**
     * Remove a robot from the World. If the robot is not in the world, then nothing happens
     * @param robot The module to remove
     */
    public void removeRobot(Robot robot) {
        if (!hasRobot(robot.getName()))
            return;

        robots.remove(robot.getName());
        robot.setWorld(null);
    }

    /**
     * Check if the module is already in the World.
     * @param name The name of the module
     * @return True if the module name exists in the world, false otherwise
     */
    public boolean hasModule(String name) {
        return modules.containsKey(name);
    }

    /**
     * Check if the robot is already in the World.
     * @param name The name of the robot
     * @return True if the robot name exists in the world, false otherwise
     */
    public boolean hasRobot(String name) {
        return robots.containsKey(name);
    }

    public WorldModule getModule(String module) {
        return modules.get(module);
    }

    public List<WorldModule> getAllModules() {
        return Collections.unmodifiableList(new ArrayList<>(modules.values()));
    }

    public Robot getRobot(String name) {
        return robots.get(name);
    }
}
