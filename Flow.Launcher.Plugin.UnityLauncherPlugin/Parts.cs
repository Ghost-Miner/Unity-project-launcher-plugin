namespace Flow.Launcher.Plugin.UnityLauncherPlugin
{
    public static class Parts
    {
        public static string quote = "\""; // File path must be in quotes if it contains a space.

        public static Hub      hub      = new Hub();
        public static Tools    tools    = new Tools();
        public static Editor   editor   = new Editor();
        public static Projects projects = new Projects();
    }
}
