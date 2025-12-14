using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class PuzzlePiece : MonoBehaviour
{
    private PuzzleManager puzzleManager;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private SpriteRenderer frameRenderer;
    [SerializeField] private SpriteMask spriteMask;
    [SerializeField] private Color defaultFrameColor = Color.white;
    [SerializeField] private Color placedFrameColor = new Color(1f, 1f, 1f, 0f);
    private Camera mainCamera;
    
    private Vector2Int correctGridIndex;
    private int correctSlotIndex = -1;
    private int currentSlotIndex = -1;
    private int lastSlotIndex = -1;
    
    private bool isPlaced;
    private bool isDragging;
    private int originalSortingOrder;
    private Vector3 pointerOffset;
    
    private Vector2 pointerScreenPosition;
    private bool pointerDownThisFrame;
    private bool pointerHeld;
    private bool pointerUpThisFrame;
    
    // Global drag state - aynı anda sadece bir parça sürüklenebilir
    private static PuzzlePiece currentlyDraggingPiece = null;
    
    public int CurrentSlotIndex => currentSlotIndex;
    public int LastSlotIndex => lastSlotIndex;
    public int CorrectSlotIndex => correctSlotIndex;
    public bool IsPlaced => isPlaced;
    
    private void Awake()
    {
        mainCamera = Camera.main ?? FindFirstObjectByType<Camera>();
        
        if (spriteRenderer != null)
        {
            originalSortingOrder = spriteRenderer.sortingOrder;
        }

        if (frameRenderer != null)
        {
            frameRenderer.color = defaultFrameColor;
        }

        // Başlangıçta parçalar kilitli değil, mask aktif olsun
        if (spriteMask != null && spriteRenderer != null)
        {
            spriteMask.enabled = true;
            spriteRenderer.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
        }
    }
    
    public void Initialize(PuzzleManager manager, int gridX, int gridY)
    {
        puzzleManager = manager;
        correctGridIndex = new Vector2Int(gridX, gridY);
        correctSlotIndex = puzzleManager != null ? puzzleManager.GetSlotIndex(gridX, gridY) : -1;
        isPlaced = false;
        isDragging = false;
        currentSlotIndex = -1;
        lastSlotIndex = -1;

        if (frameRenderer != null)
        {
            frameRenderer.color = defaultFrameColor;

            // Çerçeveyi parçanın boyutuna otomatik uydur
            if (spriteRenderer != null && frameRenderer.sprite != null)
            {
                Vector2 frameSize = frameRenderer.sprite.bounds.size;
                Vector2 pieceSize = spriteRenderer.sprite.bounds.size;

                if (frameSize.x > 0f && frameSize.y > 0f)
                {
                    Vector3 scale = new Vector3(
                        pieceSize.x / frameSize.x,
                        pieceSize.y / frameSize.y,
                        1f);
                    frameRenderer.transform.localScale = scale;
                }
            }
        }

        if (spriteMask != null && spriteMask.sprite != null && spriteRenderer != null)
        {
            Vector2 maskSize = spriteMask.sprite.bounds.size;
            Vector2 pieceSize = spriteRenderer.sprite.bounds.size;

            if (maskSize.x > 0f && maskSize.y > 0f)
            {
                Vector3 scale = new Vector3(
                    pieceSize.x / maskSize.x,
                    pieceSize.y / maskSize.y,
                    1f);
                spriteMask.transform.localScale = scale;
            }

            spriteMask.enabled = true;
            spriteRenderer.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
        }
    }
    
    private void Update()
    {
        if (isPlaced)
        {
            // Eğer parça yerleştirilmişse ve hala sürükleniyorsa, sürüklemeyi durdur
            if (isDragging)
            {
                ForceEndDrag();
            }
            return;
        }
        
        UpdatePointerState();
        
        // Eğer bu parça sürükleniyorsa, sürükleme işlemlerini yap
        if (isDragging)
        {
            // Parça bırakıldı mı kontrol et
            if (pointerUpThisFrame || !pointerHeld)
            {
                EndDrag();
            }
            else if (pointerHeld)
            {
                Drag(pointerScreenPosition);
            }
        }
        else
        {
            // Sadece başka bir parça sürüklenmiyorsa yeni sürükleme başlat
            if (pointerDownThisFrame && currentlyDraggingPiece == null)
            {
                TryBeginDrag();
            }
        }
    }
    
    private void TryBeginDrag()
    {
        if (mainCamera == null)
        {
            return;
        }
        
        // Eğer başka bir parça zaten sürükleniyorsa, yeni sürükleme başlatma
        if (currentlyDraggingPiece != null && currentlyDraggingPiece != this)
        {
            return;
        }
        
        Vector2 worldPoint = ScreenToWorld(pointerScreenPosition);
        if (IsPointOverPiece(worldPoint))
        {
            StartDragging(worldPoint);
        }
    }
    
    private void StartDragging(Vector2 worldPoint)
    {
        // Eğer başka bir parça sürükleniyorsa, önce onu bırak
        if (currentlyDraggingPiece != null && currentlyDraggingPiece != this)
        {
            currentlyDraggingPiece.ForceEndDrag();
        }
        
        isDragging = true;
        currentlyDraggingPiece = this;
        pointerOffset = transform.position - (Vector3)worldPoint;
        
        if (currentSlotIndex >= 0 && puzzleManager != null)
        {
            lastSlotIndex = currentSlotIndex;
            puzzleManager.ReleaseSlot(currentSlotIndex);
            currentSlotIndex = -1;
        }
        
        if (spriteRenderer != null)
        {
            spriteRenderer.sortingOrder = 500;
        }
        
        // Kart tutulduğunda hafif titreşim
        VibrationManager.Vibrate(VibrationType.Light, 0.05f);
    }
    
    private void Drag(Vector2 screenPoint)
    {
        if (mainCamera == null)
        {
            return;
        }
        
        Vector3 worldPoint = ScreenToWorld(screenPoint);
        transform.position = worldPoint + pointerOffset;
    }
    
    private void EndDrag()
    {
        if (!isDragging)
        {
            return;
        }
        
        // Önce sürükleme durumunu temizle
        isDragging = false;
        if (currentlyDraggingPiece == this)
        {
            currentlyDraggingPiece = null;
        }
        
        if (spriteRenderer != null)
        {
            spriteRenderer.sortingOrder = originalSortingOrder;
        }
        
        // Sonra snap işlemini yap
        bool snapped = puzzleManager != null && puzzleManager.TrySnapPieceToSlot(this, transform.position);
        if (!snapped && puzzleManager != null)
        {
            puzzleManager.RestorePieceToPreviousSlot(this);
        }
    }
    
    // Zorla sürüklemeyi sonlandır (başka bir parça sürüklenmeye başladığında)
    public void ForceEndDrag()
    {
        if (!isDragging)
        {
            return;
        }
        
        isDragging = false;
        if (currentlyDraggingPiece == this)
        {
            currentlyDraggingPiece = null;
        }
        
        if (spriteRenderer != null)
        {
            spriteRenderer.sortingOrder = originalSortingOrder;
        }
        
        // Parçayı önceki slotuna geri koy
        if (puzzleManager != null)
        {
            puzzleManager.RestorePieceToPreviousSlot(this);
        }
    }
    
    public void SetSlot(int slotIndex, Vector2 slotPosition)
    {
        currentSlotIndex = slotIndex;
        lastSlotIndex = slotIndex;
        MoveToPosition(slotPosition);
    }
    
    public void MoveToPosition(Vector2 slotPosition)
    {
        transform.position = new Vector3(slotPosition.x, slotPosition.y, transform.position.z);
    }
    
    public bool SetPlacementState(bool shouldLock)
    {
        if (isPlaced == shouldLock)
        {
            return false;
        }
        
        isPlaced = shouldLock;
        
        if (isPlaced)
        {
            isDragging = false;
            if (spriteRenderer != null)
            {
                spriteRenderer.sortingOrder = originalSortingOrder;
            }
            if (frameRenderer != null)
            {
                frameRenderer.color = placedFrameColor;
            }
            if (spriteMask != null && spriteRenderer != null)
            {
                // Doğru yere oturduğunda maske kapansın, köşeler keskinleşsin
                spriteMask.enabled = false;
                spriteRenderer.maskInteraction = SpriteMaskInteraction.None;
            }
        }
        else if (frameRenderer != null)
        {
            frameRenderer.color = defaultFrameColor;

            if (spriteMask != null && spriteRenderer != null)
            {
                spriteMask.enabled = true;
                spriteRenderer.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
            }
        }
        
        return true;
    }
    
    private Vector3 ScreenToWorld(Vector2 screenPoint)
    {
        Vector3 point = new Vector3(screenPoint.x, screenPoint.y, Mathf.Abs(mainCamera.transform.position.z));
        return mainCamera.ScreenToWorldPoint(point);
    }
    
    private bool IsPointOverPiece(Vector2 worldPoint)
    {
        return spriteRenderer != null && spriteRenderer.bounds.Contains(worldPoint);
    }
    
    private void UpdatePointerState()
    {
        pointerDownThisFrame = false;
        pointerHeld = false;
        pointerUpThisFrame = false;
        
#if ENABLE_INPUT_SYSTEM
        if (Touchscreen.current != null)
        {
            var touch = Touchscreen.current.primaryTouch;
            if (touch.press.isPressed || touch.press.wasPressedThisFrame || touch.press.wasReleasedThisFrame)
            {
                pointerScreenPosition = touch.position.ReadValue();
                pointerDownThisFrame = touch.press.wasPressedThisFrame;
                pointerUpThisFrame = touch.press.wasReleasedThisFrame;
                pointerHeld = touch.press.isPressed && !touch.press.wasReleasedThisFrame;
                return;
            }
        }
        
        if (Mouse.current != null)
        {
            pointerScreenPosition = Mouse.current.position.ReadValue();
            pointerDownThisFrame = Mouse.current.leftButton.wasPressedThisFrame;
            pointerHeld = Mouse.current.leftButton.isPressed;
            pointerUpThisFrame = Mouse.current.leftButton.wasReleasedThisFrame;
            return;
        }
#else
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            pointerScreenPosition = touch.position;
            pointerDownThisFrame = touch.phase == TouchPhase.Began;
            pointerHeld = touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary;
            pointerUpThisFrame = touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled;
            return;
        }
        
        if (Input.mousePresent)
        {
            pointerScreenPosition = Input.mousePosition;
            pointerDownThisFrame = Input.GetMouseButtonDown(0);
            pointerHeld = Input.GetMouseButton(0);
            pointerUpThisFrame = Input.GetMouseButtonUp(0);
            return;
        }
#endif
    }
}

