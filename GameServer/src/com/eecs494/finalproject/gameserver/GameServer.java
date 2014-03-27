package com.eecs494.finalproject.gameserver;

import java.io.IOException;
import java.net.ServerSocket;
import java.net.Socket;
import java.util.concurrent.ExecutorService;
import java.util.concurrent.Executors;

public class GameServer {

	private static final int SERVER_PORT = 9191;
	
	private static final ExecutorService EXECUTOR = Executors.newCachedThreadPool();
	
	public static void main(String[] args) {
		ServerSocket serverSocket = null;
		try {
			serverSocket = new ServerSocket(SERVER_PORT);
			while (true) {
				System.out.println("Waiting for client...");
				final Socket clientSocket = serverSocket.accept();
				EXECUTOR.execute(new ClientHandler(clientSocket));
			}
		} catch (IOException ioe) {
			ioe.printStackTrace();
			// TODO handle error
		} finally {
			if (serverSocket != null && !serverSocket.isClosed()) {
				try {
					serverSocket.close();
				} catch (IOException e) {
				}
			}
		}
	}
}
