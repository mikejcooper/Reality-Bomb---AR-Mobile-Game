﻿using UnityEditor;
using System.Diagnostics;
using UnityEngine;

public class BuildScripts 
{
	[MenuItem("Builds/OS X TV")]
	public static void BuildMacTV ()
	{
		// Get filename.
		string path = "";

		string[] levels = new string[] {"Assets/_Scenes/tv/Idle.unity", "Assets/_Scenes/shared/Game.unity", "Assets/_Scenes/tv/Leaderboard.unity"};

		// Build player.
		BuildPipeline.BuildPlayer(levels, path + "TV.app", BuildTarget.StandaloneOSXUniversal, BuildOptions.None);


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

		string[] levels = new string[] {"Assets/_Scenes/mobile/Idle.unity", "Assets/_Scenes/mobile/MiniGame.unity", "Assets/_Scenes/shared/Game.unity", "Assets/_Scenes/mobile/Leaderboard.unity"};

		// Build player.
		BuildPipeline.BuildPlayer(levels, path + "client.app", BuildTarget.StandaloneOSXUniversal, BuildOptions.None);


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

        string[] levels = new string[] { "Assets/_Scenes/tv/Idle.unity", "Assets/_Scenes/shared/Game.unity", "Assets/_Scenes/tv/Leaderboard.unity" };

        // Build player.
        BuildPipeline.BuildPlayer(levels, path + "TV.exe", BuildTarget.StandaloneWindows64, BuildOptions.None);


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

        string[] levels = new string[] { "Assets/_Scenes/mobile/Idle.unity", "Assets/_Scenes/mobile/MiniGame.unity", "Assets/_Scenes/shared/Game.unity", "Assets/_Scenes/mobile/Leaderboard.unity" };

        // Build player.
        BuildPipeline.BuildPlayer(levels, path + "client.exe", BuildTarget.StandaloneWindows64, BuildOptions.None);


        // Run the game (Process class from System.Diagnostics).
        Process proc = new Process();
        proc.StartInfo.FileName = path + "client.exe";
        proc.Start();
    }
}