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
		BuildAndRun ("TV.app", TV_LEVELS, BuildTarget.StandaloneOSXUniversal, GenerateBuildOptions ());
	}

	[MenuItem("Builds/OS X client")]
	public static void BuildMacClient ()
	{
		BuildAndRun ("client.app", CLIENT_LEVELS, BuildTarget.StandaloneOSXUniversal, GenerateBuildOptions ());
	}

	[MenuItem("Builds/Android")]
	public static void BuildAndroid ()
	{
		BuildAndRun ("Android.app", CLIENT_LEVELS, BuildTarget.Android, GenerateBuildOptions ());
	}

	[MenuItem("Builds/iOS")]
	public static void BuildIOS ()
	{
		BuildAndRun ("iOS.app", CLIENT_LEVELS, BuildTarget.iOS, GenerateBuildOptions ());
	}

    [MenuItem("Builds/Windows TV")]
    public static void BuildWindowsTV()
    {
		BuildAndRun ("TV.exe", TV_LEVELS, BuildTarget.StandaloneWindows64, GenerateBuildOptions ());
    }

    [MenuItem("Builds/Windows client")]
    public static void BuildWindowsClient()
    {
		BuildAndRun ("client.exe", CLIENT_LEVELS, BuildTarget.StandaloneWindows64, GenerateBuildOptions ());
    }
}