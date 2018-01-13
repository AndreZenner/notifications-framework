
/*
    -----------------------
    UDP-Receive (send to)
    -----------------------
    // [url]http://msdn.microsoft.com/de-de/library/bb979228.aspx#ID0E3BAC[/url]
*/
using UnityEngine;
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

public class UDPReceive : MonoBehaviour
{
    private Thread receiveThread;
    private UdpClient client;
    public int port = 9900;
    private string lastReceivedUDPPacket = ";0";

    public void Start()
    {
        init();
    }

    void OnApplicationQuit()
    {
        stopThread();
    }

    private void stopThread()
    {
        if (receiveThread.IsAlive)
        {
            receiveThread.Abort();
        }
        client.Close();
    }

    private void init()
    {
        // define local enpoint (where the messages get received)
        // start a new thread to receive messages
        receiveThread = new Thread(
            new ThreadStart(ReceiveData));
        receiveThread.IsBackground = true;
        receiveThread.Start();

        Debug.Log("Started listening to messages...");
    }

    private void ReceiveData()
    {
        client = new UdpClient(port);
        while (true)
        {

            try
            {
                // receive bytes
                IPEndPoint anyIP = new IPEndPoint(IPAddress.Parse("192.168.178.20"), port);
                byte[] data = client.Receive(ref anyIP);
                // decode with utf8 to string
                string text = Encoding.UTF8.GetString(data);

                // latest UDPpacket
                lastReceivedUDPPacket = text;
            }
            catch (Exception err)
            {
                print(err.ToString());
            }
        }
    }

    /*
     * returns the recieved message
     */
    public string getLatestUDPPacket()
    {
        return lastReceivedUDPPacket;
    }
}
