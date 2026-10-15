using UnityEditor;
using UnityEditor.SceneManagement;

// Pressing Play in the Editor always starts from the main menu, whichever scene is open.
// The open scene comes back after leaving Play mode.
[InitializeOnLoad]
static class PlayFromMainMenu
{
    static PlayFromMainMenu() =>
        EditorSceneManager.playModeStartScene = AssetDatabase.LoadAssetAtPath<SceneAsset>("Assets/Scenes/MainMenu.unity");
}
