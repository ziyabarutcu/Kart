using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

[Serializable]
public class LevelCardView
{
    public Image cardImage;
    public Button button;
    public TMP_Text levelLabel;
    public PuzzleLevelConfig levelConfig;
}

public class MainMenuController : MonoBehaviour
{
    [Header("Bölüm Bilgileri")]
    [SerializeField] private string chapterId = "chapter_1";
    [SerializeField] private Texture2D revealTexture;
    [SerializeField] private int rows = 4;
    [SerializeField] private int columns = 5;
    [SerializeField] private int levelOffset = 0;

    [Header("Kart Görselleri")]
    [SerializeField] private Sprite cardBackSprite;
    [SerializeField] private Color lockedTint = new Color(1f, 1f, 1f, 0.25f);
    [SerializeField] private Color unlockedTint = Color.white;
    [SerializeField] private LevelCardView[] cardViews;

    [Header("UI Referansları")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text levelRangeText;
    [SerializeField] private Button playButton;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Sprite backgroundSprite;
    [SerializeField] private Button settingsButton; // Sağ üstteki ayarlar butonu
    [SerializeField] private SettingsController settingsController; // Settings Controller referansı

    [Header("Sahne Ayarları")]
    [SerializeField] private string gameplaySceneName = "SampleScene";

    [Header("Debug/Test")]
    [SerializeField] private bool enableResetShortcut = true; // R tuşu ile reset

    private Sprite[] revealPieces;

    private void Awake()
    {
        if (backgroundImage != null && backgroundSprite != null)
        {
            backgroundImage.sprite = backgroundSprite;
        }

        GenerateRevealPieces();
        RefreshCards();
        ConfigurePlayButton();
        ConfigureSettingsButton();
        
        // Müzik MusicManager tarafından otomatik olarak yönetiliyor
        // OnEnable ve OnSceneLoaded event'leri ile scene'ler arasında kesintisiz devam ediyor
    }

    private void OnEnable()
    {
        RefreshCards();
        UpdatePlayButtonVisibility();
    }

    // Unity Editor'da Inspector'da değişiklik yapıldığında çağrılır
    private void OnValidate()
    {
        // Editor'da revealTexture değiştiğinde parçaları yeniden oluştur
        if (Application.isPlaying && revealTexture != null)
        {
            GenerateRevealPieces();
            RefreshCards();
        }
    }

    private void Update()
    {
        // R tuşu ile progress sıfırlama (sadece enableResetShortcut true ise)
        if (enableResetShortcut && Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
        {
            ResetProgress();
        }
    }

    private void GenerateRevealPieces()
    {
        if (revealTexture == null || rows <= 0 || columns <= 0)
        {
            revealPieces = Array.Empty<Sprite>();
            return;
        }

        int totalPieces = rows * columns;
        revealPieces = new Sprite[totalPieces];

        // Texture'ı eşit parçalara böl
        int pieceWidth = revealTexture.width / columns;
        int pieceHeight = revealTexture.height / rows;

        int index = 0;
        // Üstten alta, soldan sağa oluştur
        for (int row = 0; row < rows; row++)
        {
            for (int column = 0; column < columns; column++)
            {
                // Texture koordinat sistemi: (0,0) sol alt köşe
                // Biz üstten alta oluşturuyoruz, o yüzden Y'yi ters çeviriyoruz
                int startX = column * pieceWidth;
                int startY = (rows - 1 - row) * pieceHeight;

                // Texture'ın sınırlarını kontrol et
                if (startX + pieceWidth > revealTexture.width)
                {
                    pieceWidth = revealTexture.width - startX;
                }
                if (startY + pieceHeight > revealTexture.height)
                {
                    pieceHeight = revealTexture.height - startY;
                }

                Color[] pixels = revealTexture.GetPixels(startX, startY, pieceWidth, pieceHeight);
                Texture2D pieceTexture = new Texture2D(pieceWidth, pieceHeight);
                pieceTexture.SetPixels(pixels);
                pieceTexture.Apply();

                revealPieces[index] = Sprite.Create(
                    pieceTexture,
                    new Rect(0, 0, pieceWidth, pieceHeight),
                    new Vector2(0.5f, 0.5f),
                    100f
                );
                index++;
            }
        }
    }

    private void RefreshCards()
    {
        if (cardViews == null)
        {
            return;
        }

        int totalLevels = Mathf.Min(rows * columns, cardViews.Length);
        UpdateHeaderTexts(totalLevels);

        // Kartları gerçek UI pozisyonlarına göre grid'e yerleştir
        List<(LevelCardView card, int originalIndex, Vector2 anchoredPos)> cardsWithPositions = new List<(LevelCardView, int, Vector2)>();
        for (int i = 0; i < totalLevels; i++)
        {
            if (cardViews[i]?.cardImage == null)
            {
                continue;
            }

            RectTransform rectTransform = cardViews[i].cardImage.rectTransform;
            if (rectTransform != null)
            {
                Vector2 anchoredPos = rectTransform.anchoredPosition;
                cardsWithPositions.Add((cardViews[i], i, anchoredPos));
            }
        }

        // Y pozisyonuna göre sırala (yüksekten düşüğe - üstten alta)
        cardsWithPositions.Sort((a, b) => b.anchoredPos.y.CompareTo(a.anchoredPos.y));

        // Kartları satırlara grupla
        List<List<(LevelCardView card, int originalIndex, float xPos)>> cardRows = new List<List<(LevelCardView, int, float)>>();
        float currentRowY = float.MaxValue;
        float rowTolerance = 20f; // Satırlar arası tolerans (biraz artırdık)

        foreach (var cardInfo in cardsWithPositions)
        {
            float yPos = cardInfo.anchoredPos.y;
            float xPos = cardInfo.anchoredPos.x;

            // Yeni satır mı kontrol et
            if (cardRows.Count == 0 || Mathf.Abs(currentRowY - yPos) > rowTolerance)
            {
                cardRows.Add(new List<(LevelCardView, int, float)>());
                currentRowY = yPos;
            }

            cardRows[cardRows.Count - 1].Add((cardInfo.card, cardInfo.originalIndex, xPos));
        }

        // Her satırı X pozisyonuna göre sırala (soldan sağa)
        foreach (var row in cardRows)
        {
            row.Sort((a, b) => a.xPos.CompareTo(b.xPos));
        }

        // Grid pozisyonuna göre parça index'ini hesapla ve atama yap
        // revealPieces: row=0 (üst), col=0 (sol) -> index=0
        // cardRows[0] = Unity'deki en üst satır
        for (int rowIndex = 0; rowIndex < cardRows.Count; rowIndex++)
        {
            var row = cardRows[rowIndex];
            for (int colIndex = 0; colIndex < row.Count; colIndex++)
            {
                var cardInfo = row[colIndex];
                PuzzleLevelConfig config = cardInfo.card.levelConfig;
                if (config == null)
                {
                    Debug.LogWarning($"[MainMenuController] LevelCardView {cardInfo.originalIndex} için config atanmamış.");
                    continue;
                }

                // Grid pozisyonuna göre parça index'ini hesapla
                // cardRows[0] = üst satır, cardRows[rows-1] = alt satır
                // revealPieces[0] = üst sol, revealPieces[rows*columns-1] = alt sağ
                int calculatedPieceIndex = (rowIndex * columns) + colIndex;

                // config.levelIndex kullan (levelOffset + originalIndex değil)
                // Çünkü originalIndex Unity'deki kart sırası, levelIndex gerçek level numarası
                int actualLevelIndex = config != null ? config.levelIndex : (levelOffset + cardInfo.originalIndex);
                bool completed = LevelProgress.IsLevelCompleted(chapterId, actualLevelIndex);
                bool unlocked = LevelProgress.IsLevelUnlocked(chapterId, actualLevelIndex);
                
                // Debug: Level progress durumunu kontrol et
                if (actualLevelIndex == 4) // Level 5 (index 4)
                {
                    Debug.Log($"[MainMenuController] Level {actualLevelIndex + 1} (index {actualLevelIndex}): completed={completed}, unlocked={unlocked}");
                }
                
                Sprite pieceSprite = revealPieces != null && calculatedPieceIndex < revealPieces.Length ? revealPieces[calculatedPieceIndex] : null;

                ConfigureCard(cardInfo.card, cardInfo.originalIndex, pieceSprite, unlocked, completed, config);
            }
        }
    }

    private void ConfigureCard(LevelCardView cardView, int index, Sprite pieceSprite, bool unlocked, bool completed, PuzzleLevelConfig config)
    {
        if (cardView == null)
        {
            return;
        }

        if (cardView.cardImage != null)
        {
            cardView.cardImage.sprite = completed && pieceSprite != null ? pieceSprite : cardBackSprite;
            cardView.cardImage.color = completed ? unlockedTint : lockedTint;
        }

        if (cardView.levelLabel != null)
        {
            cardView.levelLabel.text = config != null ? (config.levelIndex + 1).ToString() : (levelOffset + index + 1).ToString();
        }

        if (cardView.button != null)
        {
            cardView.button.interactable = unlocked;
            cardView.button.onClick.RemoveAllListeners();
            cardView.button.onClick.AddListener(() => 
            {
                // Level kartına tıklandığında titreşim
                VibrationManager.Vibrate(VibrationType.Medium, 0.1f);
                StartLevel(config);
            });
        }
    }

    private void UpdateHeaderTexts(int totalLevels)
    {
        if (titleText != null)
        {
            titleText.text = "Bölüm 1";
        }

        if (levelRangeText != null)
        {
            int firstLevel = levelOffset + 1;
            int lastLevel = levelOffset + totalLevels;
            levelRangeText.text = $"{firstLevel} - {lastLevel}";
        }
    }

    private void ConfigurePlayButton()
    {
        if (playButton == null)
        {
            return;
        }

        playButton.onClick.RemoveAllListeners();
        playButton.onClick.AddListener(() => 
        {
            // Play Level butonuna tıklandığında titreşim
            VibrationManager.Vibrate(VibrationType.Medium, 0.1f);
            StartNextLevel();
        });
        
        // Play button'ın görünürlüğünü kontrol et
        UpdatePlayButtonVisibility();
    }
    
    private void ConfigureSettingsButton()
    {
        if (settingsButton == null)
        {
            return;
        }

        settingsButton.onClick.RemoveAllListeners();
        settingsButton.onClick.AddListener(OnSettingsButtonClicked);
    }
    
    private void OnSettingsButtonClicked()
    {
        // Ayarlar butonuna tıklandığında titreşim
        VibrationManager.Vibrate(VibrationType.Medium, 0.1f);
        
        if (settingsController != null)
        {
            settingsController.OpenSettings();
        }
    }
    
    private void UpdatePlayButtonVisibility()
    {
        if (playButton == null)
        {
            return;
        }
        
        int totalLevels = Mathf.Min(rows * columns, cardViews?.Length ?? 0);
        int nextPlayableLevel = LevelProgress.GetNextPlayableLevel(chapterId, totalLevels);
        
        // Eğer oynanabilir level yoksa butonu gizle
        bool hasPlayableLevel = nextPlayableLevel >= 0 && nextPlayableLevel < (cardViews?.Length ?? 0);
        playButton.gameObject.SetActive(hasPlayableLevel);
    }

    private void StartNextLevel()
    {
        int totalLevels = Mathf.Min(rows * columns, cardViews?.Length ?? 0);
        int targetLevelIndex = LevelProgress.GetNextPlayableLevel(chapterId, totalLevels);
        
        // Oynanabilir level yoksa çık
        if (targetLevelIndex < 0 || cardViews == null)
        {
            return;
        }
        
        // cardViews dizisinde levelIndex == targetLevelIndex olan config'i bul
        PuzzleLevelConfig targetConfig = null;
        foreach (var cardView in cardViews)
        {
            if (cardView?.levelConfig != null && 
                cardView.levelConfig.levelIndex == targetLevelIndex &&
                string.Equals(cardView.levelConfig.chapterId, chapterId, StringComparison.Ordinal))
            {
                targetConfig = cardView.levelConfig;
                break;
            }
        }
        
        if (targetConfig == null)
        {
            Debug.LogWarning($"[MainMenuController] Level {targetLevelIndex} için config bulunamadı.");
            return;
        }
        
        StartLevel(targetConfig);
    }

    private void StartLevel(PuzzleLevelConfig config)
    {
        if (config == null)
        {
            Debug.LogError("[MainMenuController] Level config atanmadı!");
            return;
        }

        LevelSelectionContext.SetSelection(
            config.chapterId,
            config.levelIndex,
            config,
            GetOrderedChapterLevels());
        SceneManager.LoadScene(gameplaySceneName);
    }

    private IReadOnlyList<PuzzleLevelConfig> GetOrderedChapterLevels()
    {
        List<PuzzleLevelConfig> configs = new List<PuzzleLevelConfig>();
        if (cardViews == null)
        {
            return configs;
        }

        foreach (LevelCardView view in cardViews)
        {
            if (view?.levelConfig == null)
            {
                continue;
            }

            if (!string.Equals(view.levelConfig.chapterId, chapterId, StringComparison.Ordinal))
            {
                continue;
            }

            configs.Add(view.levelConfig);
        }

        configs.Sort((a, b) => a.levelIndex.CompareTo(b.levelIndex));
        return configs;
    }

    /// <summary>
    /// Mevcut chapter için tüm level progress'ini sıfırlar ve kartları yeniler.
    /// Unity Editor'da bir butona bağlanabilir veya R tuşu ile çağrılabilir.
    /// </summary>
    public void ResetProgress()
    {
        int totalLevels = Mathf.Min(rows * columns, cardViews?.Length ?? 0);
        LevelProgress.ResetChapterProgress(chapterId, totalLevels);
        RefreshCards(); // Kartları yeniden yükle
        UpdatePlayButtonVisibility(); // Play button görünürlüğünü güncelle
        Debug.Log($"[MainMenuController] {chapterId} için tüm level progress sıfırlandı.");
    }

    /// <summary>
    /// Tüm chapter'lar için progress'i sıfırlar.
    /// </summary>
    public void ResetAllProgress()
    {
        LevelProgress.ResetAllProgress();
        RefreshCards(); // Kartları yeniden yükle
        UpdatePlayButtonVisibility(); // Play button görünürlüğünü güncelle
        Debug.Log("[MainMenuController] Tüm progress sıfırlandı.");
    }
}

