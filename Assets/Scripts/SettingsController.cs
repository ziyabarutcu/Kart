using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class SettingsController : MonoBehaviour
{
    [Header("Canvas References")]
    [SerializeField] private Canvas settingsCanvas;
    [SerializeField] private Canvas mainCanvas; // Ana menü canvas'ı
    
    [Header("UI Elements")]
    [SerializeField] private Image backgroundBlocker; // Ana Canvas'daki blocker Image
    
    [Header("Buttons")]
    [SerializeField] private Button closeButton; // X butonu
    [SerializeField] private Button musicToggleButton; // Sol buton - Müzik
    [SerializeField] private Button soundEffectsToggleButton; // Orta buton - Ses efektleri
    [SerializeField] private Button vibrationToggleButton; // Sağ buton - Titreşim
    
    [Header("Button Icons")]
    [SerializeField] private Sprite musicOnIcon; // Müzik açık ikonu
    [SerializeField] private Sprite musicOffIcon; // Müzik kapalı ikonu
    
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
        
        // Background blocker'ı başlangıçta gizle
        if (backgroundBlocker != null)
        {
            backgroundBlocker.gameObject.SetActive(false);
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
            
            // Blocker'ı aktif et (ana Canvas'ı kapatmak yerine)
            if (backgroundBlocker != null)
            {
                backgroundBlocker.gameObject.SetActive(true);
            }
            
            // Ana Canvas'ın butonlarını disable et
            DisableMainCanvasButtons();
            
            // Ayarlar açıldığında buton ikonlarını güncelle (butonlar artık aktif)
            RefreshButtonIcons();
        }
    }
    
    private void RefreshButtonIcons()
    {
        // Müzik durumunu oku ve ikonu güncelle
        // Önce PlayerPrefs'ten oku, eğer AudioMixer varsa gerçek durumu kontrol et
        bool musicEnabled = GetActualMusicState();
        UpdateMusicButtonIcon(musicEnabled);
    }
    
    private bool GetActualMusicState()
    {
        // Önce PlayerPrefs'ten oku
        bool prefState = PlayerPrefs.GetInt(MUSIC_ENABLED_KEY, 1) == 1;
        
        // Eğer AudioMixer varsa, gerçek volume değerini kontrol et
        if (audioMixer != null)
        {
            float currentVolume;
            if (audioMixer.GetFloat(musicVolumeParameter, out currentVolume))
            {
                // Volume -80dB veya daha düşükse müzik kapalı
                return currentVolume > -79f;
            }
        }
        
        // AudioMixer yoksa veya parametre bulunamazsa PlayerPrefs değerini kullan
        return prefState;
    }
    
    public void CloseSettings()
    {
        // X butonuna tıklandığında titreşim
        VibrationManager.Vibrate(VibrationType.Medium, 0.1f);
        
        if (settingsCanvas != null)
        {
            settingsCanvas.gameObject.SetActive(false);
            isSettingsOpen = false;
            
            // Blocker'ı kapat
            if (backgroundBlocker != null)
            {
                backgroundBlocker.gameObject.SetActive(false);
            }
            
            // Ana Canvas'ın butonlarını tekrar aktif et
            EnableMainCanvasButtons();
        }
    }
    
    private void DisableMainCanvasButtons()
    {
        // Ana Canvas'daki tüm butonları bul ve disable et
        if (mainCanvas != null)
        {
            Button[] buttons = mainCanvas.GetComponentsInChildren<Button>(true);
            foreach (Button btn in buttons)
            {
                // Settings Canvas'daki butonları hariç tut
                if (settingsCanvas != null && btn.transform.IsChildOf(settingsCanvas.transform))
                {
                    continue;
                }
                btn.interactable = false;
            }
        }
        
        // Settings Canvas'ın GraphicRaycaster'ının aktif olduğundan emin ol
        if (settingsCanvas != null)
        {
            GraphicRaycaster settingsRaycaster = settingsCanvas.GetComponent<GraphicRaycaster>();
            if (settingsRaycaster != null)
            {
                settingsRaycaster.enabled = true;
            }
        }
    }
    
    private void EnableMainCanvasButtons()
    {
        // Ana Canvas'daki tüm butonları tekrar aktif et
        if (mainCanvas != null)
        {
            Button[] buttons = mainCanvas.GetComponentsInChildren<Button>(true);
            foreach (Button btn in buttons)
            {
                // Settings Canvas'daki butonları hariç tut
                if (settingsCanvas != null && btn.transform.IsChildOf(settingsCanvas.transform))
                {
                    continue;
                }
                btn.interactable = true;
            }
        }
    }
    
    private void ToggleMusic()
    {
        // Butona tıklandığında titreşim
        VibrationManager.Vibrate(VibrationType.Medium, 0.1f);
        
        // Gerçek müzik durumunu al (PlayerPrefs veya AudioMixer'dan)
        bool currentState = GetActualMusicState();
        bool newState = !currentState;
        
        // Yeni durumu kaydet
        PlayerPrefs.SetInt(MUSIC_ENABLED_KEY, newState ? 1 : 0);
        PlayerPrefs.Save();
        
        // Müzik ayarlarını uygula
        ApplyMusicSettings(newState);
        
        // İkonu güncelle
        UpdateMusicButtonIcon(newState);
        
        Debug.Log($"[SettingsController] Müzik: {(newState ? "Açık" : "Kapalı")} (PlayerPrefs: {PlayerPrefs.GetInt(MUSIC_ENABLED_KEY, 1)})");
    }
    
    private void UpdateMusicButtonIcon(bool musicEnabled)
    {
        if (musicToggleButton == null)
        {
            return;
        }
        
        // Butonun Image component'ini bul
        Image buttonImage = musicToggleButton.GetComponent<Image>();
        if (buttonImage == null)
        {
            // Eğer butonun kendisinde Image yoksa, child'ında ara
            buttonImage = musicToggleButton.GetComponentInChildren<Image>();
        }
        
        if (buttonImage != null)
        {
            // Müzik durumuna göre ikonu değiştir
            if (musicEnabled && musicOnIcon != null)
            {
                buttonImage.sprite = musicOnIcon;
            }
            else if (!musicEnabled && musicOffIcon != null)
            {
                buttonImage.sprite = musicOffIcon;
            }
        }
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
        // Not: İkon güncellemesi OpenSettings()'te RefreshButtonIcons() ile yapılacak
        // Çünkü Awake()'de butonlar henüz aktif olmayabilir
        
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

