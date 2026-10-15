using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;

// Fills the scoreboard list each time the popup opens: newest run first, best score highlighted.
public class ScoreboardPanel : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI list;

    void OnEnable()
    {
        var entries = Scoreboard.Load();
        if (entries.Count == 0)
        {
            list.text = "No games played yet.";
            return;
        }

        int best = entries.Max(e => e.score);
        var sb = new StringBuilder("<color=#8CD9FF>#<pos=12%>SCORE<pos=45%>DATE</color>\n");
        for (int i = entries.Count - 1; i >= 0; i--)
        {
            var e = entries[i];
            var line = $"{entries.Count - i}<pos=12%>{e.score}<pos=45%>{e.date}";
            sb.AppendLine(e.score == best ? $"<color=#FFC34D>{line}<pos=85%>BEST</color>" : line);
        }
        list.text = sb.ToString();
    }
}
