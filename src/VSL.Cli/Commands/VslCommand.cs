using McMaster.Extensions.CommandLineUtils;
using System;
using System.Collections.Generic;
using System.Text;

namespace VSL.Cli.Commands
{
    [Command("vsl", Description = "VSL Debugging and Test console")]
    [Subcommand(typeof(PentestCommand))]
    internal class VslCommand
    {
        private int OnExecute(CommandLineApplication app)
        {
            app.ShowHelp();
            return 1;
        }
    }
}
