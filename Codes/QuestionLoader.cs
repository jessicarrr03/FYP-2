using System.Collections.Generic;
using UnityEngine;

public class QuestionLoader : MonoBehaviour
{
    [SerializeField] string resourceName = "questions";
    public List<Question> AllForStage { get; private set; } = new();

    void Awake()
    {
        var stageId = GameState.I?.CurrentStageId;
        if (string.IsNullOrEmpty(stageId))
        {
            Debug.LogError("No stage selected. Did you come here via a StageButton?");
            return;
        }

        var csvAsset = Resources.Load<TextAsset>(resourceName);
        if (csvAsset == null)
        {
            Debug.LogError($"Resources/{resourceName}.csv not found. Put your CSV at Assets/Resources/{resourceName}.csv");
            return;
        }

        AllForStage = LoadForStage(csvAsset.text, stageId);
        if (AllForStage.Count < 1)
            Debug.LogWarning($"No questions found for stage '{stageId}'.");
    }

    List<Question> LoadForStage(string csv, string stageFilter)
    {
        var rows = ParseCsv(csv);
        if (rows.Count == 0) return new List<Question>();

        var header = rows[0];
        var idx = new Dictionary<string,int>();
        for (int i = 0; i < header.Count; i++)
        {
            var key = header[i].Trim();
            if (!idx.ContainsKey(key)) idx[key] = i;
        }

        bool Has(string k) => idx.ContainsKey(k);
        string Cell(List<string> r, string k) => Has(k) && idx[k] < r.Count ? r[idx[k]] : "";

        var list = new List<Question>();

        for (int r = 1; r < rows.Count; r++)
        {
            var row = rows[r];
            if (row.Count == 0) continue;

            var stage = Cell(row, "Stage").Trim();
            if (string.IsNullOrEmpty(stage)) continue;
            if (stage != stageFilter) continue;

            var q = new Question
            {
                Stage   = stage,
                Text    = Cell(row, "QuestionText").Trim(),
                A       = Cell(row, "OptionA").Trim(),
                B       = Cell(row, "OptionB").Trim(),
                C       = Cell(row, "OptionC").Trim(),
                D       = Cell(row, "OptionD").Trim(),
                Correct = SafeCorrect(Cell(row, "Correct")),
                ImageKey = NormalizeResourceKey(Cell(row, "Image")),
                QuestionId = Cell(row, "QuestionId").Trim(),
            };
            list.Add(q);
        }

        return list;
    }

    static char SafeCorrect(string s)
    {
        if (string.IsNullOrWhiteSpace(s)) return 'A';
        s = s.Trim();
        return char.ToUpperInvariant(s[0]);
    }

    static string NormalizeResourceKey(string s)
    {
        if (string.IsNullOrWhiteSpace(s)) return null;
        s = s.Trim().Replace("\\", "/");
        int dot = s.LastIndexOf('.');
        return dot > 0 ? s.Substring(0, dot) : s;
    }

    static List<List<string>> ParseCsv(string text)
    {
        var rows = new List<List<string>>();
        var cur = new List<string>();
        var cell = new System.Text.StringBuilder();
        bool inQuotes = false;

        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];

            if (inQuotes)
            {
                if (c == '"')
                {
                    if (i + 1 < text.Length && text[i + 1] == '"') { cell.Append('"'); i++; }
                    else { inQuotes = false; }
                }
                else cell.Append(c);
            }
            else
            {
                if (c == '"') inQuotes = true;
                else if (c == ',') { cur.Add(cell.ToString()); cell.Length = 0; }
                else if (c == '\n' || c == '\r')
                {
                    if (c == '\r' && i + 1 < text.Length && text[i + 1] == '\n') i++;
                    cur.Add(cell.ToString()); cell.Length = 0;
                    rows.Add(cur);
                    cur = new List<string>();
                }
                else cell.Append(c);
            }
        }
        cur.Add(cell.ToString());
        rows.Add(cur);
        return rows;
    }
}
