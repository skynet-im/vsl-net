using McMaster.Extensions.CommandLineUtils;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using VSL.Common;

namespace VSL.Cli.Commands
{
    [Command("serve", Description = "Runs a VSL test server on port 32761")]
    [HelpOption]
    internal class ServeCommand
    {
        [Option(Description = "Listen not only on localhost")]
        public bool Global { get; set; }

        private void OnExecute()
        {
            IPEndPoint[] endPoints = {
                new IPEndPoint(Global ? IPAddress.Any : IPAddress.Loopback, 32761),
                new IPEndPoint(Global ? IPAddress.IPv6Any : IPAddress.IPv6Loopback, 32761)
            };

            SocketSettings settings = new SocketSettings
            {
                RsaXmlKey = Library.Keypair
            };

            VSLListener listener = new VSLListener(endPoints, settings, () => new RemoteClient());
            listener.Start();

            Console.WriteLine("VSL test server running. Press 'q' to exit...");
            while (Console.ReadKey(true).KeyChar != 'q') ;
            listener.Stop();
        }
    }
}
