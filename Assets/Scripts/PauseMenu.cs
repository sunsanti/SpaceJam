using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

// ESC toggles pause. Pausing freezes the game with Time.timeScale = 0.
public class PauseMenu : MonoBehaviour
{
    [SerializeField] GameObject pausePanel;

    bool paused;

    void Start() => Resume();

    void Update()
    {
        var kb = Keyboard.current;
        if (kb == null || !kb.escapeKey.wasPressedThisFrame) return;
        if (paused) Resume(); else Pause();
    }

    public void Pause()
    {
        paused = true;
        Time.timeScale = 0;
        pausePanel.SetActive(true);
    }

    public void Resume()
    {
        paused = false;
        Time.timeScale = 1;
        pausePanel.SetActive(false);
    }

    public void QuitToMenu()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene("MainMenu");
    }
}
