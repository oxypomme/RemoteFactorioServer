using System;
using System.Net;
using System.Net.Sockets;
using System.Text;




namespace RemoteFactorioServer
{
    class Client
    {
        #region Private Fields
        private readonly Socket sender;
        #endregion

        #region Public Constructors 
        public Client(string ip, int port)
        {
            while (true)
            {
                // Establish the remote endpoint for the socket. Uses port 34198 (factorio+1) on the computer. 
                IPAddress ipAddr = IPAddress.Parse(ip);
                IPEndPoint localEndPoint = new IPEndPoint(ipAddr, port);

                // Creation TCP/IP Socket using Socket Class Costructor 
                sender = new Socket(ipAddr.AddressFamily,
                           SocketType.Stream, ProtocolType.Tcp);

                try
                {
                    // Connect Socket to the remote endpoint using method Connect() 
                    sender.Connect(localEndPoint);
                    break;
                }
                catch (SocketException)
                {
                    // If can't connect
                    continue;
                }
            }
        }
        #endregion

        #region Private Methods
        private void SendData(string message)
        {
            byte[] messageSent = Encoding.ASCII.GetBytes(message);
            _ = sender.Send(messageSent);
        }

        private string GetData()
        {
            byte[] messageReceived = new byte[1024];
            int byteRecv = sender.Receive(messageReceived);
            return Encoding.ASCII.GetString(messageReceived, 0, byteRecv);
        }
        #endregion

        #region Public Methods
        public int LogIn(string user, string passwd)
        {
            SendData(user + "<EOF>");
            var message = GetData();
            if (message.Contains("ok"))
            {
                SendData(passwd + "<EOF>");
                message = GetData();
                if (message.Contains("ok"))
                {
                    return 0;
                }
                return 1;
            }
            return 1;
        }

        public void Stop()
        {
            // Close Socket using the method Close() 
            sender.Shutdown(SocketShutdown.Both);
            sender.Close();
        }

        public string Command_Ping()
        {
            DateTime pingStart = DateTime.Now;
            // Creation of message that we will send to Server 
            try
            {
                SendData("ping !<EOF>");
            }
            catch (SocketException)
            {
                return "SocketERROR";
            }


            // We receive the message using the method Receive(). This method returns number of bytes received, that we'll use to convert them to string 
            var message = GetData();
            Console.WriteLine("Message from Server -> {0}", message);
            if (message == "pong !<EOF>")
            {
                return DateTime.Now.Subtract(pingStart).Milliseconds.ToString();
            }
            return "+9999";
        }

        public int Command_Start(string name)
        {
            try
            {
                SendData("start " + name + "<EOF>");

                Console.WriteLine("Message from Client <- {0}", "start " + name + "<EOF>");
            }
            catch (SocketException)
            {
                return 3;
            }

            var message = GetData();
            Console.WriteLine("Message from Server -> {0}", message);
            return int.Parse(message.Split('<')[0]);
        }

        public int Command_Stop(string name)
        {
            try
            {
                SendData("stop " + name + "<EOF>");

                Console.WriteLine("Message from Client <- {0}", "stop " + name + "<EOF>");
            }
            catch (SocketException)
            {
                return 3;
            }

            var message = GetData();
            Console.WriteLine("Message from Server -> {0}", message);
            return int.Parse(message.Split('<')[0]);
        }

        public int Command_Restart(string name)
        {
            try
            {
                SendData("restart " + name + "<EOF>");

                Console.WriteLine("Message from Client <- {0}", "restart " + name + "<EOF>");
            }
            catch (SocketException)
            {
                return 3;
            }

            var message = GetData();
            Console.WriteLine("Message from Server -> {0}", message);
            if (message == "0<EOF>")
            {
                return 0;
            }
            return 1;
        }

        public int Command_Exit()
        {
            try
            {
                SendData("dc<EOF>");
            }
            catch (SocketException)
            {
                return 3;
            }
            Console.WriteLine("Message from Client <- {0}", "dc<EOF>");
            Stop();
            return 0;
        }
        #endregion
    }
}
