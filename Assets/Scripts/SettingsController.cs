using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class SettingsController : MonoBehaviour
{
    [Header("Canvas References")]
    [SerializeField] private Canvas settingsCanvas;
    [SerializeField] private Canvas mainCanvas; // Ana menü canvas'ı
    
    [Header("Buttons")]
    [SerializeField] private Button closeButton; // X butonu
    [SerializeField] private Button musicToggleButton; // Sol buton - Müzik
    [SerializeField] private Button soundEffectsToggleButton; // Orta buton - Ses efektleri
    [SerializeField] private Button vibrationToggleButton; // Sağ buton - Titreşim
    
    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer audioMixer; // AudioMixer asset'i
    [SerializeField] private string musicVolumeParameter = "MusicVolume"; // Müzik için mixer parametresi
    [SerializeField] private string soundEffectsVolumeParameter = "SoundEffectsVolume"; // Ses efektleri için mixer parametresi
    
    [Header("Settings Keys")]
    private const string MUSIC_ENABLED_KEY = "MusicEnabled";
    private const string SOUND_EFFECTS_ENABLED_KEY = "SoundEffectsEnabled";
    
    private bool isSettingsOpen = false;
    
    private void Awake()
    {
        // Settings canvas'ı başlangıçta gizle
        if (settingsCanvas != null)
        {
            settingsCanvas.gameObject.SetActive(false);
        }
        
        // Buton event'lerini ayarla
        SetupButtons();
        
        // Kaydedilmiş ayarları yükle
        LoadSettings();
    }
    
    private void SetupButtons()
    {
        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(CloseSettings);
        }
        
        if (musicToggleButton != null)
        {
            musicToggleButton.onClick.RemoveAllListeners();
            musicToggleButton.onClick.AddListener(ToggleMusic);
        }
        
        if (soundEffectsToggleButton != null)
        {
            soundEffectsToggleButton.onClick.RemoveAllListeners();
            soundEffectsToggleButton.onClick.AddListener(ToggleSoundEffects);
        }
        
        if (vibrationToggleButton != null)
        {
            vibrationToggleButton.onClick.RemoveAllListeners();
            vibrationToggleButton.onClick.AddListener(ToggleVibration);
        }
    }
    
    public void OpenSettings()
    {
        if (settingsCanvas != null)
        {
            settingsCanvas.gameObject.SetActive(true);
            isSettingsOpen = true;
            
            // Ana canvas'ı tıklanamaz hale getir (GraphicRaycaster'ı devre dışı bırak)
            DisableMainCanvasInteraction();
        }
    }
    
    public void CloseSettings()
    {
        // X butonuna tıklandığında titreşim
        VibrationManager.Vibrate(VibrationType.Medium, 0.1f);
        
        if (settingsCanvas != null)
        {
            settingsCanvas.gameObject.SetActive(false);
            isSettingsOpen = false;
            
            // Ana canvas'ı tekrar tıklanabilir hale getir
            EnableMainCanvasInteraction();
        }
    }
    
    private void DisableMainCanvasInteraction()
    {
        if (mainCanvas != null)
        {
            // Canvas'ın GraphicRaycaster component'ini devre dışı bırak
            GraphicRaycaster raycaster = mainCanvas.GetComponent<GraphicRaycaster>();
            if (raycaster != null)
            {
                raycaster.enabled = false;
            }
            
            // Alternatif olarak, CanvasGroup kullanarak tüm etkileşimi kapatabiliriz
            CanvasGroup canvasGroup = mainCanvas.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = mainCanvas.gameObject.AddComponent<CanvasGroup>();
            }
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false; // Raycast'i engelle ama görünürlüğü koru
        }
    }
    
    private void EnableMainCanvasInteraction()
    {
        if (mainCanvas != null)
        {
            // Canvas'ın GraphicRaycaster component'ini tekrar aktif et
            GraphicRaycaster raycaster = mainCanvas.GetComponent<GraphicRaycaster>();
            if (raycaster != null)
            {
                raycaster.enabled = true;
            }
            
            // CanvasGroup'u tekrar aktif et
            CanvasGroup canvasGroup = mainCanvas.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
            }
        }
    }
    
    private void ToggleMusic()
    {
        // Butona tıklandığında titreşim
        VibrationManager.Vibrate(VibrationType.Medium, 0.1f);
        
        bool currentState = PlayerPrefs.GetInt(MUSIC_ENABLED_KEY, 1) == 1;
        bool newState = !currentState;
        
        PlayerPrefs.SetInt(MUSIC_ENABLED_KEY, newState ? 1 : 0);
        PlayerPrefs.Save();
        
        ApplyMusicSettings(newState);
        
        Debug.Log($"[SettingsController] Müzik: {(newState ? "Açık" : "Kapalı")}");
    }
    
    private void ToggleSoundEffects()
    {
        // Butona tıklandığında titreşim
        VibrationManager.Vibrate(VibrationType.Medium, 0.1f);
        
        bool currentState = PlayerPrefs.GetInt(SOUND_EFFECTS_ENABLED_KEY, 1) == 1;
        bool newState = !currentState;
        
        PlayerPrefs.SetInt(SOUND_EFFECTS_ENABLED_KEY, newState ? 1 : 0);
        PlayerPrefs.Save();
        
        ApplySoundEffectsSettings(newState);
        
        Debug.Log($"[SettingsController] Ses Efektleri: {(newState ? "Açık" : "Kapalı")}");
    }
    
    private void ToggleVibration()
    {
        // Butona tıklandığında titreşim (ayar kapalıysa titreşim olmaz)
        if (VibrationManager.IsVibrationEnabled())
        {
            VibrationManager.Vibrate(VibrationType.Medium, 0.1f);
        }
        
        bool currentState = VibrationManager.IsVibrationEnabled();
        bool newState = !currentState;
        
        VibrationManager.SetVibrationEnabled(newState);
        
        Debug.Log($"[SettingsController] Titreşim: {(newState ? "Açık" : "Kapalı")}");
    }
    
    private void ApplyMusicSettings(bool enabled)
    {
        if (audioMixer != null)
        {
            // AudioMixer'da volume parametresini ayarla
            // enabled = true ise 0dB (normal ses), false ise -80dB (sessiz)
            float volume = enabled ? 0f : -80f;
            audioMixer.SetFloat(musicVolumeParameter, volume);
        }
        else
        {
            // AudioMixer yoksa, MusicManager'ı kullan
            if (MusicManager.Instance != null)
            {
                MusicManager.Instance.SetVolume(enabled ? 0.5f : 0f);
            }
        }
    }
    
    private void ApplySoundEffectsSettings(bool enabled)
    {
        if (audioMixer != null)
        {
            // AudioMixer'da volume parametresini ayarla
            float volume = enabled ? 0f : -80f;
            audioMixer.SetFloat(soundEffectsVolumeParameter, volume);
        }
        // PuzzleManager'daki AudioSource'lar AudioMixer kullanacak
    }
    
    private void LoadSettings()
    {
        // Müzik ayarını yükle
        bool musicEnabled = PlayerPrefs.GetInt(MUSIC_ENABLED_KEY, 1) == 1;
        ApplyMusicSettings(musicEnabled);
        
        // Ses efektleri ayarını yükle
        bool soundEffectsEnabled = PlayerPrefs.GetInt(SOUND_EFFECTS_ENABLED_KEY, 1) == 1;
        ApplySoundEffectsSettings(soundEffectsEnabled);
        
        // Titreşim ayarı yükleniyor (şimdilik sadece kaydediliyor)
        // bool vibrationEnabled = PlayerPrefs.GetInt(VIBRATION_ENABLED_KEY, 1) == 1;
    }
    
    public bool IsVibrationEnabled()
    {
        return VibrationManager.IsVibrationEnabled();
    }
}

