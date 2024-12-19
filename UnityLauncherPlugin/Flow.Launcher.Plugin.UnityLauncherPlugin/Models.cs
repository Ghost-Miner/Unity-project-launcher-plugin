namespace Flow.Launcher.Plugin.UnityLauncherPlugin
{
    //internal class Models { }
    public class UnityEditorInfoModel
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
