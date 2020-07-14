using UnityEditor;
using UnityEngine;
 
public class BuildForiOS : MonoBehaviour {

  static void _BuildForiOS(bool appendFlag=false, bool openInXCode=false) {

    if (EditorUserBuildSettings.selectedBuildTargetGroup != BuildTargetGroup.iOS) {
      Debug.LogError("<b>Build for iOS</b> requires <b>iOS Platform</b> to be selected.");
      return;
    }

    // Get all scenes from build setting UI.
    EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
    string[] scenesPath = new string[scenes.Length];
    for (int i = 0; i < scenesPath.Length; i++) scenesPath[i] = scenes[i].path;

    // Select the target folder
    string  previousPath = EditorPrefs.GetString("BuildForiOS.PreviousPath", Application.persistentDataPath),
            destination = EditorUtility.SaveFilePanel("Choose a destination", previousPath, EditorPrefs.GetString("BuildForiOS.Name",""), "");

    // Check if user canceled the action.
    if (!string.IsNullOrEmpty(destination)) {

      // Save settings for the next build.
      int lastSlash = destination.LastIndexOf("/");
      string path = destination.Substring(0, lastSlash), name = destination.Substring(lastSlash + 1);
      EditorPrefs.SetString("BuildForiOS.PreviousPath", path);
      EditorPrefs.SetString("BuildForiOS.Name", name);

      // Standard build options
      BuildOptions buildOps = BuildOptions.ShowBuiltPlayer | BuildOptions.Il2CPP;

      // Append?
      if (appendFlag) buildOps |= BuildOptions.AcceptExternalModificationsToPlayer;

      // Open in XCode?
      //
      // This may not work until Unity knows about XCode:
      //  - Copy the value of DVTPlugInCompatibilityUUID from XCode.app/Contents/Info.plist
      //  - Paste into Unity4XC.xcplugin/Contents/Info.plist in the Unity.app bundle
      //
      if (openInXCode) buildOps |= BuildOptions.AutoRunPlayer;

      // Additional options from Unity's "Build Settings" dialog
      if (EditorUserBuildSettings.symlinkLibraries) buildOps |= BuildOptions.SymlinkLibraries;
      if (EditorUserBuildSettings.development) {
        buildOps |= BuildOptions.Development;
        if (EditorUserBuildSettings.connectProfiler) buildOps |= BuildOptions.ConnectWithProfiler;
        if (EditorUserBuildSettings.allowDebugging) buildOps |= BuildOptions.AllowDebugging;
      }

      Debug.Log("BuildOptions = " + buildOps);

      // Build the player. May not open XCode without customizing Info.plist.
      BuildPipeline.BuildPlayer(scenesPath, destination, BuildTarget.iOS, buildOps);
    }

  }

  [MenuItem("File/Build for iOS/Build")]
  static void BuildForiOSDefault() { _BuildForiOS(); }

  [MenuItem("File/Build for iOS/Build (Append)")]
  static void BuildForiOSAppend() { _BuildForiOS(true); }

  [MenuItem("File/Build for iOS/Build and Open")]
  static void RunForiOS() { _BuildForiOS(false, true); }

  [MenuItem("File/Build for iOS/Build and Open (Append)")]
  static void RunForiOSAppend() { _BuildForiOS(true, true); }

}