using Flow.Launcher.Plugin.SharedCommands;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading;
using Microsoft.Win32;
using System.IO;
using System;
using System.Configuration.Internal;

namespace Flow.Launcher.Plugin.UnityLauncherPlugin
{
    public class UnityLauncherPlugin : IAsyncPlugin
    {
        private PluginInitContext _context;

        private List<ProjectInfoModel>     allProjectsList       = new List<ProjectInfoModel>();
        private List<ProjectInfoModel>     projectsMatchingQuery = new List<ProjectInfoModel>();
        private List<UnityEditorIntoModel> allEditoresList       = new List<UnityEditorIntoModel>();

        private string unityHubInstallPath = "";
        private string quote = "\""; // File path must be in quotes if it contains a space. 

        public async Task InitAsync(PluginInitContext context)
        {
            _context = context;

            await LoadHubAmdProjectData();
        }

        public async Task<List<Result>> QueryAsync (Query query, CancellationToken cancellationToken)
        {
            return await Task.Run(() =>
            {
                string userQuery = query.Search.ToString().Trim();

                projectsMatchingQuery = GetProjectsMatchingQuery(userQuery);

                List<Result> results = CreateListOfResults(projectsMatchingQuery);

                return results;
            }, cancellationToken );
        }

        public async Task LoadHubAmdProjectData ()
        {
            // Get Unity Hub's location
            unityHubInstallPath = GetUnityHubLocation();

            // Get location of all installed editor versions
            FindInstalledEditors();

            // Find all projects listed in Unity Hub
            FindUnityProjects();
        }

        #region Unity Hub
        private string GetUnityHubLocation()
        {
            RegistryKey unityHubKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Unity Technologies\Hub", false);
            string keyValue = (string)unityHubKey.GetValue("InstallLocation");

            if (keyValue != null)
            {
                keyValue = keyValue.Trim() + @"\Unity Hub.exe";
            }
            else
            {
                Console.WriteLine("[WARN] GetUnityHubLocation | Unity Hub's install location not foudn! Using default");
                keyValue = @"C:\Program Files\Unity Hub\Unity Hub.exe";
            }

            return keyValue;
        }

        // Use Unity Hub's command line arguments to list all installed versions
        // https://docs.unity3d.com/hub/manual/HubCLI.html
        private void FindInstalledEditors()
        {
            Console.WriteLine("Looking for installed Unity versions, and projects...");

            // Command prompt path
            string cmdPath = Environment.GetEnvironmentVariable("systemroot") + @"\system32\cmd.exe";
            // Unity hub path + arguments to get list of installed version
            string arguments = "/c " + quote + unityHubInstallPath + quote + " -- --headless editors -installed";

            // Unity hub's response
            List<string> hubResponseList = GetCommandOutput(cmdPath, arguments);

            if (hubResponseList[0].Contains("[ERROR]"))
            {
                string errorMessage = "";

                foreach (string hubResponse in hubResponseList)
                {
                    errorMessage += hubResponse + "\n";
                }

                _context.API.ShowMsgError(errorMessage);
            }

            // Split output into version and path. 
            string splitLineWString = " , installed at ";
            string[] chosenLine;

            // Add each editor version into allEditoresList
            for (int i = 0; i < hubResponseList.Count; i++)
            {
                // Skip line if it's empty 
                if (hubResponseList[i] == "" || hubResponseList[i] == null)
                {
                    continue;
                }

                // Split and format each line of output
                chosenLine = hubResponseList[i].Split(splitLineWString, StringSplitOptions.RemoveEmptyEntries);

                UnityEditorIntoModel unityEditorIntoModel = new UnityEditorIntoModel()
                {
                    version = chosenLine[0],
                    path = chosenLine[1]
                };

                allEditoresList.Add(unityEditorIntoModel);
            }
        }

        private List<string> GetCommandOutput(string programPath, string arguments)
        {
            Process proc = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = programPath,
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            List<string> commandOutput = new List<string>();

            proc.Start();

            while (!proc.StandardOutput.EndOfStream)
            {
                commandOutput.Add(proc.StandardOutput.ReadLine().Trim());
            }

            proc.WaitForExit();

            if (proc.ExitCode != 0)
            {
                commandOutput.Add("[ERROR] GetCommandOutput | Process exited with an error!");
                commandOutput.Add("Process: " + proc.ProcessName + " ID: " + proc.Id);
                commandOutput.Add("Exit code: " + proc.ExitCode.ToString().Trim());
                commandOutput.Add("Details: " + proc.StandardError.ToString().Trim());
            }

            return commandOutput;
        }
        #endregion

