using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Win32;
using System;

namespace Flow.Launcher.Plugin.UnityLauncherPlugin
{
    public class Hub
    {
        public string unityHubInstallPath = "";

        public string GetUnityHubLocation()
        {
            RegistryKey unityHubKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Unity Technologies\Hub", false);
            string keyValue = (string)unityHubKey.GetValue("InstallLocation");

            if (keyValue != null)
            {
                keyValue = keyValue.Trim() + @"\Unity Hub.exe";
            }
            else
            {
                Console.WriteLine("[WARN] GetUnityHubLocation | Unity Hub's install location not found! Using default locartion.");
                keyValue = @"C:\Program Files\Unity Hub\Unity Hub.exe";
            }

            return keyValue;
        }

        // Use Unity Hub's command line arguments to list all installed versions
        // https://docs.unity3d.com/hub/manual/HubCLI.html
        public void FindInstalledEditors()
        {
            Console.WriteLine("Looking for installed Unity versions, and projects...");

            // Command prompt path
            string cmdPath = Environment.GetEnvironmentVariable("systemroot") + @"\system32\cmd.exe";
            // Unity hub path + arguments to get list of installed version
            string arguments = "/c " + Parts.quote + unityHubInstallPath + Parts.quote + " -- --headless editors -installed";

            // Unity hub's response
            List<string> hubResponseList = GetCommandOutput(cmdPath, arguments);

            if (hubResponseList[0].Contains("[ERROR]"))
            {
                string errorMessage = "";

                foreach (string hubResponse in hubResponseList)
                {
                    errorMessage += hubResponse + "\n";
                }
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

                UnityEditorInfoModel unityEditorIntoModel = new()
                {
                    version = chosenLine[0],
                    path = chosenLine[1]
                };

                Parts.editor.allEditoresList.Add(unityEditorIntoModel);
            }
        }

        public List<string> GetCommandOutput(string programPath, string arguments)
        {
            Process proc = new()
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

            List<string> commandOutput = new();

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
    }
}
