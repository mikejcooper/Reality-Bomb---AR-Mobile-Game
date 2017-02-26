using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;

public class UtilityScripts
{
    static string[] ScenesList = new string[] { "Assets/_Scenes/shared/Game.unity", "Assets/_Scenes/mobile/Sandbox.unity" };

    [MenuItem ("Utilities/Deactivate AR")]
    public static void DeactivateAllARToolKit()
    {
        SetAllARToolKitStatus(false);
    }

    [MenuItem("Utilities/Activate AR")]
    public static void ActivateAllARToolKit()
    {
        SetAllARToolKitStatus(true);
    }

    private static void SetAllARToolKitStatus(bool state)
    {
        //string originalScene = EditorSceneManager.GetActiveScene().path;
        foreach (string scene in ScenesList)
        {
            EditorSceneManager.OpenScene(scene);
            ARController[] arcontrollers = GameObject.FindObjectsOfType<ARController>();
            foreach (ARController a in arcontrollers)
            {
                a.enabled = state;
            }
        }
        //EditorSceneManager.OpenScene(originalScene);
    }
} 