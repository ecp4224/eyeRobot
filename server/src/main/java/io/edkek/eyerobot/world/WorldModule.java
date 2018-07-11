package io.edkek.eyerobot.world;

import com.google.gson.Gson;
import com.google.gson.GsonBuilder;
import io.edkek.eyerobot.network.impl.ModuleClient;
import io.edkek.eyerobot.network.packet.module.JsonSensorInformationPacket;
import io.edkek.eyerobot.utils.PRunnable;
import io.edkek.eyerobot.utils.RobotSerializer;

import java.io.IOException;
import java.util.HashMap;

public class WorldModule {
    private String name;
    private ModuleClient client;
    private World world;

    private HashMap<String, Gson> gsonCache = new HashMap<>();

    public WorldModule(String name, ModuleClient client) {
        this.name = name;
        this.client = client;
    }

    public String getName() {
        return name;
    }

    public ModuleClient getClient() {
        return client;
    }

    public World getWorld() {
        return world;
    }

    /**
     * Request SensorInformation from a specific robot
     * @param robot The robot to get SensorInformation from
     * @param filter The filter to use
     */
    public void requestSensorInformation(Robot robot, String filter) {
        if (gsonCache.containsKey(robot.getName()))
            return; //Ignore if we already are requesting data

        RobotSerializer serializer;
        if (filter.length() > 0) {
            //Parse filter
            String[] filters = filter.split(",");
            serializer = new RobotSerializer(filters);
        } else {
            serializer = new RobotSerializer();
        }

        Gson gson = new GsonBuilder()
                .registerTypeAdapter(Robot.class, serializer)
                .create();

        gsonCache.put(robot.getName(), gson);

        robot.addSensorListener(SENSOR_CALLBACK);
    }

    void setWorld(World world) {
        this.world = world;
    }

    /**
     * This is invoked whenever the server receives a SensorInformationPacket
     * from the robot, AND if this WorldModule is listening to this robot
     */
    private final PRunnable<Robot> SENSOR_CALLBACK = new PRunnable<Robot>() {
        @Override
        public void run(Robot robot) {
            if (!gsonCache.containsKey(robot.getName()))
                return;

            Gson gson = gsonCache.get(robot.getName());

            String json = gson.toJson(robot);

            JsonSensorInformationPacket packet = new JsonSensorInformationPacket(client);
            try {
                packet.writePacket(robot.getName(), json);
            } catch (IOException e) {
                e.printStackTrace();
            }
        }
    };
}
