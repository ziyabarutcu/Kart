using UnityEngine;

/// <summary>
/// Level tamamlama / kilit açma durumlarını PlayerPrefs üzerinde saklar.
/// </summary>
public static class LevelProgress
{
    private const string CompletedFormat = "chapter_{0}_level_{1}_completed";
    private const string HighestUnlockedFormat = "chapter_{0}_highest";

    public static bool IsLevelCompleted(string chapterId, int levelIndex)
    {
        string key = string.Format(CompletedFormat, chapterId, levelIndex);
        return PlayerPrefs.GetInt(key, 0) == 1;
    }

    public static bool IsLevelUnlocked(string chapterId, int levelIndex)
    {
        if (levelIndex <= 0)
        {
            return true;
        }

        string highestKey = string.Format(HighestUnlockedFormat, chapterId);
        int highest = PlayerPrefs.GetInt(highestKey, 0);
        return levelIndex <= highest;
    }

    public static void MarkLevelComplete(string chapterId, int levelIndex, int totalLevelsInChapter)
    {
        Debug.Log($"[LevelProgress] MarkLevelComplete çağrıldı: chapterId={chapterId}, levelIndex={levelIndex}, totalLevelsInChapter={totalLevelsInChapter}");
        
        string completeKey = string.Format(CompletedFormat, chapterId, levelIndex);
        PlayerPrefs.SetInt(completeKey, 1);

        string highestKey = string.Format(HighestUnlockedFormat, chapterId);
        int highest = PlayerPrefs.GetInt(highestKey, 0);
        int nextCandidate = Mathf.Clamp(levelIndex + 1, 0, totalLevelsInChapter - 1);

        if (nextCandidate > highest)
        {
            PlayerPrefs.SetInt(highestKey, nextCandidate);
            Debug.Log($"[LevelProgress] Next level unlocked: {nextCandidate}");
        }

        PlayerPrefs.Save();
        Debug.Log($"[LevelProgress] Level {levelIndex} completed ve kaydedildi.");
    }

    public static int GetNextPlayableLevel(string chapterId, int totalLevelsInChapter)
    {
        // İlk level (0) her zaman oynanabilir
        if (!IsLevelCompleted(chapterId, 0))
        {
            return 0;
        }

        // İlk tamamlanmamış level'i bul
        for (int i = 1; i < totalLevelsInChapter; i++)
        {
            if (IsLevelUnlocked(chapterId, i) && !IsLevelCompleted(chapterId, i))
            {
                return i;
            }
        }

        // Tüm level'ler tamamlanmışsa -1 döndür (oynanabilir level yok)
        return -1;
    }

    /// <summary>
    /// Belirli bir chapter için tüm level progress'ini sıfırlar.
    /// </summary>
    public static void ResetChapterProgress(string chapterId, int totalLevelsInChapter)
    {
        // Tüm level'lerin completed durumunu sıfırla
        for (int i = 0; i < totalLevelsInChapter; i++)
        {
            string completeKey = string.Format(CompletedFormat, chapterId, i);
            PlayerPrefs.DeleteKey(completeKey);
        }

        // Highest unlocked'ı sıfırla (sadece ilk level açık kalır)
        string highestKey = string.Format(HighestUnlockedFormat, chapterId);
        PlayerPrefs.DeleteKey(highestKey);

        PlayerPrefs.Save();
        Debug.Log($"[LevelProgress] Chapter {chapterId} progress sıfırlandı.");
    }

    /// <summary>
    /// Tüm level progress'ini sıfırlar (tüm chapter'lar için).
    /// </summary>
    public static void ResetAllProgress()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        Debug.Log("[LevelProgress] Tüm progress sıfırlandı.");
    }
}

