package com.eecs494.finalproject.gameserver;

import java.io.IOException;
import java.io.InputStream;
import java.net.Socket;
import java.util.concurrent.ArrayBlockingQueue;
import java.util.concurrent.BlockingQueue;

import com.eecs494.finalproject.gameserver.protobuf.GameServerProtos.ClientInfo;

public class ClientHandler implements Runnable {

	private static final BlockingQueue<ClientInfo> PLAYER_QUEUE = new ArrayBlockingQueue<ClientInfo>(50);
	
	private Socket mSocket;
	
	public ClientHandler(Socket socket) {
		mSocket = socket;
	}
	
	@Override
	public void run() {
		InputStream inStream = null;
		try {
			inStream = mSocket.getInputStream();
			final ClientInfo incoming = ClientInfo.parseFrom(inStream);
			final ClientInfo waiting = PLAYER_QUEUE.poll();
			if (waiting == null) {
				PLAYER_QUEUE.put(incoming);
			} else {
				hookUpClients(incoming, waiting);
			}
		} catch (IOException ioe) {
			ioe.printStackTrace();
			// TODO handle error
		} catch (InterruptedException ee) {
			ee.printStackTrace();
			// TODO handle error
		} finally {
			if (inStream != null) {
				try {
					inStream.close();
				} catch (IOException e) {
				}
			}
		}
	}
	
	private void hookUpClients(ClientInfo client1, ClientInfo client2) {
		
	}
}
