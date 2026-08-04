using UnityEditor;
using UnityEditor.SceneManagement;

[InitializeOnLoad]
public static class PlayModeStartSceneSetup
{
    private const string InitScenePath = "Assets/Scenes/Init.unity";

    static PlayModeStartSceneSetup()
    {
        if (EditorSceneManager.playModeStartScene != null)
        {
            return;
        }

        SceneAsset initScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(InitScenePath);

        if (initScene == null)
        {
            return;
        }

        EditorSceneManager.playModeStartScene = initScene;
    }
}
