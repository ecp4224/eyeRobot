package io.edkek.eyerobot.world;

import io.edkek.eyerobot.network.impl.RobotClient;

public class Robot {

    private String name;
    private RobotClient client;
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

    public void update(float acc, float gyro, int motor1, int motor2, int motor3, int motor4, byte[] rgbData, byte[] depthData) {
        this.acceleration = acc;
        this.gyro = gyro;
        this.motor1 = motor1;
        this.motor2 = motor2;
        this.motor3 = motor3;
        this.motor4 = motor4;
        this.rgbData = rgbData;
        this.depthData = depthData;
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
}
