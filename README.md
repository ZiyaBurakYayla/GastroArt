# 🍽️ GastroArt - Next Gen AI-Driven Restaurant OS

**GastroArt**, standart restoran otomasyonlarının ötesine geçen; **Google Gemini AI** destekli bir "Akıllı İş Ortağı", **Three.js** tabanlı fütüristik bir müşteri deneyimi ve **SignalR** ile güçlendirilmiş gerçek zamanlı bir operasyon sistemidir.

Sıradan bir QR menü değil; işletme sahibine stratejik kararlar aldıran, mutfağı ve garsonları senkronize eden canlı bir ekosistemdir.

![GastroArt Banner](Images/banner.png)

---

## 🧠 GastroAI: Bir Asistandan Çok Daha Fazlası

Projenin kalbinde, **Google Gemini 2.5 Flash** modeli ile güçlendirilmiş, bağlam farkındalığına (Context-Aware) sahip otonom bir yapay zeka ajanı bulunur.

### 🔥 Neler Yapabilir?
* **📊 Derinlemesine Veri Analizi:** *"Geçen hafta en çok hangi saatlerde yoğunluk oldu?"* gibi kompleks soruları, milyonlarca satırlık geçmiş veriyi tarayarak saniyeler içinde yanıtlar.
* **🛠️ Otonom "Agent" Yetenekleri:** Doğal dil ile verilen emirleri SQL işlemlerine dönüştürür.
    * *"Kola stoğunu 50 yap"* → Veritabanını günceller.
    * *"Bekleyen tüm siparişleri onayla"* → Toplu işlem (Batch Processing) yapar.
* **🧠 Hafıza (Chat History):** Geçmiş konuşmaları hatırlar ve bağlamı korur.

---

## 🛡️ AI Veri Güvenliği ve Gizlilik Protokolleri

GastroArt, yapay zeka entegrasyonunda **"Privacy by Design"** (Tasarımda Gizlilik) ilkesini benimser. İşletme ve müşteri verilerinin güvenliği şu mekanizmalarla sağlanır:

1.  **Anonimleştirilmiş Veri Akışı (Data Anonymization):**
    * Yapay zekaya gönderilen analiz verileri **tamamen anonimdir**.
    * ❌ **GÖNDERİLMEZ:** Müşteri isimleri, telefon numaraları, kredi kartı bilgileri veya kişisel tanımlayıcı hiçbir veri (PII) AI servisine iletilmez.
    * ✅ **GÖNDERİLİR:** Sadece ürün isimleri, stok adetleri ve satış rakamları gibi operasyonel metrikler işlenir.

2.  **Kısıtlı Ajan Yetkisi (Sandboxed Execution):**
    * GastroAI, veritabanı üzerinde sınırsız yetkiye sahip değildir. Sadece C# tarafında tanımlanmış **"Güvenli Fonksiyonları"** (Whitelisted Methods) tetikleyebilir.
    * *Örnek:* AI stok güncelleyebilir ama veritabanını silemez, admin şifresini değiştiremez veya kritik sistem ayarlarına erişemez.

3.  **Ticari Sır Koruması:**
    * Analiz için kullanılan veriler, Google'ın kurumsal API standartları çerçevesinde işlenir ve sadece oturum süresince bağlam (context) oluşturmak için kullanılır.

---

## 🚀 Temel Modüller ve İşlevleri

### 📱 Müşteri Deneyimi (Frontend - PWA)
* **💎 3D Kristal Menü:** Three.js tabanlı, döndürülebilir ve interaktif kategori seçimi.
* **📲 PWA (Progressive Web App):** Uygulama mağazasına gerek kalmadan, tarayıcı üzerinden "Uygulama Gibi" çalışma özelliği.
* **🛒 Akıllı Sepet & Masa Takibi:** QR kod ile gelen masa bilgisini otomatik tanır.
* **🔔 IoT Garson Çağırma:** Müşteri tek tuşla garsonu çağırır, yönetim paneline anlık bildirim düşer.
* **🌍 Çoklu Dil Desteği:** Tek tıkla TR/EN içerik değişimi.

### 👨‍🍳 Yönetim Paneli (Admin Dashboard)
* **⚡ Real-Time Mutfak Ekranı (SignalR):** Siparişler **sayfa yenilenmeden** sesli bildirim ("DONG!") ile ekrana düşer.
* **📦 Akıllı Stok Yönetimi:** Stok bitince ürün otomatik olarak "TÜKENDİ" moduna geçer.
* **💵 Kasa & Vardiya Sistemi:** Gün açılış/kapanış işlemleri ve ciro takibi.
* **🖨️ Adisyon Çıktısı:** Termal yazıcılar (80mm) için özel CSS ile fiş yazdırma.

![Admin Dashboard](Images/dashboard.png)

---

## 🛠️ Mimari ve Teknoloji Yığını

Proje, sürdürülebilirlik ve performans odaklı **Clean Architecture** prensiplerine sadık kalınarak geliştirilmiştir.

| Katman | Teknoloji | Açıklama |
| :--- | :--- | :--- |
| **Core / AI** | **Google Gemini 2.5 Flash** | Doğal Dil İşleme (NLP) ve Veri Analizi |
| **Backend** | **.NET 8.0 Core MVC** | Yüksek performanslı sunucu tarafı |
| **ORM** | **Entity Framework Core** | Code-First yaklaşımı ile veritabanı yönetimi |
| **Real-Time** | **SignalR (WebSockets)** | Sunucu-İstemci arası anlık iletişim |
| **Frontend** | **JavaScript (ES6+) & HTML5** | Modüler ve dinamik arayüz |
| **3D Engine** | **Three.js** | WebGL tabanlı 3 boyutlu grafik motoru |
| **Styling** | **Tailwind CSS** | Modern ve responsive tasarım |
| **Database** | **MS SQL Server** | İlişkisel veri tabanı |

---

<div align="center">
  <strong>GastroArt</strong> © 2025 - Ziya Burak Yayla<br>
  <i>Kod, Veri ve Mutfak Sanatının Buluştuğu Nokta</i>
</div>
