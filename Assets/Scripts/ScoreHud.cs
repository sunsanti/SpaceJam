using TMPro;
using UnityEngine;

// Shows the current run's score; resets it when the Game scene starts.
[RequireComponent(typeof(TextMeshProUGUI))]
public class ScoreHud : MonoBehaviour
{
    TextMeshProUGUI text;
    int shown = -1;

    void Start()
    {
        text = GetComponent<TextMeshProUGUI>();
        Scoreboard.Current = 0;
    }

    void Update()
    {
        if (shown == Scoreboard.Current) return;
        shown = Scoreboard.Current;
        text.text = $"SCORE  {shown}";
    }
}
