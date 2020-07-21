using System.Threading.Tasks;
using CliWrap;

namespace monogamehelpers
{
    public static class Package
    {
        // Automation of the steps laid out here
        // https://docs.monogame.net/articles/packaging_games.html

        public static async Task PackageGame(string[] args)
        {
            // TODO: get current directory name
            var gameName = "";

            // TODO: bail if not in a solution directory
            if (args.Length == 2 || args[1] == "--all")
            {
                // Default to packaging whatever it can find.
                // TODO: find platforms based on project names
                // TODO: package each one
                return;
            }

            for (int i = 2; i < args.Length; i++)
            {
                var arg = args[i];

                switch (arg)
                {
                    case "--desktopgl":
                        await PackageWindows(gameName);
                        await PackageLinux(gameName);
                        await PackageMac(gameName);
                        break;
                    case "--windows":
                        await PackageWindows(gameName);
                        break;
                    case "--linux":
                        await PackageLinux(gameName);
                        break;
                    case "--mac":
                        await PackageMac(gameName);
                        break;
                    case "--android":
                        await PackageAndroid(gameName);
                        break;
                    case "--ios":
                        await PackageIOS(gameName);
                        break;
                    default:
                        Program.ShowUsage();
                        break;
                }
            }
        }

        private static async Task PackageWindows(string gameName)
        {
            // TODO: Package win

            var dotnetCLI = Cli.Wrap("dotnet").WithWorkingDirectory($"{gameName}.DesktopGL");

            var publish = dotnetCLI.WithArguments("publish -c Release -r win-x64 /p:PublishReadyToRun=false /p:TieredCompilation=false --self-contained");

            var commands = new[] {
                publish
            };

            await Program.RunCommands(commands);

        }

        private static async Task PackageLinux(string gameName)
        {
            // TODO: Package linux

            var dotnetCLI = Cli.Wrap("dotnet").WithWorkingDirectory($"{gameName}.Android");

            var publish = dotnetCLI.WithArguments("publish -c Release -r linux-x64 /p:PublishReadyToRun=false /p:TieredCompilation=false --self-contained");

            var commands = new[] {
                publish
            };

            await Program.RunCommands(commands);

        }

        private static async Task PackageMac(string gameName)
        {
            // TODO: Package mac

            var dotnetCLI = Cli.Wrap("dotnet").WithWorkingDirectory($"{gameName}.iOS");

            var publish = dotnetCLI.WithArguments("publish -c Release -r osx-x64 /p:PublishReadyToRun=false /p:TieredCompilation=false --self-contained");

            var commands = new[] {
                publish
            };

            await Program.RunCommands(commands);

        }

        private static async Task PackageAndroid(string gameName)
        {
            // TODO: Package android

        }

        private static async Task PackageIOS(string gameName)
        {
            // TODO: Package ios

        }

    }
}
