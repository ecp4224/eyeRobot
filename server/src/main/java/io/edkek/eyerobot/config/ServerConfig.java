package io.edkek.eyerobot.config;

import me.eddiep.jconfig.system.Config;
import me.eddiep.jconfig.system.annotations.DefaultValue;
import me.eddiep.jconfig.system.annotations.Getter;

public interface ServerConfig extends Config {

    @Getter(property = "serverPort")
    @DefaultValue(value = "1337")
    int getServerPort();

    @Getter(property = "serverIp")
    @DefaultValue(value = "")
    String getServerIP();

    @Getter(property = "enforceIp")
    @DefaultValue(value = "false")
    boolean enforcceIp();

    @Getter(property = "allowReconnect")
    @DefaultValue(value = "true")
    boolean allowReconnect();
}
