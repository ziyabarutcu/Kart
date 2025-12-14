using UnityEngine;

[CreateAssetMenu(menuName = "Puzzle/Level Config", fileName = "PuzzleLevelConfig")]
public class PuzzleLevelConfig : ScriptableObject
{
    [Header("Kimlik")]
    public string chapterId = "chapter_1";
    public int levelIndex;
    public int totalLevelsInChapter = 20;

    [Header("Puzzle Görseli")]
    public Texture2D puzzleImage;
    [Range(2, 10)] public int gridSizeX = 4;
    [Range(2, 10)] public int gridSizeY = 5;
    [Tooltip("Puzzle'ın dünyadaki toplam yüksekliği (unit cinsinden)")]
    public float targetWorldHeight = 8f;

    [Header("UI")]
    public string displayName = "Level";
}

