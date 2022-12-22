using S5GameServices;
using System;

namespace S5GameServer
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            CDKeyServer.Run();
            IRCServer.Run();
            Console.WriteLine("Running CDKey, IRC Server...");

            while (Console.ReadKey(true).Key != ConsoleKey.Q)
                Console.WriteLine("Press Q to exit!");
        }
    }
}