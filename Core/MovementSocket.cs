using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WebAPI.NetCore.Core
{
    public sealed class SingletonMovement
    {
        public static Socket socket;
        private static SingletonMovement instance = null;
        private static readonly object padlock = new object();
        public static bool _mode = false;
        // Thread signal. 
        public static ManualResetEvent allDone = new ManualResetEvent(false);

        public SingletonMovement()
        {
        }
        public static SingletonMovement Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                    {
                        instance = new SingletonMovement();                        
                    }
                    return instance;
                }
            }
        }
        private static IWebHostEnvironment _env;
        private static Microsoft.AspNetCore.SignalR.IHubContext<Core.ClientHub> _hub;
        public void StartListening(IWebHostEnvironment env, Microsoft.AspNetCore.SignalR.IHubContext<Core.ClientHub> hub, bool mode)
        {
            
            _env = env;
            _hub = hub;
            _mode = mode;
            // Bind the socket to the local endpoint and listen for incoming connections.  
            try
            {
                // To force the socket to Close......................
                //if (socket != null && (socket.IsBound || !_mode))
                //{
                //    if (socket.Connected)
                //        socket.Shutdown(SocketShutdown.Both);
                    
                //    // Reset safe thread
                //    allDone.Reset();
                //    socket.Close();
                //    socket.Dispose();
                //    _mode = false;
                //    socket = null;
                //    return;
                //}


                // Establish the local endpoint for the socket.  
                // The DNS name of the computer  
                // running the listener is "host.contoso.com".  
                IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
                IPAddress ipAddress = IPAddress.Parse("127.0.0.1");//ipHostInfo.AddressList[3];
                IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 11112);
                // Create a TCP/IP socket.  
                socket = new Socket(ipAddress.AddressFamily,
                    SocketType.Stream, ProtocolType.Tcp);

                socket.Bind(localEndPoint);
                socket.Listen(10);

                while (socket != null && socket.IsBound && _mode)
                {
                    // Set the event to nonsignaled state.  
                    allDone.Reset();

                    // Start an asynchronous socket to listen for connections.  
                    Debug.WriteLine("Waiting for a connection...");
                    socket.BeginAccept(
                        new AsyncCallback(AcceptCallback),
                        socket);

                    // Wait until a connection is made before continuing.  
                    allDone.WaitOne();
                }

            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
            }
            //Debug.WriteLine("\nPress ENTER to continue...");
            //Debug.Read();

        }
        public static void AcceptCallback(IAsyncResult ar)
        {
            // Signal the main thread to continue.  
            try
            {
                allDone.Set();

                // Get the socket that handles the client request.  
                Socket listener = (Socket)ar.AsyncState;
                Socket handler = listener.EndAccept(ar);

                // Create the state object.  
                MovementStateObject state = new MovementStateObject();
                state.workSocket = handler;
                handler.BeginReceive(state.buffer, 0, MovementStateObject.BufferSize, 0,
                    new AsyncCallback(ReadCallback), state);
            }
            catch (Exception ex) 
            {
                Debug.Print(ex.Message);
            }
        }
        public static void ReadCallback(IAsyncResult ar)
        {
            try
            {
                // To force the socket to close
                if (!_mode)
                {
                    allDone.Reset();
                    //return;
                }

                String content = String.Empty;

                // Retrieve the state object and the handler socket  
                // from the asynchronous state object.  
                MovementStateObject state = (MovementStateObject)ar.AsyncState;
                Socket handler = state.workSocket;

                // Read data from the client socket.   
                int bytesRead = handler.EndReceive(ar);

                if (bytesRead > 0)
                {
                    // There  might be more data, so store the data received so far.  
                    state.sb.Append(Encoding.ASCII.GetString(
                        state.buffer, 0, bytesRead));

                    // Check for end-of-file tag. If it is not there, read   
                    // more data.  
                    content = state.sb.ToString();
                    Debug.WriteLine("Read {0} bytes from socket. \n Data : {1}",
                       content.Length, content);
                    if (!string.IsNullOrEmpty(content))
                    {
                        //string path = Path.Combine(_env.WebRootPath, @"Processor\streaming");
                        // Image file must have been already uploaded to the location
                        //string name = "todo-filename-" + DateTime.Now.ToString("dd-MM-yyyy") + ".txt";
                        //File.AppendAllText(Path.Combine(path, name), content + "\n");
                        _hub.Clients.All.SendCoreAsync("ReceiveMessage", new object[] { "Movement: ", content });
                        state.sb.Clear();
                    }
                    if (content.IndexOf("<EOF>") == -1)
                    {
                        // Not all data received. Get more.  
                        handler.BeginReceive(state.buffer, 0, MovementStateObject.BufferSize, 0,
                        new AsyncCallback(ReadCallback), state);
                    }
                    // Echo the data back to the client.  
                    Send(handler, content);
                }
            }
            catch (Exception ex)
            {
                Debug.Print(ex.Message);
            }
        }
        private static void Send(Socket handler, String data)
        {
            // Convert the string data to byte data using ASCII encoding.  
            if (!_mode)
            {
                data = "<EOF>";
            }
            byte[] byteData = Encoding.ASCII.GetBytes(data);

            // Begin sending the data to the remote device.  
            handler.BeginSend(byteData, 0, byteData.Length, 0,
                new AsyncCallback(SendCallback), handler);
        }
        private static void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.  
                Socket handler = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.  
                int bytesSent = handler.EndSend(ar);
                Debug.WriteLine("Sent {0} bytes to client.", bytesSent);

                if (!_mode)
                {
                    Debug.WriteLine("Shutting down the socket...");
                    handler.Shutdown(SocketShutdown.Both);
                    handler.Close();
                    if (socket != null && (socket.IsBound || !_mode))
                    {
                        if (socket.Connected)
                            socket.Shutdown(SocketShutdown.Both);

                        // Reset safe thread
                        allDone.Reset();
                        socket.Close();
                        socket.Dispose();
                        _mode = false;
                        socket = null;
                        //return;
                    }
                }

            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
            }
        }
    }
    // State object for reading client data asynchronously  
    public class MovementStateObject
    {
        // Client  socket.  
        public Socket workSocket = null;
        // Size of receive buffer.  
        public const int BufferSize = 1024;
        // Receive buffer.  
        public byte[] buffer = new byte[BufferSize];
        // Received data string.  
        public StringBuilder sb = new StringBuilder();
    }
}
