using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class Client : MonoBehaviour
{
    public static Client instance;
    public static NetworkAction networkAction;

    public static string IP;
    public static int Port;
    public static int ID;
    public static bool IsConnected = false;
    
    public static TCP Tcp;
    public static UDP Udp;
    
    private static Dictionary<int, ActionHandler> _packetHandlers;
    private delegate void ActionHandler(Packet packet);

    public void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Debug.Log("An instance already exists, destroying this instance");
            Destroy(this);
        }
        networkAction = new NetworkAction();
        InitialisePacketsIDReader();
    }

    public void OnApplicationQuit()
    {
        instance.Disconnect();
    }

    public void Connect(string ipPARAM,int portPARAM)
    {
        IP = ipPARAM;
        Port = portPARAM;
        
        Tcp = new TCP(this);
        Tcp.Connect();
        Udp = new UDP(this);
        Udp.Connect();
        
        GameManager.gameManager.isConnected = true;    
    }

    public void Disconnect()
    {
        if (IsConnected)
        {
            IsConnected = false;
            
            Tcp.DisConnect();
            Udp.Disconnect();
        }
    }
    
    public class TCP
    {
        private Client _client;
        public TcpClient socket;
        public NetworkStream stream;
        public Packet receiveData;
        private Byte[] receiveBuffer;

        public TCP(Client client)
        {
            _client = client;
        }
        public void Connect()
        {
            Debug.Log("Connection to "+IP+":"+Port+"...");
            socket = new TcpClient {ReceiveBufferSize = 4096, SendBufferSize = 4096};
            socket.BeginConnect(IP,Port,ConnectCallback,socket);
            receiveBuffer = new byte[4096];
        }

        public void DisConnect()
        {
            Debug.Log("Disconnecting...");
            socket.Close();
            stream = null;
            receiveData.Dispose();
            receiveBuffer = null;
        }

        public void ConnectCallback(IAsyncResult result)
        {
            socket.EndConnect(result);
            if (!socket.Connected)
            {
                Debug.Log("Error the connection as failed ! (unconnected after the connection request)");
                return;
            }
            
            Debug.Log("Successfully connected to the server : "+IP);

            stream = socket.GetStream();
            receiveData = new Packet();

            stream.BeginRead(receiveBuffer, 0, 4096, ReceiveCallback, null);
        }

        public void ReceiveCallback(IAsyncResult result)
        {
            int packetLenght = stream.EndRead(result);
            if (packetLenght <= 0)
            {
                
                Debug.Log("Error cause by the lenght of the packet < 1 the client while be disconnected");
                instance.Disconnect();
                return;
            }
            
            Byte[] data = new byte[packetLenght];
            Array.Copy(receiveBuffer,data,packetLenght);
            stream.BeginRead(receiveBuffer, 0, 4096, ReceiveCallback, null);

            //TCP receive data may contain multiple packets then handle all of them.
            Packet tempPacket = new Packet(data);

            while (tempPacket.GetUnreadLenght() > 4)
            {
                int tempLenght = tempPacket.ReadInt(true);
                _client.HandlePacket(new Packet(tempPacket.ReadBytes(tempLenght,true))); //Convert into a packet and handle it.
            }
                
            tempPacket.Dispose();
        }

        
        
        public void SendPacket(Packet packet,bool reUsePacket)
        {
            packet.InsertInt(packet.GetLenght());
            //If connection always exist.
            if (socket != null)
            {
                    stream.BeginWrite(packet.ReadAllBytes(), 0, packet.GetLenght(), null, null);
            }
            else
            {
                Debug.Log("TCP isn't connected cannot send data");
            }
            if(!reUsePacket) packet.Dispose();
        }
    }

    public class UDP
    {
        private Client _client;
        public UdpClient socket;
        public IPEndPoint EndPoint;

        public UDP(Client client)
        {
            _client = client;
        }

        public void Connect()
        {
            socket = new UdpClient();
            socket.Connect(IP,Port);
            socket.BeginReceive(ReceiveCallback, null);
        }

        public void Disconnect()
        {
            socket.Close();
            EndPoint = null;
        }

        public void ReceiveCallback(IAsyncResult result)
        {
            Byte[] data = socket.EndReceive(result, ref EndPoint);
            socket.BeginReceive(ReceiveCallback, null);
            
            _client.HandlePacket(new Packet(data));
            
        }

        public void SendPacket(Packet packet,bool reUsePacket)
        {
            packet.InsertInt(ID);
            socket.BeginSend(packet.ReadAllBytes(), packet.GetLenght(), null, null);
            if(!reUsePacket) packet.Dispose();
        }
    }
    
    public void HandlePacket(Packet packet)
    {
        int packetID = packet.ReadInt(true);
        Debug.LogError("incomming packet with id : "+packetID);
        _packetHandlers[packetID](packet);
    }

    private void InitialisePacketsIDReader()
    {
        _packetHandlers = new Dictionary<int, ActionHandler>()
        {
            {(int) Packet.ServerPacketIDReference.Connected, networkAction.Connnected},
            {(int) Packet.ServerPacketIDReference.DebugMessage, networkAction.DebugMessage},
            {(int) Packet.ServerPacketIDReference.NewPlayer, networkAction.NewPlayer},
            {(int) Packet.ServerPacketIDReference.UpdatePosOfAPlayer, networkAction.UpdatePosOfAPlayer},
            {(int) Packet.ServerPacketIDReference.RemovePlayer, networkAction.RemovePlayer}
        };
    }

}
