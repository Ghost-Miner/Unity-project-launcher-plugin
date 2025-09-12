Flow.Launcher.Plugin.UnityLauncherPlugin
==================

**Launch Unity projects from [Flow launcher](https://www.flowlauncher.com/).** _(Inspired by [this plugin](https://github.com/LeLocTai/Flow.Launcher.Plugin.UnityEngine))_

### **This plugin supports Unity Hub 3**

![demo](https://raw.githubusercontent.com/Ghost-Miner/Unity-project-launcher-plugin/refs/heads/main/demo.gif)

### Usage
    u <project name> 

### Notable features
- Supports use of spaces in project names.
- Shows ★ next to favorite projects.
- Shows ⚠️ next to project which are in version that isn't installed.
- Shows project path, last modified date, and project version under project name.
---

Notes for current version:
--
Listed items are going to be changed or fixed in the next release.
- Use `u /dc` after installing a new Unity version to refresh plugin's list of installed Unity versions.
- Do **not** delete plugin's data folder (_Flow plugins folder_/UPL Data) manually. Modifying the UPL Data folder (or its contents) will cause an error when `u /dc` command is used.
