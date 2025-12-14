using UnityEngine;

public enum VibrationType
{
    Light,      // Hafif titreşim (kart tutma)
    Medium,     // Orta titreşim (buton tıklama)
    Heavy       // Güçlü titreşim (önemli olaylar)
}

public static class VibrationManager
{
    private const string VIBRATION_ENABLED_KEY = "VibrationEnabled";
    
    public static bool IsVibrationEnabled()
    {
        return PlayerPrefs.GetInt(VIBRATION_ENABLED_KEY, 1) == 1;
    }
    
    public static void Vibrate(float duration = 0.1f)
    {
        Vibrate(VibrationType.Medium, duration);
    }
    
    public static void Vibrate(VibrationType type, float duration = 0.1f)
    {
        if (!IsVibrationEnabled())
        {
            return;
        }
        
        // Unity'de titreşim sadece mobil platformlarda çalışır
        #if UNITY_ANDROID || UNITY_IOS
        if (Application.isMobilePlatform)
        {
            // Android'de farklı titreşim tipleri için farklı süreler kullanabiliriz
            // iOS'ta sadece Handheld.Vibrate() var, ama pattern kullanabiliriz
            #if UNITY_ANDROID
            // Android'de Vibration API kullanarak farklı titreşimler yapabiliriz
            // Şimdilik basit implementasyon, ileride geliştirilebilir
            Handheld.Vibrate();
            #else
            Handheld.Vibrate();
            #endif
        }
        #endif
        
        // Editor'da test için log
        #if UNITY_EDITOR
        string typeName = type switch
        {
            VibrationType.Light => "Hafif",
            VibrationType.Medium => "Orta",
            VibrationType.Heavy => "Güçlü",
            _ => "Orta"
        };
        Debug.Log($"[VibrationManager] {typeName} titreşim tetiklendi (Duration: {duration}s)");
        #endif
    }
    
    public static void SetVibrationEnabled(bool enabled)
    {
        PlayerPrefs.SetInt(VIBRATION_ENABLED_KEY, enabled ? 1 : 0);
        PlayerPrefs.Save();
    }
}

