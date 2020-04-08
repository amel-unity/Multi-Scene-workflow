using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class HideSkybox
{
    static HideSkybox ()
    {
        EditorApplication.update += FirstFrameUpdate;
    }

    static void FirstFrameUpdate ()
    {
        bool skyboxUpdated = UpdateSceneViewSkybox ();
     
        if(skyboxUpdated)
            EditorApplication.update -= FirstFrameUpdate;
    }

    static bool UpdateSceneViewSkybox ()
    {
        SceneView sceneView = SceneView.lastActiveSceneView;

        if (sceneView == null)
            return false;
        
        SceneView.SceneViewState state = sceneView.sceneViewState;
        state.SetAllEnabled(false);
        sceneView.sceneViewState = state;

        return true;
    }
}
