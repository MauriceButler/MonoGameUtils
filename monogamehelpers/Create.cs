using System.IO;
using System.Threading.Tasks;
using System.Xml;
using CliWrap;

namespace monogamehelpers
{
    public static class Create
    {
        // Automation of the steps laid out here
        // https://github.com/harry-cpp/MonoGame/blob/newprojdocs/Documentation/setting_up_project/setting_up_project_vscode.md

        public static async Task CreateSolution(string[] args)
        {
            var gameName = args[1];

            await CreateSolution(gameName);

            if (args.Length == 2)
            {
                // Default to creating a desktopGL project
                await CreateProjectTemplate(gameName, "DesktopGL", "mgdesktopgl");
                return;
            }

            for (int i = 2; i < args.Length; i++)
            {
                var arg = args[i];

                // Make a project for each platform we want our game to run on.
                switch (arg)
                {
                    case "--desktopgl":
                        await CreateProjectTemplate(gameName, "DesktopGL", "mgdesktopgl");
                        break;
                    case "--android":
                        await CreateProjectTemplate(gameName, "Android", "mgandroid");
                        break;
                    case "--ios":
                        await CreateProjectTemplate(gameName, "iOS", "mgios");
                        break;
                    default:
                        Program.ShowUsage();
                        break;
                }
            }

        }

        private static async Task CreateSolution(string gameName)
        {
            Directory.CreateDirectory(gameName);

            var dotnetCLI = Cli.Wrap("dotnet").WithWorkingDirectory(gameName);

            // Install / Update templates
            var installTemplates = dotnetCLI.WithArguments("new --install MonoGame.Templates.CSharp");

            // Now that we have our directory, let us create our game library project which will possess all the code
            var createSolution = dotnetCLI.WithArguments("new sln");
            var createStandardLibrary = dotnetCLI.WithArguments($"new mgnetstandard -n {gameName}");
            var addToSolution = dotnetCLI.WithArguments($"sln add {gameName}/{gameName}.csproj");

            var commands = new[] {
                installTemplates,
                createSolution,
                createStandardLibrary,
                addToSolution
            };

            await Program.RunCommands(commands);
        }

        private static async Task CreateProjectTemplate(string gameName, string suffix, string template)
        {
            var templateGameName = $"{gameName}.{suffix}"; ;
            var fullPath = $"{gameName}/{templateGameName}";
            var dotnetCLI = Cli.Wrap("dotnet").WithWorkingDirectory(gameName);

            // Create platform project and add to solution
            var createProject = dotnetCLI.WithArguments($"new {template} -n {templateGameName}");
            var addToSolution = dotnetCLI.WithArguments($"sln add {templateGameName}/{templateGameName}.csproj");

            // Now that we have created our platform project, we need to do a few tweaks to it.
            // First reference our game library from it
            var foo = Cli.Wrap("pwd").WithWorkingDirectory(fullPath);
            var addReferenceToSharedProject = Cli.Wrap("dotnet").WithWorkingDirectory(fullPath).WithArguments($"add {templateGameName}.csproj reference ../{gameName}/{gameName}.csproj");

            var commands = new[] {
                createProject,
                addToSolution,
                foo,
                addReferenceToSharedProject,
            };

            await Program.RunCommands(commands);

            // Next, delete the generated Game1.cs and Content as they both exist in our library project:
            File.Delete($"{fullPath}/Game1.cs");
            Directory.Delete($"{fullPath}/Content", true);

            // And finally, we need to fix the link to our content project by replacing Content\Content.mgcb with ..\$gameName\Content\Content.mgcb
            var projectFile = new XmlDocument();
            projectFile.Load($"{fullPath}/{templateGameName}.csproj");

            var contentReference = projectFile.GetElementsByTagName("MonoGameContentReference")[0];
            contentReference.Attributes["Include"].Value = @$"..\{gameName}\Content\Content.mgcb";

            var link = projectFile.CreateElement("Link");
            link.InnerText = @"Content\Content.mgcb";

            contentReference.PrependChild(link);
            projectFile.Save($"{fullPath}/{templateGameName}.csproj");
        }
    }
}
