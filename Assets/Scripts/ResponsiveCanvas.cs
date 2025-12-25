using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Canvas'ı hem portrait (9:16) hem landscape (16:9) için responsive yapar.
/// Aspect ratio'ya göre Canvas Scaler ayarlarını dinamik olarak günceller.
/// </summary>
public class ResponsiveCanvas : MonoBehaviour
{
    [Header("Canvas Scaler Reference")]
    [SerializeField] private CanvasScaler canvasScaler;
    
    [Header("Responsive Settings")]
    [Tooltip("Portrait (9:16) için Match değeri")]
    [SerializeField] [Range(0f, 1f)] private float portraitMatch = 0.5f;
    
    [Tooltip("Landscape (16:9) için Match değeri")]
    [SerializeField] [Range(0f, 1f)] private float landscapeMatch = 0.3f;
    
    [Header("Reference Resolutions")]
    [Tooltip("Portrait için referans çözünürlük")]
    [SerializeField] private Vector2 portraitResolution = new Vector2(1080, 1920);
    
    [Tooltip("Landscape için referans çözünürlük")]
    [SerializeField] private Vector2 landscapeResolution = new Vector2(1920, 1080);
    
    [Header("Screen Match Mode")]
    [Tooltip("Screen Match Mode: 0=Match Width or Height, 1=Expand, 2=Shrink")]
    [SerializeField] private CanvasScaler.ScreenMatchMode screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
    
    private int lastWidth;
    private int lastHeight;
    private float lastAspectRatio;
    
    private void Awake()
    {
        if (canvasScaler == null)
        {
            canvasScaler = GetComponent<CanvasScaler>();
        }
        
        if (canvasScaler == null)
        {
            Debug.LogWarning("[ResponsiveCanvas] CanvasScaler bulunamadı!");
            enabled = false;
            return;
        }
        
        // İlk ayarlamayı yap
        AdjustForAspectRatio();
    }
    
    private void Start()
    {
        // Start'ta da kontrol et (bazı durumlarda Awake çok erken olabilir)
        AdjustForAspectRatio();
    }
    
    private void Update()
    {
        // Ekran boyutu değiştiğinde (web'de pencere yeniden boyutlandırıldığında)
        if (Screen.width != lastWidth || Screen.height != lastHeight)
        {
            AdjustForAspectRatio();
        }
    }
    
    private void AdjustForAspectRatio()
    {
        lastWidth = Screen.width;
        lastHeight = Screen.height;
        
        float aspectRatio = (float)Screen.width / Screen.height;
        lastAspectRatio = aspectRatio;
        
        // Portrait (dikey) veya Landscape (yatay) belirle
        bool isPortrait = aspectRatio < 1.0f;
        
        if (isPortrait)
        {
            // Portrait modu (9:16, mobil)
            canvasScaler.referenceResolution = portraitResolution;
            canvasScaler.matchWidthOrHeight = portraitMatch;
        }
        else
        {
            // Landscape modu (16:9, web)
            canvasScaler.referenceResolution = landscapeResolution;
            canvasScaler.matchWidthOrHeight = landscapeMatch;
        }
        
        // Screen Match Mode'u ayarla
        canvasScaler.screenMatchMode = screenMatchMode;
        
        Debug.Log($"[ResponsiveCanvas] Aspect Ratio: {aspectRatio:F2} ({Screen.width}x{Screen.height}), " +
                  $"Mode: {(isPortrait ? "Portrait" : "Landscape")}, " +
                  $"Match: {canvasScaler.matchWidthOrHeight:F2}, " +
                  $"Reference: {canvasScaler.referenceResolution}");
    }
    
    /// <summary>
    /// Manuel olarak aspect ratio'ya göre ayarlama yapmak için
    /// </summary>
    public void ForceUpdate()
    {
        AdjustForAspectRatio();
    }
}

