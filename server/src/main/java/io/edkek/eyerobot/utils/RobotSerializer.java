package io.edkek.eyerobot.utils;

import com.google.gson.*;
import io.edkek.eyerobot.world.Robot;

import java.lang.reflect.Type;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.Iterator;
import java.util.List;

public class RobotSerializer implements JsonSerializer<Robot> {
    private List<String> fieldInclude;

    public RobotSerializer(String... toInclude) {
        fieldInclude = Arrays.asList(toInclude);
    }

    @Override
    public JsonElement serialize(Robot robot, Type type, JsonSerializationContext jsonSerializationContext) {
        JsonObject jObj = (JsonObject)new GsonBuilder().create().toJsonTree(robot);
        if (fieldInclude.size() > 0) {
            Iterator<String> memberIterator = jObj.keySet().iterator();
            while (memberIterator.hasNext()) {
                String f = memberIterator.next();

                if (fieldInclude.contains(f))
                    continue;;

                memberIterator.remove();
            }
        }

        return jObj;
    }
}
