using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.IO;
using System;

namespace Flow.Launcher.Plugin.UnityLauncherPlugin
{
    public class Editor
    {
        public List<UnityEditorInfoModel> allEditoresList = new();

<<<<<<< HEAD:UnityLauncherPlugin/Flow.Launcher.Plugin.UnityLauncherPlugin/Editor.cs
        private string editorVersionsCacheFile = Environment.GetEnvironmentVariable("appdata") + @"\FlowLauncher\Plugins\UPL Data\editors.txt";
=======
        private string editorVerCacheFile = "editors.txt";
        private string pluginDataFolder   = Environment.GetEnvironmentVariable("appdata") +
                                                 @"\FlowLauncher\Plugins\UPL Data\";

        public void CreatePluginDataFolder ()
        {
            if (!Directory.Exists (pluginDataFolder))
            {
                Directory.CreateDirectory (pluginDataFolder);

                string readMeText = "This folder is used by the Unity project launcher plugin" +
                                    "\nhttps://github.com/Ghost-Miner/Unity-project-launcher-plugin";

                if (!File.Exists (pluginDataFolder + "READ ME.txt"))
                {
                    File.WriteAllText(pluginDataFolder + "READ ME.txt", readMeText);
                }
            }
        }
>>>>>>> main:Flow.Launcher.Plugin.UnityLauncherPlugin/Editor.cs

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

        public bool VersionCacheFileExists ()
        {
<<<<<<< HEAD:UnityLauncherPlugin/Flow.Launcher.Plugin.UnityLauncherPlugin/Editor.cs
            bool fileExists = File.Exists(editorVersionsCacheFile);
=======
            bool fileExists = File.Exists(pluginDataFolder + editorVerCacheFile);
>>>>>>> main:Flow.Launcher.Plugin.UnityLauncherPlugin/Editor.cs
            return fileExists;
        }

        public void DeleteCacheFile ()
        {
<<<<<<< HEAD:UnityLauncherPlugin/Flow.Launcher.Plugin.UnityLauncherPlugin/Editor.cs
            File.Delete(editorVersionsCacheFile);
=======
            File.Delete(pluginDataFolder + editorVerCacheFile);
>>>>>>> main:Flow.Launcher.Plugin.UnityLauncherPlugin/Editor.cs
        }

        public void SaveUnityVersopns()
        {
            List<string> versionsStr = new List<string>();

            foreach (UnityEditorInfoModel version in allEditoresList)
            {
<<<<<<< HEAD:UnityLauncherPlugin/Flow.Launcher.Plugin.UnityLauncherPlugin/Editor.cs
                versionsStr.Add(version.version.Trim() + ";" + version.path.Trim());
            }

            File.WriteAllLines(editorVersionsCacheFile, versionsStr.ToArray());
=======
                versionsStr.Add(version.version + ";" + version.path);
            }

            File.WriteAllLines(pluginDataFolder + editorVerCacheFile, versionsStr.ToArray());
>>>>>>> main:Flow.Launcher.Plugin.UnityLauncherPlugin/Editor.cs
        }

        public void LoadUnityVersions()
        {
<<<<<<< HEAD:UnityLauncherPlugin/Flow.Launcher.Plugin.UnityLauncherPlugin/Editor.cs
            string[] versionsFileContent = File.ReadLines(editorVersionsCacheFile).ToArray();
=======
            string[] versionsFileContent = File.ReadLines(pluginDataFolder + editorVerCacheFile).ToArray();
>>>>>>> main:Flow.Launcher.Plugin.UnityLauncherPlugin/Editor.cs

            List<UnityEditorInfoModel> unityEditorIntoModels = new();

            foreach (string version in versionsFileContent)
            {
                string[] parts = version.ToString().Split(';');

                unityEditorIntoModels.Add(new UnityEditorInfoModel
                {
                    version = parts[0],
                    path = parts[1],
                });
            }

            allEditoresList = unityEditorIntoModels;
        }
    } // main class end
} // namespace end
