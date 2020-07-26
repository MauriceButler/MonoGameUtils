using CliWrap;
using System.IO;
using System.Threading.Tasks;
using System.Xml;

namespace MonoGameUtils
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

            for (var i = 2; i < args.Length; i++)
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

            Command dotnetCLI = Cli.Wrap("dotnet").WithWorkingDirectory(gameName);

            // Install / Update templates
            Command installTemplates = dotnetCLI.WithArguments("new --install MonoGame.Templates.CSharp");

            // Now that we have our directory, let us create our game library project which will possess all the code
            Command createSolution = dotnetCLI.WithArguments("new sln");
            Command createStandardLibrary = dotnetCLI.WithArguments($"new mgnetstandard -n {gameName}");
            Command addToSolution = dotnetCLI.WithArguments($"sln add {gameName}/{gameName}.csproj");

            Command[] commands = new[] {
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
            Command dotnetCLI = Cli.Wrap("dotnet").WithWorkingDirectory(gameName);

            // Create platform project and add to solution
            Command createProject = dotnetCLI.WithArguments($"new {template} -n {templateGameName}");
            Command addToSolution = dotnetCLI.WithArguments($"sln add {templateGameName}/{templateGameName}.csproj");

            // Now that we have created our platform project, we need to do a few tweaks to it.
            // First reference our game library from it
            Command foo = Cli.Wrap("pwd").WithWorkingDirectory(fullPath);
            Command addReferenceToSharedProject = Cli.Wrap("dotnet").WithWorkingDirectory(fullPath).WithArguments($"add {templateGameName}.csproj reference ../{gameName}/{gameName}.csproj");

            Command[] commands = new[] {
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

            XmlNode contentReference = projectFile.GetElementsByTagName("MonoGameContentReference")[0];
            contentReference.Attributes["Include"].Value = @$"..\{gameName}\Content\Content.mgcb";

            XmlElement link = projectFile.CreateElement("Link");
            link.InnerText = @"Content\Content.mgcb";

            contentReference.PrependChild(link);
            projectFile.Save($"{fullPath}/{templateGameName}.csproj");
        }
    }
}
