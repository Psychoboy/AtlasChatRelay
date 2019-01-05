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
                if(result.ToLower() == "q")
                {
                    quit = true;
                }
                else if(result.Trim().Length > 0)
                {
                    foreach(var server in _servers.Servers)
                    {
                        server.SendCustomMessage(result.Trim());
                    }
                }
            }
        }

        private static void Server_MessageReceived(object sender, string e)
        {
            if(_servers.RconOnly)
            {
                return;
            }
            foreach (var obj in _servers.Servers)
            {
                var server = (Server)obj;
                if (server.Name == ((Server)sender).Name)
                {
                    continue;
                }

                var messages = e.Split(
                    new[] { Environment.NewLine },
                    StringSplitOptions.None
                );
                foreach (var message in messages)
                {
                        server.SendChat(message);
                }
                Console.WriteLine($"SENDING {e} to {server.Name}");
            }
        }
    }
}
