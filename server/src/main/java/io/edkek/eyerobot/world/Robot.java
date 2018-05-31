package io.edkek.eyerobot.world;

import io.edkek.eyerobot.network.impl.RobotClient;
import io.edkek.eyerobot.network.packet.robot.UpdateMotorPacket;
import io.edkek.eyerobot.utils.PRunnable;

import java.io.IOException;
import java.util.ArrayList;

public class Robot {

    private transient String name;
    private transient RobotClient client;
    private transient ArrayList<PRunnable<Robot>> callbacks = new ArrayList<>();

    private float acceleration;
    private float gyro;
    private int motor1;
    private int motor2;
    private int motor3;
    private int motor4;
    private byte[] rgbData;
    private byte[] depthData;

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

    public void update(float acc, float gyro, int motor1, int motor2, int motor3, int motor4,
                       byte[] rgbData, byte[] depthData) {
        this.acceleration = acc;
        this.gyro = gyro;
        this.motor1 = motor1;
        this.motor2 = motor2;
        this.motor3 = motor3;
        this.motor4 = motor4;
        this.rgbData = rgbData;
        this.depthData = depthData;

        for (PRunnable<Robot> callback : callbacks) {
            callback.run(this);
        }
    }

    public float getAcceleration() {
        return acceleration;
    }

    public float getGyro() {
        return gyro;
    }

    public int getMotor1() {
        return motor1;
    }

    public int getMotor2() {
        return motor2;
    }

    public int getMotor3() {
        return motor3;
    }

    public int getMotor4() {
        return motor4;
    }

    public byte[] getRgbData() {
        return rgbData;
    }

    public byte[] getDepthData() {
        return depthData;
    }

    public void addSensorListener(PRunnable<Robot> callback) {
        callbacks.add(callback);
    }

    public void onConnected() throws IOException {
        World worldServer = client.getServer().getWorld();

        if (worldServer.hasRobot(name)) {
            worldServer.removeRobot(worldServer.getRobot(name)); //Remove the old instance
        }

        worldServer.addRobot(this);

        //TODO Do other stuff

        client.getServer().getLogger().info("Sending test command");
        UpdateMotorPacket p = new UpdateMotorPacket(client);
        p.writePacket(-255, 255, -255, 255);
    }
}
