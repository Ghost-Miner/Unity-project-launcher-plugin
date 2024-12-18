using System.Collections.Generic;
using System.Diagnostics;
using System;

namespace Flow.Launcher.Plugin.UnityLauncherPlugin
{
    public class Editor
    {
        public List<UnityEditorInfoModel> allEditoresList = new();

        // Use Unity's command line arguments to open project in specified version
        // https://docs.unity3d.com/6000.0/Documentation/Manual/EditorCommandLineArguments.html
        public void OpenProjectIneditor(string editorPath, string projectPath)
        {
            Process editorProcess = new()
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = editorPath, // Unity version path
                    Arguments = "-projectPath " + Parts.quote + projectPath + Parts.quote, // Project path
                    UseShellExecute = false
                }
            };
            editorProcess.Start();
        }

        public bool DoesEditorVersionExist(string editorVersion)
        {
            foreach (UnityEditorInfoModel editor in allEditoresList)
            {
                if (editor.version == editorVersion)
                {
                    return true;
                }
            }
            return false;
        }

        public string GetMatchingEditorVersionForProject(string projectVersion)
        {
            foreach (UnityEditorInfoModel editor in allEditoresList)
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

        public List<Result> CreateListOfInstalledEditors()
        {
            List<Result> results = new();
            string iconPath = @"Images\logo.png";

            foreach (UnityEditorInfoModel editor in allEditoresList)
            {
                results.Add(new Result
                {
                    Title = editor.version,
                    SubTitle = editor.path,
                    IcoPath = iconPath,
                });
            }

            return results;
        }
    }
}
