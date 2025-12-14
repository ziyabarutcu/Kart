using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;

public class MusicManager : MonoBehaviour
{
    private static MusicManager instance;
    
    [Header("Music Settings")]
    [SerializeField] private AudioClip backgroundMusic;
    [SerializeField] [Range(0f, 1f)] private float musicVolume = 0.5f;
    [SerializeField] private bool playOnStart = true;
    [SerializeField] private AudioMixerGroup musicMixerGroup; // Müzik için AudioMixerGroup
    
    private AudioSource audioSource;
    private bool isInitialized = false;
    private float savedTime = 0f; // Müziğin kaldığı yeri kaydetmek için
    
    public static MusicManager Instance
    {
        get
        {
            if (instance == null)
            {
                // Eğer instance yoksa, sahnede var mı kontrol et
                instance = FindObjectOfType<MusicManager>();
                
                // Hala yoksa yeni bir tane oluştur
                if (instance == null)
                {
                    GameObject musicManagerObject = new GameObject("MusicManager");
                    instance = musicManagerObject.AddComponent<MusicManager>();
                    DontDestroyOnLoad(musicManagerObject);
                }
            }
            return instance;
        }
    }
    
    private void Awake()
    {
        // Singleton pattern - sadece bir instance olmalı
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAudioSource();
            isInitialized = true;
            
            // Scene değiştiğinde müziğin devam etmesini sağla
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else if (instance != this)
        {
            // Eğer başka bir instance varsa, bu objeyi yok et
            Destroy(gameObject);
            return;
        }
    }
    
    private void OnDestroy()
    {
        // Event listener'ı temizle
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    private void OnEnable()
    {
        // GameObject aktif olduğunda müziğin devam etmesini sağla
        EnsureMusicPlaying();
    }
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Scene değiştiğinde müziğin devam etmesini sağla
        // Kısa bir gecikme ile çağır (scene tamamen yüklendikten sonra)
        Invoke(nameof(EnsureMusicPlaying), 0.1f);
    }
    
    private void EnsureMusicPlaying()
    {
        if (audioSource == null)
        {
            InitializeAudioSource();
        }
        
        if (audioSource != null && !audioSource.isPlaying && backgroundMusic != null)
        {
            // Eğer müzik durmuşsa ve clip atanmışsa, kaldığı yerden devam ettir
            if (audioSource.clip == backgroundMusic)
            {
                // Eğer kaydedilmiş bir time varsa, oradan devam et
                if (savedTime > 0f && savedTime < backgroundMusic.length)
                {
                    audioSource.time = savedTime;
                }
                audioSource.Play();
            }
            else if (audioSource.clip == null)
            {
                audioSource.clip = backgroundMusic;
                // Eğer kaydedilmiş bir time varsa, oradan başlat
                if (savedTime > 0f && savedTime < backgroundMusic.length)
                {
                    audioSource.time = savedTime;
                }
                audioSource.Play();
            }
        }
    }
    
    private void Update()
    {
        // Müzik çalarken time pozisyonunu sürekli kaydet
        if (audioSource != null && audioSource.isPlaying && audioSource.clip == backgroundMusic)
        {
            savedTime = audioSource.time;
        }
    }
    
    private void Start()
    {
        // Sadece ilk kez oluşturulduğunda ve playOnStart true ise müziği başlat
        // Scene'ler arasında geçişte müzik zaten çalıyorsa başlatma
        // Bu metod sadece ilk instance'da çağrılır (duplicate instance'lar destroy edilir)
        if (isInitialized && playOnStart && backgroundMusic != null)
        {
            // Eğer müzik zaten çalıyorsa, hiçbir şey yapma
            if (audioSource != null && audioSource.isPlaying)
            {
                return;
            }
            PlayMusic();
        }
    }
    
    private void InitializeAudioSource()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Clip'i sadece null ise veya hiç atanmamışsa ata
        // Eğer müzik zaten çalıyorsa, clip'i değiştirme (kesintisiz devam etsin)
        if (backgroundMusic != null && audioSource.clip == null)
        {
            audioSource.clip = backgroundMusic;
        }
        
        // AudioMixerGroup'u ata (müzik için)
        if (musicMixerGroup != null)
        {
            audioSource.outputAudioMixerGroup = musicMixerGroup;
        }
        
        audioSource.loop = true;
        audioSource.volume = musicVolume;
        audioSource.playOnAwake = false;
        
        // AudioSource'un scene değiştiğinde durmamasını sağla
        // Bu özellik Unity'de otomatik olarak DontDestroyOnLoad ile birlikte çalışır
        // Ama yine de emin olmak için kontrol ediyoruz
    }
    
    public void PlayMusic()
    {
        if (audioSource == null)
        {
            InitializeAudioSource();
        }
        
        if (backgroundMusic == null)
        {
            Debug.LogWarning("[MusicManager] Background music clip atanmamış!");
            return;
        }
        
        // Eğer müzik zaten çalıyorsa, hiçbir şey yapma (kaldığı yerden devam etsin)
        if (audioSource.isPlaying)
        {
            return;
        }
        
        // Clip'i sadece null ise ata (müzik çalmıyorsa)
        if (audioSource.clip == null)
        {
            audioSource.clip = backgroundMusic;
        }
        
        // Müziği başlat
        audioSource.Play();
    }
    
    public void StopMusic()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }
    
    public void SetVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        if (audioSource != null)
        {
            audioSource.volume = musicVolume;
        }
    }
    
    public float GetVolume()
    {
        return musicVolume;
    }
    
    public bool IsPlaying()
    {
        return audioSource != null && audioSource.isPlaying;
    }
    
    // Inspector'dan müzik clip'i değiştirildiğinde
    private void OnValidate()
    {
        if (audioSource != null && backgroundMusic != null)
        {
            // Eğer müzik çalıyorsa ve clip değiştiyse, yeniden başlat
            bool wasPlaying = audioSource.isPlaying;
            audioSource.clip = backgroundMusic;
            if (wasPlaying)
            {
                audioSource.Play();
            }
        }
    }
}

