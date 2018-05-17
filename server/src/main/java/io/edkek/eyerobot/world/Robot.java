package io.edkek.eyerobot.world;

import io.edkek.eyerobot.network.impl.RobotClient;

public class Robot {

    private String name;
    private RobotClient client;

    //Store other variables in here for the robot

    public Robot(String name, RobotClient client) {
        this.name = name;
        this.client = client;
    }

    public String getName() {
        return name;
    }

    public RobotClient getClient() {
        return client;
    }
}
