🧩 Kart - Yapboz Oyunu

Unity ile geliştirilmiş modern bir yapboz (jigsaw puzzle) oyunu. Parçaları sürükleyip bırakarak görselleri tamamlayın!

📋 İçindekiler

- [Özellikler](#özellikler)
- [Ekran Görüntüleri](#ekran-görüntüleri)
- [Kurulum](#kurulum)
- [Kullanım](#kullanım)
- [Proje Yapısı](#proje-yapısı)
- [Teknik Detaylar](#teknik-detaylar)
- [Geliştirme](#geliştirme)
- [Lisans](#lisans)

🎮 Oyunu Oyna
🌐 Web'de Oyna
Oyunu tarayıcınızda direkt oynayabilirsiniz:
[▶️ Oyunu Oyna](https://ziyabarutcu.github.io/Kart/) ]


 ✨ Özellikler

- 🎮 **Sürükle-Bırak Mekanikleri**: Parçaları dokunarak veya fare ile sürükleyip doğru yerlerine yerleştirin
- 📊 **Seviye Sistemi**: Bölümler halinde organize edilmiş çoklu seviyeler
- 🔒 **Kilit Sistemi**: Seviyeler sırayla açılır, tamamlanan seviyeler kaydedilir
- 🎵 **Müzik Yönetimi**: Arka plan müziği ve ses efektleri
- 📳 **Titreşim Desteği**: Mobil cihazlarda dokunma geri bildirimi
- ⚙️ **Ayarlar Menüsü**: Ses, müzik ve diğer ayarları özelleştirin
- 🎨 **Animasyonlu Karıştırma**: Oyun başlarken parçalar animasyonlu olarak karıştırılır
- 📱 **Android Desteği**: APK olarak derlenebilir

 🖼️ Ekran Görüntüleri


<img width="341" height="602" alt="image" src="https://github.com/user-attachments/assets/b26b45f1-968a-4a19-943e-dfa950ae553f" />

<img width="333" height="516" alt="image" src="https://github.com/user-attachments/assets/e7c73ea0-be98-40cf-b77a-408097f90261" />

<img width="356" height="566" alt="image" src="https://github.com/user-attachments/assets/f73cf599-ef08-47b4-8cba-b9bd7703002e" />

<img width="343" height="519" alt="image" src="https://github.com/user-attachments/assets/b449332f-fa56-4060-abf2-89edb08c876c" />

<img width="305" height="562" alt="image" src="https://github.com/user-attachments/assets/6b267a7e-c12b-42f7-8d7c-cd310789d5f5" />

<img width="307" height="158" alt="image" src="https://github.com/user-attachments/assets/121b1b78-9d8c-4830-9558-8d8b8c1ca2a2" />


 🚀 Kurulum

 Gereksinimler

- Unity 2022.3 veya üzeri
- .NET Framework 4.8 veya üzeri
- Android SDK (Android build için)

 Adımlar

1. Projeyi klonlayın:
```bash
git clone https://github.com/kullaniciadi/kart.git
cd kart
```

2. Unity Hub'ı açın ve projeyi ekleyin

3. Unity Editor'de projeyi açın

4. Gerekli paketler otomatik olarak yüklenecektir

🎯 Kullanım

 Oyunu Oynama

1. Ana menüden bir seviye seçin
2. Parçalar otomatik olarak karıştırılacak
3. Parçaları sürükleyip doğru yerlerine yerleştirin
4. Tüm parçalar yerleştirildiğinde seviye tamamlanır
5. Sonraki seviyeye geçebilir veya ana menüye dönebilirsiniz

Seviye Oluşturma

1. Unity Editor'da `Assets/Data/Levels` klasörüne gidin
2. Sağ tık → `Create → Puzzle → Level Config`
3. Yeni ScriptableObject'i düzenleyin:
   - Puzzle görselini ekleyin
   - Grid boyutlarını ayarlayın (2x2'den 10x10'a kadar)
   - Bölüm ID ve seviye indeksini belirleyin

📁 Proje Yapısı

```
Kart/
├── Assets/
│   ├── Scripts/              # C# scriptleri
│   │   ├── PuzzleManager.cs  # Ana oyun mantığı
│   │   ├── PuzzlePiece.cs    # Parça davranışları
│   │   ├── MainMenuController.cs  # Ana menü kontrolü
│   │   ├── LevelProgress.cs  # İlerleme kaydı
│   │   └── ...
│   ├── Scenes/               # Unity sahneleri
│   │   ├── MainMenu.unity    # Ana menü sahnesi
│   │   └── SampleScene.unity  # Oyun sahnesi
│   ├── Data/
│   │   ├── Images/           # Puzzle görselleri
│   │   └── Levels/           # Seviye konfigürasyonları
│   ├── Prefabs/              # Oyun prefab'ları
│   ├── Sounds/               # Ses dosyaları
│   └── Settings/             # Oyun ayarları
├── Builds/                   # Derlenmiş build'ler
└── ProjectSettings/          # Unity proje ayarları
```

🔧 Teknik Detaylar

 Ana Bileşenler

PuzzleManager
- Puzzle oluşturma ve yönetimi
- Parça yerleştirme mantığı
- Seviye tamamlama kontrolü
- Animasyonlu karıştırma sistemi

PuzzlePiece
- Dokunma/sürükleme algılama
- Snap-to-slot mekanizması
- Doğru yerleştirme kontrolü

LevelProgress
- PlayerPrefs kullanarak ilerleme kaydı
- Seviye kilitleme/açma sistemi
- Bölüm bazlı ilerleme takibi

MusicManager
- Singleton pattern ile müzik yönetimi
- Sahne değişimlerinde müzik devamlılığı
- AudioMixer entegrasyonu

Özellikler

- **Grid Sistemi**: 2x2'den 10x10'a kadar özelleştirilebilir grid boyutları
- **Snap Mekanizması**: Parçalar doğru yere yaklaştığında otomatik yerleşir
- **Animasyonlar**: Smooth karıştırma ve yerleştirme animasyonları
- **Ses Sistemi**: Müzik ve ses efektleri için ayrı AudioMixer grupları
- **Mobil Optimizasyon**: Android için optimize edilmiş kontroller

🛠️ Geliştirme

Yeni Özellik Ekleme

1. İlgili script dosyasını düzenleyin
2. Unity Editor'da test edin
3. Değişiklikleri commit edin

Build Alma

Android APK

1. `File → Build Settings`
2. Platform olarak Android'i seçin
3. `Player Settings`'den gerekli ayarları yapın
4. `Build` butonuna tıklayın

 Debug Modları

- `R` tuşu: Seviye ilerlemesini sıfırla (MainMenu'de)
- Console logları: Detaylı debug bilgileri

📝 Notlar

- İlerleme verileri `PlayerPrefs` kullanılarak saklanır
- Müzik yönetimi `DontDestroyOnLoad` ile sahneler arasında devam eder
- Parçalar başlangıçta doğru yerlerinde görünür, sonra animasyonlu olarak karıştırılır

🤝 Katkıda Bulunma

1. Fork edin
2. Feature branch oluşturun (`git checkout -b feature/YeniOzellik`)
3. Değişikliklerinizi commit edin (`git commit -am 'Yeni özellik eklendi'`)
4. Branch'inizi push edin (`git push origin feature/YeniOzellik`)
5. Pull Request oluşturun

📄 Lisans

Bu proje [Lisans Adı] altında lisanslanmıştır. Detaylar için `LICENSE` dosyasına bakın.

👤 Yazar

**Ziya**

- GitHub: [@kullaniciadi](https://github.com/kullaniciadi)

🙏 Teşekkürler

- Unity Technologies
- TextMesh Pro
- Tüm açık kaynak kütüphaneler

---

⭐ Bu projeyi beğendiyseniz yıldız vermeyi unutmayın!

