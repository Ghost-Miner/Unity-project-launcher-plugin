using System.Collections.Generic;
using System.IO;
using System;

namespace Flow.Launcher.Plugin.UnityLauncherPlugin
{
    public class Projects
    {
        public List<ProjectInfoModel> allProjectsList = new();
        public List<ProjectInfoModel> projectsMatchingQuery = new();

        public void FindUnityProjects()
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

        public ProjectInfoModel ParseProjectDetails(string projectDetailsString)
        {
            string removeQuotes = projectDetailsString.Replace("\"", "");
            string[] breakIntoLines = removeQuotes.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);

            string projectName = "";
            string projectParh = "";
            string editorVersion = "";
            string isFavourite = "";
            string lastModified = "";
            bool editorVersionInstalled;

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
                        lastModified = Parts.tools.ConvertUnxTimeToHuman(currentLineSplit[1]);
                        break;
                }
            }

            editorVersionInstalled = Parts.editor.DoesEditorVersionExist(editorVersion);

            ProjectInfoModel projInfo = new()
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

        public List<ProjectInfoModel> GetProjectsMatchingQuery(string query)
        {
            List<ProjectInfoModel> projectsMAtchingquery = new();

            foreach (ProjectInfoModel project in allProjectsList)
            {
                if (project.title.ToLower().Contains(query.ToLower()))
                {
                    projectsMAtchingquery.Add(project);
                }
            }
            return projectsMAtchingquery;
        }
    } // main class end
} // namespace end
