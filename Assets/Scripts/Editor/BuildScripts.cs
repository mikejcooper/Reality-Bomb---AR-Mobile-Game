using UnityEditor;
using System.Diagnostics;
using UnityEngine;
using UnityEditor.SceneManagement;

public class BuildScripts 
{
	static string[] TV_LEVELS = new string[] {"Assets/_Scenes/tv/Idle.unity", "Assets/_Scenes/shared/Game.unity", "Assets/_Scenes/tv/Leaderboard.unity"};
	static string[] CLIENT_LEVELS = new string[] {"Assets/_Scenes/mobile/Idle.unity", "Assets/_Scenes/mobile/Login.unity", "Assets/_Scenes/mobile/Sandbox.unity", "Assets/_Scenes/shared/Game.unity", "Assets/_Scenes/mobile/Leaderboard.unity"};

	private static BuildOptions GenerateBuildOptions () {
		return BuildOptions.Development | BuildOptions.AutoRunPlayer | BuildOptions.AcceptExternalModificationsToPlayer;
	}

	private static void BuildAndRun (string fileName, string[] levelsArray, BuildTarget buildTarget, BuildOptions buildOptions) {
		// Build player.
		BuildPipeline.BuildPlayer(levelsArray, fileName, buildTarget, buildOptions);
	}

	[MenuItem("Builds/OS X TV")]
	public static void BuildMacTV ()
	{
		EnsureARControllerState (false);
		BuildAndRun ("Reality Bomb Server.app", TV_LEVELS, BuildTarget.StandaloneOSXUniversal, GenerateBuildOptions ());
	}

	[MenuItem("Builds/OS X client")]
	public static void BuildMacClient ()
	{
		EnsureARControllerState (false);
		BuildAndRun ("Reality Bomb Client.app", CLIENT_LEVELS, BuildTarget.StandaloneOSXUniversal, GenerateBuildOptions ());
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
		BuildAndRun ("Reality Bomb Server.exe", TV_LEVELS, BuildTarget.StandaloneWindows64, GenerateBuildOptions ());
    }

    [MenuItem("Builds/Windows client")]
    public static void BuildWindowsClient()
    {
		EnsureARControllerState (false);
		BuildAndRun ("Reality Bomb Client.exe", CLIENT_LEVELS, BuildTarget.StandaloneWindows64, GenerateBuildOptions ());
    }

	private static void EnsureARControllerState (bool enabled) {
		// make sure ar controllers are enabled
		var scene = EditorSceneManager.GetActiveScene();
		EditorSceneManager.SaveScene(scene);
		scene = EditorSceneManager.OpenScene("Assets/_Scenes/mobile/Sandbox.unity");
		GameObject.FindObjectOfType<ARController> ().enabled = enabled;
		EditorSceneManager.SaveScene(scene);
		scene = EditorSceneManager.OpenScene("Assets/_Scenes/shared/Game.unity");
		GameObject.FindObjectOfType<ARController> ().enabled = enabled;
		EditorSceneManager.SaveScene(scene);
	}
}