        #region Unity Editor
        // Use Unity's command line arguments to open project in specified version
        // https://docs.unity3d.com/6000.0/Documentation/Manual/EditorCommandLineArguments.html
        private void OpenProjectIneditor(string editorPath, string projectPath)
        {
            Process editorProcess = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = editorPath, // Unity version path
                    Arguments = "-projectPath " + quote + projectPath + quote, // Project path
                    UseShellExecute = false
                }
            };
            editorProcess.Start();
        }

        private bool DoesEditorVersionExist(string editorVersion)
        {
            foreach (UnityEditorIntoModel editor in allEditoresList)
            {
                if (editor.version == editorVersion)
                {
                    return true;
                }
            }
            return false;
        }

        private string GetMatchingEditorVersionForProject(string projectVersion)
        {
            foreach (UnityEditorIntoModel editor in allEditoresList)
            {
                if (editor.version == projectVersion)
                {
                    return editor.path;
                }
            }
            // Return path for the first editor on the list if no matching version is found
            Console.WriteLine("[WARN] GetMatchingEditorVersionForProject | Version was not found");
            return allEditoresList[0].path;
        }
        #endregion

        #region Unity projects
        private void FindUnityProjects()
        {
            string unityHubDataPath = Environment.GetEnvironmentVariable("appdata") + "/" + "UnityHub";

            string projectsFileName = "projects-v1.json";
            string projectsDataFile = File.ReadAllText(unityHubDataPath + "/" + projectsFileName);

            string replaceBrackets = projectsDataFile.Replace("{", "&&")
                                                     .Replace("}", "&&");

            string[] splitFileIntoLines = replaceBrackets.Split(new string[] { "&&" }, StringSplitOptions.RemoveEmptyEntries);

            string projectTitleKeyword = "\"title\":";

            foreach (string line in splitFileIntoLines)
            {
                if (line.Contains(projectTitleKeyword))
                {
                    allProjectsList.Add(ParseProjectDetails(line));
                }
            }

            projectsMatchingQuery = allProjectsList;
        }

        private ProjectInfoModel ParseProjectDetails(string projectDetailsString)
        {
            string removeQuotes = projectDetailsString.Replace("\"", "");
            string[] breakIntoLines = removeQuotes.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);

            string projectName = "";
            string projectParh = "";
            string editorVersion = "";
            string isFavourite = "";
            string lastModified = "";
            bool editorVersionInstalled = true;

            foreach (string line in breakIntoLines)
            {
                string[] currentLineSplit = line.Split(':');

                switch (currentLineSplit[0])
                {
                    case "title":
                        projectName = currentLineSplit[1];
                        break;

                    case "path":
                        projectParh = currentLineSplit[1] + ":" + currentLineSplit[2].Replace(@"\\", "/");
                        break;

                    case "version":
                        editorVersion = currentLineSplit[1];
                        break;

                    case "isFavorite":
                        isFavourite = currentLineSplit[1];
                        break;

                    case "lastModified":
                        lastModified = ConvertUnxTimeToHuman(currentLineSplit[1]);
                        break;
                }
            }

            editorVersionInstalled = DoesEditorVersionExist(editorVersion);

            ProjectInfoModel projInfo = new ProjectInfoModel
            {
                title = projectName,
                path = projectParh,
                editorVersion = editorVersion,
                isFavourite = isFavourite,
                lastModified = lastModified,
                editorVeInstalled = editorVersionInstalled
            };
            return projInfo;
        }
        #endregion

        #region Other
        private string ConvertUnxTimeToHuman (string timestamp)
        {
            long stringToLong = long.Parse(timestamp.Trim());

            DateTimeOffset humanTime = DateTimeOffset.FromUnixTimeMilliseconds(stringToLong).ToLocalTime();

            string[] dateAndTime = humanTime.ToString().Split(" ");

            string humanTimeString = dateAndTime[0] + " " + dateAndTime[1];

            return humanTimeString;
        }

        private List<ProjectInfoModel> GetProjectsMatchingQuery(string query)
        {
            List<ProjectInfoModel> projectsMAtchingquery = new List<ProjectInfoModel>();

            foreach (ProjectInfoModel project in allProjectsList)
            {
                if (project.title.ToLower().Contains(query.ToLower()))
                {
                    projectsMAtchingquery.Add(project);
                }
            }

            return projectsMAtchingquery;
        }

        private List<Result> CreateListOfResults (List<ProjectInfoModel> projects)
        {
            List<Result> results = new List<Result>();
            string projectDetails = "";

            foreach (ProjectInfoModel project in projects)
            {
                string editorVersion = project.editorVersion; 
                bool   editorVersionIsInstalled = DoesEditorVersionExist(project.editorVersion);

                string isProjectStarred = project.isFavourite;
                string resultIconPath = @"Images\logo.png";
                string detailSplitter = "  |  ";

                // Project is starred
                if (isProjectStarred.ToLower() == "true")
                {
                    resultIconPath = @"Images\fav.png";
                }

                // Editor version is installed
                if (editorVersionIsInstalled)
                {
                    projectDetails = project.path + detailSplitter + "Modified: " + project.lastModified + detailSplitter + "Version: " + editorVersion;
                }
                // Editor version is missing
                else
                {
                    resultIconPath = @"Images\warning.png";
                    projectDetails = "Missing version " + editorVersion + "!" + detailSplitter + project.path + 
                                      detailSplitter + "Modified: " + project.lastModified;
                }

                // Editor version is NOT installed
                if (!editorVersionIsInstalled)
                {

                    results.Add(new Result
                    {
                        Title    = project.title,
                        SubTitle = projectDetails,
                        IcoPath  = resultIconPath,
                        Action   = _ =>
                        {
                            _context.API.ShowMsgError("Version was not found!", 
                                                      "Please install Unity " + editorVersion + " to open this project " +
                                                      "or use Uity Hub to open the project in a different version");
                            return true;
                        }
                    });
                }
                // Editor version is installed
                else
                {
                    results.Add(new Result
                    {
                        Title    = project.title,
                        SubTitle = projectDetails,
                        IcoPath  = resultIconPath,
                        Action   = _ =>
                        {
                            string editorPath = GetMatchingEditorVersionForProject(project.editorVersion);
                            OpenProjectIneditor(editorPath, project.path);

                            return true;
                        }
                    });
                }
            }
            return results;
        }
        #endregion
    }

    public class UnityEditorIntoModel
    {
        public string version = "";
        public string path = "";
    }

    public class ProjectInfoModel
    {
        public string title = "";
        public string path = "";
        public string editorVersion = "";
        public string isFavourite = "";
        public string lastModified = "";
        public bool   editorVeInstalled = true;
    }
}