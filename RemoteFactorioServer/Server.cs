﻿using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;

namespace RemoteFactorioServer
{
    class Server
    {
        #region Private Fields
        private static Process proc = new Process();
        // Establish the local endpoint for the socket. Dns.GetHostName the name of the host running the application. 
        private static string serverIP = "127.0.0.1";
        private static readonly IPAddress ipAddr = IPAddress.Parse(serverIP);
        private static readonly IPEndPoint localEndPoint = new IPEndPoint(ipAddr, 34198);

        // Creation TCP/IP Socket using Socket Class Constructor 
        private static Socket listener = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        #endregion

        #region Public Constructors
        public Server(string ip)
        {
            serverIP = ip;

            // Using Bind() method we associate a network address to the Server Socket. All client that will connect to this Server Socket must know this network Address 
            listener.Bind(localEndPoint);

            // Using Listen() method we create the Client list that will want to connect to Server 
            listener.Listen(10);

            
        }
        #endregion

        #region Private Methods
        static public void StartServer()
        {
            while (true)
            {
                // Each new client
                Console.WriteLine("Waiting connection ... ");

                // Suspend while waiting for incoming connection Using Accept() method the server will accept connection of client 
                Socket clientSocket = listener.Accept();

                bool isLogged = false;

                if (!isLogged)
                {
                    try
                    {
                        if (LogIn(clientSocket) == 1)
                        {
                            clientSocket.Send(Encoding.ASCII.GetBytes("wrong<EOF>"));
                        }
                        else
                        {
                            isLogged = true;
                        }
                    }
                    catch (SocketException) { break; }
                }

                try
                {
                    while (isLogged)
                    {
                        // Each command
                        
                        string data = GetData(clientSocket);

                        Console.WriteLine("Message received -> {0} ", data);

                        int result = Commands(data);
                        byte[] message = Encoding.ASCII.GetBytes("ERROR<EOF>");
                        if (result >= 0)
                        {
                            message = Encoding.ASCII.GetBytes(result.ToString() + "<EOF>");
                            Console.WriteLine("Message sent <- {0} ", result.ToString() + "<EOF>");
                        }
                        else if (result == -1)
                        {
                            // Close client Socket using the Close() method. After closing, we can use the closed Socket for a new Client Connection 
                            clientSocket.Shutdown(SocketShutdown.Both);
                            clientSocket.Close();
                            break;
                        }
                        else if (result == -2)
                        {
                            message = Encoding.ASCII.GetBytes("pong !<EOF>");
                            Console.WriteLine("Message sent <- {0} ", "pong !<EOF>");
                        }

                        // Send a message to Client using Send() method 
                        try { clientSocket.Send(message); }
                        catch (SocketException) { break; }
                    }
                    isLogged = false;
                }
                catch (SocketException) { continue; }
            }
        }

        static private string GetData(Socket clientSocket)
        {
            byte[] bytes = new Byte[1024];
            string data = null;
            while (true)
            {
                int numByte = clientSocket.Receive(bytes);

                data += Encoding.ASCII.GetString(bytes,
                                           0, numByte);

                if (data.IndexOf("<EOF>") > -1)
                    break;
            }
            return data;
        }

        static private int LogIn(Socket clientSocket)
        {
            List<string> usernames = new List<string>();
            List<string> passwords = new List<string>();

            using (StreamReader file = new StreamReader("users.txt"))
            {
                string ln;

                while ((ln = file.ReadLine()) != null)
                {
                    usernames.Add(ln.Split('-')[0]);
                    passwords.Add(ln.Split('-')[1]);
                }
                file.Close();
            }

            string data = GetData(clientSocket);
            if (usernames.Contains(data.Split('<')[0]))
            {
                clientSocket.Send(Encoding.ASCII.GetBytes("ok<EOF>"));
                data = GetData(clientSocket);
                if (passwords.Contains(data.Split('<')[0]))
                {
                    clientSocket.Send(Encoding.ASCII.GetBytes("ok<EOF>"));
                    return 0;
                }
                return 1;
            }
            else
            {
                return 1;
            }
        }

        static private int Commands­(string message)
        {
            if (message == "ping !<EOF>")
            {
                return -2; // Return it's a ping request
            }
            else if (message.StartsWith("start"))
            {
                string parameter = message.Substring(5).Split("<")[0];
                Console.WriteLine("\"" + parameter + "\"");
                if (parameter == "DUT" || string.IsNullOrWhiteSpace(parameter))
                {
                    proc.StartInfo.FileName = @"D:\Program Files(x86)\Steam\steamapps\common\Factorio\bin\x64\servers\factorio - dut.cmd";
                }
                else
                {
                    return 1; // Return Argument Error
                }
                proc.StartInfo.CreateNoWindow = false;
                //proc.Start();
                //proc.WaitForExit();
                Console.WriteLine("STARTED");
                return 0; // Return everything's fine
            }
            else if (message.StartsWith("stop"))
            {
                string parameter = message.Substring(4).Split("<")[0];
                Console.WriteLine("\"" + parameter + "\"");
                if (parameter == "DUT" || string.IsNullOrWhiteSpace(parameter))
                {
                    //WIP
                    Console.WriteLine("STOPPED");
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            else if (message.StartsWith("restart"))
            {
                string parameter = message.Substring(7).Split("<")[0];
                if (parameter == "DUT" || string.IsNullOrWhiteSpace(parameter))
                {
                    if (Commands("stop " + parameter + "<EOF>") == 0)
                    {
                        if (Commands("start " + parameter + "<EOF>") == 0)
                        {
                            return 0;
                        }
                    }
                    return 100; // Return Unexpected Error
                }
                else
                {
                    return 1;
                }
            }
            else if (message == "dc<EOF>")
            {
                return -1; // Return it's a disconnect request
            }
            else
            {
                return 2; // Return Unknown Command
            }
        }
        #endregion
    }
}
