package io.edkek.eyerobot.world;

import io.edkek.eyerobot.network.impl.RobotClient;
import io.edkek.eyerobot.network.packet.robot.UpdateMotorPacket;
import io.edkek.eyerobot.utils.PRunnable;
import io.edkek.eyerobot.utils.Quaternion;
import io.edkek.eyerobot.utils.Vector3;

import java.io.IOException;
import java.util.ArrayList;

public class Robot {

    private transient String name;
    private transient RobotClient client;
    private transient ArrayList<PRunnable<Robot>> callbacks = new ArrayList<>();
    private transient World world;

    private Vector3 acceleration = new Vector3();
    private Vector3 compass = new Vector3();
    private Quaternion orientation = new Quaternion();
    private int motor1;
    private int motor2;
    private int motor3;
    private int motor4;

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

    public void update(int motor1, int motor2, int motor3, int motor4,
                       float accX, float accY, float accZ,
                       float compassX, float compassY, float compassZ,
                       float orientationX, float orientationY, float orientationZ, float orientationW) {
        this.motor1 = motor1;
        this.motor2 = motor2;
        this.motor3 = motor3;
        this.motor4 = motor4;

        this.acceleration.x = accX;
        this.acceleration.y = accY;
        this.acceleration.z = accZ;

        this.compass.x = compassX;
        this.compass.y = compassY;
        this.compass.z = compassZ;

        this.orientation.x = orientationX;
        this.orientation.y = orientationY;
        this.orientation.z = orientationZ;
        this.orientation.w = orientationW;

        for (PRunnable<Robot> callback : callbacks) {
            callback.run(this);
        }
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

    public Vector3 getAcceleration() {
        return acceleration;
    }

    public Vector3 getCompass() {
        return compass;
    }

    public Quaternion getOrientation() {
        return orientation;
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

        //client.getServer().getLogger().info("Sending test command");
        //UpdateMotorPacket p = new UpdateMotorPacket(client);
        //p.writePacket(-255, 255, -255, 255);
    }

    public World getWorld() {
        return world;
    }

    void setWorld(World world) {
        this.world = world;
    }
}
