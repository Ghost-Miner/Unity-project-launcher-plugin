using System;

namespace Flow.Launcher.Plugin.UnityLauncherPlugin
{
    public class Tools
    {
        public string ConvertUnxTimeToHuman(string timestamp)
        {
            long stringToLong = long.Parse(timestamp.Trim());

            DateTimeOffset humanTime = DateTimeOffset.FromUnixTimeMilliseconds(stringToLong).ToLocalTime();

            string[] dateAndTime = humanTime.ToString().Split(" ");

            string humanTimeString = dateAndTime[0] + " " + dateAndTime[1];

            return humanTimeString;
        }
    } // main class end
} // namespace end
