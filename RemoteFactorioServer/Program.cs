using System;

using System.Text;

namespace RemoteFactorioServer
{
    class Program
    {

        private static string ip = "192.168.137.1"; //"25.42.5.80" hamachi or "192.168.137.1"

        private static Client client;

        static void Main(string[] args)
        {
            try
            {
                Start(args[0]);
            }
            catch (IndexOutOfRangeException)
            {
                Console.WriteLine("[CLIENT][ERROR] Argument Not Valid");

                Console.WriteLine("server or client ?");
                string mode = Console.ReadLine();
                Start(mode);
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

                client = new Client(ip);
                Console.WriteLine(client.Command_Ping() + " ms");

                Console.WriteLine("//////////////////\n"
                    + "Type `help` to get list of commands");
                while (true)
                {
                    string command = Console.ReadLine();
                    int result = 2;
                    if (command == "help")
                    {
                        Help();
                        result = 0;
                    }
                    else if (command.StartsWith("start"))
                    {
                        string parameter = command.Substring(5);
                        result = client.Command_Start(parameter);
                    }
                    else if (command.StartsWith("stop"))
                    {
                        string parameter = command.Substring(4);
                        result = client.Command_Stop(parameter);
                    }
                    else if (command.StartsWith("restart"))
                    {
                        string parameter = command.Substring(7);
                        result = client.Command_Restart(parameter);
                    }
                    else if (command == "exit")
                    {
                        break;
                    }
                    Errors(result);
                }
                Console.WriteLine("Remote ended...");
                client.Stop();
            }
            else
            {
                Errors(-1);
            }
        }

        private static void Help()
        {
            Console.WriteLine("start [<name>]\n"
                            + "     Start the Factorio Server, by default : The DUT one");
            Console.WriteLine("stop [<name>]\n"
                            + "     Stop the Factorio Server, by default : The DUT one");
            Console.WriteLine("restart [<name>]\n"
                            + "     Restart the Factorio Server, by default : The DUT one");
            Console.WriteLine("activate <name>\n"
                            + "     Activate the mod with the same name");
            Console.WriteLine("deactivate <name>\n"
                            + "     Deactivate the mod with the same name");
        }

        private static void Errors(int result)
        {
            if (result != 0)
            {
                string errorMessage;
                switch (result)
                {
                    case 1:
                        errorMessage = "[REMOTE] [ERROR] Unknown Argument";
                        break;
                    case 2:
                        errorMessage = "[REMOTE] [ERROR] Unknown Command";
                        break;
                    default:
                        errorMessage = "[REMOTE] [ERROR] Unexpected Error";
                        break;
                }
                Console.WriteLine(errorMessage);
            }
        }
    }
}
