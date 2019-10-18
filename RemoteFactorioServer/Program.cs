using System;



using System.IO;



namespace RemoteFactorioServer
{
    // TODO : séparer clients et console en 2 exe
    // TODO : documente + commente
    // TODO : console (check file every x sec and write it console + send it to client) (2e fenêtre ou même fenêtre)
    // TODO : retire debug
    // TODO : fichier config (users + ip + ports + etc.)

    class Program
    {
        #region Private Fields
        private static string ip = "127.0.0.1"; //"25.42.5.80" hamachi => 

        private static Client client { get; set; }
        private static Server server { get; set; }
        #endregion

        #region Main Function
        static void Main(string[] args)
        {
            if (!File.Exists("users.txt"))
            {
                File.Create("users.txt");
            }

            try
            {
                Start(args[0]);
            }
            catch (IndexOutOfRangeException)
            {
                Console.WriteLine("[REMOTE] [ERROR] Argument Not Valid, starting client");
                //Start("client");

                Console.WriteLine("server or client ?"); //DEBUG
                string mode = Console.ReadLine(); //DEBUG
                Start(mode); //DEBUG
            }

            Console.WriteLine("//////////////////\n"
                    + "Type `help` to get list of commands");

            Commands();

            Console.WriteLine("[REMOTE] [INFO] Remote ended...");
        }
        #endregion

        #region Private Methods
        private static void Start(string mode)
        {
            if (mode == "server")
            {
                Console.WriteLine(string.Format("Server listening on {0}", ip));

                server = new Server(ip);
                Server.StartServer();
            }
            else if (mode == "client")
            {
                Console.WriteLine(string.Format("Client connecting to {0}", ip));

                client = new Client(ip);

                LogIn();

                Console.WriteLine(client.Command_Ping() + " ms");
            }
            else
            {
                Errors(-1);
            }
        }

        private static void Commands()
        {
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
                else if (command.StartsWith("ping"))
                {
                    Console.WriteLine(client.Command_Ping() + " ms");
                    result = 0;
                }
                else if (command == "exit")
                {
                    result = client.Command_Exit();
                    break;
                }
                Errors(result);
            }
        }

        private static void LogIn()
        {
            Console.WriteLine("Username : ");
            string username = Console.ReadLine().ToString();
            Console.WriteLine("Password : ");
            string password = Console.ReadLine();

            if (client.LogIn(username, password) != 0)
            {
                Console.WriteLine("Wrong credentials !");
                client.Command_Exit();
                return;
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
            Console.WriteLine("help\n"
                            + "     List of commands");
            Console.WriteLine("ping\n"
                            + "     Calculate the latency between client and server");
            Console.WriteLine("exit\n"
                            + "     Exit the remote");
        }

        private static void Errors(int result)
        {

            string errorMessage;
            switch (result)
            {
                case 0:
                    errorMessage = "[REMOTE] [INFO] Command executed successfully";
                    break;
                case 1:
                    errorMessage = "[REMOTE] [ERROR] Unknown Argument";
                    break;
                case 2:
                    errorMessage = "[REMOTE] [ERROR] Unknown Command";
                    break;
                case 3:
                    errorMessage = "[REMOTE] [FATAL] Disconnected from server";
                    Console.WriteLine(errorMessage);
                    _ = client.Command_Exit();
                    break;
                default:
                    errorMessage = "[REMOTE] [ERROR] Unexpected Error";
                    break;
            }
            Console.WriteLine(errorMessage);

        }
        #endregion
    }
}
