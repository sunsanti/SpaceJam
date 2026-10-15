using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ScoreEntry
{
    public int score;
    public string date;
}

// Score of the current run + history of the last MaxEntries runs, saved as JSON in PlayerPrefs.
// History is oldest-first; once full, each new run pushes out the oldest one.
public static class Scoreboard
{
    public const int MaxEntries = 15;
    const string Key = "Scoreboard";

    public static int Current;

    [Serializable]
    class Data { public List<ScoreEntry> entries = new(); }

    public static List<ScoreEntry> Load() =>
        JsonUtility.FromJson<Data>(PlayerPrefs.GetString(Key, "{}"))?.entries ?? new List<ScoreEntry>();

    public static void Record(int score)
    {
        var entries = Load();
        entries.Add(new ScoreEntry { score = score, date = DateTime.Now.ToString("dd/MM/yyyy HH:mm") });
        if (entries.Count > MaxEntries) entries.RemoveRange(0, entries.Count - MaxEntries);
        PlayerPrefs.SetString(Key, JsonUtility.ToJson(new Data { entries = entries }));
        PlayerPrefs.Save();
    }
}
