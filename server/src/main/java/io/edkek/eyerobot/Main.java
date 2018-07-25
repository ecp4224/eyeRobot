package io.edkek.eyerobot;

import io.edkek.eyerobot.network.impl.EyeServer;

import java.util.Scanner;

public class Main {

    public static EyeServer server;
    public static void main(String[] args) {
        server = new EyeServer();
        server.start();
        server.getLogger().info("Server started");
        Scanner scanner = new Scanner(System.in);
        scanner.nextLine();
        server.stop();
        server.getLogger().info("Server stopped");
    }
}
