using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Common;
using System.Diagnostics;

namespace InetClient
{
    public delegate void RecieveEventHandler(object sender, RecieveEventArgs e);

    /// <summary>
    /// Author: Erik Stolpe (estolpe@kth.se)
    /// 
    /// A class for handling the connection to a server.
    /// Listens to server responses and raises the RecieveEvent.
    /// Contains methods for sending data to the server.
    /// </summary>

    public class Client
    {
        public event RecieveEventHandler Recieved;

        private bool _listening;
        private Socket _socketClient;
        private string _data = null;
        private List<string> _messageLog = new List<string>();

        /// <summary>
        /// Connect to the port specified in the protocol
        /// and start listen for incoming responses.
        /// </summary>
        public void Connect()
        {
            try
            {
                // Establish the remote endpoint for the socket.   
                IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
                IPAddress ipAddress = ipHostInfo.AddressList[0];
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, Protocol.PORT);

                // Create a TCP socket.  
                _socketClient = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                try
                {
                    // Loop connect to the server.
                    while (!_socketClient.Connected)
                    {
                        try
                        {
                            _socketClient.Connect(remoteEP);
                        } catch (SocketException)
                        {
                            // Do nothing.
                        }
                    }
                    
                    // Start the listener worker thread.
                    _listening = true;
                    ThreadStart threadDelegate = new ThreadStart(StartListenerThread);
                    Thread ListenerThread = new Thread(threadDelegate);
                    ListenerThread.Start();

                } catch (Exception e)
                {
                    Debug.WriteLine("Unexpected error when connecting to server : {0}", e.ToString());
                }

            } catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
            }
        }

        // Starts a worker thread for recieving responses 
        // from the server.
        private void StartListenerThread()
        {
            while (_listening)
            {
                try
                {
                    Thread.Sleep(70);

                    // If the socket was closed.
                    if (!_socketClient.Connected) break;

                    // Recieve from server.
                    if (_socketClient.Available == 0) continue;

                    byte[] bytes = new byte[Protocol.BUFFER_SIZE];
                    int bytesRec = _socketClient.Receive(bytes);

                    _data += Encoding.UTF8.GetString(bytes, 0, bytesRec);

                    // Raise recieved event on end of message.
                    if (_data.Contains(Protocol.EOF))
                    {
                        _messageLog.Add(_data);
                        _data = null;

                        RecieveEventArgs e = new RecieveEventArgs() { Data = _messageLog.Last() };
                        OnRecieved(e);
                    }
                } catch (Exception e)
                {
                    Debug.WriteLine(e.ToString());
                }
            }
        }

        /// <summary>
        /// Sends a message to the server.
        /// </summary>
        /// <param name="message"></param>
        /// <returns>-1 if no connection,
        /// 0 if message was sent successfully.</returns>
        public int SendMessage(string message)
        {
            try
            {
                // Encode the data string into a byte array.  
                byte[] bytes = Encoding.UTF8.GetBytes(message);

                // Send the data through the socket.  
                int bytesSent = _socketClient.Send(bytes);
                return 0;
            } catch (Exception e)
            {
                Debug.WriteLine("Server stopped responding... \nPlease exit the client.");
                return -1;
            }
        }

        /// <summary>
        /// Stops the listening thread and closes the listener socket.
        /// </summary>
        public void StopListening()
        {
            _listening = false;
            _socketClient.Close();
        }

        // Called when the client recieves a message from the server,
        // invokes the recieved event.
        protected virtual void OnRecieved(RecieveEventArgs e)
        {
            Recieved?.Invoke(this, e);
        }

    }
}
