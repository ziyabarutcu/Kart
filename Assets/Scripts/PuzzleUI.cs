using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class PuzzleUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject completionPanel;
    [SerializeField] private TMP_Text completionText;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button nextLevelButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button overlayMainMenuButton;
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private PuzzleManager puzzleManager;
    
    private void Awake()
    {
        puzzleManager = FindFirstObjectByType<PuzzleManager>();

        // Panel başlangıçta kapalı
        if (completionPanel != null)
        {
            completionPanel.SetActive(false);
        }
        
        // Restart butonu varsa event ekle
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(OnRestartClicked);
        }

        if (nextLevelButton != null)
        {
            nextLevelButton.onClick.AddListener(OnNextLevelClicked);
            nextLevelButton.gameObject.SetActive(false);
        }

        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.AddListener(OnMainMenuClicked);
            mainMenuButton.interactable = true; // Aktif yap
            mainMenuButton.gameObject.SetActive(false); // Başlangıçta gizle
        }

        // Overlay MainMenu butonunu görünür yap ve event ekle
        if (overlayMainMenuButton != null)
        {
            overlayMainMenuButton.onClick.AddListener(OnMainMenuClicked);
            overlayMainMenuButton.gameObject.SetActive(true);
            overlayMainMenuButton.interactable = true; // Oyun esnasında aktif
            
            // Butonun üstte olmasını sağla (RectTransform anchor ayarları)
            RectTransform rectTransform = overlayMainMenuButton.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                // Anchor'ları üst ortaya ayarla
                rectTransform.anchorMin = new Vector2(0.5f, 1f);
                rectTransform.anchorMax = new Vector2(0.5f, 1f);
                rectTransform.pivot = new Vector2(0.5f, 1f);
                // Üstten biraz aşağı (örneğin 50 piksel)
                rectTransform.anchoredPosition = new Vector2(0f, -50f);
            }
        }
    }
    
    private void Start()
    {
        // Start'ta da kontrol et (bazı durumlarda Awake çok erken olabilir)
        if (overlayMainMenuButton != null)
        {
            overlayMainMenuButton.gameObject.SetActive(true);
            overlayMainMenuButton.interactable = true;
        }
        
        if (mainMenuButton != null)
        {
            mainMenuButton.gameObject.SetActive(false);
            mainMenuButton.interactable = true;
        }
    }
    
    public void ShowCompletionPanel(bool canGoToNextLevel)
    {
        if (completionPanel != null)
        {
            completionPanel.SetActive(true);
        }
        
        if (completionText != null)
        {
            completionText.text = "Puzzle Completed!";
        }

        if (nextLevelButton != null)
        {
            nextLevelButton.gameObject.SetActive(canGoToNextLevel);
            nextLevelButton.interactable = canGoToNextLevel;
        }
        
        // Completion panel açıldığında üstteki overlay menu butonunu inaktif yap
        if (overlayMainMenuButton != null)
        {
            overlayMainMenuButton.interactable = false; // Inaktif yap ama görünür kalsın (veya gizle)
            overlayMainMenuButton.gameObject.SetActive(false); // Gizle
        }
        
        // Completion panel'deki mainMenuButton'u göster ve aktif yap
        if (mainMenuButton != null)
        {
            mainMenuButton.gameObject.SetActive(true);
            mainMenuButton.interactable = true; // Aktif yap
        }
    }
    
    public void HideCompletionPanel()
    {
        if (completionPanel != null)
        {
            completionPanel.SetActive(false);
        }

        if (nextLevelButton != null)
        {
            nextLevelButton.gameObject.SetActive(false);
        }
        
        // Completion panel kapandığında üstteki overlay menu butonunu aktif yap ve göster
        if (overlayMainMenuButton != null)
        {
            overlayMainMenuButton.gameObject.SetActive(true);
            overlayMainMenuButton.interactable = true; // Aktif yap
        }
        
        // Completion panel'deki mainMenuButton'u gizle
        if (mainMenuButton != null)
        {
            mainMenuButton.gameObject.SetActive(false);
        }
    }
    
    private void OnRestartClicked()
    {
        // Restart butonuna tıklandığında titreşim
        VibrationManager.Vibrate(VibrationType.Medium, 0.1f);
        
        if (puzzleManager != null)
        {
            puzzleManager.CreatePuzzle();
        }
    }

    private void OnNextLevelClicked()
    {
        // Next Level butonuna tıklandığında titreşim
        VibrationManager.Vibrate(VibrationType.Medium, 0.1f);
        
        puzzleManager?.LoadNextLevel();
    }

    private void OnMainMenuClicked()
    {
        // Main Menu butonuna tıklandığında titreşim
        VibrationManager.Vibrate(VibrationType.Medium, 0.1f);
        
        if (string.IsNullOrWhiteSpace(mainMenuSceneName))
        {
            return;
        }

        SceneManager.LoadScene(mainMenuSceneName);
    }
}

