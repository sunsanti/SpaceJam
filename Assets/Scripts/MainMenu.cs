using UnityEngine;
using UnityEngine.SceneManagement;

// Main menu stays visible; Instruction/Settings open as popups on top of it.
public class MainMenu : MonoBehaviour
{
    [SerializeField] GameObject instructionPanel;
    [SerializeField] GameObject settingsPanel;
    [SerializeField] string gameScene = "Game";

    void Start() => Back();

    public void StartGame() => SceneManager.LoadScene(gameScene);

    public void OpenInstruction() => ShowPopup(instructionPanel);
    public void OpenSettings()    => ShowPopup(settingsPanel);
    public void Back()            => ShowPopup(null);

    void ShowPopup(GameObject popup)
    {
        instructionPanel.SetActive(popup == instructionPanel);
        settingsPanel.SetActive(popup == settingsPanel);
    }
}
