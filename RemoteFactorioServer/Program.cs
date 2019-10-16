using System;

namespace RemoteFactorioServer
{
    class Program
    {

        private static string ip = "192.168.137.1"; //"25.42.5.80" hamachi or "192.168.137.1"

        static void Main(string[] args)
        {
            try
            {
                Start(args[0]);
            }
            catch (IndexOutOfRangeException)
            {
                Console.WriteLine("Argument Not Valid");

                Console.WriteLine("server or client ?");
                string mode = Console.ReadLine();
                Start(mode);
                Console.WriteLine("//////////////////\n"
                                + "Type /help to get list of commands");
            }
        }

        private static void Start(string mode)
        {
            if (mode == "server")
            {
                Console.WriteLine(string.Format("Server listening on {0}", ip));

                var server = new Server(ip);
            }
            else if (mode == "client")
            {
                Console.WriteLine(string.Format("Client connecting to {0}", ip));

                var client = new Client(ip);
                Console.WriteLine(client.Ping() + " ms");
                client.Stop();
            }
        }

        private static void Help()
        {
            Console.WriteLine("start [name]\n"
                            + "     Start the Factorio Server, by default : The DUT one");
            Console.WriteLine("stop [name]\n"
                            + "     Stop the Factorio Server, by default : The DUT one");
            Console.WriteLine("restart [name]\n"
                            + "     Restart the Factorio Server, by default : The DUT one");
            Console.WriteLine("activate [name]\n"
                            + "     Activate the mod with the same name");
            Console.WriteLine("deactivate [name]\n"
                            + "     Deactivate the mod with the same name");
        }
    }
}
