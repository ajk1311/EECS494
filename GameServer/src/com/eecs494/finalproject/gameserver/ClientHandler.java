package com.eecs494.finalproject.gameserver;

import java.io.IOException;
import java.io.InputStream;
import java.io.OutputStream;
import java.net.Socket;
import java.nio.ByteBuffer;
import java.nio.ByteOrder;
import java.util.concurrent.ArrayBlockingQueue;
import java.util.concurrent.BlockingQueue;

import com.eecs494.finalproject.gameserver.protobuf.GameServerProtos.ClientInfo;

public class ClientHandler implements Runnable {

	private static final int INT_SIZE = Integer.SIZE / Byte.SIZE;
	
	private static final BlockingQueue<ClientInfoWrapper> PLAYER_QUEUE = new ArrayBlockingQueue<ClientInfoWrapper>(50);
	
	private Socket mSocket;
	
	public ClientHandler(Socket socket) {
		mSocket = socket;
	}
	
	@Override
	public void run() {
		InputStream inStream = null;
		try {
			byte[] buff_len = new byte[INT_SIZE];
			inStream = mSocket.getInputStream();
			inStream.read(buff_len);
			System.out.println(buff_len);
			int length = ByteBuffer.wrap(buff_len).order(ByteOrder.LITTLE_ENDIAN).getInt();
			System.out.println("length: " + length);
			byte[] client_data = new byte[length];
			inStream.read(client_data);
			
			final ClientInfo incoming = ClientInfo.parseFrom(client_data);
			final ClientInfoWrapper waiting = PLAYER_QUEUE.poll();
			System.out.println("The connecting client is: " + incoming.getName());
			if (waiting == null) {
				System.out.println("No client currently waiting, entering wait queue");
				PLAYER_QUEUE.put(new ClientInfoWrapper(incoming, mSocket));
			} else {
				//TODO: add check to make sure connecting clients are different
				System.out.println("Found game, connecting: " + incoming.getName() + "and" + waiting.info.getName());
				hookUpPlayers(incoming, waiting);
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
	
	/** Sets up the connection between two players, passing off each one's connection information to the other. */
	private void hookUpPlayers(ClientInfo incoming, ClientInfoWrapper waiting) {
		// Make sure we have a connection for both entities
		final Socket hostSocket = mSocket;
		final Socket clientSocket = waiting.socket;
		
		// Set up the appropriate information for the host and client
		final ClientInfo hostInfo = ClientInfo.newBuilder(waiting.info).setIsHost(true).build();
		final ClientInfo clientInfo = ClientInfo.newBuilder(incoming).setIsHost(false).build();
		
		// Send the host and the client their respective information
		// Any set up handshake that takes place now executes on those devices
		OutputStream hostStream = null;
		OutputStream clientStream = null;
		try {
			hostStream = hostSocket.getOutputStream();
			int info_len = hostInfo.getSerializedSize();
			ByteBuffer bBuf = ByteBuffer.allocate(INT_SIZE + info_len).order(ByteOrder.LITTLE_ENDIAN);
			bBuf.putInt(info_len);
			bBuf.put(hostInfo.toByteArray());
			hostStream.write(bBuf.array());
			
			clientStream = clientSocket.getOutputStream();
			clientStream.write(ByteBuffer.allocate(INT_SIZE)
					.putInt(clientInfo.getSerializedSize()).order(ByteOrder.LITTLE_ENDIAN).array());
			clientInfo.writeTo(clientStream);
		} catch (IOException ioe) {
			ioe.printStackTrace();
			// TODO handle error
		} finally {
			if (hostStream != null) {
				try {
					hostStream.close();
				} catch (IOException e) {
				}
			}
			if (clientStream != null) {
				try {
					clientStream.close();
				} catch (IOException e) {
				}
			}
			if (hostSocket != null && !hostSocket.isClosed()) {
				try {
					hostSocket.close();
				} catch (IOException e) {
				}
			}
			if (clientSocket != null && !clientSocket.isClosed()) {
				try {
					clientSocket.close();
				} catch (IOException e) {
				}
			}
		}
	}
	
	/** Saves an open socket alongside the info for the associated client */
	private class ClientInfoWrapper {
		public ClientInfo info;
		public Socket socket;
		public ClientInfoWrapper(ClientInfo info, Socket socket) {
			this.info = info;
			this.socket = socket;
		}
	}
}
