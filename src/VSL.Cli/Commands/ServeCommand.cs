using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using VSL.Common;

namespace VSL.Cli.Commands
{
    [Command("serve", Description = "Runs a VSL test server")]
    [HelpOption]
    internal class ServeCommand
    {
        [Option(Description = "Listen not only on localhost")]
        public bool Global { get; set; }

        [Option(Description = "TCP/IP port to listen on")]
        public ushort Port { get; set; } = 32761;

        [Option("-o|--oldest", Description = "Oldest supported product version")]
        public ushort OldestProductVersion { get; set; }

        [Option("-l|--latest", Description = "Latest supported product version")]
        public ushort LatestProductVersion { get; set; }

        private void OnExecute()
        {
            IPEndPoint[] endPoints = {
                new IPEndPoint(Global ? IPAddress.Any : IPAddress.Loopback, Port),
                new IPEndPoint(Global ? IPAddress.IPv6Any : IPAddress.IPv6Loopback, Port)
            };

            SocketSettings settings = new SocketSettings
            {
                RsaXmlKey = Library.Keypair,
                OldestProductVersion = OldestProductVersion,
                LatestProductVersion = LatestProductVersion
            };

            ILoggerFactory loggerFactory = new LoggerFactory().AddConsole(LogLevel.Debug);

            VSLListener listener = new VSLListener(endPoints, settings, () => new RemoteClient(loggerFactory.CreateLogger<RemoteClient>()));
            listener.Start();

            Console.WriteLine("VSL test server running. Press 'q' to exit...");
            while (Console.ReadKey(true).KeyChar != 'q') ;
            listener.Stop();
        }
    }
}
