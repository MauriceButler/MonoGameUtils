using System;
using System.Reflection;
using System.Threading.Tasks;
using CliWrap;

namespace MonoGameUtils
{
    class Program
    {
        static async Task Main(string[] args)
        {
            if (args.Length == 0 || args[0] == "-h" || args[0] == "--help")
            {
                ShowUsage();
            }

            var verb = args[0];

            switch (verb)
            {
                case "create":
                    await Create.CreateSolution(args);
                    break;
                case "package":
                    await Package.PackageGame(args);
                    break;
                default:
                    ShowUsage();
                    break;
            }
        }

        public static void ShowUsage()
        {
            var versionString = Assembly.GetEntryAssembly()
                                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                                .InformationalVersion
                                .ToString();

            Console.WriteLine($"MonoGame Helpers v{versionString}");
            Console.WriteLine("------------------------\n");
            Console.WriteLine("Usage:");
            Console.WriteLine("mgh <command> [<args>]\n");
            Console.WriteLine("CREATE:");
            Console.WriteLine("Create a new solution with a shared project and platform projects:\n");
            Console.WriteLine("\tmgh create <gameName> [--desktopgl (default)] [--android] [--ios]\n");
            Console.WriteLine("PACKAGE:");
            Console.WriteLine("Package game for each platform:\n");
            Console.WriteLine("\tmgh package [--all (default)] [--windows] [--linux] [--mac] [--android] [--ios]\n");
            Environment.Exit(-1);
        }

        public static async Task RunCommands(Command[] commands)
        {
            foreach (var command in commands)
            {
                await (command | Console.WriteLine).ExecuteAsync();
            }
        }
    }
}
