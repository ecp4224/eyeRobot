package io.edkek.eyerobot.world;

import io.edkek.eyerobot.network.impl.ModuleClient;

public class WorldModule {
    private String name;
    private ModuleClient client;
    private World world;

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

    void setWorld(World world) {
        this.world = world;
    }
}
