using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace RemoteFactorioServer
{
    class Client
    {
        private Socket sender;

        private byte[] messageReceived = new byte[1024];

        public Client(string ip = "127.0.0.1")
        {
            try
            {
                // Establish the remote endpoint for the socket. Uses port 34198 (factorio+1) on the computer. 
                IPAddress ipAddr = IPAddress.Parse(ip);
                IPEndPoint localEndPoint = new IPEndPoint(ipAddr, 34198);

                // Creation TCP/IP Socket using Socket Class Costructor 
                sender = new Socket(ipAddr.AddressFamily,
                           SocketType.Stream, ProtocolType.Tcp);

                try
                {
                    // Connect Socket to the remote endpoint using method Connect() 
                    sender.Connect(localEndPoint);

                }

                // Manage of Socket's Exceptions 
                catch (ArgumentNullException ane)
                {

                    Console.WriteLine("ArgumentNullException : {0}", ane.ToString());
                }

                catch (SocketException se)
                {

                    Console.WriteLine("SocketException : {0}", se.ToString());
                }

                catch (Exception e)
                {
                    Console.WriteLine("Unexpected exception : {0}", e.ToString());
                }
            }

            catch (Exception e)
            {

                Console.WriteLine(e.ToString());
            }
        }

        public void Stop()
        {
            try
            {
                try
                {
                    // Close Socket using the method Close() 
                    sender.Shutdown(SocketShutdown.Both);
                    sender.Close();
                }
                // Manage of Socket's Exceptions 
                catch (ArgumentNullException ane)
                {

                    Console.WriteLine("ArgumentNullException : {0}", ane.ToString());
                }

                catch (SocketException se)
                {

                    Console.WriteLine("SocketException : {0}", se.ToString());
                }

                catch (Exception e)
                {
                    Console.WriteLine("Unexpected exception : {0}", e.ToString());
                }
            }
            catch (Exception e)
            {

                Console.WriteLine(e.ToString());
            }
        }

        public string Ping()
        {
            DateTime pingStart = DateTime.Now;
            // Creation of message that we will send to Server 
            byte[] messageSent = Encoding.ASCII.GetBytes("ping !");
            int byteSent = sender.Send(messageSent);

            while (true)
            {
                // We receive the message using the method Receive(). This method returns number of bytes received, that we'll use to convert them to string 
                int byteRecv = sender.Receive(messageReceived);
                var message = Encoding.ASCII.GetString(messageReceived, 0, byteRecv);
                Console.WriteLine("Message from Server -> {0}", message);
                if (message == "pong !")
                {
                    return DateTime.Now.Subtract(pingStart).Milliseconds.ToString();
                }
            }
        }
    }
}
