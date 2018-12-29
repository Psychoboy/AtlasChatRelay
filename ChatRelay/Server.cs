using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Newtonsoft.Json;
using Rcon;

namespace ChatRelay
{
    public class Server
    {
        public event EventHandler<String> MessageReceived;

        private RconClient _client = new Rcon.RconClient();
        private Timer _timer;


        public string Name { get; set; }
        public string Host { get; set; }
        public ushort Port { get; set; }
        public string Password { get; set; }

        public Server(string name, string host, ushort port, string password)
        {
            _client.ConnectionFailed += _client_ConnectionFailed;
            _client.Disconnected += _client_Disconnected;
            _client.Connected += _client_Connected;
            _client.Connecting += _client_Connecting;
            _timer = new Timer(1000);
            _timer.Elapsed += _timer_Elapsed;
            _timer.Enabled = true;
            _timer.AutoReset = true;
            Name = name;
            Host = host;
            Port = port;
            Password = password;
        }

        private void _client_Connecting(object sender, EventArgs e)
        {
            Console.WriteLine($"{Name} connecting.");
        }

        private void _client_Connected(object sender, EventArgs e)
        {
            Console.WriteLine($"{Name} connected.");
        }

        private void _client_Disconnected(object sender, bool e)
        {
            Connect();
        }

        private void _client_ConnectionFailed(object sender, string e)
        {
            Connect();
        }

        private void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _timer.Stop();
            try
            {
                if (!_client.IsConnected)
                {
                    Connect();
                }
                else
                {
                    _client.ExecuteCommandAsync(new Rcon.Commands.GetChat(), (ev, args) => ProcessChat(args));
                }
            }
            catch (Exception)
            {

            }
            _timer.Start();
        }

        private void ProcessChat(CommandExecutedEventArgs args)
        {
            if (args.Response.StartsWith("Server received") || args.Response.StartsWith("(") || args.Response.StartsWith("SERVER:") || args.Response.Trim().Length == 0)
            {
                //Do Nothing
                //Console.WriteLine($"{Name} Nothing to send.");
            }
            else
            {
                MessageReceived?.Invoke(this, $"({Name}) {args.Response}");
            }
        }

        public void SendChat(string message)
        {
            _client.ExecuteCommandAsync(new Rcon.Commands.ServerChat(message), (ev, args) => { Console.WriteLine($"{Name}: {((Rcon.Commands.ServerChat)args.Command).Message}"); });
        }

        public void SendCustomMessage(string message)
        {
            _client.ExecuteCommandAsync(new Rcon.Commands.CustomCommand(message), (ev, args) => { Console.WriteLine($"{Name}: Response: {args.Response}"); });
        }

        public void Connect()
        {
            try
            {
                _client.Connect(Host, Port, Password);
            }
            catch (Exception) { }
            if (!_timer.Enabled)
            {
                _timer.Start();
            }
        }
    }
}
