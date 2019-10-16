using System;

namespace RemoteFactorioServer
{
    class Program
    {
        static bool isToStop = false;

        static void Main(string[] args)
        {
            do
            {
                isToStop = false;

                Console.WriteLine("server or client ?");
                string mode = Console.ReadLine();
                if (mode == "server")
                {
                    string localip = "25.42.5.80"; //"25.42.5.80" hamachi or "192.168.137.1"
                    Console.WriteLine(string.Format("Server listening on {0}", localip));

                    var server = new Server();
                    server.ExecuteServer(localip);
                }
                else if (mode == "client")
                {
                    string targetip = "25.42.5.80";
                    Console.WriteLine(string.Format("Server listening on {0}", targetip));

                    var client = new Client();
                    client.StartClient(targetip);
                    Console.WriteLine(client.Ping() + " ms");
                    client.StopClient();
                }
                else
                {
                    Console.WriteLine("WHAT ?!");
                    Console.WriteLine("=======");
                    isToStop = true;
                }
            } while (isToStop);
        }
    }
}
