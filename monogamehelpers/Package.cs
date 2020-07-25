using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CliWrap;
using CliWrap.Buffered;

namespace MonoGameUtils
{
    public static class Package
    {
        // Automation of the steps laid out here
        // https://docs.monogame.net/articles/packaging_games.html

        private static readonly string[] knownSuffixes = { "DesktopGL", "Android", "iOS" };

        public static async Task PackageGame(string[] args)
        {
            var currentDirectory = Directory.GetCurrentDirectory();
            var directoryInfo = new DirectoryInfo(currentDirectory);
            var gameName = directoryInfo.Name;

            var directories = directoryInfo.GetDirectories();
            var directoryNames = directories.Select((directoryInfo) => directoryInfo.Name);
            var expectedNames = knownSuffixes.Select((suffix) => $"{gameName}.{suffix}");
            var detectedTargets = directoryNames.Intersect(expectedNames).ToList();

            // Bail if not in a known directory
            if (detectedTargets.Count() == 0)
            {
                Console.WriteLine("Expected directory structure not found.");
                Environment.Exit(-2);
            }


            if (args.Length == 1 || args[1] == "--all")
            {
                // Default to packaging whatever it can find.
                foreach (var target in detectedTargets)
                {
                    var suffix = target.Split(".").Last().ToLower();
                    await PackageTarget($"--{suffix}", gameName);
                }

                return;
            }

            for (int i = 1; i < args.Length; i++)
            {
                await PackageTarget(args[i], gameName);
            }
        }

        private static async Task PackageTarget(string target, string gameName)
        {
            switch (target)
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
                    await Task.Run(Program.ShowUsage);
                    break;
            }
        }

        private static async Task PackageWindows(string gameName)
        {
            // TODO: Package win
            Console.WriteLine("Package win");

            var dotnetCLI = Cli.Wrap("dotnet").WithWorkingDirectory($"{gameName}.DesktopGL");
            var publish = dotnetCLI.WithArguments("publish -c Release -r win-x64 /p:PublishReadyToRun=false /p:TieredCompilation=false --self-contained");
            var commands = new[] {
                publish
            };

            await Program.RunCommands(commands);
            // TODO: Create zip / self extracting file
        }

        private static async Task PackageLinux(string gameName)
        {
            Console.WriteLine("Package Linux");

            var dotnetCLI = Cli.Wrap("dotnet").WithWorkingDirectory($"{gameName}.DesktopGL");
            var publish = dotnetCLI.WithArguments("publish -c Release -r linux-x64 /p:PublishReadyToRun=false /p:TieredCompilation=false --self-contained");
            var commands = new[] {
                publish
            };

            await Program.RunCommands(commands);
            // TODO: Create tar.gz with filepermisions intact
        }

        private static async Task PackageMac(string gameName)
        {
            Console.WriteLine("Package mac");

            var dotnetCLI = Cli.Wrap("dotnet").WithWorkingDirectory($"{gameName}.DesktopGL");
            var publish = dotnetCLI.WithArguments("publish -c Release -r osx-x64 /p:PublishReadyToRun=false /p:TieredCompilation=false --self-contained");
            var result = await (publish | Console.WriteLine).ExecuteBufferedAsync();

            var publishPath = result.StandardOutput.Substring(result.StandardOutput.LastIndexOf(' ')).Trim();

            var tempPath = Path.GetFullPath(Path.Combine(publishPath, "..", "temp"));
            var appRootPublishPath = Path.GetFullPath(Path.Combine(publishPath, $"{gameName}.app"));
            var macOSPath = Path.Combine(tempPath, "Contents", "MacOS");
            var executingPath = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);


            if (Directory.Exists(appRootPublishPath))
            {
                Directory.Delete(appRootPublishPath, true);
            }

            if (Directory.Exists(tempPath))
            {
                Directory.Delete(tempPath, true);
            }

            // Mac folder
            Directory.CreateDirectory(Directory.GetParent(macOSPath).FullName);
            Directory.Move(publishPath, macOSPath);

            // plist
            var text = File.ReadAllText(Path.Combine(executingPath, "PlistTemplate.xml"));
            text = text.Replace("{gameName}", gameName);
            File.WriteAllText(Path.Combine(Path.Combine(tempPath, "Contents"), "Info.plist"), text);

            // Create Content
            Directory.CreateDirectory(Path.Combine(tempPath, "Contents", "Resources", "Content"));

            // TODO: convert bmp / ico to icns

            // Move to app folder
            Directory.CreateDirectory(publishPath);
            Directory.Move(tempPath, appRootPublishPath);
        }

        private static async Task PackageAndroid(string gameName)
        {
            // TODO: Package android
            Console.WriteLine("Package android");

        }

        private static async Task PackageIOS(string gameName)
        {
            // TODO: Package ios
            Console.WriteLine("Package ios");

        }

    }
}
