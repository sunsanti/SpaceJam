using UnityEngine;
using UnityEngine.UI;

// Loads/saves settings via PlayerPrefs and applies them. Lives on the Canvas (always active)
// so saved music settings apply on boot, even before the Settings panel is opened.
public class SettingsMenu : MonoBehaviour
{
    [SerializeField] Toggle musicToggle;
    [SerializeField] Slider musicSlider;
    [SerializeField] Toggle sfxToggle;
    [SerializeField] Slider sfxSlider;
    [SerializeField] Toggle fullscreenToggle;

    // Read these from gameplay code when playing sound effects.
    public static bool SfxOn => PlayerPrefs.GetInt("SfxOn", 1) == 1;
    public static float SfxVolume => PlayerPrefs.GetFloat("SfxVolume", 1f);

    void Start()
    {
        // Set UI from saved values before listening, so loading doesn't re-save.
        musicToggle.isOn = PlayerPrefs.GetInt("MusicOn", 1) == 1;
        musicSlider.value = PlayerPrefs.GetFloat("MusicVolume", 0.7f);
        sfxToggle.isOn = SfxOn;
        sfxSlider.value = SfxVolume;
        fullscreenToggle.isOn = Screen.fullScreen;

        musicToggle.onValueChanged.AddListener(on => { PlayerPrefs.SetInt("MusicOn", on ? 1 : 0); ApplyMusic(); });
        musicSlider.onValueChanged.AddListener(v => { PlayerPrefs.SetFloat("MusicVolume", v); ApplyMusic(); });
        sfxToggle.onValueChanged.AddListener(on => { PlayerPrefs.SetInt("SfxOn", on ? 1 : 0); sfxSlider.interactable = on; });
        sfxSlider.onValueChanged.AddListener(v => PlayerPrefs.SetFloat("SfxVolume", v));
        fullscreenToggle.onValueChanged.AddListener(on => Screen.fullScreen = on);

        sfxSlider.interactable = sfxToggle.isOn;
        ApplyMusic();
    }

    void ApplyMusic()
    {
        musicSlider.interactable = musicToggle.isOn;
        GameAudio.ApplyMusicSettings();
    }
}
