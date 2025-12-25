// log-takip.js - NORMAL YAZIMLI HALİ

document.addEventListener("DOMContentLoaded", function () {
    const email = localStorage.getItem('email');

    // Eğer giriş yapılmamışsa log tutma (Login ve Register hariç)
    let path = window.location.pathname;
    if (!email || path.includes("login.html") || path.includes("register.html")) return;

    // --- SAYFA İSİMLERİNİ AYARLA ---
    let sayfaAdi = "Bilinmeyen Sayfa";

    if (path.includes("index.html") || path === "/") {
        sayfaAdi = "Ana sayfaya gidildi";
    }
    else if (path.includes("kitaplar.html")) {
        sayfaAdi = "Kitaplar sayfasına gidildi";
    }
    else if (path.includes("odunc.html")) {
        sayfaAdi = "Ödünç sayfasına gidildi";
    }
    else if (path.includes("rezervasyon.html")) {
        sayfaAdi = "Rezervasyon sayfasına gidildi";
    }
    else if (path.includes("profil.html")) {
        sayfaAdi = "Profil sayfasına gidildi";
    }

    // Backend'e Log Gönder
    logGonder(email, sayfaAdi);
});

// Log Gönderme Fonksiyonu
function logGonder(email, mesaj) {
    fetch('/api/log/sayfa-goruntuleme', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
            Email: email,
            SayfaAdi: mesaj
        })
    }).catch(err => console.log("Log hatası:", err));
}

// --- MERKEZİ ÇIKIŞ FONKSİYONU ---
// Artık çıkış işlemi de loglanacak
function cikisYapVeLogla() {
    if (confirm("Çıkış yapmak istiyor musun?")) {
        const email = localStorage.getItem('email');

        if (email) {
            // Önce log at, sonra çıkış yap
            fetch('/api/log/sayfa-goruntuleme', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    Email: email,
                    SayfaAdi: "Çıkış yapıldı" // Veritabanında görünecek mesaj
                })
            }).finally(() => {
                // Log gitse de gitmese de temizle ve yönlendir
                localStorage.clear();
                window.location.href = "login.html";
            });
        } else {
            localStorage.clear();
            window.location.href = "login.html";
        }
    }
}