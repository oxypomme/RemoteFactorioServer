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
                    Server.ExecuteServer("25.42.5.80");
                }
                else if (mode == "client")
                {
                    var client = new Client();
                    client.New("25.42.5.80");
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
