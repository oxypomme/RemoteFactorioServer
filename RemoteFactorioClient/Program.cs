using System;



using System.IO;



namespace RemoteFactorioServer
{
    // TODO : documente + commente
    // TODO : console (check file every x sec and write it console + send it to client) (2e fenêtre ou même fenêtre)
    // TODO : retire debug
    // TODO : fichier config (users + ip + ports + etc.)

    class Program
    {
        #region Private Fields
        private static string ip = "127.0.0.1"; //"25.42.5.80" hamachi => 

        private static Client client { get; set; }
        #endregion

        #region Main Function
        static void Main(string[] args)
        {
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

            Console.WriteLine(string.Format("Client connecting to {0}", ip));

            client = new Client(ip);

            if (LogIn() == 1)
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine("[FATAL] Wrong credentials !");
                Console.ResetColor();
                client.Stop();
            }
            else
            {

                Console.WriteLine(client.Command_Ping() + " ms");

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
                else if (command == "clear")
                {
                    Console.Clear();
                    result = 0;
                }
                Errors(result);
            }
        }

        private static int LogIn()
        {
            Console.WriteLine("Username : ");
            string username = Console.ReadLine().ToString();
            Console.WriteLine("Password : ");
            string password = Console.ReadLine();

            if (client.LogIn(username, password) != 0)
            {
                return 1;
            }
            return 0;
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
                            + "     Exit the client");
            Console.WriteLine("clear\n"
                            + "     Clears the console");
        }

        private static void Errors(int result)
        {
            ConsoleColor color = ConsoleColor.White;
            string errorMessage;
            switch (result)
            {
                case 0:
                    errorMessage = "[INFO] Command executed successfully";
                    color = ConsoleColor.Yellow;
                    break;
                case 1:
                    errorMessage = "[ERROR] Unknown Argument";
                    color = ConsoleColor.Red;
                    break;
                case 2:
                    errorMessage = "[ERROR] Unknown Command";
                    color = ConsoleColor.Red;
                    break;
                case 3:
                    errorMessage = "[FATAL] Disconnected from server";
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.WriteLine(errorMessage);
                    Console.ResetColor();
                    _ = client.Command_Exit();
                    break;
                default:
                    errorMessage = "[ERROR] Unexpected Error";
                    color = ConsoleColor.Red;
                    break;
            }
            Console.ForegroundColor = color;
            Console.WriteLine(errorMessage);
            Console.ResetColor();

        }
        #endregion
    }
}
