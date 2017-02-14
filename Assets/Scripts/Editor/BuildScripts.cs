using UnityEditor;
using System.Diagnostics;
using UnityEngine;

public class BuildScripts 
{
	static string[] TV_LEVELS = new string[] {"Assets/_Scenes/tv/Idle.unity", "Assets/_Scenes/shared/Game.unity", "Assets/_Scenes/tv/Leaderboard.unity"};
	static string[] CLIENT_LEVELS = new string[] {"Assets/_Scenes/mobile/Idle.unity", "Assets/_Scenes/mobile/MiniGame.unity", "Assets/_Scenes/shared/Game.unity", "Assets/_Scenes/mobile/Leaderboard.unity"};

	[MenuItem("Builds/OS X TV")]
	public static void BuildMacTV ()
	{
		// Get filename.
		string path = "";



		// Build player.
		BuildPipeline.BuildPlayer(TV_LEVELS, path + "TV.app", BuildTarget.StandaloneOSXUniversal, BuildOptions.Development);


		// Run the game (Process class from System.Diagnostics).
		Process proc = new Process();
		proc.StartInfo.FileName = path + "TV.app";
		proc.Start();
	}

	[MenuItem("Builds/OS X client")]
	public static void BuildMacClient ()
	{
		// Get filename.
		string path = "";



		// Build player.
		BuildPipeline.BuildPlayer(CLIENT_LEVELS, path + "client.app", BuildTarget.StandaloneOSXUniversal, BuildOptions.Development);


		// Run the game (Process class from System.Diagnostics).
		Process proc = new Process();
		proc.StartInfo.FileName = path + "client.app";
		proc.Start();
	}

    [MenuItem("Builds/Windows TV")]
    public static void BuildWindowsTV()
    {
        // Get filename.
        string path = "";


        // Build player.
		BuildPipeline.BuildPlayer(TV_LEVELS, path + "TV.exe", BuildTarget.StandaloneWindows64, BuildOptions.Development);


        // Run the game (Process class from System.Diagnostics).
        Process proc = new Process();
        proc.StartInfo.FileName = path + "TV.exe";
        proc.Start();
    }

    [MenuItem("Builds/Windows client")]
    public static void BuildWindowsClient()
    {
        // Get filename.
        string path = "";


        // Build player.
		BuildPipeline.BuildPlayer(CLIENT_LEVELS, path + "client.exe", BuildTarget.StandaloneWindows64, BuildOptions.Development);


        // Run the game (Process class from System.Diagnostics).
        Process proc = new Process();
        proc.StartInfo.FileName = path + "client.exe";
        proc.Start();
    }
}