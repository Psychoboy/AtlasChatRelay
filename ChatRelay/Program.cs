using Newtonsoft.Json;
using Rcon;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatRelay
{
    class Program
    {
        private static ServerConfig _servers = new ServerConfig();
        static void Main(string[] args)
        {


            using (StreamReader file = File.OpenText(@"config.json"))
            {
                JsonSerializer serializer = new JsonSerializer();
                _servers = (ServerConfig)serializer.Deserialize(file, typeof(ServerConfig));
            }

            foreach (var server in _servers.Servers)
            {
                server.MessageReceived += Server_MessageReceived;
                server.Connect();
            }

            bool quit = false;
            while (!quit)
            {
                var result = Console.ReadLine();
                if (result.ToLower() == "q")
                {
                    quit = true;
                }
                else if (result.Trim().Length > 0)
                {
                    foreach (var server in _servers.Servers)
                    {
                        server.SendCustomMessage(result.Trim());
                    }
                }
            }
        }

        private static void Server_MessageReceived(object sender, string e)
        {
            if (_servers.RconOnly)
            {
                return;
            }
            var messages = e.Split(
                    new[] { Environment.NewLine },
                    StringSplitOptions.None
                );
            foreach (var obj in _servers.Servers)
            {
                var server = (Server)obj;
                if (server.Name == ((Server)sender).Name)
                {
                    continue;
                }

                SendServerMessage(server, messages);
                Console.WriteLine($"SENDING {e} to {server.Name}");
            }
        }

        private static void SendServerMessage(Server server, String[] messages)
        {
            foreach (var message in messages)
            {
                var lines = Split(message, 45);
                foreach (var lb in lines)
                {
                    server.SendChat(lb);
                }
            }
        }

        static IEnumerable<string> Split(string str, int chunkSize)
        {
            int partLength = chunkSize;
            string sentence = str;
            string[] words = sentence.Split(' ');
            var parts = new List<string>();
            string part = string.Empty;
            int partCounter = 0;
            foreach (var word in words)
            {
                if (part.Length + word.Length < partLength)
                {
                    part += string.IsNullOrEmpty(part) ? word : " " + word;
                }
                else
                {
                    parts.Add(part);
                    part = word;
                    partCounter++;
                }
            }
            parts.Add(part);
            return parts;
        }

        public static void LogMessage(string message)
        {
            Console.WriteLine(message);
        }
    }
}
