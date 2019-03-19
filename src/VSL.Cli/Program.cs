using McMaster.Extensions.CommandLineUtils;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using VSL.Cli.Commands;

namespace VSL.Cli
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            if (Debugger.IsAttached)
            {
                Console.Write("VSL is running in debug mode. Please enter your command: ");
                args = Console.ReadLine().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                Console.WriteLine();
                int result = await CommandLineApplication.ExecuteAsync<VslCommand>(args);
                Console.WriteLine();
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey(true);
                return result;
            }
            else return await CommandLineApplication.ExecuteAsync<VslCommand>(args);
        }
    }
}
