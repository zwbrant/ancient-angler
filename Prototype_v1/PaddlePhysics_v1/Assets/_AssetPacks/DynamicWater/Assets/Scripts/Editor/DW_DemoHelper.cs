using System.IO;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public class DW_DemoHelper : MonoBehaviour {
    private static readonly string[] DemoScenes = {
        "DW_Menu",
        "DW_Waterfall",
        "DW_Buoyancy",
        "DW_BuoyancyMobile",
        "DW_Pool",
        "DW_PoolMobile",
        "DW_Boat",
        "DW_BoatMobile",
        "DW_Character",
        "DW_Obstruction"
    };

    static DW_DemoHelper() {
        EditorApplication.playmodeStateChanged += () => {
            if (EditorApplication.isPlayingOrWillChangePlaymode &&
                !EditorApplication.isPlaying) {
                CheckLayers();
            }
        };
    }

    private static void CheckLayers() {
        // Is the scene present if the list?
        string scene = Path.GetFileNameWithoutExtension(EditorApplication.currentScene);
        foreach (string x in DemoScenes) {
            if (x != scene) 
                continue;

            DW_LayerTagChecker.ShowMissingTagsAndLayersDialog();
            break;
        }
    }
}