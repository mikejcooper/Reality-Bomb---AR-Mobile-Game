using UnityEditor;
using System.Diagnostics;
using UnityEngine;

public class BuildScripts 
{
	static string[] TV_LEVELS = new string[] {"Assets/_Scenes/tv/Idle.unity", "Assets/_Scenes/shared/Game.unity", "Assets/_Scenes/tv/Leaderboard.unity"};
	static string[] CLIENT_LEVELS = new string[] {"Assets/_Scenes/mobile/Idle.unity", "Assets/_Scenes/mobile/Sandbox.unity", "Assets/_Scenes/shared/Game.unity", "Assets/_Scenes/mobile/Leaderboard.unity"};

	private static BuildOptions GenerateBuildOptions () {
		return BuildOptions.Development | BuildOptions.AutoRunPlayer | BuildOptions.AcceptExternalModificationsToPlayer;
	}

	private static void BuildAndRun (string fileName, string[] levelsArray, BuildTarget buildTarget, BuildOptions buildOptions) {
		// Build player.
		BuildPipeline.BuildPlayer(levelsArray, fileName, buildTarget, buildOptions);

//		// Run
//		Process proc = new Process();
//		proc.StartInfo.FileName = fileName;
//		proc.Start();
	}

	[MenuItem("Builds/OS X TV")]
	public static void BuildMacTV ()
	{
		EnsureARControllerState (false);
		BuildAndRun ("TV.app", TV_LEVELS, BuildTarget.StandaloneOSXUniversal, GenerateBuildOptions ());
	}

	[MenuItem("Builds/OS X client")]
	public static void BuildMacClient ()
	{
		EnsureARControllerState (false);
		BuildAndRun ("client.app", CLIENT_LEVELS, BuildTarget.StandaloneOSXUniversal, GenerateBuildOptions ());
	}

	[MenuItem("Builds/Android")]
	public static void BuildAndroid ()
	{
		BuildAndRun ("Android.app", CLIENT_LEVELS, BuildTarget.Android, GenerateBuildOptions () | BuildOptions.SymlinkLibraries);
	}

	[MenuItem("Builds/iOS")]
	public static void BuildIOS ()
	{
		EnsureARControllerState (true);
		BuildAndRun ("iOS.app", CLIENT_LEVELS, BuildTarget.iOS, GenerateBuildOptions ());
	}

    [MenuItem("Builds/Windows TV")]
    public static void BuildWindowsTV()
    {
		EnsureARControllerState (false);
		BuildAndRun ("TV.exe", TV_LEVELS, BuildTarget.StandaloneWindows64, GenerateBuildOptions ());
    }

    [MenuItem("Builds/Windows client")]
    public static void BuildWindowsClient()
    {
		EnsureARControllerState (false);
		BuildAndRun ("client.exe", CLIENT_LEVELS, BuildTarget.StandaloneWindows64, GenerateBuildOptions ());
    }

	private static void EnsureARControllerState (bool enabled) {
		// make sure ar controllers are enabled
		EditorApplication.SaveScene();
		EditorApplication.OpenScene("Assets/_Scenes/mobile/Sandbox.unity");
		GameObject.FindObjectOfType<ARController> ().enabled = enabled;
		EditorApplication.SaveScene();
		EditorApplication.OpenScene("Assets/_Scenes/shared/Game.unity");
		GameObject.FindObjectOfType<ARController> ().enabled = enabled;
		EditorApplication.SaveScene();
	}
}