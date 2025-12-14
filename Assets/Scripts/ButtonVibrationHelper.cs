using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Butonlara titreşim eklemek için helper script.
/// Bu script'i herhangi bir Button GameObject'ine ekleyin.
/// Butona tıklandığında otomatik olarak titreşim tetiklenir.
/// </summary>
[RequireComponent(typeof(Button))]
public class ButtonVibrationHelper : MonoBehaviour
{
    [Header("Titreşim Ayarları")]
    [SerializeField] private VibrationType vibrationType = VibrationType.Medium;
    [SerializeField] private float vibrationDuration = 0.1f;
    
    private Button button;
    
    private void Awake()
    {
        button = GetComponent<Button>();
        if (button != null)
        {
            // Mevcut onClick event'lerine titreşim ekle
            button.onClick.AddListener(OnButtonClicked);
        }
    }
    
    private void OnDestroy()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(OnButtonClicked);
        }
    }
    
    private void OnButtonClicked()
    {
        VibrationManager.Vibrate(vibrationType, vibrationDuration);
    }
}

