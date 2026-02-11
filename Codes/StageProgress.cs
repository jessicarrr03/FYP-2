using System;
using System.IO;
using UnityEngine;

public static class StageProgress
{
    static readonly string FileName = "stage_results.txt";

    static string FilePath => Path.Combine(Application.persistentDataPath, FileName);


    public static void SaveStageResult(string stageId, int score, int total, bool passed)
    {
        string line = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | StageId={stageId} | Score={score}/{total} | Passed={passed}";
        File.AppendAllText(FilePath, line + Environment.NewLine);
        Debug.Log($"[StageProgress] Saved: {line}\nAt: {FilePath}");
    }

    public static bool IsStagePassed(string stageId)
    {
        if (!File.Exists(FilePath))
        {
            Debug.Log($"[StageProgress] No results file yet. {stageId} -> false");
            return false;
        }

        bool everPassed = false;
        foreach (var line in File.ReadAllLines(FilePath))
        {
            if (!line.Contains($"StageId={stageId}")) continue;

            if (line.IndexOf("Passed=true", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                everPassed = true;
                break;
            }
        }

        Debug.Log($"[StageProgress] Ever passed {stageId}? {everPassed}");
        return everPassed;
    }


    public static bool WasLastAttemptPassed(string stageId)
    {
        if (!File.Exists(FilePath)) return false;

        string lastForStage = null;
        foreach (var line in File.ReadAllLines(FilePath))
        {
            if (line.Contains($"StageId={stageId}"))
                lastForStage = line;
        }

        if (string.IsNullOrEmpty(lastForStage)) return false;
        return lastForStage.IndexOf("Passed=true", StringComparison.OrdinalIgnoreCase) >= 0;
    }
}