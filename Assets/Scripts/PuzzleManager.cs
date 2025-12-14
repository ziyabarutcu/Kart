using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class PuzzleManager : MonoBehaviour
{
    [Header("Puzzle Settings")]
    [SerializeField] private Texture2D puzzleImage;
    [SerializeField] [Range(2, 10)] private int gridSizeX = 3;
    [SerializeField] [Range(2, 10)] private int gridSizeY = 3;
    [SerializeField] private float pieceSpacing = 0.02f;
    [SerializeField] private float pixelsPerUnit = 100f;
    [SerializeField] [Range(0.4f, 1f)] private float slotSnapMultiplier = 0.65f;
    
    [Header("Prefabs")]
    [SerializeField] private GameObject puzzlePiecePrefab;
    [SerializeField] private GameObject slotPlaceholderPrefab;
    
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip snapSound;
    
    [Header("UI")]
    [SerializeField] private PuzzleUI puzzleUI;

    [Header("Config")]
    [SerializeField] private PuzzleLevelConfig defaultLevelConfig;
    private PuzzleLevelConfig activeLevelConfig;
    
    [Header("Shuffle Animation")]
    [SerializeField] private bool enableShuffleAnimation = true;
    [SerializeField] private float initialDelay = 2f; // Sahne açıldıktan sonra bekleme süresi
    [SerializeField] private float shuffleAnimationDuration = 1.5f; // Karıştırma animasyonu süresi
    [SerializeField] private AnimationCurve shuffleAnimationCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    [Header("Progress (Fallback)")]
    [SerializeField] private string chapterId = "chapter_1";
    [SerializeField] private int levelIndex;
    [SerializeField] private int levelsInChapter = 20;
    [SerializeField] private PuzzleLevelConfig[] fallbackChapterLevels;
    
    private readonly List<PuzzlePiece> puzzlePieces = new List<PuzzlePiece>();
    private readonly List<Vector2> slotPositions = new List<Vector2>();
    private readonly List<SpriteRenderer> slotRenderers = new List<SpriteRenderer>();
    private readonly List<PuzzleLevelConfig> chapterLevels = new List<PuzzleLevelConfig>();
    private PuzzlePiece[] slotOccupants;
    private Vector2 pieceSizePixels;
    private Vector2 pieceSizeWorld;
    private Vector2 startPosition;
    private float slotSnapRadius;
    private float runtimePixelsPerUnit;
    private int totalPieces;
    private int placedPieces;
    
    private void Start()
    {
        ApplySelectionContext();
        
        if (!ValidateReferences())
        {
            return;
        }
        
        CreatePuzzle();
    }

    private void ApplySelectionContext()
    {
        if (LevelSelectionContext.HasSelection)
        {
            activeLevelConfig = LevelSelectionContext.LevelConfig;
            if (activeLevelConfig != null)
            {
                chapterId = activeLevelConfig.chapterId;
                levelIndex = activeLevelConfig.levelIndex;
                levelsInChapter = activeLevelConfig.totalLevelsInChapter;
                puzzleImage = activeLevelConfig.puzzleImage;
                gridSizeX = activeLevelConfig.gridSizeX;
                gridSizeY = activeLevelConfig.gridSizeY;
                SetChapterLevels(LevelSelectionContext.ChapterLevels);
            }
            else
            {
                Debug.LogWarning("[PuzzleManager] Level config bulunamadı, default değerlere dönüyoruz.");
                UseDefaultConfig();
            }
            LevelSelectionContext.Clear();
        }
        else
        {
            UseDefaultConfig();
        }
    }

    private void UseDefaultConfig()
    {
        activeLevelConfig = defaultLevelConfig;
        if (activeLevelConfig == null)
        {
            return;
        }

        chapterId = activeLevelConfig.chapterId;
        levelIndex = activeLevelConfig.levelIndex;
        levelsInChapter = activeLevelConfig.totalLevelsInChapter;
        puzzleImage = activeLevelConfig.puzzleImage;
        gridSizeX = activeLevelConfig.gridSizeX;
        gridSizeY = activeLevelConfig.gridSizeY;
        SetChapterLevels(fallbackChapterLevels);
    }
    
    private bool ValidateReferences()
    {
        if (puzzleImage == null)
        {
            Debug.LogError("Puzzle Image is not assigned!");
            return false;
        }
        
        if (puzzlePiecePrefab == null)
        {
            Debug.LogError("Puzzle Piece Prefab is not assigned!");
            return false;
        }
        
        return true;
    }
    
    public void CreatePuzzle()
    {
        if (!ValidateReferences())
        {
            return;
        }
        
        ClearPuzzle();
        
        gridSizeX = Mathf.Max(2, gridSizeX);
        gridSizeY = Mathf.Max(2, gridSizeY);
        totalPieces = gridSizeX * gridSizeY;
        placedPieces = 0;
        
        pieceSizePixels = new Vector2(puzzleImage.width / (float)gridSizeX, puzzleImage.height / (float)gridSizeY);
        pieceSizeWorld = pieceSizePixels / pixelsPerUnit;
        runtimePixelsPerUnit = pixelsPerUnit;

        float totalWidth = (gridSizeX * pieceSizeWorld.x) + ((gridSizeX - 1) * pieceSpacing);
        float totalHeight = (gridSizeY * pieceSizeWorld.y) + ((gridSizeY - 1) * pieceSpacing);

        float desiredHeight = GetDesiredWorldHeight(totalHeight);
        if (desiredHeight > 0f && totalHeight > 0f)
        {
            float scaleFactor = desiredHeight / totalHeight;
            pieceSizeWorld *= scaleFactor;
            if (scaleFactor > 0f)
            {
                runtimePixelsPerUnit = pixelsPerUnit / scaleFactor;
            }
            totalHeight = desiredHeight;
        }

        slotSnapRadius = Mathf.Min(pieceSizeWorld.x, pieceSizeWorld.y) * slotSnapMultiplier;
        totalWidth = (gridSizeX * pieceSizeWorld.x) + ((gridSizeX - 1) * pieceSpacing);
        totalHeight = (gridSizeY * pieceSizeWorld.y) + ((gridSizeY - 1) * pieceSpacing);
        startPosition = new Vector2(-totalWidth / 2f + pieceSizeWorld.x / 2f, totalHeight / 2f - pieceSizeWorld.y / 2f);
        
        slotPositions.Clear();
        ClearSlotPlaceholders();
        
        for (int y = 0; y < gridSizeY; y++)
        {
            for (int x = 0; x < gridSizeX; x++)
            {
                Vector2 slotPosition = CalculateSlotPosition(x, y);
                slotPositions.Add(slotPosition);
                slotRenderers.Add(CreateSlotPlaceholder(x, y, slotPosition));
                CreatePuzzlePiece(x, y);
            }
        }
        
        slotOccupants = new PuzzlePiece[slotPositions.Count];
        
        // Önce parçaları doğru yerlerine yerleştir
        PlacePiecesInCorrectPositions();
        
        // Sonra animasyonlu karıştır
        if (enableShuffleAnimation)
        {
            StartCoroutine(AnimatedShuffle());
        }
        else
        {
            ShufflePieces();
        }
        
        if (puzzleUI != null)
        {
            puzzleUI.HideCompletionPanel();
        }
    }
    
    private void PlacePiecesInCorrectPositions()
    {
        // Tüm parçaları doğru yerlerine yerleştir
        for (int i = 0; i < puzzlePieces.Count; i++)
        {
            PuzzlePiece piece = puzzlePieces[i];
            if (piece != null)
            {
                int correctSlotIndex = piece.CorrectSlotIndex;
                if (correctSlotIndex >= 0 && correctSlotIndex < slotPositions.Count)
                {
                    AssignPieceToSlot(piece, correctSlotIndex, -1, true);
                }
            }
        }
    }
    
    private System.Collections.IEnumerator AnimatedShuffle()
    {
        // Başlangıç gecikmesi (Inspector'dan ayarlanabilir)
        yield return new WaitForSeconds(initialDelay);
        
        // Kısa bir bekleme (parçaların doğru yerlerinde görünmesi için)
        yield return new WaitForSeconds(0.3f);
        
        // Karıştırma için slot index'lerini hazırla
        List<int> availableSlots = new List<int>(slotPositions.Count);
        for (int i = 0; i < slotPositions.Count; i++)
        {
            availableSlots.Add(i);
        }
        
        // Her parçaya rastgele bir slot atamak için parça-slot eşleştirmesi oluştur
        Dictionary<PuzzlePiece, int> pieceToSlotMap = new Dictionary<PuzzlePiece, int>();
        List<PuzzlePiece> shuffledPieces = new List<PuzzlePiece>(puzzlePieces);
        
        // Parçaları da rastgele sırala (daha fazla rastgelelik için)
        for (int i = shuffledPieces.Count - 1; i > 0; i--)
        {
            int swapIndex = Random.Range(0, i + 1);
            PuzzlePiece temp = shuffledPieces[i];
            shuffledPieces[i] = shuffledPieces[swapIndex];
            shuffledPieces[swapIndex] = temp;
        }
        
        // Her parçaya rastgele bir slot ata (her slot'a sadece bir parça gidecek)
        for (int i = 0; i < shuffledPieces.Count && availableSlots.Count > 0; i++)
        {
            PuzzlePiece piece = shuffledPieces[i];
            if (piece != null)
            {
                int randomSlotIndex = Random.Range(0, availableSlots.Count);
                int assignedSlot = availableSlots[randomSlotIndex];
                availableSlots.RemoveAt(randomSlotIndex);
                pieceToSlotMap[piece] = assignedSlot;
            }
        }
        
        // Her parçayı animasyonlu olarak hedef slotuna taşı
        List<System.Collections.IEnumerator> animations = new List<System.Collections.IEnumerator>();
        
        foreach (var kvp in pieceToSlotMap)
        {
            PuzzlePiece piece = kvp.Key;
            int targetSlotIndex = kvp.Value;
            
            if (piece != null && targetSlotIndex >= 0 && targetSlotIndex < slotPositions.Count)
            {
                Vector2 targetPosition = slotPositions[targetSlotIndex];
                Vector2 startPosition = piece.transform.position;
                
                // Her parça için rastgele animasyon süresi (daha dinamik görünüm için)
                float randomDuration = shuffleAnimationDuration + Random.Range(-0.3f, 0.3f);
                randomDuration = Mathf.Max(0.5f, randomDuration); // Minimum 0.5 saniye
                
                animations.Add(AnimatePieceToPosition(piece, startPosition, targetPosition, targetSlotIndex, randomDuration));
            }
        }
        
        // Tüm animasyonları paralel olarak çalıştır
        yield return StartCoroutine(RunAnimationsInParallel(animations));
        
        // Animasyon bitince slot atamalarını yap
        foreach (var kvp in pieceToSlotMap)
        {
            PuzzlePiece piece = kvp.Key;
            int targetSlotIndex = kvp.Value;
            
            if (piece != null && targetSlotIndex >= 0 && targetSlotIndex < slotPositions.Count)
            {
                AssignPieceToSlot(piece, targetSlotIndex, -1, true);
            }
        }
    }
    
    private System.Collections.IEnumerator AnimatePieceToPosition(PuzzlePiece piece, Vector2 startPos, Vector2 targetPos, int targetSlotIndex, float duration)
    {
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float curveValue = shuffleAnimationCurve.Evaluate(t);
            
            Vector2 currentPos = Vector2.Lerp(startPos, targetPos, curveValue);
            piece.MoveToPosition(currentPos);
            
            yield return null;
        }
        
        // Son pozisyonu garanti et
        piece.MoveToPosition(targetPos);
    }
    
    private System.Collections.IEnumerator RunAnimationsInParallel(List<System.Collections.IEnumerator> animations)
    {
        // Tüm animasyonları paralel olarak çalıştır
        while (animations.Count > 0)
        {
            for (int i = animations.Count - 1; i >= 0; i--)
            {
                if (!animations[i].MoveNext())
                {
                    animations.RemoveAt(i);
                }
            }
            yield return null;
        }
    }
    
    private void CreatePuzzlePiece(int gridX, int gridY)
    {
        Sprite pieceSprite = CreateSpriteFromTexture(gridX, gridY);
        
        GameObject pieceObj = Instantiate(puzzlePiecePrefab, transform);
        pieceObj.name = $"PuzzlePiece_{gridX}_{gridY}";
        
        SpriteRenderer spriteRenderer = pieceObj.GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = pieceObj.AddComponent<SpriteRenderer>();
        }
        spriteRenderer.sprite = pieceSprite;
        
        PuzzlePiece puzzlePiece = pieceObj.GetComponent<PuzzlePiece>();
        if (puzzlePiece == null)
        {
            puzzlePiece = pieceObj.AddComponent<PuzzlePiece>();
        }
        
        puzzlePiece.Initialize(this, gridX, gridY);
        puzzlePieces.Add(puzzlePiece);
    }
    
    private Sprite CreateSpriteFromTexture(int gridX, int gridY)
    {
        int pieceWidth = Mathf.FloorToInt(pieceSizePixels.x);
        int pieceHeight = Mathf.FloorToInt(pieceSizePixels.y);

        int startX = Mathf.FloorToInt(gridX * pieceSizePixels.x);
        int startY = Mathf.FloorToInt((gridSizeY - 1 - gridY) * pieceSizePixels.y);

        Rect rect = new Rect(startX, startY, pieceWidth, pieceHeight);
        float pixelsPerUnitToUse = runtimePixelsPerUnit > 0f ? runtimePixelsPerUnit : pixelsPerUnit;

        return Sprite.Create(
            puzzleImage,
            rect,
            new Vector2(0.5f, 0.5f),
            pixelsPerUnitToUse
        );
    }
    
    private Vector2 CalculateSlotPosition(int gridX, int gridY)
    {
        float x = startPosition.x + (gridX * (pieceSizeWorld.x + pieceSpacing));
        float y = startPosition.y - (gridY * (pieceSizeWorld.y + pieceSpacing));
        return new Vector2(x, y);
    }
    
    private void ShufflePieces()
    {
        List<int> slotIndices = new List<int>(slotPositions.Count);
        for (int i = 0; i < slotPositions.Count; i++)
        {
            slotIndices.Add(i);
        }
        
        for (int i = slotIndices.Count - 1; i > 0; i--)
        {
            int swapIndex = Random.Range(0, i + 1);
            (slotIndices[i], slotIndices[swapIndex]) = (slotIndices[swapIndex], slotIndices[i]);
        }
        
        for (int i = 0; i < puzzlePieces.Count; i++)
        {
            AssignPieceToSlot(puzzlePieces[i], slotIndices[i], -1, true);
        }
    }
    
    private bool AssignPieceToSlot(PuzzlePiece piece, int targetSlotIndex, int previousSlotIndex, bool suppressSwap = false)
    {
        if (piece == null || targetSlotIndex < 0 || targetSlotIndex >= slotPositions.Count)
        {
            return false;
        }
        
        PuzzlePiece occupyingPiece = slotOccupants[targetSlotIndex];
        
        if (!suppressSwap)
        {
            if (occupyingPiece != null && occupyingPiece != piece)
            {
                // Eğer hedef slotta doğru yere yerleşmiş ve kilitlenmiş bir parça varsa,
                // o parçayı yerinden oynatmaya izin vermiyoruz.
                if (occupyingPiece.IsPlaced)
                {
                    return false;
                }

                if (previousSlotIndex < 0 || previousSlotIndex >= slotPositions.Count)
                {
                    return false;
                }
                
                slotOccupants[previousSlotIndex] = occupyingPiece;
                occupyingPiece.SetSlot(previousSlotIndex, slotPositions[previousSlotIndex]);
                UpdatePlacementState(occupyingPiece);
                RefreshSlotPlaceholder(previousSlotIndex);
            }
            else if (previousSlotIndex >= 0 && previousSlotIndex < slotPositions.Count)
            {
                slotOccupants[previousSlotIndex] = null;
                RefreshSlotPlaceholder(previousSlotIndex);
            }
        }
        
        slotOccupants[targetSlotIndex] = piece;
        piece.SetSlot(targetSlotIndex, slotPositions[targetSlotIndex]);
        RefreshSlotPlaceholder(targetSlotIndex);
        UpdatePlacementState(piece);
        return true;
    }
    
    public void ReleaseSlot(int slotIndex)
    {
        if (slotOccupants == null || slotIndex < 0 || slotIndex >= slotOccupants.Length)
        {
            return;
        }
        
        slotOccupants[slotIndex] = null;
        RefreshSlotPlaceholder(slotIndex);
    }
    
    public bool TrySnapPieceToSlot(PuzzlePiece piece, Vector2 worldPosition)
    {
        int slotIndex = GetNearestSlotIndex(worldPosition);
        if (slotIndex == -1)
        {
            return false;
        }
        
        return AssignPieceToSlot(piece, slotIndex, piece.LastSlotIndex, false);
    }
    
    public void RestorePieceToPreviousSlot(PuzzlePiece piece)
    {
        int previousIndex = piece.LastSlotIndex;
        if (previousIndex < 0 || previousIndex >= slotPositions.Count)
        {
            piece.MoveToPosition(slotPositions[Mathf.Clamp(piece.CorrectSlotIndex, 0, slotPositions.Count - 1)]);
            return;
        }
        
        AssignPieceToSlot(piece, previousIndex, -1, true);
    }
    
    private int GetNearestSlotIndex(Vector2 worldPosition)
    {
        float closestDistance = float.MaxValue;
        int closestIndex = -1;
        
        for (int i = 0; i < slotPositions.Count; i++)
        {
            float distance = Vector2.Distance(worldPosition, slotPositions[i]);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestIndex = i;
            }
        }
        
        if (closestDistance > slotSnapRadius)
        {
            return -1;
        }
        
        return closestIndex;
    }
    
    private void UpdatePlacementState(PuzzlePiece piece)
    {
        bool shouldLock = piece.CurrentSlotIndex == piece.CorrectSlotIndex;
        bool changed = piece.SetPlacementState(shouldLock);
        
        if (!changed)
        {
            return;
        }
        
        if (shouldLock)
        {
            placedPieces++;
            PlaySnapSound();
            
            if (placedPieces >= totalPieces)
            {
                OnPuzzleCompleted();
            }
        }
        else
        {
            placedPieces = Mathf.Max(0, placedPieces - 1);
        }
    }

    private float GetDesiredWorldHeight(float currentHeight)
    {
        if (activeLevelConfig != null && activeLevelConfig.targetWorldHeight > 0f)
        {
            return activeLevelConfig.targetWorldHeight;
        }

        return currentHeight;
    }
    
    private void PlaySnapSound()
    {
        if (audioSource != null && snapSound != null)
        {
            audioSource.PlayOneShot(snapSound);
        }
    }
    
    private void OnPuzzleCompleted()
    {
        Debug.Log("[PuzzleManager] Puzzle Completed!");
        if (!string.IsNullOrEmpty(chapterId))
        {
            LevelProgress.MarkLevelComplete(chapterId, levelIndex, levelsInChapter);
        }
        
        if (puzzleUI != null)
        {
            puzzleUI.ShowCompletionPanel(HasNextLevel());
        }
    }

    public bool HasNextLevel()
    {
        return GetNextLevelConfig() != null;
    }

    public void LoadNextLevel()
    {
        PuzzleLevelConfig nextConfig = GetNextLevelConfig();
        if (nextConfig == null)
        {
            Debug.LogWarning("[PuzzleManager] Sonraki level bulunamadı.");
            return;
        }

        LevelSelectionContext.SetSelection(
            nextConfig.chapterId,
            nextConfig.levelIndex,
            nextConfig,
            chapterLevels);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    public void ClearPuzzle()
    {
        foreach (PuzzlePiece piece in puzzlePieces)
        {
            if (piece != null)
            {
                Destroy(piece.gameObject);
            }
        }
        puzzlePieces.Clear();
        
        slotOccupants = null;
        slotPositions.Clear();
        ClearSlotPlaceholders();
        placedPieces = 0;
    }
    
    private void ClearSlotPlaceholders()
    {
        foreach (SpriteRenderer renderer in slotRenderers)
        {
            if (renderer != null)
            {
                Destroy(renderer.gameObject);
            }
        }
        slotRenderers.Clear();
    }
    
    private SpriteRenderer CreateSlotPlaceholder(int gridX, int gridY, Vector2 position)
    {
        if (slotPlaceholderPrefab == null)
        {
            return null;
        }
        
        GameObject placeholder = Instantiate(slotPlaceholderPrefab, new Vector3(position.x, position.y, 0f), Quaternion.identity, transform);
        placeholder.name = $"Slot_{gridX}_{gridY}";
        placeholder.transform.localScale = new Vector3(pieceSizeWorld.x, pieceSizeWorld.y, 1f);
        
        SpriteRenderer renderer = placeholder.GetComponent<SpriteRenderer>();
        if (renderer == null)
        {
            renderer = placeholder.AddComponent<SpriteRenderer>();
        }
        
        Color color = renderer.color;
        color.a = 0.15f;
        renderer.color = color;
        return renderer;
    }
    
    private void RefreshSlotPlaceholder(int slotIndex)
    {
        if (slotRenderers == null || slotIndex < 0 || slotIndex >= slotRenderers.Count)
        {
            return;
        }
        
        SpriteRenderer renderer = slotRenderers[slotIndex];
        if (renderer == null)
        {
            return;
        }
        
        Color color = renderer.color;
        color.a = slotOccupants != null && slotOccupants[slotIndex] == null ? 0.15f : 0f;
        renderer.color = color;
    }
    
    public int GetSlotIndex(int gridX, int gridY)
    {
        return (gridY * gridSizeX) + gridX;
    }
    
    public Vector2 GetSlotPosition(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= slotPositions.Count)
        {
            return Vector2.zero;
        }
        
        return slotPositions[slotIndex];
    }

    private void SetChapterLevels(IReadOnlyList<PuzzleLevelConfig> configs)
    {
        chapterLevels.Clear();

        if (configs != null)
        {
            for (int i = 0; i < configs.Count; i++)
            {
                PuzzleLevelConfig config = configs[i];
                if (config == null)
                {
                    continue;
                }

                if (!string.Equals(config.chapterId, chapterId, StringComparison.Ordinal))
                {
                    continue;
                }

                chapterLevels.Add(config);
            }
        }

        if ((configs == null || chapterLevels.Count == 0) && fallbackChapterLevels != null)
        {
            foreach (PuzzleLevelConfig config in fallbackChapterLevels)
            {
                if (config == null)
                {
                    continue;
                }

                if (!string.Equals(config.chapterId, chapterId, StringComparison.Ordinal))
                {
                    continue;
                }

                chapterLevels.Add(config);
            }
        }

        if (chapterLevels.Count == 0 && activeLevelConfig != null)
        {
            chapterLevels.Add(activeLevelConfig);
        }
    }

    private PuzzleLevelConfig GetNextLevelConfig()
    {
        if (chapterLevels == null || chapterLevels.Count == 0)
        {
            return null;
        }

        int targetLevelIndex = levelIndex + 1;
        foreach (PuzzleLevelConfig config in chapterLevels)
        {
            if (config == null)
            {
                continue;
            }

            if (!string.Equals(config.chapterId, chapterId, StringComparison.Ordinal))
            {
                continue;
            }

            if (config.levelIndex == targetLevelIndex)
            {
                return config;
            }
        }

        return null;
    }
}




