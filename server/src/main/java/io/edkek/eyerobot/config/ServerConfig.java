package io.edkek.eyerobot.config;

import me.eddiep.jconfig.system.Config;
import me.eddiep.jconfig.system.annotations.DefaultValue;
import me.eddiep.jconfig.system.annotations.Getter;

public interface ServerConfig extends Config {

    @Getter(property = "serverPort")
    @DefaultValue(value = "2547")
    int getServerPort();

    @Getter(property = "serverIp")
    @DefaultValue(value = "")
    String getServerIP();

    @Getter(property = "secret")
    @DefaultValue(value = "super_secret_12345")
    String getServerSecret();
}
