using System;

namespace Flow.Launcher.Plugin.UnityLauncherPlugin
{
    public class Tools
    {
        public string ConvertUnxTimeToHuman(string timestamp)
        {
            //long stringToLong = long.Parse(timestamp?.Trim());
            long stringToLong    = new long();
            long convertedString = new long();

            if (long.TryParse(timestamp.Trim(), out convertedString))
            {
                stringToLong = convertedString;
            }
            else
            {
                return "UNDEFINED";
            }

            DateTimeOffset humanTime = DateTimeOffset.FromUnixTimeMilliseconds(stringToLong).ToLocalTime();

            string[] dateAndTime = { "", "" };
            dateAndTime = humanTime.ToString()?.Split(" ");

            string humanTimeString = dateAndTime[0] + " " + dateAndTime[1];

            return humanTimeString;
        }
    } // main class end
} // namespace end
