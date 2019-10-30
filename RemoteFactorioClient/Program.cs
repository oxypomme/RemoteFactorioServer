using System;



using System.IO;

using Newtonsoft.Json;

namespace RemoteFactorioServer
{
    // TODO : documente + commente
    // TODO : console (check file every x sec and write it console + send it to client) (2e fenêtre ou même fenêtre)
    // 25.42.5.80 => 25.52.25.118

    class Program
    {
        #region Private Fields
        static Config config = new Config();
        private static Client Client { get; set; }
        #endregion

        #region Main Function
        static void Main()
        {
            if (!File.Exists("config.json"))
            {
                File.Create("config.json").Close();
                using StreamWriter file = new StreamWriter("config.json");
                file.Write(@"{
    ""RemoteIp"":""127.0.0.1"",
    ""RemotePort"": 34198,
}");
                file.Close();
            }
            using (StreamReader file = new StreamReader("config.json"))
            {
                config = JsonConvert.DeserializeObject<Config>(file.ReadToEnd());
                file.Close();
            }

            try
            {
                Start();
            }
            catch (IndexOutOfRangeException)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[ERROR] Argument Not Valid, starting client");
                Console.ResetColor();

                Start();
            }

            Console.WriteLine("Press ENTER to exit...");
            _ = Console.ReadLine();

        }
        #endregion

        #region Private Methods
        private static void Start()
        {

            Console.WriteLine("Client connecting ...");

            Client = new Client(config.RemoteIp, config.RemotePort);

            if (LogIn() == 1)
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine("[FATAL] Wrong credentials !");
                Console.ResetColor();
                Client.Stop();
            }
            else
            {

                Console.WriteLine(Client.Command_Ping() + " ms");

                Console.WriteLine("//////////////////\n"
                    + "Type `help` to get list of commands");

                Commands();

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("[INFO] Remote ended...");
                Console.ResetColor();
            }
        }

        private static void Commands()
        {
            while (true)
            {
                string command = Console.ReadLine();
                int result = 2;
                string source = "CLIENT";
                if (command == "help")
                {
                    Help();
                    result = 0;
                }
                else if (command.StartsWith("start"))
                {
                    string parameter = command.Substring(5);
                    result = Client.Command_Start(parameter);
                }
                else if (command.StartsWith("stop"))
                {
                    string parameter = command.Substring(4);
                    result = Client.Command_Stop(parameter);
                }
                else if (command.StartsWith("restart"))
                {
                    string parameter = command.Substring(7);
                    result = Client.Command_Restart(parameter);
                }
                else if (command.StartsWith("ping"))
                {
                    string ping = Client.Command_Ping();
                    if (ping == "SocketERROR")
                    {
                        Errors(3, source);
                        break;
                    }
                    else
                    {
                        Console.WriteLine(ping + " ms");
                        result = 0;
                    }
                }
                else if (command == "exit")
                {
                    _ = Client.Command_Exit();
                    break;
                }
                else if (command == "clear")
                {
                    Console.Clear();
                    result = 0;
                }

                if (result != 3 || result != 2)
                {
                    source = "SERVER";
                }
                Errors(result, source);
            }
        }

        private static int LogIn()
        {
            Console.WriteLine("Username : ");
            string username = Console.ReadLine().ToString();
            Console.WriteLine("Password : ");
            string password = Console.ReadLine();

            if (Client.LogIn(username, password) != 0)
            {
                return 1;
            }
            return 0;
        }

        private static void Help()
        {
            Console.WriteLine("start <name>\n"
                            + "     Start the Factorio Server");
            Console.WriteLine("stop\n"
                            + "     Stop the Factorio Server, need to be started with the remote first");
            Console.WriteLine("restart\n"
                            + "     Restart the Factorio Server, need to be started with the remote first");
            Console.WriteLine("activate <name>\n"
                            + "     Activate the mod with the same name");
            Console.WriteLine("deactivate <name>\n"
                            + "     Deactivate the mod with the same name");
            Console.WriteLine("help\n"
                            + "     List of commands");
            Console.WriteLine("ping\n"
                            + "     Calculate the latency between client and server");
            Console.WriteLine("exit\n"
                            + "     Exit the client");
            Console.WriteLine("clear\n"
                            + "     Clears the console");
        }

        private static void Errors(int result, string source)
        {
            ConsoleColor color = ConsoleColor.White;
            string errorMessage;
            switch (result)
            {
                case 0:
                    errorMessage = "[" + source + "][INFO] Command executed successfully";
                    color = ConsoleColor.Yellow;
                    break;
                case 1:
                    errorMessage = "[" + source + "][ERROR] Unknown Argument";
                    color = ConsoleColor.Red;
                    break;
                case 2:
                    errorMessage = "[" + source + "][ERROR] Unknown Command";
                    color = ConsoleColor.Red;
                    break;
                case 3:
                    errorMessage = "[" + source + "][FATAL] Disconnected from server";
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.WriteLine(errorMessage);
                    Console.ResetColor();
                    _ = Client.Command_Exit();
                    break;
                case 4:
                    errorMessage = "[" + source + "][ERROR] File Error";
                    color = ConsoleColor.Red;
                    break;
                case 5:
                    errorMessage = "[SERVER][ERROR] Factorio Server already started or not started";
                    color = ConsoleColor.Red;
                    break;
                default:
                    errorMessage = "[ERROR] Unexpected Error";
                    color = ConsoleColor.Red;
                    break;
            }
            if (result != 3)
            {
                Console.ForegroundColor = color;
                Console.WriteLine(errorMessage);
                Console.ResetColor();
            }

        }
        #endregion
    }
}
