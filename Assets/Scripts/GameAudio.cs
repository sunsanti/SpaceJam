using UnityEngine;
using UnityEngine.SceneManagement;

// Creates itself before the first scene loads and survives scene changes, so no scene wiring needed.
// Clips live in Assets/Resources/Audio (all CC0, see CREDITS.txt there).
// Music switches per scene; SFX respect the Settings menu toggles/volumes.
public static class GameAudio
{
    static AudioSource music, sfx;

    // Multiply looping SFX volume by this (0 when SFX is off).
    public static float SfxScale => SettingsMenu.SfxOn ? SettingsMenu.SfxVolume : 0f;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Init()
    {
        var go = new GameObject("GameAudio");
        Object.DontDestroyOnLoad(go);
        music = go.AddComponent<AudioSource>();
        music.loop = true;
        music.ignoreListenerPause = true; // keep music going in the pause menu
        sfx = go.AddComponent<AudioSource>();
        SceneManager.sceneLoaded += (scene, _) => PlayMusic(scene.name == "MainMenu" ? "MenuMusic" : "GameMusic");
    }

    static void PlayMusic(string name)
    {
        var clip = Resources.Load<AudioClip>("Audio/" + name);
        if (music.clip != clip)
        {
            music.clip = clip;
            music.Play();
        }
        ApplyMusicSettings();
    }

    public static void ApplyMusicSettings()
    {
        music.mute = PlayerPrefs.GetInt("MusicOn", 1) == 0;
        music.volume = PlayerPrefs.GetFloat("MusicVolume", 0.7f) * 0.6f; // tracks are loud next to SFX
    }

    public static void Play(string name, float volume = 1f)
    {
        if (SfxScale > 0f) sfx.PlayOneShot(Resources.Load<AudioClip>("Audio/" + name), volume * SfxScale);
    }
}
