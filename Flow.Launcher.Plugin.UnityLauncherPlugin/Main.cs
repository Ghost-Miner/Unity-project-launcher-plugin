using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

namespace Flow.Launcher.Plugin.UnityLauncherPlugin
{
    public class UnityLauncherPlugin : IAsyncPlugin
    {
        private PluginInitContext _context;

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

                if (userQuery.ToLower() == "/dc")
                {
                    Parts.editor.DeleteCacheFile();
                }    

                Parts.projects.projectsMatchingQuery = Parts.projects.GetProjectsMatchingQuery(userQuery);

                List<Result> results = CreateListOfResults(Parts.projects.projectsMatchingQuery);

                return results; 
            }, cancellationToken );
        }

        public async Task LoadHubAmdProjectData ()
        {
            // Ensure the plugin data folder exists, otherwise we crash.
            Parts.editor.CreatePluginDataFolder();

            // Get Unity Hub's location
            Parts.hub.unityHubInstallPath = Parts.hub.GetUnityHubLocation();

            bool versionCacheExists = Parts.editor.VersionCacheFileExists();

            // Get location of all installed editor versions
            switch (versionCacheExists)
            {   
                case false:
                    Parts.hub.FindInstalledEditors();
                    Parts.editor.SaveUnityVersopns();
                    break;

                case true:
                    Parts.editor.LoadUnityVersions();
                    break;
            }

            // Find all projects listed in Unity Hub
            Parts.projects.FindUnityProjects();
        }

        private List<Result> CreateListOfResults (List<ProjectInfoModel> projects)
        {
            List<Result> results = new();
            string projectDetails = "";

            foreach (ProjectInfoModel project in projects)
            {
                string editorVersion = project.editorVersion; 
                bool   editorVersionIsInstalled = Parts.editor.DoesEditorVersionExist(project.editorVersion);

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
                            string editorPath = Parts.editor.GetMatchingEditorVersionForProject(project.editorVersion);
                            Parts.editor.OpenProjectIneditor(editorPath, project.path);

                            return true;
                        }
                    });
                }
            }

            // Replace list of results with "No results" message
            if (projects.Count < 1)
            {
                results = NoResultsMessage();
            }

            return results;
        }

        private static List<Result> NoResultsMessage ()
        {
            List<Result> results = new()
            {
                new Result
                {
                    Title = "No results",
                    SubTitle = "",
                    IcoPath = @"Images\logo.png",
                    Action = _ =>
                    {
                        return true;
                    }
                }
            };
            return results;
        }
    } // main class end
} // namespace end