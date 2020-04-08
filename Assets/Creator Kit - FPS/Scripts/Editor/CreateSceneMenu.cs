using System.Collections;
using System.Collections.Generic;
using System.Data.OleDb;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class CreateSceneMenu
{
    [MenuItem("FPSKIT/Create new Scene")]
    static void CreateSceneRoom()
    {
        NewScene("Template_room");
    }
    
    [MenuItem("FPSKIT/Create new Empty Scene")]
    static void CreateSceneEmpty()
    {
        NewScene("Template");
    }

    static void NewScene(string originalScene)
    {
        string originalPath = "Assets/Scenes/Template.unity";
        string newPath = EditorUtility.SaveFilePanel("New Scene", "Assets/Creator Kit - FPS/Scenes", "MyScene", "unity");

        if (!string.IsNullOrEmpty(newPath))
        {
            newPath = newPath.Replace(Application.dataPath, "Assets");
            
            //if return false, maybe they moved the original asset, try to search for it
            if (!AssetDatabase.CopyAsset(originalPath, newPath))
            {
                string[] foundAssets = AssetDatabase.FindAssets($"{originalScene} t:Scene");

                if (foundAssets.Length == 0)
                {
                    Debug.LogError("Couldn't find the Template scene. Did you delete it? See the manual for how to create a scene manually");
                }
                else
                {
                    originalPath = AssetDatabase.GUIDToAssetPath(foundAssets[0]);
                    if (!AssetDatabase.CopyAsset(originalPath, newPath))
                    {
                        Debug.LogErrorFormat("Couldn't copy Template scene  at {0}. See the manual for how to create a scene manually", originalPath);
                    }
                    else
                    {
                        EditorSceneManager.OpenScene(newPath);
                    }
                }
            }
        }
    }
}
