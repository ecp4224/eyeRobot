package io.edkek.eyerobot.network;


import io.edkek.eyerobot.utils.Global;
import org.apache.logging.log4j.LogManager;
import org.apache.logging.log4j.Logger;

import java.io.IOException;

/**
 * Represents a Server that handles incoming packets and outgoing packets and handles the logic for them.
 */
public abstract class Server {
    private boolean running;
    private long tickRate = 16;
    private Thread tickThread;
    private int tickNano;
    protected boolean debug;
    protected Logger log = LogManager.getLogger(getClass().getSimpleName());


    public boolean isDebugMode() {
        return debug;
    }

    public Logger getLogger() {
        return log;
    }

    public void setDebugMode(boolean val) {
        this.debug = val;
    }

    /**
     * Start this server.
     */
    public final void start() {
        onStart();
        if (!running) {
            throw new IllegalStateException("super.onStart() was not invoked!");
        }
    }

    /**
     * Stop this server
     */
    public final void stop() {
        onStop();
        if (running) {
            throw new IllegalStateException("super.onStop() was not invoked!");
        }
    }

    /**
     * This method is invoked when {@link Server#start()} is invoked. <b>super.onStart()</b> should
     * always be invoked!
     */
    protected void onStart() {
        running = true;
        Global.LOGGER = log;
    }

    /**
     * This method is invoked when {@link Server#stop()} is invoked. <b>super.onStop()</b> should
     * always be invoked!
     */
    protected void onStop() {
        running = false;
        if (tickThread != null) {
            tickThread.interrupt();
            try {
                tickThread.join(15000);
            } catch (InterruptedException e) {
                e.printStackTrace();
            }
        }
    }

    /**
     * Whether this server is running or not
     * @return True if this server is running, false otherwise
     */
    public boolean isRunning() {
        return running;
    }

    /**
     * Start a {@link Runnable} process in another {@link Thread}
     * @param runnable The runnable to execute
     */
    protected void runInBackground(Runnable runnable) {
        new Thread(runnable).start();
    }

    /**
     * This method is invoked when a {@link Client} disconnects
     * @param client The client that disconnected
     * @throws IOException If there was a problem disconnecting the client
     */
    public void disconnect(Client client) throws IOException {

    }
}
