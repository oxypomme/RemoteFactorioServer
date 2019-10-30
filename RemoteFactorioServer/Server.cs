using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using Newtonsoft.Json;

namespace RemoteFactorioServer
{
    class Server
    {
        // TODO : documente + commente
        // TODO : console (check file every x sec and write it console + send it to client) (2e fenêtre ou même fenêtre)
        // TODO : retire debug
        // TODO : gestion erreurs

        #region Private Fields
        static readonly Process proc = new Process();
        static bool isFServerStarted = false;
        static Config config = new Config();

        // Creation TCP/IP Socket using Socket Class Constructor 
        static Socket listener;
        #endregion

        #region Main Function
        static void Main()
        {
            if (!File.Exists("config.json"))
            {
                File.Create("config.json").Close();
                using (StreamWriter file = new StreamWriter("config.json"))
                {
                    file.Write(@"{
    ""RemoteIp"":""127.0.0.1"",
    ""RemotePort"": 34198,
    ""Servers"":[],
    ""ServerFolder"":""D:\\Program Files (x86)\\Steam\\steamapps\\common\\Factorio\\bin\\x64\\servers\\"",
    ""ServerStartPoint"":""factorio-server.cmd"",
    ""Usernames"":[""admin""],
    ""Passwords"":[""12345""]
}");
                    file.Close();
                }
                using (StreamReader file = new StreamReader("config.json"))
                {
                    config = JsonConvert.DeserializeObject<Config>(file.ReadToEnd());
                    file.Close();
                }
            }
            else
            {
                using StreamReader file = new StreamReader("config.json");
                config = JsonConvert.DeserializeObject<Config>(file.ReadToEnd());
                file.Close();
            }

            try
            {
                IPAddress ipAddr = IPAddress.Parse(config.RemoteIp);
                IPEndPoint localEndPoint = new IPEndPoint(ipAddr, config.RemotePort);
                listener = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                // Using Bind() method we associate a network address to the Server Socket. All client that will connect to this Server Socket must know this network Address 
                listener.Bind(localEndPoint);
            }
            catch (NullReferenceException)
            {
                Console.WriteLine("[FATAL] Config.json is empty or missing parameters !");
                Console.WriteLine("Press ENTER to exit...");
                _ = Console.ReadLine();
                return;
            }

            // Using Listen() method we create the Client list that will want to connect to Server 
            listener.Listen(10);

            Console.WriteLine(string.Format("[INFO] Server listening on {0}", config.RemoteIp));
            StartServer();

            Console.WriteLine("Press ENTER to exit...");
            _ = Console.ReadLine();
        }
        #endregion

        #region Private Methods
        static void StartServer()
        {
            while (true)
            {
                // Each new client
                Console.WriteLine("[INFO] Waiting connection ... ");

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

                        Console.WriteLine("[DEBUG] Message received -> {0} ", data);

                        int result = Commands(data);
                        byte[] message = Encoding.ASCII.GetBytes("ERROR<EOF>");
                        if (result >= 0)
                        {
                            message = Encoding.ASCII.GetBytes(result.ToString() + "<EOF>");
                            Console.WriteLine("[DEBUG] Message sent <- {0} ", result.ToString() + "<EOF>");
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
                            Console.WriteLine("[DEBUG] Message sent <- {0} ", "pong !<EOF>");
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

        static string GetData(Socket clientSocket)
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

        static int LogIn(Socket clientSocket)
        {
            string data = GetData(clientSocket);
            if (config.Usernames.Contains(data.Split('<')[0]))
            {
                clientSocket.Send(Encoding.ASCII.GetBytes("ok<EOF>"));
                data = GetData(clientSocket);
                if (config.Passwords.Contains(data.Split('<')[0]))
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

        static int Commands­(string message)
        {
            if (message == "ping !<EOF>")
            {
                return -2; // Return it's a ping request
            }
            else if (message.StartsWith("start"))
            {
                string parameter = message.Substring(7).Split("<")[0];
                string servername;
                Console.WriteLine("\"" + parameter + "\"");
                if (config.Servers.Contains(parameter))
                {
                    servername = parameter;
                }
                else
                {
                    return 1; // Return Argument Error
                }
                proc.StartInfo.WorkingDirectory = string.Format("{0}{1}",config.ServerFolder, servername);
                proc.StartInfo.FileName = string.Format(@"{0}{1}\{2}", config.ServerFolder, servername, config.ServerStartPoint);
                proc.StartInfo.CreateNoWindow = true;
                if (!isFServerStarted)
                {
                    try
                    {
                        proc.Start();
                    }
                    catch (System.ComponentModel.Win32Exception)
                    {
                        return 4; // Return File Error
                    }
                }
                else
                {
                    return 5; // Return server error
                }
                isFServerStarted = true;
                proc.WaitForExit();
                return 0; // Return everything's fine
            }
            else if (message.StartsWith("stop"))
            {
                if (!isFServerStarted)
                {
                    try
                    {
                        proc.Kill();
                    }
                    catch (System.InvalidOperationException)
                    {
                        return 5;
                    }
                    isFServerStarted = false;
                }
                else
                {
                    return 5;
                }
                return 0;
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
