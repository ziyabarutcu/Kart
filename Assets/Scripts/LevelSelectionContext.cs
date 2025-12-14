using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Basit bir static context; menüden hangi bölüm ve level seçildiyse
/// oyun sahnesine kadar bu bilgiyi taşıyoruz.
/// </summary>
public static class LevelSelectionContext
{
    public static bool HasSelection { get; private set; }
    public static string ChapterId { get; private set; }
    public static int LevelIndex { get; private set; }
    public static PuzzleLevelConfig LevelConfig { get; private set; }
    public static IReadOnlyList<PuzzleLevelConfig> ChapterLevels { get; private set; }

    public static void SetSelection(
        string chapterId,
        int levelIndex,
        PuzzleLevelConfig config,
        IReadOnlyList<PuzzleLevelConfig> chapterLevels = null)
    {
        ChapterId = chapterId;
        LevelIndex = levelIndex;
        LevelConfig = config;
        ChapterLevels = chapterLevels;
        HasSelection = true;
    }

    public static void Clear()
    {
        HasSelection = false;
        LevelConfig = null;
        ChapterLevels = null;
    }
}

