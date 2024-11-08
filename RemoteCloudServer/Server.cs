﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using CommonClasses;

namespace RemoteCloudServer
{
    public class StateObject
    {
        // Size of receive buffer.  
        public const int BufferSize = 4096;

        // Receive buffer.  
        public byte[] buffer = new byte[BufferSize];

        // Received data string.
        public StringBuilder sb = new StringBuilder();

        // Client socket.
        public Socket workSocket = null;
    }

    public class AsynchronousServer
    {
        // Thread signal.  
        public static ManualResetEvent allDone = new ManualResetEvent(false);
        public static ManualResetEvent receiveDone = new ManualResetEvent(false);

        private static List<Thread> threads = new List<Thread>();
        private static int clientsCount = 1;
        private static Mutex threadListLock = new Mutex();
        private static Mutex userHandlerLock = new Mutex();

        private static List<User> loggedInUsers = new List<User>();

        public AsynchronousServer()
        {
        }
        public static void StartThreads()
        {
            while(true)
            {
                threadListLock.WaitOne(); // take ownership of the mutex
                if (clientsCount > threads.Count)
                {
                    threads.Add(new Thread(new ThreadStart(StartListening))); // start listening on a new client
                    threads.Last().IsBackground = true;
                    threads.Last().Start();
                }

                for(int i = 0; i < threads.Count; i++) // removing finished garbage threads
                {
                    if(!threads[i].IsAlive)
                    {
                        threads.RemoveAt(i);
                        i--;
                    }
                }
                threadListLock.ReleaseMutex(); // release ownership of the mutex
            }
        }


        public static void StartListening()
        {
            // Establish the local endpoint for the socket.  
            // The DNS name of the computer  
            // running the listener is "host.contoso.com".  
            IPAddress ipAddress = IPAddress.Any;
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 11000);

            int userState = 0;

            // Create a TCP/IP socket.  
            Socket listener = new Socket(ipAddress.AddressFamily,
                SocketType.Stream, ProtocolType.Tcp);

            // Bind the socket to the local endpoint and listen for incoming connections.  
            try
            {
                listener.Bind(localEndPoint);
                listener.Listen(10);

                while (true)
                {
                    // Set the event to nonsignaled state.  
                    allDone.Reset();

                    // Start an asynchronous socket to listen for connections.  
                    Console.WriteLine("Waiting for a connection...");
                    listener.BeginAccept(
                        new AsyncCallback(AcceptCallback),
                        listener);

                    // Wait until a connection is made before continuing.  
                    allDone.WaitOne();
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            clientsCount--;
            Console.WriteLine("\nPress ENTER to continue...");
            Console.Read();

        }

        // ACCEPTING
        public static void AcceptCallback(IAsyncResult ar)
        {
            try
            {
                // Signal the main thread to continue.
                allDone.Set();

                // Get the socket that handles the client request.  
                Socket listener = (Socket)ar.AsyncState;
                Socket handler = listener.EndAccept(ar);

                // Create the state object.  
                while (true)
                {
                    receiveDone.Reset();
                    StateObject state = new StateObject();
                    state.workSocket = handler;
                    handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                        new AsyncCallback(ReadCallback), state);
                    receiveDone.WaitOne();
                }
            }
            catch(Exception e)
            {

            }
        }

        public static void ReadCallback(IAsyncResult ar)
        {
            try {
                string content = string.Empty;

                // Retrieve the state object and the handler socket  
                // from the asynchronous state object.  
                StateObject state = (StateObject)ar.AsyncState;
                Socket handler = state.workSocket;

                // Read data from the client socket.
                int bytesRead = handler.EndReceive(ar);

                if (bytesRead > 0)
                {
                    // There  might be more data, so store the data received so far.  
                    state.sb.Append(Encoding.UTF8.GetString(
                        state.buffer, 0, bytesRead));

                    // Check for end-of-file tag. If it is not there, read
                    // more data.  
                    content = state.sb.ToString();

                        // All the data has been read from the
                        // client. Display it on the console.
                    Console.WriteLine("Read {0} bytes from socket. \n Data : {1}",
                        content.Length, content);

                    try
                    {
                        string[] contentSplit = content.Split(';', 3);

                        string response = HandleRequest(contentSplit[0], contentSplit[2], content);

                        Send(handler, response);
                        receiveDone.Set();
                    }
                    catch (SocketException ex)
                    {
                        Console.WriteLine();
                    }
                    catch (Exception ex)
                    {
                        string response = "ERROR";
                        Send(handler, response);
                        receiveDone.Set();
                    }
                }
            }
            catch (Exception e) { };
        }


        // SENDING
        private static void Send(Socket handler, String data)
        {
            // Convert the string data to byte data using ASCII encoding.  
            byte[] byteData = Encoding.UTF8.GetBytes(data);

            // Begin sending the data to the remote device.  
            handler.BeginSend(byteData, 0, byteData.Length, 0,
                new AsyncCallback(SendCallback), handler);
        }

        private static void SendFile(Socket handler, string path)
        {
            // Begin sending the file to the remote device
            handler.BeginSendFile(path, new AsyncCallback(SendCallback), handler);
        }

        private static void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.  
                Socket handler = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.
                int bytesSent = handler.EndSend(ar);
                Console.WriteLine("Sent {0} bytes to client.", bytesSent);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private static string HandleRequest(string request, string content, string unsplitContent)
        {
            string reply = "";

            userHandlerLock.WaitOne();
            switch (int.Parse(request))
            {
                case 200:
                    reply = RequestHandlers.HandleLoginRequest(content, ref loggedInUsers);
                    break;
                case 201:
                    reply = RequestHandlers.HandleLogoutRequest(content, ref loggedInUsers);
                    break;
                case 202:
                    reply = RequestHandlers.HandleSignupRequest(content);
                    break;
                case 300:
                case 3300:
                case 5300:
                    reply = RequestHandlers.HandleFileUploadRequest(content, ref loggedInUsers, request);
                    break;
                case 301:
                case 3301:
                case 5301:
                    reply = RequestHandlers.HandleFileDownloadRequest(content, ref loggedInUsers, request);
                    break;
                case 302:
                    reply = RequestHandlers.HandleDeleteFileRequest(content, ref loggedInUsers);
                    break;
                case 310:
                    reply = RequestHandlers.HandleMakeDirectoryRequest(content, ref loggedInUsers);
                    break;
                case 311:
                    reply = RequestHandlers.HandleDeleteDirecotryRequest(content, ref loggedInUsers);
                    break;
                case 100:
                    reply = RequestHandlers.HandleUpdateRequest(content, ref loggedInUsers);
                    break;
                default:
                    reply = RequestHandlers.HandleGenericRequest(content);
                    break;
            }

            userHandlerLock.ReleaseMutex();
            return reply;
        }
    }
}
